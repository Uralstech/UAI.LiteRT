@echo off

cd LiteRT-LM || exit /b 1
set "DRIVE=%~d0"

set "PLUGIN_DIR=../../UAI.LiteRTLM/Packages/com.uralstech.uai.litertlm/Runtime/Plugins"
set "BUILD_DIR=./bazel-bin/c"
set "BAZEL_OUT=%DRIVE%\bzl"

set "PREBUILT_LIBS_COMMON=libLiteRt libGemmaModelConstraintProvider"
set "PREBUILT_LIBS_PC=libLiteRtTopKWebGpuSampler libLiteRtWebGpuAccelerator libwebgpu_dawn"

goto :main

:build
set "CONFIG=%~1"

bazelisk                        ^
    --output_base="%BAZEL_OUT%" ^
    build                       ^
    --config=%CONFIG%           ^
    -c opt                      ^
    %~2 %~3 %~4 %~5             ^
    %~6 %~7 %~8 %~9             ^
    //c:litert-lm

exit /b %ERRORLEVEL%

:force_copy_file
set "COPY_SRC=%~1"
set "COPY_DST=%~2"

if exist "%COPY_DST%" del /f /q "%COPY_DST%"
copy /y "%COPY_SRC%" "%COPY_DST%"

exit /b 0

:copy_libs
set "BUILT_SYMBOL=%~1"
set "PLATFORM=%~2"
set "ARCH=%~3"
set "EXT=%~4"
set "PLUGIN_SUBDIR=%~5"
set "PLATFORM_LIBS=%~6"

set "LIBS_SRC_DIR=.\prebuilt\%PLATFORM%_%ARCH%"
set "LIBS_DST_DIR=%PLUGIN_DIR%\%PLUGIN_SUBDIR%\%ARCH%"

if not exist "%LIBS_DST_DIR%" mkdir "%LIBS_DST_DIR%"

for %%L in (%PREBUILT_LIBS_COMMON% %PLATFORM_LIBS%) do (
    call :force_copy_file "%LIBS_SRC_DIR%\%%L.%EXT%" "%LIBS_DST_DIR%\%%L.%EXT%"
)

call :force_copy_file "%BUILD_DIR%\%BUILT_SYMBOL%.%EXT%" "%LIBS_DST_DIR%\%BUILT_SYMBOL%.%EXT%"

exit /b 0

:copy_windows_x64_libs
set "WIN_LIBS_SRC=.\bazel-bin\python\litert_lm"
set "WIN_LIBS_DST=%PLUGIN_DIR%\Windows\x86_64"

if not exist "%WIN_LIBS_DST%" mkdir "%WIN_LIBS_DST%"

bazelisk                                    ^
    --output_base="%BAZEL_OUT%"             ^
    build                                   ^
    //python/litert_lm:copy_dxcompiler_dll

if errorlevel 1 exit /b 1

call :force_copy_file "%WIN_LIBS_SRC%\dxcompiler.dll" "%WIN_LIBS_DST%\dxcompiler.dll"

bazelisk                                    ^
    --output_base="%BAZEL_OUT%"             ^
    build                                   ^
    //python/litert_lm:copy_dxil_dll

if errorlevel 1 exit /b 1

call :force_copy_file "%WIN_LIBS_SRC%\dxil.dll" "%WIN_LIBS_DST%\dxil.dll"

exit /b 0

:main

:: ------------------------------ Windows ------------------------------

:: Note: Set $env:BAZEL_SH, JAVA_HOME before running.
:: Note: Unity needs to load the WebGPU accelerators on startup for
:: litert-lm.dll to register it.

call :build windows                             ^
    "--define=litert_link_capi_so=true"         ^
    "--define=litert_runtime_link_mode=dynamic" ^
    "--define=resolve_symbols_in_exec=false"

if errorlevel 1 exit /b 1

call :copy_libs "litert-lm" windows x86_64 dll Windows "%PREBUILT_LIBS_PC%"
call :copy_windows_x64_libs

exit /b 0