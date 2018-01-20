using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PTCG.Objects;
using PTCG.Cards;

namespace PTCG.Screens.Store
{
    class CardRevealScreen : Screen
    {
        private CardModel[] _cards;

        public CardRevealScreen()
        {
            var cards = CardProvider.GetCards(Expansion.BaseSet, CardRarity.Common);
            _cards = cards.Select(c => new CardModel(c)).ToArray();
        }

        internal override void LoadContent()
        {
            foreach (var card in _cards) {
                card.LoadContent();
            }
        }

        internal override void UnloadContent()
        {

        }

        internal override void Update(GameTime gameTime)
        {

        }

        internal override void Draw(GameTime gameTime)
        {

        }
    }
}
