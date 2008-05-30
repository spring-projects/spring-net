REM Register the Namespace
InnovaHxReg /R /N /Namespace:Spring.NET /Description:"Spring.Aop" /Collection:COL_Spring-1.0.2.hxc

REM Register the help file (title in Help 2.0 terminology)
InnovaHxReg /R /T /namespace:Spring.NET /id:Spring.NET-1.0.2 /langid:1033 /helpfile:Spring.NET.hxs

REM Plug in to the Visual Studio.NET 2002 help system
InnovaHxReg /R /P /productnamespace:MS.VSCC /producthxt:_DEFAULT /namespace:Spring.NET /hxt:_DEFAULT

REM Plug in to the Visual Studio.NET 2003 help system
InnovaHxReg /R /P /productnamespace:MS.VSCC.2003 /producthxt:_DEFAULT /namespace:Spring.NET /hxt:_DEFAULT

