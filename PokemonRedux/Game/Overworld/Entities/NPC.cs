using Microsoft.Xna.Framework;
using PokemonRedux.Game.Data.Entities;
using System;

namespace PokemonRedux.Game.Overworld.Entities
{
    class NPC : Character
    {
        private const float SPEED = 0.025f;
        private static readonly Random _random = new Random();

        private float _distanceToWalk = 0f;

        public NPC(EntityData data)
            : base(data)
        { }

        // returns the player's direction towards the npc
        private EntityFacing GetPlayerRelativeDirection()
        {
            var player = Map.World.PlayerEntity;
            var xDiff = player.Position.X - Position.X;
            var zDiff = player.Position.Z - Position.Z;
            if (Math.Abs(xDiff) > Math.Abs(zDiff))
            {
                if (xDiff > 0f)
                {
                    return EntityFacing.East;
                }
                else
                {
                    return EntityFacing.West;
                }
            }
            else
            {
                if (zDiff > 0f)
                {
                    return EntityFacing.South;
                }
                else
                {
                    return EntityFacing.North;
                }
            }
        }

        public override void Interact()
        {
            // set facing
            var playerFacing = GetPlayerRelativeDirection();
            Facing = playerFacing;

            // stop walking
            _distanceToWalk = 0f;
            _walking = false;

            UpdateSprite();

            switch (_data.npc.Type)
            {
                case NPCType.Text:
                    // show a text defined in the text property
                    Map.World.ShowText(_data.npc.GetText());
                    break;
                case NPCType.Script:
                    // start a script
                    Map.World.StartScript(_data.npc.script);
                    break;
            }
        }

        public override void Update()
        {
            if (_walking)
            {
                var velocity = Vector2.Zero;
                switch (Facing)
                {
                    case EntityFacing.North:
                        velocity = new Vector2(0, -1f);
                        break;
                    case EntityFacing.West:
                        velocity = new Vector2(-1f, 0);
                        break;
                    case EntityFacing.South:
                        velocity = new Vector2(0, 1f);
                        break;
                    case EntityFacing.East:
                        velocity = new Vector2(1f, 0);
                        break;
                }
                velocity *= SPEED;
                var targetPosition = Position + new Vector3(velocity.X, 0, velocity.Y);

                var collider = Map.World.GetCollision(CollisionType.Walk, targetPosition, Size, this);
                if (collider == null)
                {
                    Position = targetPosition;
                    _distanceToWalk -= Math.Abs(velocity.X) + Math.Abs(velocity.Y);

                    if (_distanceToWalk <= 0f)
                    {
                        _distanceToWalk = 0f;
                        _walking = false;
                    }
                }
                else
                {
                    _distanceToWalk = 0f;
                    _walking = false;
                }

                CreateWorld();
            }
            else
            {
                UpdateBehavior();
            }

            base.Update();
        }

        private void UpdateBehavior()
        {
            var behavior = _data.npc.behavior;
            if (behavior.Type == NPCBehaviorType.Wander)
            {
                var r = _random.Next(0, 500);
                if (r == 0)
                {

                    //_walking = true;
                }
                else if (r < 5)
                {
                    if (behavior.x.Length == 0)
                    {
                        Facing = (EntityFacing)(_random.Next(0, 2) * 2);
                    }
                    else if (behavior.y.Length == 0)
                    {
                        Facing = (EntityFacing)(1 + _random.Next(0, 2) * 2);
                    }
                    else
                    {
                        Facing = (EntityFacing)_random.Next(0, 4);
                    }
                }
            }
            else if (behavior.Type == NPCBehaviorType.Look)
            {
                var r = _random.Next(0, 500);
                if (r < 5)
                {
                    Facing = (EntityFacing)_random.Next(0, 4);
                }
            }
        }
    }
}
