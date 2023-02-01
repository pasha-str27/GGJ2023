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
    private bool isActiveLogo = false;

    void Awake()
    {
        DOTween.Init(autoKillMode, useSafeMode);
    }

    public void OnLogoPress()
    {
        if (!isActiveLogo)
        {
            ShowLogoAnimation();
            return;
        }

        HideLogoAnimation();
    }

    private void ShowLogoAnimation()
    {
        logo.DOFade(1f, showAnimLength).
        SetEase(showEase).
        SetDelay(showAnimDelay).
        OnComplete(delegate
        {
            isActiveLogo = true;
        });

    }

    private void HideLogoAnimation()
    {
        logo.DOFade(0f, hideAnimLength).
        SetEase(hideEase).
        OnComplete(delegate
        {
            CameraController.Instance.ShowGrid();
        });
    }
}
