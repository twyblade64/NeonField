using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System used to convert recieved forces into velocity.
/// 
/// - Raul Vera 2018
/// </summary>
[UpdateBefore(typeof(VelocityMovementSystem))]
public class ForceVelocitySystem : JobComponentSystem {
  public const float STABILITY_THERESHOLD = 0.0001f;

  [BurstCompile]
  struct ApplyForceJob : IJobProcessComponentData<Velocity, Physical> {
    public float deltaTime;

    public void Execute(ref Velocity vel, ref Physical phys) {
      if (math.lengthSquared(phys.Force) > STABILITY_THERESHOLD)
        vel.Value +=  phys.Force * deltaTime * phys.InverseMass;
      phys.Force = new float3(0,0,0);
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    ApplyForceJob job = new ApplyForceJob {
      deltaTime = math.min(Time.deltaTime, 1f/30)
    };

    JobHandle jobHandle = job.Schedule(this, 64, inputDeps);
    return jobHandle;
  }
}