from pathlib import Path
import sys

path = Path(sys.argv[1])
text = path.read_text()

old = '''    deps = ENGINE_COMMON_DEPS + [
        ":conversation_headers",
        ":engine_headers",
        "//runtime/core:engine_impl",
    ] + select({'''

new = '''    deps = ENGINE_COMMON_DEPS + [
        ":conversation_headers",
        ":engine_headers",
        "//runtime/core:engine_impl",
        "//schema/capabilities:capabilities_c",
    ] + select({'''

if old not in text:
    raise SystemExit("Could not find litert-lm deps block")

path.write_text(text.replace(old, new, 1))