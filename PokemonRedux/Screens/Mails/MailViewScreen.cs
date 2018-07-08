using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using System;
using System.Collections.Generic;
using static Core;

namespace PokemonRedux.Screens.Mails
{
    class MailViewScreen : Screen
    {
        private const int DEFAULT_AUTHOR_OFFSET = 5;
        private static readonly Color[] POKEMON_PORTRAIT_PALETTE = new[]
        {
            new Color(0, 0, 0),
            new Color(224, 96, 40),
            new Color(248, 144, 32),
            new Color(248, 200, 136),
        };
        private static readonly IReadOnlyDictionary<string, string> MAIL_BACKGROUNDS = new Dictionary<string, string>
        {
            { "BLUESKY MAIL", "bluesky.png" },
            { "EON MAIL", "eon.png" },
            { "FLOWER MAIL", "flower.png" },
            { "LITEBLUEMAIL", "liteblue.png" },
            { "LOVELY MAIL", "lovely.png" },
            { "MIRAGE MAIL", "mirage.png" },
            { "MORPH MAIL", "morph.png" },
            { "MUSIC MAIL", "music.png" },
            { "PORTRAITMAIL", "portrait.png" },
            { "SURF MAIL", "surf.png" },
        };
        private static readonly IReadOnlyDictionary<string, int> MAIL_AUTHOR_OFFSET = new Dictionary<string, int>
        {
            { "MORPH MAIL", 6 },
            { "PORTRAITMAIL", 8 },
        };

        private readonly Screen _preScreen;
        private readonly Mail _mail;

        private SpriteBatch _batch;
        private Texture2D _background;
        private PokemonFontRenderer _fontRenderer;

        public MailViewScreen(Screen preScreen, Pokemon pokemon)
        {
            _preScreen = preScreen;
            _mail = Mail.Get(pokemon.HeldItem.Name, pokemon.ItemData);
            _mail.PokemonId = pokemon.Id;
        }

        public MailViewScreen(Screen preScreen, Mail mail)
        {
            _preScreen = preScreen;
            _mail = mail;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            var backgroundFile = MAIL_BACKGROUNDS[_mail.Template];
            _background = Controller.Content.LoadDirect<Texture2D>($"Textures/UI/Mail/{backgroundFile}");
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var (unit, startX, width, height) = Border.GetDefaultScreenValues();

            // background
            _batch.Draw(_background, new Rectangle(startX, 0, width, height), Color.White);

            // message
            var lines = _mail.Lines;
            var message = lines[0];
            if (lines[1] != null)
            {
                message += Environment.NewLine + lines[1];
            }
            _fontRenderer.DrawText(_batch, message,
                new Vector2(startX + unit * 2, unit * 7), Color.Black, Border.SCALE);

            // pokemon portrait for portraitmail
            if (_mail.Template == "PORTRAITMAIL")
            {
                var sprite = PokemonTextureManager.GetFront(_mail.PokemonId, POKEMON_PORTRAIT_PALETTE, 0);
                _batch.Draw(sprite, new Rectangle(
                    startX + unit, unit * 10,
                    (int)(Border.SCALE * sprite.Width),
                    (int)(Border.SCALE * sprite.Height)),
                    null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            }

            // author
            var offsetX = DEFAULT_AUTHOR_OFFSET;
            if (MAIL_AUTHOR_OFFSET.ContainsKey(_mail.Template))
            {
                offsetX = MAIL_AUTHOR_OFFSET[_mail.Template];
            }
            _fontRenderer.DrawText(_batch, _mail.Author,
                new Vector2(startX + offsetX * unit, unit * 14), Color.Black, Border.SCALE);

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            if (GameboyInputs.APressed() || GameboyInputs.BPressed())
            {
                Close();
            }
        }

        private void Close()
        {
            GetComponent<ScreenManager>().SetScreen(_preScreen);
        }
    }
}
