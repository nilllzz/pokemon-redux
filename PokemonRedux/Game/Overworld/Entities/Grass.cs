using Microsoft.Xna.Framework;
using PokemonRedux.Game.Data.Entities;

namespace PokemonRedux.Game.Overworld.Entities
{
    class Grass : Prop
    {
        public Grass(EntityData data)
            : base(data)
        { }

        public Grass(EntityData data, Vector2 fieldSize, Vector2 steps)
            : base(data, fieldSize, steps)
        { }
    }
}
