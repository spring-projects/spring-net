@echo off
@echo .
@echo ..
@echo ...
@echo Running full Build Script, capturing output to buildlog.txt file...
build-support\tools\nant\bin\nant test > buildlog.txt
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
@echo    