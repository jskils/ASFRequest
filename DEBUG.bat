@echo off

set "sourceFile=.\\ASFRequest\\bin\\Debug\\net8.0\\ASFRequest.dll"
set "destinationFolder=I:\\ASF\\plugins"
set "dockerContainerName=asf-asf-1"

REM Debugging output
echo Source file: %sourceFile%
echo Destination folder: %destinationFolder%
echo Docker container name: %dockerContainerName%

REM Check if source file exists
if not exist "%sourceFile%" (
    echo Source file does not exist: %sourceFile%
    exit /b 1
)

REM Check if destination folder exists
if not exist "%destinationFolder%" (
    echo Destination folder does not exist: %destinationFolder%
    exit /b 1
)

REM Copy file
xcopy /y "%sourceFile%" "%destinationFolder%"
if errorlevel 1 (
    echo Failed to copy file.
    exit /b 1
)

REM Restart Docker container
docker restart %dockerContainerName%
if errorlevel 1 (
    echo Failed to restart Docker container.
    exit /b 1
)

echo Success
