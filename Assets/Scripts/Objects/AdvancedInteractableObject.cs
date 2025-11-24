using UnityEngine;

public class AdvancedInteractableObject : InteractableObject
{
    [SerializeField]
    PlayerReferencer playerReferencer;

    // [SerializeField]
    // Transform cameraTargetLocation;

    [SerializeField]
    Transform cameraTargetRotation;

    protected override void OnInteract()
    {
        playerReferencer.playerMovement.LockMovement(true);
        // playerReferencer.playerMovement.GoToPosition(cameraTargetLocation.position);
        if (cameraTargetRotation != null)
            playerReferencer.cameraControl.LockOnObject(cameraTargetRotation);
        playerReferencer.DeactivateUI();
    }

    protected override void OnDisinteract()
    {
        if (cameraTargetRotation != null)
            playerReferencer.cameraControl.UnlockObject();
        playerReferencer.playerMovement.LockMovement(false);
        playerReferencer.ActivateUI();
    }
}
