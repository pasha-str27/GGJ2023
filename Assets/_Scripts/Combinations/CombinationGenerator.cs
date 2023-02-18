using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomGeneratorWithWeight;
using System.Linq;

namespace CoreGame
{
    namespace Tile
    {
        public class CombinationGenerator : SingletonComponent<CombinationGenerator>
        {
            [SerializeField] Sprite tileBackground;
            [SerializeField] Board gameBoard;
            [SerializeField] GameObject combinationPrefab;
            //[SerializeField] PossibleTileInfo[] baseTiles;
            [SerializeField] CombinationsList combinations;
            [SerializeField] CombinationColor combColors;
            [SerializeField] List<Transform> spawnPositions;

            Dictionary<Vector3, CombinationBehaviour> availableCombinations;

            private void Start()
            {
                availableCombinations = new Dictionary<Vector3, CombinationBehaviour>();

                foreach (var transf in spawnPositions)
                    availableCombinations[transf.position] = GenerateCombination(transf.position);
            }

            public void TryGenerate()
            {
                if (Player.Instance.HaveCombinations(availableCombinations.Count(x => x.Value != null)))
                {
                    foreach (var key in availableCombinations.Keys)
                    {
                        if (availableCombinations[key] == null)
                        {
                            Generate(key);
                            return;
                        }
                    }
                }
            }

            public void RemoveCombAt(Vector2 pos) => availableCombinations[pos] = null;

            public void Generate(Vector2 pos)
            {
                availableCombinations[pos] = GenerateCombination(pos);
            }

            public List<CombinationBehaviour> GetAvailableCombinations()
            {
                return availableCombinations.Values.Where(x => x != null).ToList();
            }

            CombinationBehaviour GenerateCombination(Vector2 pos)
            {
                var shape = GetItemWithWeight.GetItem(combinations.shapes);

                var comb = Instantiate(combinationPrefab).GetComponent<CombinationBehaviour>();

                Sprite[,] combSprites = new Sprite[shape.shape.GetSize().y, shape.shape.GetSize().x];

                TileFilling[][] fillInfo = new TileFilling[shape.shape.GridSize.x + 2][];

                for (int i = 0; i < fillInfo.Length; ++i)
                    fillInfo[i] = new TileFilling[shape.shape.GridSize.y + 2];

                for (int x = 0; x < shape.shape.GetSize().x; x++)
                {
                    for (int y = 0; y < shape.shape.GetSize().y; y++)
                    {
                        if (shape.shape[y, x])
                        {
                            combSprites[y, x] = shape.view.GetCell(x, y);
                            fillInfo[x + 1][y + 1] = TileFilling.Filled;
                        }
                    }
                }

                int colorID = combColors.GetColorIndex();

                comb.transform.position = pos;

                var offset = gameBoard.GetOffset();
                var colliderSize = gameBoard.GetColliderSize();

                comb.SetTileInfo(gameBoard.GetSize(), shape, offset, colliderSize);

                comb.GenerateTiles();

                comb.SetFillingInfo(fillInfo, colorID);
                comb.SetSprites(combSprites, tileBackground, combColors.GetColor(colorID));

                for (int i = 0; i < Random.Range(0, 4); ++i)
                    comb.Rotate();

                return comb;
            }
        }
    }
}