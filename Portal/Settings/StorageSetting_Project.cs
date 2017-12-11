using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Portal.Extensions;
using Portal.Settings.ProjectStorage;
using static Portal.Extensions.EnumExtension;
using static Portal.Settings.ProjectStorageSettingTypeEnumExtension;

namespace Portal.Settings
{
    public partial class StorageSetting
    {
        private static ProjectStorageType ProjectStorageTypeFrom(string value)
        {
            var type = Enum.GetValues(typeof(ProjectStorageType)).Cast<ProjectStorageType>().FirstOrDefault(t => t.GetValue() == value);
            if (type == default(ProjectStorageType))
            {
                throw new ArgumentException($"Storages/Project value '{value}' is not defined.");
            }
            return type;
        }

        private static IProjectStorageSetting ProjectStorageSettingFrom(ProjectStorageType storageType, IConfiguration configuration)
        {
            return (IProjectStorageSetting) storageType.GetProjectStorageSettingType().InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] {configuration});
        }
    }

    internal enum ProjectStorageType
    {
        // ReSharper disable once UnusedMember.Global
        Undefined,
        [ProjectStorageSettingType(typeof(LocalProjectStorageSetting)), Value("Local")] Local,
    }

    internal static class ProjectStorageSettingTypeEnumExtension
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

        public static Type GetProjectStorageSettingType(this Enum value)
        {
            return value.GetAttribute<ProjectStorageSettingTypeAttribute>()?.Type;
        }
    }
}