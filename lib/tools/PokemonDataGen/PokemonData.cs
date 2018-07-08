namespace PokemonDataGen
{
    class PokemonData
    {
        public int id;
        public string name;
        public string gender;
        public int[] baseStats; // 0 => HP, 1 => ATK, 2 => DEF, 3 => SPCDEF, 4 => SPCATK, 5 => SPD
        public string experienceType;
        public int experienceYield;
        public string type1;
        public string type2;
        public string[] eggGroups;
        public int eggCycles;
        public int catchRate;
        public int wildFleeRate; // 0-1, chance of trying to flee in a wild battle
        public MovesetEntryData[] moves;
        public string colors = null;
    }
}
