using System;

namespace PokemonRedux.Game.Data.Entities
{
    class NPCBehaviorData : ICloneable
    {
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public string type;
        public int[] x;
        public int[] y;
#pragma warning restore 0649

        public NPCBehaviorType Type => DataHelper.ParseEnum<NPCBehaviorType>(type);

        public object Clone()
        {
            var obj = (NPCBehaviorData)MemberwiseClone();
            obj.x = (int[])x.Clone();
            obj.y = (int[])y.Clone();
            return obj;
        }
    }
}
