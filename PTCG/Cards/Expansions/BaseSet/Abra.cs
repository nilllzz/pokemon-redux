namespace PTCG.Cards.Expansions.BaseSet
{
    [Card("Abra", Expansion.BaseSet, 65, CardRarity.Common)]
    [PokemonCard(Element.Psychic, 30, 0, Element.Psychic)]
    sealed class Abra : PokemonCard
    {
        [PokemonMove("Psyshock", Element.Psychic)]
        private static void MovePsyshock()
        {

        }
    }
}
