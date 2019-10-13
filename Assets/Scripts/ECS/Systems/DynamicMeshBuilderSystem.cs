using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Rendering;
using System.Buffers;

/// <summary>
/// This system renders all the lines meshes.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class DynamicMeshBuilderSystem : JobComponentSystem {
  List<RenderMesh> rendererList = new List<RenderMesh>();
  private EntityQuery _meshQuery;
  private int gridLayer;

  ArrayPool<Vector3> vectorPool = ArrayPool<Vector3>.Create(512000,8);

  protected override void OnCreate() {
    _meshQuery = GetEntityQuery(
      typeof(RenderMesh), typeof(BufferableVertex)
    );
    gridLayer = LayerMask.NameToLayer("Grid");
  }

  protected unsafe override JobHandle OnUpdate(JobHandle inputDeps) {
    EntityManager.GetAllUniqueSharedComponentData<RenderMesh>(rendererList);
    var vertexBuffers = GetBufferFromEntity<BufferableVertex>();

    for (int i = 0; i < rendererList.Count; ++i) {
      RenderMesh renderMesh = rendererList[i];
      // if (renderMesh.mesh == null) continue;

      _meshQuery.SetFilter(renderMesh);
      var entities = _meshQuery.ToEntityArray(Allocator.TempJob);

      for (int j = 0; j < entities.Length; ++j){
        var entity = entities[j];
        var vertexBuffer = vertexBuffers[entity];
        var meshVerts = vertexBuffers[entity].Length;
        var vector3Buffer = vectorPool.Rent(meshVerts);

        UnsafeUtility.MemCpy(UnsafeUtility.AddressOf(ref vector3Buffer[0]), vertexBuffer.Reinterpret<Vector3>().GetUnsafePtr(), meshVerts * 12);
        renderMesh.mesh.vertices = vector3Buffer;

        vectorPool.Return(vector3Buffer);
      }

      entities.Dispose();
    }

    rendererList.Clear();
    return inputDeps;
  }

}