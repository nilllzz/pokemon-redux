using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("SMOKESCREEN")]
    class Smokescreen : BattleMove
    {
        public override MoveCategory GetCategory(BattlePokemon user) => MoveCategory.Status;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new SmokescreenAnimation(target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            return Battle.ActiveBattle.ChangeStat(target, PokemonStat.Accuracy, PokemonStatChange.Decrease);
        }

        public override bool StatusMoveCheck(BattlePokemon user, BattlePokemon target)
        {
            return MoveHelper.CheckStatChange(target, PokemonStat.Accuracy, PokemonStatChange.Decrease);
        }
    }
}
