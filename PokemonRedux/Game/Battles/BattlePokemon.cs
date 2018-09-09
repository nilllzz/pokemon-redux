using PokemonRedux.Game.Data;
using PokemonRedux.Game.Pokemons;
using System;
using System.Collections.Generic;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Battles
{
    // represents a pokemon in battle, with its volitle status, last used attack, etc.
    class BattlePokemon
    {
        private const int MIN_SLEEP_TURNS = 2;
        private const int MAX_SLEEP_TURNS = 7;

        public static PokemonSide ReverseSide(PokemonSide side)
            => side == PokemonSide.Enemy ? PokemonSide.Player : PokemonSide.Enemy;

        public Pokemon Pokemon { get; }
        public PokemonSide Side { get; private set; }

        public PokemonMoveData DisabledMove { get; private set; }
        public int DisabledTurns { get; set; }
        public Dictionary<PokemonStat, int> StatModifications { get; } = new Dictionary<PokemonStat, int>()
        {
            { PokemonStat.Attack, 0 },
            { PokemonStat.Defense, 0 },
            { PokemonStat.SpecialAttack, 0 },
            { PokemonStat.SpecialDefense, 0 },
            { PokemonStat.Speed, 0 },
            { PokemonStat.Accuracy, 0 },
            { PokemonStat.Evasion, 0 },
        };
        public bool HyperbeamRecharge { get; set; }
        public int SleepTurns { get; set; } // amount of turns after the pokemon wakes up
        public bool IsInfatuated { get; set; } // infatuated by another gender-opposite pokemon
        public PokemonMoveData LastUsedMove { get; set; }
        public int FocusEnergyTurns { get; set; }
        public bool Foresighted { get; set; } // under the effect of Foresight
        public bool Protected { get; set; } // protect/detect is active
        public bool LockedOn { get; set; } // lock on or mindreader has been used on this pokemon
        public bool IsFlying { get; set; } // first turn of Fly
        public bool IsUnderground { get; set; } // first turn of Dig
        public int SubstituteHP { get; set; } // substitute's HP, 0 means no sub active
        public int RageCounter { get; set; } = 1;
        public int ToxicCounter { get; set; } = 0;
        public bool Flinched { get; set; }
        public int ConfusionTurns { get; set; } = 0;

        // carry over to switched pokemon
        public int ReflectTurns { get; set; }
        public int LightScreenTurns { get; set; }

        public BattlePokemon(Pokemon pokemon, PokemonSide side)
        {
            Pokemon = pokemon;
            Side = side;

            // initialize sleep turns
            if (Pokemon.Status == PokemonStatus.SLP)
            {
                SetAsleep();
            }
        }

        public double GetStat(PokemonStat stat)
        {
            var modification = StatModifications[stat];
            if (stat == PokemonStat.Accuracy || stat == PokemonStat.Evasion)
            {
                // reverse evasion values
                if (stat == PokemonStat.Evasion)
                {
                    modification *= -1;
                }
                return Math.Max(3D, 3 + modification) / Math.Max(3D, 3 - modification);
            }
            var multiplier = Math.Max(2D, 2 + modification) / Math.Max(2D, 2 - modification);
            switch (stat)
            {
                case PokemonStat.Attack:
                    return Pokemon.Attack * multiplier;
                case PokemonStat.Defense:
                    return Pokemon.Defense * multiplier;
                case PokemonStat.SpecialAttack:
                    return Pokemon.SpecialAttack * multiplier;
                case PokemonStat.SpecialDefense:
                    return Pokemon.SpecialDefense * multiplier;
                case PokemonStat.Speed:
                    return Pokemon.Speed * multiplier;
            }
            return 0;
        }

        public string GetDisplayName()
        {
            if (Side == PokemonSide.Enemy)
            {
                return "Enemy " + Pokemon.DisplayName;
            }
            else
            {
                return Pokemon.DisplayName;
            }
        }

        public void SetAsleep()
        {
            SleepTurns = Battle.ActiveBattle.Random.Next(MIN_SLEEP_TURNS, MAX_SLEEP_TURNS + 1);
            Pokemon.Status = PokemonStatus.SLP;
        }

        public void ReducePP(string moveName)
        {
            var move = Pokemon.Moves.FirstOrDefault(m => m.name == moveName);
            if (move != null && move.pp > 0)
            {
                move.pp--;
            }
        }

        public bool GetCanFlee()
        {
            // unable to flee when frozen or asleep
            if (Pokemon.Status == PokemonStatus.FRZ || Pokemon.Status == PokemonStatus.SLP)
            {
                return false;
            }

            return true;
        }

        public void TransferStateTo(BattlePokemon target, bool batonPass)
        {
            target.ReflectTurns = ReflectTurns;
            target.LightScreenTurns = LightScreenTurns;
            if (batonPass)
            {
                // TODO: baton pass
            }
        }
    }
}
