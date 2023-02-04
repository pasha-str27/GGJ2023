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
            [SerializeField] Board gameBoard;
            [SerializeField] GameObject combinationPrefab;
            [SerializeField] PossibleTileInfo[] baseTiles;
            [SerializeField] CombinationsList combinations;
            [SerializeField] List<Transform> spawnPositions;

            Dictionary<Vector3, CombinationBehaviour> availableCombinations;

            private void Start()
            {
                availableCombinations = new Dictionary<Vector3, CombinationBehaviour>();

                foreach (var transf in spawnPositions)
                    availableCombinations[transf.position] = GenerateCombination(transf.position);
            }

            public void Generate(Vector2 pos)
            {
                availableCombinations[pos] = GenerateCombination(pos);
            }

            public List<CombinationBehaviour> GetAvailableCombinations()
            {
                return availableCombinations.Values.ToList();
            }

            [ContextMenu("Generate")]
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
                            var spriteList = baseTiles[Random.Range(0, baseTiles.Length)];
                            combSprites[y, x] = spriteList.sprites[Random.Range(0, spriteList.sprites.Length)];
                            fillInfo[x + 1][y + 1] = TileFilling.Filled;
                        }
                    }
                }

                comb.transform.position = pos;

                var offset = gameBoard.GetOffset();
                var colliderSize = gameBoard.GetColliderSize();

                comb.SetTileInfo(gameBoard.GetSize(), shape, offset, colliderSize);

                comb.GenerateTiles();

                comb.SetFillingInfo(fillInfo);
                comb.SetSprites(combSprites);

                for (int i = 0; i < Random.Range(0, 4); ++i)
                    comb.Rotate();

                return comb;
            }
        }
    }
}