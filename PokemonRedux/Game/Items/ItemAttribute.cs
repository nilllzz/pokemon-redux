using System;

namespace PokemonRedux.Game.Items
{
    class ItemAttribute : Attribute
    {
        public string Name { get; private set; }

        public ItemAttribute(string name)
        {
            Name = name;
        }
    }
}
