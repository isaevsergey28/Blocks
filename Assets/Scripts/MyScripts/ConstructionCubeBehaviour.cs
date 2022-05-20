using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class ConstructionCubeBehaviour : MonoBehaviour
{
    public static event Action<GameObject> onReachingTarget;

    public void MoveToVoxel(Vector3 target, MeshRenderer mesh)
    {
        GetComponent<ConstructionCubeColor>().SetColorMaterial(mesh.material.color);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(gameObject.transform.DOMove(target, 0.5f));
        sequence.AppendCallback(() => onReachingTarget?.Invoke(gameObject));
        sequence.AppendCallback(() => mesh.enabled = true);
    }
}
