using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(VelocityMovementSystem))]
public class FreezePositionSystem : JobComponentSystem {
  [BurstCompile]
  struct FreezePositionJob : IJobProcessComponentData<Position, FreezeAxis> {
    public void Execute(ref Position position, [ReadOnly] ref FreezeAxis freezeAxis) {
      position.Value = new float3(
        freezeAxis.FreezeFlag.x ? freezeAxis.FreezePos.x : position.Value.x,
        freezeAxis.FreezeFlag.y ? freezeAxis.FreezePos.y : position.Value.y,
        freezeAxis.FreezeFlag.z ? freezeAxis.FreezePos.z : position.Value.z
      );
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    FreezePositionJob job = new FreezePositionJob();
    JobHandle jobHandle = job.Schedule(this, 64, inputDeps);
    return jobHandle;
  }
}