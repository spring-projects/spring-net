Running NHibernate.Tests against Spring.Data.NHibernate21 from within VS:

Open "Project Properties", "Debug" Section and enter

    <StartAction>Program</StartAction>
    <StartProgram>..\..\build-support\tools\nunit\net-2.0\nunit-console-x86.exe</StartProgram>
    <StartArguments>NHibernate.Test.dll /nologo /wait</StartArguments>


To run the tests hit F5