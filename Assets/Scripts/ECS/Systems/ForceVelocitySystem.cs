using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

/// <summary>
/// System used to convert recieved forces into velocity.
/// 
/// - Raul Vera 2018
/// </summary>

[UpdateInGroup(typeof(PhysicUpdate))]
[UpdateAfter(typeof(ForceInfluenceSystem))]
[UpdateAfter(typeof(MassSpringForceSystem))]
public class ForceVelocitySystem : JobComponentSystem {
  public const float STABILITY_THERESHOLD = 0.0001f;

  [BurstCompile]
  struct ApplyForceJob : IJobProcessComponentData<Velocity, Physical> {
    public float deltaTime;

    public void Execute(ref Velocity vel, ref Physical phys) {
        vel.Value +=  phys.Force * deltaTime * phys.InverseMass;
      phys.Force = new float3(0,0,0);
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    ApplyForceJob job = new ApplyForceJob {
      deltaTime = Time.fixedDeltaTime
    };

    JobHandle jobHandle = job.Schedule(this, inputDeps);
    return jobHandle;
  }
}