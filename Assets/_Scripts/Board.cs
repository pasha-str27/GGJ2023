using UnityEngine;
using System.Collections;

namespace CoreGame
{
    using Tile;

    public class Board : MonoBehaviour
    {
        [SerializeField] string cameraTag = "MainCamera";
        [SerializeField] protected Vector2Int boardSize;
        [SerializeField] protected GameObject tile;
        [SerializeField] protected GameObject gridContainer;
        [SerializeField, Range(0.05f, 0.5f)] protected float shiftDelay;

        private int compoundRowIndex;

        protected TileInfo[,] _tiles;

        //protected SpriteRenderer[,] _tilesRendering;
        //protected Transform[,] _tilesRendering;
        protected Bounds _bounds;
        protected Vector2 _startPos;
        protected float _scale;
        protected Vector2 _offset;
        protected BoxCollider2D _collider2d;

        protected Camera gameCamera;
        protected Transform _transform;
        protected TileInfo[,] tempTiles;

        protected virtual void Awake()
        {
            _collider2d = GetComponent<BoxCollider2D>();
            _tiles = new TileInfo[boardSize.x, boardSize.y];
            _bounds = _collider2d.bounds;
            _offset = tile.GetComponent<SpriteRenderer>().bounds.size;

            GenerateTilemap();
            GenerateTiles();
        }

        protected virtual void Start()
        {
            _transform = transform;
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
            for (int i = 0; i < _tiles.GetLength(0); ++i)
                for (int j = 0; j < _tiles.GetLength(1); ++j)
                {
                    var tilePos = _tiles[i, j].tileTransform.position;
                    float offsetHalf = _offset.x / 2;

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
            bool isAddedComb;

            try
            {
                Vector2Int boardIndex = GetTileIndex(combTiles[startCombPos.y, startCombPos.x].tileTransform.position);

                //if (_tiles[boardIndex.y, boardIndex.x].fillingType == TileFilling.Filled)
                //    return false;

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

                        if (combTiles[x1, y1].fillingType == TileFilling.Filled)
                        {
                            _tiles[x, y].sprite.sprite = combTiles[x1, y1].sprite.sprite;
                            _tiles[x, y].fillingType = TileFilling.Filled;
                            _tiles[x, y].sprite.transform.rotation = rotation;
                        }
                    }
                }

                isAddedComb = true;
            }
            catch
            {
                return false;
            }

            if(isAddedComb)
                CheckOnRowCompleted();

            return isAddedComb;
        }

        private void CheckOnRowCompleted()
        {
            int mostLowerRow = -1;

            //rows
            for (int j = 0; j < _tiles.GetLength(1); ++j)
            {
                bool isCompletedRow = true;

                //cols
                for (int i = 0; i < _tiles.GetLength(0); ++i)
                    if (_tiles[i, j].fillingType != TileFilling.Filled)
                    {
                        isCompletedRow = false;
                        break;
                    }

                if (isCompletedRow)
                    mostLowerRow = j;
            }

            if (mostLowerRow >= 0)
            {
                VFXManager.Instance.PlaySparklesEffect(
                    new Vector2(_collider2d.bounds.center.x, _tiles[0, mostLowerRow].tileTransform.position.y),
                    new Vector3(_scale * _tiles.GetLength(0), _scale));

                StartCoroutine(ShiftBoard(mostLowerRow));

                print("completed row: " + mostLowerRow);
            }
        }

        public IEnumerator ShiftBoard(int compoundIndex)
        {
            InputController.Instance.BlockInput(true);
            int shiftTimes = compoundIndex;

            while (shiftTimes >= 0)
            {
                tempTiles = FillTempTiles();
                yield return new WaitForSeconds(shiftDelay);
                ShiftExistingTiles(compoundIndex);
                shiftTimes--;
            }
            InputController.Instance.BlockInput(false);
        }

        private void ShiftExistingTiles(int compoundIndex)
        {
            for (int targetRow = 0, nextTempRow = 1; nextTempRow < boardSize.y; targetRow++, nextTempRow++)
            {
                for (int columnIndex = 0; columnIndex < boardSize.x; columnIndex++)
                {
                    if (compoundIndex == boardSize.y - 1 && nextTempRow == boardSize.y)
                    {
                        _tiles[columnIndex, compoundIndex].sprite.sprite = tile.GetComponent<SpriteRenderer>().sprite;
                        _tiles[columnIndex, compoundIndex].fillingType = TileFilling.Empty;
                        _tiles[columnIndex, compoundIndex].tileTransform.rotation = Quaternion.Euler(0, 0, 0);
                        continue;
                    }
                    _tiles[columnIndex, targetRow].sprite.sprite = tempTiles[columnIndex, nextTempRow].sprite.sprite;
                    _tiles[columnIndex, targetRow].fillingType = tempTiles[columnIndex, nextTempRow].fillingType;

                    if (_tiles[columnIndex, targetRow].fillingType == TileFilling.Empty)
                        _tiles[columnIndex, targetRow].tileTransform.rotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }

        private TileInfo[,] FillTempTiles()
        {
            TileInfo[,] tempTiles = new TileInfo[boardSize.x, boardSize.y];

            for (int columnIndex = 0; columnIndex < boardSize.x; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex < boardSize.y; rowIndex++)
                    tempTiles[columnIndex, rowIndex] = _tiles[columnIndex, rowIndex];
            }
            return tempTiles;
        }
    }
}

