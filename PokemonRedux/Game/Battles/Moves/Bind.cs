using PokemonRedux.Screens.Battles.Animations.Moves;
using System;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("BIND")]
    class Bind : BattleMove
    {
        public override double EffectChance => 1;
        public override double Accuracy => 0.75;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new BindAnimation(target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            if (target.Pokemon.HP > 0 && target.BindTurns == 0)
            {
                target.BindTurns = MoveHelper.GetMultiHitAmount() + 1; // +1 because the last round is getting rid of the effect
                Battle.ActiveBattle.UI.ShowMessageAndWait($"{user.GetDisplayName()}\nused BIND on\n{target.GetDisplayName()}!");
            }
            return true;
        }

        public static void ExecuteEndOfTurn(BattlePokemon target)
        {
            if (target.BindTurns > 0)
            {
                target.BindTurns--;
                if (target.BindTurns == 0)
                {
                    Battle.ActiveBattle.UI.ShowMessageAndWait($"{target.GetDisplayName()}\nwas released from\nBIND!");
                }
                else
                {
                    var damage = (int)Math.Ceiling(target.Pokemon.MaxHP / 16d);
                    Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(new BindAnimation(target));
                    Battle.ActiveBattle.DealDamage(damage, target, false, true);
                    Battle.ActiveBattle.UI.ShowMessageAndWait($"{target.GetDisplayName()}^'s\nhurt by\nBIND!");
                }
            }
        }
    }
}
