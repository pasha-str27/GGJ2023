using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;

namespace CoreGame
{
    using Tile;

    public class CombinationBehaviour : Board
    {

        [SerializeField] float dragThreshold = 0.1f;
        [SerializeField]
        private TileFilling[,] fillingInfo;

        Vector2 tileSize;
        CombinationShape combShape;
        Sprite[,] combSprites;

        Vector2 startPosition;
        Vector2 inputOffset;

        protected override void Awake()
        {
            _tiles = new GameObject[boardSize.x, boardSize.y];
            //_bounds = collider2d.bounds;
            _offset = tile.GetComponent<SpriteRenderer>().bounds.size;
        }

        public void SetTileInfo(Vector2 size, CombinationShape shape, Vector2 offset, float colliderSize)
        {
            this._offset = offset;
            _scale = size.x;
            print(_scale);
            combShape = shape;
            boardSize = shape.shape.GetSize();
            collider2d.size = new Vector2(colliderSize * boardSize.x, colliderSize * boardSize.y); 
            GenerateTilemap();
        }

        public void SetSprites(Sprite[,] sprites) => combSprites = sprites;

        protected override void GenerateTilemap()
        {
            //_offset *= _scale;
            _bounds = collider2d.bounds;

            Vector3 scaledGridSize = new Vector2(boardSize.x * _offset.x, boardSize.y * _offset.y);

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
                    if (combShape.shape[y, x])
                    {
                        GameObject newTile;
                        Vector2 newTilePos = new Vector3((_startPos.x + (_offset.x * x)), -(_startPos.y + (_offset.y * y)));

                        newTile = Instantiate(tile, gridContainer.transform);

                        Transform trans = newTile.transform;

                        newTile.transform.localPosition = newTilePos;

                        newTile.GetComponent<SpriteRenderer>().sprite = combSprites[y, x];

                        trans.localScale *= _scale;
                        //var currTile = newTile.GetComponent<Tile.Tile>();
                        //currTile.Position = new Vector2Int(x, y);
                    }      
                }
            }
        }

        private void OnMouseDrag()
        {
            transform.position = (Vector2)gameCamera.ScreenToWorldPoint(Input.mousePosition) - inputOffset;
        }

        private void OnMouseDown()
        {
            startPosition = transform.position;
            inputOffset = (Vector2)gameCamera.ScreenToWorldPoint(Input.mousePosition) - startPosition;
        }

        private void OnMouseUp()
        {
            if (Vector2.Distance(startPosition, transform.position) < dragThreshold)
                Rotate();

            transform.position = startPosition;
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
                    print(clickedTileIndex);

                    return hit.collider.gameObject.GetComponent<Board>().TryAddCombination(clickedTileIndex, fillingInfo, _tiles);
                }
            }

            return false;
        }

        void Rotate()
        {
            fillingInfo = Utils.Matrix.RotateMatrixClockwise(fillingInfo);
            //fillingInfo = newMatrix;

            print(Utils.Matrix.ToSting(fillingInfo));

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