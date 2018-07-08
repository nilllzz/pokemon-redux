namespace PokemonRedux.Game
{
    class MenuStates
    {
        // pack
        public int PackPocketIndex { get; set; }
        public int[] PackItemIndices { get; set; }
        public int[] PackScrollIndices { get; set; }
        // pokedex
        public int PokedexLastSelectedId { get; set; }
        // pokemon
        public int PokemonPartyIndex { get; set; }

        public void Reset()
        {
            PackPocketIndex = 0;
            PackItemIndices = new int[] { 0, 0, 0, 0 };
            PackScrollIndices = new int[] { 0, 0, 0, 0 };
            PokedexLastSelectedId = 0;
            PokemonPartyIndex = 0;
        }
    }
}
