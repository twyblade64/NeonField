using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

/// <summary>
/// Slow-down the velocity of the entities.
/// Used to decrease the kinetic energy of the of the grid.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>

[UpdateInGroup(typeof(PhysicUpdate))]
[UpdateAfter(typeof(ForceVelocitySystem))]
public class VelocityDampSystem : JobComponentSystem {
  /// Velocities bellow this value will be rounded down to 0 to avoid float-point imprecision
  public const float STABILITY_THERESHOLD = 0.0001f;

  [BurstCompile]
  struct VelocityDampJob : IJobProcessComponentData<Velocity, Damper> {
    public void Execute(ref Velocity vel, [ReadOnly] ref Damper damper) {
      float3 v = vel.Value * damper.Value;
      if (math.lengthsq(v) > STABILITY_THERESHOLD)
        vel = new Velocity { Value = v };
      else
        vel = new Velocity { Value = new float3(0, 0, 0) };
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    VelocityDampJob job = new VelocityDampJob {};

    JobHandle jobHandle = job.Schedule(this, inputDeps);
    return jobHandle;
  }
}