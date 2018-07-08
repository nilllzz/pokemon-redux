using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game;
using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Pokemons;
using System;
using System.Linq;
using static Core;
using static System.Environment;

namespace PokemonRedux.Screens.Computer
{
    abstract class StorageSystemScreen : Screen
    {
        private const int VISIBLE_POKEMON = 5;
        private const int MESSAGE_DELAY_LONG = 100;
        private const int MESSAGE_DELAY_SHORT = 50;
        private const string DEFAULT_MESSAGE = "Choose a ^PK^MN.";
        private const string MESSAGE_WHATSUP = "What^'s up?";
        private const string MESSAGE_LAST_PKMN = "It^'s your last ^PK^MN!";
        private const string MESSAGE_NO_ROOM = "There^'s no room!";
        private const string MESSAGE_MOVE = "Move to where?";
        private static readonly Color[] POKEMON_PORTRAIT_PALETTE = new[]
        {
            new Color(0, 0, 0),
            new Color(120, 56, 0),
            new Color(184, 96, 0),
            new Color(248, 120, 0)
        };
        private static readonly Color POKEMON_PORTRAIT_BACKGROUND = new Color(248, 120, 0);

        private readonly Screen _preScreen;
        private Pokemon[] _pokemon;
        private string _title;

        private SpriteBatch _batch;
        private PokemonFontRenderer _fontRenderer;
        private Texture2D _itemIcon, _mailIcon, _selector, _arrow, _moveSelector;
        protected OptionsBox _optionsBox, _confirmationBox;

        private int _index;
        private int _scrollIndex;
        private string _message;
        private int _messageDelay;
        private bool _showPokemon = true; // is disabled after taking an action on a pokemon (deposite, move, withdraw, release)
        protected int _boxIndex;
        // move data
        private bool _isMoving = false; // moving pokemon between boxes & party
        private bool _movingFromParty;
        private int _movePokemonBoxIndex;
        private int _movePokemonIndex;

        private event Action MessageFinished;

        protected virtual bool CanChangeBox => false;
        protected abstract bool IsBoxMode { get; }

