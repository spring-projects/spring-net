@echo off
REM **************************************************
REM Batch file to copy the appropriate nant.exe.config file that correlates to the underlying
REM processor architecture of the system on which the build is running.
REM
REM This is necessary b/c the nant.exe.config file contains path-references for .NET Framework assemblies
REM which need to properly depend upon either %programfiles% (for x86 machines) or %programfilesx86% (for x64 machines).
REM As there is no way to make the present version of NANT "processor-architecture-aware", this crude mechanism of
REM maintaining two otherwise identical .config files (one for x86 and one or x64) and copying the appropriate one into the
REM expected location has been implemented.
REM
REM NOTE: until such time as NANT itself becomes able to accommodate the differing .NET framework install paths on x86 and x64
REM 		systems, it will be necessary for ANY builds of SPRNET that use NANT to first call this batch/command file to set the
REM			proper .config file 'active'.
REM
REM **************************************************


goto %processor_architecture%
goto end


:x86
echo x86 Processor Architecture Detected, setting up nant.exe.config for x86-based .NET reference paths...
copy tools\nant\config\NAnt.exe.config-x86 tools\nant\bin\NAnt.exe.config
goto end


:amd64
echo AMD64 Processor Architecture Detected, setting up nant.exe.config for x64-based .NET reference paths.
copy tools\nant\config\NAnt.exe.config-x64 tools\nant\bin\NAnt.exe.config
goto end

:ia64
echo IA64 Processor Architecture Detected -- WARNING: this processor architecture is not supported by the Spring.NET build scripts!
goto end

:end
