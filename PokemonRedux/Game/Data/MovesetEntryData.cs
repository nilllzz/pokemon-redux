using PokemonRedux.Game.Pokemons;

namespace PokemonRedux.Game.Data
{
    // part of a pokemon's level up/tm/breeding learnset
    public class MovesetEntryData
    {
        public string name;
        public int level;
        public bool tm;
        public bool breeding;

        internal PokemonMoveData ToPokemonMoveData()
        {
            var move = Move.Get(name);
            return new PokemonMoveData
            {
                name = move.Name,
                pp = move.MaxPP,
                maxPP = move.MaxPP
            };
        }
    }
}
