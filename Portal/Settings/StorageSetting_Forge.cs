using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Portal.Extensions;
using Portal.Settings.ForgeStorage;
using static Portal.Extensions.EnumExtension;
using static Portal.Settings.ForgeStorageSettingTypeEnumExtension;

namespace Portal.Settings
{
    public partial class StorageSetting
    {
        private static ForgeStorageType ForgeStorageTypeFrom(string value)
        {
            var type = Enum.GetValues(typeof(ForgeStorageType)).Cast<ForgeStorageType>().FirstOrDefault(t => t.GetValue() == value);
            if (type == default(ForgeStorageType))
            {
                throw new ArgumentException($"Storages/Forge value '{value}' is not defined.");
            }
            return type;
        }

        private static ForgeStorageSetting ForgeStorageSettingFrom(ForgeStorageType storageType, IConfiguration configuration)
        {
            return (ForgeStorageSetting) storageType.GetForgeStorageSettingType().InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] {configuration});
        }
    }

    public enum ForgeStorageType
    {
        // ReSharper disable once UnusedMember.Global
        Undefined,
        [ForgeStorageSettingType(typeof(LocalForgeStorageSetting)), Value("Local")] Local,
    }

    internal static class ForgeStorageSettingTypeEnumExtension
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class ForgeStorageSettingTypeAttribute : Attribute
        {
            internal Type Type { get; }

            public ForgeStorageSettingTypeAttribute(Type type)
            {
                Type = type;
            }
        }

        public static Type GetForgeStorageSettingType(this Enum value)
        {
            return value.GetAttribute<ForgeStorageSettingTypeAttribute>()?.Type;
        }
    }
}