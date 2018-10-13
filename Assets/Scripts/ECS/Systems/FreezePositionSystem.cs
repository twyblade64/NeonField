using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// This system resets uses a bitmask to reset an specific axis of the position component to 0.
/// Used to force all the grid movement in a 2D plane. Otherwise, nodes sometimes move in the Y-axis
/// affecting the final grid behaviour.
/// 
/// - Raul Vera 2018
/// </summary>

[UpdateInGroup(typeof(PhysicUpdate))]
public class FreezePositionSystem : JobComponentSystem {
  [BurstCompile]
  struct FreezePositionJob : IJobProcessComponentData<Position, FreezeAxis> {
    public void Execute(ref Position position, [ReadOnly] ref FreezeAxis freezeAxis) {
      position.Value = new float3(
        (freezeAxis.FreezeMask & FreezeAxis.AxisMask.X) != 0 ? freezeAxis.FreezePos.x : position.Value.x,
        (freezeAxis.FreezeMask & FreezeAxis.AxisMask.Y) != 0 ? freezeAxis.FreezePos.y : position.Value.y,
        (freezeAxis.FreezeMask & FreezeAxis.AxisMask.Z) != 0 ? freezeAxis.FreezePos.z : position.Value.z
      );
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    FreezePositionJob job = new FreezePositionJob();
    JobHandle jobHandle = job.Schedule(this, inputDeps);
    return jobHandle;
  }
}