using PokemonRedux.Game.Data;
using System;

namespace PokemonRedux.Game.Data.Entities
{
    class ScriptData : ICloneable
    {
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public string file;
        public string triggerType;
#pragma warning restore 0649

        public ScriptTriggerType TriggerType => DataHelper.ParseEnum<ScriptTriggerType>(triggerType);

        public object Clone()
        {
            var obj = (ScriptData)MemberwiseClone();
            return obj;
        }
    }
}
