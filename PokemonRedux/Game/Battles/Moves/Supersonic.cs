namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("SUPERSONIC")]
    class Supersonic : BattleMove
    {
        public override double Accuracy => 0.55;
        public override MoveCategory GetCategory(BattlePokemon user) => MoveCategory.Status;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            // TODO: animation
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            return Battle.ActiveBattle.TryInflictConfusion(target);
        }
    }
}
