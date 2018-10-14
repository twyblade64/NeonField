using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

/// <summary>
/// This system takes the spring and physical components 
/// to generate a pushing or a pulling force.
/// 
/// The system has two jobs
/// - HashMassSpringJob: Creates a hashMap with the masses as keys and the springs as values
/// so we can iterate per-mass basis to calculate the resulting force over that mass.
/// - MassSpringForceJob: Iterates over all the hashMap entries, calculates the resulting force 
/// and applies it to their respective masses.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>

[UpdateInGroup(typeof(PhysicUpdate))]
[UpdateAfter(typeof(LineFromEntityPairSystem))]
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

  /// <summary>
  /// Creation of a MultiHashMap containing all the forces to be applied
  /// to each entity.
  /// </summary>
  [BurstCompile]
  struct HashSpringForceJob : IJobParallelFor {
    public NativeMultiHashMap<Entity, float3>.Concurrent _hashMap;
    [ReadOnly] public ComponentDataArray<EntityPair> _springEntityPairs;
    [ReadOnly] public ComponentDataArray<Elasticity> _springElasticities;
    [ReadOnly] public ComponentDataArray<Line> _springLines;

    public void Execute(int index) {
      EntityPair entityPair = _springEntityPairs[index];
      Line line = _springLines[index];
      Elasticity elasticity = _springElasticities[index];

      float3 dist = (line.P2 - line.P1);
      float distMag = math.length(dist);
      float refDist = elasticity.ReferenceLength;

      if (distMag > refDist) {
        float3 force = dist * (1 - refDist / distMag) * elasticity.YoungModulus;
        _hashMap.Add(entityPair.E1, force);
        _hashMap.Add(entityPair.E2, -force);
      }
    }
  }

  [BurstCompile]
  struct MassSpringForceJob : IJobParallelFor {
    [ReadOnly] public NativeMultiHashMap<Entity, float3> _massSpringHashMap;
    [ReadOnly] public EntityArray _massEntities;
    public ComponentDataArray<Physical> _massPhysicals;

    public void Execute(int massIndex) {
      Entity entity = _massEntities[massIndex];
      Physical physical = _massPhysicals[massIndex];

      float3 forceSum = new float3(0, 0, 0);
      float3 force;

      NativeMultiHashMapIterator<Entity> it;
      if (_massSpringHashMap.TryGetFirstValue(entity, out force, out it)) {
        forceSum += force;
        while (_massSpringHashMap.TryGetNextValue(out force, ref it))
          forceSum += force;

        _massPhysicals[massIndex] = new Physical {
          Force = physical.Force + forceSum,
          InverseMass = physical.InverseMass
        };
      }
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    if (_massData.Length == 0 || _springData.Length == 0)
      return inputDeps;

    NativeMultiHashMap<Entity, float3> hashMap = new NativeMultiHashMap<Entity, float3>(_massData.Length * 4, Allocator.Temp);

    HashSpringForceJob hashMassSpringJob = new HashSpringForceJob {
      _hashMap = hashMap.ToConcurrent(),
      _springEntityPairs = _springData.EntityPairs,
      _springElasticities = _springData.Elasticities,
      _springLines = _springData.Lines
    };

    JobHandle hashMassSPringHandle = hashMassSpringJob.Schedule(_springData.Length, 64, inputDeps);

    MassSpringForceJob massSpringForceJob = new MassSpringForceJob {
      _massSpringHashMap = hashMap,
      _massEntities = _massData.Entities,
      _massPhysicals = _massData.Physicals
    };

    JobHandle massSpringForceHandle = massSpringForceJob.Schedule(_massData.Length, 64, hashMassSPringHandle);
    massSpringForceHandle.Complete();
    hashMap.Dispose();
    return massSpringForceHandle;
  }
}