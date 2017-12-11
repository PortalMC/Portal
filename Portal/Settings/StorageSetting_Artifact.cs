using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Portal.Extensions;
using Portal.Settings.ArtifactStorage;
using static Portal.Extensions.EnumExtension;
using static Portal.Settings.ArtifactStorageSettingTypeEnumExtension;

namespace Portal.Settings
{
    public partial class StorageSetting
    {
        private static ArtifactStorageType ArtifactStorageTypeFrom(string value)
        {
            var type = Enum.GetValues(typeof(ArtifactStorageType)).Cast<ArtifactStorageType>().FirstOrDefault(t => t.GetValue() == value);
            if (type == default(ArtifactStorageType))
            {
                throw new ArgumentException($"Storages/Artifact value '{value}' is not defined.");
            }
            return type;
        }

        private static ArtifactStorageSetting ArtifactStorageSettingFrom(ArtifactStorageType storageType, IConfiguration configuration)
        {
            return (ArtifactStorageSetting) storageType.GetArtifactStorageSettingType().InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] {configuration});
        }
    }

    internal enum ArtifactStorageType
    {
        // ReSharper disable once UnusedMember.Global
        Undefined,
        [ArtifactStorageSettingType(typeof(LocalArtifactStorageSetting)), Value("Local")] Local,
        [ArtifactStorageSettingType(typeof(AzureBlobArtifactStorageSetting)), Value("AzureBlob")] AzureBlob
    }

    internal static class ArtifactStorageSettingTypeEnumExtension
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class ArtifactStorageSettingTypeAttribute : Attribute
        {
            internal Type Type { get; }

            public ArtifactStorageSettingTypeAttribute(Type type)
            {
                Type = type;
            }
        }

        public static Type GetArtifactStorageSettingType(this Enum value)
        {
            return value.GetAttribute<ArtifactStorageSettingTypeAttribute>()?.Type;
        }
    }
}