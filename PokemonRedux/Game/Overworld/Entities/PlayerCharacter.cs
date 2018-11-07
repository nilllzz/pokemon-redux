using GameDevCommon.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PokemonRedux.Game.Data.Entities;
using static Core;

namespace PokemonRedux.Game.Overworld.Entities
{
    class PlayerCharacter : Character
    {
        private const float SPEED = 0.05f;

        // opening a door
        private bool _enteringDoor = false;
        private Door _door = null;
        private float _doorDistanceToWalk = 0f;

        private bool _warping = false;

        private static EntityData GetEntityData()
        {
            return new EntityData
            {
                textureFile = "player"
            };
        }

        public PlayerCharacter()
            : base(GetEntityData())
        {
            Size = new Vector3(0.5f, 1f, 0.5f);
        }

        public void EnterDoor(Door door)
        {
            if (!_enteringDoor)
            {
                _enteringDoor = true;
                _door = door;
                _doorDistanceToWalk = 1.5f;
                Map.World.ZoomCamera = true;
            }
        }

        public void ClosingDoor()
        {
            _doorDistanceToWalk = 0f;
            _enteringDoor = false;
            // do not keep door instance around
            _door = null;
            // set this so the player cannot be controlled
            _warping = true;
        }

        public override void Update()
        {
            if (_enteringDoor)
            {
                UpdateEnterDoor();
            }
            else
            {
                UpdateInput();
            }

            CreateWorld();
            base.Update();
        }

        private void UpdateEnterDoor()
        {
            if (_door.IsOpen)
            {
                var velocity = SPEED;
                var doorYaw = _door.Rotation.Y; // store because door might get set to null
                if (_doorDistanceToWalk <= SPEED)
                {
                    velocity = _doorDistanceToWalk;
                    _door.Close();
                    ClosingDoor();
                }
                else
                {
                    _doorDistanceToWalk -= SPEED;
                }

                if (velocity != 0f)
                {
                    _walking = true;
                    var movementVector = new Vector3(0, 0, -velocity);
                    var rotation = Matrix.CreateFromYawPitchRoll(doorYaw, 0f, 0f);
                    var adjustedVector = Vector3.Transform(movementVector, rotation);
                    Position += adjustedVector;
                }
            }
            else
            {
                _walking = false;
            }
        }

        public void WarpComplete()
        {
            _warping = false;
        }

        private void UpdateInput()
        {
            var velocity = Vector2.Zero;

            var ctrlDown = false;

            if (Map.World.PlayerCanMove && !_warping)
            {

#if DEBUG
                // walking through walls while holding ctrl
                var kHandler = GetComponent<KeyboardHandler>();
                ctrlDown = kHandler.KeyDown(Keys.LeftControl);
#endif

                if (GameboyInputs.LeftDown())
                {
                    if (Facing != EntityFacing.West)
                    {
                        Facing = EntityFacing.West;
                    }
                    else
                    {
                        velocity.X = -1f;
                    }
                }
                else if (GameboyInputs.RightDown())
                {
                    if (Facing != EntityFacing.East)
                    {
                        Facing = EntityFacing.East;
                    }
                    else
                    {
                        velocity.X = 1f;
                    }
                }
                else if (GameboyInputs.UpDown())
                {
                    if (Facing != EntityFacing.North)
                    {
                        Facing = EntityFacing.North;
                    }
                    else
                    {
                        velocity.Y = -1f;
                    }
                }
                else if (GameboyInputs.DownDown())
                {
                    if (Facing != EntityFacing.South)
                    {
                        Facing = EntityFacing.South;
                    }
                    else
                    {
                        velocity.Y = 1f;
                    }
                }

                // interaction, when player is not moving
                if (velocity == Vector2.Zero && GameboyInputs.APressed())
                {
                    // generate a 0.25 cube that is offset by the player's position to their viewing angle
                    // used to detect collision with entity to interact with
                    var interactPosition = Position;
                    switch (Facing)
                    {
                        case EntityFacing.North:
                            interactPosition += new Vector3(0, 0, -Size.Z);
                            break;
                        case EntityFacing.West:
                            interactPosition += new Vector3(-Size.X, 0, 0);
                            break;
                        case EntityFacing.South:
                            interactPosition += new Vector3(0, 0, Size.Z);
                            break;
                        case EntityFacing.East:
                            interactPosition += new Vector3(Size.X, 0, 0);
                            break;
                    }

                    var interactedEntity = Map.World.GetCollision(CollisionType.Test, interactPosition, new Vector3(0.25f), this);
                    if (interactedEntity != null)
                    {
                        interactedEntity.Interact();
                    }
                }
            }

            _walking = velocity != Vector2.Zero;

            var targetPosition = Position;
            if (velocity.X != 0f)
            {
                var xPosition = Position + new Vector3(velocity.X, 0, 0) * SPEED;
                var collidedEntity = ctrlDown ? null : Map.World.GetCollision(CollisionType.Walk, xPosition, Size, this);
                if (collidedEntity == null)
                {
                    targetPosition.X += velocity.X * SPEED;
                    _walking = true;
                }
                else
                {
                    collidedEntity.Collides(this);
                }
            }
            if (velocity.Y != 0f)
            {
                var zPosition = Position + new Vector3(0, 0, velocity.Y) * SPEED;
                var collidedEntity = ctrlDown ? null : Map.World.GetCollision(CollisionType.Walk, zPosition, Size, this);
                if (collidedEntity == null)
                {
                    targetPosition.Z += velocity.Y * SPEED;
                    _walking = true;
                }
                else
                {
                    collidedEntity.Collides(this);
                }
            }

            if (_walking)
            {
                var floor = Map.World.GetFloor(targetPosition, this);
                if (floor == null)
                {
                    targetPosition.Y = 0f;
                }
                else
                {
                    targetPosition.Y = floor.GetHeightForPosition(new Vector2(targetPosition.X, targetPosition.Z));
                    Map.World.ChangeMap(floor.Map.MapFile);
                }

                // can climb 0.25 up at most
                if (ctrlDown || targetPosition.Y - 0.25f <= Position.Y)
                {
                    Position = targetPosition;
                }
            }
        }
    }
}
