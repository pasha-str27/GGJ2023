using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

namespace CoreGame
{
    using Tile;

    public class Board : MonoBehaviour
    {
        [SerializeField] int minCellsForCombination = 5;
        [SerializeField] string cameraTag = "MainCamera";
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
        protected BoxCollider2D _collider2d;

        protected Camera gameCamera;
        protected Transform _transform;
        protected TileInfo[,] tempTiles;

        bool isShifting = false;

        Vector2Int? startCheckPos;

        Sprite baseTileSprite;

        protected virtual void Awake()
        {
            _collider2d = GetComponent<BoxCollider2D>();
            _tiles = new TileInfo[boardSize.x, boardSize.y];
            _bounds = _collider2d.bounds;
            _offset = tile.GetComponent<SpriteRenderer>().bounds.size;
            baseTileSprite = tile.GetComponent<SpriteRenderer>().sprite;

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
                    _tiles[x, y].backSprite = newTile.GetComponent<SpriteRenderer>();
                    _tiles[x, y].rootSprite = newTile.transform.GetChild(0).GetComponent<SpriteRenderer>();
                    _tiles[x, y].colorFillID = -1;
                    _tiles[x, y].tileTransform = newTile.transform;
                    _tiles[x, y].sparkles = newTile.transform.GetChild(1).GetComponent<UnityEngine.VFX.VisualEffect>();
                    _tiles[x, y].sparkles.SetVector3("Center", Vector3.zero);
                    _tiles[x, y].sparkles.SetVector3("BoxSize", Vector2.one);
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

        public bool TryAddCombination(Vector2Int startCombPos, int[,] combFilling, TileInfo[,] combTiles)
        {
            bool isAddedComb;

            if (isShifting)
                return false;

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

                        if (combTiles[x1, y1].colorFillID >= 0)
                        {
                            if (startCheckPos == null)
                                startCheckPos = new Vector2Int(x, y);

                            _tiles[x, y].backSprite.color = combTiles[x1, y1].backSprite.color;
                            _tiles[x, y].backSprite.sprite = combTiles[x1, y1].backSprite.sprite;
                            _tiles[x, y].rootSprite.sprite = combTiles[x1, y1].rootSprite.sprite;
                            _tiles[x, y].colorFillID = combTiles[x1, y1].colorFillID;

                            var tileRot = combTiles[x1, y1].tileTransform.localRotation.eulerAngles.z;

                            _tiles[x, y].tileTransform.rotation = Quaternion.Euler(0, 0, tileRot + rotation.eulerAngles.z);
                        }
                    }
                }

                //print(Utils.Matrix.ToSting(_tiles));

                isAddedComb = true;
            }
            catch
            {
                return false;
            }

            if (isAddedComb)
                CheckCombinationCompleted();

