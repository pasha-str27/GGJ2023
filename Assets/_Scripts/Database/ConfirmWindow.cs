using UnityEngine;
using System;
using TMPro;

public class ConfirmWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;

    private Action _onConfirm;

    private Action _onDecline;

    public void OpenWindow(string text, Action onConfirm = null, Action onDecline = null)
    {
        gameObject.SetActive(true);

        _label.text = text;//"
        _onConfirm = onConfirm;
        _onDecline = onDecline;
    }

    public async void OnConfirmButtonClick()
    {        
        gameObject.SetActive(false);


        _onConfirm?.Invoke();
    }

    public void OnDeclineButtonClick()
    {
        gameObject.SetActive(false);
    }
}
