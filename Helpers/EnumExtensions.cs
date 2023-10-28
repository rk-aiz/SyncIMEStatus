using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace SyncIMEStatus.Helpers
{
    internal static class EnumExtension
    {
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).OfType<T>().SingleOrDefault();
        }
        
        
        public static IEnumerable<T> GetFlagMembers<T>(this T self) where T : struct, Enum, IConvertible
        {
            _ = typeof(T).GetCustomAttributes<FlagsAttribute>() ?? throw new NotSupportedException("This type does not have 'FlagsAttribute'. ");
            var a = Convert.ToInt64(self);

            foreach (T m in Enum.GetValues(typeof(T)))
            {
                var b = Convert.ToInt64(m);
                if ((b & a) == b)
                {
                    yield return m;
                }
            }
        }
        

        /*
        public static IEnumerable<T> GetFlagMembers<T>(this T member)
        {
            _ = typeof(T).GetCustomAttributes<FlagsAttribute>() ?? throw new NotSupportedException("This type does not have 'FlagsAttribute'. ");
            return member.GetFlagMembers();
        }
        */
    }
}
