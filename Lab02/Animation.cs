using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ObjLoader.Loader.Loaders;
using QuestGame;
using SharpDX;
using SharpDX.Direct3D11;

namespace Lab01
{
    internal class Animation
    {
        public int CurrentMesh;

        public List<MeshObject> MeshObjects = new List<MeshObject>();

        public Texture texture;
        public void Load(string directoryPath, Loader loader, SamplerState samplerState, float sizeMultiplier = 1f)
        {
            ObjLoaderFactory objLoaderFactory;
            IObjLoader objLoader;
            FileStream fileStream;
            LoadResult result;

            string[] animationFiles = Directory.GetFiles(directoryPath, "*.obj");

            for (int i = 0; i < animationFiles.Length; i++)
            {
                objLoaderFactory = new ObjLoaderFactory();
                objLoader = objLoaderFactory.Create();

                fileStream = new FileStream(animationFiles[i], FileMode.Open);
                result = objLoader.Load(fileStream);

                MeshObjects.Add(loader.LoadMeshObjectFromObjFile(result, Vector4.Zero, 0f, 0f, 0f, ref texture, samplerState,
                    sizeMultiplier));
            }
        }

        public MeshObject GetCurrentMesh()
        {
            return MeshObjects[CurrentMesh];
        }

        public void ContinueAnimation()
        {
            if(CurrentMesh + 1 >= MeshObjects.Count)
                CurrentMesh = 0;
            else
            {
                CurrentMesh++;
            }
        }

        public void StopAnimation()
        {
            CurrentMesh = 0;
        }

        public void StartAnimation(Vector4 position, float yaw, float pitch, float roll)
        {
            CurrentMesh = 0;
        }
    }
}