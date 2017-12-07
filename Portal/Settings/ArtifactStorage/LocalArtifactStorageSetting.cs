using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Portal.Extensions;

namespace Portal.Settings.ArtifactStorage
{
    public class LocalArtifactStorageSetting : ArtifactStorageSetting
    {
        private readonly DirectoryInfo _root;

        public LocalArtifactStorageSetting(IConfiguration configuration)
        {
            _root = new DirectoryInfo(configuration.GetValue<string>("Root"));
        }

        public override Task AfterBuildAsync(string projectId, FileInfo artifactFile, int buildId)
        {
            var destination = _root.ResolveDir(projectId);
            if (!destination.Exists)
            {
                destination.Create();
            }
            artifactFile.MoveTo(destination.Resolve($"{buildId}.jar").FullName);
            return Task.CompletedTask;
        }

        public override ArtifactProvideMethod GetArtifactProvideMethod()
        {
            return ArtifactProvideMethod.Stream;
        }

        public override Task<Stream> GetArtifactStreamAsync(string projectId, int buildId)
        {
            return Task.FromResult<Stream>(_root.ResolveDir(projectId).Resolve($"{buildId}.jar").OpenRead());
        }
    }
}