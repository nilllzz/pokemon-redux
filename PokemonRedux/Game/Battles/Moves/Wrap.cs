using PokemonRedux.Screens.Battles.Animations.Moves;
using System;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("WRAP")]
    class Wrap : BattleMove
    {
        public override double EffectChance => 1;
        public override double Accuracy => 0.85;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new WrapAnimation(target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            if (target.Pokemon.HP > 0 && target.WrapTurns == 0)
            {
                target.WrapTurns = MoveHelper.GetMultiHitAmount() + 1; // +1 because the last round is getting rid of the effect
                Battle.ActiveBattle.UI.ShowMessageAndWait($"{target.GetDisplayName()}\nwas WRAPPED by\n{user.GetDisplayName()}!");
            }
            return true;
        }

        public static void ExecuteEndOfTurn(BattlePokemon target)
        {
            if (target.WrapTurns > 0)
            {
                target.WrapTurns--;
                if (target.WrapTurns == 0)
                {
                    Battle.ActiveBattle.UI.ShowMessageAndWait($"{target.GetDisplayName()}\nwas released from\nWRAP!");
                }
                else
                {
                    var damage = (int)Math.Ceiling(target.Pokemon.MaxHP / 16d);
                    Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(new WrapAnimation(target));
                    Battle.ActiveBattle.DealDamage(damage, target, false, true);
                    Battle.ActiveBattle.UI.ShowMessageAndWait($"{target.GetDisplayName()}^'s\nhurt by\nWRAP!");
                }
            }
        }
    }
}
