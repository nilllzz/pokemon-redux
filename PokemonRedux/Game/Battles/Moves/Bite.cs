using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("BITE")]
    class Bite : BattleMove
    {
        public override double EffectChance => 0.3;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new BiteAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            target.Flinched = true;
            return true;
        }
    }
}
