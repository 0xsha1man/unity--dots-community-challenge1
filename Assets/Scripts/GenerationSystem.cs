using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using static DOTSCC.GameOfLife.GameEvents;
using UnityEngine;

namespace DOTSCC.GameOfLife
{
    public partial struct GenerationSystem : ISystem
    {
        public static bool requestNextGeneration;
        public static bool repeatGeneration;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Config>();
            state.RequireForUpdate<Alive>();
            state.RequireForUpdate<Generation>();

            GameEvents.OnClicked += UI_OnClicked;
        }

        public void OnDestroy()
        {
            GameEvents.OnClicked -= UI_OnClicked;
            requestNextGeneration = false;
            repeatGeneration = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var generation = SystemAPI.GetSingleton<Generation>();
            var generationEntity = SystemAPI.GetSingletonEntity<Generation>();
            var aliveBuffer = SystemAPI.GetSingletonBuffer<Alive>(false);

            // UI requests changes to the grid states
            if (requestNextGeneration || repeatGeneration)
            {
                generation.ProcessNext = true; // Since we are immediately checking this few lines down.

                // setting the component here because ProcessNext is used across multiple systems.
                SystemAPI.SetComponent<Generation>(generationEntity, new Generation
                {
                    CurrentGeneration = generation.CurrentGeneration,
                    NumberOfAliveCells = generation.NumberOfAliveCells,
                    ProcessNext = true
                });
                requestNextGeneration = false;
            }

            // Begin generating the next generation
            if (generation.ProcessNext)
            {
                int aliveCount = 0;
                for (int i = 0; i < aliveBuffer.Length; i++)
                {
                    if (!aliveBuffer[i].Value) continue;

                    aliveCount++;
                }

                // Update the Generation component
                SystemAPI.SetComponent<Generation>(generationEntity, new Generation
                {
                    CurrentGeneration = generation.CurrentGeneration + 1,
                    NumberOfAliveCells = aliveCount,
                    ProcessNext = false // Ensures this stays false until changed by the UI
                });
            }
        }

        private void UI_OnClicked(GameEvents.UIEvent e)
        {
            switch (e.Type)
            {
                case Buttons_Types.GenerateNext:
                    requestNextGeneration = !requestNextGeneration;
                    break;
                case Buttons_Types.Repeat:
                    repeatGeneration = !repeatGeneration;
                    break;

            }
        }
    }
}