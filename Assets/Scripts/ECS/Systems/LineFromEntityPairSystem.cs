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
/// - Raul Vera 2018
/// </summary>
public class LineFromEntityPairSystem : JobComponentSystem {
  public struct Data {
    public readonly int Length;
    public ComponentDataArray<Line> lines;
    [ReadOnly] public ComponentDataArray<EntityPair> entityPairs;
  }

  [Inject] private Data _data;
  [Inject][ReadOnly] private ComponentDataFromEntity<Position> _positions;

  [BurstCompile]
  struct LineFromEntitiesJob : IJobParallelFor {
    public ComponentDataArray<Line> _lines;
    [ReadOnly] public ComponentDataArray<EntityPair> _entityPairs;
    [ReadOnly] public ComponentDataFromEntity<Position> _positions;

    public void Execute(int i) {
      _lines[i] = new Line {
        P1 = _positions[_entityPairs[i].E1].Value,
        P2 = _positions[_entityPairs[i].E2].Value
      };
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    if (_data.Length == 0)
      return inputDeps;

    LineFromEntitiesJob job = new LineFromEntitiesJob {
      _lines = _data.lines,
      _entityPairs = _data.entityPairs,
      _positions = _positions
    };

    JobHandle jobHandle = job.Schedule(_data.Length, 64, inputDeps);
    return jobHandle;
  }
}