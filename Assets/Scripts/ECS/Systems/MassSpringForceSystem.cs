using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(LineFromEntityPairSystem))]
public class MassSpringForceSystem : JobComponentSystem {
  EntityQuery massQuery;
  EntityQuery springQuery;

  // public struct MassData {
  //   public readonly int Length;
  //   [ReadOnly] public NativeArray<Entity> Entities;
  //   public NativeArray<Physical> Physicals;
  // }

  // public struct SpringData {
  //   public readonly int Length;
  //   [ReadOnly] public NativeArray<EntityPair> EntityPairs;
  //   [ReadOnly] public NativeArray<Elasticity> Elasticities;
  //   [ReadOnly] public NativeArray<Line> Lines;
  // }

  // [Inject] private MassData _massData;
  // [Inject] private SpringData _springData;

  /// <summary>
  /// Creation of a MultiHashMap containing all the forces to be applied
  /// to each entity.
  /// </summary>
  [BurstCompile]
  struct HashSpringForceJob : IJobForEach<EntityPair, Elasticity, Line> {
    public NativeMultiHashMap<Entity, float3>.ParallelWriter _hashMap;

    public void Execute([ReadOnly] ref EntityPair entityPair, [ReadOnly] ref Elasticity elasticity, [ReadOnly] ref Line line) {
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
  struct MassSpringForceJob : IJobForEachWithEntity<Physical> {
    [ReadOnly] public NativeMultiHashMap<Entity, float3> _massSpringHashMap;

    public void Execute(Entity entity, int index, ref Physical physical) {
      float3 forceSum = new float3(0, 0, 0);
      float3 force;

      NativeMultiHashMapIterator<Entity> it;
      if (_massSpringHashMap.TryGetFirstValue(entity, out force, out it)) {
        forceSum += force;
        while (_massSpringHashMap.TryGetNextValue(out force, ref it))
          forceSum += force;

        physical = new Physical {
          Force = physical.Force + forceSum,
          InverseMass = physical.InverseMass
        };
      }
    }
  }

  protected override void OnCreate() {
    massQuery = GetEntityQuery( new EntityQueryDesc {
      All = new [] {
        ComponentType.ReadWrite<Physical>()
      }
    });

    springQuery = GetEntityQuery ( new EntityQueryDesc {
      All = new [] {
        ComponentType.ReadOnly<EntityPair>(),
        ComponentType.ReadOnly<Elasticity>(),
        ComponentType.ReadOnly<Line>()
      }
    });
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    int massCount = massQuery.CalculateEntityCount();
    int springCount = springQuery.CalculateEntityCount();
    if (massCount == 0 || springCount == 0)
      return inputDeps;

    NativeMultiHashMap<Entity, float3> hashMap = new NativeMultiHashMap<Entity, float3>(massCount * 4, Allocator.TempJob);

    HashSpringForceJob hashMassSpringJob = new HashSpringForceJob {
      _hashMap = hashMap.AsParallelWriter()
    };

    JobHandle hashMassSPringHandle = hashMassSpringJob.Schedule(this, inputDeps);

    MassSpringForceJob massSpringForceJob = new MassSpringForceJob {
      _massSpringHashMap = hashMap
    };

    JobHandle massSpringForceHandle = massSpringForceJob.Schedule(this, hashMassSPringHandle);
    massSpringForceHandle.Complete();
    hashMap.Dispose();
    return massSpringForceHandle;
  }
}