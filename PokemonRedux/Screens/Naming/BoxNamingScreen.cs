using PokemonRedux.Game.Pokemons;

namespace PokemonRedux.Screens.Naming
{
    class BoxNamingScreen : NamingScreen
    {
        protected override string[] UpperChars => new[]
        {
            "A","B","C","D","E","F","G","H","I",
            "J","K","L","M","N","O","P","Q","R",
            "S","T","U","V","W","X","Y","Z"," ",
            "*","(",")",":",";","[","]","^PK","^MN",
            "-","?","!","♂","♀","/",".",",","&",
        };
        protected override string[] LowerChars => new[]
        {
            "a","b","c","d","e","f","g","h","i",
            "j","k","l","m","n","o","p","q","r",
            "s","t","u","v","w","x","y","z"," ",
            "é","^'d","^'l","^'m","^'r","^'s","^'t","^'v","0",
            "1","2","3","4","5","6","7","8","9",
        };

        public BoxNamingScreen(Screen preScreen)
            : base(preScreen, "BOX NAME?", StorageBox.MAX_NAME_LENGTH, "")
        { }
    }
}
