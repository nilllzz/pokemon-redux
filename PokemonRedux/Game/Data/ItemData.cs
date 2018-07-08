using Newtonsoft.Json;
using PokemonRedux.Content;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Data
{
    class ItemData
    {
        private static ItemData[] _items;

        public static ItemData Get(string name)
        {
            if (_items == null)
            {
                var contents = Controller.Content.LoadDirect<string>("Data/items.json");
                _items = JsonConvert.DeserializeObject<ItemData[]>(contents);
            }

            return _items.First(i => i.name.ToLower() == name.ToLower());
        }

        // definition
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public string name;
        public string description;
        public string pocket;
        public int cost; // when buying, selling is half
        public int machineNumber;
        public bool canUseField;
        public bool canUseBattle;
        public bool canBeRegistered; // used by select in field
#pragma warning restore 0649
    }
}
