using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("FLY")]
    class Fly : BattleMove
    {
        public override double Accuracy => 0.95;
        public override double EffectChance => 1;
        // always show animation
        // the animation class will show/hide the user depending on the BattleAnimations setting
        public override bool ShouldShowAnimation => true;

        public override MoveCategory GetCategory(BattlePokemon user)
        {
            // FLY is a status move when the user flies up
            return user.IsFlying ?
                MoveCategory.Physical :
                MoveCategory.Status;
        }

        public override bool HasAccuracyCheck(BattlePokemon user) => user.IsFlying;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new FlyAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            user.IsFlying = !user.IsFlying;
            if (user.IsFlying)
            {
                Battle.ActiveBattle.UI.ShowMessageAndWait(user.GetDisplayName() + "\nflew up high!");
            }
            return true;
        }

        public override void MoveFailed(BattlePokemon user, BattlePokemon target)
        {
            // when fly fails, the user needs to land
            // neither the hit animation nor the secondary effect were executed
            user.IsFlying = false;
            Battle.ActiveBattle.AnimationController.SetPokemonVisibility(user.Side, true);
        }

        public override bool ShouldConsumePP(BattlePokemon user) => user.IsFlying;
        public override bool ShouldShowUsedText(BattlePokemon user) => user.IsFlying;
    }
}
