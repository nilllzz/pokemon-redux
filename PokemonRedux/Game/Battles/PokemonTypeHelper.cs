using PokemonRedux.Game.Pokemons;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRedux.Game.Battles
{
    static class PokemonTypeHelper
    {
        private static readonly IReadOnlyCollection<PokemonType> PHYSICAL_TYPES = new[]
        {
            PokemonType.Normal,
            PokemonType.Fighting,
            PokemonType.Flying,
            PokemonType.Ground,
            PokemonType.Rock,
            PokemonType.Bug,
            PokemonType.Ghost,
            PokemonType.Poison,
            PokemonType.Steel,
        };

        private static readonly IReadOnlyDictionary<PokemonType, IReadOnlyDictionary<PokemonType, double>> TYPE_CHART =
            new Dictionary<PokemonType, IReadOnlyDictionary<PokemonType, double>>
            {
                { PokemonType.Normal, new Dictionary<PokemonType, double>{
                    { PokemonType.Rock, 0.5 },
                    { PokemonType.Ghost, 0 },
                    { PokemonType.Steel, 0.5 },
                } },
                { PokemonType.Fighting, new Dictionary<PokemonType, double>{
                    { PokemonType.Normal, 2 },
                    { PokemonType.Flying, 0.5 },
                    { PokemonType.Poison, 0.5 },
                    { PokemonType.Rock, 2 },
                    { PokemonType.Bug, 0.5 },
                    { PokemonType.Ghost, 0 },
                    { PokemonType.Steel, 2 },
                    { PokemonType.Psychic, 0.5 },
                    { PokemonType.Ice, 2 },
                    { PokemonType.Dark, 2 },
                } },
                { PokemonType.Flying, new Dictionary<PokemonType, double>{
                    { PokemonType.Fighting, 2 },
                    { PokemonType.Rock, 0.5 },
                    { PokemonType.Bug, 2 },
                    { PokemonType.Steel, 0.5 },
                    { PokemonType.Grass, 2 },
                    { PokemonType.Electric, 0.5 },
                } },
                { PokemonType.Poison, new Dictionary<PokemonType, double>{
                    { PokemonType.Poison, 0.5 },
                    { PokemonType.Ground, 0.5 },
                    { PokemonType.Rock, 0.5 },
                    { PokemonType.Ghost, 0.5 },
                    { PokemonType.Steel, 0 },
                    { PokemonType.Grass, 2 },
                } },
                { PokemonType.Ground, new Dictionary<PokemonType, double>{
                    { PokemonType.Flying, 0 },
                    { PokemonType.Poison, 2 },
                    { PokemonType.Rock, 2 },
                    { PokemonType.Bug, 0.5 },
                    { PokemonType.Steel, 2 },
                    { PokemonType.Fire, 2 },
                    { PokemonType.Grass, 0.5 },
                    { PokemonType.Electric, 2 },
                } },
                { PokemonType.Rock, new Dictionary<PokemonType, double>{
                    { PokemonType.Fighting, 0.5 },
                    { PokemonType.Flying, 2 },
                    { PokemonType.Ground, 0.5 },
                    { PokemonType.Bug, 2 },
                    { PokemonType.Steel, 0.5 },
                    { PokemonType.Fire, 2 },
                    { PokemonType.Ice, 2 },
                } },
                { PokemonType.Bug, new Dictionary<PokemonType, double>{
                    { PokemonType.Fighting, 0.5 },
                    { PokemonType.Flying, 0.5 },
                    { PokemonType.Poison, 0.5 },
                    { PokemonType.Ghost, 0.5 },
                    { PokemonType.Steel, 0.5 },
                    { PokemonType.Fire, 0.5 },
                    { PokemonType.Grass, 2 },
                    { PokemonType.Psychic, 2 },
                    { PokemonType.Dark, 2 },
                } },
                { PokemonType.Ghost, new Dictionary<PokemonType, double>{
                    { PokemonType.Normal, 0 },
                    { PokemonType.Ghost, 2 },
                    { PokemonType.Steel, 0.5 },
                    { PokemonType.Psychic, 2 },
                    { PokemonType.Dark, 0.5 },
                } },
                { PokemonType.Steel, new Dictionary<PokemonType, double>{
                    { PokemonType.Rock, 2 },
                    { PokemonType.Steel, 0.5 },
                    { PokemonType.Fire, 0.5 },
                    { PokemonType.Water, 0.5 },
                    { PokemonType.Electric, 0.5 },
                    { PokemonType.Ice, 2 },
                } },
                { PokemonType.Fire, new Dictionary<PokemonType, double>{
                    { PokemonType.Rock, 0.5 },
                    { PokemonType.Bug, 2 },
                    { PokemonType.Steel, 2 },
                    { PokemonType.Fire, 0.5 },
                    { PokemonType.Water, 0.5 },
                    { PokemonType.Grass, 2 },
                    { PokemonType.Ice, 2 },
                    { PokemonType.Dragon, 0.5 },
                } },
                { PokemonType.Water, new Dictionary<PokemonType, double>{
                    { PokemonType.Ground, 2 },
                    { PokemonType.Rock, 2 },
                    { PokemonType.Fire, 2 },
                    { PokemonType.Water, 0.5 },
                    { PokemonType.Grass, 0.5 },
                    { PokemonType.Dragon, 0.5 },
                } },
                { PokemonType.Grass, new Dictionary<PokemonType, double>{
                    { PokemonType.Flying, 0.5 },
                    { PokemonType.Poison, 0.5 },
                    { PokemonType.Ground, 2 },
                    { PokemonType.Rock, 2 },
                    { PokemonType.Bug, 0.5 },
                    { PokemonType.Steel, 0.5 },
                    { PokemonType.Fire, 0.5 },
                    { PokemonType.Water, 2 },
                    { PokemonType.Grass, 0.5 },
                    { PokemonType.Dragon, 0.5 },
                } },
                { PokemonType.Electric, new Dictionary<PokemonType, double>{
                    { PokemonType.Flying, 2 },
                    { PokemonType.Ground, 0 },
                    { PokemonType.Water, 2 },
                    { PokemonType.Grass, 0.5 },
                    { PokemonType.Electric, 0.5 },
                    { PokemonType.Dragon, 0.5 },
                } },
                { PokemonType.Psychic, new Dictionary<PokemonType, double>{
                    { PokemonType.Fighting, 2 },
                    { PokemonType.Poison, 2 },
                    { PokemonType.Steel, 0.5 },
                    { PokemonType.Psychic, 0.5 },
                    { PokemonType.Dark, 0 },
                } },
                { PokemonType.Ice, new Dictionary<PokemonType, double>{
                    { PokemonType.Flying, 2 },
                    { PokemonType.Ground, 2 },
                    { PokemonType.Steel, 0.5 },
                    { PokemonType.Fire, 0.5 },
                    { PokemonType.Water, 0.5 },
                    { PokemonType.Grass, 2 },
                    { PokemonType.Ice, 0.5 },
                    { PokemonType.Dragon, 2 },
                } },
                { PokemonType.Dragon, new Dictionary<PokemonType, double>{
                    { PokemonType.Steel, 0.5 },
                    { PokemonType.Dragon, 2 },
                } },
                { PokemonType.Dark, new Dictionary<PokemonType, double>{
                    { PokemonType.Fighting, 0.5 },
                    { PokemonType.Ghost, 2 },
                    { PokemonType.Steel, 0.5 },
                    { PokemonType.Psychic, 2 },
                    { PokemonType.Dark, 0.5 },
                } },
            };

        public static double GetMultiplier(PokemonType attack, PokemonType defense)
        {
            // none (unknown) types result in default type interaction
            if (attack == PokemonType.None || defense == PokemonType.None)
            {
                return 1D;
            }

            var table = TYPE_CHART[attack];
            // the table only contains non-default (1x) type interactions (0.5x, 2x, 0x)
            if (table.ContainsKey(defense))
            {
                return table[defense];
            }
            else
            {
                return 1D;
            }
        }

        public static double GetMultiplier(PokemonType attack, BattlePokemon target)
        {
            var multiplier = GetMultiplier(attack, target.Pokemon.Type1);
            if (target.Pokemon.Type2 != PokemonType.None)
            {
                multiplier *= GetMultiplier(attack, target.Pokemon.Type2);
            }
            return multiplier;
        }

        public static bool IsPhysical(PokemonType type)
            => PHYSICAL_TYPES.Contains(type);
    }
}
