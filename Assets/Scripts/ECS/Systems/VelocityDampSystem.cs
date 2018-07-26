using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(VelocityMovementSystem))]
[UpdateAfter(typeof(ForceVelocitySystem))]
public class VelocityDampSystem : JobComponentSystem {
  public const float STABILITY_THERESHOLD = 0.0001f;
  [BurstCompile]
  struct VelocityDampJob : IJobProcessComponentData<Velocity, Damper> {
    public float deltaTime;

    public void Execute(ref Velocity vel, [ReadOnly] ref Damper damper) {
      float3 v =  vel.Value * damper.Value;
      if (math.lengthSquared(v) > STABILITY_THERESHOLD)
        vel = new Velocity { Value = v };
      else
        vel = new Velocity { Value = new float3(0,0,0) };
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