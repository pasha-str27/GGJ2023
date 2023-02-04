using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using CoreGame.Tile;
using DG.Tweening;

public class Player : SingletonComponent<Player>
{
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private TextMeshProUGUI combinationCounter;
    [SerializeField] private List<GameObject> trees;
    [SerializeField] private List<int> scoreToNextStage;
    private int _treeStage;
    private int _score;
    private int _combinationCounter;

    [SerializeField] int startScoreCount = 50;
    [SerializeField] int startCombCount = 15;
    [SerializeField] int newCombCountForRow = 3;

    void Awake()
    {
        if (score == null || combinationCounter == null || !trees.Any())
            Debug.LogError("Set objects to player");
    }

    void Start()
    {
        AddScore(startScoreCount);
        AddCombCounter(startCombCount);
    }

    public void AddScore(int v)
    {
        score.text = (_score += v).ToString();

        for (int i = 0; i < scoreToNextStage.Count; ++i)
            if (_score < scoreToNextStage[i])
            {
                SetNextTreeStage(i);
                return;
            }

        SetNextTreeStage(scoreToNextStage.Count);
    }

    public bool HaveCombinations(int spawnedCombs) => _combinationCounter - spawnedCombs > 0;

    public void UseComb() => AddCombCounter(-1);

    void AddCombCounter(int v)
    {
        _combinationCounter = Mathf.Max(0, _combinationCounter + v);
        combinationCounter.text = _combinationCounter.ToString();

        if(_combinationCounter == 0)
            DOVirtual.DelayedCall(0.75f, CameraController.Instance.ShowTree);
    }

    public void AddCombCountForRow()
    {
        AddCombCounter(newCombCountForRow);
        CombinationGenerator.Instance.TryGenerate();
    }

    public int GetScore() => _score;

    public int GetCombCount() => _combinationCounter;

    private void SetNextTreeStage(int stage)
    {
        foreach (var tree in trees)
            tree.SetActive(false);

        trees[stage].SetActive(true);
    }
}
