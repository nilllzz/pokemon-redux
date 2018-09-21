using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class RockSmashAnimation : BattleAnimation
    {
        private class Rock
        {
            public const float GRAVITY = 0.03f;
            private const int TOTAL_FRAMES = 30;

            public bool IsSmall;
            public int Delay;
            public int State = TOTAL_FRAMES;
            public Vector2 Position;
            public Vector2 Velocity;
        }

        private const int HIT_FRAMES = 10;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private int _hitState = HIT_FRAMES;
        private Rock[] _rocks;

        public RockSmashAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/rocksmash.png");

            var center = GetCenter(_target.Side);
            var smallRockSize = 8 * Border.SCALE;
            var bigRockSize = 16 * Border.SCALE;
            var smallPos = new Vector2(center.X - smallRockSize / 2f, center.Y + Border.UNIT * Border.SCALE);
            var bigPos = new Vector2(center.X - bigRockSize / 2f, center.Y + Border.UNIT * Border.SCALE);

            _rocks = new[]
            {
                new Rock { IsSmall = true, Delay = 0, Position = smallPos, Velocity = new Vector2(-1.0f, -1f) },
                new Rock { IsSmall = true, Delay = 0, Position = smallPos, Velocity = new Vector2(-0.7f, -1.8f) },
                new Rock { IsSmall = true, Delay = 0, Position = smallPos, Velocity = new Vector2(0.9f, -1.3f) },
                new Rock { IsSmall = false, Delay = 0, Position = bigPos, Velocity = new Vector2(-0.9f, -1.5f) },
                new Rock { IsSmall = false, Delay = 0, Position = bigPos, Velocity = new Vector2(0.7f, -1.8f) },
                new Rock { IsSmall = false, Delay = 0, Position = bigPos, Velocity = new Vector2(0.9f, -1.1f) },

                new Rock { IsSmall = true, Delay = 15, Position = smallPos, Velocity = new Vector2(-1.0f, -1f) },
                new Rock { IsSmall = true, Delay = 15, Position = smallPos, Velocity = new Vector2(0.9f, -1.3f) },
                new Rock { IsSmall = false, Delay = 15, Position = bigPos, Velocity = new Vector2(0.9f, -1.1f) },
            };
        }

        public override void Show()
        {
            // hide status of user
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_target.Side);

            foreach (var rock in _rocks.Where(r => r.Delay == 0 && r.State > 0))
            {
                var rockSize = (int)(rock.IsSmall ? 8 * Border.SCALE : 16 * Border.SCALE);
                if (rock.IsSmall)
                {
                    batch.Draw(_texture, new Rectangle((int)rock.Position.X, (int)rock.Position.Y, rockSize, rockSize),
                        new Rectangle(16, 0, 8, 8), Color.White);
                }
                else
                {
                    batch.Draw(_texture, new Rectangle((int)rock.Position.X, (int)rock.Position.Y, rockSize, rockSize),
                        new Rectangle(0, 0, 16, 16), Color.White);
                }
            }

            if (_hitState > 0)
            {
                var frameSize = 24 * Border.SCALE;
                var posX = (int)(center.X - frameSize / 2f);
                var posY = (int)center.Y;
                batch.Draw(_texture, new Rectangle(posX, posY, (int)frameSize, (int)frameSize),
                    new Rectangle(0, 16, 24, 24), Color.White);
            }
        }

        public override void Update()
        {
            if (_hitState > 0)
            {
                _hitState--;
            }
            foreach (var rock in _rocks)
            {
                if (rock.Delay > 0)
                {
                    rock.Delay--;
                }
                else
                {
                    if (rock.State > 0)
                    {
                        rock.State--;
                    }
                    rock.Position += rock.Velocity * Border.SCALE;
                    rock.Velocity.Y += Rock.GRAVITY;
                }
            }

            if (_rocks.All(r => r.Delay == 0 && r.State == 0))
            {
                IsFinished = true;
                // show status again
                Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), true);
            }
        }
    }
}
