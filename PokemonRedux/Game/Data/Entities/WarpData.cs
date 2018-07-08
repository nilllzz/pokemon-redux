using Microsoft.Xna.Framework;
using PokemonRedux.Game.Data;
using System;

namespace PokemonRedux.Game.Data.Entities
{
    class WarpData : ICloneable
    {
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public string map;
        public float[] position;
#pragma warning restore 0649

        public object Clone()
        {
            var obj = (WarpData)MemberwiseClone();
            obj.position = (float[])position?.Clone();
            return obj;
        }

        public Vector3 Position => DataHelper.GetVector3(position, 0f);
    }
}
