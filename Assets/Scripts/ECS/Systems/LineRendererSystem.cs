using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Rendering;

/// <summary>
/// This system renders all the lines meshes.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(DynamicMeshBuilderSystem))]
public class LineRendererSystem : ComponentSystem {
  List<RenderMesh> _rendererList = new List<RenderMesh>();
  EntityQuery _dependencies;
  private int _gridLayer;

  protected override void OnCreate() {
    _gridLayer = LayerMask.NameToLayer("Grid");
    _dependencies = GetEntityQuery(
      typeof(RenderMesh), typeof(CustomRenderTag)
    );
  }

  protected override void OnUpdate() {
    Matrix4x4 identityMatrix = UnityEngine.Matrix4x4.identity;
    EntityManager.GetAllUniqueSharedComponentData<RenderMesh>(_rendererList);
    for (int i = 0; i < _rendererList.Count; ++i) {
      RenderMesh renderMesh = _rendererList[i];
      _dependencies.SetFilter(renderMesh);

      if (_dependencies.CalculateEntityCount() > 0) {
        Mesh mesh = _rendererList[i].mesh;
        if (mesh == null || mesh.vertexCount == 0) continue;

        Material material = _rendererList[i].material;
        UnityEngine.Graphics.DrawMesh(
          mesh, identityMatrix, material, _gridLayer
        );
      }
    }
    _rendererList.Clear();
  }

}