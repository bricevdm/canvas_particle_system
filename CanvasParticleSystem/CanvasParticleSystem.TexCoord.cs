using System;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class CanvasParticleSystem
{
  private ParticleSystem.TextureSheetAnimationModule textureSheetAnimation;
  private ParticleSystem.MinMaxCurve frameOverTimeCurve;

  public static void CalculateUvs(out Vector4 c0, out Vector4 c1, out Vector4 c2, out Vector4 c3)
  {
    c0 = new Vector4(0f, 0f, 0f, 0f);
    c1 = new Vector4(0f, 1f, 0f, 1f);
    c2 = new Vector4(1f, 1f, 1f, 1f);
    c3 = new Vector4(1f, 0f, 1f, 0f);
  }

  // TODO support animated UVs from Jobs
  private void CalculateAnimatedUvs(
    ParticleSystem.Particle particle,
    ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule,
    out Vector4 uv0,
    out Vector4 uv1,
    out Vector4 uv2,
    out Vector4 uv3)
  {
    float timeAlive = particle.startLifetime - particle.remainingLifetime;
    float lifeTimePerCycle = particle.startLifetime / textureSheetAnimationModule.cycleCount;
    float timePerCycle = timeAlive % lifeTimePerCycle;
    float timeAliveAnim01 = timePerCycle / lifeTimePerCycle; // in percents

    int xCount = textureSheetAnimationModule.numTilesX;
    int yCount = textureSheetAnimationModule.numTilesY;

    int maxCount = yCount * xCount;
    float progress = frameOverTimeCurve.Evaluate(timeAliveAnim01);

    float frame;

    switch (textureSheetAnimationModule.animation)
    {
      case ParticleSystemAnimationType.WholeSheet:
      {
        frame = Mathf.Floor(progress * maxCount);
        frame = Mathf.Clamp(frame, 0, maxCount - 1);
        break;
      }
      case ParticleSystemAnimationType.SingleRow:
      {
        frame = Mathf.Floor(progress * xCount);
        frame = Mathf.Clamp(frame, 0, xCount - 1);

        int currentRow = textureSheetAnimationModule.rowIndex;
        if (textureSheetAnimationModule.rowMode == ParticleSystemAnimationRowMode.Random)
        {
          Random.InitState((int) particle.randomSeed);
          currentRow = Random.Range(0, yCount);
        }

        frame += currentRow * xCount;
        break;
      }
      default:
        throw new ArgumentOutOfRangeException();
    }

    int x = (int) frame % xCount;
    int y = (int) frame / xCount;
    y = yCount - 1 - y;

    float xDelta = 1f / xCount;
    float yDelta = 1f / yCount;

    float sX = x * xDelta;
    float sY = y * yDelta;
    float eX = sX + xDelta;
    float eY = sY + yDelta;

    uv0 = new Vector4(sX, sY, 0f, 0f);
    uv1 = new Vector4(sX, eY, 0f, 1f);
    uv2 = new Vector4(eX, eY, 1f, 1f);
    uv3 = new Vector4(eX, sY, 1f, 0f);
  }
}