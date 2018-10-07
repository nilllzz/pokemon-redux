using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;
using System.Collections.Generic;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class SandAttackAnimation : BattleMoveAnimation
    {
        private const int TOTAL_SANDS = 8;
        private const double SAND_SPEED = 0.05;
        private const int SAND_WIDTH = 14;
        private const int SAND_HEIGHT = 35;

        private List<double> _sands = new List<double>();
        private int _totalSands = 0;
        private double _elapsed = 0;

        public SandAttackAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("sandattack");
        }

        public override void Show()
        {
            base.Show();

            _sands.Add(0);
            _totalSands++;
        }

        public override void Draw(SpriteBatch batch)
        {
            var sandWidth = (int)(SAND_WIDTH * Border.SCALE);
            var sandHeight = (int)(SAND_HEIGHT * Border.SCALE);
            var startPoint = GetCenter(_user.Side);
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
            for (var i = 0; i < _sands.Count; i++)
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
                Finish();
            }
        }
    }
}
