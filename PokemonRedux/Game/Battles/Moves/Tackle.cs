using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("TACKLE")]
    class Tackle : BattleMove
    {
        public override double Accuracy => 0.95;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new TackleAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
