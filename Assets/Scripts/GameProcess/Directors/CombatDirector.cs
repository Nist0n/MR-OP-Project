using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GameProcess.Cards;
using GameProcess.Directors;
using GameProcess.Directors.Functions;
using UnityEngine;
using UnityEngine.Events;

public class CombatDirector : MonoBehaviour
{
    private class DirectorMoneyWave
    {
      public float interval;
      public float timer;
      public float multiplier;
      private float accumulatedAward;

      public float Update(float deltaTime, float difficultyCoefficient)
      {
        this.timer += deltaTime;
        if ((double) this.timer > (double) this.interval)
        {
          float num = 0.5f;
          this.timer -= this.interval;
          this.accumulatedAward += (float) ((double) this.interval * (double) this.multiplier * (1.0 + 0.40000000596046448 * (double) difficultyCoefficient)) * num;
        }
        float num1 = (float) Mathf.FloorToInt(this.accumulatedAward);
        this.accumulatedAward -= num1;
        return num1;
      }
    }
}
