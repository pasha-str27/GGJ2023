using UnityEngine;

namespace Array2DEditor
{
    [System.Serializable]
    public class Array2DBool : Array2D<bool>
    {
        [SerializeField]
        CellRowBool[] cells = new CellRowBool[Consts.defaultGridSize];

        protected override CellRow<bool> GetCellRow(int idx)
        {
            return cells[idx];
        }

        public bool this[int i, int j]
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

        public Vector2Int GetSize() => new Vector2Int(GridSize.x, GridSize.y);
    }
}
