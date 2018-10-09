using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("SLAM")]
    class Slam : BattleMove
    {
        public override double Accuracy => 0.75;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new SlamAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
