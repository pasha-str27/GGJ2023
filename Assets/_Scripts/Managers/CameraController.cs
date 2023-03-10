using UnityEngine;
using DG.Tweening;

public enum CameraState
{
    Tree,
    Grid,
    Leaderboard
}

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
    [SerializeField] private float toBoardMoveSpeed;
    [SerializeField] private float treeScaleSpeed;
    [SerializeField] private Ease gridEase;
    [SerializeField] private Ease treeEase;
    [SerializeField] private Ease treeScaleEase;

    [Header("Targets")]
    [SerializeField] private Transform tree;
    [SerializeField] private Transform grid;
    [SerializeField] private Transform leaderboard;

    [Header("Score")]
    [SerializeField] private int score;
    [SerializeField] private float scaleCoeff;
    [SerializeField] private float startPoint;
    [SerializeField] private float maxDistance = -170f;
    [SerializeField] private CustomEvent onGameOver;

    private Transform _treeCamTform;
    private Transform _backCamTform;
    private Transform _treeTform;
    private Transform _gridTform;
    private Transform _leaderTform;
    private InputController _input;
    private VFXManager _vfx;
    private Player _player;
    private int prevScore;

    private CameraState _state;

    void Awake()
    {
        _input = InputController.Instance;
        _vfx = VFXManager.Instance;
        _player = Player.Instance;
        _state = CameraState.Tree;
        DOTween.Init(autoKillMode, useSafeMode);
    }

    void Start()
    {
        if (tree == null || grid == null || leaderboard == null)
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
        _leaderTform = leaderboard.transform;
    }

    public void ShowTree()
    {
        if (_input.IsInputBlocked() || !_vfx.IsLogoShown() || _state == CameraState.Tree)
            return;

        _input.BlockInput(true);
        _state = CameraState.Tree;

        if (_treeCamTform.position.x != _treeTform.position.x)
        {
            _treeCamTform.DOMoveX(_treeTform.position.x, toTreeMoveSpeed).
            SetEase(treeEase).
            OnComplete(() =>
            {
                Debug.Log("Tree is shown");
                _input.BlockInput(false);
            });

            return;
        }

        _treeCamTform.DOMoveY(_treeTform.position.y, toTreeMoveSpeed).
            SetEase(treeEase).
            OnComplete(() =>
            {
                Debug.Log("Tree is shown");
                ScaleBackground();
            });
    }

    private void ScaleBackground()
    {
        score = _player.GetScore();

        if (score == prevScore)
        {
            _input.BlockInput(false);
            return;
        }

        prevScore = score;

        var camStartPos = _backCamTform.position;
        var distance = Mathf.Clamp(-startPoint - score * scaleCoeff, maxDistance, 0f);
        _backCamTform.DOMove(new Vector3(camStartPos.x, camStartPos.y, distance), treeScaleSpeed).
            SetEase(treeScaleEase).
            OnComplete(() =>
            {
                _input.BlockInput(false);
                Debug.Log("Background is scaled");
            });
    }

    public void GameOver(float delay = 6)
    {
        DOVirtual.DelayedCall(delay, OnGameOver);
    }

    private void OnGameOver() => onGameOver?.Invoke();

    public void ShowGrid()
    {
        if (_input.IsInputBlocked() || !_vfx.IsLogoShown() || _state != CameraState.Tree)
            return;

        _input.BlockInput(true);
        _state = CameraState.Grid;

        _treeCamTform.DOMoveY(_gridTform.position.y, toGridMoveSpeed).
            SetEase(gridEase).
            OnComplete(() =>
            {
                _input.BlockInput(false);
                Debug.Log("Grid is shown");
            });
    }

    public void ShowLeaderboard()
    {
        if (_input.IsInputBlocked() || !_vfx.IsLogoShown() || _state != CameraState.Tree)
            return;

        _input.BlockInput(true);
        _state = CameraState.Leaderboard;

        _treeCamTform.DOMoveX(_leaderTform.position.x, toBoardMoveSpeed).
            SetEase(gridEase).
            OnComplete(() =>
            {
                _input.BlockInput(false);
                Debug.Log("Leaderboard is shown");
            });
    }

    public CameraState GetCameraState() => _state;
}
