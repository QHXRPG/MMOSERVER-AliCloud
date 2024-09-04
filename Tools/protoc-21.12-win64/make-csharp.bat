
@echo off

set foo="%cd%\out\csharp"

if not exist %foo% (
    md %foo%
)

::遍历文件
for %%i in (proto/*.proto) do ( 
    %cd%/bin/protoc -I=proto --csharp_out=out/csharp %%i
    echo %%i Done
)


::copy out/csharp/*.cs C:/Users/Hello/source/repos/mmo-server/Common/Summer/Proto
:: cmd复制文件
copy /y out\csharp\Message.cs D:\work\mmo-game\mmo-server\Common\Summer\Proto
copy /y out\csharp\Message.cs D:\work\mmo-game\mmo-game\Assets\Plugins\Summer\Proto

echo "OK"
pause