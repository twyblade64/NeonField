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
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Rendering;

[UpdateBefore(typeof(LineRendererSystem))]
[UpdateAfter(typeof(LineFromEntitiesSystem))]
public class LineMeshBuilderSystem : JobComponentSystem {
  private struct Dependencies {
    [ReadOnly] public SharedComponentDataArray<LineRenderer> _LineRenderers;
  }
  [Inject] Dependencies _dependencies;

  private ComponentGroup group;
  private List<LineRenderer> lineRenderers = new List<LineRenderer>();

  unsafe struct LineMeshBuilderJob : IJobParallelFor {
    [ReadOnly] ComponentDataArray<Spring> _lines;
    [NativeDisableUnsafePtrRestriction] void * _vertices;
    [NativeDisableUnsafePtrRestriction] void * _normals;

    NativeCounter.Concurrent _counter;

    public void Initialize(ComponentGroup group, Vector3[] vertices, Vector3[] normals, NativeCounter.Concurrent counter) {
      _lines = group.GetComponentDataArray<Spring>();
      _vertices = UnsafeUtility.AddressOf(ref vertices[0]);
      _normals = UnsafeUtility.AddressOf(ref normals[0]);
      _counter = counter;
    }

    public void Execute(int i) {

      float3 perp = math.normalize(math.cross((_lines[i].p2 - _lines[i].p1), new float3(0,1,0))) * 0.5f * _lines[i].width;
      float3 p0 = _lines[i].p1 - perp;
      float3 p1 = _lines[i].p1 + perp;
      float3 p2 = _lines[i].p2 - perp;
      float3 p3 = _lines[i].p2 + perp;
      
      int vertIndex = _counter.Increment() * 4;

      UnsafeUtility.WriteArrayElement(_vertices, vertIndex + 0, (Vector3) p0);
      UnsafeUtility.WriteArrayElement(_vertices, vertIndex + 1, (Vector3) p1);
      UnsafeUtility.WriteArrayElement(_vertices, vertIndex + 2, (Vector3) p2);
      UnsafeUtility.WriteArrayElement(_vertices, vertIndex + 3, (Vector3) p3);

      UnsafeUtility.WriteArrayElement(_normals, vertIndex + 0, Vector3.up);
      UnsafeUtility.WriteArrayElement(_normals, vertIndex + 1, Vector3.up);
      UnsafeUtility.WriteArrayElement(_normals, vertIndex + 2, Vector3.up);
      UnsafeUtility.WriteArrayElement(_normals, vertIndex + 3, Vector3.up);
    }
  }

  protected override void OnCreateManager(int capacity) {
    group = GetComponentGroup(
      typeof(LineRenderer), typeof(Spring)
    );
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    EntityManager.GetAllUniqueSharedComponentDatas(lineRenderers);

    LineMeshBuilderJob job = new LineMeshBuilderJob();
    for (int i = 0; i < lineRenderers.Count; ++i) {
      LineRenderer renderer = lineRenderers[i];

      group.SetFilter(renderer);
      int groupCount = group.CalculateLength();
      if (groupCount == 0) continue;

      job.Initialize(group, renderer.Vertices, renderer.Normals, renderer.ConcurrentCounter);
      inputDeps = job.Schedule(groupCount, 8, inputDeps);
      inputDeps.Complete();
      renderer.Counter.Count = 0;
    }
    lineRenderers.Clear();
    return inputDeps;
  }

}