using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(ForceInfluenceSystem))]
public class CopyForceFromExplosionSystem : ComponentSystem {
  public struct Data {
    public readonly int Length;
    public ComponentDataArray<ForceGenerator> forceGenerator;
    //public ComponentDataArray<Position> positions;
    //[ReadOnly] public ComponentArray<Transform> transform;
    [ReadOnly] public ComponentArray<ForceExplosionController> forceExplosion;
    [ReadOnly] public ComponentDataArray<CopyForceFromExplosion> copyForceFromExplosion;
  }

  [Inject] private Data m_Data;

  protected override void OnUpdate() {
    for (int i = 0; i < m_Data.Length; ++i) {
      //m_Data.positions[i] = new Position { Value = m_Data.transform[i].position };
      m_Data.forceGenerator[i] = new ForceGenerator {
        force = m_Data.forceExplosion[i].force,
        distance = m_Data.forceExplosion[i].distance
      };
    }
  }
}