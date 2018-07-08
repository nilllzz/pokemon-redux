using PokemonRedux.Game.Pokemons;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("SLEEP POWDER")]
    class SleepPowder : BattleMove
    {
        public override double Accuracy => 0.75;
        public override MoveCategory GetCategory(BattlePokemon user) => MoveCategory.Status;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            // TODO: animation
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            return Battle.ActiveBattle.TryInflictStatusEffect(target, PokemonStatus.SLP);
        }
    }
}
