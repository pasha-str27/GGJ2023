using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomGeneratorWithWeight;

namespace GameCore
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