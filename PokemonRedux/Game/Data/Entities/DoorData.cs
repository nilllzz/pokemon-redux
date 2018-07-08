using System;

namespace PokemonRedux.Game.Data.Entities
{
    class DoorData : ICloneable
    {
        public WarpData warpData;
        public bool hidden = false;

        public object Clone()
        {
            var obj = (DoorData)MemberwiseClone();
            obj.warpData = (WarpData)warpData?.Clone();
            return obj;
        }
    }
}
