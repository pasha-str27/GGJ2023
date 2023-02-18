using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using UnityEditor;

namespace CoreGame
{
    namespace Tile
    {
        public struct TileInfo
        {
            //[SerializeField]
            //private TileFillingInfo fillingInfo;

            public SpriteRenderer backSprite;
            public SpriteRenderer rootSprite;
            public Transform tileTransform;
            public TileFilling fillingType;

            public override string ToString()
            {
                return fillingType.ToString();
            }

            //private Vector2Int _position;

            //public Vector2Int Position { get => _position; set => _position = value; }

            //[ContextMenu("Rotate")]
            //public void Rotate()
            //{
            //    fillingInfo.Rotate();

            //    var oldRotation = transform.rotation.eulerAngles;

            //    transform.rotation = Quaternion.Euler(oldRotation + Vector3.back * 90);
            //}
        }
    }
}
