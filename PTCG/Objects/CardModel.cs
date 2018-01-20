using GameDevCommon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PTCG.Cards;
using PTCG.Content;
using System;
using static Core;

namespace PTCG.Objects
{
    class CardModel : BaseObject
    {
        public Card Card { get; }

        public CardModel(Card card)
        {
            Card = card;
        }

        private Texture2D GenerateTexture()
        {
            var back = Controller.Content.LoadFromImage("Textures/Cards/back.png");
            var front = Card.GetTexture();
            var target = new RenderTarget2D(Controller.GraphicsDevice, back.Width + front.Width, Math.Max(back.Height, front.Height));
            var batch = new SpriteBatch(Controller.GraphicsDevice);

            RenderTargetManager.BeginRenderToTarget(target);

            batch.Draw(back, Vector2.Zero, Color.White);
            batch.Draw(front, new Vector2(back.Width, 0), Color.White);

            RenderTargetManager.EndRenderToTarget();

            return target;
        }

        public override void LoadContent()
        {
            Texture = GenerateTexture();

            base.LoadContent();
        }
    }
}
