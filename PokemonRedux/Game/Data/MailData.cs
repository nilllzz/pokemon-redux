namespace PokemonRedux.Game.Data
{
    // stores data for mail stored on the player's PC
    class MailData
    {
        public string author;
        public string message;
        public string template; // type, corresponds to item name
        public int pokemonId; // stores the pokemon's id the mail was given to for PORTRAITMAIL

    }
}
