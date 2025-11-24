using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionControl : MonoBehaviour
{
    [SerializeField]
    private Transform playerHead;

    public Transform PlayerHead => playerHead;

    [SerializeField]
    private float interactionDistance = 3f;
    public float InteractionDistance => interactionDistance;

    [SerializeField]
    private LayerMask interactionLayer;

    [SerializeField]
    PlayerReferencer playerReferencer;

    [SerializeField]
    private InteractionPointer interactionPointer;
    public InteractionPointer InteractionPointer => interactionPointer;

    private GameObject activeObject;
    private Renderer activeRenderer;
    private Color defaultColor;
    private bool usingEmissionHighlight = false;

    private Vector3 HeadDirection => playerHead.forward;
    private Vector3 ViewVector => HeadDirection * interactionDistance;

    private InteractableObject interactingObject = null;

    public void StopInteraction() => interactingObject = null;

    private MovableObject holdingObject = null;
    private bool IsHoldingObject => holdingObject != null;

    void Update()
    {
        if (interactingObject != null && Input.GetKeyDown(KeyCode.Escape))
        {
            interactingObject.Disinteract();
            interactingObject = null;
        }

        if (!IsHoldingObject)
            HandleInteractionRaycast();
        else
        {
            if (Input.GetMouseButtonDown(0))
                DropObject(holdingObject);
            else
                OnUpdateHoldingObjectColor();
        }
    }

    private void HandleInteractionRaycast()
    {
        if (interactingObject == null)
        {
            RaycastHit hit;
            bool hasHit = Physics.Raycast(
                playerHead.position,
                HeadDirection,
                out hit,
                interactionDistance,
                interactionLayer
            );

            if (hasHit)
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Interactive"))
                {
                    if (activeObject != null)
                        ClearHighlight();
                    return;
                }
                GameObject hitObject = hit.transform.gameObject;
                InteractableObject interactable = hitObject.GetComponent<InteractableObject>();

                if (!interactable && hitObject.transform.parent != null)
                    interactable = hitObject.transform.parent.GetComponent<InteractableObject>();

                if (!interactable || !interactable.Interactable)
                    return;

                if (activeObject != hitObject)
                {
                    Renderer renderer = hitObject.GetComponent<Renderer>();
                    if (renderer != null)
                        Highlight(renderer);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (interactable != null)
                    {
                        Collectable item = interactable.GetComponent<Collectable>();
                        if (item)
                        {
                            bool collected = playerReferencer.Inventory.AddItem(item);
                            Debug.Log("[InteractionControl] Item collected: " + collected);

                            interactable.Interact();
                        }
                        else
                        {
                            ClearHighlight();
                            interactable.Interact();
                            if (interactable.ContinuousInteraction)
                            {
                                interactingObject = interactable;
                            }
                        }
                    }
                }
            }
            else
            {
                ClearHighlight();
            }
        }
    }

    private void Highlight(Renderer renderer)
    {
        ClearHighlight(true);

        activeObject = renderer.gameObject;
        activeRenderer = renderer;

        Material mat = renderer.material;

        if (mat.HasProperty("_Color"))
        {
            defaultColor = mat.color;
            mat.color = defaultColor + Color.white * 0.25f;
            usingEmissionHighlight = false;
        }
        interactionPointer.Interact(true);
    }

    private void ClearHighlight(bool ignorePointer = false)
    {
        if (activeRenderer != null)
        {
            Material mat = activeRenderer.material;

            if (!usingEmissionHighlight && mat.HasProperty("_Color"))
            {
                mat.color = defaultColor;
            }
        }

        activeObject = null;
        activeRenderer = null;
        usingEmissionHighlight = false;

        if (!ignorePointer)
            interactionPointer.Interact(false);
    }

    private void OnDrawGizmos()
    {
        if (playerHead == null)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(playerHead.position, playerHead.position + ViewVector);
    }

    #region Holding System

    public bool HoldObject(MovableObject obj)
    {
        if (IsHoldingObject)
            return false;

        holdingObject = obj;

        playerReferencer.DeactivateUI();
        return true;
    }

    public bool DropObject(MovableObject obj)
    {
        if (!holdingObject || obj != holdingObject || !obj.CanBeDropped)
            return false;

        holdingObject = null;

        obj.Disinteract();
        playerReferencer.ActivateUI();
        return true;
    }

    private void OnUpdateHoldingObjectColor()
    {
        if (!IsHoldingObject || !holdingObject.CollisionStateChanged)
            return;

        Debug.Log("Holding Object state : " + holdingObject.CanBeDropped);

        Material[] materials = holdingObject.GetRenderer().materials;

        for (int i = 0; i < materials.Length; i++)
        {
            Color color = materials[i].color;
            color.r = holdingObject.CanBeDropped ? 0f : Mathf.Clamp01(color.r + 1f);
            materials[i].color = color;
        }
        return;
    }

    #endregion
}
