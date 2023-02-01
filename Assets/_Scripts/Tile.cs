using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class Tile : MonoBehaviour
    {
        private Vector2Int _position;

        public Vector2Int Position { get => _position; set => _position = value; }
    }
}
