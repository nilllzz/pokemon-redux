using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("WING ATTACK")]
    class WingAttack : BattleMove
    {
        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new WingAttackAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
