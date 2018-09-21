using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("TAKE DOWN")]
    class TakeDown : BattleMove
    {
        public override double Accuracy => 0.85;
        public override double EffectChance => 1;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new TakeDownAnimation(target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            // 25% recoil damage
            MoveHelper.DealRecoilDamage(user, target, 0.25);
            return true;
        }
    }
}