        protected StorageSystemScreen(Screen preScreen)
        {
            _preScreen = preScreen;
            _boxIndex = Controller.ActivePlayer.ActiveBoxIndex;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _itemIcon = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokemon/item.png");
            _mailIcon = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokemon/mail.png");
            _selector = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Computer/selector.png");
            _moveSelector = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Computer/moveSelector.png");
            _arrow = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Computer/arrow.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            _optionsBox = new OptionsBox();
            _optionsBox.LoadContent();
            _optionsBox.OffsetY -= (int)(Border.SCALE * Border.UNIT * 3);
            _optionsBox.OffsetX += (int)(Border.SCALE * Border.UNIT * 9);
            _optionsBox.OptionSelected += OptionSelected;
            _optionsBox.CloseAfterSelection = false;

            _confirmationBox = new OptionsBox();
            _confirmationBox.LoadContent();
            _confirmationBox.OffsetY -= (int)(Border.SCALE * Border.UNIT);
            _confirmationBox.OffsetX += (int)(Border.SCALE * Border.UNIT * 14);

            _message = DEFAULT_MESSAGE;
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var (unit, startX, width, height) = Border.GetDefaultScreenValues();

            // fill background
            Border.DrawCenter(_batch, startX, 0, Border.SCREEN_WIDTH, Border.SCREEN_HEIGHT, Border.SCALE);

            // pokemon list box
            Border.Draw(_batch, startX + unit * 8, 0, 12, 14, Border.SCALE);

            // title box
            Border.Draw(_batch, startX + unit * 8, 0, 12, 3, Border.SCALE);
            _fontRenderer.DrawText(_batch, _title,
                new Vector2(startX + unit * 10, unit), Color.Black, Border.SCALE);

            if (CanChangeBox)
            {
                _batch.Draw(_arrow, new Rectangle(
                    startX + unit * 8, unit,
                    (int)(_arrow.Width * Border.SCALE),
                    (int)(_arrow.Height * Border.SCALE)),
                    null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
                _batch.Draw(_arrow, new Rectangle(
                    startX + unit * 19, unit,
                    (int)(_arrow.Width * Border.SCALE),
                    (int)(_arrow.Height * Border.SCALE)), Color.White);
            }

            // pokemon list
            var visiblePokemon = _pokemon.Skip(_scrollIndex).Take(VISIBLE_POKEMON).Select(p => p.DisplayName).ToList();
            if (visiblePokemon.Count < VISIBLE_POKEMON)
            {
                visiblePokemon.Add("CANCEL");
            }
            var pokemonListText = string.Join(NewLine, visiblePokemon);
            _fontRenderer.DrawText(_batch, pokemonListText,
                new Vector2(startX + unit * 9, unit * 4), Color.Black, Border.SCALE);

            // selector
            if (!DialogVisible)
            {
                if (_isMoving)
                {
                    _batch.Draw(_moveSelector, new Rectangle(
                        (int)(startX + unit * 9),
                        (int)(unit * 3 + unit * 2 * _index),
                        (int)(_moveSelector.Width * Border.SCALE),
                        (int)(_moveSelector.Height * Border.SCALE)), Color.White);
                }
                else
                {
                    _batch.Draw(_selector, new Rectangle(
                        (int)(startX + unit * 8 + 7 * Border.SCALE),
                        (int)(unit * 3 + Border.SCALE + unit * 2 * _index),
                        (int)(_selector.Width * Border.SCALE),
                        (int)(_selector.Height * Border.SCALE)), Color.White);
                }
            }

            // portrait background
            var targetRect = new Rectangle(
                startX + unit, unit * 4,
                (int)(PokemonTextureManager.TEXTURE_SIZE * Border.SCALE),
                (int)(PokemonTextureManager.TEXTURE_SIZE * Border.SCALE));

            if (!_optionsBox.Visible && !_isMoving)
            {
                _batch.DrawRectangle(targetRect, POKEMON_PORTRAIT_BACKGROUND);
            }

            if ((_pokemon.Length > _index + _scrollIndex || _isMoving) && _showPokemon)
            {
                // pokemon portrait
                var selectedPokemon = GetSelectedPokemon();
                Texture2D sprite;

                if (_optionsBox.Visible || _isMoving)
                {
                    sprite = selectedPokemon.GetFrontSprite();
                }
                else
                {
                    sprite = PokemonTextureManager.GetFront(selectedPokemon.Id, POKEMON_PORTRAIT_PALETTE, selectedPokemon.UnownLetter);
                }
                _batch.Draw(sprite, targetRect, Color.White);

                // pokemon info
                var levelStr = selectedPokemon.Level == Pokemon.MAX_LEVEL ?
                    Pokemon.MAX_LEVEL.ToString().PadRight(4) :
                    ("^:L" + selectedPokemon.Level).PadRight(6);
                var infoStr = levelStr + PokemonStatHelper.GetGenderChar(selectedPokemon.Gender) + NewLine +
                    selectedPokemon.Name;
                _fontRenderer.DrawText(_batch, infoStr,
                    new Vector2(startX + unit, unit * 12), Color.Black, Border.SCALE);

                // draw item box if the pokemon holds an item
                var heldItem = selectedPokemon.HeldItem;
                if (heldItem != null)
                {
                    Texture2D icon;
                    if (heldItem.IsMail)
                    {
                        icon = _mailIcon;
                    }
                    else
                    {
                        icon = _itemIcon;
                    }
                    _batch.Draw(icon, new Rectangle(
                        startX + unit * 7, unit * 12,
                        (int)(Border.SCALE * _itemIcon.Width),
                        (int)(Border.SCALE * _itemIcon.Height)), Color.White);
                }
            }

            // text box
            Border.Draw(_batch, startX, unit * 15, Border.SCREEN_WIDTH, 3, Border.SCALE);
            _fontRenderer.DrawText(_batch, _message,
                new Vector2(startX + unit, unit * 16), Color.Black, Border.SCALE);

            _optionsBox.Draw(_batch, Border.DefaultWhite);
            _confirmationBox.Draw(_batch, Border.DefaultWhite);

            _batch.End();
        }

        private bool DialogVisible =>
            _optionsBox.Visible || _confirmationBox.Visible;

        internal override void Update(GameTime gameTime)
        {
            // wait for message to disappear
            if (_messageDelay > 0)
            {
                _messageDelay--;
                if (_messageDelay == 0)
                {
                    MessageFinished?.Invoke();
                }
                return;
            }

            if (!DialogVisible)
            {
                if (GameboyInputs.DownPressed() && _index + _scrollIndex < _pokemon.Length)
                {
                    _index++;
                    if (_index == VISIBLE_POKEMON)
                    {
                        _index--;
                        _scrollIndex++;
                        if (_scrollIndex + _index == _pokemon.Length + 1) // +1 because 'cancel'
                        {
                            _scrollIndex--;
                        }
                    }
                }
                else if (GameboyInputs.UpPressed() && _index + _scrollIndex > 0)
                {
                    _index--;
                    if (_index == -1)
                    {
                        _index++;
                        _scrollIndex--;
                        if (_scrollIndex == -1)
                        {
                            _scrollIndex++;
                        }
                    }
                }
                else if (GameboyInputs.LeftPressed())
                {
                    LeftPressed();
                }
                else if (GameboyInputs.RightPressed())
                {
                    RightPressed();
                }

                if (GameboyInputs.APressed())
                {
                    if (_isMoving)
                    {
                        ConfirmMovePokemon();
                    }
                    else
                    {
                        if (_pokemon.Length > _index + _scrollIndex)
                        {
                            _message = MESSAGE_WHATSUP;
                            OpenOptionsMenu();
                        }
                        else
                        {
                            // cancel option
                            Close();
                        }
                    }
                }
                else if (GameboyInputs.BPressed())
                {
                    if (_isMoving)
                    {
                        CancelMovePokemon();
                    }
                    else
                    {
                        Close();
                    }
                }
            }
            else if (_confirmationBox.Visible)
            {
                _confirmationBox.Update();
            }
            else if (_optionsBox.Visible)
            {
                _optionsBox.Update();
            }
        }

        protected abstract void OpenOptionsMenu();
        protected abstract void UpdatePokemonList();
        protected virtual void LeftPressed() { }
        protected virtual void RightPressed() { }

        private void OptionSelected(string option, int index)
        {
            _message = DEFAULT_MESSAGE;
            switch (option)
            {
                case "WITHDRAW":
                    WithdrawPokemon();
                    break;
                case "DEPOSIT":
                    DepositPokemon();
                    break;
                case "MOVE":
                    StartMovePokemon();
                    break;
                case "STATS":
                    ShowStats();
                    break;
                case "RELEASE":
                    ReleasePokemon();
                    break;
                case "CANCEL":
                    _optionsBox.Close();
                    break;
            }
        }

        #region withdraw

        private void WithdrawPokemon()
        {
            if (Controller.ActivePlayer.PartyPokemon.Length < Player.MAX_POKEMON)
            {
                var pokemon = _pokemon[_index + _scrollIndex];
                var box = Controller.ActivePlayer.Boxes[_boxIndex];
                box.Withdraw(_index + _scrollIndex);
                UpdatePokemonList();
                _showPokemon = false;
                ShowMessage("Got " + pokemon.DisplayName + "!", WithdrawFinished, MESSAGE_DELAY_SHORT);
            }
            else
            {
                ShowMessage("The party^'s full!", WithdrawFailed);
            }
        }

        private void WithdrawFinished()
        {
            MessageFinished -= WithdrawFinished;
            _optionsBox.Close();
            _showPokemon = true;
            _message = DEFAULT_MESSAGE;
        }

        private void WithdrawFailed()
        {
            MessageFinished -= WithdrawFailed;
            _message = MESSAGE_WHATSUP;
            OpenOptionsMenu();
        }

        #endregion

        #region deposit

        private void DepositPokemon()
        {
            var box = Controller.ActivePlayer.Boxes[Controller.ActivePlayer.ActiveBoxIndex];
            var (canDeposit, message) = box.CanDeposit();
            if (canDeposit)
            {
                var pokemon = _pokemon[_index + _scrollIndex];
                box.Deposit(_index + _scrollIndex);
                UpdatePokemonList();
                _showPokemon = false;
                ShowMessage("Stored " + pokemon.DisplayName + "!", DeposingFinished, MESSAGE_DELAY_SHORT);
            }
            else
            {
                ShowMessage(message, DeposingFailed);
            }
        }

        private void DeposingFinished()
        {
            MessageFinished -= DeposingFinished;
            _optionsBox.Close();
            _showPokemon = true;
            _message = DEFAULT_MESSAGE;
        }

        private void DeposingFailed()
        {
            MessageFinished -= DeposingFailed;
            _message = MESSAGE_WHATSUP;
            OpenOptionsMenu();
        }

        #endregion

        #region move

        private void StartMovePokemon()
        {
            // last pokemon in party cannot be moved
            if (!IsBoxMode && Controller.ActivePlayer.PartyPokemon.Length == 1)
            {
                ShowMessage(MESSAGE_LAST_PKMN, MovingFailedLastPokemon);
            }
            else
            {
                _isMoving = true;
                _movingFromParty = !IsBoxMode;
                if (IsBoxMode)
                {
                    _movePokemonBoxIndex = _boxIndex;
                }
                _movePokemonIndex = _index + _scrollIndex;
                _message = MESSAGE_MOVE;
                _optionsBox.Close();
            }
        }

        private void MovingFailedLastPokemon()
        {
            MessageFinished -= MovingFailedLastPokemon;
            _message = MESSAGE_WHATSUP;
            OpenOptionsMenu();
        }

        private void ConfirmMovePokemon()
        {
            // check if space is available
            if (IsBoxMode && _movingFromParty)
            {
                var box = Controller.ActivePlayer.Boxes[_boxIndex];
                if (box.PokemonCount == StorageBox.POKEMON_PER_BOX)
                {
                    ShowMessage(MESSAGE_NO_ROOM, MovingFailedNoRoom, MESSAGE_DELAY_SHORT);
                    return;
                }
            }
            else if (!IsBoxMode && !_movingFromParty)
            {
                if (Controller.ActivePlayer.PartyPokemon.Length == Player.MAX_POKEMON)
                {
                    ShowMessage(MESSAGE_NO_ROOM, MovingFailedNoRoom, MESSAGE_DELAY_SHORT);
                    return;
                }
            }

            // space IS available, commence moving
            var insertIndex = _index + _scrollIndex;
            var hasMoved = true;
            if (_movingFromParty && IsBoxMode) // deposit into box from party
            {
                var box = Controller.ActivePlayer.Boxes[_boxIndex];
                box.Deposit(_movePokemonIndex);
                // move pokemon within box to selected position
                box.Move(box.PokemonCount - 1, insertIndex);
            }
            else if (!_movingFromParty && !IsBoxMode) // withdraw from box into party
            {
                var box = Controller.ActivePlayer.Boxes[_movePokemonBoxIndex];
                box.Withdraw(_movePokemonIndex);
                // move pokemon within party to selected position
                Controller.ActivePlayer.MovePokemon(Controller.ActivePlayer.PartyPokemon.Length - 1, insertIndex);
            }
            else if (_movingFromParty && !IsBoxMode) // rearrange within party
            {
                // only move if the indices are not the same
                if (_movePokemonIndex != insertIndex)
                {
                    Controller.ActivePlayer.MovePokemon(_movePokemonIndex, insertIndex);
                }
                else
                {
                    hasMoved = false;
                }
            }
            else if (!_movingFromParty && IsBoxMode && _movePokemonBoxIndex != _boxIndex) // move to a different box
            {
                var fromBox = Controller.ActivePlayer.Boxes[_movePokemonBoxIndex];
                var pokemon = fromBox.GetPokemonAt(_movePokemonIndex);
                fromBox.Release(_movePokemonIndex);
                var toBox = Controller.ActivePlayer.Boxes[_boxIndex];
                toBox.Add(pokemon, insertIndex);
            }
            else if (!_movingFromParty && IsBoxMode && _movePokemonBoxIndex == _boxIndex) // rearrange within box
            {
                // only move if the indices are not the same
                if (_movePokemonIndex != insertIndex)
                {
                    var box = Controller.ActivePlayer.Boxes[_boxIndex];
                    box.Move(_movePokemonIndex, insertIndex);
                }
                else
                {
                    hasMoved = false;
                }
            }

            if (hasMoved)
            {
                // retain selection
                var tempIndex = _index;
                var tempScrollIndex = _scrollIndex;
                UpdatePokemonList();
                _scrollIndex = tempScrollIndex;
                _index = tempIndex;
            }
            _isMoving = false;
            _message = DEFAULT_MESSAGE;
        }

        private void MovingFailedNoRoom()
        {
            MessageFinished -= MovingFailedNoRoom;
            _message = MESSAGE_MOVE;
        }

        private void CancelMovePokemon()
        {
            _isMoving = false;
            _message = DEFAULT_MESSAGE;
        }

        #endregion

        #region summary

        private void ShowStats()
        {
            var summaryScreen = new SummaryScreen(this, _pokemon[_index + _scrollIndex], GetNextPokemon, GetPreviousPokemon);
            summaryScreen.LoadContent();
            GetComponent<ScreenManager>().SetScreen(summaryScreen);
        }

        private Pokemon GetNextPokemon()
        {
            if (_index + _scrollIndex == _pokemon.Length - 1)
            {
                return null;
            }
            else
            {
                _index++;
                if (_index == VISIBLE_POKEMON)
                {
                    _index--;
                    _scrollIndex++;
                }
                return _pokemon[_index + _scrollIndex];
            }
        }

        private Pokemon GetPreviousPokemon()
        {
            if (_index + _scrollIndex == 0)
            {
                return null;
            }
            else
            {
                _index--;
                if (_index == -1)
                {
                    _index++;
                    _scrollIndex--;
                }
                return _pokemon[_index + _scrollIndex];
            }
        }

        #endregion

        #region release

        private void ReleasePokemon()
        {
            if (Controller.ActivePlayer.PartyPokemon.Length > 1)
            {
                _message = "Release ^PK^MN?";
                _confirmationBox.Show(new[] { "YES", "NO" });
                _confirmationBox.OptionSelected += ReleaseOptionSelected;
            }
            else
            {
                ShowMessage(MESSAGE_LAST_PKMN, ReleaseByeMessageFinished);
            }
        }

        private void ReleaseOptionSelected(string option, int index)
        {
            _confirmationBox.OptionSelected -= ReleaseOptionSelected;
            if (index == 0) // yes
            {
                _showPokemon = false;
                ShowMessage("Released ^PK^MN.", ReleaseFinished);
            }
            else // no
            {
                _message = MESSAGE_WHATSUP;
            }
        }

        private void ReleaseFinished()
        {
            MessageFinished -= ReleaseFinished;
            var pokemon = _pokemon[_index + _scrollIndex];
            if (IsBoxMode)
            {
                var box = Controller.ActivePlayer.Boxes[_boxIndex];
                box.Release(_index + _scrollIndex);
            }
            else
            {
                Controller.ActivePlayer.RemoveFromParty(_index + _scrollIndex);
            }
            UpdatePokemonList();
            ShowMessage("Bye " + pokemon.Name + "!", ReleaseByeMessageFinished);
        }

        // after the "Bye Pokemon" message closed
        private void ReleaseByeMessageFinished()
        {
            MessageFinished -= ReleaseByeMessageFinished;
            _optionsBox.Close();
            _showPokemon = true;
            _message = DEFAULT_MESSAGE;
        }

        #endregion

        private void ShowMessage(string message, Action finishedAction, int delay = MESSAGE_DELAY_LONG)
        {
            _message = message;
            _messageDelay = delay;
            if (finishedAction != null)
            {
                MessageFinished += finishedAction;
            }
        }

        private Pokemon GetSelectedPokemon()
        {
            if (_isMoving)
            {
                if (_movingFromParty)
                {
                    return Controller.ActivePlayer.PartyPokemon[_movePokemonIndex];
                }
                else
                {
                    return Controller.ActivePlayer.Boxes[_movePokemonBoxIndex].GetPokemonAt(_movePokemonIndex);
                }
            }
            else
            {
                return _pokemon[_index + _scrollIndex];
            }

        }

        protected void SetPokemonList(Pokemon[] pokemon, string title)
        {
            _pokemon = pokemon;
            _title = title;
            _index = 0;
            _scrollIndex = 0;
        }

        private void Close()
        {
            GetComponent<ScreenManager>().SetScreen(_preScreen);
        }
    }
}
