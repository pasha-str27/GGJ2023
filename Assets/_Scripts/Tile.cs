using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using UnityEditor;

namespace GameCore
{
    namespace Tile
    {
        public class Tile : MonoBehaviour
        {           
            [SerializeField]
            private TileFillingInfo tileFillingInfo;


            private Vector2Int _position;

            public Vector2Int Position { get => _position; set => _position = value; }

            [ContextMenu("Rotate")]
            public void Rotate()
            {
                tileFillingInfo.Rotate();

                var oldRotation = transform.rotation.eulerAngles;

                transform.rotation = Quaternion.Euler(oldRotation + Vector3.back * 90);
            }
        }
    }
}
