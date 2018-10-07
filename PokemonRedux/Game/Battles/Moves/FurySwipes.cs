using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("FURY SWIPES")]
    class FurySwipes : BattleMove
    {
        // stores the current hit counter for the animation
        private int _hitCounter = 0;

        public override double Accuracy => 0.8;

        public override int GetHitAmount()
        {
            _hitCounter = 0;
            return MoveHelper.GetMultiHitAmount();
        }

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new FurySwipesAnimation(user, target, _hitCounter);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
            _hitCounter++;
        }
    }
}
