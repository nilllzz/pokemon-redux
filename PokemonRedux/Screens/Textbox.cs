using GameDevCommon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using System;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens
{
    class Textbox
    {
        public const int HEIGHT = 6;
        private const int ARROW_BLINK_DELAY = 16;
        private const int CHARDELAY_SLOW = 4;
        private const int CHARDELAY_MID = 2;
        private const int CHARDELAY_FAST = 1;

        private PokemonFontRenderer _fontRenderer;
        private Texture2D _continueArrow;
        private RenderTarget2D _textTarget;
        private SpriteBatch _textBatch;

        private string[][] _text;
        private int _textIndex = 0; // which block of text is shown
        private int _lineIndex = 0; // the current (and next) line shown
        private int _charIndex = 0; // how many chars of the current two lines are shown
        private int _charDelay = 0; // delay between chars to show up
        private bool _scrolling = false;
        private float _scrollValue = 0f; // how far up the text has scrolled
        private int _arrowBlinkingDelay = ARROW_BLINK_DELAY;
        private bool _arrowVisible = true;
        private bool _firedFinished = false; // if the finished event has been fired

        public bool Visible { get; private set; }
        public int OffsetY { get; set; }
        public bool AlwaysDisplayContinueArrow { get; set; } // displays continue arrow even for last line

        public event Action Finished; // when the last char was displayed
        public event Action Closed;

        public Textbox()
        {
            // set default yOffset to be bottom of the screen
            OffsetY = (int)(Controller.ClientRectangle.Height - HEIGHT * Border.UNIT * Border.SCALE);
        }

        public void LoadContent()
        {
            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
            _continueArrow = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Borders/continueArrow.png");
            _textTarget = RenderTargetManager.CreateRenderTarget((int)(Border.SCREEN_WIDTH * Border.UNIT * Border.SCALE), (int)(HEIGHT * Border.UNIT * Border.SCALE) - 52);
            _textBatch = new SpriteBatch(Controller.GraphicsDevice);
        }

        public void Draw(SpriteBatch batch, Color color)
        {
            if (Visible)
            {
                var height = (int)(HEIGHT * Border.UNIT * Border.SCALE);
                var width = (int)(Border.SCREEN_WIDTH * Border.UNIT * Border.SCALE);
                var unit = (int)(Border.UNIT * Border.SCALE);
                var startX = Controller.ClientRectangle.Width / 2 - width / 2;
                var startY = OffsetY;

                Border.Draw(batch, startX, startY, Border.SCREEN_WIDTH, HEIGHT, Border.SCALE, color);

                var previousTargets = Controller.GraphicsDevice.GetRenderTargets();
                RenderTargetManager.BeginRenderToTarget(_textTarget);
                Controller.GraphicsDevice.Clear(Color.Transparent);

                _textBatch.Begin(samplerState: SamplerState.PointClamp);

                var lines = _text[_textIndex];

                var line1 = lines[_lineIndex];
                var line1Length = line1.Length;

                line1 = line1.Substring(0, Math.Min(_charIndex, line1Length));

                _fontRenderer.DrawText(_textBatch, line1,
                    new Vector2(unit, unit * 2 - 32 - _scrollValue),
                    Color.Black, Border.SCALE);

                if (line1Length == line1.Length && lines.Length > _lineIndex + 1)
                {
                    var line2 = lines[_lineIndex + 1];
                    line2 = line2.Substring(0, _charIndex - line1Length);

                    _fontRenderer.DrawText(_textBatch, line2,
                        new Vector2(unit, unit * 4 - 32 - _scrollValue),
                        Color.Black, Border.SCALE);
                }

                _textBatch.End();

                Controller.GraphicsDevice.SetRenderTargets(previousTargets);

                batch.Draw(_textTarget, new Vector2(0, 32) + new Vector2(startX, startY), Color.White);

                // draw advance arrow
                // do this when there is another text block/line
                if (_arrowVisible && !_scrolling && CanAdvance() &&
                    (AlwaysDisplayContinueArrow || (lines.Length > _lineIndex + 2 || _text.Length > _textIndex + 1)))
                {
                    batch.Draw(_continueArrow,
                        new Rectangle(startX + width - unit * 2, startY + height - unit, unit, unit),
                        color);
                }
            }
        }

        public void Update()
        {
            if (Visible)
            {
                if (_scrolling)
                {
                    switch (Controller.ActivePlayer.TextSpeed)
                    {
                        case 0:
                            _scrollValue += Border.SCALE * 2;
                            break;
                        case 1:
                            _scrollValue += Border.SCALE * 1.5f;
                            break;
                        case 2:
                            _scrollValue += Border.SCALE;
                            break;
                    }
                    if (_scrollValue >= Border.UNIT * 2 * Border.SCALE)
                    {
                        _scrolling = false;
                        _scrollValue = 0f;
                        _lineIndex++;
                        // reset char index so it only shows the now first line
                        var lines = _text[_textIndex];
                        var line1 = lines[_lineIndex];
                        _charIndex = line1.Length;
                    }
                }
                else
                {
                    var lines = _text[_textIndex];

                    if (_charDelay > 0)
                    {
                        _charDelay--;
                        if (_charDelay == 0)
                        {
                            _charDelay = 0;
                        }
                    }
                    else
                    {
                        if (GameboyInputs.ADown() || GameboyInputs.BDown())
                        {
                            _charDelay = 0;
                        }
                        else
                        {
                            switch (Controller.ActivePlayer.TextSpeed)
                            {
                                case 0:
                                    _charDelay = CHARDELAY_FAST;
                                    break;
                                case 1:
                                    _charDelay = CHARDELAY_MID;
                                    break;
                                case 2:
                                    _charDelay = CHARDELAY_SLOW;
                                    break;
                            }
                        }
                        var line1 = lines[_lineIndex];
                        if (_charIndex < line1.Length)
                        {
                            // skip escape char
                            if (_charIndex + 2 < line1.Length && line1[_charIndex] == PokemonFontRenderer.ESCAPE_CHAR)
                            {
                                _charIndex += 3;
                            }
                            else
                            {
                                _charIndex++;
                            }
                        }
                        else
                        {
                            if (lines.Length > _lineIndex + 1)
                            {
                                var line2 = lines[_lineIndex + 1];
                                var line2Index = _charIndex - line1.Length;
                                if (line2Index < line2.Length)
                                {
                                    // skip escape char
                                    if (line2Index + 2 < line2.Length && line2[line2Index] == PokemonFontRenderer.ESCAPE_CHAR)
                                    {
                                        _charIndex += 3;
                                    }
                                    else
                                    {
                                        _charIndex++;
                                    }
                                }
                            }
                        }
                    }

                    if (CanAdvance())
                    {
                        if (!_firedFinished && IsFinished)
                        {
                            Finished?.Invoke();
                            _firedFinished = true;
                        }

                        if (_arrowBlinkingDelay > 0)
                        {
                            _arrowBlinkingDelay--;
                            if (_arrowBlinkingDelay == 0)
                            {
                                _arrowBlinkingDelay = ARROW_BLINK_DELAY;
                                _arrowVisible = !_arrowVisible;
                            }
                        }

                        if (GameboyInputs.APressed() || GameboyInputs.BPressed())
                        {
                            // scroll or show next text block
                            if (lines.Length > _lineIndex + 2)
                            {
                                _scrolling = true;
                            }
                            else
                            {
                                // close textbox when through all text
                                if (_textIndex == _text.Length - 1)
                                {
                                    Close();
                                }
                                else
                                {
                                    _textIndex++;
                                    _charIndex = 0;
                                    _lineIndex = 0;
                                    _charDelay = 4;
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool IsFinished
            => _textIndex == _text.Length - 1 && _lineIndex >= _text[_textIndex].Length - 2 && CanAdvance();

        public void Close()
        {
            Visible = false;
            Closed?.Invoke();
        }

        private bool CanAdvance()
        {
            var lines = _text[_textIndex];
            var line1 = lines[_lineIndex];
            var length = line1.Length;

            if (lines.Length > _lineIndex + 1)
            {
                var line2 = lines[_lineIndex + 1];
                length += line2.Length;
            }

            return length == _charIndex;
        }

        public void Show(string text)
        {
            var blocks = text.Split(new[] { "\n\n" }, StringSplitOptions.None)
                .Select(line => line.Split('\n')).ToArray();
            Show(blocks);
        }

        public void Show(string[][] text)
        {
            Visible = true;
            _text = text;
            _textIndex = 0;
            _charIndex = 0;
            _lineIndex = 0;
            _scrolling = false;
            _scrollValue = 0f;
            _charDelay = 4;
            _arrowBlinkingDelay = ARROW_BLINK_DELAY;
            _firedFinished = false;
        }

        // displays the text and skips to the last line, displaying all chars immideatly.
        // do not use with more than 2 lines in 1 block
        public void ShowAndSkip(string text)
        {
            Show(text);
            _textIndex = _text.Length - 1; // skip to last block
            _lineIndex = Math.Max(0, _text[_textIndex].Length - 2); // skip to second-last line
            _charIndex = _text[_textIndex].Sum(l => l.Length); // advance to last char in block
        }
    }
}
