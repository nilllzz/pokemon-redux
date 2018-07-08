namespace PokemonRedux.Game.Data
{
    // single char description for a font
    public struct CharDescription
    {
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public string chars;
        public bool isStr;
        public int x;
        public int y;
#pragma warning restore 0649
    }
}
