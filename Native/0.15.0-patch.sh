cd LiteRT-LM || exit 1

FIX_COMMIT="8bee4dddc3794958b4bdd8a3a4ba75bcb71f6fbb"
PLUGIN_DIR="../../UAI.LiteRTLM/Packages/com.uralstech.uai.litertlm/Runtime/Plugins"
BUILT_SYMBOL="liblitert-lm"

PATCH_FILE="libLiteRtTopKOpenClSampler.so"
PATCH_SRC="prebuilt/android_arm64/${PATCH_FILE}"
PATCH_DST="${PLUGIN_DIR}/Android/arm64/${PATCH_FILE}"

set -euo pipefail
rm -f "${PATCH_DST}"

git show "${FIX_COMMIT}:${PATCH_SRC}"   \
    | git lfs smudge                    \
    > "${PATCH_DST}"

if ! patchelf --print-needed "${PATCH_DST}" | grep -q "^${BUILT_SYMBOL}\.so$"; then
    patchelf --add-needed "${BUILT_SYMBOL}.so" "${PATCH_DST}"
fi