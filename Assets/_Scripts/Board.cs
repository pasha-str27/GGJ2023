using UnityEngine;

namespace CoreGame
{
    using Tile;

    public class Board : MonoBehaviour
    {
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
            gameCamera = GameObject.FindGameObjectWithTag(cameraTag).GetComponent<Camera>();
        }

        protected virtual void GenerateTilemap()
        {
            float xScale = _bounds.size.x / (boardSize.x * _offset.x);
            float yScale = _bounds.size.y / (boardSize.y * _offset.y);

            _scale = Mathf.Min(xScale, yScale);
            _offset *= _scale;

            Vector3 scaledGridSize = new Vector2(boardSize.x * _offset.x, boardSize.y * _offset.y);

            _startPos = _bounds.center - scaledGridSize * 0.5f;
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
                    Vector2 newTilePos = new Vector3(_startPos.x + (_offset.x * x), _startPos.y + (_offset.y * y));

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

        public void ShiftBoard(int compoundRowIndex)
        {
            if (compoundRowIndex >= boardSize.y - 1)
            {
                Debug.LogError("Compound row index higher than board size!");
                return;
            }
            ShiftExistingTiles(compoundRowIndex);
            FillWithBlankTiles(compoundRowIndex);
        }

        private void ShiftExistingTiles(int compoundRowIndex)
        {
            int tempCompoundRowIndex = compoundRowIndex;
            TileInfo[,] tempTiles = FillTempTiles();

            for (int columnIndex = 0; columnIndex < boardSize.x; columnIndex++)
            {
                for (int rowIndex = boardSize.y - 1; rowIndex > compoundRowIndex && compoundRowIndex >= 0; rowIndex--)
                {
                    _tiles[columnIndex, rowIndex].sprite.sprite = tempTiles[columnIndex, compoundRowIndex].sprite.sprite;
                    _tiles[columnIndex, rowIndex].fillingType = tempTiles[columnIndex, compoundRowIndex].fillingType;
                    compoundRowIndex--;
                }
                compoundRowIndex = tempCompoundRowIndex;
            }
        }

        private void FillWithBlankTiles(int compoundRowIndex)
        {
            for (int columnIndex = 0; columnIndex < boardSize.x; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex <= compoundRowIndex; rowIndex++)
                {
                    _tiles[columnIndex, rowIndex].sprite.sprite = tile.GetComponent<SpriteRenderer>().sprite;
                    _tiles[columnIndex, rowIndex].fillingType = TileFilling.Empty;
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

