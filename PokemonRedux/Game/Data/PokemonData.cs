using Newtonsoft.Json;
using PokemonRedux.Content;
using System.Collections.Generic;
using static Core;

namespace PokemonRedux.Game.Data
{
    public class PokemonData
    {
        public class Palette
        {
            public int[][] normal;
            public int[][] shiny;
        }

        private static Dictionary<int, PokemonData> _dataBuffer = new Dictionary<int, PokemonData>();

        public static PokemonData Get(int id)
        {
            if (!_dataBuffer.TryGetValue(id, out var data))
            {
                var contents = Controller.Content.LoadDirect<string>($"Data/Pokemon/{id}.json");
                data = JsonConvert.DeserializeObject<PokemonData>(contents);
                _dataBuffer.Add(id, data);
            }

            return data;
        }

        public int id;
        public string name;
        public string gender;
        public int[] baseStats; // 0 => HP, 1 => ATK, 2 => DEF, 3 => SPCDEF, 4 => SPCATK, 5 => SPD
        public string experienceType;
        public int experienceYield;
        public string type1;
        public string type2;
        public string[] eggGroups;
        public int eggCycles;
        public int catchRate;
        public double wildFleeRate; // 0-1, chance of trying to flee in a wild battle
        public MovesetEntryData[] moves;
        public Palette colors;

        internal PokemonGenderNominalRatio GenderRatio => DataHelper.ParseEnum<PokemonGenderNominalRatio>(gender);
        internal ExperienceType ExperienceType => DataHelper.ParseEnum<ExperienceType>(experienceType);
    }
}
