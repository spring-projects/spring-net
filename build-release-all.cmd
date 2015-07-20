@echo off
cd build-support
call set-nant-config-per-processor-architecture.cmd
cd ..
@echo .
@echo ..
@echo ...
@echo Running full Build Script, capturing output to buildlog.txt file...
@echo Start Time: %time%
build-support\tools\nant\bin\nant clean package -f:spring.build -D:project.build.sign=true -D:build-ems=true -D:test.integration.ems=false -D:test.integration.data=true -D:mstest.exe="c:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\mstest.exe" -D:package.version=2.0.1 -D:vs-net.mstest.bin.dir="c:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE" -D:skipmstestrun=true > buildlog.txt
rem for reference, package suffix arg = -D:nuget.version.suffix=RC2
@echo .
@echo ..
@echo ...
@echo Launching text file viewer to display buildlog.txt contents...
start "ignored but required placeholder window title argument" buildlog.txt
@echo .
@echo ..
@echo ...
@echo ************************
@echo Build Complete!
@echo ************************
@echo End Time: %time%
@echo    



