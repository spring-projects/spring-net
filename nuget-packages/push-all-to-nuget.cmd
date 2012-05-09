@echo off
echo *** Pushing all packages in this folder to NuGet.org ***
echo Note: this assumes the APIKey has already been set on this computer.
echo If not, run the following command to set the ApiKey:
echo            Nuget.exe setApiKey [API_KEY]
echo .
for /f %%F in ('dir /b *.nupkg') DO CALL :INVOKE %%F
GOTO :EOF

:INVOKE
echo .
echo Processing file %1 ...
echo .
..\build-support\tools\NuGet\NuGet.exe push %1"
echo .
echo                       ...complete!
GOTO :EOF
