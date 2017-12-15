using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Portal.Extensions;
using Portal.Settings.CoremodStorage;
using static Portal.Extensions.EnumExtension;
using static Portal.Settings.CoremodStorageSettingTypeEnumExtension;

namespace Portal.Settings
{
    public partial class StorageSetting
    {
        private static CoremodStorageType CoremodStorageTypeFrom(string value)
        {
            var type = Enum.GetValues(typeof(CoremodStorageType)).Cast<CoremodStorageType>().FirstOrDefault(t => t.GetValue() == value);
            if (type == default(CoremodStorageType))
            {
                throw new ArgumentException($"Storages/Coremod value '{value}' is not defined.");
            }
            return type;
        }

        private static CoremodStorageSetting CoremodStorageSettingFrom(CoremodStorageType storageType, IConfiguration configuration)
        {
            return (CoremodStorageSetting) storageType.GetCoremodStorageSettingType().InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] {configuration});
        }
    }

    public enum CoremodStorageType
    {
        // ReSharper disable once UnusedMember.Global
        Undefined,
        [CoremodStorageSettingType(typeof(LocalCoremodStorageSetting)), Value("Local")] Local,
    }

    internal static class CoremodStorageSettingTypeEnumExtension
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class CoremodStorageSettingTypeAttribute : Attribute
        {
            internal Type Type { get; }

            public CoremodStorageSettingTypeAttribute(Type type)
            {
                Type = type;
            }
        }

        public static Type GetCoremodStorageSettingType(this Enum value)
        {
            return value.GetAttribute<CoremodStorageSettingTypeAttribute>()?.Type;
        }
    }
}