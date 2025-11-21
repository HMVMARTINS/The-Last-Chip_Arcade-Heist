using UnityEngine;

public class LockObject : InteractableObject
{
    [SerializeField]
    PlayerReferencer playerReferencer;

    [SerializeField]
    InteractionPointer interactionPointer;

    protected override void OnInteract()
    {
        playerReferencer.playerMovement.LockMovement(true);

        interactionPointer.HidePointer();
    }

    protected override void OnDisinteract()
    {
        playerReferencer.playerMovement.LockMovement(false);

        interactionPointer.ShowPointer();
    }
}
