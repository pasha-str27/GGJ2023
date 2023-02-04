using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class Player : SingletonComponent<Player>
{
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private TextMeshProUGUI combinationCounter;
    [SerializeField] private List<GameObject> trees;
    [SerializeField] private List<int> scoreToNextStage;
    private int _treeStage;
    private int _score;
    private int _combinationCounter;

    void Awake()
    {
        if (score == null || combinationCounter == null || !trees.Any())
            Debug.LogError("Set objects to player");
    }

    void Start()
    {
        AddScore(50);
        AddCombCounter(0);
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

    public void AddCombCounter(int v) => combinationCounter.text = (_combinationCounter += v).ToString();

    public int GetScore() => _score;

    public int GetCombCount() => _combinationCounter;

    private void SetNextTreeStage(int stage)
    {
        foreach (var tree in trees)
            tree.SetActive(false);

        trees[stage].SetActive(true);
    }
}
