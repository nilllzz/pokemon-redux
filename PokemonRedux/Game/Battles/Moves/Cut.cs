using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("CUT")]
    class Cut : BattleMove
    {
        public override double Accuracy => 0.95;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new CutAnimation(target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
