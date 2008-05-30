REM Un-comment to remove the plug in to the Visual Studio.NET 2005 help system
InnovaHxReg /U /P /productnamespace:MS.VSIPCC.v80 /producthxt:_DEFAULT /namespace:Spring.NET-1.0.2.2 /hxt:_DEFAULT

REM Un-Register the help file (title in Help 2.0 terminology)
InnovaHxReg /U /T /namespace:Spring.NET-1.0.2.2 /id:Spring.NET-1.0.2-.NET-2.0 /langid:1033 /helpfile:Spring.NET.hxs

REM Un-Register the Namespace
InnovaHxReg /U /N /Namespace:Spring.NET-1.0.2.2 /Description:"Spring.Aop" /Collection:COL_Spring-1.0.2.hxc
