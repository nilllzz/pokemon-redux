using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using static Core;

namespace PokemonRedux.Screens.Pokedex
{
    class PokedexUnownScreen : Screen
    {
        private static readonly string[] UNOWN_TEXTS = new[]
        {
            "ANGRY",
            "BEAR",
            "CHASE",
            "DIRECT",
            "ENGAGE",
            "FIND",
            "GIVE",
            "HELP",
            "INCREASE",
            "JOIN",
            "KEEP",
            "LAUGH",
            "MAKE",
            "NUZZLE",
            "OBSERVE",
            "PERFORM",
            "QUICKEN",
            "REASSURE",
            "SEARCH",
            "TELL",
            "UNDO",
            "VANISH",
            "WANT",
            "XXXXX",
            "YIELD",
            "ZOOM"
        };

        private readonly Screen _preScreen;

        private SpriteBatch _batch;
        private Texture2D _unownLetters, _overlay, _selector;

        private int _index;

        public PokedexUnownScreen(Screen preScreen)
        {
            _preScreen = preScreen;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _unownLetters = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/unownLetters.png");
            _overlay = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/unownOverlay.png");
            _selector = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/unownSelector.png");
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var unit = (int)(Border.SCALE * Border.UNIT);
            var width = Border.SCREEN_WIDTH * unit;
            var height = Border.SCREEN_HEIGHT * unit;
            var startX = (int)(Controller.ClientRectangle.Width / 2f - width / 2f);

            // black background
            _batch.DrawRectangle(new Rectangle(startX, 0, width, height), Color.Black);

            var selectedLetter = Controller.ActivePlayer.UnownsCaught[_index];

            // sprite
            var unownColor = UnownHelper.GetUnownDexColor(selectedLetter);
            var unownColor2 = new Color(unownColor.R / 2, unownColor.G / 2, unownColor.B / 2); // darker shade
            var palette = new[]
            {
                new Color(0, 0, 0),
                unownColor2,
                unownColor,
                new Color(248, 248, 248)
            };
            var sprite = PokemonTextureManager.GetFront(UnownHelper.UNOWN_ID, palette, selectedLetter);
            _batch.DrawRectangle(new Rectangle(
                startX + unit * 6, unit * 5,
                (int)(Border.SCALE * sprite.Width),
                (int)(Border.SCALE * sprite.Height)), Border.DefaultWhite);
            _batch.Draw(sprite, new Rectangle(
                startX + unit * 6, unit * 5,
                (int)(Border.SCALE * sprite.Width),
                (int)(Border.SCALE * sprite.Height)), Color.White);

            // word
            var word = UNOWN_TEXTS[selectedLetter];
            for (int i = 0; i < word.Length; i++)
            {
                var c = word[i] - 65;
                DrawUnownLetter(c, startX + unit * (4 + i), unit * 15);
            }

            // cursor
            Point cursorPos;
            if (_index < 8)
            {
                cursorPos = new Point(unit * 3, unit * (11 - _index));
            }
            else if (_index == 8)
            {
                cursorPos = new Point(unit * 3, unit * 2);
            }
            else if (_index < 18)
            {
                cursorPos = new Point(unit * (5 + _index - 9), unit * 2);
            }
            else if (_index == 18)
            {
                cursorPos = new Point(unit * 15, unit * 2);
            }
            else
            {
                cursorPos = new Point(unit * 15, unit * (4 + _index - 19));
            }
            _batch.Draw(_selector, new Rectangle(
                startX + cursorPos.X,
                cursorPos.Y,
                (int)(Border.SCALE * _selector.Width),
                (int)(Border.SCALE * _selector.Height)), Border.DefaultWhite);

            // unown letter frame
            for (int i = 0; i < Controller.ActivePlayer.UnownsCaught.Length; i++)
            {
                Point letterPos;
                if (i < 8)
                {
                    letterPos = new Point(unit * 4, unit * (11 - i));
                }
                else if (i == 8)
                {
                    letterPos = new Point(unit * 4, unit * 3);
                }
                else if (i < 18)
                {
                    letterPos = new Point(unit * (5 + i - 9), unit * 3);
                }
                else if (i == 18)
                {
                    letterPos = new Point(unit * 14, unit * 3);
                }
                else
                {
                    letterPos = new Point(unit * 14, unit * (4 + i - 19));
                }
                var letter = Controller.ActivePlayer.UnownsCaught[i];
                DrawUnownLetter(letter, startX + letterPos.X, letterPos.Y);
            }

            // overlay
            _batch.Draw(_overlay, new Rectangle(startX, 0, width, height), Color.White);

            _batch.End();
        }

        private void DrawUnownLetter(int letter, int x, int y)
        {
            _batch.Draw(_unownLetters, new Rectangle(
                x, y,
                (int)(Border.SCALE * _unownLetters.Height),
                (int)(Border.SCALE * _unownLetters.Height)),
                new Rectangle(letter * 8, 0, 8, 8), Border.DefaultWhite);
        }

        internal override void Update(GameTime gameTime)
        {
            if (GameboyInputs.RightPressed() && _index < Controller.ActivePlayer.UnownsCaught.Length - 1)
            {
                _index++;
            }
            else if (GameboyInputs.LeftPressed() && _index > 0)
            {
                _index--;
            }

            if (GameboyInputs.APressed() || GameboyInputs.BPressed())
            {
                Close();
            }
        }

        private void Close()
        {
            GetComponent<ScreenManager>().SetScreen(_preScreen);
        }
    }
}
