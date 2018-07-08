using static Core;

namespace PokemonRedux.Screens.Computer
{
    class StorageSystemDepositScreen : StorageSystemScreen
    {
        protected override bool IsBoxMode => false;

        public StorageSystemDepositScreen(Screen preScreen)
            : base(preScreen)
        { }

        internal override void LoadContent()
        {
            base.LoadContent();

            UpdatePokemonList();
        }

        protected override void OpenOptionsMenu()
        {
            _optionsBox.BufferUp = 1;
            _optionsBox.BufferRight = 1;
            _optionsBox.Show(new[] { "DEPOSIT", "STATS", "RELEASE", "CANCEL" });
        }

        protected override void UpdatePokemonList()
        {
            SetPokemonList(Controller.ActivePlayer.PartyPokemon, "PARTY ^PK^MN");
        }
    }
}
