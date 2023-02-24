using System;
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

    [Header("(tile-C1)*C2")]
    [SerializeField] float scoreC1 = 3f;
    [SerializeField] float scoreC2 = 7f;
    [Header("[(e^(t - C1)) ^ C2 - C3]")]
    [SerializeField] float combC1 = 0.5f;
    [SerializeField] float combC2 = 1f;
    [SerializeField] int combC3 = 1;

    //[SerializeField] int newCombCountForRow = 3;

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

    public void CalculateAndAddScore(int tileCount)
    {
        AddScore(Mathf.FloorToInt((tileCount + scoreC1) * scoreC2));
    }

    public void CalculateAndAddComb(int tileCount)
    {
        AddCombCounter(Mathf.FloorToInt(Mathf.Pow(Mathf.Exp(tileCount - combC1), combC2)) - combC3);
    }

    void AddScore(int v)
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

    void AddCombCounter(int v)
    {
        _combinationCounter = Mathf.Max(0, _combinationCounter + v);
        combinationCounter.text = _combinationCounter.ToString();

        if (_combinationCounter == 0)
        {
            DOVirtual.DelayedCall(2.75f, CameraController.Instance.ShowTree);
            CameraController.Instance.GameOver();
        }
    }

    // public void AddCombCountForRow()
    // {
    //     AddCombCounter(newCombCountForRow);
    //     CombinationGenerator.Instance.TryGenerate();
    // }

    public bool HaveCombinations(int spawnedCombs) => _combinationCounter - spawnedCombs > 0;

    public void UseComb() => AddCombCounter(-1);

    public int GetScore() => _score;

    public int GetCombCount() => _combinationCounter;

    private void SetNextTreeStage(int stage)
    {
        foreach (var tree in trees)
            tree.SetActive(false);

        trees[stage].SetActive(true);
    }
}
