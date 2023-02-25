using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavesBehaviour : MonoBehaviour
{
    private List<Material> _leaves = new List<Material>();

    void Awake()
    {
        for (int i = 0; i < transform.childCount; ++i)
            _leaves.Add(transform.GetChild(i).GetComponent<Renderer>().material);
    }

    public void SetShaderToMove()
    {
        foreach (var child in _leaves)
            child.SetInt("_Move", 1);
    }
}
