using Microsoft.Xna.Framework;
using PokemonRedux.Game.Data.Entities;
using System;

namespace PokemonRedux.Game.Overworld.Entities
{
    internal class Door : Prop
    {
        private const float SWING_SPEED = 0.05f;

        private bool _opening = false;
        private Entity _other = null;
        private Vector3 _swingRotation = Vector3.Zero;
        private bool _entering = true;

        public bool IsOpen { get; private set; } = false;

        public Door(EntityData data)
            : base(data)
        {
            if (data.doorData.hidden)
            {
                IsVisible = false;
            }
        }

        protected override void CreateWorld()
        {
            var swingOffset = Vector3.Zero;
            if (_entering)
            {
                swingOffset = new Vector3(-0.5f, 0, -0.5f);
            }
            else
            {
                swingOffset = new Vector3(0.5f, 0, 0.5f);
            }

            World =
                Matrix.CreateRotationX(Rotation.X) *
                Matrix.CreateRotationY(Rotation.Y) *
                Matrix.CreateRotationZ(Rotation.Z) *
                Matrix.CreateTranslation(swingOffset) *
                Matrix.CreateRotationX(_swingRotation.X) *
                Matrix.CreateRotationY(_swingRotation.Y) *
                Matrix.CreateRotationZ(_swingRotation.Z) *
                Matrix.CreateTranslation(Position - swingOffset);
        }

        public override void Update()
        {
            if (_opening)
            {
                // if the player is opening the door and is behind it, only open it a notch and teleport directly.
                if (!_entering)
                {
                    _swingRotation.Y -= SWING_SPEED;
                    if (_swingRotation.Y <= -MathHelper.PiOver4)
                    {
                        ((PlayerCharacter)_other).ClosingDoor();
                        WarpPlayer();
                    }
                }
                else
                {
                    _swingRotation.Y -= SWING_SPEED;
                    // +0.01 to not overlay with the houseInside
                    if (_swingRotation.Y <= -MathHelper.PiOver2 + 0.01f)
                    {
                        _swingRotation.Y = -MathHelper.PiOver2 + 0.01f;
                        IsOpen = true;
                    }
                }
            }
            else
            {
                if (_swingRotation.Y < 0f)
                {
                    _swingRotation.Y += SWING_SPEED * 1.5f;
                    if (_swingRotation.Y >= 0f)
                    {
                        _swingRotation.Y = 0f;
                        WarpPlayer();
                    }
                }
            }

            CreateWorld();

            base.Update();
        }

        private void WarpPlayer()
        {
            IsOpen = false;

            // warp player according to door data
            if (_other is PlayerCharacter)
            {
                Map.World.WarpTo(_data.doorData.warpData);
            }
        }

        public void Close()
        {
            _opening = false;
        }

        public override void Collides(Entity other)
        {
            if (!_opening && Math.Abs(Position.X - other.Position.X) <= 0.2f)
            {
                _other = other;
                _opening = true;
                if (_other is PlayerCharacter)
                {
                    ((PlayerCharacter)_other).EnterDoor(this);
                }

                // make the door visible if interacted with and it was hidden before
                if (_data.doorData.hidden && !IsVisible)
                {
                    IsVisible = true;
                }

                _entering = _other.Position.Z > Position.Z;
            }
        }
    }
}
