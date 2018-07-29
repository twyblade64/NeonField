using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(VelocityMovementSystem))]
public class VelocityDampSystem : JobComponentSystem {
  [BurstCompile]
  struct VelocityDampJob : IJobProcessComponentData<Velocity, Damper> {
    public float deltaTime;

    public void Execute(ref Velocity vel, [ReadOnly] ref Damper damper) {
      vel = new Velocity { Value = vel.Value * damper.Value };
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    VelocityDampJob job = new VelocityDampJob {
      deltaTime = Time.deltaTime
    };

    JobHandle jobHandle = job.Schedule(this, 64, inputDeps);
    return jobHandle;
  }
}