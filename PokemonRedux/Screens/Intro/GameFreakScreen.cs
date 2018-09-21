using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Screens.Title;
using PokemonRedux.Screens.Transition;
using System;
using static Core;

namespace PokemonRedux.Screens.Intro
{
    class GameFreakScreen : Screen
    {
        private const int FRAMES_PER_ROW = 4;
        private const int FRAMES_TOTAL = 102;
        private const int FRAME_WIDTH = 160;
        private const int FRAME_HEIGHT = 144;
        private const float FRAME_SPEED = 0.8f;
        private const int END_DELAY = 100;

        private SpriteBatch _batch;
        private Texture2D _texture;

        private float _currentFrame = 0;
        private int _endDelay = END_DELAY;

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Intro/gameFreak.png");
        }

        internal override void UnloadContent()
        {
        }

        internal override void Draw(GameTime gameTime)
        {
            _batch.Begin(samplerState: SamplerState.PointClamp);

            _batch.DrawRectangle(Controller.ClientRectangle, Color.Black);

            var texX = (int)Math.Floor(_currentFrame);
            var texY = 0;
            while (texX >= FRAMES_PER_ROW)
            {
                texX -= FRAMES_PER_ROW;
                texY++;
            }
            texX *= FRAME_WIDTH;
            texY *= FRAME_HEIGHT;

            var height = Controller.ClientRectangle.Height;
            var width = (int)(Controller.ClientRectangle.Height * (FRAME_WIDTH / (float)FRAME_HEIGHT));

            _batch.Draw(_texture, new Rectangle(Controller.ClientRectangle.Width / 2 - width / 2, 0, width, height),
                new Rectangle(texX, texY, FRAME_WIDTH, FRAME_HEIGHT), Color.White);

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            if (_currentFrame < FRAMES_TOTAL - 1)
            {
                if (_currentFrame > 80)
                {
                    _currentFrame += FRAME_SPEED / 1.5f;
                }
                else
                {
                    _currentFrame += FRAME_SPEED;
                }
                if (_currentFrame >= FRAMES_TOTAL - 1)
                {
                    _currentFrame = FRAMES_TOTAL - 1;
                }
            }
            else
            {
                _endDelay--;
                if (_endDelay == 0)
                {
                    var titleScreen = new TitleScreen();
                    titleScreen.LoadContent();

                    var transitionScreen = new FadeTransitionScreen(this, titleScreen, 0.1f);
                    transitionScreen.LoadContent();

                    GetComponent<ScreenManager>().SetScreen(transitionScreen);
                }
            }
        }
    }
}
