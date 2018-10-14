using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System used to create explosion forces from GameObjects.
/// GameObjects must have a ForceExplosionController and a CopyForceFromExplosion components
/// in order to be eligible to generate an explosion force.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>

[UpdateInGroup(typeof(PhysicUpdate))]
public class CopyForceFromExplosionSystem : ComponentSystem {
  public struct Data {
    public readonly int Length;
    public ComponentDataArray<ForceGenerator> forceGenerator;
    [ReadOnly] public ComponentArray<ForceExplosionController> forceExplosion;
    [ReadOnly] public ComponentDataArray<CopyForceFromExplosion> copyForceFromExplosion;
  }

  [Inject] private Data m_Data;

  protected override void OnUpdate() {
    for (int i = 0; i < m_Data.Length; ++i) {
      ForceExplosionController forceExplosionController = m_Data.forceExplosion[i];

      m_Data.forceGenerator[i] = new ForceGenerator {
        force = forceExplosionController.force,
        distance = forceExplosionController.distance
      };
    }
  }
}