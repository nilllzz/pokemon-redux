namespace PokemonRedux.Screens.Transition
{
    abstract class TransitionScreen : Screen
    {
        protected readonly Screen _preScreen, _nextScreen;

        public TransitionScreen(Screen preScreen, Screen nextScreen)
        {
            _preScreen = preScreen;
            _nextScreen = nextScreen;
        }
    }
}
