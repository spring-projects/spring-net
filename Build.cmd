@echo off
cd build-support
call set-nant-config-per-processor-architecture.cmd
cd ..
@echo .
@echo ..
@echo ...
@echo Running full Build Script, capturing output to buildlog.txt file...
@echo Start Time: %time%
build-support\tools\nant\bin\nant -D:test.withcoverage=false %1 %2 %3 %4 %5 %6 %7 %8 %9 > buildlog.txt
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
