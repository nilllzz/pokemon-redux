using GameDevCommon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Core;

namespace PokemonRedux.Screens.Transition
{
    class FadeTransitionScreen : TransitionScreen
    {
        private readonly float _speed;

        private RenderTarget2D _target;
        private SpriteBatch _batch;

        private float _progress;

        public FadeTransitionScreen(Screen preScreen, Screen nextScreen, float speed)
            : base(preScreen, nextScreen)
        {
            _speed = speed;
        }

        internal override void LoadContent()
        {
            _target = RenderTargetManager.CreateScreenTarget();
            _batch = new SpriteBatch(Controller.GraphicsDevice);
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _nextScreen.Draw(gameTime);

            var previousTargets = Controller.GraphicsDevice.GetRenderTargets();
            Controller.GraphicsDevice.SetRenderTarget(_target);
            Controller.GraphicsDevice.Clear(Color.Transparent);

            _preScreen.Draw(gameTime);

            Controller.GraphicsDevice.SetRenderTargets(previousTargets);

            _batch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.NonPremultiplied);
            _batch.Draw(_target, Vector2.Zero, new Color(255, 255, 255, (int)(255 * (1f - _progress))));
            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            _progress += _speed;
            if (_progress >= 1f)
            {
                _progress = 1f;
                GetComponent<ScreenManager>().SetScreen(_nextScreen);
            }
        }
    }
}
