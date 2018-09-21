using Microsoft.Xna.Framework;
using PokemonRedux.Screens.Intro;

namespace PokemonRedux.Screens
{
    internal class ScreenManager : IGameComponent
    {
        internal Screen ActiveScreen { get; private set; }

        public void Initialize()
        {
            var introScreen = new GameFreakScreen();
            SetScreen(introScreen);
        }

        public void SetScreen(Screen screen)
        {
            ActiveScreen?.UnloadContent();
            ActiveScreen = screen;
        }
    }
}
