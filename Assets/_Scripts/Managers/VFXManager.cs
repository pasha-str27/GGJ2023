using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using UnityEngine.VFX;

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
    [SerializeField] private CustomEvent onShowLogoAnim;
    [SerializeField] private CustomEvent onHideLogoAnim;

    [Header("Object Animations")]
    [SerializeField] private VisualEffect sparkles;
    [SerializeField] private VisualEffect dust;

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
            onShowLogoAnim?.Invoke();
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

    public void FadeIn(TextMeshProUGUI text) => text.DOFade(0f, 1f);

    public void FadeOut(TextMeshProUGUI text) => text.DOFade(1f, 1f);

    public bool IsLogoShown() => isLogoShown;

    public void PlaySparklesEffect(Vector3 center, Vector3 size)
    {
        if (sparkles == null)
            return;

        sparkles.SetVector3("Center", center);
        sparkles.SetVector3("BoxSize", size);
        sparkles.Play();
    }

    public void PlayDustEffect(Vector3 center)
    {
        if (dust == null)
            return;

        dust.SetVector3("Position", center);
        dust.Play();
    }
}
