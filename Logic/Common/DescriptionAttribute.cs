using System;
using System.Reflection;

namespace Logic.Common
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DescriptionAttribute: Attribute
    {
        private readonly PropertyInfo property;
        public DescriptionAttribute(String name, Type resource)
        {
            property = resource.GetProperty(name, BindingFlags.Static | BindingFlags.NonPublic);
        }

        public String GetDescription()
        {
            return (String) property.GetValue(null, null);
        }
    }
}
