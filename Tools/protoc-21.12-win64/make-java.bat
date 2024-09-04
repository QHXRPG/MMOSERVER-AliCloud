
@echo off

set foo="%cd%\out\java"

if not exist %foo% (
    md %foo%
)

::遍历文件
for %%i in (proto/*.proto) do ( 
    %cd%/bin/protoc -I=proto --java_out=out/java %%i
    echo %%i Done
)


echo "OK"
pause