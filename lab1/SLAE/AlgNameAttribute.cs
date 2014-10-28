using System;

namespace SLAE
{
    public class AlgNameAttribute : Attribute
    {
        public readonly string Name;

        public AlgNameAttribute(string name)
        {
            Name = name;
        }
    }
}
