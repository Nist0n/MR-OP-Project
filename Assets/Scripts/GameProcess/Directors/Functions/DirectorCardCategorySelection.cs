using System;
using UnityEngine;

namespace GameProcess.Directors.Functions
{
  public class DirectorCardCategorySelection : ScriptableObject
  {
    public Category[] categories = Array.Empty<Category>();

    public static event CalcCardWeight calcCardWeight;

    public float SumAllWeightsInCategory(Category category)
    {
      float num = 0.0f;
      for (int index = 0; index < category.cards.Length; ++index)
      { 
        num += category.cards[index].SelectionWeight;
      }
      return num;
    }

    public int FindCategoryIndexByName(string categoryName)
    {
      for (int categoryIndexByName = 0; categoryIndexByName < this.categories.Length; ++categoryIndexByName)
      {
        if (string.CompareOrdinal(this.categories[categoryIndexByName].name, categoryName) == 0)
          return categoryIndexByName;
      }
      return -1;
    }

    public void CopyFrom(DirectorCardCategorySelection src)
    {
      DirectorCardCategorySelection.Category[] categories = src.categories;
      Array.Resize<DirectorCardCategorySelection.Category>(ref this.categories, src.categories.Length);
      for (int index = 0; index < this.categories.Length; ++index)
      {
        ref DirectorCardCategorySelection.Category local = ref this.categories[index];
        local = categories[index];
      }
    }

    public WeightedSelection<DirectorCard> GenerateDirectorCardWeightedSelection()
    {
      WeightedSelection<DirectorCard> weightedSelection = new WeightedSelection<DirectorCard>();
      for (int index = 0; index < this.categories.Length; ++index)
      {
        ref DirectorCardCategorySelection.Category local = ref this.categories[index];
        float num1 = this.SumAllWeightsInCategory(local);
        float num2 = local.selectionWeight / num1;
        if ((double) num1 > 0.0)
        {
          foreach (DirectorCard card in local.cards)
          {
              float weight = (float) card.SelectionWeight * num2;
              DirectorCardCategorySelection.CalcCardWeight calcCardWeight = DirectorCardCategorySelection.calcCardWeight;
              if (calcCardWeight != null)
                calcCardWeight(card, ref weight);
              weightedSelection.AddChoice(card, weight);
          }
        }
      }
      return weightedSelection;
    }

    public void Clear() => this.categories = Array.Empty<DirectorCardCategorySelection.Category>();

    public int AddCategory(string name, float selectionWeight)
    {
      DirectorCardCategorySelection.Category category = new DirectorCardCategorySelection.Category()
      {
        name = name,
        cards = Array.Empty<DirectorCard>(),
        selectionWeight = selectionWeight
      };
      //ArrayUtils.ArrayAppend<DirectorCardCategorySelection.Category>(ref this.categories, in category);
      return this.categories.Length - 1;
    }

    public int AddCard(int categoryIndex, DirectorCard card)
    {
      if ((long) (uint) categoryIndex >= (long) this.categories.Length)
        throw new ArgumentOutOfRangeException(nameof (categoryIndex));
      ref DirectorCard[] local = ref this.categories[categoryIndex].cards;
      //ArrayUtils.ArrayAppend<DirectorCard>(ref local, in card);
      return local.Length - 1;
    }

    public void RemoveCardsThatFailFilter(Predicate<DirectorCard> predicate)
    {
      for (int index = 0; index < this.categories.Length; ++index)
      {
        ref DirectorCardCategorySelection.Category local = ref this.categories[index];
        for (int position = local.cards.Length - 1; position >= 0; --position)
        {
          DirectorCard card = local.cards[position];
          //if (!predicate(card))
            //ArrayUtils.ArrayRemoveAtAndResize<DirectorCard>(ref local.cards, position);
        }
      }
    }

    public virtual bool IsAvailable() => true;
    
    public void OnValidate()
    {
      for (int index1 = 0; index1 < this.categories.Length; ++index1)
      {
        DirectorCardCategorySelection.Category category = this.categories[index1];
        if ((double) category.selectionWeight <= 0.0)
          Debug.LogErrorFormat("'{0}' in '{1}' has no weight!", (object) category.name, (object) this);
        for (int index2 = 0; index2 < category.cards.Length; ++index2)
        {
          DirectorCard card = category.cards[index2];
          if ((double) card.SelectionWeight <= 0.0)
            Debug.LogErrorFormat("'{0}' in '{1}' has no weight!", (object) card.spawnCard.name, (object) this);
        }
      }
    }

    public delegate void CalcCardWeight(DirectorCard card, ref float weight);

    [Serializable]
    public struct Category
    {
      [Tooltip("A name to help identify this category")]
      public string name;
      public DirectorCard[] cards;
      public float selectionWeight;
    }
  }
}
