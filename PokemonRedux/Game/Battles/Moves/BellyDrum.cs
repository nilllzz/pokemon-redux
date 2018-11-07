using PokemonRedux.Screens.Battles.Animations.Moves;
using System;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("BELLY DRUM")]
    class BellyDrum : BattleMove
    {
        public override MoveCategory GetCategory(BattlePokemon user) => MoveCategory.Status;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new BellyDrumAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            var hp = (int)Math.Floor(user.Pokemon.MaxHP / 2f);
            user.Pokemon.HP -= hp;
            if (user.Side == PokemonSide.Player)
            {
                Battle.ActiveBattle.UI.AnimatePlayerHPAndWait();
            }
            else
            {
                Battle.ActiveBattle.UI.AnimateEnemyHPAndWait();
            }
            user.StatModifications[PokemonStat.Attack] = 6;
            Battle.ActiveBattle.UI.ShowMessageAndWait($"{user.GetDisplayName()}\ncut its HP and\nmaximized ATTACK!");
            return true;
        }

        public override bool StatusMoveCheck(BattlePokemon user, BattlePokemon target)
        {
            // can't use if HP is lower than half or Attack is already +6
            if (!MoveHelper.CheckStatChange(user, PokemonStat.Attack, PokemonStatChange.Increase))
            {
                return false;
            }
            else
            {
                var hp = (int)Math.Floor(user.Pokemon.MaxHP / 2f);
                if (user.Pokemon.HP <= hp)
                {
                    Battle.ActiveBattle.UI.ShowMessageAndWait("But it failed!");
                    return false;
                }
            }
            return true;
        }
    }
}
