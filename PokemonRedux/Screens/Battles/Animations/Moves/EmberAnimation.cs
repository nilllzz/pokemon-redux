using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using System.Collections.Generic;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class EmberAnimation : BattleAnimation
    {
        private class Flame
        {
            public float Progress;
            public int Number;
            public int Stage;
            public int StageDelay = FLAME_DELAY;
        }

        private const int FLAME_DELAY = 4;
        private const float FLAME_ANIMATION_SPEED = 0.03f;
        private const int SMALL_FLAME_SIZE = 8;
        private const int LARGE_FLAME_DELAY = 6;
        private const int LARGE_FLAME_SIZE = 16;
        private const int FLAME_STAGES = 2;
        private const int TOTAL_LARGE_FLAME_STAGES = 4;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private List<Flame> _flames = new List<Flame>();
        private bool _largeFlamesVisible = false;
        private int _largeFlameStage = 0;
        private int _largeFlameDelay = LARGE_FLAME_DELAY;
        private int _totalLargeFlameStages = 0;

        public EmberAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/ember.png");
            _flames.Add(new Flame { Number = 2 });
        }

        public override void Show()
        {
            // hide status of user
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
        }

        public override void Draw(SpriteBatch batch)
        {
            var smallFlameSize = (int)(SMALL_FLAME_SIZE * Border.SCALE);
            var startPoint = GetCenter(BattlePokemon.ReverseSide(_target.Side));
            if (_target.Side == PokemonSide.Enemy)
            {
                startPoint.X += GetPokemonSpriteSize() / 2 - smallFlameSize * 2;
            }
            else
            {
                startPoint.X -= GetPokemonSpriteSize() / 2;
            }

            foreach (var flame in _flames)
            {
                var endPoint = GetCenter(_target.Side);
                endPoint.Y += GetPokemonSpriteSize() / 2 - smallFlameSize;
                endPoint.X += GetPokemonSpriteSize() * 0.25f * (flame.Number - 1) - smallFlameSize / 2;
                var offset = endPoint - startPoint;

                var pos = new Vector2(offset.X * flame.Progress, offset.Y * flame.Progress) + startPoint;
                batch.Draw(_texture, new Rectangle((int)pos.X, (int)pos.Y, smallFlameSize, smallFlameSize),
                    new Rectangle(flame.Stage * SMALL_FLAME_SIZE, 0, SMALL_FLAME_SIZE, SMALL_FLAME_SIZE), Color.White);
            }

            if (_largeFlamesVisible)
            {
                var largeFlameWidth = (int)(LARGE_FLAME_SIZE * Border.SCALE * 2.5);
                var largeFlameHeight = (int)(LARGE_FLAME_SIZE * Border.SCALE);
                var center = GetCenter(_target.Side);
                var x = (int)(center.X - largeFlameWidth / 2f);
                var y = (int)(center.Y + GetPokemonSpriteSize() / 2 - largeFlameHeight);
                batch.Draw(_texture, new Rectangle(x, y, largeFlameWidth, largeFlameHeight),
                    new Rectangle(0, 8 + _largeFlameStage * LARGE_FLAME_SIZE, 40, 16), Color.White);
            }
        }

        public override void Update()
        {
            for (int i = 0; i < _flames.Count; i++)
            {
                var flame = _flames[i];
                if (flame.Progress < 1)
                {
                    flame.Progress += FLAME_ANIMATION_SPEED;
                    if (flame.Progress >= 1f)
                    {
                        flame.Progress = 1f;
                        if (_flames.All(f => f.Progress == 1f))
                        {
                            _largeFlamesVisible = true;
                        }
                    }
                }
                flame.StageDelay--;
                if (flame.StageDelay == 0)
                {
                    flame.Stage++;
                    flame.StageDelay = FLAME_DELAY;
                    if (flame.Stage == FLAME_STAGES)
                    {
                        flame.Stage = 0;
                        if (_flames.Count < 3)
                        {
                            _flames.Add(new Flame { Number = 2 - _flames.Count });
                        }
                    }
                }
            }

            if (_largeFlamesVisible)
            {
                _largeFlameDelay--;
                if (_largeFlameDelay == 0)
                {
                    _largeFlameDelay = LARGE_FLAME_DELAY;
                    _largeFlameStage++;
                    if (_largeFlameStage == FLAME_STAGES)
                    {
                        _largeFlameStage = 0;
                        _totalLargeFlameStages++;
                        if (_totalLargeFlameStages == TOTAL_LARGE_FLAME_STAGES)
                        {
                            IsFinished = true;
                            // show status again
                            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), true);
                        }
                    }
                }
            }
        }
    }
}
