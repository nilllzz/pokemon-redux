using PokemonRedux.Game.Pokemons;
using System.Collections.Generic;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Battles
{
    static class BadgeHelper
    {
        // kanto
        public const int BOULDER_BADGE = 1;
        public const int CASCADE_BADGE = 2;
        public const int THUNDER_BADGE = 3;
        public const int RAINBOW_BADGE = 4;
        public const int SOUL_BADGE = 5;
        public const int MARSH_BADGE = 6;
        public const int VOLCANO_BADGE = 7;
        public const int EARTH_BADGE = 8;

        // johto
        public const int ZEPHYR_BADGE = 9;
        public const int HIVE_BADGE = 10;
        public const int PLAIN_BADGE = 11;
        public const int FOG_BADGE = 12;
        public const int STORM_BADGE = 13;
        public const int MINERAL_BADGE = 14;
        public const int GLACIER_BADGE = 15;
        public const int RISING_BADGE = 16;

        private static readonly IReadOnlyDictionary<PokemonType, int> TYPE_BUFF_BADGES = new Dictionary<PokemonType, int>
        {
            { PokemonType.Rock, BOULDER_BADGE },
            { PokemonType.Water, CASCADE_BADGE },
            { PokemonType.Electric, THUNDER_BADGE },
            { PokemonType.Grass, RAINBOW_BADGE },
            { PokemonType.Poison, SOUL_BADGE },
            { PokemonType.Psychic, MARSH_BADGE },
            { PokemonType.Fire, VOLCANO_BADGE },
            { PokemonType.Ground, EARTH_BADGE },
            { PokemonType.Flying, ZEPHYR_BADGE },
            { PokemonType.Bug, HIVE_BADGE },
            { PokemonType.Normal, PLAIN_BADGE },
            { PokemonType.Ghost, FOG_BADGE },
            { PokemonType.Fighting, STORM_BADGE },
            { PokemonType.Steel, MINERAL_BADGE },
            { PokemonType.Ice, GLACIER_BADGE },
            { PokemonType.Dragon, RISING_BADGE },
        };

        // the highest level at which pokemon obey the player
        public static int GetObedienceLevel()
        {
            if (Controller.ActivePlayer.Badges.Contains(RISING_BADGE))
            {
                return 100;
            }
            else if (Controller.ActivePlayer.Badges.Contains(STORM_BADGE))
            {
                return 70;
            }
            if (Controller.ActivePlayer.Badges.Contains(FOG_BADGE))
            {
                return 50;
            }
            if (Controller.ActivePlayer.Badges.Contains(HIVE_BADGE))
            {
                return 30;
            }
            // no significant badges
            return 10;
        }

        public static double GetBadgeTypeMultilplier(PokemonType type)
        {
            if (TYPE_BUFF_BADGES.ContainsKey(type))
            {
                var badge = TYPE_BUFF_BADGES[type];
                if (Controller.ActivePlayer.Badges.Contains(badge))
                {
                    return 1.125;
                }
            }
            return 1;
        }
    }
}
