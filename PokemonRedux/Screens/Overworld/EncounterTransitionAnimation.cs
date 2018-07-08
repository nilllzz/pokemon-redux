using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using System;
using static Core;

namespace PokemonRedux.Screens.Overworld
{
    class EncounterTransitionAnimation
    {
        private const int ENCOUNTER_ANIMATIONS = 1;
        private const int ANIMATION_DELAY = 2;
        private const int FRAME_WIDTH = 160;
        private const int FRAME_HEIGHT = 144;

        private static readonly Random _random = new Random();

        private SpriteBatch _batch;
        private Texture2D _texture;

        private readonly int _animation;
        private int _frame;
        private int _frames;
        private int _animationDelay = ANIMATION_DELAY;

        public event Action AnimationFinished;

        public EncounterTransitionAnimation()
        {
            _animation = _random.Next(0, ENCOUNTER_ANIMATIONS);
        }

        public void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _texture = Controller.Content.LoadDirect<Texture2D>($"Textures/UI/World/Encounter/{_animation}.png");
            _frames = (int)(Math.Floor((double)_texture.Width / FRAME_WIDTH));
        }

        public void UnloadContent()
        {

        }

        public void Draw(GameTime gameTime)
        {
            _batch.Begin(samplerState: SamplerState.PointClamp);

            _batch.Draw(_texture, Controller.ClientRectangle,
                new Rectangle(FRAME_WIDTH * _frame, 0, FRAME_WIDTH, FRAME_HEIGHT), Color.White);

            _batch.End();
        }

        public void Update(GameTime gameTime)
        {
            _animationDelay--;
            if (_animationDelay == 0)
            {
                _animationDelay = ANIMATION_DELAY;
                if (_frame == _frames - 1)
                {
                    AnimationFinished?.Invoke();
                }
                _frame++;
            }
        }
    }
}
