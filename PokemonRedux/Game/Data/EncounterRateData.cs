using PokemonRedux.Game.Overworld;

namespace PokemonRedux.Game.Data
{
    class EncounterRateData
    {
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public string time;
        public int chance; // in % (0-100)
        public int[] levels;
#pragma warning restore 0649

        private EncounterData _parent;

        public Daytime Time => DataHelper.ParseEnum<Daytime>(time);

        public void SetParent(EncounterData data)
        {
            _parent = data;
        }

        public int GetPokemonId() => _parent.id;

        public (int min, int max) GetLevelRange()
        {
            if (levels.Length == 1)
            {
                return (levels[0], levels[0]);
            }
            else
            {
                return (levels[0], levels[1]);
            }
        }
    }
}
