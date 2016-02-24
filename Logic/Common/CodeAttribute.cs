using System;

namespace Logic.Common
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class CodeAttribute : Attribute
    {
        public String Code { get; private set; }

        public CodeAttribute(String code)
        {
            Code = code;
        }
    }
}
