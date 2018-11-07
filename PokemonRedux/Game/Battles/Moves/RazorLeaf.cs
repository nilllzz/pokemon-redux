using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("RAZOR LEAF")]
    class RazorLeaf : BattleMove
    {
        public override bool IncreasedCriticalHitRatio => true;
        public override double Accuracy => 0.95;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new RazorLeafAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
