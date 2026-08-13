import argparse
import sys
from pathlib import Path
import numpy as np
import flatbuffers
from tensorflow.lite.python import schema_py_generated as schema
import shutil

def make_tensor(name, shape, dtype, buffer_index):
    """Helper to create a TFLite tensor definition."""
    t = schema.TensorT()
    t.name = name
    t.shape = list(shape)
    t.type = dtype
    t.buffer = buffer_index
    return t

def copy_runtime_file(input_dir: Path, output_dir: Path, filename: str):
    """Copy an already-valid runtime file into the output directory."""
    source = input_dir / filename
    destination = output_dir / filename

    if not source.exists():
        print(f"Warning: Runtime file '{source}' not found. Skipping.")
        return

    shutil.copy2(source, destination)
    print(f"  Copied: {source} -> {destination}")

def convert_embedding_model(
    input_path: Path,
    output_path: Path,
    force_fp32: bool = False,
    flatten_first_dim: bool = False,
):
    """
    Generic function to convert embedding lookup tables (.npy) into TFLite models
    using a GATHER operator with static/dynamic input and output shapes.
    """
    print("\n" + "=" * 60)
    print(f"Converting Embedding: {input_path.name}")

    if not input_path.exists():
        print(f"Warning: Source file '{input_path}' not found. Skipping.")
        return

    weights = np.load(input_path, mmap_mode="r", allow_pickle=False)
    print(f"  Source Shape: {weights.shape}")
    print(f"  Source Dtype: {weights.dtype}")

    # Optional dimensionality reduction/flattening (e.g., for MTP)
    if flatten_first_dim:
        assert weights.ndim == 3, f"Expected 3D tensor, got {weights.ndim}D"
        dim1, dim2, dim3 = weights.shape
        weights = weights.reshape(dim1 * dim2, dim3)
        print(f"  Flattened Shape: {weights.shape}")
    else:
        assert weights.ndim == 2, f"Expected 2D tensor, got {weights.ndim}D"

    # Optional conversion to FP32 (required if downstream runtime expects float32)
    if force_fp32 and weights.dtype != np.float32:
        weights = weights.astype(np.float32)
        print("  Converted weights to FLOAT32")

    # Ensure memory is C-contiguous
    weights = np.ascontiguousarray(weights)
    vocab, hidden = weights.shape
    print(f"  Final Table Shape: {(vocab, hidden)}")
    print(f"  Memory Size: {weights.nbytes / 1024**2:.2f} MB")

    # Buffers (0 is empty for runtime, 1 holds the constant weight matrix)
    empty_buffer = schema.BufferT()
    weight_buffer = schema.BufferT()
    weight_buffer.data = weights.view(np.uint8).tobytes()
    buffers = [empty_buffer, weight_buffer]

    # Resolve TFLite Type
    if weights.dtype == np.float16:
        dtype = schema.TensorType.FLOAT16
    elif weights.dtype == np.float32:
        dtype = schema.TensorType.FLOAT32
    else:
        raise ValueError(f"Unsupported embedding data type: {weights.dtype}")

    # Define Tensors
    input_shape = [1]
    output_shape = [1, hidden]

    input_tensor = make_tensor("input_ids", input_shape, schema.TensorType.INT32, 0)
    weight_tensor = make_tensor("embedding_table", [vocab, hidden], dtype, 1)
    output_tensor = make_tensor("embeddings", output_shape, dtype, 0)
    tensors = [input_tensor, weight_tensor, output_tensor]

    # Configure GATHER operator
    gather_code = schema.OperatorCodeT()
    gather_code.builtinCode = schema.BuiltinOperator.GATHER
    gather_code.version = 1

    gather_op = schema.OperatorT()
    gather_op.opcodeIndex = 0
    gather_op.inputs = [1, 0]  # params, indices
    gather_op.outputs = [2]

    options = schema.GatherOptionsT()
    options.axis = 0
    gather_op.builtinOptions = options
    gather_op.builtinOptionsType = schema.BuiltinOptions.GatherOptions

    # Subgraph
    subgraph = schema.SubGraphT()
    subgraph.tensors = tensors
    subgraph.inputs = [0]
    subgraph.outputs = [2]
    subgraph.operators = [gather_op]
    subgraph.name = "embedding"

    # Assemble Model
    model = schema.ModelT()
    model.version = 3
    model.operatorCodes = [gather_code]
    model.subgraphs = [subgraph]
    model.buffers = buffers
    model.description = f"Embedding lookup ({'FP32' if dtype == schema.TensorType.FLOAT32 else 'FP16'})"

    # Adaptive buffer size optimization for flatbuffer builder
    builder_size = max(1024, min(weights.nbytes + 1024 * 1024, 256 * 1024 * 1024))
    builder = flatbuffers.Builder(builder_size)
    model_offset = model.Pack(builder)
    builder.Finish(model_offset, file_identifier=b"TFL3")

    data = bytes(builder.Output())
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_bytes(data)

    print(f"  Created: {output_path} ({len(data) / 1024**2:.2f} MB)")


