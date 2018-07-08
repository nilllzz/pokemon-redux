using Newtonsoft.Json;
using PokemonRedux.Content;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Data
{
    class Phonebook
    {
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public Contact[] contacts;
#pragma warning restore 0649

        public static Phonebook Load()
        {
            var contents = Controller.Content.LoadDirect<string>("Data/phonebook.json");
            return JsonConvert.DeserializeObject<Phonebook>(contents);
        }

        public Contact GetContact(string name)
        {
            return contacts.First(c => c.name.ToLower() == name.ToLower());
        }
    }
}
