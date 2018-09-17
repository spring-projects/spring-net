rem scripts to setup integration test database on CI server
rem NOTE: sqlcmd params are hard-coded to credentials and instance name for SQL Server 2012 as per http://www.appveyor.com/docs/services-databases

rem first, create the databases and the SpringQA user
sqlcmd -S ".\SQL2017" -U sa -P Password12! -i "%APPVEYOR_BUILD_FOLDER%\build-support\create-integration-test-databases-and-users.sql"

rem add the 'Spring' schema and data
sqlcmd -S ".\SQL2017" -U sa -P Password12! -i "%APPVEYOR_BUILD_FOLDER%\test\Spring\Spring.Data.Integration.Tests\Data\Spring.Data.Integration.Tests_Spring_database.sql"

rem add the 'Credits' schema
sqlcmd -S ".\SQL2017" -U sa -P Password12! -i "%APPVEYOR_BUILD_FOLDER%\test\Spring\Spring.Data.Integration.Tests\Data\Spring.Data.Integration.Tests_Credits_database.sql"

rem add the 'Debits' schema
sqlcmd -S ".\SQL2017" -U sa -P Password12! -i "%APPVEYOR_BUILD_FOLDER%\test\Spring\Spring.Data.Integration.Tests\Data\Spring.Data.Integration.Tests_Debits_database.sql"

rem add the 'CreditsAndDebits' schema
sqlcmd -S ".\SQL2017" -U sa -P Password12! -i "%APPVEYOR_BUILD_FOLDER%\test\Spring\Spring.Data.Integration.Tests\Data\Spring.Data.Integration.Tests_CreditsAndDebits_database.sql"


