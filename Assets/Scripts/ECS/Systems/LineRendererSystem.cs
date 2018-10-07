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
/// - Raul Vera 2018
/// </summary>
[UpdateAfter(typeof(PreLateUpdate.ParticleSystemBeginUpdateAll))]
[UpdateAfter(typeof(MeshCullingBarrier))]
public class LineRendererSystem : ComponentSystem {
  List<LineRenderer> rendererList = new List<LineRenderer>();
  private ComponentGroup _dependency;
  private int gridLayer;

  protected override void OnCreateManager(int capacity) {
    _dependency = GetComponentGroup(
      typeof(Line), typeof(LineRenderer)
    );
    gridLayer = LayerMask.NameToLayer("Grid");
  }

  protected override void OnUpdate() {
    Matrix4x4 identityMatrix = UnityEngine.Matrix4x4.identity;
    EntityManager.GetAllUniqueSharedComponentDatas<LineRenderer>(rendererList);
    for (int i = 0; i < rendererList.Count; ++i) {
      LineRenderer render = rendererList[i];
      Mesh mesh = render.WorkMesh;

      if (mesh == null) continue;

      var meshIsReady = mesh.vertexCount > 0;

      mesh.vertices = render.Vertices;
      mesh.normals = render.Normals;

      UnityEngine.Graphics.DrawMesh(
        mesh, identityMatrix, render.Material, gridLayer
      );
    }
    rendererList.Clear();
  }

}