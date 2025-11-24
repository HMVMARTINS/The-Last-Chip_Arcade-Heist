using System.Collections;
using UnityEngine;

public class MovableObject : InteractableObject
{
    [SerializeField]
    PlayerReferencer playerReferencer;

    [SerializeField]
    Rigidbody rb;

    [SerializeField]
    Collider coll;

    [SerializeField]
    Renderer render;

    public Rigidbody GetRigidbody() => rb;

    public Renderer GetRenderer() => render;

    private bool canBeDropped = true;
    private bool canBeDroppedLastFrame = true;
    public bool CanBeDropped => canBeDropped;
    public bool CollisionStateChanged => canBeDropped != canBeDroppedLastFrame;

    private Color[] defaultColors;

    void Start()
    {
        Material[] materials = render.materials;
        defaultColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
            defaultColors[i] = materials[i].color;
    }

    protected override void OnInteract()
    {
        if (playerReferencer.interactionControl.HoldObject(this))
        {
            // object holded
            coll.isTrigger = true;

            Transform playerHead = playerReferencer.interactionControl.PlayerHead;

            transform.parent = playerHead;
            transform.localPosition =
                Vector3.forward * playerReferencer.interactionControl.InteractionDistance;

            rb.isKinematic = true;
        }
        else
        {
            // already holding some object
        }
    }

    protected override void OnDisinteract()
    {
        // object dropped
        coll.isTrigger = false;
        ResetAllMaterialsColor();
        playerReferencer.interactionControl.StopInteraction();
        transform.parent = playerReferencer.mapTransform;
        transform.localPosition = Vector3.zero;

        Transform playerHead = playerReferencer.interactionControl.PlayerHead;

        transform.position = playerHead.TransformPoint(
            Vector3.forward * playerReferencer.interactionControl.InteractionDistance
        );

        rb.isKinematic = false;
    }

    private void OnTriggerStay(Collider other)
    {
        canBeDroppedLastFrame = canBeDropped;
        canBeDropped = false;
    }

    private void OnTriggerExit(Collider other)
    {
        canBeDroppedLastFrame = canBeDropped;
        canBeDropped = true;
        StartCoroutine(UpdateCollisionState(true));
    }

    IEnumerator UpdateCollisionState(bool state)
    {
        yield return null; // wait 1 frame
        canBeDroppedLastFrame = state ? canBeDropped : !canBeDropped;
    }

    private void ResetAllMaterialsColor()
    {
        Material[] materials = render.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].color = defaultColors[i];
            Debug.Log("Color reset: " + defaultColors[i]);
        }
    }
}
