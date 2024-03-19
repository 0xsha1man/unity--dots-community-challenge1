using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace DOTSCC.GameOfLife
{
    /// <summary>
    /// Code is heavily inspired by the Entities Tutorial: Firefighters that was reviewed by
    /// the https://learn.unity.com/tutorial/dots-bootcamp
    /// See Also: https://github.com/Unity-Technologies/EntityComponentSystemSamples/tree/master/EntitiesSamples/Assets/Tutorials/Firefighters
    /// </summary>
    public partial struct GridGenerationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Config>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false; // This update will run once and only once

            var config = SystemAPI.GetSingleton<Config>();

            // Create Grid
            var celltransform = state.EntityManager.GetComponentData<LocalTransform>(config.CellPrefab);

            for (int column = 0; column < config.NumOfColumns; column++)
            {
                for (int row = 0; row < config.NumOfRows; row++)
                {
                    var cellEntity = state.EntityManager.Instantiate(config.CellPrefab);
                    celltransform.Position.x = column + 0.5f;
                    celltransform.Position.z = row + 0.5f;

                    state.EntityManager.SetComponentData(cellEntity, celltransform);
                    state.EntityManager.SetComponentData(cellEntity, new URPMaterialPropertyBaseColor
                    {
                        Value = config.DeadColor
                    });
                }
            }

            // Moving the cells to align their index order in the aliveBuffer. Using a trick due to how SystemAPI.Query returns the collection.
            // As long as the entities matched doesn't change, the returned collection will always be in the same order.
            var x = 0;
            var z = 0;

            foreach (var transform in
                SystemAPI.Query<RefRW<LocalTransform>>()
                .WithAll<Cell>())
            {
                transform.ValueRW.Position.x = x + 0.5f;
                transform.ValueRW.Position.z = z + 0.5f;

                x++; // Increase the columns

                if (x >= config.NumOfColumns)
                {
                    x = 0;  // Reset the columns
                    z++;    // Increase the rows
                }
            }
        }
    }
}