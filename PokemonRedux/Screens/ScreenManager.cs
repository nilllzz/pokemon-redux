using Microsoft.Xna.Framework;
using PokemonRedux.Screens.Overworld;

namespace PokemonRedux.Screens
{
    internal class ScreenManager : IGameComponent
    {
        internal Screen ActiveScreen { get; private set; }

        public void Initialize()
        {
            var titleScreen = new Title.TitleScreen();
            SetScreen(titleScreen);
        }

        public void SetScreen(Screen screen)
        {
            ActiveScreen?.UnloadContent();
            ActiveScreen = screen;
        }
    }
}
