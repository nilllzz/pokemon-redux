using PokemonRedux.Game.Pokemons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core;

namespace PokemonRedux.Screens.Computer
{
    class StorageSystemMoveScreen : StorageSystemScreen
    {
        protected override bool CanChangeBox => true;
        // box index of -1 indicates party
        protected override bool IsBoxMode => _boxIndex >= 0;

        public StorageSystemMoveScreen(Screen preScreen)
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
            _optionsBox.Show(new[] { "MOVE", "STATS", "RELEASE", "CANCEL" });
        }

        protected override void UpdatePokemonList()
        {
            if (_boxIndex == -1)
            {
                SetPokemonList(Controller.ActivePlayer.PartyPokemon, "PARTY ^PK^MN");
            }
            else
            {
                var box = Controller.ActivePlayer.Boxes[_boxIndex];
                SetPokemonList(box.GetPokemon(), box.Name);
            }
        }

        protected override void LeftPressed()
        {
            // -1 is party
            _boxIndex--;
            if (_boxIndex == -2)
            {
                _boxIndex = StorageBox.BOX_COUNT - 1;
            }
            UpdatePokemonList();
        }

        protected override void RightPressed()
        {
            // -1 is party
            _boxIndex++;
            if (_boxIndex == StorageBox.BOX_COUNT)
            {
                _boxIndex = -1;
            }
            UpdatePokemonList();
        }
    }
}
