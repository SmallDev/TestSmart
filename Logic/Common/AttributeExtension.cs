using System;
using System.Linq;

namespace Logic.Common
{
    public static class AttributeExtension
    {
        public static String GetCode(this Enum element)
        {
            return GetAttribute<CodeAttribute>(element).Code;
        }
        private static T GetAttribute<T>(this Enum element) where T: Attribute
        {
            var type = element.GetType();
            var memberInfo = type.GetField(Enum.GetName(type, element));

            var attribute = (T)Attribute.GetCustomAttribute(memberInfo, typeof(T));
            if (attribute == null)
                throw new ArgumentException(String.Format("{0} attribute has not found", typeof(T).Name));

            return attribute;
        }

        public static TEnum? GetNullableElementByCode<TEnum>(this String code) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("T must be an enum type");

            var result = (TEnum?)typeof(TEnum).GetEnumValues().OfType<TEnum>().FirstOrDefault(
                value => typeof(TEnum).GetField(value.ToString()).GetCustomAttributes(typeof(CodeAttribute), false)
                             .Cast<CodeAttribute>().Any(attr => attr.Code == code));

            return typeof(TEnum).GetEnumValues().OfType<TEnum>().Any(
                value => typeof(TEnum).GetField(value.ToString()).GetCustomAttributes(typeof(CodeAttribute), false)
                             .Cast<CodeAttribute>().Any(attr => attr.Code == code))
                       ? result
                       : null;
        }
        public static TEnum GetElementByCode<TEnum>(this String code) where TEnum : struct
        {
            var result = GetNullableElementByCode<TEnum>(code);
            if (result == null)
                throw new ArgumentException("Element not found");

            return result.Value;
        }
    }
}