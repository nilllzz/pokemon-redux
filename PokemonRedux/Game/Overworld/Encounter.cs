using Newtonsoft.Json;
using PokemonRedux.Content;
using PokemonRedux.Game.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Overworld
{
    class Encounter
    {
        private static Dictionary<string, Encounter> _dataBuffer = new Dictionary<string, Encounter>();
        private static Random _random = new Random();

        public static Encounter Get(string file)
        {
            var key = file.ToLower();
            if (!_dataBuffer.TryGetValue(key, out var encounter))
            {
                var contents = Controller.Content.LoadDirect<string>(file);
                var data = JsonConvert.DeserializeObject<EncounterData[]>(contents);
                encounter = new Encounter(data);
                _dataBuffer.Add(key, encounter);
            }
            return encounter;
        }

        private Encounter(EncounterData[] data)
        {
            _data = data;
        }

        private readonly EncounterData[] _data;

        // returns the id and level of a pokemon according to the encounter table
        // if no encounter data is present for the input data, null is returned.
        public EncounterResult? GetResult(EncounterMethod method, Daytime daytime)
        {
            var applicable = _data.Where(d => d.DoesApply(method, daytime));

            // no data found for the input
            if (applicable.Count() == 0)
            {
                return null;
            }

            // pick a random pokemon based on its encounter chance
            var rates = applicable.Select(a => a.GetRate(daytime)).ToArray();
            var totalChance = rates.Sum(r => r.chance);
            var pick = _random.Next(0, totalChance) + 1;
            var runner = 0;
            for (var i = 0; i < rates.Length; i++)
            {
                var rate = rates[i];
                runner += rate.chance;
                if (runner >= pick)
                {
                    var id = rate.GetPokemonId();
                    var (minLevel, maxLevel) = rate.GetLevelRange();
                    var level = _random.Next(minLevel, maxLevel + 1);
                    return new EncounterResult { Id = id, Level = level };
                }
            }

            // fallback
            return null;
        }
    }
}
