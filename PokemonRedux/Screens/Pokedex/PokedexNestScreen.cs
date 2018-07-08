using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.TownMap;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens.Pokedex
{
    class PokedexNestScreen : Screen
    {
        private const int NEST_FLICKER_DELAY = 20;
        private static readonly Color FONT_COLOR = new Color(224, 248, 160);

        private readonly Screen _preScreen;
        private readonly PokedexEntry _entry;
        private readonly string _location;

        private SpriteBatch _batch;
        private Texture2D _mapJohto, _mapKanto, _nestIndicator, _player;
        private MapEntry[] _entries;
        private PokemonFontRenderer _fontRenderer;

        private int _index; // 0 => Johto, 1 => Kanto
        private bool _playerVisible = false;
        private bool _nestsVisible = true;
        private int _nestFlickerDelay = NEST_FLICKER_DELAY;

        public PokedexNestScreen(Screen preScreen, PokedexEntry entry)
        {
            _preScreen = preScreen;
            _entry = entry;
            // TODO: set location from current map location
            _location = "NEW BARK TOWN";
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _mapJohto = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/mapJohto.png");
            _mapKanto = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/mapKanto.png");
            _nestIndicator = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/nestIndicator.png");
            _player = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/player.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            LoadMapEntries();
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
            _batch.DrawRectangle(new Rectangle(startX, 0, width, unit), Color.Black);

            // draw title
            _fontRenderer.DrawText(_batch, _entry.Name + "'S NEST",
                new Vector2(startX + unit * 2, 0), FONT_COLOR, Border.SCALE);

            // draw map
            var mapRect = new Rectangle(startX, unit, width, height - unit);
            var mapTexture = _index == 0 ? _mapJohto : _mapKanto;
            _batch.Draw(mapTexture, mapRect, Color.White);

            // draw nest locations
            if (_entry.Areas.Length > 0 && !_playerVisible && _nestsVisible)
            {
                foreach (var mapEntry in _entries)
                {
                    if (_entry.Areas.Contains(mapEntry.Name.Replace("\n", " ").ToUpper()))
                    {
                        _batch.Draw(_nestIndicator, new Rectangle(
                            (int)(startX + mapEntry.Position.X * Border.SCALE + unit),
                            (int)(mapEntry.Position.Y * Border.SCALE + unit),
                            unit * 2, unit * 2), Color.White);
                    }
                }
            }

            // draw player location
            if (_playerVisible)
            {
                var mapEntry = _entries.FirstOrDefault(e => e.Name.Replace("\n", " ").ToUpper() == _location.ToUpper());
                if (!string.IsNullOrEmpty(mapEntry.Name)) // is in the current region
                {
                    _batch.Draw(_player, new Rectangle(
                        (int)(startX + mapEntry.Position.X * Border.SCALE + unit),
                        (int)(mapEntry.Position.Y * Border.SCALE + unit),
                        unit * 2, unit * 2), Color.White);
                }
            }

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            _playerVisible = GameboyInputs.SelectDown();

            if (!_playerVisible)
            {
                _nestFlickerDelay--;
                if (_nestFlickerDelay == 0)
                {
                    _nestFlickerDelay = NEST_FLICKER_DELAY;
                    _nestsVisible = !_nestsVisible;
                }
            }

            if (GameboyInputs.APressed() || GameboyInputs.BPressed())
            {
                Close();
            }

            if (GameboyInputs.RightPressed() && _index == 0 && Controller.ActivePlayer.VisitedKanto)
            {
                _index = 1;
                LoadMapEntries();
            }
            else if (GameboyInputs.LeftPressed() && _index == 1)
            {
                _index = 0;
                LoadMapEntries();
            }
        }

        private void LoadMapEntries()
        {
            switch (_index)
            {
                case 0:
                    _entries = MapEntry.GetJohtoMapData();
                    break;
                case 1:
                    _entries = MapEntry.GetKantoMapData();
                    break;
            }
        }

        private void Close()
        {
            GetComponent<ScreenManager>().SetScreen(_preScreen);
        }
    }
}
