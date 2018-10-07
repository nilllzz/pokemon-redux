using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("HYDRO PUMP")]
    class HydroPump : BattleMove
    {
        public override double Accuracy => 0.8;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new HydroPumpAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