            return isAddedComb;
        }

        void CheckCombinationCompleted()
        {
            var completedCells = GetCompletedCells((Vector2Int)startCheckPos);

            if (completedCells.Count >= minCellsForCombination)
            {

                Player.Instance.CalculateAndAddScore(completedCells.Count);
                Player.Instance.CalculateAndAddComb(completedCells.Count);

                //int cell = 0;

                foreach (var pos in completedCells)
                {
                    //DOVirtual.DelayedCall(cell * 0.01f * 3, delegate
                    //{
                    //    VFXManager.Instance.PlaySparklesEffect(_tiles[pos.x, pos.y].tileTransform.position,
                    //            new Vector3(_scale, _scale));
                    //});

                    //++cell;
                    _tiles[pos.x, pos.y].sparkles.Play();
                    _tiles[pos.x, pos.y].colorFillID = -1;
                    _tiles[pos.x, pos.y].rootSprite.sprite = null;
                    _tiles[pos.x, pos.y].backSprite.sprite = baseTileSprite;
                    _tiles[pos.x, pos.y].backSprite.color = Color.white;
                    _tiles[pos.x, pos.y].tileTransform.rotation = Quaternion.identity;
                }

                print("combination is done");
            }

            startCheckPos = null;

            //if (!CheckOnRowCompleted())
            //{
            //if (CheckOnGameOver())
            //{
            //    DOVirtual.DelayedCall(2.75f, CameraController.Instance.ShowTree);
            //    CameraController.Instance.GameOver();
            //    //InputController.Instance.BlockInput(true);

            //    Debug.LogError("GAME OVER");
            //}
            //}
        }

        List<Vector2Int> GetCompletedCells(Vector2Int startPos)
        {
            int colorID = _tiles[startPos.x, startPos.y].colorFillID;

            Stack<List<Vector2Int>> cellsSameColor = new Stack<List<Vector2Int>>();

            List<Vector2Int> positionsList = new List<Vector2Int>();

            positionsList.Add(startPos);
            cellsSameColor.Push(positionsList);

            List<Vector2Int> checkedPositions = new List<Vector2Int>();
            checkedPositions.Add(startPos);

            do
            {
                List<Vector2Int> secondStepPositions = new List<Vector2Int>();

                foreach (var pos in cellsSameColor.Peek())
                {
                    void TryAddPosition(Vector2Int secondPos)
                    {
                        if (!checkedPositions.Contains(secondPos)
                            && IsPointOnBoard(secondPos)
                            && _tiles[secondPos.x, secondPos.y].colorFillID == colorID)
                        {
                            checkedPositions.Add(secondPos);
                            secondStepPositions.Add(secondPos);
                        }
                    }

                    TryAddPosition(pos + Vector2Int.up);
                    TryAddPosition(pos + Vector2Int.right);
                    TryAddPosition(pos + Vector2Int.down);
                    TryAddPosition(pos + Vector2Int.left);
                }

                cellsSameColor.Push(secondStepPositions);
            }
            while (cellsSameColor.Peek().Count > 0);

            return checkedPositions;
        }

        bool IsPointOnBoard(Vector2Int pos)
        {
            return pos.x >= 0 && pos.y >= 0
                && pos.y < _tiles.GetLength(1)
                && pos.x < _tiles.GetLength(0);
        }

        public TileInfo[,] GetTiles() => _tiles;

        [ContextMenu("CheckOnGameOver")]
        public bool CheckOnGameOver()
        {
            var availableCombinations = CombinationGenerator.Instance.GetAvailableCombinations();

            foreach (var comb in availableCombinations)
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

        int[,] GetFillingMatrix(TileInfo[,] comb)
        {
            int[,] matrix = new int[comb.GetLength(0), comb.GetLength(1)];

            for (int i = 0; i < comb.GetLength(0); ++i)
                for (int j = 0; j < comb.GetLength(1); ++j)
                    matrix[i, j] = comb[i, j].colorFillID;

            return matrix;
        }

        bool CanAddCombination(Vector2Int boardIndex, int[,] fillingInfo, Vector2Int startCombPos)
        {
            for (int y, y1 = 0; y1 < fillingInfo.GetLength(1); ++y1)
            {
                for (int x, x1 = 0; x1 < fillingInfo.GetLength(0); ++x1)
                {
                    x = boardIndex.y - startCombPos.y + x1;
                    y = boardIndex.x - startCombPos.x + y1;

                    if (x >= _tiles.GetLength(0))
                        return false;

                    if (y >= _tiles.GetLength(1))
                        return false;

                    //1-x
                    if (fillingInfo[x1, y1] >= 0)
                    {
                        //1-x
                        if (_tiles[x, y].colorFillID >= 0)
                            return false;
                    }
                }
            }

            return true;
        }

        //private bool CheckOnRowCompleted()
        //{
        //    int mostLowerRow = -1;

        //    int rowsCompleted = 0;

        //    //rows
        //    for (int j = 0; j < _tiles.GetLength(1); ++j)
        //    {
        //        bool isCompletedRow = true;

        //        //cols
        //        for (int i = 0; i < _tiles.GetLength(0); ++i)
        //            if (_tiles[i, j].fillingType != TileFilling.Filled)
        //            {
        //                isCompletedRow = false;
        //                break;
        //            }

        //        if (isCompletedRow)
        //        {
        //            print("completed row: " + j);
        //            ++rowsCompleted;

        //            //Vector2 particlesCenter = new Vector2(_collider2d.bounds.center.x, _tiles[0, j].tileTransform.position.y);

        //            //DOVirtual.DelayedCall(rowsCompleted * 0.01f, delegate {
        //            //    VFXManager.Instance.PlaySparklesEffect(particlesCenter,
        //            //            new Vector3(_scale * _tiles.GetLength(0), _scale)); });

        //            Player.Instance.AddScore(_tiles.GetLength(0));
        //            Player.Instance.AddCombCountForRow();
        //            CombinationGenerator.Instance.TryGenerate();
        //            mostLowerRow = j;
        //        }
        //    }

        //    return mostLowerRow >= 0;
        //}
    }
}

