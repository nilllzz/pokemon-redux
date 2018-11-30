using System;
using static Core;

namespace PokemonRedux.Game.Data.Entities
{
    class NPCData : ICloneable
    {
        private const string PLACEHOLDER_PLAYER_NAME = "<player>";

        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public string type;
        public string text;
        public string script;
        public NPCBehaviorData behavior;
#pragma warning restore 0649

        public NPCType Type => DataHelper.ParseEnum<NPCType>(type);

        public string GetText()
        {
            // fill player name
            var t = text.Replace(PLACEHOLDER_PLAYER_NAME, Controller.ActivePlayer.Name);
            return t;
        }

        public object Clone()
        {
            var obj = (NPCData)MemberwiseClone();
            obj.behavior = (NPCBehaviorData)behavior.Clone();
            return obj;
        }
    }
}
