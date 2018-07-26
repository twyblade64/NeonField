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

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  public static void Initialize() {
    var entityManager = World.Active.GetOrCreateManager<EntityManager>();

    NodeArchetype = entityManager.CreateArchetype(
      typeof(Position), typeof(Anchor), typeof(Velocity), 
      typeof(Physical), typeof(Drag), typeof(Elasticity),
      typeof(GridPosition), typeof(GridRender),
      typeof(TransformMatrix) //, typeof(MeshInstanceRenderer)
    );

    World.Active.GetExistingManager<MeshRenderSystem>().Enabled = false;
  }

  public void Start() {
    EntityManager entityManager = World.Active.GetOrCreateManager<EntityManager>();

    // Create nodes
    NativeArray<Entity> nodeEntities = new NativeArray<Entity>(xNodes * yNodes, Allocator.Temp);
    entityManager.CreateEntity(NodeArchetype, nodeEntities);

    
    MeshInstanceRenderer meshInstanceRenderer = new MeshInstanceRenderer {
      mesh = nodeMesh,
      material = nodeMaterial,
      subMesh = 0,
      receiveShadows = false,
      castShadows = UnityEngine.Rendering.ShadowCastingMode.Off
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

    for (int i = 0; i < nodeEntities.Length; ++i) {
      
      float pX = 1f * (i%xNodes) / (xNodes-1) * nodeField.width + nodeField.x;
      float pY = 1f * (i/xNodes) / (yNodes-1) * nodeField.height + nodeField.y;
      entityManager.SetComponentData(nodeEntities[i], new Position { Value = new float3(pX, (Mathf.Sin(2f * Mathf.PI * (i/xNodes)/20 ) + Mathf.Cos(2f * Mathf.PI * (i%xNodes)/20 ))*startHeight ,pY)});
      entityManager.SetComponentData(nodeEntities[i], new Anchor { Value = new float3(pX, 0, pY)});
      entityManager.SetComponentData(nodeEntities[i], new Velocity { Value = new float3(0, 0, 0)});
      if (i%xNodes == 0 || i/xNodes == 0 || i%xNodes == xNodes-1 || i/xNodes == yNodes-1)
        entityManager.SetComponentData(nodeEntities[i], new Physical { Force = new float3(0, 0, 0), Mass = 10000f});
      else
        entityManager.SetComponentData(nodeEntities[i], new Physical { Force = new float3(0, 0, 0), Mass = 1f});
      entityManager.SetComponentData(nodeEntities[i], new Drag { Value = nodeDrag});
      entityManager.SetComponentData(nodeEntities[i], new Elasticity{ Value = nodeElasticity});
      entityManager.SetComponentData(nodeEntities[i], new GridPosition{ Value = new int2(i%xNodes, i/xNodes)});
      entityManager.SetSharedComponentData(nodeEntities[i], gridRender);
      //entityManager.SetSharedComponentData(nodeEntities[i], meshInstanceRenderer);
    }
    nodeEntities.Dispose();
  }

}