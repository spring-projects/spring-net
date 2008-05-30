This folder is where source code for all subprojects of Spring.Net are located.

There should be one subproject under this directory for each of the assemblies
that will be produced when Spring.Net is built. Each subproject should be named
exactly how the target assembly should be named, without .dll at the end.

Currently identified subprojects are:

Each subproject should set root namespace to "Spring". Source code should always be in subdirectories matching
the namespace. For example, Spring.Aop subproject will have following hierarchy:

\Spring.Aop
	\Aop				
		\Framework		
			\AutoProxy
			...
		\Interceptor
		\Support
		\Target
		...
	\Spring.Aop.2003.csproj	(project file)
	\AssemblyInfo.cs

Other subprojects should have similar structure.

