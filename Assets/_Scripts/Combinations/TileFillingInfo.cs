using UnityEngine;
using GameCore.Tile;

namespace Array2DEditor
{
    [System.Serializable]
    public class TileFillingInfo : Array2D<TileFilling>
    {
        [SerializeField]
        CellRowTileFillingEnum[] cells = new CellRowTileFillingEnum[Consts.defaultGridSize];

        protected override CellRow<TileFilling> GetCellRow(int idx)
        {
            return cells[idx];
        }

        public TileFilling this[int i, int j]
        {
            get 
            { 
                return GetCellRow(i)[j];
            }
            set 
            { 
                cells[i][j] = value; 
            }
        }

        [ContextMenu("Rotate")]
        public void Rotate()
        {
            int n = cells.Length;

            for (int i = 0; i < n / 2; i++)
            {
                for (int j = i; j < n - i - 1; j++)
                {
                    // Swapping elements after each iteration in Clockwise direction
                    var temp = cells[i][j];
                    cells[i][j] = cells[n - 1 - j][i];
                    cells[n - 1 - j][i] = cells[n - 1 - i][n - 1 - j];
                    cells[n - 1 - i][n - 1 - j] = cells[j][n - 1 - i];
                    cells[j][n - 1 - i] = temp;
                }
            }
        }
    }

    [System.Serializable]
    public class CellRowTileFillingEnum : CellRow<TileFilling> { }
}
