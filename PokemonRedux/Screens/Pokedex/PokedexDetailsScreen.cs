using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Pokemons;
using System;
using System.Globalization;
using System.Linq;
using static Core;
using static System.Environment;

namespace PokemonRedux.Screens.Pokedex
{
    class PokedexDetailsScreen : Screen
    {
        private const int SELECTOR_FLICKER_DELAY = 7;
        private static readonly string[] OPTIONS = new[]
        {
            "PAGE",
            "AREA",
            "CRY",
            "BACK"
        };

        private readonly Screen _preScreen;
        private readonly Func<PokedexEntry> _getNextEntry, _getPreviousEntry;

        private SpriteBatch _batch;
        private Texture2D _overlay, _pageIndicators, _footprints;
        private PokemonFontRenderer _fontRenderer;

        private PokedexEntry _entry;
        private string[] _pages;
        private string _height, _weight;
        private Color[] _portraitPalette;
        private int _pageIndex = 0; // description pages
        private int _cursorIndex = 0; // page/area/cry/back
        private bool _selectorVisible = true;
        private int _selectorFlickerDelay = SELECTOR_FLICKER_DELAY;

        public PokedexDetailsScreen(Screen preScreen, PokedexEntry entry, Func<PokedexEntry> GetNextEntry, Func<PokedexEntry> GetPreviousEntry)
        {
            _preScreen = preScreen;
            _entry = entry;
            _getNextEntry = GetNextEntry;
            _getPreviousEntry = GetPreviousEntry;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _overlay = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/detailsOverlay.png");
            _pageIndicators = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/pageIndicators.png");
            _footprints = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/footprints.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            // initialize first entry
            InitializeEntry();
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

            // draw black background
            _batch.DrawRectangle(new Rectangle(startX, 0, width, height), Color.Black);

            // draw pokemon portrait
            _batch.DrawRectangle(new Rectangle(
                (int)(startX + Border.SCALE * 3), unit,
                unit * 7, unit * 7), Border.DefaultWhite);
            var portrait = PokemonTextureManager.GetFront(_entry.Id, _portraitPalette);
            _batch.Draw(portrait, new Rectangle(
                (int)(startX + Border.SCALE * 3), unit,
                (int)(Border.SCALE * portrait.Width),
                (int)(Border.SCALE * portrait.Height)), Color.White);

            // pokemon id
            _fontRenderer.DrawText(_batch, "^No." + _entry.Id.ToString("D3"),
                new Vector2(startX + Border.SCALE * 3 + unit, unit * 8), Border.DefaultWhite, Border.SCALE);

            // basic info
            _fontRenderer.DrawText(_batch,
                _entry.Name + NewLine +
                _entry.Species + NewLine +
                "HT" + _height + NewLine +
                "WT" + _weight,
                new Vector2(startX + Border.SCALE * 3 + unit * 8, unit * 3), Border.DefaultWhite, Border.SCALE);

            // footprint
            // 16 footprints per row
            var footprintTextureX = ((_entry.Id - 1) % 16) * 16;
            var footprintTextureY = (int)Math.Floor((_entry.Id - 1) / 16d) * 16;
            _batch.Draw(_footprints, new Rectangle(
                (int)(startX + unit * 17 + Border.SCALE * 3),
                unit, unit * 2, unit * 2),
                new Rectangle(footprintTextureX, footprintTextureY, 16, 16), Color.White);

            if (_entry.IsCaught)
            {
                // text page
                _fontRenderer.DrawText(_batch, _pages[_pageIndex],
                    new Vector2(startX + Border.SCALE * 3 + unit, unit * 11), Border.DefaultWhite, Border.SCALE);

                // page indicator
                _batch.Draw(_pageIndicators, new Rectangle(
                    (int)(startX + Border.SCALE * 3),
                    (int)(unit * 9 + Border.SCALE * 5),
                    (int)(Border.SCALE * _pageIndicators.Width),
                    (int)(Border.SCALE * _pageIndicators.Height / 2)),
                    new Rectangle(0, _pageIndex * 8, 16, 8), Color.White);
            }

            // options and cursor
            var optionsStr = string.Join("", OPTIONS.Select((o, i) =>
            {
                if (i == _cursorIndex && _selectorVisible)
                {
                    return ">" + o;
                }
                else
                {
                    return " " + o;
                }
            }));
            _fontRenderer.DrawText(_batch, optionsStr,
                new Vector2(startX + Border.SCALE * 3, unit * 17), Border.DefaultWhite, Border.SCALE);


            // overlay
            _batch.Draw(_overlay, new Rectangle(startX, 0, width, height), Color.White);

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            _selectorFlickerDelay--;
            if (_selectorFlickerDelay == 0)
            {
                _selectorFlickerDelay = SELECTOR_FLICKER_DELAY;
                _selectorVisible = !_selectorVisible;
            }

            if (GameboyInputs.RightPressed() && _cursorIndex + 1 < OPTIONS.Length)
            {
                _cursorIndex++;
            }
            else if (GameboyInputs.LeftPressed() && _cursorIndex > 0)
            {
                _cursorIndex--;
            }
            else if (GameboyInputs.UpPressed())
            {
                var newEntry = _getPreviousEntry();
                if (newEntry != null)
                {
                    _entry = newEntry;
                    _cursorIndex = 0; // set cursor to PAGE
                    _pageIndex = 0;
                    InitializeEntry();
                }
            }
            else if (GameboyInputs.DownPressed())
            {
                var newEntry = _getNextEntry();
                if (newEntry != null)
                {
                    _entry = newEntry;
                    _cursorIndex = 0; // set cursor to PAGE
                    _pageIndex = 0;
                    InitializeEntry();
                }
            }

            if (GameboyInputs.APressed())
            {
                switch (_cursorIndex)
                {
                    case 0: // PAGE
                        _pageIndex++;
                        if (_pageIndex == _pages.Length)
                        {
                            _pageIndex = 0;
                        }
                        break;
                    case 1: // AREA
                        var nestScreen = new PokedexNestScreen(this, _entry);
                        nestScreen.LoadContent();
                        GetComponent<ScreenManager>().SetScreen(nestScreen);
                        break;
                    case 2: // CRY
                        // TODO: play cry
                        break;
                    case 3: // BACK
                        Close();
                        break;
                }
            }
            else if (GameboyInputs.BPressed())
            {
                Close();
            }
        }

        private void InitializeEntry()
        {
            // extract and load palette
            var pokemonData = PokemonData.Get(_entry.Id);
            _portraitPalette = PokemonTextureManager.GetPalette(pokemonData.colors.normal);

            // split text into pages
            _pages = _entry.Text.Split(new[] { "\n\n" }, StringSplitOptions.None);

            // get height/weigth and format
            if (_entry.IsCaught)
            {
                var height = _entry.Height;
                var feet = (int)Math.Floor(height);
                var inches = ((int)Math.Round((height - feet) * 100)).ToString("D2");
                _height = (feet.ToString() + "^'1" + inches + "^'2").PadLeft(11);

                _weight = _entry.Weigth.ToString("N1", CultureInfo.InvariantCulture) + "lb";
                _weight = _weight.PadLeft(8);
            }
            else
            {
                _height = "?^'1??^'2".PadLeft(11);
                _weight = "???lb".PadLeft(8);
            }

            // save selected entry
            Controller.ActivePlayer.MenuStates.PokedexLastSelectedId = _entry.Id;
        }

        private void Close()
        {
            GetComponent<ScreenManager>().SetScreen(_preScreen);
        }
    }
}
