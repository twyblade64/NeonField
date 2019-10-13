using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// The bootstrap class for the grid ecs systems and components.
/// This bootstrap creates the grid using the modulus operator for each
/// node and spring, so that the grid is divided in 'sections' which
/// will also be divided in 'sections'. Each section division has its own
/// parameters for the node and spding entities.
/// 
/// - Raúl Vera Ortega 2018
/// </summary>
public sealed class RecursiveGridBootstrap : MonoBehaviour {
  /// <summary>
  /// The settings used to create nodes and springs.
  /// </summary>
  [Serializable]
  public struct NodeSettings {
    public float elasticity;
    public float damp;
    public float width;
    public float maxSpeed;
  }

  [Serializable]
  public struct RecursionSetting {
    /// The frecuency of the section division. Must be at least (1,1).
    public Vector2Int frequency;

    /// The settings for this section division.
    public NodeSettings nodeSettings;
  }

  /// <summary>
  /// The space that the nodes will cover.
  /// </summary>
  public Rect nodeField;

  /// <summary>
  /// The ammount of node columns.
  /// </summary>
  public int xNodes;

  /// <summary>
  /// The ammount of node rows.
  /// </summary>
  public int yNodes;

  /// <summary>
  /// The material used to render each line conection nodes.
  /// </summary>
  public Material lineMaterial;

  /// <summary>
  /// The divisions to be created in the grid.
  /// Diisions are applied from first to last.
  /// </summary>
  public List<RecursionSetting> recursions;

  /// <summary>
  /// Minimum/Default division settings.
  /// </summary>
  public NodeSettings defaultSetting;

  /// <summary>
  /// Archetype of the grid nodes.
  /// </summary>
  public static EntityArchetype NodeArchetype;

  /// <summary>
  /// Archetype of the grid springs.
  /// </summary>
  public static EntityArchetype SpringArchetype;

  public static EntityArchetype LineRendererArchetype;

  /// <summary>
  /// Initialize archetypes
  /// </summary>
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  public static void Initialize() {
    var entityManager = World.Active.EntityManager;

    // The node archetype contains the information of a node in space.
    NodeArchetype = entityManager.CreateArchetype(
      typeof(Translation), typeof(Velocity), typeof(MaxSpeed),
      typeof(Physical), typeof(Damper), typeof(FreezeAxis)
    );

    // The spring archetype contains the reference to two nodes and the spring forces in them.
    // Also used to render the line between them
    SpringArchetype = entityManager.CreateArchetype(
      typeof(Line), typeof(EntityPair),
      typeof(Elasticity), typeof(Thickness), typeof(LineRendererRef)
    );

    LineRendererArchetype = entityManager.CreateArchetype (
      typeof(RenderMesh), typeof(BufferableVertex), typeof(CustomRenderTag)
    );
  }

