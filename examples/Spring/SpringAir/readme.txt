For .NET 1.1,
The VS.NET solution is SpringAir.2003.sln
In order to be able to run SpringAir you will need to create 'SpringAir.2003' 
virtual directory using IIS Administrator and point it to the following directory :
 examples\Spring\SpringAir\src\SpringAir.Web.2003 

For .NET 2.0, 
The VS.NET solution is SpringAir.2005.sln



Set your startup project as SpringAir.Web  
and the start page to .\Web\Home.aspx

If you run .NET 2.0 first and then .NET 1.1, run the nant target 'clean-objs' to remove the object files before starting vs.net 2003
Or do a clean build from vs.net 2005 before starting vs.net 2003.
