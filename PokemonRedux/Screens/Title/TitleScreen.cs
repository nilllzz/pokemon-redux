using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Screens.SaveSelection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Core;

namespace PokemonRedux.Screens.Title
{
    class TitleScreen : Screen
    {
        private const int CLOUD_OFFSET_DELAY = 8;
        private const int HOOH_STAGES = 6;
        private const int HOOH_DELAY = 12;
        private static readonly Color BACKGROUND_COLOR = new Color(120, 160, 248);
        private static readonly ReadOnlyDictionary<int, int> HOOH_OFFSET_FRAMES =
            new ReadOnlyDictionary<int, int>(new Dictionary<int, int>() {
            { 6, -1 },
            { 17, -2 },
            { 18, -1 },
            { 28, 0 },
            { 40, 1 },
            { 64, 0 },
        });

        private Texture2D _clouds, _copyright, _logo, _horizon, _hooh;
        private SpriteBatch _batch;

        private int _cloudOffset = 0;
        private int _cloudDelay = CLOUD_OFFSET_DELAY;
        private int _hoohStage = 0;
        private int _hoohDelay = HOOH_DELAY;
        private int _hoohOffset = 0;

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _clouds = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Title/clouds.png");
            _copyright = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Title/copyright.png");
            _logo = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Title/logo.png");
            _horizon = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Title/horizon.png");
            _hooh = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Title/hooh.png");
        }

        internal override void UnloadContent()
        {
        }

        internal override void Draw(GameTime gameTime)
        {
            _batch.Begin(samplerState: SamplerState.PointClamp);

            _batch.DrawRectangle(Controller.ClientRectangle, BACKGROUND_COLOR);

            var unit = Border.SCALE * Border.UNIT;
            var screenHeight = Border.SCREEN_HEIGHT * unit;
            var screenBottom = (int)(Controller.ClientRectangle.Height / 2 + screenHeight / 2);
            var screenTop = (int)(Controller.ClientRectangle.Height / 2 - screenHeight / 2);

            // logo
            var logoWidth = (int)(Border.SCALE * _logo.Width);
            var logoHeight = (int)(Border.SCALE * _logo.Height);
            _batch.Draw(_logo,
                new Rectangle(Controller.ClientRectangle.Width / 2 - logoWidth / 2,
                    (int)(screenTop + 2 * Border.SCALE),
                    logoWidth, logoHeight),
                Color.White);

            // non-moving horizon pieces
            var horizonWidth = (int)(Border.SCALE * _horizon.Width);
            var horizonHeight = (int)(Border.SCALE * _horizon.Height);
            for (var i = 0; i < Controller.ClientRectangle.Width; i += horizonWidth)
            {
                _batch.Draw(_horizon, new Rectangle(i, (int)(screenBottom - Border.SCALE * 52), horizonWidth, horizonHeight), Color.White);
            }

            // moving clouds
            var cloudWidth = (int)(Border.SCALE * _clouds.Width);
            var cloudHeight = (int)(Border.SCALE * _clouds.Height);
            for (var i = -cloudWidth; i < Controller.ClientRectangle.Width; i += cloudWidth)
            {
                _batch.Draw(_clouds, new Rectangle((int)(i + _cloudOffset * Border.SCALE), (int)(screenBottom - Border.SCALE * 44), cloudWidth, cloudHeight), Color.White);
            }

            // fill lower part with white
            _batch.DrawRectangle(new Rectangle(0, (int)(screenBottom - Border.SCALE * 12),
                Controller.ClientRectangle.Width,
                (int)(Controller.ClientRectangle.Height - screenBottom + Border.SCALE * 12)), Border.DefaultWhite);

            // copyright notice
            var copyrightWidth = (int)(Border.SCALE * _copyright.Width);
            var copyrightHeight = (int)(Border.SCALE * _copyright.Height);
            _batch.Draw(_copyright,
                new Rectangle(Controller.ClientRectangle.Width / 2 - copyrightWidth / 2,
                    (int)(Controller.ClientRectangle.Height - Border.SCALE - copyrightHeight),
                    copyrightWidth, copyrightHeight), Color.White);

            // hooh
            var hoohWidth = (int)(Border.SCALE * _hooh.Width / HOOH_STAGES);
            var hoohHeight = (int)(Border.SCALE * _hooh.Height);
            _batch.Draw(_hooh,
                new Rectangle(Controller.ClientRectangle.Width / 2 - hoohWidth / 2,
                    (int)(screenTop + (52 + _hoohOffset) * Border.SCALE), hoohWidth, hoohHeight),
                new Rectangle(_hoohStage * 64, 0, 64, 64), Color.White);

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            _cloudDelay--;
            if (_cloudDelay == 0)
            {
                _cloudDelay = CLOUD_OFFSET_DELAY;
                _cloudOffset++;
                if (_cloudOffset == 32)
                {
                    _cloudOffset = 0;
                }
            }
            _hoohDelay--;
            if (_hoohDelay == 0)
            {
                _hoohDelay = HOOH_DELAY;
                _hoohStage++;
                if (_hoohStage == HOOH_STAGES)
                {
                    _hoohStage = 0;
                }
            }
            var totalFrame = _hoohStage * HOOH_DELAY + (HOOH_DELAY - _hoohDelay);
            if (HOOH_OFFSET_FRAMES.ContainsKey(totalFrame))
            {
                _hoohOffset = HOOH_OFFSET_FRAMES[totalFrame];
            }

            if (GameboyInputs.APressed() || GameboyInputs.StartPressed())
            {
                var saveSelectionScreen = new SaveSelectionScreen(this);
                saveSelectionScreen.LoadContent();
                GetComponent<ScreenManager>().SetScreen(saveSelectionScreen);
            }
        }
    }
}
