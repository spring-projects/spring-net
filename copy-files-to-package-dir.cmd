@echo off
call build-support\set-nant-config-per-processor-architecture.cmd
@echo .
@echo ..
@echo ...
@echo Running full Build Script, capturing output to buildlog.txt file...
@echo Start Time: %time%
build-support\tools\nant\bin\nant package-release-files -f:spring.build -D:project.build.sign=true -D:mstest.exe="c:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\mstest.exe" -D:package.version=1.3.2 -D:vs-net.mstest.bin.dir="c:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE" > buildlog.txt
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



