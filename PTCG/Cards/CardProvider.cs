using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PTCG.Cards
{
    static class CardProvider
    {
        private static IEnumerable<Type> CardTypes =>
            typeof(CardProvider).Assembly.GetTypes()
            .Where(t => t.GetCustomAttributes<CardAttribute>().Count() > 0);

        private static CardAttribute GetAttr(Type type)
             => type.GetCustomAttribute<CardAttribute>();

        private static Card[] GetCards(IEnumerable<Type> cardTypes)
            => cardTypes.Select(c => (Card)Activator.CreateInstance(c)).ToArray();

        public static Card[] GetCards(Expansion expansion, CardRarity rarity)
        {
            return GetCards(CardTypes.Where(c =>
            {
                var attr = GetAttr(c);
                return attr.Expansion == expansion && attr.Rarity == rarity;
            }));
        }
    }
}
