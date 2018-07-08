using GameDevCommon.Drawing;
using GameDevCommon.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Data.Entities;
using System.IO;
using static Core;

namespace PokemonRedux.Game.Overworld.Entities
{
    class Prop : Entity
    {
        protected float[] _heights;
        private Vector2 _fieldSize, _steps;
        private int _animationFrame = 0, _animationFrameDelay = 0;
        private Texture2D[] _frames;

        public Prop(EntityData data)
            : this(data, Vector2.One, Vector2.One)
        { }

        public Prop(EntityData data, Vector2 fieldSize, Vector2 steps)
            : base(data)
        {
            _fieldSize = fieldSize;
            _steps = steps;

            if (!_data.centered)
            {
                var size = _data.Size3;
                if (fieldSize != Vector2.One)
                {
                    size = new Vector3(fieldSize.X, size.Y, fieldSize.Y);
                }
                var offset = new Vector3(((size.X * _steps.X) - 1f) / 2f, (size.Y - 1f) / 2f, ((size.Z * _steps.Y) - 1f) / 2f);
                _data.AddToPosition(offset);
                Position = _data.Position;
            }

            _animationFrameDelay = _data.frameDelay;
        }

        public override void LoadContent()
        {
            LoadTexture();
            base.LoadContent();
        }

        public override void LoadTexture()
        {
            var textureFile = $"Textures/World/{_data.textureFile}";
            if (!textureFile.EndsWith(".png"))
            {
                var location = ContentLoader.GetPath(Controller.Content, textureFile);
                var daytimeKey = "-" + Map.World.Daytime.ToString().ToLower() + ".png";
                var desiredFile = location + daytimeKey;
                if (File.Exists(desiredFile))
                {
                    textureFile += daytimeKey;
                }
                else
                {
                    textureFile += ".png";
                }
            }

            Texture = Controller.Content.LoadDirect<Texture2D>(textureFile);

            if (_data.isAnimated)
            {
                // get frames
                var frames = _data.Frames;
                if (frames != null && frames.Length > 0)
                {
                    _frames = new Texture2D[frames.Length];
                    for (int i = 0; i < frames.Length; i++)
                    {
                        var frame = frames[i];
                        var frameColor = Texture.GetData(frame);
                        var frameTexture = new Texture2D(Controller.GraphicsDevice, frame.Width, frame.Height);
                        frameTexture.SetData(frameColor);
                        _frames[i] = frameTexture;
                    }
                    // set initial texture
                    Texture = _frames[0];
                }
            }
        }

        public override void Update()
        {
            if (_data.isAnimated && _frames != null)
            {
                _animationFrameDelay--;
                if (_animationFrameDelay <= 0)
                {
                    _animationFrameDelay = _data.frameDelay;
                    _animationFrame++;
                    if (_animationFrame >= _frames.Length)
                    {
                        _animationFrame = 0;
                    }

                    Texture = _frames[_animationFrame];
                }
            }

            base.Update();
        }

        protected override void CreateGeometry()
        {
            if (_fieldSize == Vector2.One)
            {

                var entityGeometry = EntityGeometry.Create(_data, Texture);
                _heights = entityGeometry.Heights;
                Size = entityGeometry.Size;
                RotateSize();
                Geometry.AddVertices(entityGeometry.Vertices);

            }
            else
            {

                var entityGeometry = EntityGeometry.Create(_data, Texture);
                _heights = entityGeometry.Heights;

                Size = new Vector3(_steps.X * _fieldSize.X - (_steps.X - 1f) + (entityGeometry.Size.X - 1f),
                    entityGeometry.Size.Y,
                    _steps.Y * _fieldSize.Y - (_steps.Y - 1f) + (entityGeometry.Size.Z - 1f));

                RotateSize();

                var vertices = entityGeometry.Vertices;
                // center the field on the main position
                VertexTransformer.Offset(vertices, -new Vector3((_fieldSize.X - 1f) * _steps.X / 2f, 0,
                    (_fieldSize.Y - 1f) * _steps.Y / 2f));

                var indexOffset = 0;
                var vLength = vertices.Length;

                for (int z = 0; z < _fieldSize.Y; z++)
                {
                    for (int x = 0; x < _fieldSize.X; x++)
                    {
                        // optimize field geometry for creation time
                        // do not index vertices 
                        Geometry.AddIndexedVertices(vertices);
                        var indices = new int[vLength];
                        for (int i = 0; i < vLength; i++)
                        {
                            indices[i] = indexOffset + i;
                        }
                        Geometry.AddIndices(indices);
                        indices = null;
                        indexOffset += vLength;

                        VertexTransformer.Offset(vertices, new Vector3(_steps.X, 0, 0));
                    }
                    VertexTransformer.Offset(vertices, new Vector3(-_fieldSize.X * _steps.X, 0, _steps.Y));
                }

            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (_frames != null)
                    {
                        for (int i = 0; i < _frames.Length; i++)
                        {
                            _frames[i].Dispose();
                        }
                    }
                }

                _frames = null;
            }

            base.Dispose(disposing);
        }
    }
}
