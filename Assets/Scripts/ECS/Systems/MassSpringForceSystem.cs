using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[UpdateBefore(typeof(ForceVelocitySystem))]
[UpdateAfter(typeof(LineFromEntitiesSystem))]
public class MassSpringForceSystem : JobComponentSystem {
  public struct MassData {
    public readonly int Length;
    [ReadOnly] public EntityArray Entities;
    public ComponentDataArray<Physical> Physicals;
  }

  public struct SpringData {
    public readonly int Length;
    [ReadOnly] public ComponentDataArray<EntityPair> EntityPairs;
    [ReadOnly] public ComponentDataArray<Elasticity> Elasticities;
    [ReadOnly] public ComponentDataArray<Line> Lines;
  }

  [Inject] private MassData _massData;
  [Inject] private SpringData _springData;

  [BurstCompile]
  struct MassSpringForceJob : IJobParallelFor {
    public int _springAmmount;
    [ReadOnly] public ComponentDataArray<EntityPair> _springEntityPairs;
    [ReadOnly] public ComponentDataArray<Elasticity> _springElasticities;
    [ReadOnly] public ComponentDataArray<Line> _springLines;
    [ReadOnly] public EntityArray _massEntities;
    public ComponentDataArray<Physical> _massPhysicals;

    public void Execute(int index) {
      Entity massEntity = _massEntities[index];
      float3 forceSum = new float3(0,0,0);
      for (int i = 0; i < _springAmmount; ++i) {
        if (massEntity == _springEntityPairs[i].E1) 
          forceSum += (_springLines[i].p2 - _springLines[i].p1)*_springElasticities[i].Value;
        if (massEntity == _springEntityPairs[i].E2)
          forceSum += (_springLines[i].p1 - _springLines[i].p2)*_springElasticities[i].Value;
      }
      _massPhysicals[index] = new Physical { 
        Force = _massPhysicals[index].Force + forceSum, 
        InverseMass = _massPhysicals[index].InverseMass
      };
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    if (_massData.Length == 0 || _springData.Length == 0)
      return inputDeps;

    MassSpringForceJob job = new MassSpringForceJob {
      _springAmmount = _springData.Length,
      _springEntityPairs = _springData.EntityPairs,
      _springElasticities = _springData.Elasticities,
      _springLines = _springData.Lines,
      _massEntities = _massData.Entities,
      _massPhysicals = _massData.Physicals
    };

    JobHandle jobHandle = job.Schedule(_massData.Length, 64, inputDeps);
    return jobHandle;
  }
}