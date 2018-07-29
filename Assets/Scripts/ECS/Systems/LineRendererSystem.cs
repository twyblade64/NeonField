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
using System.Collections.Generic;
using System.Text;

[UpdateAfter(typeof(PreLateUpdate.ParticleSystemBeginUpdateAll))]
[UpdateAfter(typeof(MeshCullingBarrier))]
//[UnityEngine.ExecuteInEditMode]
public class LineRendererSystem : ComponentSystem {
  List<LineRenderer> rendererList = new List<LineRenderer>();
  private ComponentGroup _dependency;
  private int gridLayer;

  protected override void OnCreateManager(int capacity) {
    _dependency = GetComponentGroup(
       typeof(Spring), typeof(LineRenderer)
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

      render.Counter.Count = 0;
    }
    rendererList.Clear();
  }

}