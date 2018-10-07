using PokemonRedux.Game.Pokemons;
using System;

namespace PokemonRedux.Game.Battles
{
    static class MoveHelper
    {
        // get amount of hits for moves like fury swipes
        // or amount of turns for moves like bind
        public static int GetMultiHitAmount()
        {
            // 37.5% => 2x
            // 37.5% => 3x
            // 12.5% => 4x
            // 12.5% => 5x
            var r = Battle.ActiveBattle.Random.Next(0, 1000);
            if (r < 375)
            {
                return 2;
            }
            else if (r < 750)
            {
                return 3;
            }
            else if (r < 875)
            {
                return 4;
            }
            return 5;
        }

        // calculates restored HP for moves like Leech Life
        public static int GetRestoredHP(BattlePokemon target, int damage)
        {
            var hp = 1;
            if (damage > 1)
            {
                // 50%, ceiling division (5 damage results in 3 hp)
                hp = (int)Math.Ceiling(damage / 2f);
            }

            if (hp + target.Pokemon.HP > target.Pokemon.MaxHP)
            {
                hp = target.Pokemon.MaxHP - target.Pokemon.HP;
            }

            return hp;
        }

        public static void DealRecoilDamage(BattlePokemon user, BattlePokemon target, double amount)
        {
            var damage = (int)Math.Ceiling(target.LastDamageReceived * amount);
            Battle.ActiveBattle.DealDamage(damage, user, substituteAffected: false, hideAnimation: true);
            Battle.ActiveBattle.UI.ShowMessageAndKeepOpen($"{user.GetDisplayName()}^'s\nhit with recoil!", 20);
        }

        public static bool CheckConfused(BattlePokemon target)
        {
            if (target.ConfusionTurns > 0)
            {
                Battle.ActiveBattle.UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\nalready confused!");
                return false;
            }
            return true;
        }

        public static bool CheckPokemonStatus(BattlePokemon target, PokemonStatus status)
        {
            if (target.Pokemon.Status != PokemonStatus.OK)
            {
                if (target.Pokemon.Status != status)
                {
                    // is afflicted by status, but not the one trying to inclict
                    Battle.ActiveBattle.UI.ShowMessageAndWait("It didn^'t affect\n" + target.GetDisplayName() + "!");
                }
                else
                {
                    switch (status)
                    {
                        case PokemonStatus.PAR:
                            Battle.ActiveBattle.UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\nalready paralyzed!");
                            break;
                        case PokemonStatus.SLP:
                            Battle.ActiveBattle.UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\nalready asleep!");
                            break;
                        case PokemonStatus.BRN:
                            Battle.ActiveBattle.UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\nalready burnt!"); // TODO: message
                            break;
                        case PokemonStatus.FRZ:
                            Battle.ActiveBattle.UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\nalready frozen!"); // TODO: message
                            break;
                        case PokemonStatus.PSN:
                        case PokemonStatus.TOX:
                            Battle.ActiveBattle.UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\nalready poisoned!"); // TODO: message
                            break;
                    }
                }
                return false;
            }
            return true;
        }

        public static bool CheckStatChange(BattlePokemon target, PokemonStat stat, PokemonStatChange change)
        {
            var currentStat = target.StatModifications[stat];
            var statName = PokemonStatHelper.GetDisplayString(stat);
            if (currentStat == 6 && (change == PokemonStatChange.Increase || change == PokemonStatChange.SharpIncrease))
            {
                Battle.ActiveBattle.UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\n" + statName + " won^'t\nrise anymore!");
                return false;
            }
            else if (currentStat == -6 && (change == PokemonStatChange.Decrease || change == PokemonStatChange.SharpDecrease))
            {
                Battle.ActiveBattle.UI.ShowMessageAndWait(target.GetDisplayName() + "^'s\n" + statName + " won^'t\ndrop anymore!");
                return false;
            }
            return true;
        }
    }
}
