using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portal.Extensions;
using Portal.Services;
using static Portal.Extensions.EnumExtension;
using static Portal.Settings.BuildMethodTypeEnumExtension;

namespace Portal.Settings
{
    public class BuildSetting
    {
        private readonly BuildMethodType _buildMethodType;
        private readonly object _buildMethodSetting;

        public BuildSetting(IConfiguration configuration)
        {
            _buildMethodType = BuildMethodTypeFrom(configuration.GetValue<string>("Method"));
            _buildMethodSetting = BuildMethodSettingFrom(_buildMethodType, configuration.GetSection("Config"));
        }

        public BuildMethodType GetBuildMethodType()
        {
            return _buildMethodType;
        }

        public T GetBuildMethodSetting<T>()
        {
            return (T) _buildMethodSetting;
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


    public enum BuildMethodType
    {
        // ReSharper disable once UnusedMember.Global
        Undefined,
        [BuildSettingType(typeof(DockerBuildMethodSetting)), Value("Docker")] Docker
    }

    internal static class BuildMethodTypeEnumExtension
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class BuildSettingTypeAttribute : Attribute
        {
            internal Type Type { get; }

            public BuildSettingTypeAttribute(Type type)
            {
                Type = type;
            }
        }

        public static Type GetBuildMethodSettingType(this Enum value)
        {
            return value.GetAttribute<BuildSettingTypeAttribute>()?.Type;
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