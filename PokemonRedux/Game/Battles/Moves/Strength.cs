using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("STRENGTH")]
    class Strength : BattleMove
    {
        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new StrengthAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
