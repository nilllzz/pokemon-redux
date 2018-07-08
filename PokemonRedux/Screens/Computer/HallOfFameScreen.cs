using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Pokemons;
using static Core;

namespace PokemonRedux.Screens.Computer
{
    class HallOfFameScreen : Screen
    {
        private readonly Screen _preScreen;

        private SpriteBatch _batch;
        private PokemonFontRenderer _fontRenderer;

        private int _index;
        private Pokemon _pokemon; // pokemon to get sprite and gender from

        public HallOfFameScreen(Screen preScreen)
        {
            _preScreen = preScreen;
            _index = Controller.ActivePlayer.HallOfFame.Length - 1; // reverse order
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
            _fontRenderer.LineGap = 0;

            SetPokemon();
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var (unit, startX, width, height) = Border.GetDefaultScreenValues();

            // background
            Border.DrawCenter(_batch, startX, 0, Border.SCREEN_WIDTH, Border.SCREEN_HEIGHT, Border.SCALE);

            var entry = Controller.ActivePlayer.HallOfFame[_index];

            // upper box
            Border.Draw(_batch, startX, 0, Border.SCREEN_WIDTH, 5, Border.SCALE);
            _fontRenderer.DrawText(_batch, entry.number.ToString().PadLeft(3) + "-Time Famer",
                new Vector2(startX + unit * 2, unit * 2), Color.Black, Border.SCALE);

            // pokemon sprite
            var sprite = _pokemon.GetFrontSprite();
            _batch.Draw(sprite, new Rectangle(
                startX + unit * 6, unit * 5,
                (int)(Border.SCALE * sprite.Width),
                (int)(Border.SCALE * sprite.Height)), Color.White);

            // lower box
            Border.Draw(_batch, startX, unit * 12, Border.SCREEN_WIDTH, 6, Border.SCALE);
            var levelStr = ("^:L" + _pokemon.Level).PadRight(8);
            if (_pokemon.Level == Pokemon.MAX_LEVEL)
            {
                levelStr = _pokemon.Level.ToString().PadRight(6);
            }
            _fontRenderer.DrawText(_batch,
                $"^No.{_pokemon.Id.ToString("D3")} {_pokemon.DisplayName.PadRight(Pokemon.MAX_NAME_LENGTH)} {PokemonStatHelper.GetGenderChar(_pokemon.Gender)}\n" +
                new string(' ', 7) + "/" + _pokemon.Name + "\n\n" +
                levelStr + "^ID^No/" + entry.trainerId.ToString("D5"),
                new Vector2(startX + unit, unit * 13), Color.Black, Border.SCALE);

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            if (GameboyInputs.APressed())
            {
                if (_index > 0)
                {
                    _index--;
                    SetPokemon();
                }
                else
                {
                    Close();
                }
            }
            else if (GameboyInputs.BPressed() || GameboyInputs.StartPressed())
            {
                Close();
            }
        }

        private void SetPokemon()
        {
            var entry = Controller.ActivePlayer.HallOfFame[_index];
            var data = PokemonSaveData.GenerateNew();
            data.dv = entry.dv;
            data.id = entry.id;
            data.nickname = entry.nickname;
            data.experience = entry.experience;
            _pokemon = Pokemon.Get(data);
        }

        private void Close()
        {
            GetComponent<ScreenManager>().SetScreen(_preScreen);
        }
    }
}
