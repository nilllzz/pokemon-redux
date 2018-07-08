using Microsoft.Xna.Framework;

namespace PokemonRedux.Screens.TownMap
{
    struct MapEntry
    {
        public Point Position;
        public string Name;

        public MapEntry(string name, int x, int y)
        {
            Position = new Point(x, y);
            Name = name;
        }


        #region Map data

        public static MapEntry[] GetJohtoMapData()
        {
            return new[]
            {
                new MapEntry("NEW BARK\nTOWN", 124, 84),
                new MapEntry("ROUTE 29", 112, 84),
                new MapEntry("CHERRYGROVE\nCITY", 84, 84),
                new MapEntry("ROUTE 30", 84, 64),
                new MapEntry("ROUTE 31", 80, 44),
                new MapEntry("VIOLET CITY", 68, 44),
                new MapEntry("SPROUT\nTOWER", 69, 42),
                new MapEntry("ROUTE 32", 68, 76),
                new MapEntry("RUINS\nOF ALPH", 60, 60),
                new MapEntry("UNION CAVE", 68, 108),
                new MapEntry("ROUTE 33", 66, 108),
                new MapEntry("AZALEA TOWN", 52, 108),
                new MapEntry("SLOWPOKE\nWELL", 54, 106),
                new MapEntry("ILEX\nFOREST", 36, 104),
                new MapEntry("ROUTE 34", 36, 96),
                new MapEntry("GOLDENROD\nCITY", 36, 76),
                new MapEntry("RADIO TOWER", 34, 76),
                new MapEntry("ROUTE 35", 36, 60),
                new MapEntry("NATIONAL\nPARK", 36, 44),
                new MapEntry("ROUTE 36", 48, 44),
                new MapEntry("ROUTE 37", 52, 36),
                new MapEntry("ECRUTEAK\nCITY", 52, 28),
                new MapEntry("TIN TOWER", 54, 26),
                new MapEntry("BURNED\nTOWER", 50, 26),
                new MapEntry("ROUTE 38", 36, 28),
                new MapEntry("ROUTE 39", 20, 32),
                new MapEntry("OLIVINE\nCITY", 20, 44),
                new MapEntry("LIGHTHOUSE", 22, 46),
                new MapEntry("ROUTE 40", 12, 48),
                new MapEntry("WHIRL\nISLANDS", 12, 76),
                new MapEntry("ROUTE 41", 12, 84),
                new MapEntry("CIANWOOD\nCITY", 4, 84),
                new MapEntry("ROUTE 42", 76, 28),
                new MapEntry("MT.MORTAR", 68, 28),
                new MapEntry("MAHOGANY\nTOWN", 92, 28),
                new MapEntry("ROUTE 43", 92, 20),
                new MapEntry("LAKE OF\nRAGE", 92, 12),
                new MapEntry("ROUTE 44", 104, 28),
                new MapEntry("ICE PATH", 114, 22),
                new MapEntry("BLACKTHORN\nCITY", 116, 28),
                new MapEntry("DRAGON'S\nDEN", 116, 20),
                new MapEntry("ROUTE 45", 116, 48),
                new MapEntry("DARK CAVE", 96, 56),
                new MapEntry("ROUTE 46", 108, 72),
                new MapEntry("SILVER CAVE", 132, 52),
            };
        }

        public static MapEntry[] GetKantoMapData()
        {
            return new[]
            {
                new MapEntry("PALLET TOWN", 36, 92),
                new MapEntry("ROUTE 1", 36, 76),
                new MapEntry("VIRIDIAN\nCITY", 36, 60),
                new MapEntry("ROUTE 2", 36, 48),
                new MapEntry("PEWTER CITY", 36, 36),
                new MapEntry("ROUTE 3", 48, 36),
                new MapEntry("MT.MOON", 60, 36),
                new MapEntry("ROUTE 4", 72, 36),
                new MapEntry("CERULEAN\nCITY", 84, 36),
                new MapEntry("ROUTE 24", 84, 28),
                new MapEntry("ROUTE 25", 92, 20),
                new MapEntry("ROUTE 5", 84, 44),
                new MapEntry("UNDERGROUND", 92, 60),
                new MapEntry("ROUTE 6", 84, 60),
                new MapEntry("VERMILION\nCITY", 84, 68),
                new MapEntry("DIGLETT^'s\nCAVE", 72, 44),
                new MapEntry("ROUTE 7", 72, 52),
                new MapEntry("ROUTE 8", 100, 52),
                new MapEntry("ROUTE 9", 100, 36),
                new MapEntry("ROCK TUNNEL", 116, 36),
                new MapEntry("ROUTE 10", 116, 40),
                new MapEntry("POWER PLANT", 116, 44),
                new MapEntry("LAVENDER\nTOWN", 116, 52),
                new MapEntry("LAV\nRADIO TOWER", 124, 52),
                new MapEntry("CELADON\nCITY", 60, 52),
                new MapEntry("SAFFRON\nCITY", 84, 52),
                new MapEntry("ROUTE 11", 100, 68),
                new MapEntry("ROUTE 12", 116, 64),
                new MapEntry("ROUTE 13", 108, 84),
                new MapEntry("ROUTE 14", 100, 96),
                new MapEntry("ROUTE 15", 88, 100),
                new MapEntry("ROUTE 16", 52, 52),
                new MapEntry("ROUTE 17", 52, 76),
                new MapEntry("ROUTE 18", 64, 100),
                new MapEntry("FUCHSIA\nCITY", 76, 100),
                new MapEntry("ROUTE 19", 76, 112),
                new MapEntry("ROUTE 20", 64, 116),
                new MapEntry("SEAFOAM\nISLANDS", 52, 116),
                new MapEntry("CINNABAR\nISLAND", 36, 116),
                new MapEntry("ROUTE 21", 36, 104),
                new MapEntry("ROUTE 22", 20, 52),
                new MapEntry("VICTORY\nROAD", 12, 36),
                new MapEntry("ROUTE 23", 12, 28),
                new MapEntry("INDIGO\nPLATEAU", 12, 20),
                new MapEntry("ROUTE 26", 12, 76),
                new MapEntry("ROUTE 27", 4, 84),
                new MapEntry("TOHJO FALLS", -4, 84),
                new MapEntry("ROUTE 28", 4, 52),
            };
        }

        #endregion
    }
}
