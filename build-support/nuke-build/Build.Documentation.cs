using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

public partial class Build
{
    readonly AbsolutePath ReferenceDocumentationPath = RootDirectory / "doc" / "reference";

    Target Docs => _ => _
        .DependsOn(DocsReference, DocsSdk)
        .Executes(() =>
        {
        });

    Target DocsReference => _ => _
        .Executes(() =>
        {
            var targetDir = ReferenceDocumentationPath / "target";
            targetDir.CreateOrCleanDirectory();

            ProcessTasks.StartProcess(
                workingDirectory: targetDir,
                toolPath: "java",
                arguments: """
                               -cp "../lib/saxon6-5-5/saxon.jar:../lib/xslthl-2.0.0/xslthl-2.0.0.jar" com.icl.saxon.StyleSheet ../src/index.xml "../lib/docbook-xsl-snapshot/html/springnet.xsl" highlight.xslthl.config="file:///${project.basedir}/lib/docbook-xsl-snapshot/highlighting/xslthl-config.xml"
                           """
            ).AssertWaitForExit();

            FileSystemTasks.CopyDirectoryRecursively(ReferenceDocumentationPath / "src" / "styles", targetDir / "html" / "styles", DirectoryExistsPolicy.Merge);
            FileSystemTasks.CopyDirectoryRecursively(ReferenceDocumentationPath / "src" / "images", targetDir / "html" / "images", DirectoryExistsPolicy.Merge, excludeFile: file => file.Extension != ".gif" && file.Extension != ".svg" && file.Extension != ".jpg" && file.Extension != ".png");
            FileSystemTasks.CopyDirectoryRecursively(ReferenceDocumentationPath / "lib" / "docbook-xsl-snapshot" / "images", targetDir / "html" / "images", DirectoryExistsPolicy.Merge);
        });

    Target DocsSdk => _ => _
        .Executes(() =>
        {
        });

    Target Website => _ => _
        .DependsOn(DocsReference, DocsSdk)
        .Executes(() =>
        {
            var webSiteDir = RootDirectory / "website";
            ProcessTasks.StartProcess("yarn", arguments: "install", workingDirectory: webSiteDir).AssertWaitForExit();
            ProcessTasks.StartProcess("yarn", arguments: "build", workingDirectory: webSiteDir).AssertWaitForExit();

            // copy reference docs
            FileSystemTasks.CopyDirectoryRecursively(ReferenceDocumentationPath / "target" / "html", webSiteDir / "public" / "doc-latest" / "reference" / "html", DirectoryExistsPolicy.Merge);
        });
}
