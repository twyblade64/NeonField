using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

/// <summary>
/// The bootstrap class for the grid ecs systems and components.
/// 
/// - Raul Vera 2018
/// </summary>
public sealed class NormalGridBootstrap : MonoBehaviour {
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
  /// The force a node exerts over others when beign pulled.
  /// </summary>
  public float nodeElasticity;

  /// <summary>
  /// Removal of overall energy in the system.
  /// </summary>
  public float nodeDamp;

  /// <summary>
  /// Width of the line rendered between nodes.
  /// </summary>
  public float nodeWidth;

  /// <summary>
  /// Limit the speed of the moving nodes.
  /// </summary>
  public float nodeMaxSpeed;

  /// <summary>
  /// The material used to render each line conection nodes.
  /// </summary>
  public Material lineMaterial;

  /// <summary>
  /// Archetype of the grid nodes.
  /// </summary>
  public static EntityArchetype NodeArchetype;
  
  /// <summary>
  /// Archetype of the grid springs.
  /// </summary>
  public static EntityArchetype SpringArchetype;

  /// <summary>
  /// Initialize archetypes
  /// </summary>
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  public static void Initialize() {
    var entityManager = World.Active.GetOrCreateManager<EntityManager>();

    // The node archetype contains the information of a node in space.
    NodeArchetype = entityManager.CreateArchetype(
      typeof(Position), typeof(Velocity), typeof(MaxSpeed),
      typeof(Physical), typeof(Damper), typeof(FreezeAxis)
    );

    // The spring archetype contains the reference to two nodes and the spring forces in them.
    // Also used to render the line between them
    SpringArchetype = entityManager.CreateArchetype(
      typeof(Line), typeof(EntityPair),
      typeof(Elasticity), typeof(Thickness), typeof(LineRenderer)
    );
  }

  public void Start() {
    EntityManager entityManager = World.Active.GetOrCreateManager<EntityManager>();

    // Create node entities.
    NativeArray<Entity> nodeEntities = new NativeArray<Entity>(xNodes * yNodes, Allocator.Temp);
    entityManager.CreateEntity(NodeArchetype, nodeEntities);

    // Create spring entities. 
    // There are 2 springs for any node that is not on the right or bottom borders
    // and 1 for those that are, so we remove the extra springs
    NativeArray<Entity> springEntities = new NativeArray<Entity>(2 * xNodes * yNodes - xNodes - yNodes, Allocator.Temp);
    entityManager.CreateEntity(SpringArchetype, springEntities);

    // Setup a shared lineRenderer component, containing the mesh information of all the grid.
    // For each spring well have 4 vertices and normals
    LineRenderer lineRenderer = new LineRenderer() {
      WorkMesh = new Mesh(),
      Material = lineMaterial,
      Vertices = new Vector3[(2 * xNodes * yNodes - xNodes - yNodes)*4],
      Normals = new Vector3[(2 * xNodes * yNodes - xNodes - yNodes)*4]
    };

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
      // Get the position in WorldSpace of the node
      float pX = 1f * (i % xNodes) / (xNodes - 1) * nodeField.width + nodeField.x;
      float pY = 1f * (i / xNodes) / (yNodes - 1) * nodeField.height + nodeField.y;

      // Node Position
      entityManager.SetComponentData(nodeEntities[i], new Position { Value = new float3(pX, 0, pY) });

      // Node Velocity
      entityManager.SetComponentData(nodeEntities[i], new Velocity { Value = new float3(0, 0, 0) });

      // Node Max Speed
      entityManager.SetComponentData(nodeEntities[i], new MaxSpeed { Value = nodeMaxSpeed });

      // Node Damper
      entityManager.SetComponentData(nodeEntities[i], new Damper { Value = nodeDamp });

      // Disable node movement in Y axis
      entityManager.SetComponentData(nodeEntities[i], new FreezeAxis { FreezeMask = FreezeAxis.AxisMask.Y, FreezePos = new float3(0,0,0) });

      // Inmovable border nodes
      if (i % xNodes == 0 || i / xNodes == 0 || i % xNodes == xNodes - 1 || i / xNodes == yNodes - 1)
        entityManager.SetComponentData(nodeEntities[i], new Physical { Force = new float3(0, 0, 0), InverseMass = 0f });
      else
        entityManager.SetComponentData(nodeEntities[i], new Physical { Force = new float3(0, 0, 0), InverseMass = 1f });

      //---- Springs
      // Horizontal springs. No horizontal spring on final column.
      if (i % xNodes != xNodes - 1) { 
        // Spring component. Spring position will be updated in system, so no need to do it here. Use base horizontal spring length.
        entityManager.SetComponentData(springEntities[springIndex], new Line { P1 = new float3(0, 0, 0), P2 = new float3(0, 0, 0)});

        // Setup node references
        entityManager.SetComponentData(springEntities[springIndex], new EntityPair { E1 = nodeEntities[i], E2 = nodeEntities[i + 1] });

        // Spring elasticity
        entityManager.SetComponentData(springEntities[springIndex], new Elasticity { YoungModulus = nodeElasticity, ReferenceLength = hSpringLength });

        // Springthickness
        entityManager.SetComponentData(springEntities[springIndex], new Thickness() { Value = nodeWidth});

        // Spring lineRenderer
        entityManager.SetSharedComponentData(springEntities[springIndex], lineRenderer);

        // Increase spring counter
        ++springIndex;
      }

      // Vertical springs. No vertical spring on final row.
      if (i / xNodes != yNodes - 1) { 
        // Spring component. Spring position will be updated in system, so no need to do it here. Use base vertical spring length.
        entityManager.SetComponentData(springEntities[springIndex], new Line { P1 = new float3(0, 0, 0), P2 = new float3(0, 0, 0)});
        
        // Setup node references
        entityManager.SetComponentData(springEntities[springIndex], new EntityPair { E1 = nodeEntities[i], E2 = nodeEntities[i + xNodes] });
        
        // Spring elasticity
        entityManager.SetComponentData(springEntities[springIndex], new Elasticity { YoungModulus = nodeElasticity, ReferenceLength = vSpringLength });

        // Springthickness
        entityManager.SetComponentData(springEntities[springIndex], new Thickness() { Value = nodeWidth});
        
        // Spring lineRenderer
        entityManager.SetSharedComponentData(springEntities[springIndex], lineRenderer);
        
        // Increase spring counter
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

    lineRenderer.WorkMesh.indexFormat = IndexFormat.UInt32;
    lineRenderer.WorkMesh.vertices = lineRenderer.Vertices;
    lineRenderer.WorkMesh.normals = lineRenderer.Normals;
    lineRenderer.WorkMesh.uv = lineUV;
    lineRenderer.WorkMesh.triangles = lineTris;
    lineRenderer.WorkMesh.MarkDynamic();
    lineRenderer.WorkMesh.bounds = new Bounds(Vector3.zero, new Vector3(nodeField.x * 1.5f, 10, nodeField.y * 1.5f));

    // Dispose of temporal arrays
    springEntities.Dispose();
    nodeEntities.Dispose();
  }

}