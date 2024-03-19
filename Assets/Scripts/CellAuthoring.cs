using Unity.Entities;
using UnityEngine;

namespace DOTSCC.GameOfLife
{
    public class CellAuthoring : MonoBehaviour
    {
        private class Baker : Baker<CellAuthoring>
        {
            public override void Bake(CellAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent<Cell>(entity);
            }
        }
    }

    public struct Cell : IComponentData
    {
    }

    public struct Alive : IBufferElementData
    {
        public bool Value;
    }
}