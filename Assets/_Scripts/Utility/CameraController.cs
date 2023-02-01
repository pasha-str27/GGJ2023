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
    [SerializeField] private float toGridMoveSpeed;
    [SerializeField] private float toTreeMoveSpeed;
    [SerializeField] private float treeScaleSpeed;

    [SerializeField] private Ease gridEase;
    [SerializeField] private Ease treeEase;
    [SerializeField] private Ease treeScaleEase;


    [Header("Temp Targets")]
    [SerializeField] private Transform tree;
    [SerializeField] private Transform grid;

    [Header("Score")]
    [SerializeField] private int score;

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

    [ContextMenu("ShowTree")]
    public void ShowTree()
    {
        _treeCamTform.DOMoveY(_treeTform.position.y, toTreeMoveSpeed).
            SetEase(treeEase).
            OnComplete(ScaleBackground);
        Debug.Log("Showed tree");
    }

    private void ScaleBackground()
    {
        var scale = score * 0.01f;
        if (scale > 1f)
        {
            var camStartPos = _backCamTform.position;
            _backCamTform.DOMove(new Vector3(camStartPos.x, camStartPos.y, -10 - 10 * scale), treeScaleSpeed).
                SetEase(treeScaleEase);
        }
    }

    [ContextMenu("ShowGrid")]
    public void ShowGrid()
    {
        _treeCamTform.DOMoveY(_gridTform.position.y, toGridMoveSpeed).
            SetEase(gridEase);
        Debug.Log("Showed grid");
    }
}
