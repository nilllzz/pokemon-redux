using Newtonsoft.Json;
using PokemonRedux.Content;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Data
{
    class MoveData
    {
        private static MoveData[] _moves;

        public static MoveData Get(string name)
        {
            if (_moves == null)
            {
                var contents = Controller.Content.LoadDirect<string>("Data/moves.json");
                _moves = JsonConvert.DeserializeObject<MoveData[]>(contents);
            }

            return _moves.First(m => m.name == name);
        }

        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public string name;
        public string type;
        public int maxPP;
        public bool isHM;
        public string description;
        public int attk;
#pragma warning restore 0649
    }
}
