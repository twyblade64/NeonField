using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class LineFromEntitiesSystem : JobComponentSystem {
  public struct Data {
    public readonly int Length;
    public ComponentDataArray<Line> lines;
    [ReadOnly] public ComponentDataArray<EntityPair> entityPairs;
  }

  [Inject] private Data _data;
  [Inject] [ReadOnly] private ComponentDataFromEntity<Position> _positions;

  [BurstCompile]
  struct LineFromEntitiesJob : IJobParallelFor {
    public ComponentDataArray<Line> _lines;
    [ReadOnly] public ComponentDataArray<EntityPair> _entityPairs;
    [ReadOnly] public ComponentDataFromEntity<Position> _positions;

    public void Execute(int i) {
      _lines[i] = new Line {
        p1 = _positions[_entityPairs[i].E1].Value,
        p2 = _positions[_entityPairs[i].E2].Value,
        width = _lines[i].width
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