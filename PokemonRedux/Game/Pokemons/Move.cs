using PokemonRedux.Game.Data;

namespace PokemonRedux.Game.Pokemons
{
    class Move
    {
        private MoveData _data;

        private Move(MoveData data)
        {
            _data = data;
        }

        public static Move Get(string name)
        {
            return new Move(MoveData.Get(name));
        }

        public string Name => _data.name;
        public string Description => _data.description;
        public int MaxPP => _data.maxPP;
        public PokemonType Type => DataHelper.ParseEnum<PokemonType>(_data.type);
        public int Attack => _data.attk;
        public bool IsHM => _data.isHM;
    }
}
