using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomGeneratorWithWeight;

[CreateAssetMenu()]
public class CombinationColor : ScriptableObject
{
    [SerializeField] List<ItemForRandom<Color>> colors;

    public int GetColorIndex() => GetItemWithWeight.GetIndex(colors);

    public Color GetColor() => GetItemWithWeight.GetItem(colors);

    public Color GetColor(int index) => colors[index].GetItem();
}
