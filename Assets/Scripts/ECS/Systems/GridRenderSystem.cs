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
public class GridRenderSystem : ComponentSystem {
  public struct Data {
    public readonly int Length;
    [ReadOnly] public SharedComponentDataArray<GridRender> gridRender;
  }

  [Inject] private Data m_Data;

  List<GridRender> gridRenderers = new List<GridRender>();
  private ComponentGroup dependency;
  private int gridLayer;

  protected override void OnCreateManager(int capacity) {
    dependency = GetComponentGroup(
      typeof(GridRender)
    );
    gridLayer = LayerMask.NameToLayer("Grid");
  }

  protected override void OnUpdate() {
    Matrix4x4 identityMatrix = UnityEngine.Matrix4x4.identity;
    EntityManager.GetAllUniqueSharedComponentDatas<GridRender>(gridRenderers);
    for (int i = 0; i < gridRenderers.Count; ++i) {
      GridRender render = gridRenderers[i];
      Mesh mesh = render.WorkMesh;

      if (mesh == null) continue;

      var meshIsReady = mesh.vertexCount > 0;

      mesh.vertices = render.Vertices;
      mesh.normals = render.Normals; 

      if (mesh.triangles.Length == 0) {
        Debug.Log("Creating tris!");
        int xNodes = render.Size.x;
        int yNodes = render.Size.y;
        int[] tris = new int[(xNodes-1)*(yNodes-1)*2*3];
        int index = 0;
        for (int y = 0; y < yNodes-1; ++y) {
          for (int x = 0; x < xNodes-1; ++x) {
            tris[index+0] = (x  )+(y  )*xNodes;
            tris[index+1] = (x  )+(y+1)*xNodes;
            tris[index+2] = (x+1)+(y+1)*xNodes;

            tris[index+3] = (x  )+(y  )*xNodes;
            tris[index+4] = (x+1)+(y+1)*xNodes;
            tris[index+5] = (x+1)+(y  )*xNodes;
            index += 6;
          }
        }
        mesh.triangles = tris;

        Vector2[] uv = new Vector2[xNodes*yNodes];
        for (int y = 0; y < yNodes; ++y) {
          for (int x = 0; x < xNodes; ++x) {
            uv[x+y*xNodes] = new Vector2(1f*x/(xNodes-1),1f*y/(yNodes-1));
          }
        }
        mesh.uv = uv;
      }

      //Debug.Log("Data: "+ mesh.vertices[50] +" "+mesh.normals[50]+" "+mesh.triangles[50]);

      // Draw call
      UnityEngine.Graphics.DrawMesh(
          mesh, identityMatrix, render.Material, gridLayer
      );
    }
    gridRenderers.Clear();
  }

}