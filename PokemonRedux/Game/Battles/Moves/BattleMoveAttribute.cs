using System;

namespace PokemonRedux.Game.Battles.Moves
{
    [AttributeUsage(AttributeTargets.Class)]
    class BattleMoveAttribute : Attribute
    {
        public string Name { get; }

        public BattleMoveAttribute(string name)
        {
            Name = name;
        }
    }
}
