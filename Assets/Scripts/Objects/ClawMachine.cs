using UnityEngine;

public class ClawMachine : InteractableObject
{
    [SerializeField]
    PlayerReferencer playerReferencer;

    [SerializeField]
    Transform cameraTargetLocation;

    [SerializeField]
    Transform cameraTargetRotation;

    protected override void OnInteract()
    {
        playerReferencer.playerMovement.LockMovement(true);
        playerReferencer.playerMovement.GoToPosition(cameraTargetLocation.position);
        playerReferencer.cameraControl.LockOnObject(cameraTargetRotation);
    }

    protected override void OnDisinteract()
    {
        playerReferencer.cameraControl.UnlockObject();
        playerReferencer.playerMovement.LockMovement(false);
    }
}
