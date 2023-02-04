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

                if (!CanAddCombination(boardIndex, GetFillingMatrix(combTiles), startCombPos))
                    return false;

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

                            var tileRot = combTiles[x1, y1].tileTransform.localRotation.eulerAngles.z;

                            _tiles[x, y].tileTransform.rotation = Quaternion.Euler(0, 0, tileRot + rotation.eulerAngles.z);
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
            {
                if (!CheckOnRowCompleted())
                {
                    if (CheckOnGameOver())
                    {
                        CameraController.Instance.ShowTree();
                        //InputController.Instance.BlockInput(true);

                        Debug.LogError("GAME OVER");
                    }
                }

                if (CheckOnGameOver())
                {
                    CameraController.Instance.ShowTree();
                    //InputController.Instance.BlockInput(true);

                    Debug.LogError("GAME OVER");
                }
            }

            return isAddedComb;
        }

        public TileInfo[,] GetTiles() => _tiles;

        [ContextMenu("CheckOnGameOver")]
        bool CheckOnGameOver()
        {
            var availableCombinations = CombinationGenerator.Instance.GetAvailableCombinations();

            foreach(var comb in availableCombinations)
            {
                var combFilling = GetFillingMatrix(comb.GetTiles());

                for (int rot = 0; rot < 4; ++rot) 
                {
                    //bool canAddCombination = false;

                    for (int i = 0; i < _tiles.GetLength(0); ++i)
                        for (int j = 0; j < _tiles.GetLength(1); ++j)
                        {
                            if (CanAddCombination(new Vector2Int(j, i), combFilling, Vector2Int.zero))
                            {
                                Debug.LogError("You can continue");
                                return false;
                            }
                        }

                    combFilling = Utils.Matrix.RotateMatrixClockwise(combFilling);
                }
            }

            Debug.LogError("GAME OVER");

            return true;
        }

        TileFilling[,] GetFillingMatrix(TileInfo[,] comb)
        {
            TileFilling[,] matrix = new TileFilling[comb.GetLength(0), comb.GetLength(1)];

            for (int i = 0; i < comb.GetLength(0); ++i)
                for (int j = 0; j < comb.GetLength(1); ++j)
                    matrix[i, j] = comb[i, j].fillingType;

            return matrix;
        }

        bool CanAddCombination(Vector2Int boardIndex, TileFilling[,] fillingInfo, Vector2Int startCombPos)
        {
            print(fillingInfo.GetLength(1));
            print(fillingInfo.GetLength(0));

            for (int y, y1 = 0; y1 < fillingInfo.GetLength(1); ++y1)
            {
                for (int x, x1 = 0; x1 < fillingInfo.GetLength(0); ++x1)
                {
                    x = boardIndex.y - startCombPos.y + x1;
                    y = boardIndex.x - startCombPos.x + y1;

                    print("x = " + x);

                    if(x >= _tiles.GetLength(0))
                        return false;

                    if (y >= _tiles.GetLength(1))
                        return false;

                    //1-x
                    if (fillingInfo[x1, y1] == TileFilling.Filled)
                    {
                        //1-x
                        if (_tiles[x, y].fillingType == TileFilling.Filled)
                            return false;
                    }
                }
            }

            return true;
        }

        private bool CheckOnRowCompleted()
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

                //StartCoroutine(ShiftBoard(mostLowerRow));

                print("completed row: " + mostLowerRow);

                return true;
            }

            return false;
        }

        public IEnumerator ShiftBoard(int compoundRowIndex)
        {
            if (compoundRowIndex == 0)
            {
                yield break;
            }

            while (compoundRowIndex > 0)
            {
                tempTiles = FillTempTiles();
                yield return new WaitForSeconds(shiftDelay);
                ShiftExistingTiles(compoundRowIndex);
                compoundRowIndex--;
            }
        }

        [ContextMenu("Shift")]
        public void Shift()
        {
            StartCoroutine(ShiftBoard(compoundRowIndex));
        }

        private void ShiftExistingTiles(int compoundRow)
        {
            for (int targetRow = compoundRow - 1, nextTempRow = compoundRow; nextTempRow < boardSize.y; targetRow++, nextTempRow++)
            {
                for (int columnIndex = 0; columnIndex < boardSize.x; columnIndex++)
                {
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
                {
                    tempTiles[columnIndex, rowIndex] = _tiles[columnIndex, rowIndex];
                }
            }
            return tempTiles;
        }
    }
}

