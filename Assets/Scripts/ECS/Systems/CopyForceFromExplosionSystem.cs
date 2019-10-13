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

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(ForceInfluenceSystem))]
public class CopyForceFromExplosionSystem : ComponentSystem {
  // [SerializeField] ForceExplosionController forceExplosionController;
  protected override void OnUpdate() {
    Entities.ForEach((Entity entity, ref ForceGenerator forceGenerator, ref CopyForceFromExplosion copyForceFromExplosion) => {
      var forceExplosionController = EntityManager.GetComponentObject<ForceExplosionController>(entity);
      forceGenerator = new ForceGenerator {
        force = forceExplosionController.force,
        distance = forceExplosionController.distance
      };
    });
  }
}