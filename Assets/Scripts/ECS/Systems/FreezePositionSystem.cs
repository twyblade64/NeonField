using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(VelocityMovementSystem))]
public class FreezePositionSystem : JobComponentSystem {
  [BurstCompile]
  struct FreezePositionJob : IJobProcessComponentData<Position, FreezeAxis> {
    public void Execute(ref Position position, [ReadOnly] ref FreezeAxis freezeAxis) {
      position.Value = new float3(
        (freezeAxis.FreezeMask | FreezeAxis.AxisMask.X) != 0 ? freezeAxis.FreezePos.x : position.Value.x,
        (freezeAxis.FreezeMask | FreezeAxis.AxisMask.Y) != 0 ? freezeAxis.FreezePos.y : position.Value.y,
        (freezeAxis.FreezeMask | FreezeAxis.AxisMask.Z) != 0 ? freezeAxis.FreezePos.z : position.Value.z
      );
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    FreezePositionJob job = new FreezePositionJob();
    JobHandle jobHandle = job.Schedule(this, 64, inputDeps);
    return jobHandle;
  }
}