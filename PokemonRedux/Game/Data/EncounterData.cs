using PokemonRedux.Game.Overworld;
using System.Linq;

namespace PokemonRedux.Game.Data
{
    class EncounterData
    {
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public int id; // pokedex no
        public string method;
        public int[] levels; // used if no levels are defined in the rates (or no rates are defined)
        public int chance; // flat rate for all times
        public EncounterRateData[] rates; // different rates and possibly levels per time
#pragma warning restore 0649

        private EncounterMethod Method => DataHelper.ParseEnum<EncounterMethod>(method);

        public bool DoesApply(EncounterMethod method, Daytime daytime)
        {
            // different method?
            if (Method != method)
            {
                return false;
            }

            // if no special rates are defined, daytime does not matter
            if (rates == null || rates.Length == 0)
            {
                return true;
            }

            // if the daytime is included in the rates, this data applies
            return rates.Any(r => r.Time.Contains(daytime));
        }

        public EncounterRateData GetRate(Daytime daytime)
        {
            if (rates == null || rates.Length == 0)
            {
                var rate = new EncounterRateData
                {
                    chance = chance,
                    levels = levels,
                };
                rate.SetParent(this);
                return rate;
            }
            else
            {
                var rate = rates.First(r => r.Time.Contains(daytime));
                // when no levels are set in the rate, overwrite with levels of the encounter data
                if (rate.levels == null || rate.levels.Length == 0)
                {
                    rate.levels = levels;
                }
                rate.SetParent(this);
                return rate;
            }
        }
    }
}
