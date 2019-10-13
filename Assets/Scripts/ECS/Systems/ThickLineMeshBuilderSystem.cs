using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// This system prepares each of the line meshes in the grid before rendering them.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateBefore(typeof(DynamicMeshBuilderSystem))]
public class ThickLineMeshBuilderSystem : JobComponentSystem {
  private EntityQuery lineQuery;
  private EntityQuery meshQuery;
  private List<LineRendererRef> lineRendererRefs = new List<LineRendererRef>();

  unsafe struct LineMeshBuilderJob : IJobForEachWithEntity<Thickness, Line> {
    [NativeDisableParallelForRestriction] 
    public DynamicBuffer<BufferableVertex> _vertexBuffer;

    public NativeCounter.Concurrent _counter;

    public void Execute(Entity entity, int index, [ReadOnly] ref Thickness thickness, [ReadOnly] ref Line line) {
      float3 dist = line.P2 - line.P1;
      float3 perp = new float3(-dist.z, dist.y, dist.x);
      perp = math.normalizesafe(perp) * 0.5f * thickness.Value;

      float3 p0 = line.P1 - perp;
      float3 p1 = line.P1 + perp;
      float3 p2 = line.P2 - perp;
      float3 p3 = line.P2 + perp;

      index *= 4;
      if (index+4 > _vertexBuffer.Length) return;

      void* verticesPtr = _vertexBuffer.GetUnsafePtr();
      UnsafeUtility.WriteArrayElement(verticesPtr, index + 0, p0);
      UnsafeUtility.WriteArrayElement(verticesPtr, index + 1, p1);
      UnsafeUtility.WriteArrayElement(verticesPtr, index + 2, p2);
      UnsafeUtility.WriteArrayElement(verticesPtr, index + 3, p3);
    }
  }

  protected override void OnCreate() {
    lineQuery = GetEntityQuery(
      typeof(LineRendererRef), typeof(Line), typeof(Thickness)
    );
    
    meshQuery = GetEntityQuery (
      typeof(RenderMesh), typeof(BufferableVertex)
    );
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    EntityManager.GetAllUniqueSharedComponentData<LineRendererRef>(lineRendererRefs);
    NativeCounter lineCounter = new NativeCounter(Allocator.TempJob);
    
    var vertexBuffers = GetBufferFromEntity<BufferableVertex>();

    for (int i = 0; i < lineRendererRefs.Count; ++i) {
      Entity lineRendererEntity = lineRendererRefs[i].Value;
      LineRendererRef rendererRef = lineRendererRefs[i];

      lineQuery.SetFilter(rendererRef);
      int groupCount = lineQuery.CalculateEntityCount();
      if (groupCount == 0) continue;

      lineCounter.Count = 0;
      LineMeshBuilderJob job = new LineMeshBuilderJob {
        _counter = lineCounter,
        _vertexBuffer = (vertexBuffers[lineRendererEntity]),
      };
      inputDeps = job.Schedule(this, inputDeps);
      inputDeps.Complete();
    }

    lineRendererRefs.Clear();
    lineCounter.Dispose();
    return inputDeps;
  }

}