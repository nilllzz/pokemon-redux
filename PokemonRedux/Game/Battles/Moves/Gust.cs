namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("GUST")]
    class Gust : BattleMove
    {
        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            // TODO: animation
        }

        public override int GetBasePower(BattlePokemon user, BattlePokemon target)
        {
            // double damage if target is hit during Fly.
            // battle.cs does the miss ignore
            if (target.IsFlying)
            {
                return base.GetBasePower(user, target) * 2;
            }
            return base.GetBasePower(user, target);
        }
    }
}
