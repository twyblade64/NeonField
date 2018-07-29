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
  struct HashMassSpringJob : IJobParallelFor {
    public NativeMultiHashMap<Entity, int>.Concurrent _hashMap;
    [ReadOnly] public ComponentDataArray<EntityPair> _springEntityPairs;

    public void Execute(int index) {
      _hashMap.Add(_springEntityPairs[index].E1, index);
      _hashMap.Add(_springEntityPairs[index].E2, index);
    }
  }

  [BurstCompile]
  struct MassSpringForceJob : IJobParallelFor {
    [ReadOnly] public NativeMultiHashMap<Entity, int> _massSpringHashMap;
    [ReadOnly] public ComponentDataArray<EntityPair> _springEntityPairs;
    [ReadOnly] public ComponentDataArray<Elasticity> _springElasticities;
    [ReadOnly] public ComponentDataArray<Spring> _springLines;
    [ReadOnly] public EntityArray _massEntities;
    public ComponentDataArray<Physical> _massPhysicals;

    void ApplySpringForce(int massIndex, int springIndex) {
      Entity massEntity = _massEntities[massIndex];
      float3 force = new float3(0,0,0);
      float3 dir = (_springLines[springIndex].p2 - _springLines[springIndex].p1);
      float dist = math.length(dir);
      float refDist = _springLines[springIndex].length;
      if (dist > refDist*refDist)
        force = dir/dist*math.min(dist-refDist,1)*_springElasticities[springIndex].Value;
      if (massEntity == _springEntityPairs[springIndex].E2) 
        force = -force;

      _massPhysicals[massIndex] = new Physical { 
        Force = _massPhysicals[massIndex].Force + force, 
        InverseMass = _massPhysicals[massIndex].InverseMass
      };
    }

    public void Execute(int massIndex) {
      int springIndex;
      NativeMultiHashMapIterator<Entity> it;
      if (_massSpringHashMap.TryGetFirstValue(_massEntities[massIndex], out springIndex, out it)) {
        ApplySpringForce(massIndex, springIndex);
        while (_massSpringHashMap.TryGetNextValue(out springIndex, ref it))
          ApplySpringForce(massIndex, springIndex);
      }
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    if (_massData.Length == 0 || _springData.Length == 0)
      return inputDeps;

    NativeMultiHashMap<Entity, int> hashMap = new NativeMultiHashMap<Entity,int>(_massData.Length*4, Allocator.Temp);

    HashMassSpringJob hashMassSpringJob = new HashMassSpringJob {
      _hashMap = hashMap,
      _springEntityPairs = _springData.EntityPairs
    };

    JobHandle hashMassSPringHandle = hashMassSpringJob.Schedule(_springData.Length, 64, inputDeps);

    MassSpringForceJob massSpringForceJob = new MassSpringForceJob {
      _massSpringHashMap = hashMap,
      _springEntityPairs = _springData.EntityPairs,
      _springElasticities = _springData.Elasticities,
      _springLines = _springData.Lines,
      _massEntities = _massData.Entities,
      _massPhysicals = _massData.Physicals
    };

    JobHandle massSpringForceHandle = massSpringForceJob.Schedule(_massData.Length, 64, hashMassSPringHandle);
    massSpringForceHandle.Complete();
    hashMap.Dispose();
    return massSpringForceHandle;
  }
}