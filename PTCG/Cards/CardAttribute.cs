using System;

namespace PTCG.Cards
{
    class CardAttribute : Attribute
    {
        public string Name { get; }
        public int Num { get; }
        public Expansion Expansion { get; }
        public CardRarity Rarity { get; }

        public CardAttribute(string name, Expansion expansion, int num, CardRarity rarity)
        {
            Name = name;
            Expansion = expansion;
            Num = num;
            Rarity = rarity;
        }
    }
}
