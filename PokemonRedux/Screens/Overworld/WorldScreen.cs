using GameDevCommon.Drawing;
using GameDevCommon.Drawing.Font;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Overworld;
using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Battles;
using System;
using static Core;

namespace PokemonRedux.Screens.Overworld
{
    class WorldScreen : Screen, ITextboxScreen
    {
        private WorldShader _shader;
        private WorldCamera _camera;
        private SpriteBatch _batch;
        private LocationSign _locationSign;
        private FontRenderer _fontRenderer;
        private EncounterTransitionAnimation _encounterAnimation;

        public bool EncounterStarted { get; private set; }
        public World World { get; private set; }
        public Textbox Textbox { get; private set; }
        public StartMenu StartMenu { get; private set; }

        internal override void LoadContent()
        {
            World = new World();

            _locationSign = new LocationSign();
            _locationSign.LoadContent();
            World.MapChanged += MapChanged;

            Textbox = new Textbox();
            Textbox.LoadContent();

            StartMenu = new StartMenu(this);
            StartMenu.LoadContent();

            World.LoadContent();
            World.ChangeMap(Controller.ActivePlayer.Map);
            World.PlayerEntity.Position = Controller.ActivePlayer.Position + World.ActiveMap.Offset;

            _shader = new WorldShader(this);
            ((BasicEffect)_shader.Effect).LightingEnabled = false;

            _camera = new WorldCamera(World, World.PlayerEntity);
            _batch = new SpriteBatch(Controller.GraphicsDevice);
            _fontRenderer = new FontRenderer("main");
        }

        internal override void UnloadContent()
        {

        }

        private void MapChanged()
        {
            _locationSign.Show(World.ActiveMap.Name);
        }

        internal override void Update(GameTime gameTime)
        {
            World.Update(gameTime);
            _camera.Update();

            Textbox.Update();
            _locationSign.Update();
            if (!Textbox.Visible)
            {
                if (!StartMenu.Visible && GameboyInputs.StartPressed())
                {
                    StartMenu.Show();
                }
                else
                {
                    StartMenu.Update();
                }
            }
            if (_locationSign.Visible && (Textbox.Visible || StartMenu.Visible || EncounterStarted))
            {
                _locationSign.Close();
            }

            World.IsPaused = Textbox.Visible || StartMenu.Visible || EncounterStarted;

#if DEBUG
            if (GetComponent<GameDevCommon.Input.KeyboardHandler>().KeyPressed(Microsoft.Xna.Framework.Input.Keys.B))
            {
                EncounterStarted = true;
                (_shader as WorldShader).StartEncounter();
                (_shader as WorldShader).EncounterAnimationFinished += EncounterFlashAnimationFinished;
            }
#endif

            _shader.Update();
            _encounterAnimation?.Update(gameTime);
        }

        internal override void Draw(GameTime gameTime)
        {
            Controller.GraphicsDevice.ResetFull();
            Controller.GraphicsDevice.ClearFull(GetClearColor());

            _shader.Prepare(_camera);
            World.Draw(_shader);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            if (World.FadeAlpha > 0f)
            {
                _batch.DrawRectangle(Controller.ClientRectangle, new Color(0, 0, 0, (int)(255 * World.FadeAlpha)));
            }

            _locationSign.Draw(_batch);
            Textbox.Draw(_batch, Color.White);
            StartMenu.Draw(_batch);

            _encounterAnimation?.Draw(gameTime);

#if DEBUG
            var playerPosition = World.PlayerEntity.Position;
            playerPosition -= World.ActiveMap.Offset;

            _batch.DrawText(_fontRenderer, $"X:{Math.Round(playerPosition.X, 0)}, Y:{Math.Round(playerPosition.Y, 2)}, Z:{Math.Round(playerPosition.Z, 0)}; {World.ActiveMap.MapFile}",
                Vector2.Zero, Color.White);
#endif

            _batch.End();
        }

        // event handler for the shader's encounter animation
        private void EncounterFlashAnimationFinished()
        {
            _encounterAnimation = new EncounterTransitionAnimation();
            _encounterAnimation.LoadContent();
            _encounterAnimation.AnimationFinished += EncounterAnimationFinished;
        }

        private void EncounterAnimationFinished()
        {
            // dispose of encounter animation resources
            _encounterAnimation.AnimationFinished -= EncounterAnimationFinished;
            _encounterAnimation.UnloadContent();
            _encounterAnimation = null;
            EncounterStarted = false;

            // load battle screen
            var battleScreen = new WildBattleScreen(this, Pokemon.Get(87, 35));
            battleScreen.LoadContent();
            GetComponent<ScreenManager>().SetScreen(battleScreen);
        }

        private Color GetClearColor()
        {
            switch (World?.Daytime)
            {
                case Daytime.Day:
                case Daytime.Morning:
                    return new Color(53, 53, 52);

                case Daytime.Night:
                    return Color.Black;

                default:
                    return Color.Black;
            }
        }

        public void ShowTextbox(string text, bool skip)
        {
            Textbox.Show(text);
        }
    }
}
