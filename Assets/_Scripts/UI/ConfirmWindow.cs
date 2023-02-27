using UnityEngine;
using System;
using TMPro;

public class ConfirmWindow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _acceptButtonText;
    [SerializeField] private TextMeshProUGUI _declineButtonText;

    [SerializeField] private TextMeshProUGUI _label;

    private Action _onConfrim;
    private Action _onDecline;

    public void OpenWindow(string text, string acceptButtonText, string declineButtonText, Action onConfirm = null, Action onDecline = null)
    {
        gameObject.SetActive(true);

        _label.text = text;
        _onConfrim = onConfirm;
        _onDecline = onDecline;
        _acceptButtonText.text = acceptButtonText;
        _declineButtonText.text = declineButtonText;
    }

    public void OnConfirmButtonClick()
    {        
        gameObject.SetActive(false);

        _onConfrim?.Invoke();
    }

    public void OnDeclineButtonClick()
    {
        gameObject.SetActive(false);

        _onDecline?.Invoke();
    }
}
