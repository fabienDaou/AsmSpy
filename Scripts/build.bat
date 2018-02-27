@SETLOCAL
@ECHO OFF
@ECHO "Build started."

CALL "%VS140COMNTOOLS%\VsDevCmd.bat"

CD..
MSBUILD "scripts\build.proj"    

ENDLOCAL