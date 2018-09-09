using GameDevCommon;
using GameDevCommon.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game;
using PokemonRedux.Game.Battles;
using PokemonRedux.Game.Data;
using PokemonRedux.Screens;
using static Core;

namespace PokemonRedux
{
    class GameController : Microsoft.Xna.Framework.Game, IGame
    {
        public const int RENDER_WIDTH = 1280;
        public const int RENDER_HEIGHT = 736;

        private SpriteBatch _batch;

        internal GraphicsDeviceManager DeviceManager { get; }
        internal ComponentManager ComponentManager { get; }
        public Microsoft.Xna.Framework.Game GetGame() => this;
        public ComponentManager GetComponentManager() => ComponentManager;
        internal Rectangle ClientRectangle => new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        internal Player ActivePlayer { get; set; }

        public GameController()
        {
            DeviceManager = new GraphicsDeviceManager(this);
            DeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            ComponentManager = new ComponentManager();
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            GameInstanceProvider.SetInstance(this);
            ComponentManager.LoadComponents();

            DeviceManager.PreferredBackBufferWidth = RENDER_WIDTH;
            DeviceManager.PreferredBackBufferHeight = RENDER_HEIGHT;
            DeviceManager.ApplyChanges();

            base.Initialize();

            GameboyInputs.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            // TODO: move player loading
            // load player here
            ActivePlayer = new Player();
            if (!PlayerData.SaveFileExists())
            {
                var data = PlayerData.CreateNew("NILS");
                ActivePlayer.Load(data);
            }
            else
            {
                ActivePlayer.Load(null);
            }

            _batch = new SpriteBatch(GraphicsDevice);
            RenderTargetManager.Initialize(RENDER_WIDTH, RENDER_HEIGHT);
            GetComponent<ScreenManager>().ActiveScreen.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            ActivePlayer?.Update(gameTime);

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

        public Vector2 TranslateScreenToRender(Vector2 screenV)
        {
            var translate = new Vector2((float)RENDER_WIDTH / Window.ClientBounds.Width, (float)RENDER_HEIGHT / Window.ClientBounds.Height);
            return screenV * translate;
        }
    }
}
