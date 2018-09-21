using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("HARDEN")]
    class Harden : BattleMove
    {
        public override MoveCategory GetCategory(BattlePokemon user) => MoveCategory.Status;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new HardenAnimation(target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            return Battle.ActiveBattle.ChangeStat(user, PokemonStat.Defense, PokemonStatChange.Increase);
        }

        public override bool StatusMoveCheck(BattlePokemon user, BattlePokemon target)
        {
            return MoveHelper.CheckStatChange(user, PokemonStat.Defense, PokemonStatChange.Decrease);
        }
    }
}
