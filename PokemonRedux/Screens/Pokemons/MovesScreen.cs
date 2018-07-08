using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using System;
using static Core;

namespace PokemonRedux.Screens.Pokemons
{
    class MovesScreen : Screen
    {
        private const int ANIMATION_DELAY = 8;

        private readonly Screen _preScreen;

        private SpriteBatch _batch;
        private PokemonFontRenderer _fontRenderer;
        private Texture2D _overlay;

        private int _partyIndex;
        private int _moveIndex = 0;
        private int _animationIndex = 0;
        private int _animationDelay = ANIMATION_DELAY;
        private bool _isOrdering;
        private int _orderMoveIndex;

        private Pokemon Pokemon => Controller.ActivePlayer.PartyPokemon[_partyIndex];

        public event Action<int> ChangedPartyIndex;

        public MovesScreen(Screen preScreen, int startIndex)
        {
            _preScreen = preScreen;
            _partyIndex = startIndex;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _overlay = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokemon/moveOverlay.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var unit = (int)(Border.SCALE * Border.UNIT);
            var width = Border.SCREEN_WIDTH * unit;
            var height = 10 * unit;
            var startX = (int)(Controller.ClientRectangle.Width / 2f - width / 2f);

            // ui
            Border.DrawCenter(_batch, startX, 0, Border.SCREEN_WIDTH, 1, Border.SCALE);

            Border.Draw(_batch, startX, unit, Border.SCREEN_WIDTH, 11, Border.SCALE);

            var levelStrWidth = Pokemon.Level == Pokemon.MAX_LEVEL ? 3 : 4;
            Border.DrawCenter(_batch, startX + unit * 2, unit,
                levelStrWidth + Pokemon.DisplayName.Length + Pokemon.Level.ToString().Length,
                1, Border.SCALE);

            Border.Draw(_batch, startX, height + unit, Border.SCREEN_WIDTH, 7, Border.SCALE);

            // TODO: replace with correct border texture
            _batch.Draw(_overlay, new Rectangle(
                startX, height,
                (int)(_overlay.Width * Border.SCALE),
                (int)(_overlay.Height * Border.SCALE)
                ), Border.DefaultWhite);

            // switch arrows
            if (_partyIndex > 0)
            {
                _fontRenderer.DrawText(_batch, "<",
                    new Vector2(startX + unit * 16, 0), Color.Black, Border.SCALE);
            }
            if (_partyIndex < Controller.ActivePlayer.PartyPokemon.Length - 1)
            {
                _fontRenderer.DrawText(_batch, ">",
                    new Vector2(startX + unit * 18, 0), Color.Black, Border.SCALE);
            }

            // pokemon sprite/name/level
            _batch.Draw(Pokemon.GetMenuSprite(), new Rectangle(
                (int)(startX + unit * 2.5),
                (int)(unit * 0.5),
                (int)(16 * Border.SCALE),
                (int)(16 * Border.SCALE)),
                new Rectangle(_animationIndex * 16, 0, 16, 16), Color.White);

            var lvStr = Pokemon.Level.ToString();
            if (Pokemon.Level < Pokemon.MAX_LEVEL)
            {
                lvStr = "^:L" + lvStr;
            }
            _fontRenderer.DrawText(_batch, Pokemon.DisplayName + lvStr,
                new Vector2(startX + unit * 5, unit), Color.Black, Border.SCALE);

            // move list
            var moveListText = "";
            for (int i = 0; i < Pokemon.MAX_MOVES; i++)
            {
                if (i < Pokemon.Moves.Length)
                {
                    // selection arrow
                    if (_moveIndex == i)
                    {
                        moveListText += ">";
                    }
                    else if (_isOrdering && i == _orderMoveIndex)
                    {
                        moveListText += "^>>";
                    }
                    else
                    {
                        moveListText += " ";
                    }

                    var move = Pokemon.Moves[i];
                    moveListText += move.name + Environment.NewLine +
                        new string(' ', 9) +
                        $"^PP^PP {move.pp.ToString().PadLeft(2)}/{move.maxPP.ToString().PadLeft(2)}" +
                        Environment.NewLine;
                }
                else
                {
                    moveListText += " -" + Environment.NewLine +
                        new string(' ', 9) + "--" + Environment.NewLine;
                }
            }

            _fontRenderer.LineGap = 0;
            _fontRenderer.DrawText(_batch, moveListText,
                new Vector2(startX + unit, unit * 3), Color.Black, Border.SCALE);

            // move info
            if (_isOrdering)
            {
                _fontRenderer.DrawText(_batch, "Where?",
                    new Vector2(startX + unit, height + unit * 2), Color.Black, Border.SCALE);
            }
            else
            {
                var selectedMove = Pokemon.Moves[_moveIndex].GetMove();
                var attk = selectedMove.Attack == -1 ? "---" : selectedMove.Attack.ToString().PadLeft(3);
                _fontRenderer.DrawText(_batch,
                    "TYPE/" + Environment.NewLine + " " +
                    selectedMove.Type.ToString().ToUpper().PadRight(9) +
                    "ATTK/" + attk,
                    new Vector2(startX + unit, height + unit), Color.Black, Border.SCALE);

                _fontRenderer.LineGap = 1;
                _fontRenderer.DrawText(_batch, selectedMove.Description,
                    new Vector2(startX + unit, height + unit * 4), Color.Black, Border.SCALE);
            }

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            _animationDelay--;
            if (_animationDelay == 0)
            {
                _animationDelay = ANIMATION_DELAY;
                _animationIndex++;
                if (_animationIndex == 2)
                {
                    _animationIndex = 0;
                }
            }

            if (GameboyInputs.LeftPressed() && !_isOrdering && _partyIndex > 0)
            {
                _partyIndex--;
                _moveIndex = 0;
                ChangedPartyIndex?.Invoke(_partyIndex);
            }
            else if (GameboyInputs.RightPressed() && !_isOrdering && _partyIndex < Controller.ActivePlayer.PartyPokemon.Length - 1)
            {
                _partyIndex++;
                _moveIndex = 0;
                ChangedPartyIndex?.Invoke(_partyIndex);
            }
            else if (GameboyInputs.UpPressed() && _moveIndex > 0)
            {
                _moveIndex--;
            }
            else if (GameboyInputs.DownPressed() && _moveIndex < Pokemon.Moves.Length - 1)
            {
                _moveIndex++;
            }

            if (GameboyInputs.APressed())
            {
                if (_isOrdering)
                {
                    if (_orderMoveIndex != _moveIndex)
                    {
                        Pokemon.SwapMoves(_moveIndex, _orderMoveIndex);
                    }
                    _isOrdering = false;
                }
                else
                {
                    _isOrdering = true;
                    _orderMoveIndex = _moveIndex;
                }
            }
            else if (GameboyInputs.BPressed())
            {
                if (_isOrdering)
                {
                    _isOrdering = false;
                }
                else
                {
                    Close();
                }
            }
        }

        private void Close()
        {
            var screenManager = GetComponent<ScreenManager>();
            screenManager.SetScreen(_preScreen);
        }
    }
}
