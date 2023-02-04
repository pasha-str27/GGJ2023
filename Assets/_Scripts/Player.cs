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
        AddScore(0);
    }

    public void AddScore(int v)
    {
        score.text = (_score += v).ToString();
        if (_score >= scoreToNextStage[_treeStage])
            SetNextTreeStage();
    }

    public void AddCombCounter(int v) => combinationCounter.text = (_combinationCounter += v).ToString();

    public int GerSCore() => _score;

    public int GetCombCount() => _combinationCounter;

    private void SetNextTreeStage()
    {
        DeactivateTree(_treeStage);
        ActivateTreeStage(_treeStage++);
    }

    private void ActivateTreeStage(int stage) => trees[stage].SetActive(true);
    private void DeactivateTree(int stage) => trees[stage].SetActive(false);
}
