using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    // this animation is used both for the move itself and the end-of-turn effect
    class BindAnimation : BattleMoveAnimation
    {
        private const int TOTAL_STAGES = 3;
        private const float PROGRESS_PER_FRAME = 0.1f;
        private const int CONSTRICT_INITIAL_DELAY = 30;
        private const int CONSTRICT_DELAY = 8;
        private const int CONSTRICT_STAGES = 13;
        private const int FRAME_WIDTH = 56;
        private const int FRAME_HEIGHT = 32;

        private int _stage = 0;
        private float _stageProgress = 0f;
        private int _constrictDelay = CONSTRICT_INITIAL_DELAY;
        private int _constrictStage = 0;

        // don't request the user here cause the animation needs to be used where only the target is available
        public BindAnimation(BattlePokemon target)
            : base(null, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("bind");
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_stageProgress < 1f || _stage < TOTAL_STAGES - 1)
            {
                for (var i = 0; i < _stage + 1; i++)
                {
                    var progress = i < _stage ? 1f : _stageProgress;
                    DrawStage(batch, i, progress);
                }
            }
            else
            {
                var constricted = _constrictStage % 2 == 1;
                var stage = TOTAL_STAGES - 1;
                if (constricted)
                {
                    stage = TOTAL_STAGES;
                }
                DrawStage(batch, stage, 1f);
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
            if (_stage < TOTAL_STAGES && _stageProgress < 1f)
            {
                _stageProgress += PROGRESS_PER_FRAME;
                if (_stageProgress >= 1f)
                {
                    _stage++;
                    if (_stage == TOTAL_STAGES)
                    {
                        _stage = TOTAL_STAGES - 1;
                        _stageProgress = 1f;
                    }
                    else
                    {
                        _stageProgress = 0f;
                    }
                }
            }
            else
            {
                _constrictDelay--;
                if (_constrictDelay == 0)
                {
                    _constrictDelay = CONSTRICT_DELAY;
                    _constrictStage++;
                    if (_constrictStage == CONSTRICT_STAGES)
                    {
                        Finish();
                    }
                }
            }
        }
    }
}
