using PokemonRedux.Game.Pokemons;

namespace PokemonRedux.Game.Data
{
    class HallOfFameEntryData
    {
        public int number; // number of times the player has beaten the elite 4 when this was recorded
        // pokemon data
        public int id;
        public int experience;
        public int trainerId;
        public string nickname;
        public ushort dv;

        public static HallOfFameEntryData CreateFromPokemon(int number, Pokemon pokemon)
        {
            return new HallOfFameEntryData
            {
                number = number,

                id = pokemon.Id,
                experience = pokemon.Experience,
                nickname = pokemon.Nickname,
                trainerId = pokemon.TrainerID,
                dv = pokemon.GetSaveData().dv
            };
        }
    }
}
