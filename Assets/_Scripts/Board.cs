using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D collider2d;
        [SerializeField] private Vector2Int boardSize;
        [SerializeField] private GameObject tile;
        [SerializeField] private GameObject gridContainer;

        private GameObject[,] _tiles;
        private Bounds _bounds;
        private Vector2 _startPos;
        private float _scale;
        private Vector2 _offset;

        private void Start()
        {
            _tiles = new GameObject[boardSize.x, boardSize.y];
            _bounds = collider2d.bounds;
            _offset = tile.GetComponent<SpriteRenderer>().bounds.size;
            GenerateTilemap();
        }

        private void GenerateTilemap()
        {
            float xScale = _bounds.size.x / (boardSize.x * _offset.x);
            float yScale = _bounds.size.y / (boardSize.y * _offset.y);

            _scale = xScale < yScale ? xScale : yScale;
            _offset *= _scale;

            Vector3 scaledGridSize = new Vector2(boardSize.x * _offset.x, boardSize.y * _offset.y);

            _startPos = _bounds.center - scaledGridSize * 0.5f;
            _startPos.x += _offset.x * 0.5f;
            _startPos.y += _offset.y * 0.5f;

            GenerateTiles();
        }
        private void GenerateTiles()
        {
            for (int x = 0; x < boardSize.x; x++)
            {
                for (int y = 0; y < boardSize.y; y++)
                {
                    GameObject newTile;
                    Vector2 newTilePos = new Vector3(_startPos.x + (_offset.x * x), _startPos.y + (_offset.y * y));

                    newTile = Instantiate(tile, newTilePos, Quaternion.identity);

                    Transform trans = newTile.transform;

                    trans.localScale *= _scale;
                    trans.SetParent(gridContainer.transform);

                    _tiles[x, y] = newTile;

                    Tile currTile = newTile.GetComponent<Tile>();
                    currTile.Position = new Vector2Int(x, y);
                }
            }
        }
    }
}

