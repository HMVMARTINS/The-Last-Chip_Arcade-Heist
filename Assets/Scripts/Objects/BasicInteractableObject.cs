using UnityEngine;

public class BasicInteractableObject : InteractableObject
{
    [SerializeField]
    PlayerReferencer playerReferencer;

    [SerializeField]
    InteractionPointer interactionPointer;

    protected override void OnInteract()
    {
        playerReferencer.playerMovement.LockMovement(true);
        playerReferencer.cameraControl.LockOnObject(transform);

        interactionPointer.HidePointer();
    }

    protected override void OnDisinteract()
    {
        playerReferencer.playerMovement.LockMovement(false);

        interactionPointer.ShowPointer();
    }
}
