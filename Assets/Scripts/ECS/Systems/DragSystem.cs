using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(VelocityMovementSystem))]
public class ForceInertiaSystem : JobComponentSystem {
  [BurstCompile]
  struct ForceInertiaJob : IJobProcessComponentData<Velocity, Drag> {
    public float deltaTime;

    public void Execute(ref Velocity vel, [ReadOnly] ref Drag drag) {
      vel = new Velocity { Value = vel.Value * drag.Value };
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    ForceInertiaJob job = new ForceInertiaJob {
      deltaTime = Time.deltaTime
    };

    JobHandle jobHandle = job.Schedule(this, 64, inputDeps);
    return jobHandle;
  }
}