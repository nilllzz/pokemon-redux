namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("PSYBEAM")]
    class Psybeam : BattleMove
    {
        public override double EffectChance => 0.1;

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
