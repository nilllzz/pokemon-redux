using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("SLEEP POWDER")]
    class SleepPowder : BattleMove
    {
        public override double Accuracy => 0.75;
        public override MoveCategory GetCategory(BattlePokemon user) => MoveCategory.Status;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new SleepPowderAnimation(target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            return Battle.ActiveBattle.TryInflictStatusEffect(target, PokemonStatus.SLP);
        }

        public override bool StatusMoveCheck(BattlePokemon user, BattlePokemon target)
        {
            return MoveHelper.CheckPokemonStatus(target, PokemonStatus.SLP);
        }
    }
}
