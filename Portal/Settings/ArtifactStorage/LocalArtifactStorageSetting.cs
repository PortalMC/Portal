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

        public override DirectoryInfo GetRootDirectory()
        {
            return _root;
        }

        public override Task AfterBuildAsync(string projectId)
        {
            return Task.CompletedTask;
        }

        public override ArtifactProvideMethod GetArtifactProvideMethod()
        {
            return ArtifactProvideMethod.Stream;
        }

        public override Task<Stream> GetArtifactStreamAsync(string projectId, string buildId)
        {
            return Task.FromResult<Stream>(GetRootDirectory().ResolveDir(projectId).Resolve($"{buildId}.jar").OpenRead());
        }
    }
}