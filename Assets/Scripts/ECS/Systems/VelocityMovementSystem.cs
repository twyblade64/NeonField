using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

/// <summary>
/// System to apply the current velocity to the position component.
/// 
/// - Raul Vera 2018
/// </summary>

[UpdateInGroup(typeof(PhysicUpdate))]
[UpdateAfter(typeof(VelocityDampSystem))]
public class VelocityMovementSystem : JobComponentSystem {
  [BurstCompile]
  struct MoveJob : IJobProcessComponentData<Position, Velocity> {
    public float deltaTime;

    public void Execute(ref Position pos, [ReadOnly] ref Velocity vel) {
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