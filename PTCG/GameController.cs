using GameDevCommon;
using GameDevCommon.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PTCG.Screens;
using static Core;

namespace PTCG
{
    class GameController : Game, IGame
    {
        public const int RENDER_WIDTH = 1280;
        public const int RENDER_HEIGHT = 720;

        private SpriteBatch _batch;

        internal GraphicsDeviceManager DeviceManager { get; }
        internal ComponentManager ComponentManager { get; }
        public Game GetGame() => this;
        public ComponentManager GetComponentManager() => ComponentManager;
        internal Rectangle ClientRectangle => new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

        public GameController()
        {
            DeviceManager = new GraphicsDeviceManager(this);
            DeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            ComponentManager = new ComponentManager();
        }

        protected override void Initialize()
        {
            GameInstanceProvider.SetInstance(this);
            ComponentManager.LoadComponents();

            base.Initialize();

            DeviceManager.PreferredBackBufferWidth = RENDER_WIDTH;
            DeviceManager.PreferredBackBufferHeight = RENDER_HEIGHT;
            DeviceManager.ApplyChanges();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            _batch = new SpriteBatch(GraphicsDevice);
            RenderTargetManager.Initialize(RENDER_WIDTH, RENDER_HEIGHT);
            GetComponent<ScreenManager>().ActiveScreen.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            GetComponent<ControlsHandler>().Update();
            GetComponent<ScreenManager>().ActiveScreen.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            GraphicsDevice.SetRenderTarget(RenderTargetManager.DefaultTarget);

            GetComponent<ScreenManager>().ActiveScreen.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(null);

            _batch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            _batch.Draw(RenderTargetManager.DefaultTarget, ClientRectangle, Color.White);
            _batch.End();

            base.Draw(gameTime);
        }
    }
}
