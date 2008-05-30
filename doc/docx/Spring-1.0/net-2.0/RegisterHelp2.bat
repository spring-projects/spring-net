REM Register the Namespace
InnovaHxReg /R /N /Namespace:Spring.NET-1.0.2.2 /Description:"Spring.Aop" /Collection:COL_Spring-1.0.2.hxc

REM Register the help file (title in Help 2.0 terminology)
InnovaHxReg /R /T /namespace:Spring.NET-1.0.2.2 /id:Spring.NET-1.0.2-.NET-2.0 /langid:1033 /helpfile:Spring.NET.hxs

REM Plug in to the Visual Studio.NET 2005 help system
InnovaHxReg /R /P /productnamespace:MS.VSIPCC.v80 /producthxt:_DEFAULT /namespace:Spring.NET-1.0.2.2 /hxt:_DEFAULT