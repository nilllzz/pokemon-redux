namespace PTCG.Cards
{
    class Weakness
    {
        public Element To { get; }
        public WeaknessType Type { get; }
        public int Value { get; }

        // old weakness
        public Weakness(Element to) : this(to, WeaknessType.Multiply, 2) { }

        public Weakness(Element to, int minusValue) : this(to, WeaknessType.Minus, minusValue) { }

        public Weakness(Element to, WeaknessType type, int value)
        {
            To = to;
            Type = type;
            Value = value;
        }
    }
}
