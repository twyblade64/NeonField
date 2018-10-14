using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

/// <summary>
/// System used to generate the interactions betwen force generators and recievers.
/// 
/// Recievers in this case are the grid nodes, and generators are explosions and stuff.
/// Since the ForceGenerator component is defined with no direction data, forces are
/// assumed to be radially generated.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>

[UpdateInGroup(typeof(PhysicUpdate))]
[UpdateAfter(typeof(CopyForceFromExplosion))]
[UpdateAfter(typeof(CopyTransformToGameObject))]
public class ForceInfluenceSystem : JobComponentSystem {
  public struct ForceRecievers {
    public readonly int Length;
    public ComponentDataArray<Physical> physical;
    [ReadOnly] public ComponentDataArray<Position> position;
  }

  [Inject] private ForceRecievers forceRecievers;

  public struct ForceGenerators {
    public readonly int Length;
    [ReadOnly] public ComponentDataArray<Position> position;
    [ReadOnly] public ComponentDataArray<ForceGenerator> forceGenerator;
  }

  [Inject] private ForceGenerators forceGenerators;

  [BurstCompile]
  struct ForceInfluenceJob : IJobParallelFor {
    public ComponentDataArray<Physical> recieverPhysicals;
    [ReadOnly] public ComponentDataArray<Position> recieverPositions;
    [ReadOnly] public ComponentDataArray<Position> generatorPositions;
    [ReadOnly] public ComponentDataArray<ForceGenerator> generatorForces;
    [ReadOnly] public int generatorCount;

    public void Execute(int i) {
      Position position = recieverPositions[i];
      Physical physical = recieverPhysicals[i];

      float3 forceSum = new float3(0, 0, 0);

      for (int j = 0; j < generatorCount; ++j) {
        Position generatorPosition = generatorPositions[j];
        ForceGenerator forceGenerator = generatorForces[j];

        float3 distance = position.Value - generatorPosition.Value;
        float distanceMagSqr = math.lengthsq(distance);
        if (distanceMagSqr < forceGenerator.distance * forceGenerator.distance) {

          // Linear decay over distance
          float f = 1 - math.sqrt(distanceMagSqr) / forceGenerator.distance;
          forceSum += math.normalize(distance) * f * forceGenerator.force;
        }
      }

      recieverPhysicals[i] = new Physical {
        Force = physical.Force + forceSum,
        InverseMass = physical.InverseMass
      };
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    if (forceRecievers.Length == 0 || forceGenerators.Length == 0)
      return inputDeps;

    ForceInfluenceJob job = new ForceInfluenceJob {
      recieverPhysicals = forceRecievers.physical,
      recieverPositions = forceRecievers.position,
      generatorPositions = forceGenerators.position,
      generatorForces = forceGenerators.forceGenerator,
      generatorCount = forceGenerators.Length
    };

    JobHandle jobHandle = job.Schedule(forceRecievers.Length, 64, inputDeps);
    return jobHandle;
  }
}