using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class InputController : SingletonComponent<InputController>, IDragHandler
{
    [Range(0.01f, 0.9f)][SerializeField] private float inputThreshold;
    private CameraController _cam;
    private VFXManager _vfx;
    private bool isInputBlocked;
    private enum DraggedDirection { Up, Down, Right, Left }
    private int SCREEN_HEIGHT = Screen.height;

    void Awake()
    {
        _cam = CameraController.Instance;
        _vfx = VFXManager.Instance;
        inputThreshold *= SCREEN_HEIGHT;
    }

    private DraggedDirection GetDragDirection(Vector3 dragVector)
    {
        float positiveX = Mathf.Abs(dragVector.x);
        float positiveY = Mathf.Abs(dragVector.y);

        DraggedDirection draggedDir;

        if (positiveX > positiveY)
            draggedDir = (dragVector.x > 0) ? DraggedDirection.Right : DraggedDirection.Left;
        else
            draggedDir = (dragVector.y > 0) ? DraggedDirection.Up : DraggedDirection.Down;

        Debug.Log(draggedDir);

        return draggedDir;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (isInputBlocked || !_vfx.IsLogoShown())
        {
            Debug.LogWarning("Input is blocked");
            return;
        }

        Vector3 dragVector = (eventData.position - eventData.pressPosition);
        if (dragVector.magnitude > inputThreshold)
        {
            var dragDir = GetDragDirection(dragVector.normalized);

            if (dragDir == DraggedDirection.Down && _cam.GetCameraState() == CameraState.Grid)
                _cam.ShowTree();

            if (dragDir == DraggedDirection.Up)
                _cam.ShowGrid();

            if (dragDir == DraggedDirection.Left)
                _cam.ShowLeaderboard();

            if (dragDir == DraggedDirection.Right && _cam.GetCameraState() == CameraState.Leaderboard)
                _cam.ShowTree();
        }
    }

    public void BlockInput(bool v) => isInputBlocked = v;

    public bool IsInputBlocked() => isInputBlocked;
}
