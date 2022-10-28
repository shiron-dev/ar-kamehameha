using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaMat : MonoBehaviour
{
    MeshRenderer mesh;

    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        mesh.material.color = mesh.material.color - new Color32(0, 0, 0, 0);
    }

    public void SetAlpha(float alpha)
    {
        Debug.Log(mesh);
        mesh.material.color = mesh.material.color - new Color32(0, 0, 0, (byte)(255f * alpha));
    }
}