def convert_projection_model(input_path: Path, output_path: Path):
    """
    Converts two-layer PyTorch linear projections (.npz) with custom SiLU
    activations into a TFLite fully connected model.
    """
    print("\n" + "=" * 60)
    print(f"Converting Projection MLP: {input_path.name}")

    if not input_path.exists():
        print(f"Warning: Source file '{input_path}' not found. Skipping.")
        return

    z = np.load(input_path, allow_pickle=False)
    w1 = np.asarray(z["w1"], dtype=np.float32)
    b1 = np.asarray(z["b1"], dtype=np.float32)
    w2 = np.asarray(z["w2"], dtype=np.float32)
    b2 = np.asarray(z["b2"], dtype=np.float32)

    print(f"  w1 shape: {w1.shape}")
    print(f"  b1 shape: {b1.shape}")
    print(f"  w2 shape: {w2.shape}")
    print(f"  b2 shape: {b2.shape}")

    assert w1.shape == (2048, 2048)
    assert b1.shape == (2048,)
    assert w2.shape == (1024, 2048)
    assert b2.shape == (1024,)

    # Setup Buffers (0: runtime tensor, 1-4: weights/biases)
    buffers = [schema.BufferT()]
    for x in [w1, b1, w2, b2]:
        buf = schema.BufferT()
        buf.data = np.ascontiguousarray(x).view(np.uint8).reshape(-1)
        buffers.append(buf)

    # Define Tensors
    tensors = [
        make_tensor("input", [1, 2048], schema.TensorType.FLOAT32, 0),
        make_tensor("w1", [2048, 2048], schema.TensorType.FLOAT32, 1),
        make_tensor("b1", [2048], schema.TensorType.FLOAT32, 2),
        make_tensor("fc1", [1, 2048], schema.TensorType.FLOAT32, 0),
        make_tensor("sigmoid", [1, 2048], schema.TensorType.FLOAT32, 0),
        make_tensor("silu", [1, 2048], schema.TensorType.FLOAT32, 0),
        make_tensor("w2", [1024, 2048], schema.TensorType.FLOAT32, 3),
        make_tensor("b2", [1024], schema.TensorType.FLOAT32, 4),
        make_tensor("output", [1, 1024], schema.TensorType.FLOAT32, 0),
    ]

    # Fully Connected 1
    fc_code = schema.OperatorCodeT()
    fc_code.builtinCode = schema.BuiltinOperator.FULLY_CONNECTED
    fc_code.version = 1

    fc1 = schema.OperatorT()
    fc1.opcodeIndex = 0
    fc1.inputs = [0, 1, 2]
    fc1.outputs = [3]

    fc1_options = schema.FullyConnectedOptionsT()
    fc1_options.fusedActivationFunction = schema.ActivationFunctionType.NONE
    fc1.builtinOptions = fc1_options
    fc1.builtinOptionsType = schema.BuiltinOptions.FullyConnectedOptions

    # Logistic (Sigmoid)
    logistic_code = schema.OperatorCodeT()
    logistic_code.builtinCode = schema.BuiltinOperator.LOGISTIC
    logistic_code.version = 1

    logistic = schema.OperatorT()
    logistic.opcodeIndex = 1
    logistic.inputs = [3]
    logistic.outputs = [4]

    # Multiplication (SiLU = x * sigmoid(x))
    mul_code = schema.OperatorCodeT()
    mul_code.builtinCode = schema.BuiltinOperator.MUL
    mul_code.version = 1

    mul = schema.OperatorT()
    mul.opcodeIndex = 2
    mul.inputs = [3, 4]
    mul.outputs = [5]

    # Fully Connected 2
    fc2 = schema.OperatorT()
    fc2.opcodeIndex = 0
    fc2.inputs = [5, 6, 7]
    fc2.outputs = [8]

    fc2_options = schema.FullyConnectedOptionsT()
    fc2_options.fusedActivationFunction = schema.ActivationFunctionType.NONE
    fc2.builtinOptions = fc2_options
    fc2.builtinOptionsType = schema.BuiltinOptions.FullyConnectedOptions

    # Assemble Subgraph
    subgraph = schema.SubGraphT()
    subgraph.tensors = tensors
    subgraph.inputs = [0]
    subgraph.outputs = [8]
    subgraph.operators = [fc1, logistic, mul, fc2]
    subgraph.name = "text_projection"

    # Assemble Model
    model = schema.ModelT()
    model.version = 3
    model.operatorCodes = [fc_code, logistic_code, mul_code]
    model.subgraphs = [subgraph]
    model.buffers = buffers
    model.description = "Qwen3 TTS text projection"

    builder = flatbuffers.Builder(1024)
    model_offset = model.Pack(builder)
    builder.Finish(model_offset, file_identifier=b"TFL3")

    data = bytes(builder.Output())
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_bytes(data)

    print(f"  Created: {output_path} ({len(data) / 1024**2:.2f} MB)")


