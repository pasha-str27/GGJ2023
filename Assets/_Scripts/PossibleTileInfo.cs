using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using UnityEditor;

namespace CoreGame
{
    namespace Tile
    {
        [CreateAssetMenu()]
        public class PossibleTileInfo : ScriptableObject
        {
            public Sprite[] sprites; 

            [SerializeField]
            private TileFillingInfo tileFillingInfo;


            //[ContextMenu("Rotate")]
            //public void Rotate()
            //{
            //    tileFillingInfo.Rotate();

            //    var oldRotation = transform.rotation.eulerAngles;

            //    transform.rotation = Quaternion.Euler(oldRotation + Vector3.back * 90);
            //}
        }
    }
}
