using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("QUICK ATTACK")]
    class QuickAttack : BattleMove
    {
        public override int Priority => 1;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new QuickAttackAnimation(target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
