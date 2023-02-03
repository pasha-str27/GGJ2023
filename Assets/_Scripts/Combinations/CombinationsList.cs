using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomGeneratorWithWeight;

namespace CoreGame
{
    namespace Tile
    {
        [CreateAssetMenu()]
        public class CombinationsList : ScriptableObject
        {
            public List<ItemForRandom<CombinationShape>> shapes;
        }
    }
}