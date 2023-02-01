using UnityEditor;
using GameCore.Tile;

namespace Array2DEditor
{
    [CustomPropertyDrawer(typeof(TileFillingInfo))]
    public class Array2DTileFillingEnumDrawer : Array2DEnumDrawer<TileFilling> { }
}
