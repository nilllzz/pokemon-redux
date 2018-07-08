using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using System;
using System.Collections.Generic;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class SmokescreenAnimation : BattleAnimation
    {
        private class Cloud
        {
            public float X;
            public int Y;
            public bool Direction = true; // true => right, false => left
            public bool PassedHalf = false;
        }

        private const float BALL_SPEED = 0.025f;
        private const int BALL_SIZE = 32;
        private const int SMOKE_STAGES = 3;
        private const int SMOKE_ANIMATION_DELAY = 5;
        private const int CLOUD_TOTAL_PASSES = 7;
        private const int CLOUD_WIDTH = 32;
        private const int CLOUD_HEIGHT = 8;
        private const int TOTAL_CLOUDS = 8;
        private const float CLOUD_SPEED = 0.07f;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private bool _ballVisible = true;
        private float _ballProgress = 0f;
        private bool _smokeVisible = false;
        private int _smokeStage = 0;
        private int _smokeDelay = SMOKE_ANIMATION_DELAY;
        private List<Cloud> _clouds = new List<Cloud>();
        private int _totalClouds = 0;

        public SmokescreenAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/smokescreen.png");
        }

        public override void Show()
        {
            // hide status of user
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
        }

        public override void Draw(SpriteBatch batch)
        {
            var startPoint = GetCenter(BattlePokemon.ReverseSide(_target.Side));
            var endPoint = GetCenter(_target.Side);
            var halfSpriteSize = GetPokemonSpriteSize() / 2f;
            var ballSize = (int)(BALL_SIZE * Border.SCALE);
            if (_target.Side == PokemonSide.Enemy)
            {
                startPoint += new Vector2(halfSpriteSize - ballSize / 2f, -halfSpriteSize + ballSize / 2f);
                endPoint += new Vector2(-halfSpriteSize + ballSize / 2f, halfSpriteSize - ballSize / 2f);
            }
            else
            {
                startPoint -= new Vector2(GetPokemonSpriteSize() / 2f);
                endPoint += new Vector2(GetPokemonSpriteSize() / 2f);
            }
            if (_smokeVisible)
            {
                var pos = endPoint - new Vector2(ballSize / 2f);
                batch.Draw(_texture, new Rectangle((int)pos.X, (int)pos.Y, ballSize, ballSize),
                    new Rectangle(_smokeStage * BALL_SIZE + BALL_SIZE, 0, BALL_SIZE, BALL_SIZE), Color.White);
            }
            if (_ballVisible)
            {
                var offset = (endPoint - startPoint) * _ballProgress;
                var pos = offset + startPoint - new Vector2(ballSize / 2f);

                // make ball bounce
                var yOffset = Math.Abs(Math.Sin(_ballProgress * MathHelper.TwoPi * 2)) * 12 * Border.SCALE;

                batch.Draw(_texture, new Rectangle((int)pos.X, (int)(pos.Y - yOffset), ballSize, ballSize),
                    new Rectangle(0, 0, BALL_SIZE, BALL_SIZE), Color.White);
            }

            foreach (var cloud in _clouds)
            {
                var cloudWidth = (int)(CLOUD_WIDTH * Border.SCALE);
                var cloudHeight = (int)(CLOUD_HEIGHT * Border.SCALE);

                var cloudDefaultPos = GetCenter(_target.Side);
                cloudDefaultPos.X -= GetPokemonSpriteSize() / 2f;
                cloudDefaultPos.Y += GetPokemonSpriteSize() * 0.25f;

                var pos = cloudDefaultPos;
                pos.X += cloud.X * GetPokemonSpriteSize();
                var y = cloud.Y * 2;
                if (cloud.PassedHalf)
                {
                    y++;
                }
                pos.Y -= (y / (CLOUD_TOTAL_PASSES * 3f)) * (GetPokemonSpriteSize() * 0.75f);
                pos -= new Vector2(cloudWidth, cloudHeight) / 2f;

                var texY = cloud.Direction ? 0 : 1;

                batch.Draw(_texture, new Rectangle((int)pos.X, (int)pos.Y, cloudWidth, cloudHeight),
                    new Rectangle(128, CLOUD_HEIGHT * texY, CLOUD_WIDTH, CLOUD_HEIGHT), Color.White);
            }
        }

        public override void Update()
        {
            if (_ballProgress < 1f)
            {
                _ballProgress += BALL_SPEED;
                if (_ballProgress >= 1f)
                {
                    _ballProgress = 1f;
                    _smokeVisible = true;
                }
            }

            if (_smokeVisible)
            {
                _smokeDelay--;
                if (_smokeDelay == 0)
                {
                    _smokeDelay = SMOKE_ANIMATION_DELAY;
                    _smokeStage++;
                    if (_smokeStage == SMOKE_STAGES)
                    {
                        _smokeVisible = false;
                        _clouds.Add(new Cloud());
                        _totalClouds++;
                    }
                    else if (_smokeStage == 1)
                    {
                        _ballVisible = false;
                    }
                }
            }

            for (int i = 0; i < _clouds.Count; i++)
            {
                var cloud = _clouds[i];
                void spawnCloud()
                {
                    if (i == 0 && _totalClouds < TOTAL_CLOUDS)
                    {
                        _clouds.Add(new Cloud());
                        _totalClouds++;
                    }
                }

                if (cloud.Direction)
                {
                    cloud.X += CLOUD_SPEED;
                    if (cloud.X >= 1f)
                    {
                        cloud.X = 1f;
                        cloud.Direction = false;
                        cloud.Y++;
                        cloud.PassedHalf = false;
                        spawnCloud();
                    }
                    else if (cloud.X >= 0.5f && !cloud.PassedHalf)
                    {
                        cloud.PassedHalf = true;
                        spawnCloud();
                    }
                }
                else
                {
                    cloud.X -= CLOUD_SPEED;
                    if (cloud.X <= 0f)
                    {
                        cloud.X = 0f;
                        cloud.Direction = true;
                        cloud.Y++;
                        cloud.PassedHalf = false;
                        spawnCloud();
                    }
                    else if (cloud.X <= 0.5f && !cloud.PassedHalf)
                    {
                        cloud.PassedHalf = true;
                        spawnCloud();
                    }
                }

                if (cloud.Y == CLOUD_TOTAL_PASSES)
                {
                    _clouds.RemoveAt(i);
                    i--;
                    if (_clouds.Count == 0)
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
