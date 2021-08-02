using Nuke.Common;

public partial class Build
{
    Target Ci => _ => _
        .DependsOn(Compile, Test, Pack)
        .Executes();
}