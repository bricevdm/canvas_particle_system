using System;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class CanvasParticleSystem
{
  private ParticleSystem.TextureSheetAnimationModule textureSheetAnimation;
  private ParticleSystem.MinMaxCurve frameOverTimeCurve;

  public static void CalculateUvs(out Vector2 c0, out Vector2 c1, out Vector2 c2, out Vector2 c3)
  {
    c0 = new Vector2(0f, 0f);
    c1 = new Vector2(0f, 1f);
    c2 = new Vector2(1f, 1f);
    c3 = new Vector2(1f, 0f);
  }

  // TODO support animated UVs from Jobs
  private void CalculateAnimatedUvs(
    ParticleSystem.Particle particle,
    ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule,
    out Vector2 uv0,
    out Vector2 uv1,
    out Vector2 uv2,
    out Vector2 uv3)
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

    uv0 = new Vector2(sX, sY);
    uv1 = new Vector2(sX, eY);
    uv2 = new Vector2(eX, eY);
    uv3 = new Vector2(eX, sY);
  }
}