from pathlib import Path
import sys

path = Path(sys.argv[1])
text = path.read_text()

old = '''cc_binary(
    name = "litert-lm",
    srcs = [
        "conversation.cc",
        "engine.cc",
    ],
    copts = ["-fvisibility=hidden"],
    features = ["-legacy_whole_archive"],
    linkopts = select({
        "@platforms//os:macos": [],
        "@platforms//os:ios": [],
        "@platforms//os:windows": [],
        "//conditions:default": [
            # Report unresolved symbol references as errors during linking.
            "-Wl,--no-undefined",
            # Strip all symbols to reduce library size (debug info is removed).
            "-Wl,--strip-all",
            # Remove unused sections/functions to further reduce library size.
            "-Wl,--gc-sections",
            # Ensure symbols are resolved internally first.
            "-Wl,-Bsymbolic",
            # Update the rpath
            "-Wl,-rpath,$$ORIGIN",
        ],
    }),
    linkshared = 1,
    deps = ENGINE_COMMON_DEPS + [
        ":conversation_headers",
        ":engine_headers",
        "//runtime/core:engine_impl",
    ] + select({
        "//conditions:default": [],
    }),
)'''

new = '''cc_binary(
    name = "litert-lm",
    srcs = [
        "conversation.cc",
        "engine.cc",
    ],
    copts = ["-fvisibility=hidden"],
    features = ["-legacy_whole_archive"],
    linkopts = select({
        "@platforms//os:macos": [],
        "@platforms//os:ios": [],
        "@platforms//os:windows": [
            "/EXPORT:litert_lm_loaded_file_create",
            "/EXPORT:litert_lm_loaded_file_delete",
            "/EXPORT:litert_lm_loaded_file_has_speculative_decoding_support",
        ],
        "//conditions:default": [
            # Report unresolved symbol references as errors during linking.
            "-Wl,--no-undefined",
            # Strip all symbols to reduce library size (debug info is removed).
            "-Wl,--strip-all",
            # Remove unused sections/functions to further reduce library size.
            "-Wl,--gc-sections",
            # Ensure symbols are resolved internally first.
            "-Wl,-Bsymbolic",
            # Update the rpath
            "-Wl,-rpath,$$ORIGIN",
        ],
    }),
    linkshared = 1,
    deps = ENGINE_COMMON_DEPS + [
        ":conversation_headers",
        ":engine_headers",
        "//runtime/core:engine_impl",
        "//schema/capabilities:capabilities_c",
    ] + select({
        "//conditions:default": [],
    }),
)'''

if old not in text:
    raise SystemExit("Could not find litert-lm deps block")

path.write_text(text.replace(old, new, 1))