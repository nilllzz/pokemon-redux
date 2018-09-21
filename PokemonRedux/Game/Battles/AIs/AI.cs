namespace PokemonRedux.Game.Battles.AIs
{
    abstract class AI
    {
        public abstract BattleAction TakeAction();

        protected BattleAction? GetMultiTurnMoveAction(BattlePokemon actor)
        {
            // returns a battle action if the user is in a multi turn move like fly

            // FLY
            if (actor.IsFlying)
            {
                return new BattleAction
                {
                    ActionType = BattleActionType.Move,
                    MoveName = "FLY",
                };
            }

            return null;
        }
    }
}
