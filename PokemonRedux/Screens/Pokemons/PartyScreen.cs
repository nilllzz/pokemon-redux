using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game;
using PokemonRedux.Game.Items;
using PokemonRedux.Game.Items.Normal;
using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Mails;
using System;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens.Pokemons
{
    class PartyScreen : Screen
    {
        private const int ANIMATION_DELAY_HEALTHY = 8; // > 0.5 hp
        private const int ANIMATION_DELAY_HURT = 20; // < 0.5 hp
        private const int ANIMATION_DELAY_FAINTED = 100; // < 0.2 hp

        private Screen _preScreen;
        private string _instructionText;
        private PartyMode _mode;

        // held item selection
        private Item _giveItem;
        // attach mail
        private Mail _mail;
        private SpriteBatch _batch;
        private PokemonFontRenderer _fontRenderer;
        private HPBar _hpBar;
        private OptionsBox _options, _itemOptions, _confirmBox;
        private Textbox _textbox;
        private Texture2D _itemIcon, _mailIcon;
        private readonly Pokemon _currentlyBattling; // pokemon currently battling

        private int[] _animationIndices;
        private int[] _animationDelays;
        private bool _isOrdering;
        private int _orderPokemonIndex;

        public event Action<Pokemon> GaveHeldItem;
        public event Action AttachedMail;
        public event Action<int> SelectedPokemon;

        private int Index
        {
            get => Controller.ActivePlayer.MenuStates.PokemonPartyIndex;
            set => Controller.ActivePlayer.MenuStates.PokemonPartyIndex = value;
        }

        public PartyScreen(Screen preScreen)
        {
            Initialize(preScreen, PartyMode.Summary);
        }

        public PartyScreen(Screen preScreen, Pokemon currentlyBattling)
        {
            _currentlyBattling = currentlyBattling;
            Initialize(preScreen, PartyMode.Battle);
        }

        public PartyScreen(Screen preScreen, Item giveItem)
        {
            Initialize(preScreen, PartyMode.HeldItem);
            _giveItem = giveItem;
        }

        public PartyScreen(Screen preScreen, Mail mail)
        {
            Initialize(preScreen, PartyMode.AttachMail);
            _mail = mail;
        }

        private void Initialize(Screen preScreen, PartyMode mode)
        {
            _preScreen = preScreen;
            _mode = mode;

            switch (_mode)
            {
                case PartyMode.Summary:
                case PartyMode.AttachMail:
                case PartyMode.Battle:
                    _instructionText = "Choose a POKéMON.";
                    break;
                case PartyMode.HeldItem:
                    _instructionText = "To which ^PK^MN?";
                    break;
            }

            _animationIndices = new int[Player.MAX_POKEMON];
            // init animation delays
            _animationDelays = new int[Player.MAX_POKEMON];
            for (var i = 0; i < Controller.ActivePlayer.PartyPokemon.Length; i++)
            {
                _animationDelays[i] = GetAnimationDelay(Controller.ActivePlayer.PartyPokemon[i]);
            }
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
            _fontRenderer.LineGap = 0;

            _hpBar = new HPBar();
            _hpBar.LoadContent();

            _itemIcon = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokemon/item.png");
            _mailIcon = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokemon/mail.png");

            _options = new OptionsBox();
            _options.LoadContent();
            _options.OffsetY = (int)(Border.SCREEN_HEIGHT * Border.SCALE * Border.UNIT);
            if (_mode == PartyMode.Battle)
            {
                _options.OffsetX += (int)(11 * Border.SCALE * Border.UNIT);
                _options.BufferUp = 0;
            }
            else
            {
                _options.OffsetX += (int)(6 * Border.SCALE * Border.UNIT);
                _options.BufferUp = 1;
            }

            _itemOptions = new OptionsBox();
            _itemOptions.LoadContent();
            _itemOptions.OffsetY = (int)(Border.SCREEN_HEIGHT * Border.SCALE * Border.UNIT);
            _itemOptions.OffsetX += (int)(12 * Border.SCALE * Border.UNIT);
            _itemOptions.BufferUp = 1;
            _itemOptions.BufferRight = 1;

            _textbox = new Textbox();
            _textbox.LoadContent();
            _textbox.OffsetY = (int)(Border.SCREEN_HEIGHT * Border.SCALE * Border.UNIT - Textbox.HEIGHT * Border.UNIT * Border.SCALE);

            _confirmBox = new OptionsBox();
            _confirmBox.LoadContent();
            _confirmBox.OffsetY = _textbox.OffsetY;
            _confirmBox.OffsetX += (int)(14 * Border.SCALE * Border.UNIT);
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
            var height = 14 * unit;
            var startX = (int)(Controller.ClientRectangle.Width / 2f - width / 2f);

            Border.DrawCenter(_batch, startX, 0, Border.SCREEN_WIDTH, Border.SCREEN_HEIGHT, Border.SCALE);

            var selectionChar = DialogVisible ? "^>>" : ">";

            for (var i = 0; i < Controller.ActivePlayer.PartyPokemon.Length; i++)
            {
                // compose and draw menu text
                var menuText = "";
                if (i == Index)
                {
                    menuText += selectionChar + "  ";
                }
                else if (i == _orderPokemonIndex && _isOrdering)
                {
                    menuText += "^>>  ";
                }
                else
                {
                    menuText += "   ";
                }

                var pokemon = Controller.ActivePlayer.PartyPokemon[i];

                menuText += pokemon.DisplayName.PadRight(10) +
                    pokemon.HP.ToString().PadLeft(3) + "/" +
                    pokemon.MaxHP.ToString().PadLeft(3) + Environment.NewLine;

                var statusText = (pokemon.Status == PokemonStatus.OK ? "" : pokemon.Status.ToString()).PadLeft(3);
                var lvText = "";
                if (pokemon.Level == Pokemon.MAX_LEVEL)
                {
                    lvText = Pokemon.MAX_LEVEL.ToString();
                }
                else
                {
                    lvText = "^:L" + pokemon.Level.ToString();
                }
                menuText += "     " + statusText + lvText;

                _fontRenderer.DrawText(_batch, menuText,
                    new Vector2(startX, unit * (i * 2 + 1)), Color.Black, Border.SCALE);

                // pokemon sprite
                var animationIndex = _animationIndices[i];

                var jumpOffset = 0;
                if (animationIndex == 1 &&
                    ((i == Index && !_isOrdering) ||
                     (i == _orderPokemonIndex && _isOrdering)))
                {
                    switch (PokemonStatHelper.GetPokemonHealth(pokemon.HP, pokemon.MaxHP))
                    {
                        case PokemonHealth.Healthy:
                            jumpOffset = -2;
                            break;
                        case PokemonHealth.Hurt:
                            jumpOffset = -1;
                            break;
                    }
                }

                var offsetX = i == Index || (_isOrdering && i == _orderPokemonIndex) ? unit : 0;

                var menuSprite = pokemon.GetMenuSprite();
                _batch.Draw(menuSprite,
                    new Rectangle(
                        startX + offsetX,
                        (int)(unit * 0.5 + i * unit * 2 + jumpOffset * Border.SCALE),
                        (int)(16 * Border.SCALE),
                        (int)(16 * Border.SCALE)
                    ), new Rectangle(animationIndex * 16, 0, 16, 16), Color.White);

                // item box
                var heldItem = pokemon.HeldItem;
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
                    _batch.Draw(icon,
                        new Rectangle(
                            startX + offsetX,
                            (int)(unit * 1.5 + i * unit * 2),
                            (int)(8 * Border.SCALE),
                            (int)(8 * Border.SCALE)
                        ), Color.White);
                }

                // hp bar
                _hpBar.Draw(_batch, new Vector2(startX + unit * 11, (i + 1) * 2 * unit), pokemon.HP, pokemon.MaxHP, Border.SCALE);
            }

            // cancel option
            var cancelText = Index == Controller.ActivePlayer.PartyPokemon.Length ?
                ">CANCEL" :
                " CANCEL";
            _fontRenderer.DrawText(_batch, cancelText,
                new Vector2(startX, (Controller.ActivePlayer.PartyPokemon.Length * 2 + 1) * unit),
                Color.Black, Border.SCALE);

            // instruction text
            Border.Draw(_batch, startX, height, Border.SCREEN_WIDTH, 4, Border.SCALE);
            if (!DialogVisible)
            {
                var instruction = _isOrdering ? "Move to where?" : _instructionText;
                _fontRenderer.DrawText(_batch, instruction,
                    new Vector2(startX + unit, height + unit * 2), Color.Black, Border.SCALE);
            }

            _textbox.Draw(_batch, Border.DefaultWhite);
            _options.Draw(_batch, Border.DefaultWhite);
            _itemOptions.Draw(_batch, Border.DefaultWhite);
            _confirmBox.Draw(_batch, Border.DefaultWhite);

            _batch.End();
        }

        private static int GetAnimationDelay(Pokemon pokemon)
        {
            switch (PokemonStatHelper.GetPokemonHealth(pokemon.HP, pokemon.MaxHP))
            {
                case PokemonHealth.Healthy:
                    return ANIMATION_DELAY_HEALTHY;
                case PokemonHealth.Hurt:
                    return ANIMATION_DELAY_HURT;
                case PokemonHealth.Fainted:
                    return ANIMATION_DELAY_FAINTED;
            }
            return 0;
        }

        private bool DialogVisible
            => _options.Visible || _itemOptions.Visible || _textbox.Visible || _confirmBox.Visible;

        internal override void Update(GameTime gameTime)
        {
            // update sprite animations
            for (var i = 0; i < Controller.ActivePlayer.PartyPokemon.Length; i++)
            {
                _animationDelays[i]--;
                if (_animationDelays[i] == 0)
                {
                    _animationDelays[i] = GetAnimationDelay(Controller.ActivePlayer.PartyPokemon[i]);
                    _animationIndices[i]++;
                    if (_animationIndices[i] == 2)
                    {
                        _animationIndices[i] = 0;
                    }
                }
            }

            if (!DialogVisible)
            {
                if (GameboyInputs.DownPressed())
                {
                    Index++;
                    if (_isOrdering)
                    {
                        // cancel option not selectable when ordering
                        if (Index == Controller.ActivePlayer.PartyPokemon.Length)
                        {
                            Index = 0;
                        }
                    }
                    else
                    {
                        if (Index > Controller.ActivePlayer.PartyPokemon.Length)
                        {
                            Index = 0;
                        }
                    }
                }
                else if (GameboyInputs.UpPressed())
                {
                    Index--;
                    if (Index == -1)
                    {
                        if (_isOrdering)
                        {
                            Index = Controller.ActivePlayer.PartyPokemon.Length - 1;
                        }
                        else
                        {
                            Index = Controller.ActivePlayer.PartyPokemon.Length;
                        }
                    }
                }

                if (GameboyInputs.APressed())
                {
                    if (Index == Controller.ActivePlayer.PartyPokemon.Length)
                    {
                        Close();
                    }
                    else
                    {
                        SelectPokemon();
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
            else if (_options.Visible)
            {
                _options.Update();
            }
            else if (_itemOptions.Visible)
            {
                _itemOptions.Update();
            }
            else if (_confirmBox.Visible)
            {
                _confirmBox.Update();
            }
            else if (_textbox.Visible)
            {
                _textbox.Update();
            }
        }

        private void SelectPokemon()
        {
            switch (_mode)
            {
                case PartyMode.Summary:
                    if (_isOrdering)
                    {
                        // swap pokemon
                        if (Index != _orderPokemonIndex)
                        {
                            Controller.ActivePlayer.SwapPokemon(Index, _orderPokemonIndex);
                        }
                        _isOrdering = false;
                    }
                    else
                    {
                        var itemOption = "ITEM";
                        var heldItem = Controller.ActivePlayer.PartyPokemon[Index].HeldItem;
                        if (heldItem != null && heldItem.IsMail)
                        {
                            itemOption = "MAIL";
                        }
                        var options = new[] { "STATS", "SWITCH", "MOVE", itemOption, "CANCEL" };
                        _options.BufferRight = 11 - options.Max(o => o.Length);
                        _options.Show(options);
                        _options.OptionSelected += OptionsOptionSelected;
                    }
                    break;
                case PartyMode.HeldItem:
                    GiveHeldItem();
                    break;
                case PartyMode.AttachMail:
                    AttachMail();
                    break;
                case PartyMode.Battle:
                    _options.BufferRight = 0;
                    _options.Show(new[] { "SWITCH", "STATS", "CANCEL" });
                    _options.OptionSelected += BattleOptionSelected;
                    break;
            }
        }

        #region Summary

        private void OptionsOptionSelected(string option, int index)
        {
            _options.OptionSelected -= OptionsOptionSelected;
            switch (option)
            {
                // TODO: field moves
                case "STATS":
                    // open summary screen
                    {
                        var screenManager = GetComponent<ScreenManager>();
                        var summaryScreen = new SummaryScreen(this, Controller.ActivePlayer.PartyPokemon[Index], GetNextPokemon, GetPreviousPokemon);
                        summaryScreen.LoadContent();
                        screenManager.SetScreen(summaryScreen);
                    }
                    break;

                case "SWITCH":
                    // only order when more than 1 pokemon available
                    if (Controller.ActivePlayer.PartyPokemon.Length > 1)
                    {
                        _isOrdering = true;
                        _orderPokemonIndex = Index;
                    }
                    break;

                case "MOVE":
                    // open moves screen
                    {
                        var screenManager = GetComponent<ScreenManager>();
                        var movesScreen = new MovesScreen(this, Index);
                        movesScreen.LoadContent();
                        movesScreen.ChangedPartyIndex += PartySelectionChanged;
                        screenManager.SetScreen(movesScreen);
                    }
                    break;

                case "ITEM":
                    _itemOptions.Show(new[] { "GIVE", "TAKE" }, 2);
                    _itemOptions.OptionSelected += ItemOptionSelected;
                    break;

                case "MAIL":
                    _itemOptions.Show(new[] { "READ", "TAKE" }, 2);
                    _itemOptions.OptionSelected += MailOptionSelected;
                    break;
            }
        }

        // used for the summary screen, returns null when out of bounds
        private Pokemon GetNextPokemon()
        {
            if (Index < Controller.ActivePlayer.PartyPokemon.Length - 1)
            {
                Index++;
                return Controller.ActivePlayer.PartyPokemon[Index];
            }
            return null;
        }
        private Pokemon GetPreviousPokemon()
        {
            if (Index > 0)
            {
                Index--;
                return Controller.ActivePlayer.PartyPokemon[Index];
            }
            return null;
        }

        // when an outside screen changed party selection, reflect the change in this screen
        private void PartySelectionChanged(int index)
        {
            Index = index;
        }

        #endregion

        #region Mail

        private void AttachMail()
        {
            var mailItem = _mail.GetItem();
            var itemData = _mail.GetItemData();
            var pokemon = Controller.ActivePlayer.PartyPokemon[Index];

            pokemon.HeldItem = mailItem;
            pokemon.ItemData = itemData;

            Controller.ActivePlayer.RemoveMailFromPC(_mail);

            _textbox.Show("The MAIL was moved\nfrom the MAILBOX.");
            _textbox.Closed += AttachMailFinished;
        }

        private void AttachMailFinished()
        {
            _textbox.Closed -= AttachMailFinished;
            AttachedMail?.Invoke();
            Close();
        }

        private void MailOptionSelected(string option, int index)
        {
            _itemOptions.OptionSelected -= MailOptionSelected;
            switch (index)
            {
                case 0: // read
                    var viewScreen = new MailViewScreen(this, Controller.ActivePlayer.PartyPokemon[Index]);
                    viewScreen.LoadContent();
                    GetComponent<ScreenManager>().SetScreen(viewScreen);
                    break;
                case 1: // take
                    _textbox.Show("Send the removed\nMAIL to your PC?");
                    _textbox.Finished += MailTakeQuestionFinished;
                    break;
            }
        }

        private void MailTakeQuestionFinished()
        {
            _textbox.Finished -= MailTakeQuestionFinished;
            _confirmBox.Show(new[] { "YES", "NO" });
            _confirmBox.OptionSelected += MailTakeConfirmOptionSelected;
        }

        private void MailTakeConfirmOptionSelected(string option, int index)
        {
            _confirmBox.OptionSelected -= MailTakeConfirmOptionSelected;
            switch (index)
            {
                case 0: // yes
                    // send to pc
                    _textbox.Show("The MAIL was sent\nto your PC.");
                    SendMailToPC();
                    break;
                case 1: // no
                    // inventory does not store mail data, ask to erase
                    _textbox.Show("The MAIL will lose\nits message. OK?");
                    _textbox.Finished += MailRemoveMessageQuestionFinished;
                    break;
            }
        }

        private void SendMailToPC()
        {
            var pokemon = Controller.ActivePlayer.PartyPokemon[Index];
            var item = pokemon.HeldItem;

            Controller.ActivePlayer.AddMailToPC(item.Name, pokemon.ItemData, pokemon.Id);

            // remove mail and data from pokemon
            pokemon.ItemData = null;
            pokemon.HeldItem = null;
        }

        private void MailRemoveMessageQuestionFinished()
        {
            _textbox.Finished -= MailRemoveMessageQuestionFinished;
            _confirmBox.Show(new[] { "YES", "NO" });
            _confirmBox.OptionSelected += MailRemoveMessageOptionSelected;
        }

        private void MailRemoveMessageOptionSelected(string option, int index)
        {
            _confirmBox.OptionSelected -= MailRemoveMessageOptionSelected;
            if (index == 0) // yes
            {
                var pokemon = Controller.ActivePlayer.PartyPokemon[Index];
                var item = pokemon.HeldItem;
                pokemon.HeldItem = null;
                pokemon.ItemData = null;
                Controller.ActivePlayer.AddItem(item.Name, 1);
                _textbox.Show($"MAIL detached from\n{pokemon.DisplayName}.");
            }
            else
            {
                _textbox.Close();
            }
        }

        #endregion

        #region Held Item

        private void ItemOptionSelected(string option, int index)
        {
            _itemOptions.OptionSelected -= ItemOptionSelected;
            switch (index)
            {
                case 0: // give
                    GiveItem();
                    break;
                case 1: // take
                    TakeItem();
                    break;
            }
        }

        private void GiveItem()
        {
            // open pack screen in held item mode
            var screenManager = GetComponent<ScreenManager>();
            var packScreen = new Pack.PackScreen(this, Controller.ActivePlayer.PartyPokemon[Index]);
            packScreen.LoadContent();
            screenManager.SetScreen(packScreen);
        }

        private void TakeItem()
        {
            var pokemon = Controller.ActivePlayer.PartyPokemon[Index];
            var item = pokemon.HeldItem;
            if (item == null)
            {
                _textbox.Show($"{pokemon.DisplayName} isn^'t\nholding anything.");
            }
            else
            {
                pokemon.HeldItem = null;
                Controller.ActivePlayer.AddItem(item.Name, 1);
                _textbox.Show($"Took {item.Name}\nfrom {pokemon.DisplayName}.");
            }
        }

        // selected pokemon from pack screen to hold an item
        private void GiveHeldItem()
        {
            var pokemon = Controller.ActivePlayer.PartyPokemon[Index];
            if (pokemon.HeldItem == null)
            {
                pokemon.HeldItem = _giveItem;
                Controller.ActivePlayer.RemoveItem(_giveItem.Name, 1);
                _textbox.Show($"Made {pokemon.DisplayName}\nhold {_giveItem.Name}.");
                _textbox.Closed += GiveHeldItemFinished;
            }
            else
            {
                if (pokemon.HeldItem is MailItem)
                {
                    // mails cannot be swapped.
                    _textbox.Show("Please remove the\nMAIL first.");
                    _textbox.Closed += GiveItemHoldingMailFinished;
                }
                else
                {
                    _textbox.Show($"{pokemon.DisplayName} is\nalready holding\n\n{pokemon.HeldItem.Name}.\nSwitch items?");
                    _textbox.Finished += SwapItemsQuestionFinished;
                }
            }
        }

        private void GiveItemHoldingMailFinished()
        {
            _textbox.Finished -= GiveItemHoldingMailFinished;
            Close();
        }

        // show question after asking to swap items
        private void SwapItemsQuestionFinished()
        {
            _textbox.Finished -= SwapItemsQuestionFinished;
            _confirmBox.Show(new[] { "YES", "NO" });
            _confirmBox.OptionSelected += SwapItemsOptionSelected;
        }

        // result of question to swap items
        private void SwapItemsOptionSelected(string option, int index)
        {
            _confirmBox.OptionSelected -= SwapItemsOptionSelected;
            var pokemon = Controller.ActivePlayer.PartyPokemon[Index];
            switch (index)
            {
                case 0: // yes
                    Controller.ActivePlayer.AddItem(pokemon.HeldItem.Name, 1);
                    Controller.ActivePlayer.RemoveItem(_giveItem.Name, 1);
                    _textbox.Show($"Took {pokemon.DisplayName}^'s\n{pokemon.HeldItem.Name} and\n\nmade it hold\n{_giveItem.Name}.");
                    pokemon.HeldItem = _giveItem;
                    _textbox.Closed += GiveHeldItemFinished;
                    break;
                case 1: // no
                    Close();
                    break;
            }
        }

        private void GiveHeldItemFinished()
        {
            _textbox.Closed -= GiveHeldItemFinished;
            GaveHeldItem?.Invoke(Controller.ActivePlayer.PartyPokemon[Index]);
            if (Controller.ActivePlayer.PartyPokemon[Index].HeldItem.IsMail)
            {
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
            Controller.ActivePlayer.PartyPokemon[Index].ItemData = itemData;
        }

        #endregion

        #region Battle

        private void BattleOptionSelected(string option, int index)
        {
            _options.OptionSelected -= BattleOptionSelected;
            switch (index)
            {
                case 0: // SWITCH
                    var selectedPokemon = Controller.ActivePlayer.PartyPokemon[Index];
                    if (selectedPokemon == _currentlyBattling)
                    {
                        _textbox.Show(selectedPokemon.DisplayName + "\nis already out.");
                    }
                    else if (selectedPokemon.CanBattle)
                    {
                        SelectedPokemon?.Invoke(Index);
                        Close();
                    }
                    else
                    {
                        _textbox.Show("There^'s no will to\nbattle!");
                    }
                    break;
                case 1: // STATS
                    // open summary screen
                    {
                        var summaryScreen = new SummaryScreen(this, Controller.ActivePlayer.PartyPokemon[Index], GetNextPokemon, GetPreviousPokemon);
                        summaryScreen.LoadContent();
                        GetComponent<ScreenManager>().SetScreen(summaryScreen);
                    }
                    break;
            }
        }

        #endregion

        private void Close()
        {
            var screenManager = GetComponent<ScreenManager>();
            screenManager.SetScreen(_preScreen);
        }
    }
}
