using System;

namespace PTCG.Cards
{
    class PokemonMoveAttribute : Attribute
    {
        public string Name { get; }
        public Element[] Energies { get; }

        public PokemonMoveAttribute(string name, params Element[] energies)
        {
            Name = name;
            Energies = energies;
        }
    }
}
