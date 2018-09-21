using PokemonRedux.Game.Data;

namespace PokemonRedux.Game
{
    class Options
    {
        private OptionsData _data;

        public int BorderFrameType
        {
            get => _data.frame;
            set => _data.frame = value;
        }
        public int TextSpeed
        {
            get => _data.textSpeed;
            set => _data.textSpeed = value;
        }
        public bool BattleAnimations
        {
            get => _data.battleScene;
            set => _data.battleScene = value;
        }
        public bool BattleStyle
        {
            get => _data.battleStyle;
            set => _data.battleStyle = value;
        }
        public bool MenuExplanations
        {
            get => _data.menuAccount;
            set => _data.menuAccount = value;
        }

        public void Load()
        {
            if (OptionsData.OptionsFileExists())
            {
                _data = OptionsData.Load();
            }
            else
            {
                _data = OptionsData.CreateNew();
                _data.Save(); // save default options upon creation
            }
        }

        public void Save()
        {
            _data.Save();
        }
    }
}
