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
      typeof(Position), typeof(Anchor), typeof(Velocity), 
      typeof(Physical), typeof(Damper), typeof(Elasticity),
      typeof(GridPosition), typeof(GridRender),
      typeof(TransformMatrix) //, typeof(MeshInstanceRenderer)
    );

    SpringArchetype = entityManager.CreateArchetype(
      typeof(Line), typeof(EntityPair), typeof(LineRenderer), 
      typeof(Elasticity), typeof(LineRendererSettings)
    );

    World.Active.GetExistingManager<MeshRenderSystem>().Enabled = false;
  }

  public void Start() {
    EntityManager entityManager = World.Active.GetOrCreateManager<EntityManager>();

    // Create nodes
    NativeArray<Entity> nodeEntities = new NativeArray<Entity>(xNodes * yNodes, Allocator.Temp);
    NativeArray<Entity> springEntities = new NativeArray<Entity>((xNodes-1)*yNodes + (yNodes-1)*xNodes, Allocator.Temp);
    entityManager.CreateEntity(NodeArchetype, nodeEntities);
    entityManager.CreateEntity(NodeArchetype, springEntities);

    
    MeshInstanceRenderer meshInstanceRenderer = new MeshInstanceRenderer {
      mesh = nodeMesh,
      material = nodeMaterial,
      subMesh = 0,
      receiveShadows = false,
      castShadows = UnityEngine.Rendering.ShadowCastingMode.Off
    };

    LineRendererSettings lineRenderSettings = new LineRendererSettings {
      Material = nodeMaterial
    };

    /*GridIdentifier gridIdentifier = new GridIdentifier {
      id = 0,
      size = new int2(xNodes, yNodes)
    };*/

    GridRender gridRender = new GridRender {
      //Material = (Material) Resources.Load("Assets/Resources/Materials/Line Material.mat", typeof(Material)),
      Material = gridMaterial,
      Width = nodeWidth,
      Size = new int2(xNodes, yNodes),
      Vertices = new Vector3[xNodes*yNodes],
      Normals = new Vector3[xNodes*yNodes],
      WorkMesh = new Mesh()
    };

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

    Vector2[] uv = new Vector2[xNodes*yNodes];
    for (int y = 0; y < yNodes; ++y) {
      for (int x = 0; x < xNodes; ++x) {
        uv[x+y*xNodes] = new Vector2(1f*x/(xNodes-1),1f*y/(yNodes-1));
      }
    }
    
    gridRender.WorkMesh.vertices = gridRender.Vertices;
    gridRender.WorkMesh.normals = gridRender.Normals;
    gridRender.WorkMesh.uv = uv;
    gridRender.WorkMesh.SetTriangles(tris,0);
    gridRender.WorkMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    gridRender.WorkMesh.MarkDynamic();

    int springIndex = 0;
    for (int i = 0; i < nodeEntities.Length; ++i) {
      
      float pX = 1f * (i%xNodes) / (xNodes-1) * nodeField.width + nodeField.x;
      float pY = 1f * (i/xNodes) / (yNodes-1) * nodeField.height + nodeField.y;
      entityManager.SetComponentData(nodeEntities[i], new Position { Value = new float3(pX, (Mathf.Sin(2f * Mathf.PI * (i/xNodes)/20 ) + Mathf.Cos(2f * Mathf.PI * (i%xNodes)/20 ))*startHeight ,pY)});
      entityManager.SetComponentData(nodeEntities[i], new Anchor { Value = new float3(pX, 0, pY)});
      entityManager.SetComponentData(nodeEntities[i], new Velocity { Value = new float3(0, 0, 0)});
      if (i%xNodes == 0 || i/xNodes == 0 || i%xNodes == xNodes-1 || i/xNodes == yNodes-1)
        entityManager.SetComponentData(nodeEntities[i], new Physical { Force = new float3(0, 0, 0), InverseMass = 0f});
      else
        entityManager.SetComponentData(nodeEntities[i], new Physical { Force = new float3(0, 0, 0), InverseMass = 1f});

      if (i%xNodes != xNodes-1) {
        Vector2[] lineUV = new Vector2[]{new Vector2(0,0),new Vector2(1,0),new Vector2(0,1),new Vector2(1,1)};
        int[] lineTris = new int[]{0,1,3,0,3,2};
        LineRenderer lineRenderer = new LineRenderer();
        lineRenderer.Vertices = new Vector3[4];
        lineRenderer.Normals = new Vector3[4];
        lineRenderer.WorkMesh.vertices = lineRenderer.Vertices;
        lineRenderer.WorkMesh.normals = lineRenderer.Normals;

        entityManager.SetComponentData(springEntities[springIndex], new Line { start = new float3(0, 0, 0), end = new float3(0, 0, 0), width = nodeWidth});
        entityManager.SetComponentData(springEntities[springIndex], new EntityPair { E1 = nodeEntities[i], E2 = nodeEntities[i+1] });
        entityManager.SetComponentData(springEntities[springIndex], lineRenderer);
        entityManager.SetComponentData(springEntities[springIndex], new Elasticity {Value = nodeElasticity});
        entityManager.SetSharedComponentData(springEntities[springIndex], lineRenderSettings);
        ++springIndex;
      }
      if (i/xNodes == yNodes-1) {
        Vector2[] lineUV = new Vector2[]{new Vector2(0,0),new Vector2(1,0),new Vector2(0,1),new Vector2(1,1)};
        int[] lineTris = new int[]{0,1,3,0,3,2};
        LineRenderer lineRenderer = new LineRenderer();
        lineRenderer.Vertices = new Vector3[4];
        lineRenderer.Normals = new Vector3[4];
        lineRenderer.WorkMesh.vertices = lineRenderer.Vertices;
        lineRenderer.WorkMesh.normals = lineRenderer.Normals;

        entityManager.SetComponentData(springEntities[springIndex], new Line { start = new float3(0, 0, 0), end = new float3(0, 0, 0), width = nodeWidth});
        entityManager.SetComponentData(springEntities[springIndex], new EntityPair { E1 = nodeEntities[i], E2 = nodeEntities[i+xNodes] });
        entityManager.SetComponentData(springEntities[springIndex], lineRenderer);
        entityManager.SetComponentData(springEntities[springIndex], new Elasticity {Value = nodeElasticity});
        entityManager.SetSharedComponentData(springEntities[springIndex], lineRenderSettings);
        ++springIndex;
      }
      
      entityManager.SetComponentData(nodeEntities[i], new Damper { Value = nodeDrag});
      entityManager.SetComponentData(nodeEntities[i], new Elasticity{ Value = nodeElasticity});
      entityManager.SetComponentData(nodeEntities[i], new GridPosition{ Value = new int2(i%xNodes, i/xNodes)});
      entityManager.SetSharedComponentData(nodeEntities[i], gridRender);
      //entityManager.SetSharedComponentData(nodeEntities[i], meshInstanceRenderer);
    }
    springEntities.Dispose();
    nodeEntities.Dispose();
  }

}