def convert_speaker_embedding(input_path: Path, output_path: Path):
    """Extracts speaker weights into a raw flat float32 binary."""
    print("\n" + "=" * 60)
    print(f"Converting Speaker Embedding: {input_path.name}")

    if not input_path.exists():
        print(f"Warning: Source file '{input_path}' not found. Skipping.")
        return

    weights = np.load(input_path)
    print(f"  Source Shape: {weights.shape}")
    print(f"  Source Dtype: {weights.dtype}")

    weights = np.asarray(weights, dtype=np.float32).reshape(-1)
    assert weights.size == 1024, f"Expected flat size of 1024, got {weights.size}"

    output_path.parent.mkdir(parents=True, exist_ok=True)
    weights.tofile(output_path)

    print(f"  Created: {output_path} ({len(weights.tobytes())} bytes)")


def patch_kv_cache_signatures(input_path: Path, output_path: Path):
    """
    Patches a TFLite model's input/output signature definitions to properly reference
    kv_cache keys expected by the inference engine.
    """
    print("\n" + "=" * 60)
    print(f"Patching KV Cache Signatures: {input_path.name}")

    if not input_path.exists():
        print(f"Warning: Source model for patching '{input_path}' not found. Skipping.")
        return

    try:
        from tensorflow.lite.tools import flatbuffer_utils
    except ImportError:
        print("  Error: Could not import 'tensorflow.lite.tools.flatbuffer_utils'.")
        print("  Skipping signature patching step.")
        return

    model = flatbuffer_utils.read_model(str(input_path))
    patched_count = 0

    for sig in model.signatureDefs or []:
        key = sig.signatureKey
        if isinstance(key, bytes):
            key = key.decode()

        print(f"  Examining Signature Def: {key}")

        for x in sig.inputs or []:
            name = x.name.decode() if isinstance(x.name, bytes) else x.name
            if name == "args_3":
                print(f"    INPUT : {name} -> kv_cache_k_0")
                x.name = b"kv_cache_k_0"
                patched_count += 1
            elif name == "args_4":
                print(f"    INPUT : {name} -> kv_cache_v_0")
                x.name = b"kv_cache_v_0"
                patched_count += 1

        for x in sig.outputs or []:
            name = x.name.decode() if isinstance(x.name, bytes) else x.name
            if name == "output_1":
                print(f"    OUTPUT: {name} -> kv_cache_k_0")
                x.name = b"kv_cache_k_0"
                patched_count += 1
            elif name == "output_2":
                print(f"    OUTPUT: {name} -> kv_cache_v_0")
                x.name = b"kv_cache_v_0"
                patched_count += 1

    if patched_count != 4:
        print(f"  Warning: Expected 4 patch locations, but only matched {patched_count}.")
    else:
        print("  Signature patching completed successfully.")

    output_path.parent.mkdir(parents=True, exist_ok=True)
    flatbuffer_utils.write_model(model, str(output_path))
    print(f"  Created Patched Model: {output_path}")


