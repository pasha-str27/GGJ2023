using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomGeneratorWithWeight
{
    //extend this class for more functionality
    [Serializable]
    public class ItemForRandom <T> : IItem
    {
        [SerializeField]
        int _weight = 1;

        [SerializeField]
        T _item;

        public int GetWeight() => _weight;

        public T GetItem() => _item;
    }
}
