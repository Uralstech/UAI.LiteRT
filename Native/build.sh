cd LiteRT-LM || exit 1

PLUGIN_DIR="../../UAI.LiteRTLM/Packages/com.uralstech.uai.litertlm/Runtime/Plugins"
BUILT_SYMBOL="liblitert-lm"
BUILD_DIR="./bazel-bin/c"

PREBUILT_LIBS_COMMON="libLiteRt libGemmaModelConstraintProvider"
PREBUILT_LIBS_ANDROID="libLiteRtTopKOpenClSampler libLiteRtOpenClAccelerator"
PREBUILT_LIBS_APPLE="libLiteRtTopKMetalSampler libLiteRtMetalAccelerator"

build() {
    local config="$1"
    shift

    bazel build                     \
        --config="${config}"        \
        -c opt "$@"                 \
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

force_copy_file() {
    local src_file="$1"
    local dst_file="$2"

    rm -f "${dst_file}"
    cp "${src_file}" "${dst_file}"
}

copy_libs() {
    local platform="$1"
    local arch="$2"
    local extension="$3"
    local plugin_subdir="$4"
    local platform_libs="$5"

    local src="./prebuilt/${platform}_${arch}"
    local dst="${PLUGIN_DIR}/${plugin_subdir}/${arch}"

    mkdir -p "$dst"
    for lib in ${PREBUILT_LIBS_COMMON} ${platform_libs}; do

        force_copy_file                     \
            "${src}/${lib}.${extension}"    \
            "${dst}/${lib}.${extension}"
    done

    force_copy_file                                 \
        "${BUILD_DIR}/${BUILT_SYMBOL}.${extension}" \
        "${dst}/${BUILT_SYMBOL}.${extension}"
}

# Set ANDROID_NDK_HOME before running this
# ------------------------------ Android ------------------------------

build android_arm64 --linkopt=-Wl,-z,max-page-size=16384 || exit 1
copy_libs android arm64 so Android "${PREBUILT_LIBS_ANDROID}"
patch_prebuilt_lib_android arm64 libLiteRtTopKOpenClSampler


# ------------------------------  macOS  ------------------------------

# Note: Unity needs to load the Metal accelerators on startup for
# liblitert-lm.dylib to register it.

build macos_arm64 --linkopt=-Wl,-rpath,@loader_path \
    --define=litert_link_capi_so=true               \
    --define=litert_runtime_link_mode=dynamic       \
    --define=resolve_symbols_in_exec=false || exit 1

copy_libs macos arm64 dylib macOS "${PREBUILT_LIBS_APPLE}"


# ------------------------------   iOS   ------------------------------

build ios_arm64 || exit 1
copy_libs ios arm64 dylib iOS "${PREBUILT_LIBS_APPLE}"

build ios_sim_arm64 || exit 1
copy_libs ios sim_arm64 dylib iOS "${PREBUILT_LIBS_APPLE}"
