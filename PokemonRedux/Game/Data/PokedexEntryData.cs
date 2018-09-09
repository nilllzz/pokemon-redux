using Newtonsoft.Json;
using PokemonRedux.Content;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Data
{
    class PokedexEntryData
    {
        private static PokedexEntryData[] _entries;

        public static PokedexEntryData[] GetAll()
        {
            if (_entries == null)
            {
                var contents = Controller.Content.LoadDirect<string>("Data/pokedex.json");
                _entries = JsonConvert.DeserializeObject<PokedexEntryData[]>(contents)
                    .OrderBy(e => e.id).ToArray(); // ordered by national id
            }

            return _entries;
        }

        public static PokedexEntryData Get(int id)
        {
            return _entries.First(e => e.id == id);
        }

        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public int id;
        public int newid;
        public string name;
        public string species;
        public double height;
        public double weigth;
        public string text;
        public string[] areas;
#pragma warning restore 0649
    }
}
