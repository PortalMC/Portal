using Microsoft.Extensions.Configuration;
using Portal.Settings.ArtifactStorage;
using Portal.Settings.ForgeStorage;
using Portal.Settings.ProjectStorage;

namespace Portal.Settings
{
    public partial class StorageSetting
    {
        private readonly IProjectStorageSetting _projectStorageSetting;
        private readonly ArtifactStorageSetting _artifactStorageSetting;
        private readonly ForgeStorageSetting _forgeStorageSetting;

        public StorageSetting(IConfiguration configuration)
        {
            var projectStorageConfig = configuration.GetSection("Project");
            var artifactStorageConfig = configuration.GetSection("Artifact");
            var forgeStorageConfig = configuration.GetSection("Forge");
            var projectStorageType = ProjectStorageTypeFrom(projectStorageConfig.GetValue<string>("Method"));
            var artifactStorageType = ArtifactStorageTypeFrom(artifactStorageConfig.GetValue<string>("Method"));
            var forgeStorageType = ForgeStorageTypeFrom(forgeStorageConfig.GetValue<string>("Method"));
            _projectStorageSetting = ProjectStorageSettingFrom(projectStorageType, projectStorageConfig.GetSection("Config"));
            _artifactStorageSetting = ArtifactStorageSettingFrom(artifactStorageType, artifactStorageConfig.GetSection("Config"));
            _forgeStorageSetting = ForgeStorageSettingFrom(forgeStorageType, forgeStorageConfig.GetSection("Config"));
        }

        public IProjectStorageSetting GetProjectStorageSetting()
        {
            return _projectStorageSetting;
        }

        public ArtifactStorageSetting GetArtifactStorageSetting()
        {
            return _artifactStorageSetting;
        }

        public ForgeStorageSetting GetForgeStorageSetting()
        {
            return _forgeStorageSetting;
        }
    }
}