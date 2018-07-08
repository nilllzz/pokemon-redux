using GameDevCommon.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Data.Entities;
using System;

namespace PokemonRedux.Game.Overworld.Entities
{
    abstract class Entity : Base3DObject<VertexPositionNormalTexture>, IComparable
    {
        protected EntityData _data;

        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Size = Vector3.One;
        public Map Map { get; set; }
        public virtual bool IsStatic => true;

        public Entity(EntityData data)
        {
            _data = data;
            IsVisible = _data.visible;
            Position = _data.Position;
            IsOpaque = _data.isOpaque;
            if (_data.Rotation != Vector3.Zero)
            {
                Rotation = _data.Rotation * MathHelper.PiOver2;
                RotateSize();
            }
        }

        protected void RotateSize()
        {
            if (_data.Rotation.Y % 2 == 1 && _data.Rotation.X == 0f && _data.Rotation.Z == 0f)
            {
                var temp = Size.X;
                Size.X = Size.Z;
                Size.Z = temp;
            }
        }

        protected override void CreateWorld()
        {
            World = Matrix.CreateRotationX(Rotation.X) *
                Matrix.CreateRotationY(Rotation.Y) *
                Matrix.CreateRotationZ(Rotation.Z) *
                Matrix.CreateTranslation(Position);
        }

        public abstract void LoadTexture();

        private static int Compare(Entity e1, Entity e2)
        {
            if (e1.Position.Z < e2.Position.Z)
                return -1;
            else if (e1.Position.Z > e2.Position.Z)
                return 1;
            else if (e1.Position.Y < e2.Position.Y)
                return -1;
            else if (e1.Position.Y > e2.Position.Y)
                return 1;
            return 0;
        }

        public int CompareTo(object other)
        {
            return Compare(this, (Entity)other);
        }

        public virtual float GetHeightForPosition(Vector2 position)
            => Position.Y;

        public virtual bool IsValidFloor(Vector3 position, Entity other)
            => false;

        public virtual bool DoesCollide(CollisionType collisionType, Vector3 position, Vector3 size, Entity other)
        {
            if (!_data.hasCollision)
                return false;

            return
                position.X + size.X / 2f >= Position.X - Size.X / 2f &&
                position.X - size.X / 2f <= Position.X + Size.X / 2f &&

                position.Z + size.Z / 2f >= Position.Z - Size.Z / 2f &&
                position.Z - size.Z / 2f <= Position.Z + Size.Z / 2f &&

                position.Y + size.Y / 2f > Position.Y - Size.Y / 2f &&
                position.Y - size.Y / 2f < Position.Y + Size.Y / 2f;
        }

        public virtual void Collides(Entity other) { }

        public virtual void Interact() { }
    }
}
