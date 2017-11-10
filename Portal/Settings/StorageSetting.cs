using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Portal.Extensions;
using Portal.Settings.ArtifactStorage;
using Portal.Settings.ProjectStorage;
using static Portal.Extensions.EnumExtension;
using static Portal.Settings.StorageSettingTypeEnumExtension;

namespace Portal.Settings
{
    public class StorageSetting
    {
        private readonly IProjectStorageSetting _projectStorageSetting;
        private readonly ArtifactStorageSetting _artifactStorageSetting;

        public StorageSetting(IConfiguration configuration)
        {
            var projectStorageConfig = configuration.GetSection("Project");
            var artifactStorageConfig = configuration.GetSection("Artifact");
            var projectStorageType = ProjectStorageTypeFrom(projectStorageConfig.GetValue<string>("Method"));
            var artifactStorageType = ArtifactStorageTypeFrom(artifactStorageConfig.GetValue<string>("Method"));
            _projectStorageSetting = ProjectStorageSettingFrom(projectStorageType, projectStorageConfig.GetSection("Config"));
            _artifactStorageSetting = ArtifactStorageSettingFrom(artifactStorageType, artifactStorageConfig.GetSection("Config"));
        }

        public IProjectStorageSetting GetProjectStorageSetting()
        {
            return _projectStorageSetting;
        }

        public ArtifactStorageSetting GetArtifactStorageSetting()
        {
            return _artifactStorageSetting;
        }

        private static ProjectStorageType ProjectStorageTypeFrom(string value)
        {
            var type = Enum.GetValues(typeof(ProjectStorageType)).Cast<ProjectStorageType>().FirstOrDefault(t => t.GetValue() == value);
            if (type == default(ProjectStorageType))
            {
                throw new ArgumentException($"Storages/Project value '{value}' is not defined.");
            }
            return type;
        }

        private static ArtifactStorageType ArtifactStorageTypeFrom(string value)
        {
            var type = Enum.GetValues(typeof(ArtifactStorageType)).Cast<ArtifactStorageType>().FirstOrDefault(t => t.GetValue() == value);
            if (type == default(ArtifactStorageType))
            {
                throw new ArgumentException($"Storages/Artifact value '{value}' is not defined.");
            }
            return type;
        }

        private static IProjectStorageSetting ProjectStorageSettingFrom(ProjectStorageType storageType, IConfiguration configuration)
        {
            return (IProjectStorageSetting) storageType.GetProjectStorageSettingType().InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] {configuration});
        }

        private static ArtifactStorageSetting ArtifactStorageSettingFrom(ArtifactStorageType storageType, IConfiguration configuration)
        {
            return (ArtifactStorageSetting) storageType.GetArtifactStorageSettingType().InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] {configuration});
        }
    }

    public enum ProjectStorageType
    {
        // ReSharper disable once UnusedMember.Global
        Undefined,
        [ProjectStorageSettingType(typeof(LocalProjectStorageSetting)), Value("Local")] Local,
    }

    public enum ArtifactStorageType
    {
        // ReSharper disable once UnusedMember.Global
        Undefined,
        [ArtifactStorageSettingType(typeof(LocalArtifactStorageSetting)), Value("Local")] Local,
        [ArtifactStorageSettingType(typeof(AzureBlobArtifactStorageSetting)), Value("AzureBlob")] AzureBlob
    }

    public static class StorageSettingTypeEnumExtension
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class ProjectStorageSettingTypeAttribute : Attribute
        {
            internal Type Type { get; }

            public ProjectStorageSettingTypeAttribute(Type type)
            {
                Type = type;
            }
        }

        [AttributeUsage(AttributeTargets.Field)]
        public class ArtifactStorageSettingTypeAttribute : Attribute
        {
            internal Type Type { get; }

            public ArtifactStorageSettingTypeAttribute(Type type)
            {
                Type = type;
            }
        }

        public static Type GetProjectStorageSettingType(this Enum value)
        {
            return value.GetAttribute<ProjectStorageSettingTypeAttribute>()?.Type;
        }

        public static Type GetArtifactStorageSettingType(this Enum value)
        {
            return value.GetAttribute<ArtifactStorageSettingTypeAttribute>()?.Type;
        }
    }
}