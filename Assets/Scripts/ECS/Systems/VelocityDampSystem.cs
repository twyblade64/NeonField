using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Slow-down the velocity of the entities.
/// Used to decrease the kinetic energy of the of the grid.
/// 
/// - Raul Vera 2018
/// </summary>
[UpdateBefore(typeof(VelocityMovementSystem))]
[UpdateAfter(typeof(ForceVelocitySystem))]
public class VelocityDampSystem : JobComponentSystem {
  /// Velocities bellow this value will be rounded down to 0 to avoid float-point imprecision
  public const float STABILITY_THERESHOLD = 0.0001f;

  [BurstCompile]
  struct VelocityDampJob : IJobProcessComponentData<Velocity, Damper> {
    public float deltaTime;

    public void Execute(ref Velocity vel, [ReadOnly] ref Damper damper) {
      float3 v = vel.Value * damper.Value;
      if (math.lengthSquared(v) > STABILITY_THERESHOLD)
        vel = new Velocity { Value = v };
      else
        vel = new Velocity { Value = new float3(0, 0, 0) };
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    VelocityDampJob job = new VelocityDampJob {
      deltaTime = Time.deltaTime
    };

    JobHandle jobHandle = job.Schedule(this, inputDeps);
    return jobHandle;
  }
}