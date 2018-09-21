using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("LEECH LIFE")]
    class LeechLife : BattleMove
    {
        public override double EffectChance => 1;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new LeechLifeAnimation(target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            if (user.Pokemon.HP < user.Pokemon.MaxHP)
            {
                var restoredHP = MoveHelper.GetRestoredHP(target, target.LastDamageReceived);
                user.Pokemon.HP += restoredHP;
                if (user.Side == PokemonSide.Player)
                {
                    Battle.ActiveBattle.UI.AnimatePlayerHPAndWait();
                }
                else
                {
                    Battle.ActiveBattle.UI.AnimateEnemyHPAndWait();
                }
            }
            Battle.ActiveBattle.UI.ShowMessageAndWait("Sucked health from\n" + target.GetDisplayName() + "!");
            return true;
        }
    }
}
