using PokemonRedux.Game.Pokemons;
using System;

namespace PokemonRedux.Game.Data
{
    // moves in the moveset of a pokemon
    class PokemonMoveData : ICloneable
    {
        public string name;
        public int pp;
        public int maxPP; // defaults to move's max pp, can be raised through pp ups/maxs

        public object Clone()
        {
            return (PokemonMoveData)MemberwiseClone();
        }

        public Move GetMove()
        {
            return Move.Get(name);
        }
    }
}
