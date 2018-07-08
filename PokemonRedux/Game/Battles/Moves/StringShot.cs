namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("STRING SHOT")]
    class StringShot : BattleMove
    {
        public override double Accuracy => 0.95;
        public override MoveCategory GetCategory(BattlePokemon user) => MoveCategory.Status;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            // TODO: animation
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            return Battle.ActiveBattle.ChangeStat(target, PokemonStat.Speed, PokemonStatChange.Decrease);
        }
    }
}
