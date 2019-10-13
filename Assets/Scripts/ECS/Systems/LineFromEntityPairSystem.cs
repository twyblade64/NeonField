using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// This system defines an line location using two entities and their positions.
/// It's used to build the spring between the grid nodes.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class LineFromEntityPairSystem : JobComponentSystem {
  [BurstCompile]
  struct LineFromEntitiesJob : IJobForEach<Line, EntityPair> {
    [ReadOnly] public ComponentDataFromEntity<Translation> _translations;

    public void Execute(ref Line line, ref EntityPair entityPair)
    {
      line = new Line {
        P1 = _translations[entityPair.E1].Value,
        P2 = _translations[entityPair.E2].Value,
      };
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    LineFromEntitiesJob job = new LineFromEntitiesJob {
      _translations = GetComponentDataFromEntity<Translation>()
    };

    return job.Schedule(this, inputDeps);
  }
}