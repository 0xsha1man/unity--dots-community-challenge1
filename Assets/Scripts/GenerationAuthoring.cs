using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace DOTSCC.GameOfLife
{
    public class GenerationAuthoring : MonoBehaviour
    {
        private class Baker : Baker<GenerationAuthoring>
        {
            public override void Bake(GenerationAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent<Generation>(entity, new Generation
                {
                    CurrentGeneration = 0,
                    NumberOfAliveCells = 0,
                    ProcessNext = false
                });
            }
        }
    }

    public struct Generation : IComponentData
    {
        public int CurrentGeneration;
        public int NumberOfAliveCells;
        public bool ProcessNext;
    }
}