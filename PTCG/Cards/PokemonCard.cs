using System.Reflection;

namespace PTCG.Cards
{
    abstract class PokemonCard : Card
    {
        private PokemonCardAttribute _pokemonAttribute;

        private PokemonCardAttribute GetPokemonAttribute()
        {
            if (_pokemonAttribute == null) {
                _pokemonAttribute = GetType().GetCustomAttribute<PokemonCardAttribute>();
            }

            return _pokemonAttribute;
        }

        public PokemonCard()
            : base(CardType.Pokemon)
        {
            var attr = GetPokemonAttribute();
            SetMaxHP(attr.HP);
        }
    }
}
