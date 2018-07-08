namespace PokemonRedux.Game.Data
{
    public struct FontDescription
    {
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public int charWidth;
        public int charHeight;
        public CharDescription[] chars;
#pragma warning restore 0649
    }
}
