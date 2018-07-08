using PokemonRedux.Game.Data;
using PokemonRedux.Game.Items.Machine;
using PokemonRedux.Game.Items.Normal;
using PokemonRedux.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Core;

namespace PokemonRedux.Game.Items
{
    abstract class Item
    {
        private static Dictionary<string, Item> _items;

        public static Item Get(string name)
        {
            if (_items == null)
            {
                // find all items
                _items = new Dictionary<string, Item>();
                var itemTypes = typeof(Item).Assembly.GetTypes()
                    .Where(t => t.GetCustomAttributes<ItemAttribute>(true).Count() > 0);
                foreach (var itemType in itemTypes)
                {
                    var item = (Item)Activator.CreateInstance(itemType);
                    var itemName = itemType.GetCustomAttribute<ItemAttribute>().Name;
                    item._data = ItemData.Get(itemName);
                    _items.Add(itemName, item);
                }
            }

            return _items[name];
        }

        protected ItemData _data;

        // used for amounts in player's inventory
        // only state within an item, is not persistent.
        // Every time an item amount is requested for a specific item, this gets overwritten
        public int Amount { get; set; }

        public string Name => _data.name;
        public virtual string Description => _data.description;
        public ItemPocket Pocket => DataHelper.ParseEnum<ItemPocket>(_data.pocket);
        public int Cost => _data.cost;

        public bool IsQuantifyable => Pocket != ItemPocket.KeyItems && !(this is MachineItem m && m.IsHM);
        public bool IsMail => this is MailItem;

        public bool CanUseField
        {
            get
            {
                switch (Pocket)
                {
                    case ItemPocket.Items:
                        return _data.canUseField;
                    case ItemPocket.Balls:
                        return false;
                    case ItemPocket.KeyItems:
                    case ItemPocket.TMHM:
                        return true;
                }
                return false;
            }
        }
        public bool CanUseBattle
        {
            get
            {
                switch (Pocket)
                {
                    case ItemPocket.Items:
                        return _data.canUseBattle;
                    case ItemPocket.Balls:
                        return true;
                    case ItemPocket.KeyItems:
                    case ItemPocket.TMHM:
                        return false;
                }
                return false;
            }
        }

        public virtual void UseField()
        {
            ShowMessage($"OAK: {Controller.ActivePlayer.Name}!\nThis isn't the\ntime to use that!");
        }

        public virtual void UseBattle()
        {
            throw new Exception($"This item ({Name}) should not be able to be used in battle.");
        }

        protected void ShowMessage(string message)
        {
            if (GetComponent<ScreenManager>().ActiveScreen is ITextboxScreen tScreen)
            {
                tScreen.ShowTextbox(message, false);
            }
        }
    }
}
