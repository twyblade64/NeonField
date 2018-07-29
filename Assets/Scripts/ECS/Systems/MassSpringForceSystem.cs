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
    [ReadOnly] public ComponentDataArray<Spring> Lines;
  }

  [Inject] private MassData _massData;
  [Inject] private SpringData _springData;

  [BurstCompile]
  struct MassSpringForceJob : IJobParallelFor {
    public int _massAmmount;
    public int _springAmmount;
    [ReadOnly] public ComponentDataArray<EntityPair> _springEntityPairs;
    [ReadOnly] public ComponentDataArray<Elasticity> _springElasticities;
    [ReadOnly] public ComponentDataArray<Spring> _springLines;
    [ReadOnly] public EntityArray _massEntities;
    public ComponentDataArray<Physical> _massPhysicals;

    public void Execute(int index) {
    //public void Execute() {
      //for (int index = 0; index < _massAmmount; ++index) {
        Entity massEntity = _massEntities[index];
        float3 forceSum = new float3(0,0,0);
        for (int i = 0; i < _springAmmount; ++i) {
          if (massEntity == _springEntityPairs[i].E1) {
            float3 dir = (_springLines[i].p2 - _springLines[i].p1);
            float dist = math.length(dir);
            float refDist = _springLines[i].length;
            if (dist > refDist*refDist)
              forceSum += dir/dist*math.min(dist-refDist,1)*_springElasticities[i].Value;
          }
          if (massEntity == _springEntityPairs[i].E2) {
            float3 dir = (_springLines[i].p1 - _springLines[i].p2);
            float dist = math.length(dir);
            float refDist = _springLines[i].length;
            if (dist > refDist*refDist)
              forceSum += dir/dist*math.min(dist-refDist,1)*_springElasticities[i].Value;
          }
        }

        //Debug.Log("ForceSum: "+forceSum);
        _massPhysicals[index] = new Physical { 
          Force = _massPhysicals[index].Force + forceSum, 
          InverseMass = _massPhysicals[index].InverseMass
        };
      //}
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    if (_massData.Length == 0 || _springData.Length == 0)
      return inputDeps;

    MassSpringForceJob job = new MassSpringForceJob {
      //_massAmmount = _massData.Length,
      _springAmmount = _springData.Length,
      _springEntityPairs = _springData.EntityPairs,
      _springElasticities = _springData.Elasticities,
      _springLines = _springData.Lines,
      _massEntities = _massData.Entities,
      _massPhysicals = _massData.Physicals
    };

    JobHandle jobHandle = job.Schedule(_massData.Length, 64, inputDeps);
    //JobHandle jobHandle = job.Schedule(inputDeps);
    return jobHandle;
  }
}