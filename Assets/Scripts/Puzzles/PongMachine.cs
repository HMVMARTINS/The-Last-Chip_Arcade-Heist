using UnityEngine;

public class PongMachine : InteractableObject
{
    [SerializeField]
    PlayerReferencer playerReferencer;

    [SerializeField]
    Transform cameraTargetLocation;

    [SerializeField]
    Transform cameraTargetRotation;

    [SerializeField]
    GameObject pongGameScript;

    private InteractionPointer interactionPointer;

    void Start() => interactionPointer = playerReferencer.interactionControl.InteractionPointer;

    protected override void OnInteract()
    {
        playerReferencer.playerMovement.LockMovement(true);
        playerReferencer.playerMovement.GoToPosition(cameraTargetLocation.position);
        playerReferencer.cameraControl.LockOnObject(cameraTargetRotation);

        interactionPointer.HidePointer();
    }

    protected override void OnDisinteract()
    {
        playerReferencer.cameraControl.UnlockObject();
        playerReferencer.playerMovement.ForceStopAutoPilot();
        playerReferencer.playerMovement.LockMovement(false);

        interactionPointer.ShowPointer();
    }
}
