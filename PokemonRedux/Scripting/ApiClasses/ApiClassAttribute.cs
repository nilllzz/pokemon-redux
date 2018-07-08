using System;

namespace PokemonRedux.Scripting.ApiClasses
{
    class ApiClassAttribute : Attribute
    {
        public string ClassName { get; }

        public ApiClassAttribute(string className)
        {
            ClassName = className;
        }
    }
}
