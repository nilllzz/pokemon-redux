using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Data;
using static Core;

namespace PokemonRedux.Screens.Options
{
    class OptionsScreen : Screen
    {
        private const int OPTIONS_COUNT = 6; // 5 options + 1 cancel button

        private readonly Screen _preScreen;

        private SpriteBatch _batch;
        private PokemonFontRenderer _fontRenderer;

        private int _index = 0;
        private string _optionsText;

        public OptionsScreen(Screen preScreen)
        {
            _preScreen = preScreen;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);
            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
            _fontRenderer.LineGap = 0;

            _optionsText = GetOptionsText();
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var unit = Border.UNIT * Border.SCALE;
            var startX = (int)(Controller.ClientRectangle.Width / 2f - Border.SCREEN_WIDTH * unit / 2f);

            Border.Draw(_batch, startX, 0, Border.SCREEN_WIDTH, OPTIONS_COUNT * 2 + 2, Border.SCALE);

            _fontRenderer.DrawText(_batch, _optionsText,
                new Vector2(startX + unit, unit * 2), Color.Black, Border.SCALE);

            _batch.End();
        }

        private string GetOptionsText()
        {
            string getSelector(int i)
            {
                return i == _index ? ">" : " ";
            };

            var text = "";

            // 0: text speed
            text += getSelector(0) + "TEXT SPEED\n         :";
            switch (Controller.ActivePlayer.TextSpeed)
            {
                case 0:
                    text += "FAST";
                    break;
                case 1:
                    text += "MID";
                    break;
                case 2:
                    text += "SLOW";
                    break;
            }
            // 1: battle scene
            text += "\n" + getSelector(1) + "BATTLE SCENE\n         :";
            text += Controller.ActivePlayer.BattleAnimations ? "ON" : "OFF";
            // 2: battle style
            text += "\n" + getSelector(2) + "BATTLE STYLE\n         :";
            text += Controller.ActivePlayer.BattleStyle ? "SET" : "SHIFT";
            // 3: menu account
            text += "\n" + getSelector(3) + "MENU ACCOUNT\n         :";
            text += Controller.ActivePlayer.MenuExplanations ? "ON" : "OFF";
            // 4: border frame type
            text += "\n" + getSelector(4) + "FRAME\n         :TYPE ";
            text += (Controller.ActivePlayer.BorderFrameType + 1).ToString();

            // 5: cancel option
            text += "\n" + getSelector(5) + "CANCEL";

            return text;
        }

        internal override void Update(GameTime gameTime)
        {
            if (GameboyInputs.RightPressed())
            {
                ChangeOption(1);
                _optionsText = GetOptionsText();
            }
            else if (GameboyInputs.LeftPressed())
            {
                ChangeOption(-1);
                _optionsText = GetOptionsText();
            }

            if (GameboyInputs.DownPressed())
            {
                _index++;
                if (_index == OPTIONS_COUNT)
                {
                    _index = 0;
                }
                _optionsText = GetOptionsText();
            }
            if (GameboyInputs.UpPressed())
            {
                _index--;
                if (_index == -1)
                {
                    _index = OPTIONS_COUNT - 1;
                }
                _optionsText = GetOptionsText();
            }

            if (GameboyInputs.StartPressed() || GameboyInputs.BPressed() || (GameboyInputs.APressed() && _index == OPTIONS_COUNT - 1))
            {
                Close();
            }
        }

        private void Close()
        {
            var screenManager = GetComponent<ScreenManager>();
            screenManager.SetScreen(_preScreen);
        }

        private void ChangeOption(int direction)
        {
            switch (_index)
            {
                // text speed
                case 0:
                    Controller.ActivePlayer.TextSpeed += direction;
                    if (Controller.ActivePlayer.TextSpeed == PlayerData.TEXT_SPEEDS)
                    {
                        Controller.ActivePlayer.TextSpeed = 0;
                    }
                    else if (Controller.ActivePlayer.TextSpeed == -1)
                    {
                        Controller.ActivePlayer.TextSpeed = PlayerData.TEXT_SPEEDS - 1;
                    }
                    break;

                // battle scene
                case 1:
                    Controller.ActivePlayer.BattleAnimations = !Controller.ActivePlayer.BattleAnimations;
                    break;

                // battle style
                case 2:
                    Controller.ActivePlayer.BattleStyle = !Controller.ActivePlayer.BattleStyle;
                    break;

                // menu account
                case 3:
                    Controller.ActivePlayer.MenuExplanations = !Controller.ActivePlayer.MenuExplanations;
                    break;

                // frame
                case 4:
                    Controller.ActivePlayer.BorderFrameType += direction;
                    if (Controller.ActivePlayer.BorderFrameType == Border.BORDER_TYPES)
                    {
                        Controller.ActivePlayer.BorderFrameType = 0;
                    }
                    else if (Controller.ActivePlayer.BorderFrameType == -1)
                    {
                        Controller.ActivePlayer.BorderFrameType = Border.BORDER_TYPES - 1;
                    }
                    break;
            }
        }
    }
}
