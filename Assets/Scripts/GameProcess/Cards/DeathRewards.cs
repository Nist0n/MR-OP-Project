using UnityEngine;

namespace GameProcess.Cards
{
  public class DeathRewards : MonoBehaviour
  {
    private float fallbackGold;

    public float goldReward
    {
      get
      {
        return fallbackGold;
      }
      set
      {
        fallbackGold = value;
      }
    }

    public float expReward { get; set; }

    public int spawnValue { get; set; }
  }
}
