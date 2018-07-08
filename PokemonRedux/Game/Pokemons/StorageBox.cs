using PokemonRedux.Game.Data;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Pokemons
{
    class StorageBox
    {
        public const int BOX_COUNT = 14;
        public const int POKEMON_PER_BOX = 20;
        public const int MAX_NAME_LENGTH = 8;

        public readonly StorageBoxData _data;

        public StorageBox(StorageBoxData data)
        {
            _data = data;
        }

        public string Name
        {
            get => _data.name;
            set => _data.name = value;
        }

        public int PokemonCount => _data.pokemon.Length;

        public Pokemon[] GetPokemon()
        {
            return _data.pokemon.Select(p => Pokemon.Get(p)).ToArray();
        }

        public Pokemon GetPokemonAt(int index)
        {
            return Pokemon.Get(_data.pokemon[index]);
        }

        // returns if a pokemon can be deposited in this box
        // fails either if box is full or if only 1 pokemon in party
        public (bool success, string reason) CanDeposit()
        {
            if (Controller.ActivePlayer.PartyPokemon.Length == 1)
            {
                return (false, "It^'s your last ^PK^MN!");
            }
            else
            {
                if (_data.pokemon.Length == POKEMON_PER_BOX)
                {
                    return (false, "The BOX is full.");
                }
            }
            return (true, string.Empty);
        }

        // deposits the pokemon at <partyIndex> into this box
        public void Deposit(int partyIndex)
        {
            var pokemon = Controller.ActivePlayer.PartyPokemon[partyIndex];

            var pokemonList = _data.pokemon.ToList();
            pokemonList.Add(pokemon.GetSaveData());
            _data.pokemon = pokemonList.ToArray();

            Controller.ActivePlayer.RemoveFromParty(partyIndex);
        }

        // withdraws from this box at <pokemonIndex> into player's party
        public void Withdraw(int pokemonIndex)
        {
            var pokemonList = _data.pokemon.ToList();
            var pokemon = pokemonList[pokemonIndex];
            pokemonList.RemoveAt(pokemonIndex);
            _data.pokemon = pokemonList.ToArray();
            Controller.ActivePlayer.AddPokemon(Pokemon.Get(pokemon));
        }

        // -1 adds to end, everything else inserts at index
        public void Add(Pokemon pokemon, int insertIndex = -1)
        {
            var pokemonList = _data.pokemon.ToList();
            if (insertIndex == -1)
            {
                pokemonList.Add(pokemon.GetSaveData());
            }
            else
            {
                pokemonList.Insert(insertIndex, pokemon.GetSaveData());
            }
            _data.pokemon = pokemonList.ToArray();
        }

        public void Release(int pokemonIndex)
        {
            var pokemonList = _data.pokemon.ToList();
            pokemonList.RemoveAt(pokemonIndex);
            _data.pokemon = pokemonList.ToArray();
        }

        public void Move(int oldIndex, int newIndex)
        {
            var pokemonList = _data.pokemon.ToList();
            var pokemon = pokemonList[oldIndex];
            pokemonList.RemoveAt(oldIndex);
            // shift index to accomodate for removed item
            if (newIndex > oldIndex)
            {
                newIndex--;
            }
            pokemonList.Insert(newIndex, pokemon);
            _data.pokemon = pokemonList.ToArray();
        }
    }
}
