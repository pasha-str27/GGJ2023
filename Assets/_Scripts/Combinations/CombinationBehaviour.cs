using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;
using UnityEngine.Rendering;
using DG.Tweening;


namespace CoreGame
{
    using Tile;

    public class CombinationBehaviour : Board
    {
        [SerializeField] float GOScale = 0.75f;
        [SerializeField] float moveSpeed = 5;
        [SerializeField] float dragThreshold = 0.1f;

        public LayerMask layerMask;

        private int[,] fillingInfo;

        CombinationShape combShape;
        Sprite[,] combSprites;

        Vector2 startPosition;
        Vector2 inputOffset;

        SortingGroup sorting;

        bool wasClickOnTrigger = false;

        Vector2 moveTarget;
        Vector2 scaleTarget;
        Vector2 startScale;

        int colorID = -1;

        Board gameBoard = null;

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

            _transform = transform;
            startScale = Vector3.one * GOScale;

            _transform.localScale = Vector3.zero;
            _transform.DOScale(startScale, 0.3f).OnComplete(delegate { _collider2d.enabled = true; });
        }

        public void SetSprites(Sprite[,] sprites, Sprite backSprite, Color tileColor)
        {
            for (int i = 0; i < sprites.GetLength(0); ++i)
                for (int j = 0; j < sprites.GetLength(1); ++j)
                {
                    _tiles[j, i].backSprite.color = tileColor;

                    if (sprites[i, j])
                    {
                        _tiles[j, i].rootSprite.sprite = sprites[i, j];
                        _tiles[j, i].backSprite.sprite = backSprite;
                    }
                    else
                    {
                        _tiles[j, i].rootSprite.sprite = null;
                        _tiles[j, i].backSprite.sprite = null;
                    }
                }
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

                    trans.localScale *= _scale;

                    _tiles[x, y] = new TileInfo();
                    _tiles[x, y].backSprite = newTile.GetComponent<SpriteRenderer>();
                    _tiles[x, y].rootSprite = newTile.transform.GetChild(0).GetComponent<SpriteRenderer>();
                    _tiles[x, y].colorFillID = -1;
                    _tiles[x, y].tileTransform = newTile.transform;

                    if (combShape.shape[y, x])
                        _tiles[x, y].colorFillID = colorID;
                    else
                    {
                        _tiles[x, y].rootSprite.sprite = null;
                        _tiles[x, y].backSprite.sprite = null;
                    }
                }
            }
        }

        private void Update()
        {
            //REMOVE THIS IN FUTURE!!!!!!!!!!!!!!!!!!!!!!!
            if (moveTarget == Vector2.zero)
                return;

            _transform.position = Vector2.MoveTowards(transform.position, moveTarget, Time.deltaTime * moveSpeed);
        }

        private void OnMouseDrag()
        {
            if (!wasClickOnTrigger)
                return;

            if (Vector2.Distance(startPosition, transform.position) > dragThreshold)
                _transform.localScale = Vector2.one;

            moveTarget = (Vector2)gameCamera.ScreenToWorldPoint(Input.mousePosition) - inputOffset;
        }

        private void OnMouseDown()
        {
            if (InputController.Instance.IsInputBlocked())
                return;

            //print(Utils.Matrix.ToSting(fillingInfo));

            wasClickOnTrigger = true;

            InputController.Instance.BlockInput(true);
            sorting.sortingOrder++;
            startPosition = transform.position;
            inputOffset = (Vector2)gameCamera.ScreenToWorldPoint(Input.mousePosition) - startPosition;
        }

        private void OnMouseUp()
        {
            if (!wasClickOnTrigger && InputController.Instance.IsInputBlocked())
                return;

            if (Vector2.Distance(startPosition, transform.position) < dragThreshold)
                Rotate();

            _transform.localScale = startScale;

            InputController.Instance.BlockInput(false);
            wasClickOnTrigger = false;

            if (MoveCombinationToBoard())
            {
                UseCombination();
                return;
            }

            sorting.sortingOrder--;
            moveTarget = startPosition;
        }

        void UseCombination()
        {
            int tilesCount = 0;

            foreach (Transform child in _transform)
            {
                var sprite = child.GetComponent<SpriteRenderer>();
                if (sprite != null && sprite.enabled)
                    tilesCount++;
            }

            //var mousePos = gameCamera.ScreenToWorldPoint(Input.mousePosition);

            //VFXManager.Instance.PlayDustEffect(new Vector3(mousePos.x, mousePos.y, 0));

            //Player.Instance.AddScore(tilesCount);
            Player.Instance.UseComb();
            CombinationGenerator.Instance.RemoveCombAt(startPosition);
            Destroy(gameObject);
            CombinationGenerator.Instance.TryGenerate();

            if (gameBoard && gameBoard.CheckOnGameOver())
            {
                DOVirtual.DelayedCall(2.75f, CameraController.Instance.ShowTree);
                CameraController.Instance.GameOver();
                //InputController.Instance.BlockInput(true);

                Debug.LogError("GAME OVER");
            }
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

                    gameBoard = hit.collider.gameObject.GetComponent<Board>();

                    return gameBoard.TryAddCombination(clickedTileIndex, fillingInfo, _tiles);
                }
            }

            return false;
        }

        public void Rotate()
        {
            fillingInfo = Utils.Matrix.RotateMatrixClockwise(fillingInfo);
            _tiles = Utils.Matrix.RotateMatrixClockwise(_tiles);
            //fillingInfo = newMatrix;

            // print(Utils.Matrix.ToSting(fillingInfo));

            var oldRotation = transform.rotation.eulerAngles;

            transform.rotation = Quaternion.Euler(oldRotation + Vector3.back * 90);
        }

        public void SetFillingInfo(int[][] fillInfo, int colorID)
        {
            this.colorID = colorID;

            fillingInfo = new int[fillInfo.Length, fillInfo[0].Length];

            for (int i = 0; i < fillInfo.Length; ++i)
            {
                for (int j = 0; j < fillInfo[i].Length; ++j)
                    fillingInfo[i, j] = fillInfo[i][j];
            }
        }
    }
}