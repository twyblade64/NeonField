using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System used to generate the interactions betwen force generators and recievers.
/// 
/// Recievers in this case are the grid nodes, and generators are explosions and stuff.
/// Since the ForceGenerator component is defined with no direction data, forces are
/// assumed to be radially generated.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class ForceInfluenceSystem : JobComponentSystem {

  [BurstCompile]
  struct ForceInfluenceJob : IJobForEach<Physical, Translation> {
    [DeallocateOnJobCompletionAttribute]
    [ReadOnly] public NativeArray<LocalToWorld> generatorPositions;
    [DeallocateOnJobCompletionAttribute]
    [ReadOnly] public NativeArray<ForceGenerator> generatorForces;
    [ReadOnly] public int generatorCount;

    public void Execute(ref Physical physical, [ReadOnly] ref Translation position) {
      float3 forceSum = new float3(0, 0, 0);

      for (int j = 0; j < generatorCount; ++j) {
        float3 generatorPosition = generatorPositions[j].Position;
        ForceGenerator forceGenerator = generatorForces[j];

        float3 distance = position.Value - generatorPosition;
        float distanceMagSqr = math.lengthsq(distance);
        if (distanceMagSqr < forceGenerator.distance * forceGenerator.distance) {

          // Linear decay over distance
          float f = 1 - math.sqrt(distanceMagSqr) / forceGenerator.distance;
          forceSum += math.normalize(distance) * f * forceGenerator.force;
        }
      }

      physical = new Physical {
        Force = physical.Force + forceSum,
        InverseMass = physical.InverseMass
      };

    }
  }

  EntityQuery _forceGeneratorsQuery;
  EntityQuery _physicalQuery;

  protected override void OnCreate() {
    _forceGeneratorsQuery = GetEntityQuery(new EntityQueryDesc {
      All = new [] {
        ComponentType.ReadOnly<LocalToWorld>(),
        ComponentType.ReadOnly<ForceGenerator>()
      }
    });

    _physicalQuery = GetEntityQuery(new EntityQueryDesc {
      All = new [] {
        ComponentType.ReadWrite<Physical>(),
        ComponentType.ReadOnly<Translation>()
      }
    });

  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    var generatorPositions = _forceGeneratorsQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
    var generatorForces = _forceGeneratorsQuery.ToComponentDataArray<ForceGenerator>(Allocator.TempJob);

    ForceInfluenceJob job = new ForceInfluenceJob {
      generatorPositions = generatorPositions,
      generatorForces = generatorForces,
      generatorCount = generatorPositions.Length
    };

    JobHandle jobHandle = job.Schedule(_physicalQuery, inputDeps);
    return jobHandle;
  }
}