using Microsoft.Xna.Framework;
using PokemonRedux.Screens.Overworld;

namespace PokemonRedux.Screens
{
    internal class ScreenManager : IGameComponent
    {
        internal Screen ActiveScreen { get; private set; }

        public void Initialize()
        {
            var worldScreen = new WorldScreen();
            SetScreen(worldScreen);
            //SetScreen(new CardRevealScreen(BoosterProvider.GetBooster(Expansion.BaseSet, "Standard Booster")));
        }

        public void SetScreen(Screen screen)
        {
            ActiveScreen?.UnloadContent();
            ActiveScreen = screen;
        }
    }
}
