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

/// <summary>
/// This system prepares each of the line meshes in the grid before rendering them.
/// 
/// - Raul Vera 2018
/// </summary>

public class ThickLineMeshBuilderSystem : JobComponentSystem {
  private struct Dependencies {
    [ReadOnly] public SharedComponentDataArray<LineRenderer> _LineRenderers;
  }

  [Inject] Dependencies _dependencies;

  private ComponentGroup group;
  private List<LineRenderer> lineRenderers = new List<LineRenderer>();

  unsafe struct LineMeshBuilderJob : IJobParallelFor {
    [ReadOnly] ComponentDataArray<Thickness> _thicknesses;
    [ReadOnly] ComponentDataArray<Line> _lines;
    [NativeDisableUnsafePtrRestriction] void * _vertices;
    [NativeDisableUnsafePtrRestriction] void * _normals;

    NativeCounter.Concurrent _counter;

    public void Initialize(ComponentGroup group, Vector3[] vertices, Vector3[] normals, NativeCounter.Concurrent counter) {
      _thicknesses = group.GetComponentDataArray<Thickness>();
      _lines = group.GetComponentDataArray<Line>();
      _vertices = UnsafeUtility.AddressOf(ref vertices[0]);
      _normals = UnsafeUtility.AddressOf(ref normals[0]);
      _counter = counter;
    }

    public void Execute(int i) {
      Line line = _lines[i];
      float3 dist = line.P2 - line.P1;
      float3 perp = new float3(-dist.z, dist.y, dist.x);
      perp = math.normalize(perp) * 0.5f * _thicknesses[i].Value;

      float3 p0 = line.P1 - perp;
      float3 p1 = line.P1 + perp;
      float3 p2 = line.P2 - perp;
      float3 p3 = line.P2 + perp;

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

  protected override void OnCreateManager() {
    group = GetComponentGroup(
      typeof(LineRenderer), typeof(Line), typeof(Thickness)
    );
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    EntityManager.GetAllUniqueSharedComponentData<LineRenderer>(lineRenderers);

    NativeCounter lineCounter = new NativeCounter(Allocator.Temp);
    LineMeshBuilderJob job = new LineMeshBuilderJob();

    for (int i = 0; i < lineRenderers.Count; ++i) {
      LineRenderer renderer = lineRenderers[i];

      group.SetFilter(renderer);
      int groupCount = group.CalculateLength();
      if (groupCount == 0) continue;

      lineCounter.Count = 0;
      job.Initialize(group, renderer.Vertices, renderer.Normals, lineCounter);
      inputDeps = job.Schedule(groupCount, 8, inputDeps);
      inputDeps.Complete();
    }
    lineRenderers.Clear();
    lineCounter.Dispose();
    return inputDeps;
  }

}