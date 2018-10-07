using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("SCRATCH")]
    class Scratch : BattleMove
    {
        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new ScratchAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