def main():
    parser = argparse.ArgumentParser(
        description=(
            "Convert Qwen3-TTS LiteRT host-side embedding tables into standalone "
            "TFLite lookup models, convert the text projection MLP, export the "
            "speaker embedding, and patch the MTP KV-cache signatures by default."
        )
    )
    parser.add_argument(
        "--input-dir",
        "-i",
        type=str,
        required=True,
        help=(
            "Root directory containing the model files in the same layout as "
            "https://huggingface.co/litert-community/Qwen3-TTS-12Hz-0.6B-Base/tree/main. "
            "Expected paths include tables/*.npy, tables/*.npz, voices/demo_speaker.npy, "
            "and mtp_fp32.tflite."
        ),
    )
    parser.add_argument(
        "--output-dir",
        "-o",
        type=str,
        default="./converted",
        help="Directory where converted models will be saved. Default: ./converted",
    )
    parser.add_argument(
        "--skip-patching",
        action="store_true",
        help="Skip patching the MTP KV-cache signature names.",
    )

    args = parser.parse_args()

    input_dir = Path(args.input_dir)
    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)

    # 1. Text Embedding Conversion
    convert_embedding_model(
        input_path=input_dir / "tables/text_embedding_fp16.npy",
        output_path=output_dir / "text_embedding.tflite",
        force_fp32=True,
        flatten_first_dim=False,
    )

    # 2. Codec Embedding Conversion
    convert_embedding_model(
        input_path=input_dir / "tables/codec_embedding_fp32.npy",
        output_path=output_dir / "codec_embedding.tflite",
        force_fp32=False,
        flatten_first_dim=False,
    )

    # 3. MTP Embedding Conversion (FP16 -> FP32 + Dimension Flattening)
    convert_embedding_model(
        input_path=input_dir / "tables/mtp_embeddings_fp16.npy",
        output_path=output_dir / "mtp_embedding.tflite",
        force_fp32=True,
        flatten_first_dim=True,
    )

    # 4. Text Projection (MLP) Conversion
    convert_projection_model(
        input_path=input_dir / "tables/text_projection_fp32.npz",
        output_path=output_dir / "text_projection.tflite",
    )

    # 5. Speaker Embedding Binary Export
    convert_speaker_embedding(
        input_path=input_dir / "voices/demo_speaker.npy",
        output_path=output_dir / "voices" / "demo_speaker.bin",
    )

    # 6. Signature Patching for KV Cache (If requested and present)
    if not args.skip_patching:
        patch_kv_cache_signatures(
            input_path=input_dir / "mtp_fp32.tflite",
            output_path=output_dir / "mtp_fp32.tflite",
        )
    # 7. Copy files that are already runtime-ready
    copy_runtime_file(input_dir, output_dir, "tokenizer.json")
    copy_runtime_file(input_dir, output_dir, "talker_int4.tflite")
    copy_runtime_file(input_dir, output_dir, "codec_decoder_fp32.tflite")
    
    print("\nConversion execution step completed.")


if __name__ == "__main__":
    main()