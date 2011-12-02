@echo off
@echo ...
@echo Running reference documentation Build Script, capturing output to buildlog.txt ...
@echo Start Time: %time%
..\..\build-support\tools\nant\bin\nant %1 %2 %3 %4 %5 %6 %7 %8 %9 > buildlog.txt 
type buildlog.txt
@echo ...
@echo ************************
@echo Build Complete!
@echo ************************
@echo End Time: %time%
@echo    
