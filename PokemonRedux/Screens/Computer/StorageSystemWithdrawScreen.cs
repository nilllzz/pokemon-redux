using PokemonRedux.Game.Pokemons;
using static Core;

namespace PokemonRedux.Screens.Computer
{
    class StorageSystemWithdrawScreen : StorageSystemScreen
    {
        protected override bool CanChangeBox => true;
        protected override bool IsBoxMode => true;

        public StorageSystemWithdrawScreen(Screen preScreen)
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
            _optionsBox.BufferRight = 0;
            _optionsBox.Show(new[] { "WITHDRAW", "STATS", "RELEASE", "CANCEL" });
        }

        protected override void UpdatePokemonList()
        {
            var box = Controller.ActivePlayer.Boxes[_boxIndex];
            SetPokemonList(box.GetPokemon(), box.Name);
        }

        protected override void LeftPressed()
        {
            _boxIndex--;
            if (_boxIndex == -1)
            {
                _boxIndex = StorageBox.BOX_COUNT - 1;
            }
            UpdatePokemonList();
        }

        protected override void RightPressed()
        {
            _boxIndex++;
            if (_boxIndex == StorageBox.BOX_COUNT)
            {
                _boxIndex = 0;
            }
            UpdatePokemonList();
        }
    }
}
