using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionPointer : MonoBehaviour
{
    [SerializeField]
    Transform pointer;

    [SerializeField]
    private Vector2 pointerSize; // default, interacting
    private float targetSize;

    [SerializeField]
    private float precision = 0.0005f;

    [SerializeField]
    [Range(0, 1)]
    private float animationSpeed;
    private bool interacting = false;

    void Start() => targetSize = pointerSize.x;

    private void OnInteract()
    {
        if (interacting)
            return;

        interacting = true;
        targetSize = pointerSize.y;
    }

    private void OnDisinteract()
    {
        if (!interacting)
            return;

        interacting = false;
        targetSize = pointerSize.x;
    }

    void Update()
    {
        if (Mathf.Abs(pointer.localScale.x - targetSize) > precision)
        {
            pointer.localScale = Vector3.Lerp(
                pointer.localScale,
                new Vector3(targetSize, targetSize, targetSize),
                animationSpeed
            );
        }
    }

    public void Interact(bool interact)
    {
        if (interact)
            OnInteract();
        else
            OnDisinteract();
    }

    public void ShowPointer() => pointer.gameObject.SetActive(true);

    public void HidePointer() => pointer.gameObject.SetActive(false);
}
