using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using static Core;

namespace PokemonRedux.Screens.TownMap
{
    class TownMapScreen : Screen
    {
        private const int PLAYER_FRAME_TIME = 10;
        private static readonly int[] PLAYER_ANIMATION_FRAMES = new[] { 0, 1, 0, 2 };

        private readonly Screen _preScreen;
        private readonly string _region;
        private readonly string _location;
        private readonly MapEntry[] _entries;
        private readonly int _playerIndex;

        private SpriteBatch _batch;
        private Texture2D _map, _selector, _player;
        private PokemonFontRenderer _fontRenderer;

        private int _selectedIndex;
        private int _playerFrameIndex = 0;
        private int _playerFrameDelay = PLAYER_FRAME_TIME;

        public TownMapScreen(Screen preScreen, string region, string location)
        {
            _preScreen = preScreen;
            _region = region;
            _location = location;

            switch (_region)
            {
                case "johto":
                    _entries = MapEntry.GetJohtoMapData();
                    break;
                case "kanto":
                    _entries = MapEntry.GetKantoMapData();
                    break;
            }

            for (var i = 0; i < _entries.Length; i++)
            {
                var entry = _entries[i];
                if (entry.Name.Replace('\n', ' ').ToLower() == _location.ToLower())
                {
                    _selectedIndex = i;
                    _playerIndex = i;
                    break;
                }
            }
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);
            _map = Controller.Content.LoadDirect<Texture2D>($"Textures/UI/TownMap/{_region}.png");
            _selector = Controller.Content.LoadDirect<Texture2D>("Textures/UI/TownMap/selector.png");
            _player = Controller.Content.LoadDirect<Texture2D>("Textures/UI/TownMap/player.png");
            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
            _fontRenderer.LineGap = 0;
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);
            _batch.Begin(samplerState: SamplerState.PointClamp);

            Draw(gameTime, _batch);

            _batch.End();
        }

        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            var startX = (int)(Controller.ClientRectangle.Width / 2f - _map.Width * Border.SCALE / 2f);

            batch.Draw(_map, new Rectangle(startX, 0,
                (int)(_map.Width * Border.SCALE),
                (int)(_map.Height * Border.SCALE)), Color.White);

            var selectedEntry = _entries[_selectedIndex];
            batch.Draw(_selector,
                new Rectangle(startX + (int)((selectedEntry.Position.X + 8) * Border.SCALE),
                            (int)((selectedEntry.Position.Y + 8) * Border.SCALE),
                            (int)(_selector.Width * Border.SCALE),
                            (int)(_selector.Height * Border.SCALE)),
                Color.White);

            var playerEntry = _entries[_playerIndex];
            batch.Draw(_player,
                new Rectangle(startX + (int)((playerEntry.Position.X + 8) * Border.SCALE),
                            (int)((playerEntry.Position.Y + 8) * Border.SCALE),
                            (int)(16 * Border.SCALE),
                            (int)(16 * Border.SCALE)),
                new Rectangle(PLAYER_ANIMATION_FRAMES[_playerFrameIndex] * 16, 0, 16, 16),
                Color.White);

            _fontRenderer.DrawText(batch, selectedEntry.Name,
                new Vector2(startX + 18 * 4 * Border.SCALE, 0), Color.Black, Border.SCALE);
        }

        internal override void Update(GameTime gameTime)
        {
            if (GameboyInputs.DownPressed())
            {
                _selectedIndex--;
                if (_selectedIndex < 0)
                {
                    _selectedIndex = _entries.Length - 1;
                }
            }
            else if (GameboyInputs.UpPressed())
            {
                _selectedIndex++;
                if (_selectedIndex == _entries.Length)
                {
                    _selectedIndex = 0;
                }
            }

            // only allow player to close the screen if it's on a screen stack
            // if it doesn't have a pre screen, it means it's embedded in another screen (pokegear)
            if (_preScreen != null && GameboyInputs.BPressed())
            {
                Close();
            }

            _playerFrameDelay--;
            if (_playerFrameDelay == 0)
            {
                _playerFrameDelay = PLAYER_FRAME_TIME;
                _playerFrameIndex++;
                if (_playerFrameIndex == PLAYER_ANIMATION_FRAMES.Length)
                {
                    _playerFrameIndex = 0;
                }
            }
        }

        private void Close()
        {
            var screenManager = GetComponent<ScreenManager>();
            screenManager.SetScreen(_preScreen);
        }
    }
}
