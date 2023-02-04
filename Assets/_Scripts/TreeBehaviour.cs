using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TreeBehaviour : MonoBehaviour
{
    [SerializeField] private VisualEffect fallingLeaves;
    [SerializeField] private VisualEffect fireflies;
    public void ActivateEffects()
    {
        fallingLeaves.enabled = true;
        fallingLeaves.Play();
        fireflies.enabled = true;
        fireflies.Play();
    }
}
