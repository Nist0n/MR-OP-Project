using System;
using UnityEngine;

namespace GameProcess.Directors.Functions
{
    public class WeightedSelection<T>
    {
        [HideInInspector]
  [SerializeField]
  public WeightedSelection<T>.ChoiceInfo[] choices;
  [SerializeField]
  [HideInInspector]
  private int _count;
  [SerializeField]
  [HideInInspector]
  private float totalWeight;
  private const int minCapacity = 8;

  public int Count
  {
    get => this._count;
    private set => this._count = value;
  }

  public WeightedSelection(int capacity = 8) => this.choices = new WeightedSelection<T>.ChoiceInfo[capacity];

  public int Capacity
  {
    get => this.choices.Length;
    set
    {
      if (value < 8 || value < this.Count)
        throw new ArgumentOutOfRangeException(nameof (value));
      WeightedSelection<T>.ChoiceInfo[] choices1 = this.choices;
      this.choices = new WeightedSelection<T>.ChoiceInfo[value];
      WeightedSelection<T>.ChoiceInfo[] choices2 = this.choices;
      int count = this.Count;
      Array.Copy((Array) choices1, (Array) choices2, count);
    }
  }

  public void AddChoice(T value, float weight) => this.AddChoice(new WeightedSelection<T>.ChoiceInfo()
  {
    value = value,
    weight = weight
  });

  public void AddChoice(WeightedSelection<T>.ChoiceInfo choice)
  {
    if (this.Count == this.Capacity)
      this.Capacity *= 2;
    this.choices[this.Count++] = choice;
    this.totalWeight += choice.weight;
  }

  public void RemoveChoice(int choiceIndex)
  {
    int index1 = choiceIndex >= 0 && this.Count > choiceIndex ? choiceIndex : throw new ArgumentOutOfRangeException(nameof (choiceIndex));
    for (int index2 = this.Count - 1; index1 < index2; ++index1)
      this.choices[index1] = this.choices[index1 + 1];
    this.choices[--this.Count] = new WeightedSelection<T>.ChoiceInfo();
    this.RecalculateTotalWeight();
  }

  public void ModifyChoiceWeight(int choiceIndex, float newWeight)
  {
    this.choices[choiceIndex].weight = newWeight;
    this.RecalculateTotalWeight();
  }

  public void Clear()
  {
    for (int index = 0; index < this.Count; ++index)
      this.choices[index] = new WeightedSelection<T>.ChoiceInfo();
    this.Count = 0;
    this.totalWeight = 0.0f;
  }

  private void RecalculateTotalWeight()
  {
    this.totalWeight = 0.0f;
    for (int index = 0; index < this.Count; ++index)
      this.totalWeight += this.choices[index].weight;
  }

  public T Evaluate(float normalizedIndex) => this.choices[this.EvaluateToChoiceIndex(normalizedIndex)].value;

  public int EvaluateToChoiceIndex(float normalizedIndex) => this.EvaluateToChoiceIndex(normalizedIndex, (int[]) null);

  public int EvaluateToChoiceIndex(float normalizedIndex, int[] ignoreIndices)
  {
    if (this.Count == 0)
      throw new InvalidOperationException("Cannot call Evaluate without available choices.");
    float totalWeight = this.totalWeight;
    if (ignoreIndices != null)
    {
      foreach (int ignoreIndex in ignoreIndices)
        totalWeight -= this.choices[ignoreIndex].weight;
    }
    float num1 = normalizedIndex * totalWeight;
    float num2 = 0.0f;
    for (int toChoiceIndex = 0; toChoiceIndex < this.Count; ++toChoiceIndex)
    {
      if (ignoreIndices == null || Array.IndexOf<int>(ignoreIndices, toChoiceIndex) == -1)
      {
        num2 += this.choices[toChoiceIndex].weight;
        if ((double) num1 < (double) num2)
          return toChoiceIndex;
      }
    }
    return this.Count - 1;
  }

  public WeightedSelection<T>.ChoiceInfo GetChoice(int i) => this.choices[i];

  [Serializable]
  public struct ChoiceInfo
  {
    public T value;
    public float weight;
  }
    }
}
