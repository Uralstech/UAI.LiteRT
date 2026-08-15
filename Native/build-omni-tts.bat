@echo off

cd LiteRT-LM || exit /b 1
set "DRIVE=%~d0"

for /f %%H in ('git rev-parse HEAD') do set HEAD_COMMIT=%%H

set "PLUGIN_DIR=../../UAI.LiteRTLM/Packages/com.uralstech.uai.litertlm.omni/Runtime/Plugins"
set "BUILD_DIR=./bazel-bin/c/tts"
set "BAZEL_OUT=%DRIVE%\bzl"

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
    //c/tts:litert-omni-tts

exit /b %ERRORLEVEL%

:force_copy_file
set "COPY_SRC=%~1"
set "COPY_DST=%~2"

if exist "%COPY_DST%" del /f /q "%COPY_DST%"
copy /y "%COPY_SRC%" "%COPY_DST%"

exit /b 0

:copy_build
set "BUILT_SYMBOL=%~1"
set "PLATFORM=%~2"
set "ARCH=%~3"
set "EXT=%~4"
set "PLUGIN_SUBDIR=%~5"

set "LIBS_DST_DIR=%PLUGIN_DIR%\%PLUGIN_SUBDIR%\%ARCH%"
if not exist "%LIBS_DST_DIR%" mkdir "%LIBS_DST_DIR%"

call :force_copy_file "%BUILD_DIR%\%BUILT_SYMBOL%.%EXT%" "%LIBS_DST_DIR%\%BUILT_SYMBOL%.%EXT%"

exit /b 0

:main

:: ------------------------------ Windows ------------------------------

:: Note: Set $env:BAZEL_SH, JAVA_HOME before running.

call :build windows                             ^
    "--define=litert_runtime_link_mode=dynamic" ^
    "--define=resolve_symbols_in_exec=false"

if errorlevel 1 exit /b 1

call :copy_build "litert-omni-tts" windows x86_64 dll Windows

echo LITERT_LM_REV = "%HEAD_COMMIT%" > "%PLUGIN_DIR%\.build_sources.windows_x64.txt"

exit /b 0