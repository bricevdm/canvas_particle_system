#if CANVAS_PFX_JOBS

using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;

public struct SampledMinMaxGradient : System.IDisposable
{
  private NativeArray<Color32> sampledColor;

  /// <param name="gradient"></param>
  /// <param name="samples">Must be 2 or higher</param>
  public SampledMinMaxGradient(ParticleSystem.MinMaxGradient gradient, int samples)
  {
    sampledColor = new NativeArray<Color32>(samples, Allocator.Persistent);
    float timeStep = 1f / (samples - 1);
 
    for (int i = 0; i < samples; i++)
    {
      sampledColor[i] = gradient.Evaluate(i * timeStep);
    }
  }
 
  public void Dispose()
  {
    sampledColor.Dispose();
  }
 
  /// <param name="time">Must be from 0 to 1</param>
  public Color32 Evaluate(float time)
  {
    int length = sampledColor.Length - 1;
    float clamp01 = time < 0 ? 0 : time > 1 ? 1 : time;
    float floatIndex = clamp01 * length;
    int floorIndex = (int)math.floor(floatIndex);
    
    if (floorIndex == length)
    {
      return sampledColor[length];
    }

    if (floorIndex < 0)
    {
      return sampledColor[0];
    }
 
    Color32 lowerValue = sampledColor[floorIndex];
    Color32 higherValue = sampledColor[floorIndex + 1];
    return Color32.Lerp(lowerValue, higherValue, math.frac(floatIndex));
  }
}

#endif