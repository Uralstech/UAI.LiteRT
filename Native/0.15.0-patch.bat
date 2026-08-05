@echo off

cd LiteRT-LM || exit /b 1

REM TODO: Add the commit ID when it's actually fixed upstream.
set "FIX_COMMIT="
set "PLUGIN_DIR=..\..\UAI.LiteRTLM\Packages\com.uralstech.uai.litertlm\Runtime\Plugins"

set "PATCH_FILE=libLiteRtTopKWebGpuSampler.dll"
set "PATCH_SRC=prebuilt/windows_x86_64/%PATCH_FILE%"
set "PATCH_DST=%PLUGIN_DIR%\Windows\x86_64\%PATCH_FILE%"

if exist "%PATCH_DST%" del /f /q "%PATCH_DST%"

git show "%FIX_COMMIT%:%PATCH_SRC%" ^
    | git lfs smudge                ^
    > "%PATCH_DST%"