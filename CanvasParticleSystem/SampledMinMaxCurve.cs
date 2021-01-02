#if CANVAS_PFX_JOBS

using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
 
public struct SampledMinMaxCurve : System.IDisposable
{
  private NativeArray<float> sampledFloat;

  /// <param name="curve"></param>
  /// <param name="samples">Must be 2 or higher</param>
  public SampledMinMaxCurve(ParticleSystem.MinMaxCurve curve, int samples)
  {
    sampledFloat = new NativeArray<float>(samples, Allocator.Persistent);
    float timeStep = 1f / (samples - 1);
 
    for (int i = 0; i < samples; i++)
    {
      sampledFloat[i] = curve.Evaluate(i * timeStep);
    }
  }
 
  public void Dispose()
  {
    sampledFloat.Dispose();
  }
 
  /// <param name="time">Must be from 0 to 1</param>
  public float Evaluate(float time)
  {
    int length = sampledFloat.Length - 1;
    float clamp01 = time < 0 ? 0 : time > 1 ? 1 : time;
    float floatIndex = clamp01 * length;
    float remainder = math.frac(floatIndex);
    int floorIndex = (int)(floatIndex - remainder);
    
    if (floorIndex == length)
    {
      return sampledFloat[length];
    }

    if (floorIndex < 0)
    {
      return sampledFloat[0];
    }

    float lowerValue = sampledFloat[floorIndex];
    float higherValue = sampledFloat[floorIndex + 1];
    return math.lerp(lowerValue, higherValue, remainder);
  }
}

#endif