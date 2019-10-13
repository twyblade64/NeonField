using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System to apply the current velocity to the position component.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(VelocityLimitSystem))]
public class VelocityMovementSystem : JobComponentSystem {
  [BurstCompile]
  struct MoveJob : IJobForEach<Translation, Velocity> {
    public float deltaTime;

    public void Execute(ref Translation pos, [ReadOnly] ref Velocity vel) {
      pos.Value += vel.Value * deltaTime;
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    MoveJob job = new MoveJob {
      deltaTime = Time.fixedDeltaTime
    };

    JobHandle jobHandle = job.Schedule(this, inputDeps);
    return jobHandle;
  }
}