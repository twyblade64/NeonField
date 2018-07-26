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

[UpdateBefore(typeof(GridRenderSystem))]
[UpdateAfter(typeof(VelocityMovementSystem))]
public class GridMeshBuilderSystem : JobComponentSystem {
  /*public struct Data {
    public readonly int Length;
    public SharedComponentDataArray<GridRender> gridRenderers;
    [ReadOnly] public ComponentDataArray<Position> positions;
    [ReadOnly] public ComponentDataArray<GridPosition> gridPositions;
  }

  [Inject] private Data m_Data;*/

  private ComponentGroup group;
  private List<GridRender> renderers = new List<GridRender>();

  unsafe struct GridMeshBuilderJob : IJobParallelFor {
    [ReadOnly] int2 _gridSize;
    [ReadOnly] ComponentDataArray<Position> _positions;
    [ReadOnly] ComponentDataArray<GridPosition> _gridPositions;
    [NativeDisableUnsafePtrRestriction] void * _vertices;
    [NativeDisableUnsafePtrRestriction] void * _normals;

    public void Initialize(int2 gridSize, ComponentGroup group, Vector3[] vertices, Vector3[] normals) {
      _gridSize = gridSize;
      _positions = group.GetComponentDataArray<Position>();
      _gridPositions = group.GetComponentDataArray<GridPosition>();
      _vertices = UnsafeUtility.AddressOf(ref vertices[0]);
      _normals = UnsafeUtility.AddressOf(ref normals[0]);
    }

    public void Execute(int i) {
      int2 gridPos = _gridPositions[i].Value;
      Vector3 spacePos = _positions[i].Value;
      int index = gridPos.x + gridPos.y * _gridSize.x;
      UnsafeUtility.WriteArrayElement(_vertices, index, spacePos);
      UnsafeUtility.WriteArrayElement(_normals, index, Vector3.up);
    }
  }

  protected override void OnCreateManager(int capacity) {
    group = GetComponentGroup(
      typeof(GridRender), typeof(Position), typeof(GridPosition)
    );
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    /*if (m_Data.Length == 0)
      return inputDeps;*/

    EntityManager.GetAllUniqueSharedComponentDatas(renderers);

    GridMeshBuilderJob job = new GridMeshBuilderJob();
    for (int i = 0; i < renderers.Count; ++i) {
      GridRender renderer = renderers[i];

      group.SetFilter(renderer);
      int groupCount = group.CalculateLength();
      if (groupCount == 0) continue;

      job.Initialize(renderer.Size, group, renderer.Vertices, renderer.Normals);
      inputDeps = job.Schedule(groupCount, 8, inputDeps);
    }
    /*GridMeshBuilderJob job = new GridMeshBuilderJob {
      gridRenderers = m_Data.gridRenderers,
      positions = m_Data.positions,
      gridPositions = m_Data.gridPositions
    };*/
    renderers.Clear();

    return inputDeps;
  }

}