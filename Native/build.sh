cd LiteRT-LM || exit 1

PLUGIN_DIR="../../UAI.LiteRTLM/Packages/com.uralstech.uai.litertlm/Runtime/Plugins"
BUILT_SYMBOL="liblitert-lm"
BUILD_DIR="./bazel-bin/c"

build() {
    local config="$1"
    shift

    bazel build                                     \
        --config="${config}"                        \
        "$@"                                        \
        //c:litert-lm || return 1

}

patch_prebuilt_lib_android() {
    local arch="$1"
    local lib="$2"
    local dst="${PLUGIN_DIR}/Android/${arch}/${lib}.so"

    if ! patchelf --print-needed "${dst}" | grep -q "^${BUILT_SYMBOL}\.so$"; then
        patchelf --add-needed "${BUILT_SYMBOL}.so" "${dst}"
    fi
}

copy_libs() {
    local platform="$1"
    local arch="$2"
    local extension="$3"
    local plugin_subdir="$4"

    local src="./prebuilt/${platform}_${arch}"
    local dst="${PLUGIN_DIR}/${plugin_subdir}/${arch}"

    mkdir -p "$dst"
    for lib in libGemmaModelConstraintProvider  \
               libLiteRtTopKWebGpuSampler       \
               libLiteRtWebGpuAccelerator       \
               libwebgpu_dawn; do

        cp \
            "${src}/${lib}.${extension}" \
            "${dst}/${lib}.${extension}"
    done

    cp \
        "${BUILD_DIR}/${BUILT_SYMBOL}.${extension}" \
        "${dst}/${BUILT_SYMBOL}.${extension}"
}

# Set ANDROID_NDK_HOME before running this
# ------------------------------ Android ------------------------------

build android_arm64 --linkopt=-Wl,-z,max-page-size=16384 || exit 1
copy_libs android arm64 so Android
patch_prebuilt_lib_android arm64 libLiteRtTopKWebGpuSampler
