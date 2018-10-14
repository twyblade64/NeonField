using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

/// <summary>
/// Limit the maximum speed of the entities.
/// Needed to avoid some inestability issues with high elasticity 
/// setups that suddenly explode.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>

[UpdateInGroup(typeof(PhysicUpdate))]
[UpdateAfter(typeof(VelocityDampSystem))]
public class VelocityLimitSystem : JobComponentSystem {
  
  [BurstCompile]
  struct VelocityLimitJob : IJobProcessComponentData<Velocity, MaxSpeed> {
    public void Execute(ref Velocity vel, [ReadOnly] ref MaxSpeed maxSpeed) {
      float3 v = vel.Value;
      float mag = math.lengthsq(v);
      if (mag > maxSpeed.Value * maxSpeed.Value)
        vel = new Velocity { Value = v / math.sqrt(mag) * maxSpeed.Value };
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    VelocityLimitJob job = new VelocityLimitJob { };

    JobHandle jobHandle = job.Schedule(this, inputDeps);
    return jobHandle;
  }
}