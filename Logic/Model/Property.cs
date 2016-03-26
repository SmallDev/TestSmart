using System;

namespace Logic.Model
{
    public class Property
    {
        public PropertyType Type { get; set; }
        public Double Mean { get; set; }
        public Double Variance { get; set; }
        public String Value { get; set; }
    }
}