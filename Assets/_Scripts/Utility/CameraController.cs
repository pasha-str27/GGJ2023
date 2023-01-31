using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : SingletonComponent<CameraController>
{
    [Header("Cameras")]
    [SerializeField] private Camera treeCam;
    [SerializeField] private Camera backCam;

    [Header("DOTween")]
    [SerializeField] private bool autoKillMode = true;
    [SerializeField] private bool useSafeMode = true;

    [Header("Move")]
    [SerializeField] private float moveSpeed;

    [Header("Temp Targets")]
    [SerializeField] private Transform tree;
    [SerializeField] private Transform grid;

    private Transform _treeCamTform;
    private Transform _backCamTform;
    private Transform _treeTform;
    private Transform _gridTform;

    void Awake()
    {
        DOTween.Init(autoKillMode, useSafeMode);
    }

    void Start()
    {
        if (tree == null || grid == null)
        {
            Debug.LogError("Wrong objects transform data");
            return;
        }

        if (treeCam == null || backCam == null)
        {
            Debug.LogError("Wrong cameras data");
            return;
        }

        _treeCamTform = treeCam.transform;
        _backCamTform = backCam.transform;
        _treeTform = tree.transform;
        _gridTform = grid.transform;
    }

    public void ShowTree()
    {

    }

    public void ShowGrid()
    {
        _treeCamTform.DOMoveY(grid.transform.position.y, moveSpeed);
    }
}
