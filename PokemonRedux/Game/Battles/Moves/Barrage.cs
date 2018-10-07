using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("BARRAGE")]
    class Barrage : BattleMove
    {
        public override double Accuracy => 0.85;

        public override int GetHitAmount() => MoveHelper.GetMultiHitAmount();

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new BarrageAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }
    }
}
