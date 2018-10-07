using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("HEADBUTT")]
    class Headbutt : BattleMove
    {
        public override double EffectChance => 0.3;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new HeadbuttAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            target.Flinched = true;
            return true;
        }
    }
}
