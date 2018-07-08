namespace PokemonRedux.Screens
{
    // implemented by screens that can show a textbox
    // used by stuff like items, phone calls etc that can be shown eg in the PACK and WORLD screens
    interface ITextboxScreen
    {
        void ShowTextbox(string text, bool skip);
    }
}
