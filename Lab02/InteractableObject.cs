using System;
using QuestGame.Infrastructure;
using SharpDX;

namespace QuestGame.Logic
{
    internal class InteractableObject
    {
        public MeshObject MeshObject;
        public BoundingBox MeshCollider;
        public Action OnInteract;

        public InteractableObject(MeshObject meshObject, BoundingBox collider)
        {
            MeshObject = meshObject;
            MeshCollider = collider;
        }

        public void Interact()
        {
            OnInteract?.Invoke();
        }
    }
}