using PokemonRedux.Game.Data;
using PokemonRedux.Screens.Pokedex;
using System;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Pokemons
{
    class PokedexEntry
    {
        private readonly PokedexEntryData _data;

        private PokedexEntry(PokedexEntryData data)
        {
            _data = data;
        }

        private static PokedexEntry[] GetMany(PokedexEntryData[] data)
        {
            return data.Select(d => new PokedexEntry(d)).ToArray();
        }

        public static PokedexEntry[] GetRegional()
        {
            var entries = GetMany(PokedexEntryData.GetAll());

            // all seen/caught
            var seen = Controller.ActivePlayer.PokedexSeen.Concat(Controller.ActivePlayer.PokedexCaught);

            // determine id range
            var nationalEntries = entries.Where(e => seen.Contains(e.Id));
            var firstNewId = nationalEntries.Min(e => e.NewId);
            var lastNewId = nationalEntries.Max(e => e.NewId);

            // get all regional entries within that range (including not seen)
            var regionlEntries = entries.Where(e =>
            {
                return e.NewId >= firstNewId && e.NewId <= lastNewId;
            });

            // return regional entries ordered by regional id
            return regionlEntries.OrderBy(e => e.NewId).ToArray();
        }

        public static PokedexEntry[] GetRegionalAtoZ()
        {
            var entries = GetMany(PokedexEntryData.GetAll());

            // all seen/caught
            var seen = Controller.ActivePlayer.PokedexSeen.Concat(Controller.ActivePlayer.PokedexCaught);

            // get all seen/caught entries
            var nationalEntries = entries.Where(e => seen.Contains(e.Id));

            // return entries ordered by name
            return nationalEntries.OrderBy(e => e.Name).ToArray();
        }

        public static PokedexEntry[] GetNational()
        {
            var entries = GetMany(PokedexEntryData.GetAll());

            // all seen/caught
            var seen = Controller.ActivePlayer.PokedexSeen.Concat(Controller.ActivePlayer.PokedexCaught);

            // determine id range (001 - last seen/caught)
            var nationalEntries = entries.Where(e => seen.Contains(e.Id));
            var lastId = nationalEntries.Max(e => e.Id);

            // return all entries below (including) the last id
            return entries.Where(e => e.Id <= lastId).ToArray();
        }

        public static PokedexEntry[] GetTypeFiltered(PokemonType type1, PokemonType type2, PokedexListMode listMode)
        {
            var entries = GetMany(PokedexEntryData.GetAll());

            // get all caught
            var caught = Controller.ActivePlayer.PokedexCaught;
            var caughtEntries = entries.Where(e => caught.Contains(e.Id));

            // filter by input type(s)
            var filtered = caughtEntries.Where(e =>
            {
                var data = PokemonData.Get(e.Id);
                if (type2 == PokemonType.None) // only filter by first input type
                {
                    return type1 == DataHelper.ParseEnum<PokemonType>(data.type1) ||
                        type1 == DataHelper.ParseEnum<PokemonType>(data.type2);
                }
                else // filter by both input types
                {
                    var ptype1 = DataHelper.ParseEnum<PokemonType>(data.type1);
                    var ptype2 = DataHelper.ParseEnum<PokemonType>(data.type2);
                    return (type1 == ptype1 ||
                            type1 == ptype2) &&
                           (type2 == ptype1 ||
                            type2 == ptype2);
                }
            });

            switch (listMode)
            {
                case PokedexListMode.National:
                    // order by national id
                    return filtered.OrderBy(e => e.Id).ToArray();
                case PokedexListMode.Regional:
                    // order by regional id
                    return filtered.OrderBy(e => e.NewId).ToArray();
                case PokedexListMode.AtoZ:
                    // order by name a-z
                    return filtered.OrderBy(e => e.Name).ToArray();
            }

            throw new InvalidOperationException("Invalid list mode provided to filter.");
        }

        public int Id => _data.id;
        public int NewId => _data.newid;
        public string Name => _data.name;
        public string Species => _data.species;
        public double Height => _data.height;
        public double Weigth => _data.weigth;
        public string Text => _data.text;
        public string[] Areas => _data.areas;

        // if the pokemon has been seen or caught by the player
        public bool IsKnown
            => Controller.ActivePlayer.PokedexSeen.Contains(Id) ||
            Controller.ActivePlayer.PokedexCaught.Contains(Id);

        public bool IsCaught
            => Controller.ActivePlayer.PokedexCaught.Contains(Id);
    }
}
