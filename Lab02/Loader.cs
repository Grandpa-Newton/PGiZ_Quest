﻿using System;
using System.Collections.Generic;
using ObjLoader.Loader.Loaders;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.WIC;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace QuestGame
{
    class Loader : IDisposable
    {
        private DirectX3DGraphics _directX3DGraphics;
        private ImagingFactory _imagingFactory;

        public Loader(DirectX3DGraphics directX3DGraphics)
        {
            _directX3DGraphics = directX3DGraphics;
            _imagingFactory = new ImagingFactory();
        }

        public Texture LoadTextureFromFile(string fileName,
            SamplerState samplerState)
        {
            BitmapDecoder decoder = new BitmapDecoder(_imagingFactory,
                fileName, DecodeOptions.CacheOnDemand);
            BitmapFrameDecode bitMapFirstFrame = decoder.GetFrame(0);
            Utilities.Dispose(ref decoder);

            FormatConverter imageFormatConverter = new FormatConverter(_imagingFactory);
            imageFormatConverter.Initialize(bitMapFirstFrame,
                PixelFormat.Format32bppRGBA, BitmapDitherType.None, null, 0.0,
                BitmapPaletteType.Custom);
            int stride = imageFormatConverter.Size.Width * 4;
            DataStream buffer = new DataStream(
                imageFormatConverter.Size.Height * stride, true, true);
            imageFormatConverter.CopyPixels(stride, buffer);

            int width = imageFormatConverter.Size.Width;
            int height = imageFormatConverter.Size.Height;

            Texture2DDescription textureDescription = new Texture2DDescription()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = _directX3DGraphics.SampleDescription,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            Texture2D textureObject = new Texture2D(_directX3DGraphics.Device,
                textureDescription, new DataRectangle(buffer.DataPointer,
                stride));
            ShaderResourceViewDescription shaderResourceViewDescription =
                new ShaderResourceViewDescription()
                {
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Format = Format.R8G8B8A8_UNorm,
                    Texture2D =
                        new ShaderResourceViewDescription.Texture2DResource
                        {
                            MostDetailedMip = 0,
                            MipLevels = -1
                        }
                };
            ShaderResourceView shaderResourceView =
                new ShaderResourceView(_directX3DGraphics.Device, textureObject,
                shaderResourceViewDescription);

            Utilities.Dispose(ref imageFormatConverter);

            return new Texture(textureObject, shaderResourceView, width, height,
                samplerState);

        }
        
        public MeshObject LoadMeshObjectFromObjFile(LoadResult loadResult, Vector4 position, float yaw, float pitch, float roll, ref Texture texture, SamplerState sampler)
        {
            var currentGroup = loadResult.Groups[0];

            List<uint> indices = new List<uint>();

            List<Renderer.VertexDataStruct> vertices = new List<Renderer.VertexDataStruct>();
            
            foreach(var face in currentGroup.Faces)
            {
                for (int i = face.Count - 1; i >= 0; i--)
                {
                    var vertexPosition = loadResult.Vertices[face[i].VertexIndex - 1];
                    //var texturePosition = loadResult.Textures[face[i].TextureIndex - 1];
                    var normalPosition = loadResult.Normals[face[i].NormalIndex - 1];
                    Random random = new Random();
                    vertices.Add(new Renderer.VertexDataStruct
                    {
                        Position = new Vector4(vertexPosition.X, vertexPosition.Y, vertexPosition.Z, 1.0f),
                        //Tex0 = new Vector2(texturePosition.X, 1.0f - texturePosition.Y),
                        Tex0 = new Vector2(random.NextFloat(0f, 1f), random.NextFloat(0f, 1f)),
                        Normal = new Vector4(normalPosition.X, normalPosition.Y, normalPosition.Z, 1.0f)
                    });
                }
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                indices.Add((uint)i);
            }

            //texture = LoadTextureFromFile(currentGroup.Material.DiffuseTextureMap, sampler);

            return new MeshObject(_directX3DGraphics, position, yaw, pitch, roll, vertices.ToArray(), indices.ToArray());
        }

        public MeshObject MakePlot(Vector4 position, float yaw, float pitch, float roll, float height, float weight, float yValue, ref BoundingBox boundingBox)
        {
            MeshObject plot = MakePlot(position, yaw, pitch, roll, height, weight, yValue);
            
            boundingBox = new BoundingBox(new Vector3(-weight, yValue, -height) + new Vector3(position.X, position.Y, position.Z), new Vector3(weight, yValue, height) + new Vector3(position.X, position.Y, position.Z));

            return plot;
        }
        public MeshObject MakePlot(Vector4 position, float yaw, float pitch, float roll, float height, float width, float yValue)
        {
            Renderer.VertexDataStruct[] vertices = new Renderer.VertexDataStruct[5]
            {
                new Renderer.VertexDataStruct
                {
                    Position = new Vector4(-width, yValue, height, 1.0f),
                    Normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.0f)
                },
                new Renderer.VertexDataStruct
                {
                    Position = new Vector4(width, yValue, height, 1.0f),
                    Normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(1.0f, 0.0f),
                },
                new Renderer.VertexDataStruct
                {
                    Position = new Vector4(width, yValue, -height, 1.0f),
                    Normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(1.0f, 1.0f),
                },
                new Renderer.VertexDataStruct
                {
                    Position = new Vector4 (width, yValue, -height, 1.0f),
                    Normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(1.0f, 1.0f),
                },
                new Renderer.VertexDataStruct
                {
                    Position = new Vector4 (-width, yValue, -height, 1.0f),
                    Normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 1.0f),
                }
            };

            uint[] indices = new uint[6]
            {
                2, 1, 0,
                0, 4, 3
            };

            return new MeshObject(_directX3DGraphics, position, yaw, pitch, roll, vertices, indices);
        }

        public MeshObject MakeLittleTetrahedron(Vector4 position, float yaw, float pitch, float roll)
        {
            Renderer.VertexDataStruct[] vertices = new Renderer.VertexDataStruct[12]
            {
                new Renderer.VertexDataStruct // 0
                {
                    Position = new Vector4(0.0f, 0.1f, 0.03f, 1.0f),
                    Normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.0f),
                },
                new Renderer.VertexDataStruct // 1
                {
                    Position = new Vector4 (0.1f, -0.1f, 0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.0f),
                },
                new Renderer.VertexDataStruct // 2
                {
                    Position = new Vector4 (-0.1f, -0.1f, 0.1f, 1f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.5f),
                },
                new Renderer.VertexDataStruct // 3
                {
                    Position = new Vector4 (0.0f, 0.1f, 0.03f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.5f),
                },
                new Renderer.VertexDataStruct // 4
                {
                    Position = new Vector4 (0.0f, -0.1f, -0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.0f)
                },
                new Renderer.VertexDataStruct // 5
                {
                    Position = new Vector4 (-0.1f, -0.1f, 0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.5f)
                },
                new Renderer.VertexDataStruct // 6
                {
                    Position = new Vector4 (0.0f, 0.1f, 0.03f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.0f),
                },
                new Renderer.VertexDataStruct // 7
                {
                    Position = new Vector4 (0.0f, -0.1f, -0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(1.0f, 0.0f),
                },
                new Renderer.VertexDataStruct // 8
                {
                    Position = new Vector4 (0.1f, -0.1f, 0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.5f),
                },
                new Renderer.VertexDataStruct // 9
                {
                    Position = new Vector4 (0.1f, -0.1f, 0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(1.0f, 0.0f),
                },
                new Renderer.VertexDataStruct // 10
                {
                    Position = new Vector4 (0.0f, -0.1f, -0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(1.0f, 0.5f),
                },
                new Renderer.VertexDataStruct // 11
                {
                    Position = new Vector4 (-0.1f, -0.1f, 0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.5f),
                },
            };

            uint[] indices = new uint[12]
            {
               0, 1, 2,
               3, 5, 4,
               6, 7, 8,
               9, 10, 11

            };

            return new MeshObject(_directX3DGraphics, position, yaw, pitch, roll, vertices, indices);
        }

        public MeshObject MakeBoxCollider(BoundingBox boundingBox, Vector4 position, float yaw, float pitch, float roll)
        {

            Renderer.VertexDataStruct[] vertices = new Renderer.VertexDataStruct[8]
            {
                new Renderer.VertexDataStruct // 0
                {
                    Position = new Vector4(boundingBox.Minimum.X, boundingBox.Maximum.Y, boundingBox.Minimum.Z, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.0f),
                },
                new Renderer.VertexDataStruct // 1
                {
                    Position = new Vector4 (boundingBox.Maximum.X, boundingBox.Maximum.Y, boundingBox.Minimum.Z, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.0f),
                },
                new Renderer.VertexDataStruct // 2
                {
                    Position = new Vector4 (boundingBox.Maximum.X, boundingBox.Minimum.Y, boundingBox.Minimum.Z, 1f),
                    Tex0 = new Vector2(0.0f, 0.5f),
                },
                new Renderer.VertexDataStruct // 3
                {
                    Position = new Vector4(boundingBox.Minimum.X, boundingBox.Minimum.Y, boundingBox.Minimum.Z, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.0f),
                },
                new Renderer.VertexDataStruct // 4
                {
                    Position = new Vector4 (boundingBox.Minimum.X, boundingBox.Maximum.Y, boundingBox.Maximum.Z, 1f),
                    Tex0 = new Vector2(0.0f, 0.5f),
                },
                new Renderer.VertexDataStruct // 5
                {
                    Position = new Vector4 (boundingBox.Maximum.X, boundingBox.Maximum.Y, boundingBox.Maximum.Z, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.5f)
                },
                new Renderer.VertexDataStruct // 6
                {
                    Position = new Vector4 (boundingBox.Maximum.X, boundingBox.Minimum.Y, boundingBox.Maximum.Z, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.0f),
                },
                new Renderer.VertexDataStruct // 7
                {
                    Position = new Vector4 (boundingBox.Minimum.X,boundingBox.Minimum.Y, boundingBox.Maximum.Z, 1.0f),
                    Tex0 = new Vector2(1.0f, 0.0f),
                },
            };

            uint[] indices = new uint[36]
            {
               2, 1, 0,
               3, 2, 0,
               3, 0, 4,
               7, 3, 4,
               5, 4, 7,
               5, 7, 6,
               2, 5, 1,
               6, 2, 5,
               7, 6, 2,
               3, 2, 7,
               1, 5, 4,
               0, 1, 4
            };

            return new MeshObject(_directX3DGraphics, position, yaw, pitch, roll, vertices, indices);

        }

        public MeshObject MakeCube(Vector4 position, float yaw, float pitch, float roll)
        {
            Renderer.VertexDataStruct[] vertices = new Renderer.VertexDataStruct[8]
            {
                new Renderer.VertexDataStruct // 0
                {
                    Position = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                    Normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.0f),
                },
                new Renderer.VertexDataStruct // 1
                {
                    Position = new Vector4 (0.1f, 0.0f, 0.0f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.0f),
                },
                new Renderer.VertexDataStruct // 2
                {
                    Position = new Vector4 (0.1f, -0.1f, 0.0f, 1f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.5f),
                },
                new Renderer.VertexDataStruct // 3
                {
                    Position = new Vector4(0.0f, -0.1f, 0.0f, 1.0f),
                    Normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.0f),
                },
                new Renderer.VertexDataStruct // 4
                {
                    Position = new Vector4 (0.0f, 0.0f, 0.1f, 1f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.5f),
                },
                new Renderer.VertexDataStruct // 5
                {
                    Position = new Vector4 (0.1f, 0.0f, 0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.5f)
                },
                new Renderer.VertexDataStruct // 6
                {
                    Position = new Vector4 (0.1f, -0.1f, 0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.0f),
                },
                new Renderer.VertexDataStruct // 7
                {
                    Position = new Vector4 (0.0f, -0.1f, 0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(1.0f, 0.0f),
                },
            };

            uint[] indices = new uint[36]
            {
               2, 1, 0,
               3, 2, 0,
               3, 0, 4,
               7, 3, 4,
               5, 4, 7,
               5, 7, 6,
               2, 5, 1,
               6, 2, 5,
               7, 6, 2,
               3, 2, 7,
               1, 5, 4,
               0, 1, 4
            };

            return new MeshObject(_directX3DGraphics, position, yaw, pitch, roll, vertices, indices);
        }
        
        public MeshObject MakeParallelepiped(Vector4 position, float yaw, float pitch, float roll)
        {
            Renderer.VertexDataStruct[] vertices = new Renderer.VertexDataStruct[8]
            {
                new Renderer.VertexDataStruct // 0
                {
                    Position = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                    Normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.0f),
                },
                new Renderer.VertexDataStruct // 1
                {
                    Position = new Vector4 (0.1f, 0.0f, 0.0f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.0f),
                },
                new Renderer.VertexDataStruct // 2
                {
                    Position = new Vector4 (0.1f, -0.3f, 0.0f, 1f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.5f),
                },
                new Renderer.VertexDataStruct // 3
                {
                    Position = new Vector4(0.0f, -0.3f, 0.0f, 1.0f),
                    Normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.0f),
                },
                new Renderer.VertexDataStruct // 4
                {
                    Position = new Vector4 (0.0f, 0.0f, 0.1f, 1f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.5f),
                },
                new Renderer.VertexDataStruct // 5
                {
                    Position = new Vector4 (0.1f, 0.0f, 0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.5f)
                },
                new Renderer.VertexDataStruct // 6
                {
                    Position = new Vector4 (0.1f, -0.3f, 0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.0f),
                },
                new Renderer.VertexDataStruct // 7
                {
                    Position = new Vector4 (0.0f, -0.3f, 0.1f, 1.0f),
                    Normal = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    Tex0 = new Vector2(1.0f, 0.0f),
                },
            };

            uint[] indices = new uint[36]
            {
               2, 1, 0,
               3, 2, 0,
               3, 0, 4,
               7, 3, 4,
               5, 4, 7,
               5, 7, 6,
               2, 5, 1,
               6, 2, 5,
               7, 6, 2,
               3, 2, 7,
               1, 5, 4,
               0, 1, 4
            };

            return new MeshObject(_directX3DGraphics, position, yaw, pitch, roll, vertices, indices);
        }

        public MeshObject MakeGrid(Vector4 position, float yaw, float pitch, float roll)
        {
            int frequency = 20;
            int width = 30;

            int index = 0;

            Renderer.VertexDataStruct[] vertices = new Renderer.VertexDataStruct[frequency * frequency * (width + 1) * (width + 1)];

            for (int i = -width / 2; i < width / 2 + 1; i++)
            {
                for (int di = 0; di < frequency; di++)
                {
                    float x = (float)i + (float)di / (float)frequency;
                    for (int j = -width / 2; j < width / 2 + 1; j++)
                    {
                        for (int dj = 0; dj < frequency; dj++)
                        {
                            float z = (float)j + (float)dj / (float)frequency;

                            vertices[index++] = new Renderer.VertexDataStruct
                            {
                                Position = new Vector4(x, -10.0f, z, 1.0f),
                                Normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f)
                            };
                        }
                    }
                }

            }

            uint[] indices = new uint[frequency * frequency * width * width * 6];

            index = 0;

            for (int i = 0; i < (width - 1) * frequency; i++)
            {
                for (int j = 0; j < (width - 1) * frequency; j++)
                {
                    indices[index++] = (uint)(i * (width + 1) * frequency + j);
                    indices[index++] = (uint)(i * (width + 1) * frequency + j + 1);
                    indices[index++] = (uint)((i + 1) * (width + 1) * frequency + j + 1);

                    indices[index++] = (uint)((i + 1) * (width + 1) * frequency + j + 1);
                    indices[index++] = (uint)((i + 1) * (width + 1) * frequency + j);
                    indices[index++] = (uint)(i * (width + 1) * frequency + j);
                }
            }

            return new MeshObject(_directX3DGraphics, position, yaw, pitch, roll, vertices, indices);

        }

        public MeshObject MakeTetrahedron(Vector4 position, float yaw, float pitch, float roll)
        {

            Vector4 firstNormal = new Vector4();
            Renderer.VertexDataStruct[] vertices = new Renderer.VertexDataStruct[12]
            {
                new Renderer.VertexDataStruct // 0
                {
                    Position = new Vector4(0.0f, 1.0f, 0.3f, 1.0f),
                    Normal = new Vector4(0.0f, 0.33f, 0.9438f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.0f),
                },
                new Renderer.VertexDataStruct // 1
                {
                    Position = new Vector4 (1.0f, -1.0f, 1.0f, 1.0f),
                    Normal = new Vector4(0.0f, 0.33f, 0.9438f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.0f),
                },
                new Renderer.VertexDataStruct // 2
                {
                    Position = new Vector4 (-1.0f, -1.0f, 1.0f, 1.0f),
                    Normal = new Vector4(0.0f, 0.33f, 0.9438f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.5f),
                },
                new Renderer.VertexDataStruct // 3
                {
                    Position = new Vector4 (0.0f, 1.0f, 0.3f, 1.0f),
                    Normal = new Vector4(-0.8588f, 0.279f, -0.429f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.5f),
                },
                new Renderer.VertexDataStruct // 4
                {
                    Position = new Vector4 (0.0f, -1.0f, -1.0f, 1.0f),
                    Normal = new Vector4(-0.8588f, 0.279f, -0.429f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.0f)
                },
                new Renderer.VertexDataStruct // 5
                {
                    Position = new Vector4 (-1.0f, -1.0f, 1.0f, 1.0f),
                    Normal = new Vector4(-0.8588f, 0.279f, -0.429f, 1.0f),
                    Tex0 = new Vector2(0.0f, 0.5f)
                },
                new Renderer.VertexDataStruct // 6
                {
                    Position = new Vector4 (0.0f, 1.0f, 0.3f, 1.0f),
                    Normal = new Vector4(0.8588f, 0.279f, -0.429f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.0f),
                },
                new Renderer.VertexDataStruct // 7
                {
                    Position = new Vector4 (0.0f, -1.0f, -1.0f, 1.0f),
                    Normal = new Vector4(0.8588f, 0.279f, -0.429f, 1.0f),
                    Tex0 = new Vector2(1.0f, 0.0f),
                },
                new Renderer.VertexDataStruct // 8
                {
                    Position = new Vector4 (1.0f, -1.0f, 1.0f, 1.0f),
                    Normal = new Vector4(0.8588f, 0.279f, -0.429f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.5f),
                },
                new Renderer.VertexDataStruct // 9
                {
                    Position = new Vector4 (1.0f, -1.0f, 1.0f, 1.0f),
                    Normal = new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(1.0f, 0.0f),
                },
                new Renderer.VertexDataStruct // 10
                {
                    Position = new Vector4 (0.0f, -1.0f, -1.0f, 1.0f),
                    Normal = new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(1.0f, 0.5f),
                },
                new Renderer.VertexDataStruct // 11
                {
                    Position = new Vector4 (-1.0f, -1.0f, 1.0f, 1.0f),
                    Normal = new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                    Tex0 = new Vector2(0.5f, 0.5f),
                },
            };

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position *= 0.1f;
                vertices[i].Position.W = 1.0f;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                var x = vertices[i].Position.X;
                var y = vertices[i].Position.Y;

                //vertices[i].Position = new Vector4(x - y, (x + y) / 2, vertices[i].Position.Z, 1.0f);
            }

            /*Vector3 vectorA = (Vector3)vertices[1].Position - (Vector3)vertices[0].Position;
            Vector3 vectorB = (Vector3)vertices[2].Position - (Vector3)vertices[0].Position;

            Vector3 vectorC = (new Vector3(vectorA.Y * vectorB.Z - vectorA.Z * vectorB.Y,
                vectorA.Z * vectorB.X - vectorA.X * vectorB.Z, 
                vectorA.X * vectorB.Y - vectorA.Y * vectorB.X)) / 4.0f;

            Vector3 vectorC = Vector3.Cross(vectorA, vectorB);

            vectorC.Normalize();

            Vector4 vector4C = new Vector4(-vectorC.X, -vectorC.Y, -vectorC.Z, 1.0f);

            vertices[0].Normal = vector4C;
            vertices[1].Normal = vector4C;
            vertices[2].Normal = vector4C;

            vectorA = (Vector3)vertices[4].Position - (Vector3)vertices[3].Position;
            vectorB = (Vector3)vertices[5].Position - (Vector3)vertices[3].Position;

            /*vectorC = (new Vector3(vectorA.Y * vectorB.Z - vectorA.Z * vectorB.Y,
                vectorA.Z * vectorB.X - vectorA.X * vectorB.Z,
                vectorA.X * vectorB.Y - vectorA.Y * vectorB.X)) / 4.0f;

            vectorC = Vector3.Cross(vectorA, vectorB);

            vectorC.Normalize();

            vector4C = new Vector4(-vectorC.X, -vectorC.Y, -vectorC.Z, 1.0f);

            vertices[3].Normal = vector4C;
            vertices[4].Normal = vector4C;
            vertices[5].Normal = vector4C;


            vectorA = (Vector3)vertices[7].Position - (Vector3)vertices[6].Position;
            vectorB = (Vector3)vertices[8].Position - (Vector3)vertices[6].Position;

            //vectorC = (new Vector3(vectorA.Y * vectorB.Z - vectorA.Z * vectorB.Y, vectorA.Z * vectorB.X - vectorA.X * vectorB.Z, vectorA.X * vectorB.Y - vectorA.Y * vectorB.X)) / 4.0f;

            vectorC = Vector3.Cross(vectorA, vectorB);

            vectorC.Normalize();

            vector4C = new Vector4(-vectorC.X, -vectorC.Y, -vectorC.Z, 1.0f);

            vertices[6].Normal = vector4C;
            vertices[7].Normal = vector4C;
            vertices[8].Normal = vector4C;


            vectorA = (Vector3)vertices[10].Position - (Vector3)vertices[9].Position;
            vectorB = (Vector3)vertices[11].Position - (Vector3)vertices[9].Position;

            //vectorC = (new Vector3(vectorA.Y * vectorB.Z - vectorA.Z * vectorB.Y, vectorA.Z * vectorB.X - vectorA.X * vectorB.Z, vectorA.X * vectorB.Y - vectorA.Y * vectorB.X)) / 4.0f;

            vectorC = Vector3.Cross(vectorA, vectorB);

            vectorC.Normalize();

            vector4C = new Vector4(-vectorC.X, -vectorC.Y, -vectorC.Z, 1.0f);

            vertices[9].Normal = vector4C;
            vertices[10].Normal = vector4C;
            vertices[11].Normal = vector4C;

            

            Vector3 vertA, vertB, vertC;

            vertA = new Vector3(1.0f, -1.0f, 1.0f);
            vertB = new Vector3(0.0f, -1.0f, -1.0f);
            vertC = new Vector3(-1.0f, -1.0f, 1.0f);

            Vector3 vectA, vectB, vectC;

            vectA = vertB - vertA;
            vectB = vertC - vertA;

            vectC = Vector3.Cross(vectA, vectB);

            //vectC = (new Vector3(vectA.Y * vectB.Z - vectA.Z * vectB.Y, vectA.Z * vectB.X - vectA.X * vectB.Z, vectA.X * vectB.Y - vectA.Y * vectB.X));

            vectC.Normalize();*/





            uint[] indices = new uint[12]
            {
               0, 1, 2,
               3, 5, 4,
               6, 7, 8,
               9, 10, 11

            };


            return new MeshObject(_directX3DGraphics, position, yaw, pitch, roll, vertices, indices);
        }

        public void Dispose()
        {
            Utilities.Dispose(ref _imagingFactory);
        }
    }
}