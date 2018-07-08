using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Core;

namespace PokemonRedux.Screens
{
    // dialog that allows to select an amount
    class AmountSelector
    {
        private const int WIDTH = 5;
        private const int WIDTH_COST = 13;
        public const int HEIGHT = 3;

        private int _max;
        private int _current;
        private int _cost;
        private bool _displayCost;

        private PokemonFontRenderer _fontRenderer;

        public bool Visible { get; private set; }
        public int OffsetY { get; set; }

        public event Action<int> AmountSelected;
        public event Action Dismissed;

        public void LoadContent()
        {
            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
        }

        public void Show(int max)
        {
            _max = max;
            _current = 1;
            _displayCost = false;
            Visible = true;
        }

        public void Show(int max, int cost)
        {
            _max = max;
            _current = 1;
            _cost = cost;
            _displayCost = true;
            Visible = true;
        }

        public void Draw(SpriteBatch batch, Color color)
        {
            if (Visible)
            {
                var unit = (int)(Border.SCALE * Border.UNIT);
                var startY = OffsetY;
                if (_displayCost)
                {
                    var startX = (int)(Controller.ClientRectangle.Width / 2f + Border.SCREEN_WIDTH * unit / 2f - WIDTH_COST * unit);
                    Border.Draw(batch, startX, OffsetY, WIDTH_COST, HEIGHT, Border.SCALE, color);
                    var cost = ("$" + (_cost * _current).ToString()).PadLeft(7);
                    _fontRenderer.DrawText(batch, "*" + _current.ToString("D2") + cost,
                        new Vector2(startX + unit, startY + unit), Color.Black, Border.SCALE);
                }
                else
                {
                    var startX = (int)(Controller.ClientRectangle.Width / 2f + Border.SCREEN_WIDTH * unit / 2f - WIDTH * unit);
                    Border.Draw(batch, startX, OffsetY, WIDTH, HEIGHT, Border.SCALE, color);
                    _fontRenderer.DrawText(batch, "*" + _current.ToString("D2"),
                        new Vector2(startX + unit, startY + unit), Color.Black, Border.SCALE);
                }
            }
        }

        public void Update()
        {
            if (Visible)
            {
                if (GameboyInputs.UpPressed())
                {
                    _current++;
                    if (_current > _max)
                    {
                        _current = 1;
                    }
                }
                else if (GameboyInputs.DownPressed())
                {
                    _current--;
                    if (_current == 0)
                    {
                        _current = _max;
                    }
                }
                else if (GameboyInputs.RightPressed())
                {
                    _current += 10;
                    if (_current > _max)
                    {
                        _current = _max;
                    }
                }
                else if (GameboyInputs.LeftPressed())
                {
                    _current -= 10;
                    if (_current <= 0)
                    {
                        _current = 1;
                    }
                }

                if (GameboyInputs.APressed())
                {
                    AmountSelected?.Invoke(_current);
                    Close();
                }
                else if (GameboyInputs.BPressed())
                {
                    Dismissed?.Invoke();
                    Close();
                }
            }
        }

        private void Close()
        {
            Visible = false;
        }
    }
}
