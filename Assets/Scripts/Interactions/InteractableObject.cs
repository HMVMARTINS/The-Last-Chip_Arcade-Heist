using System;
using System.Collections;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    [SerializeField]
    bool interactable;
    public bool Interactable => interactable;

    public void SetInteractability(bool interactability) => interactable = interactability;

    [SerializeField]
    GameObject[] objectsToActivate;

    [SerializeField]
    GameObject[] objectsToDeactivate;

    [SerializeField]
    bool continuousInteraction;
    public bool ContinuousInteraction => continuousInteraction;

    public void Interact()
    {
        if (!Interactable)
            return;

        foreach (GameObject obj in objectsToDeactivate)
            obj.SetActive(false);
        foreach (GameObject obj in objectsToActivate)
            obj.SetActive(true);

        OnInteract();
    }

    public void Disinteract()
    {
        foreach (GameObject obj in objectsToDeactivate)
            obj.SetActive(true);
        foreach (GameObject obj in objectsToActivate)
            obj.SetActive(false);

        OnDisinteract();
    }

    protected abstract void OnInteract();

    protected virtual void OnDisinteract() { }

}
