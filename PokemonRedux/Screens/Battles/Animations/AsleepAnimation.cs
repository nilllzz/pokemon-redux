using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using System;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations
{
    class AsleepAnimation : BattleAnimation
    {
        private class Letter
        {
            public int Status;
            public int Texture = 0;
        }

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private Letter[] _letters;

        public AsleepAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/asleep.png");
            _letters = new[] { new Letter { Status = 0 }, new Letter { Status = -40 }, new Letter { Status = -80 } };
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_target.Side);
            var frameSize = (int)(14 * Border.SCALE);
            foreach (var letter in _letters)
            {
                if (letter.Status >= 0 && letter.Status <= 64)
                {
                    var sinOffset = Math.Sin(letter.Status / 8d) * Border.SCALE * -4f;
                    var xOffset = GetPokemonSpriteSize() / 2f;
                    if (_target.Side == PokemonSide.Enemy)
                    {
                        sinOffset *= -1;
                        xOffset *= -1;
                    }

                    var posX = center.X + sinOffset + xOffset - frameSize / 2f;
                    var posY = center.Y - letter.Status / 2.5f * Border.SCALE - frameSize / 2f;

                    var textureIndex = letter.Texture;
                    if (textureIndex == 3)
                    {
                        textureIndex = 1;
                    }

                    batch.Draw(_texture, new Rectangle((int)posX, (int)posY, frameSize, frameSize),
                        new Rectangle(14 * textureIndex, 0, 14, 14), Color.White);
                }
            }
        }

        public override void Update()
        {
            foreach (var letter in _letters)
            {
                letter.Status++;
                if (letter.Status % 10 == 0)
                {
                    letter.Texture++;
                    if (letter.Texture == 4)
                    {
                        letter.Texture = 0;
                    }
                }
            }

            if (_letters.All(l => l.Status > 64))
            {
                IsFinished = true;
            }
        }
    }
}
