using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Docker.DockerTasks;
using Nuke.Docker;
using Nuke.Common.Tooling;

partial class Build : NukeBuild
{
    // Docker image naming
    [Parameter("Docker image prefix for Sitecore base")]
    readonly string BaseImagePrefix = "sitecore-base-";

    [Parameter("Docker image version tag for Sitecore base")]
    readonly string BaseVersion = "1.0.0-ltsc2019";

    private string BaseFullImageName(string name) => $"{RepoImagePrefix}{BaseImagePrefix}{name}:{BaseVersion}";
    
    Target BaseOpenJdk => _ => _
        .Executes(() =>
        {
            DockerBuild(x => x
                .SetPath(".")
                .SetFile("base/openjdk/Dockerfile")
                .SetTag(BaseFullImageName("openjdk"))
            );
        });

    Target BaseSitecore => _ => _
        .Executes(() =>
        {
            DockerBuild(x => x
                .SetPath(".")
                .SetFile("base/sitecore/Dockerfile")
                .SetTag(BaseFullImageName("sitecore"))
            );
        });

    Target BaseSolrBuilder => _ => _
        .DependsOn(BaseSitecore)
        .Executes(() =>
        {
            var baseImage = BaseFullImageName("sitecore");

            DockerBuild(x => x
                .SetPath(".")
                .SetFile("base/solr-builder/Dockerfile")
                .SetTag(BaseFullImageName("solr-builder"))
                .SetBuildArg(new string[] {
                    $"BASE_IMAGE={baseImage}"
                })
            );
        });

    Target Base => _ => _
        .DependsOn(BaseOpenJdk, BaseSitecore);

    Target PushBase => _ => _
        .Executes(() => {
            DockerPush(x => x.SetName(BaseFullImageName("openjdk")));
            DockerPush(x => x.SetName(BaseFullImageName("sitecore")));
        });
}
