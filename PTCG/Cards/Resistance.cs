namespace PTCG.Cards
{
    class Resistance
    {
        public Element To { get; }
        public int Value { get; }

        // old resistance
        public Resistance(Element to) : this(to, 30) { }

        public Resistance(Element to, int value)
        {
            To = to;
            Value = value;
        }
    }
}
