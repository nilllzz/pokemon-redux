using PokemonRedux.Game.Pokemons;

namespace PokemonRedux.Game.Battles
{
    static class StatusMoveChecks
    {
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
