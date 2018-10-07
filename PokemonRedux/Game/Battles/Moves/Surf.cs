using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("SURF")]
    class Surf : BattleMove
    {
        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new SurfAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
