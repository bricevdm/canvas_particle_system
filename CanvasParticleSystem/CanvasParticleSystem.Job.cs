#if CANVAS_PFX_JOBS

using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;

public class CanvasParticleSystemJobHelper
{
  private NativeArray<ParticleSystem.Particle> particleArray; 
  
  private NativeArray<Vector3> vertices;
  private NativeArray<Color32> colors;
  private NativeArray<Vector2> coords;
  private NativeArray<int> triangles;

  private SampledMinMaxCurve sizeCurve;
  private SampledMinMaxGradient colorGradient;
  
  public CanvasParticleSystemJobHelper(ParticleSystem pfx, int maxSize)
  {
    particleArray = new NativeArray<ParticleSystem.Particle>(maxSize, Allocator.Persistent);
    
    int vertexCount = maxSize * 4;
    vertices = new NativeArray<Vector3>(vertexCount, Allocator.Persistent);
    colors = new NativeArray<Color32>(vertexCount, Allocator.Persistent);
    coords = new NativeArray<Vector2>(vertexCount, Allocator.Persistent);
    triangles = new NativeArray<int>(maxSize * 6, Allocator.Persistent);

    var sizeOverLifetimeModule = pfx.sizeOverLifetime;
    var colorOverLifetimeModule = pfx.colorOverLifetime;
    
    ParticleSystem.MinMaxCurve s = sizeOverLifetimeModule.enabled ? sizeOverLifetimeModule.size : 1;
    ParticleSystem.MinMaxGradient c = colorOverLifetimeModule.enabled ? colorOverLifetimeModule.color : new ParticleSystem.MinMaxGradient(Color.white);
    sizeCurve = new SampledMinMaxCurve(s, sizeOverLifetimeModule.enabled ? 256 : 2);
    colorGradient = new SampledMinMaxGradient(c, colorOverLifetimeModule.enabled ? 256 : 2);
  }

  public void Clear()
  {
    if (particleArray.IsCreated) particleArray.Dispose();
    if (vertices.IsCreated) vertices.Dispose();
    if (colors.IsCreated) colors.Dispose();
    if (coords.IsCreated) coords.Dispose();
    if (triangles.IsCreated) triangles.Dispose();
    
    sizeCurve.Dispose();
    colorGradient.Dispose();
  }

  public void Compute(Mesh mesh, ParticleSystem particleSystem, bool isWorldSpace, Transform transform)
  {
    int particleCount = particleSystem.GetParticles(particleArray);

    var job = new CanvasParticleSystemJob
    {
      particles = particleArray,
      startSize = particleSystem.main.startSize.Evaluate(0),
      sizeCurve = sizeCurve,
      colorGradient = colorGradient,
      activeCount = particleCount,
      isWorldSpace = isWorldSpace,
      transformMatrix = transform.worldToLocalMatrix,
      
      vertices = vertices,
      colors = colors,
      coords = coords,
      triangles =  triangles
    };
    
    var jobHandle = job.Schedule();
			
    jobHandle.Complete();
    
    mesh.Clear();
    
    int activeVerticesCount = particleCount * 4;
    mesh.SetVertices(vertices, 0, activeVerticesCount);
    mesh.SetColors(colors, 0, activeVerticesCount);
    mesh.SetUVs(0, coords, 0, activeVerticesCount);

    int activeTrianglesCount = particleCount * 6;

    int[] indices = triangles.ToArray();
    mesh.SetTriangles(indices, 0, activeTrianglesCount, 0);
  }

  [BurstCompile]
  private struct CanvasParticleSystemJob : IJob
  {
    [ReadOnly]
    public NativeArray<ParticleSystem.Particle> particles;

    [ReadOnly]
    public SampledMinMaxCurve sizeCurve;
    
    [ReadOnly]
    public SampledMinMaxGradient colorGradient;

    [ReadOnly]
    public float startSize;
    
    [ReadOnly]
    public int activeCount;

    [ReadOnly]
    public bool isWorldSpace;

    [ReadOnly]
    public Matrix4x4 transformMatrix;
    
    public NativeArray<Vector3> vertices;
    public NativeArray<Color32> colors;
    public NativeArray<Vector2> coords;
    public NativeArray<int> triangles;
    
    public void Execute()
    {
      for (int i = 0; i < activeCount; i++)
      {
        ParticleSystem.Particle particle = particles[i];

        float timeAlive = particle.startLifetime - particle.remainingLifetime;
        float normalizedLifetime = Mathf.Clamp01(timeAlive / particle.startLifetime);
        float size = startSize * sizeCurve.Evaluate(normalizedLifetime);
        Color32 color32 = colorGradient.Evaluate(normalizedLifetime);
        
        CanvasParticleSystem.GetPositions(particle, isWorldSpace, Vector3.one * size, transformMatrix, 
          out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3);
        CanvasParticleSystem.CalculateUvs(out Vector2 c0, out Vector2 c1, out Vector2 c2, out Vector2 c3);

        int vertexIndex = i * 4;
        int v0 = vertexIndex;
        int v1 = vertexIndex + 1;
        int v2 = vertexIndex + 2;
        int v3 = vertexIndex + 3;

        vertices[v0] = p0; vertices[v1] = p1; vertices[v2] = p2; vertices[v3] = p3;
        colors[v0] = color32; colors[v1] = color32; colors[v2] = color32; colors[v3] = color32;
        coords[v0] = c0; coords[v1] = c1; coords[v2] = c2; coords[v3] = c3;

        int triangleIndex = i * 6;

        triangles[triangleIndex] = v0;
        triangles[triangleIndex + 1] = v1;
        triangles[triangleIndex + 2] = v2;

        triangles[triangleIndex + 3] = v2;
        triangles[triangleIndex + 4] = v3;
        triangles[triangleIndex + 5] = v0;
      }
    }
  }
}

#endif