using Microsoft.Xna.Framework.Graphics;
using PTCG.Content;
using System.IO;
using System.Reflection;
using static Core;

namespace PTCG.Cards
{
    abstract class Card
    {
        private CardAttribute _attribute;

        private CardAttribute GetAttribute()
        {
            if (_attribute == null) {
                _attribute = GetType().GetCustomAttribute<CardAttribute>();
            }

            return _attribute;
        }

        public string Name => GetAttribute().Name;
        public Expansion Expansion => GetAttribute().Expansion;
        public int Num => GetAttribute().Num;
        public CardType CardType { get; }

        public int HP { get; set; }
        public int MaxHP { get; protected set; }

        public Card(CardType cardType)
        {
            CardType = cardType;
        }

        protected void SetMaxHP(int hp)
        {
            MaxHP = hp;
            HP = hp;
        }

        public Texture2D GetTexture()
        {
            var texturePath = Path.Combine("Textures/Cards", Expansion.ToString(), Num.ToString() + ".jpg");
            return Controller.Content.LoadFromImage(texturePath);
        }
    }
}
