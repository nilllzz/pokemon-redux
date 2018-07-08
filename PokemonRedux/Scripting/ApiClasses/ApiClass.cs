using Kolben.Adapters;
using Kolben.Types;
using System;

namespace PokemonRedux.Scripting.ApiClasses
{
    abstract class ApiClass
    {
        protected static bool EnsureTypeContract(SObject[] parameters, Type[] typeContract, out object[] netObjects, int optionalCount = 0)
        {
            if (parameters.Length + optionalCount >= typeContract.Length)
            {
                netObjects = new object[parameters.Length];
                var i = 0;

                while (i < parameters.Length)
                {
                    netObjects[i] = ScriptOutAdapter.Translate(parameters[i]);

                    if (i < typeContract.Length && typeContract[i] != netObjects[i]?.GetType())
                    {
                        return false;
                    }

                    i++;
                }

                return true;
            }
            else
            {
                netObjects = null;
                return false;
            }
        }
    }
}
