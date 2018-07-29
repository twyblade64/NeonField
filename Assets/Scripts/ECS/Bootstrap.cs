using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEngine;

public sealed class Bootstrap : MonoBehaviour {
  public Rect nodeField;
  public int xNodes;
  public int yNodes;
  public float startHeight;

  public Mesh nodeMesh;
  public Material nodeMaterial;
  public float nodeElasticity;
  public float nodeDrag;
  public float nodeWidth;
  public Material gridMaterial;

  public static EntityArchetype NodeArchetype;
  public static EntityArchetype SpringArchetype;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  public static void Initialize() {
    var entityManager = World.Active.GetOrCreateManager<EntityManager>();

    NodeArchetype = entityManager.CreateArchetype(
      typeof(Position), typeof(Velocity),
      typeof(Physical), typeof(Damper)
    );

    SpringArchetype = entityManager.CreateArchetype(
      typeof(Line), typeof(EntityPair),
      typeof(Elasticity), typeof(LineRenderer)
    );

    World.Active.GetExistingManager<MeshRenderSystem>().Enabled = false;
  }

  public void Start() {
    EntityManager entityManager = World.Active.GetOrCreateManager<EntityManager>();

    // Create nodes
    NativeArray<Entity> nodeEntities = new NativeArray<Entity>(xNodes * yNodes, Allocator.Temp);
    NativeArray<Entity> springEntities = new NativeArray<Entity>(2 * xNodes * yNodes - xNodes - yNodes, Allocator.Temp);
    entityManager.CreateEntity(NodeArchetype, nodeEntities);
    entityManager.CreateEntity(SpringArchetype, springEntities);

    MeshInstanceRenderer meshInstanceRenderer = new MeshInstanceRenderer {
      mesh = nodeMesh,
      material = nodeMaterial,
      subMesh = 0,
      receiveShadows = false,
      castShadows = UnityEngine.Rendering.ShadowCastingMode.Off
    };

    var counter = new NativeCounter(Allocator.Persistent);
    LineRenderer lineRenderer = new LineRenderer() {
      WorkMesh = new Mesh(),
      Material = nodeMaterial,
      Vertices = new Vector3[(2 * xNodes * yNodes - xNodes - yNodes)*4],
      Normals = new Vector3[(2 * xNodes * yNodes - xNodes - yNodes)*4],
      Counter = counter,
      ConcurrentCounter = counter
    };

    int springIndex = 0;
    for (int i = 0; i < nodeEntities.Length; ++i) {

      float pX = 1f * (i % xNodes) / (xNodes - 1) * nodeField.width + nodeField.x;
      float pY = 1f * (i / xNodes) / (yNodes - 1) * nodeField.height + nodeField.y;
      entityManager.SetComponentData(nodeEntities[i], new Position { Value = new float3(pX, (Mathf.Sin(2f * Mathf.PI * (i / xNodes) / 20) + Mathf.Cos(2f * Mathf.PI * (i % xNodes) / 20)) * startHeight, pY) });
      entityManager.SetComponentData(nodeEntities[i], new Velocity { Value = new float3(0, 0, 0) });
      if (i % xNodes == 0 || i / xNodes == 0 || i % xNodes == xNodes - 1 || i / xNodes == yNodes - 1)
        entityManager.SetComponentData(nodeEntities[i], new Physical { Force = new float3(0, 0, 0), InverseMass = 0f });
      else
        entityManager.SetComponentData(nodeEntities[i], new Physical { Force = new float3(0, 0, 0), InverseMass = 1f });

      entityManager.SetComponentData(nodeEntities[i], new Damper { Value = nodeDrag });

      // Springs
      if (i % xNodes != xNodes - 1) {
        entityManager.SetComponentData(springEntities[springIndex], new Line { p1 = new float3(0, 0, 0), p2 = new float3(0, 0, 0), width = nodeWidth });
        entityManager.SetComponentData(springEntities[springIndex], new EntityPair { E1 = nodeEntities[i], E2 = nodeEntities[i + 1] });
        entityManager.SetComponentData(springEntities[springIndex], new Elasticity { Value = nodeElasticity });
        entityManager.SetSharedComponentData(springEntities[springIndex], lineRenderer);
        ++springIndex;
      }
      if (i / xNodes != yNodes - 1) {
        entityManager.SetComponentData(springEntities[springIndex], new Line { p1 = new float3(0, 0, 0), p2 = new float3(0, 0, 0), width = nodeWidth });
        entityManager.SetComponentData(springEntities[springIndex], new EntityPair { E1 = nodeEntities[i], E2 = nodeEntities[i + xNodes] });
        entityManager.SetComponentData(springEntities[springIndex], new Elasticity { Value = nodeElasticity });
        entityManager.SetSharedComponentData(springEntities[springIndex], lineRenderer);
        ++springIndex;
      }
    }

    // Startup shared LineRenderer WorkMesh
    lineRenderer = entityManager.GetSharedComponentData<LineRenderer>(springEntities[0]);
    int[] lineTris = new int[(2 * xNodes * yNodes - xNodes - yNodes) * 2 * 3];
    int lineIndex = 0;
    for (int i = 0; i < lineTris.Length; i += 6) {
      lineTris[i + 0] = lineIndex * 4 + 0;
      lineTris[i + 1] = lineIndex * 4 + 1;
      lineTris[i + 2] = lineIndex * 4 + 3;

      lineTris[i + 3] = lineIndex * 4 + 0;
      lineTris[i + 4] = lineIndex * 4 + 3;
      lineTris[i + 5] = lineIndex * 4 + 2;
      ++lineIndex;
    }

    Vector2[] lineUV = new Vector2[(2 * xNodes * yNodes - xNodes - yNodes) * 4];
    for (int i = 0; i < lineUV.Length; i += 4) {
      lineUV[i + 0] = new Vector2(0f, 0f);
      lineUV[i + 1] = new Vector2(1f, 0f);
      lineUV[i + 2] = new Vector2(0f, 1f);
      lineUV[i + 3] = new Vector2(1f, 1f);
    }

    lineRenderer.WorkMesh.vertices = lineRenderer.Vertices;
    lineRenderer.WorkMesh.normals = lineRenderer.Normals;
    lineRenderer.WorkMesh.uv = lineUV;
    lineRenderer.WorkMesh.triangles = lineTris;
    lineRenderer.WorkMesh.MarkDynamic();

    springEntities.Dispose();
    nodeEntities.Dispose();
  }

}