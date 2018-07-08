namespace PokemonRedux.Game.Data
{
    // data an item contains that the player has in their inventory
    class PlayerItemData
    {
        public string name;
        public int amount;

        public override string ToString()
        {
            return $"{{{name}x{amount}}}";
        }
    }
}
