using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using System;
using System.Collections.Generic;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class SandAttackAnimation : BattleAnimation
    {
        private const int TOTAL_SANDS = 8;
        private const double SAND_SPEED = 0.05;
        private const int SAND_WIDTH = 14;
        private const int SAND_HEIGHT = 35;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private List<double> _sands = new List<double>();
        private int _totalSands = 0;
        private double _elapsed = 0;

        public SandAttackAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/sandattack.png");
        }

        public override void Show()
        {
            // hide status of user
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
            _sands.Add(0);
            _totalSands++;
        }

        public override void Draw(SpriteBatch batch)
        {
            var sandWidth = (int)(SAND_WIDTH * Border.SCALE);
            var sandHeight = (int)(SAND_HEIGHT * Border.SCALE);
            var startPoint = GetCenter(BattlePokemon.ReverseSide(_target.Side));
            if (_target.Side == PokemonSide.Enemy)
            {
                startPoint.X += GetPokemonSpriteSize() / 2f - sandWidth;
            }
            else
            {
                startPoint.X -= GetPokemonSpriteSize() / 2f;
            }
            startPoint.Y -= sandHeight / 2f;

            var effect = _target.Side == PokemonSide.Enemy ?
                SpriteEffects.None :
                SpriteEffects.FlipHorizontally;
            foreach (var sand in _sands)
            {
                var endPoint = GetCenter(_target.Side);
                endPoint.Y -= sandHeight / 2f;
                var offset = endPoint - startPoint;

                var pos = new Vector2((float)(offset.X * sand), (float)(offset.Y * sand)) + startPoint;

                var texX = (int)Math.Floor(sand / 0.25) * SAND_WIDTH;

                batch.Draw(_texture, new Rectangle((int)pos.X, (int)pos.Y, sandWidth, sandHeight),
                    new Rectangle(texX, 0, SAND_WIDTH, SAND_HEIGHT), Color.White, 0f, Vector2.Zero, effect, 0f);
            }
        }

        public override void Update()
        {
            for (int i = 0; i < _sands.Count; i++)
            {
                _sands[i] += SAND_SPEED;
                if (_sands[i] >= 1)
                {
                    _sands.RemoveAt(i);
                    i--;
                }
            }
            _elapsed += SAND_SPEED;
            if (_elapsed >= 0.3)
            {
                _elapsed = 0;
                if (_totalSands < TOTAL_SANDS)
                {
                    _totalSands++;
                    _sands.Add(0);
                }
            }
            if (_sands.Count == 0)
            {
                IsFinished = true;
                // show status again
                Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), true);
            }
        }
    }
}
