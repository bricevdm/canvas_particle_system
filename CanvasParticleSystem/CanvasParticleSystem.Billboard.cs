using UnityEngine;
using UnityEngine.Profiling;

public partial class CanvasParticleSystem
{
  private void AddParticleBillboard(int particleIndex, bool isWorldSimulationSpace)
  {
    ParticleSystem.Particle particle = particles[particleIndex];

    Color32 color32 = particle.GetCurrentColor(pfx);

    // compute texture coordinates

    Profiler.BeginSample("compute texture coordinates");

    Vector4 coord0, coord1, coord2, coord3;

    if (textureSheetAnimation.enabled)
      CalculateAnimatedUvs(particle, textureSheetAnimation, out coord0, out coord1, out coord2, out coord3);
    else
      CalculateUvs(out coord0, out coord1, out coord2, out coord3);


    Profiler.EndSample();

    // compute vertex positions

    Profiler.BeginSample("compute vertex positions");

    Vector3 size3D = particle.GetCurrentSize3D(pfx);
    Matrix4x4 toLocalMatrix = rectTransform.worldToLocalMatrix;
    
    GetPositions(particle, isWorldSimulationSpace, size3D, toLocalMatrix,
      out Vector3 leftBottom, out Vector3 leftTop, out Vector3 rightTop, out Vector3 rightBottom);

    Profiler.EndSample();

    Profiler.BeginSample("set mesh data");

    // set vertices

    void AddVertex(int index, Vector3 position, Color32 color, Vector4 uv0)
    {
      vertices[index] = position;
      colors[index] = color;
      coords[index] = uv0;
    }

    int vertexIndex = particleIndex * 4;
    int v0 = vertexIndex;
    int v1 = vertexIndex + 1;
    int v2 = vertexIndex + 2;
    int v3 = vertexIndex + 3;

    AddVertex(v0, leftBottom, color32, coord0);
    AddVertex(v1, leftTop, color32, coord1);
    AddVertex(v2, rightTop, color32, coord2);
    AddVertex(v3, rightBottom, color32, coord3);

    if (setScalesAndAgeToCoord1)
    {
      Vector4 data = new Vector4(size3D.x, size3D.y, size3D.z, 1f - Mathf.Clamp01(particle.remainingLifetime / particle.startLifetime));
      coords1[v0] = coords1[v1] = coords1[v2] = coords1[v3] = data;
    }

    // set triangles indices

    int triangleIndex = particleIndex * 6;

    triangles[triangleIndex] = v0;
    triangles[triangleIndex + 1] = v1;
    triangles[triangleIndex + 2] = v2;

    triangles[triangleIndex + 3] = v2;
    triangles[triangleIndex + 4] = v3;
    triangles[triangleIndex + 5] = v0;

    Profiler.EndSample();
  }

  public void GetPositions(
    ParticleSystem.Particle particle,
    bool isWorldSpace,
    Vector3 size3D,
    Matrix4x4 transformMatrix,
    out Vector3 leftBottom,
    out Vector3 leftTop,
    out Vector3 rightTop,
    out Vector3 rightBottom)
  {
    Vector3 center = particle.position;
    Quaternion rotation = Quaternion.Euler(particle.rotation3D);

    if (isWorldSpace)
    {
      center = transformMatrix.MultiplyPoint(center);
    }

    float halfX = size3D.x * 0.5f;
    float halfY = size3D.y * 0.5f;

    if (setCenterAsPosition)
    {
      leftBottom = leftTop = rightTop = rightBottom = center;
      return;
    }

    leftBottom = new Vector3(-halfX, -halfY);
    leftTop = new Vector3(-halfX, halfY);
    rightTop = new Vector3(halfX, halfY);
    rightBottom = new Vector3(halfX, -halfY);

    leftBottom = rotation * leftBottom + center;
    leftTop = rotation * leftTop + center;
    rightTop = rotation * rightTop + center;
    rightBottom = rotation * rightBottom + center;
  }
}