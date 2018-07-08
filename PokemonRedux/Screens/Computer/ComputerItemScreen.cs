using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Items;
using PokemonRedux.Game.Items.Machine;
using System.Linq;
using static Core;
using static System.Environment;

namespace PokemonRedux.Screens.Computer
{
    class ComputerItemScreen : Screen
    {
        private const int ITEMS_VISIBLE = 4;

        private readonly Screen _preScreen;
        private readonly ComputerItemMode _mode;

        private SpriteBatch _batch;
        private PokemonFontRenderer _fontRenderer;
        private Texture2D _arrow;
        private AmountSelector _amountSelector;
        private OptionsBox _optionsBox;
        private Textbox _textbox;

        private Item[] _items;
        private int _index;
        private int _scrollIndex;

        public ComputerItemScreen(Screen preScreen, ComputerItemMode mode)
        {
            _preScreen = preScreen;
            _mode = mode;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _arrow = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Computer/arrow2.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            _textbox = new Textbox();
            _textbox.LoadContent();
            _textbox.OffsetY = (int)(Border.SCREEN_HEIGHT * Border.SCALE * Border.UNIT - Textbox.HEIGHT * Border.UNIT * Border.SCALE);

            _optionsBox = new OptionsBox();
            _optionsBox.LoadContent();
            _optionsBox.OffsetY = _textbox.OffsetY;
            _optionsBox.OffsetX += (int)(Border.SCALE * Border.UNIT * 14);

            _amountSelector = new AmountSelector();
            _amountSelector.LoadContent();
            _amountSelector.OffsetY = _textbox.OffsetY - (int)(Border.SCALE * Border.UNIT * AmountSelector.HEIGHT);

            SetItems();
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var (unit, startX, width, height) = Border.GetDefaultScreenValues();

            // item box
            Border.Draw(_batch, startX, 0, Border.SCREEN_WIDTH, 12, Border.SCALE);

            // item list
            var visibleItems = _items
                .Skip(_scrollIndex)
                .Take(ITEMS_VISIBLE)
                .Select(i =>
                {
                    if (i is MachineItem machine)
                    {
                        return machine.MachineItemName;
                    }
                    else
                    {
                        return i.Name;
                    }
                }).ToList();
            if (visibleItems.Count < ITEMS_VISIBLE)
            {
                visibleItems.Add("CANCEL");
            }
            var itemListText = "";
            for (int i = 0; i < visibleItems.Count; i++)
            {
                if (i == _index)
                {
                    if (DialogVisible)
                    {
                        itemListText += "^>>";
                    }
                    else
                    {
                        itemListText += ">";
                    }
                }
                else
                {
                    itemListText += " ";
                }

                var itemIndex = i + _scrollIndex;
                if (_items.Length <= itemIndex ||
                    !_items[itemIndex].IsQuantifyable)
                {
                    itemListText += visibleItems[i] + NewLine;
                }
                else
                {
                    var item = _items[itemIndex];
                    itemListText += visibleItems[i] + NewLine +
                        new string(' ', 10) + "*" + item.Amount.ToString().PadLeft(2);
                }
                itemListText += NewLine;
            }
            _fontRenderer.LineGap = 0;
            _fontRenderer.DrawText(_batch, itemListText,
                new Vector2(startX + unit * 4, unit * 2), Color.Black, Border.SCALE);

            // up/down arrows
            if (_scrollIndex > 0)
            {
                _batch.Draw(_arrow, new Rectangle(startX + unit * 18, unit, unit, unit),
                    null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
            }
            if (_scrollIndex < _items.Length - ITEMS_VISIBLE + 1)
            {
                _batch.Draw(_arrow, new Rectangle(startX + unit * 18, unit * 10, unit, unit), Color.White);
            }

            // item description
            Border.Draw(_batch, startX, unit * 12, Border.SCREEN_WIDTH, 6, Border.SCALE);
            if (_items.Length > _index + _scrollIndex)
            {
                var description = _items[_index + _scrollIndex].Description;
                _fontRenderer.LineGap = 1;
                _fontRenderer.DrawText(_batch, description,
                    new Vector2(startX + unit, unit * 14), Color.Black, Border.SCALE);
            }

            _textbox.Draw(_batch, Border.DefaultWhite);
            _optionsBox.Draw(_batch, Border.DefaultWhite);
            _amountSelector.Draw(_batch, Border.DefaultWhite);

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            if (!DialogVisible)
            {
                if (GameboyInputs.DownPressed() && _index + _scrollIndex < _items.Length)
                {
                    _index++;
                    if (_index == ITEMS_VISIBLE)
                    {
                        _index--;
                        _scrollIndex++;
                    }
                }
                else if (GameboyInputs.UpPressed() && _index + _scrollIndex > 0)
                {
                    _index--;
                    if (_index == -1)
                    {
                        _index++;
                        _scrollIndex--;
                    }
                }

                if (GameboyInputs.APressed())
                {
                    if (_items.Length > _index + _scrollIndex)
                    {
                        SelectItem();
                    }
                    else
                    {
                        Close();
                    }
                }
                else if (GameboyInputs.BPressed())
                {
                    Close();
                }
            }
            else if (_amountSelector.Visible)
            {
                _amountSelector.Update();
            }
            else if (_optionsBox.Visible)
            {
                _optionsBox.Update();
            }
            else if (_textbox.Visible)
            {
                _textbox.Update();
            }
        }

        private void SelectItem()
        {
            switch (_mode)
            {
                case ComputerItemMode.Withdraw:
                    WithdrawItem();
                    break;
                case ComputerItemMode.Toss:
                    TossItem();
                    break;
            }
        }

        #region Withdraw

        private void WithdrawItem()
        {
            var item = _items[_index + _scrollIndex];
            if (item.IsQuantifyable)
            {
                _textbox.Show("How many do you\nwant to withdraw?");
                _textbox.Finished += WithdrawQuestionFinished;
            }
            else
            {
                CommitWithdraw(item.Name, 1);
            }
        }

        private void WithdrawQuestionFinished()
        {
            _textbox.Finished -= WithdrawQuestionFinished;
            var item = _items[_index + _scrollIndex];
            _amountSelector.Show(item.Amount);
            _amountSelector.AmountSelected += WithdrawConfirmed;
            _amountSelector.Dismissed += WithdrawDismissed;
        }

        private void WithdrawConfirmed(int amount)
        {
            _amountSelector.AmountSelected -= WithdrawConfirmed;
            CommitWithdraw(_items[_index + _scrollIndex].Name, amount);
        }

        private void WithdrawDismissed()
        {
            _amountSelector.Dismissed -= WithdrawDismissed;
            _textbox.Close();
        }

        private void CommitWithdraw(string itemName, int amount)
        {
            Controller.ActivePlayer.WithdrawItem(itemName, amount);
            Controller.ActivePlayer.AddItem(itemName, amount);
            _textbox.Show($"Withdrew {amount}\n{itemName}(S).");
            _textbox.Closed += WithdrawFinished;
        }

        private void WithdrawFinished()
        {
            _textbox.Closed -= WithdrawFinished;
            SetItems();
        }

        #endregion

        #region Toss

        private void TossItem()
        {
            var item = _items[_index + _scrollIndex];
            if (item.IsQuantifyable)
            {
                _textbox.Show($"Toss out how many\n{item.Name}(S)?");
                _textbox.Finished += TossAmountQuestionFinished;
            }
            else
            {
                _textbox.Show("That^'s too impor-\ntant to toss out!");
            }
        }

        private void TossAmountQuestionFinished()
        {
            var item = _items[_index + _scrollIndex];
            _textbox.Finished -= TossAmountQuestionFinished;
            _amountSelector.Show(item.Amount);
            _amountSelector.AmountSelected += TossAmountSelected;
            _amountSelector.Dismissed += TossAmountDismissed;
        }

        private int _tempTossAmount;

        private void TossAmountSelected(int amount)
        {
            var item = _items[_index + _scrollIndex];
            _amountSelector.AmountSelected -= TossAmountSelected;
            _tempTossAmount = amount;
            _textbox.Show($"Throw away {amount}\n{item.Name}(S)?");
            _textbox.Finished += TossQuestionFinished;
        }

        private void TossAmountDismissed()
        {
            _amountSelector.Dismissed -= TossAmountDismissed;
            _textbox.Close();
        }

        private void TossQuestionFinished()
        {
            _textbox.Finished -= TossQuestionFinished;
            _optionsBox.Show(new[] { "YES", "NO" });
            _optionsBox.OptionSelected += TossQuestionOptionSelected;
        }

        private void TossQuestionOptionSelected(string option, int index)
        {
            _optionsBox.OptionSelected -= TossQuestionOptionSelected;
            if (index == 0) // yes
            {
                var item = _items[_index + _scrollIndex];
                _textbox.Show($"Discarded\n{item.Name}(S).");
                // only withdrawing removes the item from the pc, but doesn't add it to the player's inventory
                Controller.ActivePlayer.WithdrawItem(item.Name, _tempTossAmount);
                _textbox.Closed += TossFinished;
            }
            else // no
            {
                _textbox.Close();
            }
        }

        private void TossFinished()
        {
            _textbox.Closed -= TossFinished;
            SetItems();
        }

        #endregion

        private void SetItems()
        {
            _items = Controller.ActivePlayer.GetItems(true);
            // if the cursor/scroll is in an invalid position, scroll up
            if (_scrollIndex + ITEMS_VISIBLE > _items.Length + 1) // +1 because cancel
            {
                _scrollIndex--;
            }
        }

        private bool DialogVisible =>
            _textbox.Visible || _optionsBox.Visible || _amountSelector.Visible;

        private void Close()
        {
            GetComponent<ScreenManager>().SetScreen(_preScreen);
        }
    }
}
