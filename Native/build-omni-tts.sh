cd LiteRT-LM || exit 1

HEAD_COMMIT=$(git rev-parse HEAD)

PLUGIN_DIR="../../UAI.LiteRTLM/Packages/com.uralstech.uai.litertlm.omni/Runtime/Plugins"
BUILT_SYMBOL="liblitert-omni-tts"
BUILD_DIR="./bazel-bin/c/tts"

build() {
    local config="$1"
    shift

    bazel build                     \
        --config="${config}"        \
        -c opt "$@"                 \
        //c/tts:litert-omni-tts || return 1
}

force_copy_file() {
    local src_file="$1"
    local dst_file="$2"

    rm -f "${dst_file}"
    cp "${src_file}" "${dst_file}"
}

copy_build() {
    local platform="$1"
    local arch="$2"
    local extension="$3"
    local plugin_subdir="$4"

    local dst="${PLUGIN_DIR}/${plugin_subdir}/${arch}"
    mkdir -p "$dst"

    force_copy_file                                 \
        "${BUILD_DIR}/${BUILT_SYMBOL}.${extension}" \
        "${dst}/${BUILT_SYMBOL}.${extension}"
}

# Set ANDROID_NDK_HOME before running this
# ------------------------------ Android ------------------------------

build android_arm64 --linkopt=-Wl,-z,max-page-size=16384 || exit 1
copy_build android arm64 so Android


# # ------------------------------  macOS  ------------------------------

build macos_arm64 --linkopt=-Wl,-rpath,@loader_path \
    --define=litert_link_capi_so=true               \
    --define=litert_runtime_link_mode=dynamic       \
    --define=resolve_symbols_in_exec=false || exit 1

copy_build macos arm64 dylib macOS

# ------------------------------   iOS   ------------------------------

build ios_arm64 || exit 1
copy_build ios arm64 dylib iOS

build ios_sim_arm64 || exit 1
copy_build ios sim_arm64 dylib iOS

echo "LITERT_LM_REV = \"${HEAD_COMMIT}\"" > "${PLUGIN_DIR}/.build_sources.arm64.txt"