  public void Start() {
    // Lets add the deafult setting at the end of the list
    recursions.Add(new RecursionSetting { frequency = new Vector2Int(1, 1), nodeSettings = defaultSetting });

    EntityManager entityManager = World.Active.EntityManager;

    // Create node entities.
    NativeArray<Entity> nodeEntities = new NativeArray<Entity>(xNodes * yNodes, Allocator.Temp);
    entityManager.CreateEntity(NodeArchetype, nodeEntities);

    // Create spring entities. 
    // There are 2 springs for any node that is not on the right or bottom borders
    // and 1 for those that are, so we remove the extra springs
    NativeArray<Entity> springEntities = new NativeArray<Entity>(2 * xNodes * yNodes - xNodes - yNodes, Allocator.Temp);
    entityManager.CreateEntity(SpringArchetype, springEntities);

    // TODO Document
    var gridEntity = entityManager.CreateEntity(LineRendererArchetype);

    var vertexBuffer = entityManager.GetBuffer<BufferableVertex>(gridEntity);
    vertexBuffer.ResizeUninitialized((2 * xNodes * yNodes - xNodes - yNodes)*4);
    var renderMesh = new RenderMesh {
      receiveShadows = false,
      castShadows = ShadowCastingMode.Off,
      layer = 0,
      material = lineMaterial,
      mesh = new Mesh(),
      subMesh = 0
    };

    // Quad: 
    // 3 ════ 2
    // ║ \\   ║
    // ║   \\ ║
    // 1 ════ 0
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

    renderMesh.mesh.indexFormat = IndexFormat.UInt32;
    renderMesh.mesh.vertices = new Vector3[vertexBuffer.Length];
    renderMesh.mesh.normals = new Vector3[vertexBuffer.Length];
    renderMesh.mesh.uv = lineUV;
    renderMesh.mesh.triangles = lineTris;
    renderMesh.mesh.MarkDynamic();
    renderMesh.mesh.bounds = new Bounds(Vector3.zero, new Vector3(nodeField.x * 1.5f, 10, nodeField.y * 1.5f));

    entityManager.SetSharedComponentData(gridEntity, renderMesh);

    //-- Initializing each node and spring data
    // Since we sometimes setup 2 springs and sometimes 1, it's a bit hard to keep track
    // of the current spring, so lets use a counter.
    int springIndex = 0;

    // Base spring length. A bit tensed up.
    float hSpringLength = (nodeField.width / xNodes) * 0.95f;
    float vSpringLength = (nodeField.height / yNodes) * 0.95f;

    // Set each node and spring data
    for (int i = 0; i < nodeEntities.Length; ++i) {
      //---- Nodes
      // Node grid position
      int nX = i % xNodes;
      int nY = i / xNodes;

      // Get the position in WorldSpace of the node
      float pX = 1f * nX / (xNodes - 1) * nodeField.width + nodeField.x;
      float pY = 1f * nY / (yNodes - 1) * nodeField.height + nodeField.y;

      // Node configuration
      NodeSettings settings = recursions[GetRecursionLevel(nX, nY)].nodeSettings;

      // Node Position
      entityManager.SetComponentData(nodeEntities[i], new Translation { Value = new float3(pX, 0, pY) });

      // Node Velocity
      entityManager.SetComponentData(nodeEntities[i], new Velocity { Value = new float3(0, 0, 0) });

      // Node Max Speed
      entityManager.SetComponentData(nodeEntities[i], new MaxSpeed { Value = settings.maxSpeed });

      // Node Damper
      entityManager.SetComponentData(nodeEntities[i], new Damper { Value = settings.damp });

      // Disable node movement in Y axis
      entityManager.SetComponentData(nodeEntities[i], new FreezeAxis { FreezeMask = FreezeAxis.AxisMask.Y, FreezePos = new float3(0, 0, 0) });

      // Inmovable border nodes
      if (i % xNodes == 0 || i / xNodes == 0 || i % xNodes == xNodes - 1 || i / xNodes == yNodes - 1)
        entityManager.SetComponentData(nodeEntities[i], new Physical { Force = new float3(0, 0, 0), InverseMass = 0f });
      else
        entityManager.SetComponentData(nodeEntities[i], new Physical { Force = new float3(0, 0, 0), InverseMass = 1f });

      //---- Springs
      // Horizontal springs. No horizontal spring on final column.
      if (i % xNodes != xNodes - 1) {
        NodeSettings lineSettings = recursions[Math.Max(GetRecursionLevel(nX, nY), GetRecursionLevel(nX + 1, nY))].nodeSettings;

        // Spring component. Spring position will be updated in system, so no need to do it here. Use base horizontal spring length.
        entityManager.SetComponentData(springEntities[springIndex], new Line { P1 = new float3(0, 0, 0), P2 = new float3(0, 0, 0) });

        // Setup node references
        entityManager.SetComponentData(springEntities[springIndex], new EntityPair { E1 = nodeEntities[i], E2 = nodeEntities[i + 1] });

        // Spring elasticity
        entityManager.SetComponentData(springEntities[springIndex], new Elasticity { YoungModulus = lineSettings.elasticity, ReferenceLength = hSpringLength });

        // Springthickness
        entityManager.SetComponentData(springEntities[springIndex], new Thickness() { Value = lineSettings.width });

        // Spring lineRenderer reference
        entityManager.SetSharedComponentData(springEntities[springIndex], new LineRendererRef { Value = gridEntity } );

        // Increase spring counter
        ++springIndex;
      }

      // Vertical springs. No vertical spring on final row.
      if (i / xNodes != yNodes - 1) {
        NodeSettings lineSettings = recursions[Math.Max(GetRecursionLevel(nX, nY), GetRecursionLevel(nX, nY+1))].nodeSettings;

        // Spring component. Spring position will be updated in system, so no need to do it here. Use base vertical spring length.
        entityManager.SetComponentData(springEntities[springIndex], new Line { P1 = new float3(0, 0, 0), P2 = new float3(0, 0, 0) });

        // Setup node references
        entityManager.SetComponentData(springEntities[springIndex], new EntityPair { E1 = nodeEntities[i], E2 = nodeEntities[i + xNodes] });

        // Spring elasticity
        entityManager.SetComponentData(springEntities[springIndex], new Elasticity { YoungModulus = lineSettings.elasticity, ReferenceLength = vSpringLength });

        // Springthickness
        entityManager.SetComponentData(springEntities[springIndex], new Thickness() { Value = lineSettings.width });

        // Spring lineRenderer reference
        entityManager.SetSharedComponentData(springEntities[springIndex], new LineRendererRef { Value = gridEntity } );

        // Increase spring counter
        ++springIndex;
      }
    }

    // Dispose of temporal arrays
    springEntities.Dispose();
    nodeEntities.Dispose();
  }

  int GetRecursionLevel(int x, int y) {
    for (int i = 0; i < recursions.Count; ++i) {
      x %= recursions[i].frequency.x;
      y %= recursions[i].frequency.y;
      if (x == 0 || y == 0)
        return i;
    }
    return -1;
  }
}