using System;

namespace PTCG.Cards
{
    class PokemonCardAttribute : Attribute
    {
        public int HP { get; }
        public Element Type { get; }
        public string EvolvesFrom { get; }
        public int RetreatCost { get; }
        public Weakness Weakness { get; }
        public Resistance Resistance { get; }

        public PokemonCardAttribute(Element type, int HP, int retreatCost, string evolvesFrom, Element weakness, Element resistance)
        {
            Type = type;
            this.HP = HP;
            RetreatCost = retreatCost;
            EvolvesFrom = evolvesFrom;
            Weakness = new Weakness(weakness);
            Resistance = new Resistance(resistance);
        }

        public PokemonCardAttribute(Element type, int HP, int retreatCost, Element weakness)
        {
            Type = type;
            this.HP = HP;
            RetreatCost = retreatCost;
            EvolvesFrom = "";
            Weakness = new Weakness(weakness);
            Resistance = null;
        }

        public PokemonCardAttribute(Element type, int HP, int retreatCost, Element weakness, Element resistance)
        {
            Type = type;
            this.HP = HP;
            RetreatCost = retreatCost;
            EvolvesFrom = "";
            Weakness = new Weakness(weakness);
            Resistance = new Resistance(resistance);
        }
    }
}
