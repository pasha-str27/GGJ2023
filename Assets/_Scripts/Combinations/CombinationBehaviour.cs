using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using UnityEngine.Rendering;

namespace CoreGame
{
    using Tile;

    public class CombinationBehaviour : Board
    {
        [SerializeField] float moveSpeed = 5;
        [SerializeField] float dragThreshold = 0.1f;

        public LayerMask layerMask;

        private TileFilling[,] fillingInfo;

        CombinationShape combShape;
        Sprite[,] combSprites;

        Vector2 startPosition;
        Vector2 inputOffset;

        SortingGroup sorting;

        bool wasClickOnTrigger = false;

        Vector2 moveTarget;


        protected override void Awake()
        {
            //_tiles = new GameObject[boardSize.x, boardSize.y];
            //_bounds = collider2d.bounds;
            _collider2d = GetComponent<BoxCollider2D>();
            _offset = tile.GetComponent<SpriteRenderer>().bounds.size;
        }

        protected override void Start()
        {
            base.Start();
            sorting = gameObject.GetComponent<SortingGroup>();
            moveTarget = transform.position;
        }

        public void SetTileInfo(Vector2 size, CombinationShape shape, Vector2 offset, float colliderSize)
        {
            this._offset = offset;
            _scale = size.x;
            combShape = shape;
            boardSize = shape.shape.GetSize();
            _tiles = new TileInfo[boardSize.x, boardSize.y];

            _collider2d.size = new Vector2(colliderSize * boardSize.x, colliderSize * boardSize.y); 
            GenerateTilemap();
        }

        public void SetSprites(Sprite[,] sprites)
        {
            for (int i = 0; i < sprites.GetLength(0); ++i)
                for (int j = 0; j < sprites.GetLength(1); ++j)
                    _tiles[j, i].sprite.sprite = sprites[i, j];
        }

        protected override void GenerateTilemap()
        {
            //_offset *= _scale;
            _bounds = _collider2d.bounds;
            _startPos = -_bounds.extents;
            _startPos.x += _offset.x * 0.5f;
            _startPos.y += _offset.y * 0.5f;
        }

        public override void GenerateTiles()
        {
            for (int y = 0; y < combShape.shape.GridSize.y; y++)
            {
                for (int x = 0; x < combShape.shape.GridSize.x; x++)
                {
                    GameObject newTile;
                    Vector2 newTilePos = new Vector3((_startPos.x + (_offset.x * x)), -(_startPos.y + (_offset.y * y)));

                    newTile = Instantiate(tile, gridContainer.transform);

                    Transform trans = newTile.transform;

                    newTile.transform.localPosition = newTilePos;

                    //newTile.GetComponent<SpriteRenderer>().sprite = combSprites[y, x];

                    trans.localScale *= _scale;

                    _tiles[x, y] = new TileInfo();
                    _tiles[x, y].sprite = newTile.GetComponent<SpriteRenderer>();
                    _tiles[x, y].fillingType = TileFilling.Empty;
                    _tiles[x, y].tileTransform = newTile.transform;

                    if (combShape.shape[y, x])
                        _tiles[x, y].fillingType = TileFilling.Filled;
                    else
                        _tiles[x, y].sprite.enabled = false;
                }
            }
        }

        private void Update()
        {
            _transform.position = Vector2.MoveTowards(transform.position, moveTarget, Time.deltaTime * moveSpeed);
        }

        private void OnMouseDrag()
        {
            if (!wasClickOnTrigger)
                return;

            moveTarget = (Vector2)gameCamera.ScreenToWorldPoint(Input.mousePosition) - inputOffset;
        }

        private void OnMouseDown()
        {
            if (InputController.Instance.IsInputBlocked())
                return;

            wasClickOnTrigger = true;

            InputController.Instance.BlockInput(true);
            sorting.sortingOrder++;
            startPosition = transform.position;
            inputOffset = (Vector2)gameCamera.ScreenToWorldPoint(Input.mousePosition) - startPosition;
        }

        private void OnMouseUp()
        {
            if (Vector2.Distance(startPosition, transform.position) < dragThreshold)
                Rotate();

            InputController.Instance.BlockInput(false);
            wasClickOnTrigger = false;

            if (MoveCombinationToBoard())
            {
                CombinationGenerator.Instance.GenerateCombination(startPosition);
                Destroy(gameObject);
                return;
            }

            sorting.sortingOrder--;
            moveTarget = startPosition;
        }

        bool MoveCombinationToBoard()
        {
            Vector2 worldPoint = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, 0, layerMask);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Board"))
                {
                    Vector2Int clickedTileIndex = GetTileIndex(worldPoint);

                    return hit.collider.gameObject.GetComponent<Board>().TryAddCombination(clickedTileIndex, fillingInfo, _tiles);
                }
            }

            return false;
        }

        void Rotate()
        {
            fillingInfo = Utils.Matrix.RotateMatrixClockwise(fillingInfo);
            _tiles = Utils.Matrix.RotateMatrixClockwise(_tiles);
            //fillingInfo = newMatrix;

            // print(Utils.Matrix.ToSting(fillingInfo));

            var oldRotation = transform.rotation.eulerAngles;

            transform.rotation = Quaternion.Euler(oldRotation + Vector3.back * 90);
        }

        public void SetFillingInfo(TileFilling[][] fillInfo)
        {
            fillingInfo = new TileFilling[fillInfo.Length, fillInfo[0].Length];

            for (int i = 0; i < fillInfo.Length; ++i) 
            {
                for (int j = 0; j < fillInfo[i].Length; ++j)
                    fillingInfo[i, j] = fillInfo[i][j];
            }
        }
    }
}