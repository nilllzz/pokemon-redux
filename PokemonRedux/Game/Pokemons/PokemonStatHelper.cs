using Microsoft.Xna.Framework;
using PokemonRedux.Game.Battles;
using PokemonRedux.Game.Data;
using static System.Math;

namespace PokemonRedux.Game.Pokemons
{
    static class PokemonStatHelper
    {
        // hp bar colors
        private static readonly Color HP_GREEN = new Color(0, 184, 0);
        private static readonly Color HP_YELLOW = new Color(248, 184, 0);
        private static readonly Color HP_RED = new Color(248, 0, 0);

        public static int GetExperienceForLevel(ExperienceType experienceType, int level)
        {
            switch (experienceType)
            {
                case ExperienceType.Fast:
                    return (int)((4 * Pow(level, 3)) / 5);
                case ExperienceType.MediumFast:
                    return (int)Pow(level, 3);
                case ExperienceType.MediumSlow:
                    return (int)(1.2 * Pow(level, 3) - 15 * Pow(level, 2) + 100 * level - 140);
                case ExperienceType.Slow:
                    return (int)((5 * Pow(level, 3)) / 4);
            }
            return 0;
        }

        public static int GetLevelForExperience(ExperienceType experienceType, int experience)
        {
            for (var i = 1; i <= Pokemon.MAX_LEVEL; i++)
            {
                var exp = GetExperienceForLevel(experienceType, i + 1);
                if (exp > experience)
                {
                    return i;
                }
            }
            return Pokemon.MAX_LEVEL;
        }

        public static int CalcHPStat(int level, int baseStat, byte dv, ushort ev)
        {
            return CalcStat(level, baseStat, dv, ev) + level + 5;
        }

        public static int CalcStat(int level, int baseStat, byte dv, ushort ev)
        {
            return (int)(Floor((((baseStat + dv) * 2 + Floor((Ceiling(Sqrt(ev))) / 4)) * level) / 100) + 5);
        }

        public static PokemonHealth GetPokemonHealth(int hp, int maxHp)
        {
            var remaining = (double)hp / maxHp;
            if (remaining < 0.2)
            {
                return PokemonHealth.Fainted;
            }
            else if (remaining < 0.5)
            {
                return PokemonHealth.Hurt;
            }
            return PokemonHealth.Healthy;
        }

        public static string GetGenderChar(PokemonGender gender)
        {
            switch (gender)
            {
                case PokemonGender.Male:
                    return "♂";
                case PokemonGender.Female:
                    return "♀";
                case PokemonGender.Genderless:
                    return " ";
            }
            return "";
        }

        public static Color GetHPBarColor(PokemonHealth health)
        {
            switch (health)
            {
                case PokemonHealth.Healthy:
                    return HP_GREEN;
                case PokemonHealth.Hurt:
                    return HP_YELLOW;
                case PokemonHealth.Fainted:
                    return HP_RED;
            }
            return Color.White;
        }

        public static string GetDisplayString(PokemonStat stat)
        {
            switch (stat)
            {
                case PokemonStat.Attack:
                    return "ATTACK";
                case PokemonStat.Defense:
                    return "DEFENSE";
                case PokemonStat.SpecialAttack:
                    return "SPCL.ATK";
                case PokemonStat.SpecialDefense:
                    return "SPCL.DEF";
                case PokemonStat.Speed:
                    return "SPEED";
                case PokemonStat.Accuracy:
                    return "ACCURACY";
                case PokemonStat.Evasion:
                    return "EVASION";
            }
            return "INVALID";
        }
    }
}
