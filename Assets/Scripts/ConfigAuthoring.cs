using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTSCC.GameOfLife
{
    public class ConfigAuthoring : MonoBehaviour
    {
        [Header("Grid")]
        public int NumOfRows;
        public int NumOfColumns;
        public Color DeadColor;
        public Color MinAliveColor;
        public Color MaxAliveColor;
        public float AliveOscillationScale;

        [Header("Prefabs")]
        public GameObject CellPrefab;

        class Baker : Baker<ConfigAuthoring>
        {
            public override void Bake(ConfigAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new Config
                {
                    NumOfRows = authoring.NumOfRows,
                    NumOfColumns = authoring.NumOfColumns,
                    DeadColor = (Vector4)authoring.DeadColor,
                    MinAliveColor = (Vector4)authoring.MinAliveColor,
                    MaxAliveColor = (Vector4)authoring.MaxAliveColor,
                    AliveOscillationScale = authoring.AliveOscillationScale,
                    CellPrefab = GetEntity(authoring.CellPrefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct Config : IComponentData
    {
        public int NumOfRows;
        public int NumOfColumns;
        public float4 DeadColor;
        public float4 MinAliveColor;
        public float4 MaxAliveColor;
        public float AliveOscillationScale;
        public Entity CellPrefab;
    }
}