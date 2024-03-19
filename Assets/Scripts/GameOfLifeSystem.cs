using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace DOTSCC.GameOfLife
{
    public partial struct GameOfLifeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Config>();
            state.RequireForUpdate<Cell>();
            state.RequireForUpdate<Alive>();
            state.RequireForUpdate<Generation>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<Config>();
            var generation = SystemAPI.GetSingleton<Generation>();
            var aliveBuffer = SystemAPI.GetSingletonBuffer<Alive>(false);

            // This flag is set by GenerationSystem.cs and currently is set to true for 1 frame at game start,
            // and then again whenever the generate next button is clicked via the UI.
            if (generation.ProcessNext)
            {
                // Check the next generation of cells
                state.Dependency = DeadOrAlive_ParallelJob(state.Dependency, ref state, aliveBuffer, config);
            }

            // Update the colors of the cells
            state.Dependency = DeadOrAliveColoring_ParallelJob(state.Dependency, ref state, aliveBuffer, config);
        }

        private JobHandle DeadOrAlive_ParallelJob(JobHandle dependency, ref SystemState state,
            DynamicBuffer<Alive> aliveBuffer, Config config)
        {
            var deadOrAliveJob = new DeadOrAliveJob_Parallel
            {
                aliveRW = aliveBuffer.AsNativeArray(),
                aliveRO = aliveBuffer.ToNativeArray(state.WorldUpdateAllocator),
                NumColumns = config.NumOfColumns,
                NumRows = config.NumOfRows
            };

            return deadOrAliveJob.Schedule(aliveBuffer.Length, 100, dependency);
        }

        [BurstCompile]
        public struct DeadOrAliveJob_Parallel : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Alive> aliveRO;
            public NativeArray<Alive> aliveRW;
            public int NumColumns;
            public int NumRows;

            public void Execute(int index)
            {
                int row = index / NumColumns;
                int col = index % NumColumns;

                int neighborsAlive = 0;

                // Clamping here since math functions are optimized
                int prevCol = math.clamp(col - 1, 0, NumColumns - 1);
                int nextCol = math.clamp(col + 1, 0, NumColumns - 1);
                int prevRow = math.clamp(row - 1, 0, NumRows - 1);
                int nextRow = math.clamp(row + 1, 0, NumRows - 1);

                // Check neighbors using 'AliveCount' function for alive state
                // Top Row
                neighborsAlive += AliveCount(prevRow, prevCol); // Top-Left
                neighborsAlive += AliveCount(prevRow, col);     // Top-Center
                neighborsAlive += AliveCount(prevRow, nextCol); // Top-Right

                // Middle Row
                neighborsAlive += AliveCount(row, prevCol);    // Middle-Left
                // This is the current cell, skipped
                neighborsAlive += AliveCount(row, nextCol);    // Middle-Right

                // Bottom Row
                neighborsAlive += AliveCount(nextRow, prevCol); // Bottom-Left
                neighborsAlive += AliveCount(nextRow, col);     // Bottom-Center
                neighborsAlive += AliveCount(nextRow, nextCol); // Bottom-Right

                // Game of Life Rules:
                bool staysAlive = aliveRO[index].Value; // Is the cell dead or alive?

                // Currently living cell
                if (staysAlive)
                {
                    // Rule 1: Live cells with 0 or 1 neighbors, die due to underpopulation
                    if (neighborsAlive == 0 || neighborsAlive == 1)
                        staysAlive = false;

                    // Rule 2: Live cells with 2 or 3 live neighbors, live for another generation
                    if (neighborsAlive == 2 || neighborsAlive == 3)
                        staysAlive = true;

                    // Rule 3: Live cells with 4 or more live neighbors, die due to overpopulation
                    if (neighborsAlive >= 4)
                        staysAlive = false;
                }
                // Currently dead cell
                else
                {
                    // Rule 4: Dead cells with exactly 3 live neighbors, become alive (birth)
                    if (neighborsAlive == 3)
                        staysAlive = true;
                }

                // Update the cell in the 'aliveRW' array
                aliveRW[index] = new Alive { Value = staysAlive };
            }

            /// <summary>
            /// Checks cell for out of bounds and current value
            /// </summary>
            /// <returns>0 if dead, 1 if alive</returns>
            private int AliveCount(int row, int col)
            {
                if (col < 0 || col >= NumColumns || row < 0 || row >= NumRows)
                {
                    return 0; // Out of bounds, thus by its nature it's dead
                }

                return aliveRO[row * NumColumns + col].Value ? 1 : 0;
            }
        }

        private JobHandle DeadOrAliveColoring_ParallelJob(JobHandle dependency, ref SystemState state,
            DynamicBuffer<Alive> aliveBuffer, Config config)
        {
            var random = new Unity.Mathematics.Random((uint)SystemAPI.Time.ElapsedTime * 1000);
            var deadOrAliveColoringJob = new DeadOrAliveColoringJob_Parallel
            {
                ElapsedTime = (float)SystemAPI.Time.ElapsedTime,
                MinAliveColor = config.MinAliveColor,
                MaxAliveColor = config.MaxAliveColor,
                DeadColor = config.DeadColor,
                OscillationSpeed = config.AliveOscillationScale,
                OscillationOffset = random.NextFloat(0.0f, 2.0f * math.PI),
                AliveBuffer = aliveBuffer,
            };

            return deadOrAliveColoringJob.ScheduleParallel(dependency);
        }

        [BurstCompile]
        public partial struct DeadOrAliveColoringJob_Parallel : IJobEntity
        {
            public float ElapsedTime;
            public float4 MinAliveColor;
            public float4 MaxAliveColor;
            public float4 DeadColor;
            public float OscillationSpeed;
            public float OscillationOffset;

            [ReadOnly] public DynamicBuffer<Alive> AliveBuffer;

            public void Execute(ref URPMaterialPropertyBaseColor color, [EntityIndexInQuery] int entityIdx)
            {
                var alive = AliveBuffer[entityIdx].Value;

                if (alive)
                {
                    // Oscillate to simulate the cell is 'breathing'
                    float oscillationFactor = (math.sin(ElapsedTime * OscillationSpeed) + 1) * 0.5f;
                    float4 aliveColor = math.lerp(MinAliveColor, MaxAliveColor, oscillationFactor);
                    color.Value = aliveColor;
                }
                else
                {
                    // Cell is dead
                    color.Value = DeadColor;
                }
            }
        }
    }
}