using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreGame
{
    using Tile;

    public class Board : MonoBehaviour
    {
        [SerializeField] string cameraTag = "MainCamera";
        [SerializeField] protected BoxCollider2D collider2d;
        [SerializeField] protected Vector2Int boardSize;
        [SerializeField] protected GameObject tile;
        [SerializeField] protected GameObject gridContainer;

        protected TileInfo[,] _tiles;

        //protected SpriteRenderer[,] _tilesRendering;
        //protected Transform[,] _tilesRendering;
        protected Bounds _bounds;
        protected Vector2 _startPos;
        protected float _scale;
        protected Vector2 _offset;

        protected Camera gameCamera;

        protected virtual void Awake()
        {
            _tiles = new TileInfo[boardSize.x, boardSize.y];
            _bounds = collider2d.bounds;
            _offset = tile.GetComponent<SpriteRenderer>().bounds.size;

            GenerateTilemap();
            GenerateTiles();
        }

        protected virtual void Start()
        {
            gameCamera = GameObject.FindGameObjectWithTag(cameraTag).GetComponent<Camera>();
        }

        protected virtual void GenerateTilemap()
        {
            float xScale = _bounds.size.x / (boardSize.x * _offset.x);
            float yScale = _bounds.size.y / (boardSize.y * _offset.y);

            _scale = Mathf.Min(xScale, yScale);
            _offset *= _scale;

            Vector3 scaledGridSize = new Vector2(boardSize.x * _offset.x, boardSize.y * _offset.y);

            _startPos = new Vector2(_bounds.center.x - scaledGridSize.x * 0.5f, _bounds.center.y + scaledGridSize.y * 0.41f);// _bounds.center - scaledGridSize * 0.5f;
            _startPos.x += _offset.x * 0.5f;
            _startPos.y += _offset.y * 0.5f;
        }

        public Vector2 GetOffset() => _offset;

        public float GetColliderSize() => Mathf.Min(_bounds.size.x / boardSize.x, _bounds.size.y / boardSize.y);

        public virtual void GenerateTiles()
        {
            for (int x = 0; x < boardSize.x; x++)
            {
                for (int y = 0; y < boardSize.y; y++)
                {
                    GameObject newTile;
                    Vector2 newTilePos = new Vector3(_startPos.x + (_offset.x * x), _startPos.y - (_offset.y * y));

                    newTile = Instantiate(tile, newTilePos, Quaternion.identity);

                    Transform trans = newTile.transform;

                    trans.localScale *= _scale;
                    trans.SetParent(gridContainer.transform);

                    _tiles[x, y] = new TileInfo();
                    _tiles[x, y].sprite = newTile.GetComponent<SpriteRenderer>();
                    _tiles[x, y].fillingType = TileFilling.Empty;
                    _tiles[x, y].tileTransform = newTile.transform;
                }
            }
        }

        protected Vector2Int GetTileIndex(Vector2 position)
        {
            //print(position);
            for (int i = 0; i < _tiles.GetLength(0); ++i)
                for (int j = 0; j < _tiles.GetLength(1); ++j)
                {
                    var tilePos = _tiles[i, j].tileTransform.position;
                    float offsetHalf = _offset.x / 2;

                    //print(offsetHalf);
                    //print(tilePos);

                    //print(tilePos.x - offsetHalf >= position.x);

                    if (tilePos.x - offsetHalf <= position.x && tilePos.x + offsetHalf >= position.x
                        && tilePos.y - offsetHalf <= position.y && tilePos.y + offsetHalf >= position.y)
                    {
                        return new Vector2Int(j, i);
                    }
                }


            return new Vector2Int(-1, -1);
        }

        public Vector2 GetSize() => new Vector2(_scale, _scale);

        public bool TryAddCombination(Vector2Int startCombPos, TileFilling[,] combFilling, TileInfo[,] combTiles)
        {
            Vector2Int boardIndex = GetTileIndex(gameCamera.ScreenToWorldPoint(Input.mousePosition));

            print(boardIndex);

         
            try
            {
                if (_tiles[boardIndex.y, boardIndex.x].fillingType == TileFilling.Filled)
                    return false;

                for (int y, y1 = 0; y1 < combTiles.GetLength(1); ++y1)
                { 
                    for (int x, x1 = 0; x1 < combTiles.GetLength(0); ++x1) 
                    {
                        x = boardIndex.y - startCombPos.y + x1;
                        y = boardIndex.x - startCombPos.x + y1;

                        //1-x
                        if(combTiles[x1, y1].fillingType == TileFilling.Filled)
                        {
                            //1-x
                            if (_tiles[x, y].fillingType == TileFilling.Filled)
                                return false;
                        }
                    }
                }

                var rotation = combTiles[0, 0].tileTransform.parent.rotation;

                for (int y, y1 = 0; y1 < combTiles.GetLength(1); ++y1)
                {
                    for (int x, x1 = 0; x1 < combTiles.GetLength(0); ++x1)
                    {
                        x = boardIndex.y - startCombPos.y + x1;
                        y = boardIndex.x - startCombPos.x + y1;

                        //1-x
                        if (combTiles[x1, y1].fillingType == TileFilling.Filled)
                        {
                            _tiles[x, y].sprite.sprite = combTiles[x1, y1].sprite.sprite;
                            _tiles[x, y].fillingType = TileFilling.Filled;
                            _tiles[x, y].sprite.transform.rotation = rotation;
                            //1-x
                            //if (_tiles[x, y].fillingType == TileFilling.Filled)
                            //    return false;
                        }
                    }
                }

                //for (int y = boardIndex.x - startCombPos.y, y1 = 0; y1 < combTiles.GetLength(1); ++y, ++y1)
                //{
                //    for (int x = boardIndex.y - startCombPos.x, x1 = 0; x1 < combTiles.GetLength(0); ++x, ++x1)
                //    {
                //        if (combTiles[x1, y1].fillingType == TileFilling.Filled)
                //        {
                //            _tiles[x, y].sprite.sprite = combTiles[x1, y1].sprite.sprite;
                //            _tiles[x, y].fillingType = TileFilling.Filled;
                //        }
                //    }
                //}

                //for (int y = boardIndex.y - startCombPos.y; y < boardIndex.y + combTiles.GetLength(1); ++y)
                //{
                //    for (int x = boardIndex.x - startCombPos.x; x < boardIndex.x + combTiles.GetLength(0); ++x)
                //    {
                //        if (_tiles[y, x].fillingType != TileFilling.Filled
                //            && combTiles[])
                //        {

                //        }
                //    }
                //}

                return true;
            }
            catch
            {
                return false;
            }

              //if(startCombPos.y > boardIndex.y
              //  || (combFilling.GetLength(0) - startCombPos.y) + boardIndex.y > _tiles.GetLength(0))
              //  return false;
        }
    }
}

