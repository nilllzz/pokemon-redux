using PokemonRedux.Game.Pokemons;

namespace PokemonRedux.Game.Items.Machine
{
    abstract class MachineItem : Item
    {
        private Move _move;

        public Move GetMove()
        {
            if (_move == null)
            {
                _move = Move.Get(Name);
            }
            return _move;
        }

        public string MachineNumber
        {
            get
            {
                if (IsHM)
                {
                    return "H" + _data.machineNumber;
                }
                else
                {
                    return _data.machineNumber.ToString("D2");
                }
            }
        }
        public string MachineItemName
            => (IsHM ? "H" : "T") + "M" + _data.machineNumber.ToString("D2");
        public bool IsHM => GetMove().IsHM;
        public override string Description => GetMove().Description;
    }
}
