using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Door : InteractableObject
{
    [SerializeField]
    PlayerReferencer playerReferencer;

    [SerializeField]
    Collectable key;

    [SerializeField]
    Transform hingeObject;

    [SerializeField]
    float rotationRange;

    [SerializeField]
    AnimationCurve animationCurve;

    [SerializeField]
    float animationDuration;

    public bool HasKey() => key != null;

    bool doorOpen = false;
    Coroutine coroutine;

    protected override void OnInteract()
    {
        if (!doorOpen)
        {
            if (HasKey())
            {
                Inventory inventory = playerReferencer.Inventory;
                Collectable holdingItem = inventory.HoldingItem;

                if (Equals(holdingItem, key))
                    TriggerDoor();
            }
            else
                TriggerDoor();
        }
        else
        {
            TriggerDoor();
        }
    }

    protected override void OnDisinteract() { }

    private void TriggerDoor()
    {
        coroutine = StartCoroutine(DoorAnimation(!doorOpen, animationDuration));
    }

    IEnumerator DoorAnimation(bool open, float duration)
    {
        doorOpen = !doorOpen;
        SetInteractability(false);

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float p = time / duration;
            float r = open ? 0f : 1f;

            float v = animationCurve.Evaluate(Mathf.Abs(p - r));

            Vector3 targetRotation = new Vector3(0, rotationRange * v, 0f);
            hingeObject.transform.localRotation = Quaternion.Euler(targetRotation);
            yield return null;
        }
        SetInteractability(true);
    }
}
