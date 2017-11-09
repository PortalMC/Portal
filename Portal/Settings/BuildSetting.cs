using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portal.Services;
using static Portal.Settings.TypeEnumExtension;

namespace Portal.Settings
{
    public class BuildSetting
    {
        private readonly StorageType _storageType;
        private readonly IBuildStorageSetting _buildStorageSetting;
        private readonly BuildMethodType _buildMethodType;
        private readonly object _buildMethodSetting;

        public BuildSetting(IConfiguration configuration)
        {
            _storageType = StorageTypeFrom(configuration.GetValue<string>("Storage"));
            _buildStorageSetting = BuildStorageSettingFrom(_storageType, configuration.GetSection("StorageConfig"));
            _buildMethodType = BuildMethodTypeFrom(configuration.GetValue<string>("Method"));
            _buildMethodSetting = BuildMethodSettingFrom(_buildMethodType, configuration.GetSection("BuildConfig"));
        }

        public StorageType GetStorageType()
        {
            return _storageType;
        }

        public IBuildStorageSetting GetBuildStorageSetting()
        {
            return _buildStorageSetting;
        }

        public BuildMethodType GetBuildMethodType()
        {
            return _buildMethodType;
        }

        public T GetBuildMethodSetting<T>()
        {
            return (T) _buildMethodSetting;
        }

        private static StorageType StorageTypeFrom(string value)
        {
            var type = Enum.GetValues(typeof(StorageType)).Cast<StorageType>().FirstOrDefault(t => t.GetValue() == value);
            if (type == default(StorageType))
            {
                throw new ArgumentException($"Builds/Storage value '{value}' is not defined.");
            }
            return type;
        }

        private static IBuildStorageSetting BuildStorageSettingFrom(StorageType storageType, IConfiguration configuration)
        {
            return (IBuildStorageSetting) storageType.GetStorageSettingType().InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] {configuration});
        }

        private static BuildMethodType BuildMethodTypeFrom(string value)
        {
            var type = Enum.GetValues(typeof(BuildMethodType)).Cast<BuildMethodType>().FirstOrDefault(t => t.GetValue() == value);
            if (type == default(BuildMethodType))
            {
                throw new ArgumentException($"Builds/Method value '{value}' is not defined.");
            }
            return type;
        }

        private static object BuildMethodSettingFrom(BuildMethodType buildMethodType, IConfiguration configuration)
        {
            return buildMethodType.GetBuildMethodSettingType().InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] {configuration});
        }
    }

    public enum StorageType
    {
        // ReSharper disable once UnusedMember.Global
        Undefined,
        [StorageSettingType(typeof(LocalBuildStorageSetting)), Value("Local")] Local,
        [StorageSettingType(typeof(AzureBlobBuildStorageSetting)), Value("AzureBlob")] AzureBlob
    }

    public enum BuildMethodType
    {
        // ReSharper disable once UnusedMember.Global
        Undefined,
        [BuildSettingType(typeof(DockerBuildMethodSetting)), Value("Docker")] Docker
    }

    internal static class TypeEnumExtension
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class StorageSettingTypeAttribute : Attribute
        {
            internal Type Type { get; }

            public StorageSettingTypeAttribute(Type type)
            {
                Type = type;
            }
        }

        [AttributeUsage(AttributeTargets.Field)]
        public class BuildSettingTypeAttribute : Attribute
        {
            internal Type Type { get; }

            public BuildSettingTypeAttribute(Type type)
            {
                Type = type;
            }
        }

        public static Type GetStorageSettingType(this Enum value)
        {
            return value.GetAttribute<StorageSettingTypeAttribute>()?.Type;
        }

        public static Type GetBuildMethodSettingType(this Enum value)
        {
            return value.GetAttribute<BuildSettingTypeAttribute>()?.Type;
        }

        [AttributeUsage(AttributeTargets.Field)]
        public class ValueAttribute : Attribute
        {
            internal string Value { get; }

            public ValueAttribute(string value)
            {
                Value = value;
            }
        }

        public static string GetValue(this Enum value)
        {
            return value.GetAttribute<ValueAttribute>()?.Value;
        }

        private static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attributes = fieldInfo.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>();
            var enumerable = attributes as TAttribute[] ?? attributes.ToArray();
            return !enumerable.Any() ? null : enumerable.First();
        }
    }

    public static class BuildServiceCollectionExtension
    {
        public static void AddBuildService(this IServiceCollection services, BuildMethodType type)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            switch (type)
            {
                case BuildMethodType.Docker:
                    services.AddSingleton<IBuildService, DockerBuildService>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}