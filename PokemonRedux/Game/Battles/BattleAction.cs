namespace PokemonRedux.Game.Battles
{
    // action taken by player/wild pokemon/trainer
    struct BattleAction
    {
        public BattleActionType ActionType;
        public string MoveName; // selected a move to use
        public string ItemName; // selected an item to use
        public int SwitchToIndex; // switch to a different pokemon
    }
}
