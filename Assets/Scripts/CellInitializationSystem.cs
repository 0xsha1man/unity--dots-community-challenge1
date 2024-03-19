using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTSCC.GameOfLife
{
    public partial struct CellInitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Config>();
            state.RequireForUpdate<Cell>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            var random = new Unity.Mathematics.Random((uint)SystemAPI.Time.ElapsedTime * 1000);
            var config = SystemAPI.GetSingleton<Config>();
            var entity = state.EntityManager.CreateEntity();
            var aliveBuffer = state.EntityManager.AddBuffer<Alive>(entity);
            var generationEntity = SystemAPI.GetSingletonEntity<Generation>();
            var aliveCount = 0;
            aliveBuffer.Length = config.NumOfColumns * config.NumOfRows;
            // Setting the initial grid alive or dead states
            for (int i = 0; i < aliveBuffer.Length; i++)
            {
                var deadOrAlive = random.NextFloat() < 0.5f;
                aliveBuffer[i] = new Alive { Value = deadOrAlive }; // 50% change of being alive
                if (deadOrAlive) aliveCount++;
            }

            // Update the Generation component
            SystemAPI.SetComponent<Generation>(generationEntity, new Generation
            {
                CurrentGeneration = 0, // Generation begins with 1
                NumberOfAliveCells = aliveCount, // Half of the cells are alive
                ProcessNext = true // This triggers the generation system and the game of life system
            });
        }
    }
}