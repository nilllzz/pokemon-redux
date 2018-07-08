using GameDevCommon.Rendering;
using GameDevCommon.Rendering.Composers;
using GameDevCommon.Rendering.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRedux.Game.Overworld.Entities
{
    struct EntityGeometry
    {
        public VertexPositionNormalTexture[] Vertices;
        public float[] Heights;
        public Vector3 Size;

        public static EntityGeometry Create(EntityData data, Texture2D propTexture)
        {
            switch (data.GeometryType)
            {
                case GeometryType.Cube:
                    return Cube(data, propTexture);
                case GeometryType.Walls:
                    return Walls(data, propTexture);
                case GeometryType.Wall:
                    return Wall(data, propTexture);
                case GeometryType.Plane:
                    return Plane(data, propTexture);
                case GeometryType.Billboard:
                    return Billboard(data, propTexture);
                case GeometryType.Ramp:
                    return Ramp(data, propTexture);
                case GeometryType.Corner:
                    return Corner(data, propTexture);
                case GeometryType.HouseInside:
                    return HouseInside(data, propTexture);
                case GeometryType.Tube:
                    return Tube(data, propTexture);
                case GeometryType.Cylinder:
                    return Cylinder(data, propTexture);
                default:
                    throw new ArgumentException($"Invalid geometry type \"{data.geometry}\" supplied.");
            }
        }

        private static EntityGeometry Cube(EntityData data, Texture2D propTexture)
        {
            ITextureDefintion texture = DefaultTextureDefinition.Instance;
            var textures = data.Textures;
            if (textures != null)
            {
                if (textures.Length == 1)
                {
                    texture = new TextureRectangle(textures[0], propTexture);
                }
                else
                {
                    var cuboidTexture = new TextureCuboidWrapper();
                    for (int i = 0; i < 6; i++)
                    {
                        if (textures.Length > i)
                        {
                            cuboidTexture.AddSide((CuboidSide)(i + 1), new TextureRectangle(textures[i], propTexture));
                        }
                        else
                        {
                            cuboidTexture.AddSide((CuboidSide)(i + 1), DefaultTextureDefinition.Instance);
                        }
                    }
                    texture = cuboidTexture;
                }
            }

            var size = data.Size3;

            var geometry = new EntityGeometry
            {
                Vertices = CuboidComposer.Create(size.X, size.Y, size.Z, texture),
                Heights = new[] { size.Y, size.Y, size.Y, size.Y },
                Size = size
            };
            return geometry;
        }

        private static EntityGeometry Walls(EntityData data, Texture2D propTexture)
        {
            var size = data.Size3;

            var textureDefs = new ITextureDefintion[4];
            var textureDefsRects = new Rectangle[4];

            void setTexture(Rectangle rect, int dest)
            {
                if (data.tileTexture)
                {
                    if (dest % 2 == 1)
                    {
                        rect.Width = (int)(rect.Width * size.Z);
                    }
                    else
                    {
                        rect.Width = (int)(rect.Width * size.X);
                    }
                    rect.Height = (int)(rect.Height * size.Y);
                }
                textureDefs[dest] = new TextureRectangle(rect, propTexture);
                textureDefsRects[dest] = rect;
            }

            var textures = data.Textures;
            if (textures != null && textures.Length > 0)
            {
                if (textures.Length < 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        setTexture(textures[0], i);
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        setTexture(textures[i], i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    setTexture(propTexture.Bounds, i);
                }
            }

            var texture = new TextureCuboidWrapper();
            texture.AddSide(CuboidSide.Front, textureDefs[0]);
            texture.AddSide(CuboidSide.Left, textureDefs[1]);
            texture.AddSide(CuboidSide.Back, textureDefs[2]);
            texture.AddSide(CuboidSide.Right, textureDefs[3]);

            var vertices = CuboidComposer.Create(size.X, size.Y, size.Z, texture).ToList();
            // remove top and bottom vertices:
            vertices.RemoveRange(0, 6);
            vertices.RemoveRange(vertices.Count - 6, 6);

            // remove sides with 0, 0 textures:
            // shift the right vertices to th end of the list
            var right = vertices.Skip(12).Take(6).ToArray();
            vertices.RemoveRange(12, 6);
            vertices.AddRange(right);

            var removedOffset = 0;
            for (int i = 0; i < 4; i++)
            {
                if (textureDefsRects[i].Width == 0 && textureDefsRects[i].Height == 0)
                {
                    vertices.RemoveRange(removedOffset, 6);
                }
                else
                {
                    removedOffset += 6;
                }
            }

            var geometry = new EntityGeometry
            {
                Vertices = vertices.ToArray(),
                Heights = new[] { size.Y, size.Y, size.Y, size.Y },
                Size = size
            };
            return geometry;
        }

        private static EntityGeometry Wall(EntityData data, Texture2D propTexture)
        {
            var size = data.Size3;

            ITextureDefintion texture = DefaultTextureDefinition.Instance;
            var textures = data.Textures;
            if (textures != null && textures.Length > 0)
            {
                var rect = textures[0];
                if (data.tileTexture)
                {
                    rect.Width = (int)(rect.Width * size.X);
                    rect.Height = (int)(rect.Height * size.Y);
                }
                texture = new TextureRectangle(rect, propTexture);
            }
            else
            {
                if (data.tileTexture)
                {
                    texture = new TextureRectangle(0, 0, size.X, size.Y);
                }
            }

            var vertices = RectangleComposer.Create(size.X, size.Y, texture);
            VertexTransformer.Rotate(vertices, new Vector3(MathHelper.PiOver2, 0, 0));
            VertexTransformer.Offset(vertices, new Vector3(0, 0, 0.5f));

            var geometry = new EntityGeometry
            {
                Vertices = vertices,
                Heights = new[] { size.Y, size.Y, size.Y, size.Y },
                Size = new Vector3(size.X, size.Y, size.Z)
            };
            return geometry;
        }

        private static EntityGeometry Plane(EntityData data, Texture2D propTexture)
        {
            var size = data.Size2;

            ITextureDefintion texture = DefaultTextureDefinition.Instance;
            var textures = data.Textures;
            if (textures != null && textures.Length > 0)
            {
                var rect = textures[0];
                if (data.tileTexture)
                {
                    rect.Width = (int)(rect.Width * size.X);
                    rect.Height = (int)(rect.Height * size.Y);
                }
                texture = new TextureRectangle(rect, propTexture);
            }
            else
            {
                if (data.tileTexture)
                {
                    texture = new TextureRectangle(0, 0, size.X, size.Y);
                }
            }

            VertexPositionNormalTexture[] vertices;
            float[] heights;

            var floorOffset = -0.5f;

            if (data.elevation == null || data.elevation.Length < 4)
            {
                vertices = RectangleComposer.Create(size.X, size.Y, texture);
                VertexTransformer.Offset(vertices, new Vector3(0, floorOffset, 0));
                heights = new float[] { 0, 0, 0, 0 };
            }
            else
            {
                var halfWidth = size.X / 2f;
                var halfHeight = size.Y / 2f;
                vertices = RectangleComposer.Create(new[]
                {
                    new Vector3(-halfWidth, data.elevation[0] + floorOffset, -halfHeight),
                    new Vector3(halfWidth, data.elevation[1] + floorOffset, -halfHeight),
                    new Vector3(-halfWidth, data.elevation[2] + floorOffset, halfHeight),
                    new Vector3(halfWidth, data.elevation[3] + floorOffset, halfHeight),
                }, texture);
                heights = new float[] { data.elevation[0], data.elevation[1], data.elevation[2], data.elevation[3] };
            }

            var geometry = new EntityGeometry
            {
                Vertices = vertices,
                Heights = heights,
                Size = new Vector3(size.X, 1, size.Y)
            };
            return geometry;
        }

        private static EntityGeometry Ramp(EntityData data, Texture2D propTexture)
        {
            var plane = Plane(data, propTexture);

            // do not do anything to the plane if the elevation data is not there/flat
            if (data.elevation == null || data.elevation.Length < 4 || data.elevation.All(e => e == 0f))
            {
                return plane;
            }

            var vertices = plane.Vertices.ToList();

            // try to figure out the direction of the ramp:
            // zDir means the ramp rises/falls along the z axis
            var zDir = data.elevation[0] == data.elevation[1] && data.elevation[2] == data.elevation[3];

            var size = data.Size2;
            var halfWidth = size.X / 2f;
            var halfHeight = size.Y / 2f;

            var textures = data.Textures;
            var texX = 0f;
            var texY = 0f;
            var texWidth = 1f;
            var texHeight = 1f;

            if (textures != null && textures.Length > 1)
            {
                var rect = textures[1];
                texX = (float)rect.X / propTexture.Width;
                texY = (float)rect.Y / propTexture.Height;
                texWidth = (float)rect.Width / propTexture.Width;
                texHeight = (float)rect.Height / propTexture.Height;
            }

            // TODO: normals
            if (zDir)
            {
                // ramp goes into background
                if (data.elevation[0] > data.elevation[2])
                {
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(halfWidth, -0.5f, -halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX, texY + texHeight)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(halfWidth, data.elevation[0] - 0.5f, -halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX, texY)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(halfWidth, -0.5f, halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texWidth + texX, texY + texHeight)
                    });

                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-halfWidth, -0.5f, -halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX, texY + texHeight)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-halfWidth, data.elevation[0] - 0.5f, -halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX, texY)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-halfWidth, -0.5f, halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texWidth + texX, texY + texHeight)
                    });
                }
                else
                {
                    // ramp goes into foreground
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(halfWidth, -0.5f, -halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX, texY + texHeight)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(halfWidth, data.elevation[2] - 0.5f, halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX + texWidth, texY)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(halfWidth, -0.5f, halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texWidth + texX, texY + texHeight)
                    });

                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-halfWidth, -0.5f, -halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX, texY + texHeight)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-halfWidth, data.elevation[2] - 0.5f, halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX + texWidth, texY)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-halfWidth, -0.5f, halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texWidth + texX, texY + texHeight)
                    });
                }
            }
            else
            {
                if (data.elevation[0] > data.elevation[1])
                {
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-halfWidth, -0.5f, halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX, texY + texHeight)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-halfWidth, data.elevation[0] - 0.5f, halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX, texY)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(halfWidth, -0.5f, halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX + texWidth, texY + texHeight)
                    });

                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-halfWidth, -0.5f, -halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX, texY + texHeight)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-halfWidth, data.elevation[2] - 0.5f, -halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX, texY)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(halfWidth, -0.5f, -halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX + texWidth, texY + texHeight)
                    });
                }
                else
                {
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-halfWidth, -0.5f, halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX, texY + texHeight)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(halfWidth, data.elevation[1] - 0.5f, halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX + texWidth, texY)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(halfWidth, -0.5f, halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX + texWidth, texY + texHeight)
                    });

                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-halfWidth, -0.5f, -halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX, texY + texHeight)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(halfWidth, data.elevation[3] - 0.5f, -halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX + texWidth, texY)
                    });
                    vertices.Add(new VertexPositionNormalTexture
                    {
                        Position = new Vector3(halfWidth, -0.5f, -halfHeight),
                        Normal = Vector3.One,
                        TextureCoordinate = new Vector2(texX + texWidth, texY + texHeight)
                    });
                }
            }

            plane.Vertices = vertices.ToArray();

            return plane;
        }

        private static EntityGeometry Billboard(EntityData data, Texture2D propTexture)
        {
            ITextureDefintion texture = DefaultTextureDefinition.Instance;
            var textures = data.Textures;
            if (textures != null && textures.Length > 0)
            {
                texture = new TextureRectangle(textures[0], propTexture);
            }

            var size = data.Size2;
            var vertices = RectangleComposer.Create(1f, 1f, texture);
            VertexTransformer.Rotate(vertices, new Vector3(data.billboardTilt, 0, 0));
            VertexTransformer.Scale(vertices, new Vector3(size.X, size.Y, 1f));
            if (size.Y != 1f)
            {
                VertexTransformer.Offset(vertices, new Vector3(0, (size.Y - 1f) / 2f, 0));
            }

            var geometry = new EntityGeometry
            {
                Vertices = vertices,
                Heights = new[] { size.Y, size.Y, size.Y, size.Y },
                Size = new Vector3(size.X, size.Y, size.X)
            };
            return geometry;
        }

        private static EntityGeometry Corner(EntityData data, Texture2D propTexture)
        {
            var textures = data.Textures;
            var texX = 0f;
            var texY = 0f;
            var texWidth = 1f;
            var texHeight = 1f;
            if (textures != null && textures.Length > 0)
            {
                var rect = textures[0];
                texX = (float)rect.X / propTexture.Width;
                texY = (float)rect.Y / propTexture.Height;
                texWidth = (float)rect.Width / propTexture.Width;
                texHeight = (float)rect.Height / propTexture.Height;
            }

            var size = data.Size3;
            var halfWidth = size.X / 2f;
            var height = size.Y;
            var halfDepth = size.Z / 2f;
            var vertices = new List<VertexPositionNormalTexture>();

            // TODO: normals
            // back
            {
                vertices.Add(new VertexPositionNormalTexture
                {
                    Position = new Vector3(-halfWidth, -0.5f, -halfDepth),
                    Normal = Vector3.One,
                    TextureCoordinate = new Vector2(texX, texY + texHeight)
                });
                vertices.Add(new VertexPositionNormalTexture
                {
                    Position = new Vector3(halfWidth, -0.5f, -halfDepth),
                    Normal = Vector3.One,
                    TextureCoordinate = new Vector2(texX + texWidth, texY + texHeight)
                });
                vertices.Add(new VertexPositionNormalTexture
                {
                    Position = new Vector3(halfWidth, height - 0.5f, halfDepth),
                    Normal = Vector3.One,
                    TextureCoordinate = new Vector2(texX + texWidth, texY)
                });
            }
            // left
            {
                vertices.Add(new VertexPositionNormalTexture
                {
                    Position = new Vector3(-halfWidth, -0.5f, -halfDepth),
                    Normal = Vector3.One,
                    TextureCoordinate = new Vector2(texX, texY + texHeight)
                });
                vertices.Add(new VertexPositionNormalTexture
                {
                    Position = new Vector3(-halfWidth, -0.5f, halfDepth),
                    Normal = Vector3.One,
                    TextureCoordinate = new Vector2(texX + texWidth, texY + texHeight)
                });
                vertices.Add(new VertexPositionNormalTexture
                {
                    Position = new Vector3(halfWidth, height - 0.5f, halfDepth),
                    Normal = Vector3.One,
                    TextureCoordinate = new Vector2(texX + texWidth, texY)
                });
            }

            var geometry = new EntityGeometry
            {
                Vertices = vertices.ToArray(),
                Heights = new[] { 0, 0, 0, height },
                Size = size
            };
            return geometry;
        }

        private static EntityGeometry HouseInside(EntityData data, Texture2D propTexture)
        {
            ITextureDefintion texture = DefaultTextureDefinition.Instance;
            var textures = data.Textures;
            if (textures != null && textures.Length > 0)
            {
                texture = new TextureRectangle(textures[0], propTexture);
            }

            var size = data.Size3;
            var allVertices = new List<VertexPositionNormalTexture>();

            // back
            {
                var vertices = RectangleComposer.Create(1f, 1f, texture);
                VertexTransformer.Scale(vertices, new Vector3(size.X, size.Y, 1f));
                VertexTransformer.Rotate(vertices, new Vector3(MathHelper.PiOver2, 0, 0));
                VertexTransformer.Offset(vertices, new Vector3(0, 0, -size.Z / 2f));
                allVertices.AddRange(vertices);
            }
            // left
            {
                var vertices = RectangleComposer.Create(1f, 1f, texture);
                VertexTransformer.Scale(vertices, new Vector3(size.Z, size.Y, 1f));
                VertexTransformer.Rotate(vertices, new Vector3(MathHelper.PiOver2, MathHelper.PiOver2, 0));
                VertexTransformer.Offset(vertices, new Vector3(-size.X / 2f, 0, 0));
                allVertices.AddRange(vertices);
            }
            // right
            {
                var vertices = RectangleComposer.Create(1f, 1f, texture);
                VertexTransformer.Scale(vertices, new Vector3(size.Z, size.Y, 1f));
                VertexTransformer.Rotate(vertices, new Vector3(MathHelper.PiOver2, -MathHelper.PiOver2, 0));
                VertexTransformer.Offset(vertices, new Vector3(size.X / 2f, 0, 0));
                allVertices.AddRange(vertices);
            }
            // floor
            {
                var vertices = RectangleComposer.Create(1f, 1f, texture);
                VertexTransformer.Scale(vertices, new Vector3(size.X, 1f, size.Z));
                VertexTransformer.Offset(vertices, new Vector3(0, -0.499f, 0)); // do not put 0.5 to not interfer with floors
                allVertices.AddRange(vertices);
            }

            var geometry = new EntityGeometry
            {
                Vertices = allVertices.ToArray(),
                Heights = new[] { size.Y, size.Y, size.Y, size.Y },
                Size = new Vector3(size.X, size.Y, size.Z)
            };
            return geometry;
        }

        private static EntityGeometry Tube(EntityData data, Texture2D propTexture)
        {
            var elementCount = data.tubeResolution;
            ITextureDefintion texture = DefaultTextureDefinition.Instance;
            var wrapTexture = data.wrapTexture;

            var textures = data.Textures;
            if (textures != null && textures.Length > 0)
            {
                if (wrapTexture)
                {
                    texture = new TextureTubeWrapper(textures[0], propTexture.Bounds, elementCount);
                }
                else
                {
                    texture = new TextureRectangle(textures[0], propTexture);
                }
            }

            var size = data.Size2;
            // /2 because it's radius, not diameter.
            var vertices = TubeComposer.Create(size.X / 2f, size.Y, elementCount, texture);

            var geometry = new EntityGeometry
            {
                Vertices = vertices,
                Heights = new[] { size.Y, size.Y, size.Y, size.Y },
                Size = new Vector3(size.X, size.Y, size.X)
            };
            return geometry;
        }

        private static EntityGeometry Cylinder(EntityData data, Texture2D propTexture)
        {
            var elementCount = data.tubeResolution;
            ITextureDefintion sideTexture = DefaultTextureDefinition.Instance;
            ITextureDefintion topTexture = DefaultTextureDefinition.Instance;
            ITextureDefintion bottomTexture = DefaultTextureDefinition.Instance;

            var wrapTexture = data.wrapTexture;

            var textures = data.Textures;
            if (textures != null && textures.Length > 0)
            {
                if (wrapTexture)
                {
                    sideTexture = new TextureTubeWrapper(textures[0], propTexture.Bounds, elementCount);
                }
                else
                {
                    sideTexture = new TextureRectangle(textures[0], propTexture);
                }
            }
            if (textures != null && textures.Length == 2)
            {
                topTexture = new TextureRectangle(textures[1], propTexture);
                bottomTexture = new TextureRectangle(textures[1], propTexture);
            }
            else if (textures != null && textures.Length > 2)
            {
                topTexture = new TextureRectangle(textures[1], propTexture);
                bottomTexture = new TextureRectangle(textures[2], propTexture);
            }

            var size = data.Size2;
            // /2 because it's radius, not diameter.
            var vertices = CylinderComposer.Create(size.X / 2f, size.Y, elementCount, sideTexture, topTexture, bottomTexture);

            var geometry = new EntityGeometry
            {
                Vertices = vertices,
                Heights = new[] { size.Y, size.Y, size.Y, size.Y },
                Size = new Vector3(size.X, size.Y, size.X)
            };
            return geometry;
        }
    }
}
