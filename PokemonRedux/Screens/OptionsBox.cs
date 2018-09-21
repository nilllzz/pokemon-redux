using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens
{
    class OptionsBox
    {
        private string[] _options;
        private int _width;
        private int _cancelIndex; // index of the option that gets activated when B is pressed
        private PokemonFontRenderer _fontRenderer;

        public bool Visible { get; private set; } = false;
        public int Index { get; private set; } = 0;
        public int OffsetX { get; set; }
        public int OffsetY { get; set; } // baseline of the box, gets drawn -height of box
        public int BufferUp { get; set; } = 0; // the amount of buffer units above the first option
        public int BufferRight { get; set; } = 0; // the amount of buffer to the right of options
        public int Height { get; private set; }
        public bool CloseAfterSelection { get; set; } = true;
        public bool CanCancel { get; set; } = true;

        // string == name of option, int == index of option
        public event Action<string, int> OptionSelected;

        public OptionsBox()
        {
            // default offsets
            var unit = (int)(Border.UNIT * Border.SCALE);
            OffsetX = (int)(Controller.ClientRectangle.Width / 2f - Border.SCREEN_WIDTH * unit / 2f);
            OffsetY = Controller.ClientRectangle.Height - Textbox.HEIGHT * unit;
        }

        public void LoadContent()
        {
            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
        }

        // cancelindex -1 sets it to the last option
        public void Show(string[] options, int cancelIndex = -1)
        {
            Index = 0;
            Visible = true;

            _options = options;

            Height = 2 + (_options.Length - 1) * 2 + 1 + BufferUp;
            _width = 3 + _options.Max(t => PokemonFontRenderer.PrintableCharAmount(t)) + BufferRight;

            _cancelIndex = cancelIndex;
            if (_cancelIndex == -1)
            {
                _cancelIndex = _options.Length - 1;
            }
        }

        public void Close()
        {
            Visible = false;
        }

        public void Draw(SpriteBatch batch, Color color)
        {
            if (Visible)
            {
                var width = (int)(_width * Border.UNIT * Border.SCALE);
                var height = (int)(Height * Border.UNIT * Border.SCALE);
                var unit = (int)(Border.UNIT * Border.SCALE);
                var startX = OffsetX;
                var startY = OffsetY - height;

                Border.Draw(batch, startX, startY, _width, Height, Border.SCALE, color);

                var text = string.Join(Environment.NewLine, _options.Select((t, i) => (i == Index ? ">" : " ") + t));

                _fontRenderer.DrawText(batch, text,
                    new Vector2(startX + unit, startY + unit + BufferUp * unit),
                    Color.Black, Border.SCALE);
            }
        }

        public void Update()
        {
            if (Visible)
            {
                if (GameboyInputs.UpPressed())
                {
                    Index--;
                    if (Index < 0)
                    {
                        Index = _options.Length - 1;
                    }
                }
                else if (GameboyInputs.DownPressed())
                {
                    Index++;
                    if (Index == _options.Length)
                    {
                        Index = 0;
                    }
                }
                else if (CanCancel && GameboyInputs.BPressed())
                {
                    if (_options.Length > _cancelIndex)
                    {
                        OptionSelected?.Invoke(_options[_cancelIndex], _cancelIndex);
                    }
                    else
                    {
                        // option to cancel is not in the options set
                        OptionSelected?.Invoke(null, _cancelIndex);
                    }
                    if (CloseAfterSelection)
                    {
                        Close();
                    }
                }
                else if (GameboyInputs.APressed())
                {
                    OptionSelected?.Invoke(_options[Index], Index);
                    if (CloseAfterSelection)
                    {
                        Close();
                    }
                }
            }
        }
    }
}
