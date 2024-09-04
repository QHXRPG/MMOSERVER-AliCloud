@SET EXCEL_FOLDER=.\excel
@SET JSON_FOLDER=.\out
@SET CODE_FOLDER=.\out
@SET EXE=.\excel2json.exe

@ECHO 复制到指定位置，会生成Define和Data两个目录
@SET SERVER_DEST=D:\work\mmo-game\mmo-server\GameServer
@SET CLIENT_DEST=D:\work\mmo-game\mmo-game\Assets\Resources


@ECHO ------------------------------------------

@ECHO Converting excel files in folder %EXCEL_FOLDER% ...
for /f "delims=" %%i in ('dir /b /a-d /s %EXCEL_FOLDER%\*.xlsx') do (
    @echo   processing %%~nxi 
    @CALL %EXE% --excel %EXCEL_FOLDER%\%%~nxi --json %JSON_FOLDER%\%%~ni.json --csharp %CODE_FOLDER%\%%~ni.cs --header 3
)


@ECHO Copying JSON files to destination folder ...
for /r %JSON_FOLDER% %%i in (*.json) do (
    @echo   copying %%~nxi 
    @COPY "%%i" "%SERVER_DEST%\Data\%%~nxi"
    @COPY "%%i" "%CLIENT_DEST%\Data\%%~nxi"
)

for /r %JSON_FOLDER% %%i in (*.cs) do (
    @echo   copying %%~nxi 
    @COPY "%%i" "%SERVER_DEST%\Define\%%~nxi"
    @COPY "%%i" "%CLIENT_DEST%\Define\%%~nxi"
)


echo "OK"
pause