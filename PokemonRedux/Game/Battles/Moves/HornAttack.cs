using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("HORN ATTACK")]
    class HornAttack : BattleMove
    {
        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new HornAttackAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
