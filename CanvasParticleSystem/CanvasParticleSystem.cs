using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem), typeof(CanvasRenderer))]
public partial class CanvasParticleSystem : MaskableGraphic
{
  [SerializeField]
  private ParticleSystem pfx;

  [SerializeField]
  private Texture texture;

  [SerializeField]
  private bool useJobs;
  
#if CANVAS_PFX_JOBS
  private CanvasParticleSystemJobHelper jobHelper;
#endif

  [SerializeField]
  private bool setCenterAsPosition;
  
  [SerializeField]
  private bool setScalesAndAgeToCoord1;

  private ParticleSystem.Particle[] particles;
  private readonly Bounds meshBounds = new Bounds(Vector3.zero, Screen.height * Vector3.one);

  private Vector3[] vertices;
  private Color32[] colors;
  private Vector4[] coords;
  private Vector4[] coords1;
  private int[] triangles;
  

  public override Texture mainTexture
  {
    get
    {
      if (!ReferenceEquals(texture, null)) return texture;

      if (material != null && material.mainTexture != null)
      {
        return material.mainTexture;
      }

      return s_WhiteTexture;
    }
  }

  protected override void OnEnable()
  {
    Init();

    base.OnEnable();
  }

  protected override void OnDestroy()
  {
#if CANVAS_PFX_JOBS
    if (useJobs) jobHelper?.Clear();
#endif

    base.OnDestroy();
  }

  protected void Update()
  {
    pfx.Simulate(Time.deltaTime, true, false);

    UpdateGeometry();
  }
  
  private void Init()
  {
    int maxCount = pfx.main.maxParticles;

    textureSheetAnimation = pfx.textureSheetAnimation;
    frameOverTimeCurve = textureSheetAnimation.frameOverTime;
    
#if CANVAS_PFX_JOBS

    if (useJobs && Application.isPlaying)
    {
      jobHelper = new CanvasParticleSystemJobHelper(pfx, maxCount);
    }
    else
    
#endif
    
    {
      int vertexCount = maxCount * 4; // 4 vertices per quad
      vertices = new Vector3[vertexCount];
      colors = new Color32[vertexCount];
      coords = new Vector4[vertexCount];

      if (setScalesAndAgeToCoord1)
      {
        coords1 = new Vector4[vertexCount];
      }
    
      triangles = new int[maxCount * 6]; // 2 triangles per quad
    }
  }

#pragma warning disable 672
  protected override void OnPopulateMesh(Mesh mesh)
  {
    CreateParticleSystemMesh(mesh);

    // deal with edge case of invalid bounds when all vertices are in the same position
    if (setCenterAsPosition && pfx.particleCount == 1) mesh.bounds = meshBounds;
  }
#pragma warning restore 672

  private void CreateParticleSystemMesh(Mesh mesh)
  {
    if (pfx.particleCount < 1)
    {
      mesh.Clear();
      
      return;
    }

    var mainModule = pfx.main;

    bool isWorldSimulationSpace = mainModule.simulationSpace == ParticleSystemSimulationSpace.World;

#if CANVAS_PFX_JOBS

    if (useJobs && Application.isPlaying)
    {
      jobHelper.Compute(mesh, pfx, isWorldSimulationSpace, transform);
    }
    else
    
#endif
    
    {
      if (ReferenceEquals(particles, null) || particles.Length < mainModule.maxParticles) 
        particles = new ParticleSystem.Particle[mainModule.maxParticles];

      int particleCount = pfx.GetParticles(particles);
      CreateAllBillboards(mesh, particleCount, isWorldSimulationSpace);
    }
  }

  private void CreateAllBillboards(Mesh mesh, int particleCount, bool isWorldSimulationSpace)
  {
    for (int particleIndex = 0; particleIndex < particleCount; particleIndex++)
    {
      AddParticleBillboard(particleIndex, isWorldSimulationSpace);
    }

    mesh.Clear();

    int activeVerticesCount = particleCount * 4;
    mesh.SetVertices(vertices, 0, activeVerticesCount);
    mesh.SetColors(colors, 0, activeVerticesCount);
    mesh.SetUVs(0, coords, 0, activeVerticesCount);
    
    if (setScalesAndAgeToCoord1)
    {
      mesh.SetUVs(1, coords1, 0, activeVerticesCount);
    }

    int activeTrianglesCount = particleCount * 6;
    mesh.SetTriangles(triangles, 0, activeTrianglesCount, 0);
  }
}