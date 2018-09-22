using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using System;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens.TrainerCard
{
    class TrainerCardScreen : Screen
    {
        private const int TIME_COLON_SWITCH_DELAY = 40;
        private const int BADGE_FLIP_DELAY = 8;

        private readonly Screen _preScreen;

        private SpriteBatch _batch;
        private Texture2D _background, _badges, _leaders, _pageLabels;
        private PokemonFontRenderer _fontRenderer;

        private int _pageIndex = 0;
        private bool _timeColonVisible = true;
        private int _timeColonDelay = TIME_COLON_SWITCH_DELAY;
        private int _badgeFlipIndex = 0;
        private int _badgeFlipDelay = BADGE_FLIP_DELAY;

        public TrainerCardScreen(Screen preScreen)
        {
            _preScreen = preScreen;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _background = Controller.Content.LoadDirect<Texture2D>("Textures/UI/TrainerCard/background.png");
            _badges = Controller.Content.LoadDirect<Texture2D>("Textures/UI/TrainerCard/badges.png");
            _leaders = Controller.Content.LoadDirect<Texture2D>("Textures/UI/TrainerCard/leaders.png");
            _pageLabels = Controller.Content.LoadDirect<Texture2D>("Textures/UI/TrainerCard/pageLabels.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var startX = (int)(Controller.ClientRectangle.Width / 2f - _background.Width * Border.SCALE / 2f);
            var unit = (int)(Border.SCALE * Border.UNIT);

            _batch.Draw(_background,
                new Rectangle(startX, 0, (int)(_background.Width * Border.SCALE), (int)(_background.Height * Border.SCALE)),
                Color.White);

            var money = "$" + Controller.ActivePlayer.Money.ToString();
            _fontRenderer.DrawText(_batch,
                $"NAME/{Controller.ActivePlayer.Name}" + Environment.NewLine +
                $"^ID^NO {Controller.ActivePlayer.IDNo.ToString("D5")}" + Environment.NewLine +
                "MONEY" + money.PadLeft(7),
                new Vector2(startX + unit * 2, unit * 2), Color.Black, Border.SCALE);

            switch (_pageIndex)
            {
                case 0:
                    DrawPage1(startX, unit);
                    break;
                case 1:
                    DrawPage2(startX, unit);
                    break;
            }

            _batch.End();
        }

        private void DrawPage1(int startX, int unit)
        {
            // draw label
            _batch.Draw(_pageLabels,
                new Rectangle(startX + unit * 2, unit * 8, unit * 5, unit),
                new Rectangle(0, 0, 40, 8), Color.White);

            var dexStr = string.Empty;
            // only put "POKEDEX" information if the player has received one yet
            if (Controller.ActivePlayer.HasPokedex)
            {
                var pokedex = (Controller.ActivePlayer.PokedexSeen.Length + Controller.ActivePlayer.PokedexCaught.Length).ToString();
                dexStr = "POKéDEX" + pokedex.PadLeft(9);
            }
            var (hours, minutes) = Controller.ActivePlayer.GetDisplayTime();
            var length = _timeColonVisible ? 9 : 7;
            var time = hours + (_timeColonVisible ? "^:2" : " ") + minutes;

            _fontRenderer.DrawText(_batch,
                dexStr + Environment.NewLine +
                "PLAY TIME" + time.PadLeft(length),
                new Vector2(startX + unit * 2, unit * 10), Color.Black, Border.SCALE);

            _fontRenderer.DrawText(_batch, "BADGES>",
                new Vector2(startX + unit * 12, unit * 15), Color.Black, Border.SCALE);
        }

        private void DrawPage2(int startX, int unit)
        {
            // draw label
            _batch.Draw(_pageLabels,
                new Rectangle(startX + unit * 2, unit * 8, unit * 5, unit),
                new Rectangle(0, 8, 40, 8), Color.White);

            _batch.Draw(_leaders,
                new Rectangle(startX + unit * 2, unit * 10,
                (int)(_leaders.Width * Border.SCALE),
                (int)(_leaders.Height * Border.SCALE)), Color.White);

            var badges = Controller.ActivePlayer.Badges;
            var x = 0;
            var y = 0;
            for (var i = 9; i <= 16; i++)
            {
                if (badges.Contains(i))
                {
                    Rectangle rect;
                    if (_badgeFlipIndex == 0)
                    {
                        rect = new Rectangle(x * 16, y * 16, 16, 16);
                    }
                    else
                    {
                        rect = new Rectangle((_badgeFlipIndex - 1) * 16, 32, 16, 16);
                    }

                    _batch.Draw(_badges,
                        new Rectangle(startX + unit * 2 + unit * x * 4, unit * 11 + unit * y * 3, unit * 2, unit * 2),
                        rect, Color.White);
                }

                x++;
                if (x == 4)
                {
                    x = 0;
                    y++;
                }
            }
        }

        internal override void Update(GameTime gameTime)
        {
            _timeColonDelay--;
            if (_timeColonDelay == 0)
            {
                _timeColonDelay = TIME_COLON_SWITCH_DELAY;
                _timeColonVisible = !_timeColonVisible;
            }

            _badgeFlipDelay--;
            if (_badgeFlipDelay == 0)
            {
                _badgeFlipDelay = BADGE_FLIP_DELAY;
                _badgeFlipIndex++;
                if (_badgeFlipIndex == 4)
                {
                    _badgeFlipIndex = 0;
                }
            }

            if (GameboyInputs.APressed())
            {
                if (_pageIndex == 0)
                {
                    _pageIndex = 1;
                }
                else
                {
                    Close();
                }
            }
            else if (GameboyInputs.BPressed())
            {
                Close();
            }
            else if (_pageIndex == 0 && GameboyInputs.RightPressed())
            {
                _pageIndex = 1;
            }
            else if (_pageIndex == 1 && GameboyInputs.LeftPressed())
            {
                _pageIndex = 0;
            }
        }

        private void Close()
        {
            var screenManager = GetComponent<ScreenManager>();
            screenManager.SetScreen(_preScreen);
        }
    }
}
