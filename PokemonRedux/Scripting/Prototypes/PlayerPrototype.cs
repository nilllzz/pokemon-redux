using static Core;
using Kolben.Adapters;

namespace PokemonRedux.Scripting.Prototypes
{
    [ScriptPrototype(VariableName = "Player")]
    sealed class PlayerPrototype
    {
        [ScriptFunction(ScriptFunctionType.Getter, VariableName = "name", IsStatic = true)]
        public static object GetName(object This, ScriptObjectLink objLink, object[] parameters)
        {
            return Controller.ActivePlayer.Name;
        }
    }
}
