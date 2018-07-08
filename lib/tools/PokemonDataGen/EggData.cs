using Newtonsoft.Json;
using System;
using System.IO;

namespace PokemonDataGen
{
    class EggData
    {
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public int id;
        public int eggCycles;
        public string[] eggGroups;
#pragma warning restore 0649

        public static EggData[] Load()
        {
            var contents = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eggdata.json"));
            return JsonConvert.DeserializeObject<EggData[]>(contents);
        }

        public static string FormatEggGroup(string input)
        {
            var group = input.Replace(" ", "").Replace("-", "");
            return group[0].ToString().ToLower() + group.Substring(1);
        }
    }
}
