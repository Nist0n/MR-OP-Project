using UnityEngine;

namespace GameProcess.Cards
{
  public class DeathRewards : MonoBehaviour
  {
    private uint fallbackGold;

    public uint goldReward
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

    public uint expReward { get; set; }

    public int spawnValue { get; set; }
  }
}
