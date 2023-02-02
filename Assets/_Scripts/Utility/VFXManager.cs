using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class VFXManager : SingletonComponent<VFXManager>
{

    [Header("DOTween")]
    [SerializeField] private bool autoKillMode = true;
    [SerializeField] private bool useSafeMode = false;

    [Header("Logo Animation")]
    [SerializeField] private TextMeshProUGUI logo;
    [SerializeField] private Ease showEase;
    [SerializeField] private Ease hideEase;
    [SerializeField] private float showAnimDelay;
    [SerializeField] private float showAnimLength;
    [SerializeField] private float hideAnimLength;
    [SerializeField] private CustomEvent showLogo;
    [SerializeField] private CustomEvent hideLogo;
    [SerializeField] private CustomEvent onHideLogoAnim;
    private bool isLogoShown;
    private bool isLogoActive;
    private InputController _input;

    void Awake()
    {
        _input = InputController.Instance;
        DOTween.Init(autoKillMode, useSafeMode);
    }

    public void OnLogoPress()
    {
        if (!isLogoActive)
        {
            showLogo?.Invoke();
            return;
        }

        hideLogo?.Invoke();
    }

    public void ShowLogoAnimation()
    {
        if (_input.IsInputBlocked())
            return;

        _input.BlockInput(true);

        logo.DOFade(1f, showAnimLength).
        SetEase(showEase).
        SetDelay(showAnimDelay).
        OnComplete(() =>
        {
            isLogoActive = true;
            _input.BlockInput(false);
        });

    }

    public void HideLogoAnimation()
    {
        if (_input.IsInputBlocked())
            return;

        _input.BlockInput(true);

        logo.DOFade(0f, hideAnimLength).
        SetEase(hideEase).
        OnComplete(() =>
        {
            isLogoShown = true;
            isLogoActive = false;
            _input.BlockInput(false);
            onHideLogoAnim?.Invoke();
        });
    }

    public bool IsLogoShown() => isLogoShown;
}
