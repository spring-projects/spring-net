@echo off
cd build-support
call set-nant-config-per-processor-architecture.cmd
cd ..
@echo .
@echo ..
@echo ...
@echo Running full Build Script, capturing output to buildlog.txt file...
@echo Start Time: %time%
build-support\tools\nant\bin\nant -D:test.withcoverage=false -D:build-ems=false -D:test.integration.data=true -D:test.integration.ems=false -D:test.integration.nms=false %1 %2 %3 %4 %5 %6 %7 %8 %9
@echo ************************
@echo Build Complete!
@echo ************************
@echo End Time: %time%
 
