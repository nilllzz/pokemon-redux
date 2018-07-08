using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Items;
using PokemonRedux.Game.Items.Machine;
using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Mails;
using System;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens.Pack
{
    class PackScreen : Screen, ITextboxScreen
    {
        private const int ITEMS_VISIBLE = 5;
        private const int POCKET_COUNT = 4;
        private static Color ARROW_COLOR = new Color(248, 0, 0);

        private readonly Screen _preScreen;
        private PackMode _mode;

        private Pokemon _targetPokemon; // give held item to
        private SpriteBatch _batch;
        private Texture2D _background, _pockets;
        private PokemonFontRenderer _fontRenderer;
        private Textbox _descriptionTextbox; // displays the item description
        private Textbox _textbox; // used for item text displays
        private AmountSelector _amountSelector; // throwing away items
        private OptionsBox _optionsBox;
        private OptionsBox _confirmationBox;

        private Item[] _items;
        private bool _isOrdering = false; // if items are being ordered (sorted manually)
        private int _orderItemIndex; // the other item that should be switched
        private int _tempTossAmount; // temporarely stores the amount of items to be tossed
        private Item _givenMail; // temporarely holds the give mail item to open the composition screen

        private int PocketIndex
        {
            get => Controller.ActivePlayer.MenuStates.PackPocketIndex;
            set => Controller.ActivePlayer.MenuStates.PackPocketIndex = value;
        }
        private int[] ItemIndices => Controller.ActivePlayer.MenuStates.PackItemIndices;
        private int[] ScrollIndices => Controller.ActivePlayer.MenuStates.PackScrollIndices;

        public PackScreen(Screen preScreen, PackMode mode)
        {
            _preScreen = preScreen;
            _mode = mode; // field or deposit
            SetItems();
        }

        public PackScreen(Screen preScreen, Pokemon targetPokemon)
        {
            _preScreen = preScreen;
            _targetPokemon = targetPokemon;
            _mode = PackMode.HeldItem;
            SetItems();
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _background = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pack/background.png");
            _pockets = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pack/pockets.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
            _fontRenderer.LineGap = 0;

            _descriptionTextbox = new Textbox();
            _descriptionTextbox.LoadContent();
            _descriptionTextbox.OffsetY = (int)(Border.SCREEN_HEIGHT * Border.SCALE * Border.UNIT - Textbox.HEIGHT * Border.UNIT * Border.SCALE);
            ChangedItemSelection();

            _textbox = new Textbox();
            _textbox.LoadContent();
            _textbox.OffsetY = (int)(Border.SCREEN_HEIGHT * Border.SCALE * Border.UNIT - Textbox.HEIGHT * Border.UNIT * Border.SCALE);

            _optionsBox = new OptionsBox();
            _optionsBox.LoadContent();
            _optionsBox.OffsetY = _descriptionTextbox.OffsetY;

            _confirmationBox = new OptionsBox();
            _confirmationBox.LoadContent();
            _confirmationBox.OffsetY = _descriptionTextbox.OffsetY;
            _confirmationBox.OffsetX += (int)(Border.SCALE * Border.UNIT * 14);

            _amountSelector = new AmountSelector();
            _amountSelector.LoadContent();
            _amountSelector.OffsetY = _textbox.OffsetY - (int)(Border.SCALE * Border.UNIT * AmountSelector.HEIGHT);
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
            var height = Border.SCREEN_HEIGHT * unit;
            var startX = (int)(Controller.ClientRectangle.Width / 2f - width / 2f);

            _batch.Draw(_background, new Rectangle(
                startX, 0, width,
                (int)(_background.Height * Border.SCALE)), Color.White);

            // draw pocket
            _batch.Draw(_pockets,
                new Rectangle(startX, unit * 3, unit * 5, unit * 3),
                new Rectangle(PocketIndex * 40, 0, 40, 24),
                Color.White);

            // draw pocket name
            _batch.Draw(_pockets,
                new Rectangle(startX, unit * 7, unit * 5, unit * 3),
                new Rectangle(PocketIndex * 40, 24, 40, 24),
                Color.White);

            // draw item list
            var scrollIndex = ScrollIndices[PocketIndex];
            var itemIndex = ItemIndices[PocketIndex];

            var items = GetVisiblePocketItems();

            var itemListText = "";
            for (int i = 0; i < ITEMS_VISIBLE; i++)
            {
                if (items.Length > i)
                {
                    var item = items[i];
                    var numStr = item.Amount.ToString();
                    if (numStr.Length == 1)
                    {
                        numStr = " " + numStr;
                    }
                    // prepend tm/hm no.
                    if (PocketIndex == (int)ItemPocket.TMHM)
                    {
                        var machineItem = item as MachineItem;
                        itemListText += machineItem.MachineNumber + " " + item.Name + Environment.NewLine;
                        if (item.IsQuantifyable)
                        {
                            itemListText += "*".PadLeft(13) + numStr;
                        }
                        itemListText += Environment.NewLine;
                    }
                    else if (item.IsQuantifyable)
                    {
                        itemListText += item.Name + Environment.NewLine +
                            "*".PadLeft(10) + numStr + Environment.NewLine;
                    }
                    // no amounts for non quantifyable items
                    else
                    {
                        itemListText += item.Name + Environment.NewLine + Environment.NewLine;
                    }
                }
                else if (items.Length == i) // last item is cancel option
                {
                    if (PocketIndex == (int)ItemPocket.TMHM)
                    {
                        itemListText += "   CANCEL";
                    }
                    else
                    {
                        itemListText += "CANCEL";
                    }
                }
            }

            int itemListTextX = PocketIndex == (int)ItemPocket.TMHM ? 5 : 8;
            _fontRenderer.DrawText(_batch, itemListText,
                            new Vector2(startX + unit * itemListTextX, unit * 2), Color.Black, Border.SCALE);

            // draw ordering selector
            if (_isOrdering)
            {
                var drawIndex = _orderItemIndex - ScrollIndices[PocketIndex];
                if (drawIndex >= 0 && drawIndex < ITEMS_VISIBLE)
                {
                    _fontRenderer.DrawText(_batch, "^>>",
                        new Vector2(startX + unit * 7, unit * 2 + drawIndex * unit * 2), ARROW_COLOR, Border.SCALE);
                }
            }

            // draw selector
            var selectorChar = DialogVisible ? "^>>" : ">";
            _fontRenderer.DrawText(_batch, selectorChar,
                            new Vector2(startX + unit * 7, unit * 2 + itemIndex * unit * 2), ARROW_COLOR, Border.SCALE);

            _descriptionTextbox.Draw(_batch, Border.DefaultWhite);
            _textbox.Draw(_batch, Border.DefaultWhite);
            _optionsBox.Draw(_batch, Border.DefaultWhite);
            _confirmationBox.Draw(_batch, Border.DefaultWhite);
            _amountSelector.Draw(_batch, Border.DefaultWhite);

            _batch.End();
        }

        private bool DialogVisible
            => _optionsBox.Visible || _textbox.Visible || _amountSelector.Visible || _confirmationBox.Visible;

        internal override void Update(GameTime gameTime)
        {
            if (!DialogVisible)
            {
                if (GameboyInputs.RightPressed())
                {
                    PocketIndex++;
                    if (PocketIndex == POCKET_COUNT)
                    {
                        PocketIndex = 0;
                    }
                    ChangedItemSelection();
                }
                else if (GameboyInputs.LeftPressed())
                {
                    PocketIndex--;
                    if (PocketIndex == -1)
                    {
                        PocketIndex = POCKET_COUNT - 1;
                    }
                    ChangedItemSelection();
                }
                else if (GameboyInputs.DownPressed())
                {
                    var items = GetPocketItems();
                    if (ItemIndices[PocketIndex] < ITEMS_VISIBLE - 1 &&
                        ItemIndices[PocketIndex] < items.Length)
                    {
                        ItemIndices[PocketIndex]++;
                    }
                    else if (ScrollIndices[PocketIndex] + ITEMS_VISIBLE < items.Length + 1) // +1 because cancel
                    {
                        ScrollIndices[PocketIndex]++;
                    }
                    ChangedItemSelection();
                }
                else if (GameboyInputs.UpPressed())
                {
                    if (ItemIndices[PocketIndex] > 0)
                    {
                        ItemIndices[PocketIndex]--;
                    }
                    else if (ScrollIndices[PocketIndex] > 0)
                    {
                        ScrollIndices[PocketIndex]--;
                    }
                    ChangedItemSelection();
                }

                if (GameboyInputs.APressed())
                {
                    var items = GetVisiblePocketItems();
                    if (items.Length > ItemIndices[PocketIndex])
                    {
                        if (_isOrdering)
                        {
                            ReorderItems();
                        }
                        else
                        {
                            SelectItem();
                        }
                    }
                    else
                    {
                        // cancel option selected
                        Close();
                    }
                }
                else if (GameboyInputs.BPressed())
                {
                    Close();
                }
                else if (GameboyInputs.SelectPressed() && _mode == PackMode.Field)
                {
                    var items = GetVisiblePocketItems();
                    if (_isOrdering)
                    {
                        // ordering
                        if (items.Length > ItemIndices[PocketIndex])
                        {
                            ReorderItems();
                        }
                        else
                        {
                            // cancel option
                            _isOrdering = false;
                            ChangedItemSelection();
                        }
                    }
                    else
                    {
                        if (items.Length > ItemIndices[PocketIndex])
                        {
                            _isOrdering = true;
                            _orderItemIndex = ScrollIndices[PocketIndex] + ItemIndices[PocketIndex];
                            ChangedItemSelection();
                        }
                    }
                }
            }
            else if (_optionsBox.Visible)
            {
                _optionsBox.Update();
            }
            else if (_confirmationBox.Visible)
            {
                _confirmationBox.Update();
            }
            else if (_amountSelector.Visible)
            {
                _amountSelector.Update();
            }
            else if (_textbox.Visible)
            {
                _textbox.Update();
            }
        }

        private void SelectItem()
        {
            var item = GetSelectedItem();
            switch (_mode)
            {
                case PackMode.Field:
                    switch (item.Pocket)
                    {
                        case ItemPocket.Items:
                            if (item.CanUseField)
                            {
                                _optionsBox.Show(new[] { "USE", "GIVE", "TOSS", "QUIT" });
                            }
                            else
                            {
                                _optionsBox.Show(new[] { "GIVE", "TOSS", "QUIT" });
                            }
                            break;
                        case ItemPocket.Balls:
                            _optionsBox.Show(new[] { "GIVE", "TOSS", "QUIT" });
                            break;
                        case ItemPocket.KeyItems: // TODO: Register on select items
                            _optionsBox.Show(new[] { "USE", "QUIT" });
                            break;
                        case ItemPocket.TMHM:
                            var machineItem = item as MachineItem;
                            if (machineItem.IsHM)
                            {
                                _optionsBox.Show(new[] { "USE", "QUIT" });
                            }
                            else
                            {
                                _optionsBox.Show(new[] { "USE", "GIVE", "QUIT" });
                            }
                            break;
                    }

                    _optionsBox.OptionSelected += SelectedItemOptionSelected;
                    break;
                case PackMode.HeldItem:
                    if (item.Pocket == ItemPocket.KeyItems || item.Pocket == ItemPocket.TMHM)
                    {
                        _textbox.Show("This item can^'t be\nheld.");
                    }
                    else
                    {
                        if (_targetPokemon.HeldItem == null)
                        {
                            _textbox.Show($"Made {_targetPokemon.DisplayName}\nhold {item.Name}.");
                            _targetPokemon.HeldItem = item;
                            Controller.ActivePlayer.RemoveItem(item.Name, 1);
                            if (item.IsMail)
                            {
                                _givenMail = item;
                            }
                            _textbox.Closed += GiveHeldItemComplete;
                        }
                        else
                        {
                            _textbox.Show($"{_targetPokemon.DisplayName} is\nalready holding\n\n{_targetPokemon.HeldItem.Name}.\nSwitch items?");
                            _textbox.Finished += SwapItemsQuestionFinished;
                        }
                    }
                    break;
                case PackMode.Battle:
                    // TODO: battle item handling
                    break;
                case PackMode.Deposit:
                    DepositItem();
                    break;
            }
        }

        private void SelectedItemOptionSelected(string option, int index)
        {
            _optionsBox.OptionSelected -= SelectedItemOptionSelected;
            var item = GetSelectedItem();
            switch (option)
            {
                case "USE":
                    item.UseField();
                    break;
                case "GIVE":
                    var screenManager = GetComponent<ScreenManager>();
                    var partyScreen = new Pokemons.PartyScreen(this, item);
                    partyScreen.LoadContent();
                    partyScreen.GaveHeldItem += GaveHeldItem;
                    screenManager.SetScreen(partyScreen);
                    break;
                case "TOSS":
                    _descriptionTextbox.ShowAndSkip("Throw away how\nmany?");
                    _amountSelector.Show(item.Amount);
                    _amountSelector.AmountSelected += TossItemAmountSelected;
                    _amountSelector.Dismissed += TossItemAmountDismissed;
                    break;
            }
        }

        #region Tossing

        private void TossItemAmountDismissed()
        {
            _amountSelector.Dismissed -= TossItemAmountDismissed;
            ChangedItemSelection();
        }

        private void TossItemAmountSelected(int amount)
        {
            _amountSelector.AmountSelected -= TossItemAmountSelected;
            var item = GetSelectedItem();
            _descriptionTextbox.ShowAndSkip($"Throw away {amount}\n{item.Name}(S)?");
            _confirmationBox.Show(new[] { "YES", "NO" });
            _confirmationBox.OptionSelected += ConfirmTossOptionSelected;
            _tempTossAmount = amount;
        }

        private void ConfirmTossOptionSelected(string option, int index)
        {
            _confirmationBox.OptionSelected -= ConfirmTossOptionSelected;
            if (index == 0) // yes
            {
                var item = GetSelectedItem();
                Controller.ActivePlayer.RemoveItem(item.Name, _tempTossAmount);
                _textbox.Show($"Threw away\n{item.Name}(S).");
                _textbox.Closed += TossCompleted;
            }
            else
            {
                ChangedItemSelection();
            }
        }

        private void TossCompleted()
        {
            _textbox.Closed -= TossCompleted;
            SetItems();
            ChangedItemSelection();
        }

        #endregion

        #region HeldItem

        // question to swap items displayed
        private void SwapItemsQuestionFinished()
        {
            _textbox.Finished -= SwapItemsQuestionFinished;
            _confirmationBox.Show(new[] { "YES", "NO" });
            _confirmationBox.OptionSelected += SwapItemsOptionSelected;
        }

        // swap items question answered
        private void SwapItemsOptionSelected(string option, int index)
        {
            _confirmationBox.OptionSelected -= SwapItemsOptionSelected;
            var item = GetSelectedItem();
            switch (index)
            {
                case 0: // yes
                    Controller.ActivePlayer.AddItem(_targetPokemon.HeldItem.Name, 1);
                    Controller.ActivePlayer.RemoveItem(item.Name, 1);
                    _textbox.Show($"Took {_targetPokemon.DisplayName}^'s\n{_targetPokemon.HeldItem.Name} and\n\nmade it hold\n{item.Name}.");
                    _targetPokemon.HeldItem = item;
                    _textbox.Closed += GiveHeldItemComplete;
                    if (item.IsMail)
                    {
                        _givenMail = item;
                    }
                    break;
                case 1: // no
                    Close();
                    break;
            }
        }

        private void GiveHeldItemComplete()
        {
            _textbox.Closed -= GiveHeldItemComplete;
            if (_givenMail != null)
            {
                // start mail compose screen that returns to this screen's pre screen
                var composeScreen = new MailComposeScreen(_preScreen);
                composeScreen.LoadContent();
                composeScreen.MailDataGenerated += MailDataGenerated;
                GetComponent<ScreenManager>().SetScreen(composeScreen);
            }
            else
            {
                Close();
            }
        }

        private void MailDataGenerated(string itemData)
        {
            _targetPokemon.ItemData = itemData;
        }

        // given an item to a pokemon through the party screen
        private void GaveHeldItem(Pokemon pokemon)
        {
            // update items
            SetItems();
            ChangedItemSelection();
        }

        #endregion

        #region Deposit

        private void DepositItem()
        {
            _textbox.Show("How many do you\nwant to deposit?");
            _textbox.Finished += DepositItemQuestionFinished;
        }

        private void DepositItemQuestionFinished()
        {
            _textbox.Finished -= DepositItemQuestionFinished;
            var item = GetSelectedItem();
            _amountSelector.Show(item.Amount);
            _amountSelector.AmountSelected += DepositItemAmountSelected;
            _amountSelector.Dismissed += DepositItemDismissed;
        }

        private void DepositItemAmountSelected(int amount)
        {
            _amountSelector.AmountSelected -= DepositItemAmountSelected;
            var item = GetSelectedItem();
            Controller.ActivePlayer.DepositItem(item.Name, amount);
            Controller.ActivePlayer.RemoveItem(item.Name, amount);
            _textbox.Show($"Deposited {amount}\n{item.Name}(S).");
            _textbox.Closed += DepositItemFinished;
        }

        private void DepositItemFinished()
        {
            _textbox.Closed -= DepositItemFinished;
            SetItems();
        }

        private void DepositItemDismissed()
        {
            _amountSelector.Dismissed -= DepositItemDismissed;
            _textbox.Close();
        }

        #endregion

        private void ReorderItems()
        {
            var item1 = GetSelectedItem();
            var item2 = GetPocketItems()[_orderItemIndex];

            Controller.ActivePlayer.SwapItems(item1.Name, item2.Name);

            _isOrdering = false;
            SetItems();
            ChangedItemSelection();
        }

        private void Close()
        {
            if (_isOrdering)
            {
                _isOrdering = false;
                ChangedItemSelection();
            }
            else
            {
                var screenManager = GetComponent<ScreenManager>();
                screenManager.SetScreen(_preScreen);
            }
        }

        private void ChangedItemSelection()
        {
            if (_isOrdering)
            {
                _descriptionTextbox.ShowAndSkip("Where should this\nbe moved to?");
            }
            else
            {
                var items = GetVisiblePocketItems();
                if (items.Length > ItemIndices[PocketIndex])
                {
                    var selectedItem = items[ItemIndices[PocketIndex]];
                    _descriptionTextbox.ShowAndSkip(selectedItem.Description);
                }
                else
                {
                    _descriptionTextbox.ShowAndSkip("");
                }
            }
        }

        private Item[] GetPocketItems()
            => _items.Where(i => (int)i.Pocket == PocketIndex).ToArray();

        private Item[] GetVisiblePocketItems()
            => _items
            .Where(i => (int)i.Pocket == PocketIndex)
            .Skip(ScrollIndices[PocketIndex]).Take(ITEMS_VISIBLE).ToArray();

        private Item GetSelectedItem()
            => GetVisiblePocketItems()[ItemIndices[PocketIndex]];

        private void SetItems()
        {
            _items = Controller.ActivePlayer.GetItems(false);

            // if the cursor/scroll is in an invalid position, scroll up
            if (ScrollIndices[PocketIndex] + ITEMS_VISIBLE > GetPocketItems().Length + 1) // +1 because cancel
            {
                ScrollIndices[PocketIndex]--;
            }
        }

        public void ShowTextbox(string text, bool skip)
        {
            _textbox.Show(text);
        }
    }
}
