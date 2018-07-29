using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

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
      deltaTime = math.min(Time.deltaTime, 1f/30)
    };

    JobHandle jobHandle = job.Schedule(this, 64, inputDeps);
    return jobHandle;
  }
}