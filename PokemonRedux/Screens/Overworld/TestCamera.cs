using GameDevCommon.Input;
using GameDevCommon.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static Core;

namespace PokemonRedux.Screens.Overworld
{
    class TestCamera : PerspectiveCamera
    {
        private const float SPEED = 0.2f;

        private float _mouseWheelState;

        public TestCamera()
        {
            _mouseWheelState = Mouse.GetState().ScrollWheelValue;

            Yaw = 0f;
            Pitch = -0.4f;
            FOV = 90f;

            Position = new Vector3(0, 2, 0);

            CreateProjection();
            CreateView();
        }

        //protected override void CreateProjection()
        //{
        //    Projection = Matrix.CreateOrthographicOffCenter(-30f, 30f, -20f, 20f, 0.1f, 200f);
        //}

        public override void Update()
        {
            var kHandler = GetComponent<KeyboardHandler>();
            var velocity = Vector3.Zero;
            if (kHandler.KeyDown(Keys.Left))
            {
                velocity.X -= 1f;
            }
            if (kHandler.KeyDown(Keys.Right))
            {
                velocity.X += 1f;
            }
            if (kHandler.KeyDown(Keys.Up))
            {
                velocity.Z -= 1f;
            }
            if (kHandler.KeyDown(Keys.Down))
            {
                velocity.Z += 1f;
            }

            velocity *= SPEED;

            if (kHandler.KeyDown(Keys.S))
            {
                Pitch -= 0.03f;
            }
            if (kHandler.KeyDown(Keys.W))
            {
                Pitch += 0.03f;
            }

            var mState = Mouse.GetState();
            var scrollDiff = _mouseWheelState - mState.ScrollWheelValue;
            _mouseWheelState = mState.ScrollWheelValue;
            velocity.Y = scrollDiff * 0.01f;

            Position += velocity;
            CreateView();

            //CreateProjection();
        }
    }
}
