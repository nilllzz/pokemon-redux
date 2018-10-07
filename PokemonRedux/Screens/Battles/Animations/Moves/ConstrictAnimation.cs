using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class ConstrictAnimation : BattleMoveAnimation
    {
        private const int TOTAL_STAGES = 4;
        private const float PROGRESS_PER_FRAME = 0.1f;
        private const int FRAME_WIDTH = 56;
        private const int FRAME_HEIGHT = 40;
        private const int END_DELAY = 60;

        private int _stage = 0;
        private float _stageProgress = 0f;
        private int _endDelay = 0;

        public ConstrictAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("constrict");
        }

        public override void Draw(SpriteBatch batch)
        {
            for (var i = 0; i < _stage + 1; i++)
            {
                var progress = i < _stage ? 1f : _stageProgress;
                DrawStage(batch, i, progress);
            }
        }

        private void DrawStage(SpriteBatch batch, int stage, float progress)
        {
            var center = GetCenter(_target.Side);
            var totalFrameWidth = FRAME_WIDTH * Border.SCALE;
            var totalFrameHeight = FRAME_HEIGHT * Border.SCALE;

            var textureWidth = (int)Math.Ceiling(FRAME_WIDTH * progress);
            var frameWidth = textureWidth * Border.SCALE;
            var textureRectangle = new Rectangle(stage * FRAME_WIDTH + FRAME_WIDTH - textureWidth, 0, textureWidth, FRAME_HEIGHT);
            var posY = (int)(center.Y - totalFrameHeight / 2f);

            SpriteEffects effect;
            int posX;
            if (_target.Side == PokemonSide.Enemy)
            {
                effect = SpriteEffects.FlipHorizontally;
                posX = (int)(center.X - totalFrameWidth / 2f);
            }
            else
            {
                effect = SpriteEffects.None;
                posX = (int)(center.X - totalFrameWidth / 2f + (totalFrameWidth - frameWidth));
            }

            batch.Draw(_texture, new Rectangle(posX, posY, (int)frameWidth, (int)totalFrameHeight),
                textureRectangle, Color.White, 0f, Vector2.Zero, effect, 0f);
        }

        public override void Update()
        {
            if (_endDelay > 0)
            {
                _endDelay--;
                if (_endDelay == 0)
                {
                    Finish();
                }
            }
            else
            {
                _stageProgress += PROGRESS_PER_FRAME;
                if (_stageProgress >= 1f)
                {
                    _stage++;
                    if (_stage == TOTAL_STAGES)
                    {
                        _stage = TOTAL_STAGES - 1;
                        _stageProgress = 1f;
                        _endDelay = END_DELAY;
                    }
                    else
                    {
                        _stageProgress = 0f;
                    }
                }
            }
        }
    }
}
