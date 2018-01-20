using Microsoft.Xna.Framework;
using PTCG.Screens.Game;

namespace PTCG.Screens
{
    internal class ScreenManager : IGameComponent
    {
        internal Screen ActiveScreen { get; private set; }

        public void Initialize()
        {
            SetScreen(new GameScreen());
        }

        public void SetScreen(Screen screen)
        {
            ActiveScreen?.UnloadContent();
            ActiveScreen = screen;
            ActiveScreen.LoadContent();
        }
    }
}
