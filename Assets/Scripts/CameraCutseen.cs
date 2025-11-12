using System.Collections;
using UnityEngine;

public class CameraCutseen : MonoBehaviour
{
    [SerializeField]
    PlayerReferencer playerReferencer;

    [SerializeField]
    private Transform targetTransform;

    [SerializeField]
    private Transform targetLocation;

    [SerializeField]
    float animationTime = 1f;

    [SerializeField]
    private AnimationCurve positionCurve;

    [SerializeField]
    private AnimationCurve rotationCurve;

    [SerializeField]
    float finalDelay = 0f;

    [SerializeField]
    float finalFOV;

    private Vector3 startingPosition;
    private Vector3 startingDirection;

    [SerializeField]
    CameraCutseen nextCutseen;
    private bool finished = false;
    public bool Finished => finished;

    public void StartCutseen()
    {
        startingPosition = Camera.main.transform.position;
        startingDirection = Camera.main.transform.forward;

        Vector3 targetDirection = (targetTransform.position - targetLocation.position).normalized;

        StartCoroutine(
            StartCutseenCoroutine(
                startingPosition,
                Camera.main.transform.forward,
                targetLocation.position,
                targetDirection,
                animationTime,
                finalDelay,
                Camera.main.fieldOfView,
                finalFOV
            )
        );
    }

    private IEnumerator StartCutseenCoroutine(
        Vector3 startingPosition,
        Vector3 startingDirection,
        Vector3 targetPosition,
        Vector3 targetDirection,
        float animationTime,
        float delay,
        float startingFOV,
        float targetFOV
    )
    {
        float realTime = 0f;

        TogglePlayerControls(false);
        playerReferencer.inventoryControler.DeactivateInventory();
        playerReferencer.interactionControl.InteractionPointer.HidePointer();

        while (realTime < 1f)
        {
            realTime += Time.deltaTime / animationTime;

            float animTime = positionCurve.Evaluate(realTime);
            float rotTime = rotationCurve.Evaluate(realTime);
            Camera.main.fieldOfView = Mathf.Lerp(startingFOV, targetFOV, animTime);
            Camera.main.transform.position = Vector3.Lerp(
                startingPosition,
                targetPosition,
                animTime
            );
            Camera.main.transform.forward = Vector3.Lerp(
                startingDirection,
                targetDirection,
                rotTime
            );
            yield return null;
        }
        yield return new WaitForSeconds(delay);

        if (nextCutseen != null)
        {
            nextCutseen.StartCutseen();
            while (!nextCutseen.Finished)
                yield return null;
        }

        Camera.main.transform.position = startingPosition;
        Camera.main.transform.rotation = Quaternion.LookRotation(startingDirection);
        Camera.main.fieldOfView = startingFOV;

        TogglePlayerControls(true);
        finished = true;

        playerReferencer.inventoryControler.ActivateInventory();
        playerReferencer.interactionControl.InteractionPointer.ShowPointer();

        yield break;
    }

    void TogglePlayerControls(bool value)
    {
        playerReferencer.playerMovement.LockMovement(!value);
        playerReferencer.cameraControl.LockCamera(!value);
    }
}
