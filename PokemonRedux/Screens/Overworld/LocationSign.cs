using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using static Core;

namespace PokemonRedux.Screens.Overworld
{
    class LocationSign
    {
        private const int HEIGHT = 128;
        private const int WIDTH = 640;
        private const int DELAY_FRAMES = 100;
        private const float TEXT_SIZE = 4f;

        private Texture2D _texture;
        private PokemonFontRenderer _fontRenderer;
        private int _y;
        private string _text;
        private int _delay = 0;

        public bool Visible { get; private set; } = false;

        public void LoadContent()
        {
            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/UI/World/locationSign.png");
        }

        public void Show(string text)
        {
            text = text.ToUpper();
            if (text != _text)
            {
                Visible = true;
                _y = HEIGHT;
                _text = text;
                _delay = DELAY_FRAMES;
            }
        }

        public void Close()
        {
            Visible = false;
        }

        public void Update()
        {
            if (Visible)
            {
                if (_delay > 0)
                {
                    if (_y > 0)
                    {
                        _y -= 4;
                    }
                    else
                    {
                        _delay--;
                    }
                }
                else
                {
                    _y += 4;
                    if (_y >= HEIGHT)
                    {
                        Visible = false;
                    }
                }
            }
        }

        public void Draw(SpriteBatch batch)
        {
            if (Visible)
            {
                batch.Draw(_texture, new Rectangle(Controller.ClientRectangle.Width / 2 - WIDTH / 2,
                    Controller.ClientRectangle.Height - HEIGHT + _y, WIDTH, HEIGHT), Color.White);

                var textSize = _fontRenderer.MeasureText(_text, TEXT_SIZE);
                _fontRenderer.DrawText(batch, _text, new Vector2(Controller.ClientRectangle.Width / 2f - textSize.X / 2f,
                    Controller.ClientRectangle.Height - HEIGHT / 2f + _y - textSize.Y / 2f), Color.Black, TEXT_SIZE);
            }
        }
    }
}
