using PokemonRedux.Game;

namespace PokemonRedux.Screens.Naming
{
    class PlayerNamingScreen : NamingScreen
    {
        protected override string[] UpperChars => new[]
        {
            "A","B","C","D","E","F","G","H","I",
            "J","K","L","M","N","O","P","Q","R",
            "S","T","U","V","W","X","Y","Z"," ",
            "-","?","!","/",".",","," "," "," ",
        };
        protected override string[] LowerChars => new[]
        {
            "a","b","c","d","e","f","g","h","i",
            "j","k","l","m","n","o","p","q","r",
            "s","t","u","v","w","x","y","z"," ",
            "*","(",")",":",";","[","]","^PK","^MN",
        };

        public PlayerNamingScreen(Screen preScreen)
            : base(preScreen, "YOUR NAME?", Player.MAX_NAME_LENGTH, "")
        { }
    }
}
