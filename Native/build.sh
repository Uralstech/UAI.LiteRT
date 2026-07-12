cd LiteRT-LM || exit 1

PLUGIN_DIR="../../UAI.LiteRTLM/Packages/com.uralstech.uai.litertlm/Runtime/Plugins"
BUILD_DIR="./bazel-bin/c"

build() {
    local config="$1"

    bazel build                                     \
        --config="${config}"                        \
        --define=litert_runtime_link_mode=dynamic   \
        --define=resolve_symbols_in_exec=false      \
        //c:litert-lm || return 1

}

copy_libs() {
    local platform="$1"
    local arch="$2"
    local extension="$3"
    local plugin_subdir="$4"

    local src="./prebuilt/${platform}_${arch}"
    local dst="${PLUGIN_DIR}/${plugin_subdir}/${arch}"

    mkdir -p "$dst"
    for lib in libLiteRtTopKOpenClSampler   \
               libLiteRtTopKWebGpuSampler; do

        cp \
            "${src}/${lib}.${extension}" \
            "${dst}/${lib}.${extension}"
    done

    cp \
        "${BUILD_DIR}/liblitert-lm.${extension}" \
        "${dst}/liblitert-lm.${extension}"
}

# Set ANDROID_NDK_HOME before running this
# ------------------------------ Android ------------------------------

build android_arm64 || exit 1
copy_libs android arm64 so Android
