using UnityEngine;

public class PlayerCameraControl : MonoBehaviour
{
    [SerializeField]
    PlayerMovement playerMovement;

    [SerializeField, Range(0.1f, 50f)]
    private float sensitivity = 2f;

    [SerializeField]
    private Vector2 cameraLimits = new Vector2(-80f, 55f);

    [Range(0f, 1f)]
    public float cameraSmoothEffect = 0.1f;

    [Range(0f, 1f)]
    public float cameraShakeSmoothEffect = 0.1f;

    [SerializeField]
    Vector2 cameraShakeAmmount;

    [SerializeField]
    Vector2 cameraMoveAmmount;

    [SerializeField]
    private Transform playerBody;

    private Vector3 currentRotation;
    private Vector3 targetRotation;
    private Vector3 defaultPosition;

    [SerializeField]
    float defaultCameraMovementSpeed;

    private float defaultCameraMovementValue;

    private bool controllable = true;
    private Transform objectLocked;

    public bool isLockedOnObject => objectLocked != null;

    public float Direction => GetAngle(currentRotation.y);

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        defaultPosition = transform.localPosition;
    }

    void Update()
    {
        if (!controllable && !isLockedOnObject)
            return;

        if (isLockedOnObject)
        {
            currentRotation.y = GetAngle(currentRotation.y);
            Vector3 direction = objectLocked.position - transform.position;
            targetRotation = DirectionToEuler(direction.normalized);

            float smooth = 1f - Mathf.Exp(-cameraSmoothEffect * 15f * Time.deltaTime);

            currentRotation.x = Mathf.LerpAngle(currentRotation.x, targetRotation.x, smooth);
            currentRotation.y = Mathf.LerpAngle(currentRotation.y, targetRotation.y, smooth);
            currentRotation.z = Mathf.LerpAngle(currentRotation.z, targetRotation.z, smooth);
        }
        else
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

            targetRotation.x -= mouseY;
            targetRotation.y += mouseX;
            targetRotation.z = -mouseX / 2 * 5f * Time.deltaTime;

            targetRotation.x = Mathf.Clamp(targetRotation.x, cameraLimits.x, cameraLimits.y);

            currentRotation = Vector3.Lerp(
                currentRotation,
                targetRotation,
                1f - Mathf.Exp(-cameraSmoothEffect)
            );
        }
    }

    void LateUpdate()
    {
        if (!controllable && !isLockedOnObject)
            return;

        CameraControl();
    }

    private void CameraControl()
    {
        float stepAmmount = playerMovement.stepMovementControl.StepAmmount;
        float verticalAnim = Mathf.Sin(stepAmmount * Mathf.PI * 2);

        int sign = playerMovement.stepMovementControl.LeftFoot ? -1 : 1;
        float horizontalAnim = sign * Mathf.Sin(stepAmmount / 2 * Mathf.PI * 2);

        defaultCameraMovementValue += Time.deltaTime * defaultCameraMovementSpeed;
        Vector2 defaultCameraMovement = new Vector3(
            Mathf.Cos(defaultCameraMovementValue * 1.86834f),
            Mathf.Sin(defaultCameraMovementValue * 3.1864f)
        );

        playerBody.rotation = Quaternion.Euler(
            0f,
            GetAngle(currentRotation.y + defaultCameraMovement.x),
            0f
        );
        transform.localRotation = Quaternion.Euler(
            currentRotation.x - verticalAnim * cameraShakeAmmount.y + defaultCameraMovement.y,
            0f,
            currentRotation.z
        );

        Vector3 cameraOffset = new Vector3(
            horizontalAnim
                * cameraMoveAmmount.x
                * (
                    playerMovement.stepMovementControl.LastDistanceMoved
                    / Time.deltaTime
                    / playerMovement.SprintSpeed
                ),
            verticalAnim
                * cameraMoveAmmount.y
                * (
                    playerMovement.stepMovementControl.LastDistanceMoved
                    / Time.deltaTime
                    / playerMovement.SprintSpeed
                ),
            0
        );

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            defaultPosition + cameraOffset,
            cameraShakeSmoothEffect
        );
    }

    private float GetAngle(float degrees)
    {
        float angle = degrees % 360f;
        if (angle < 0f)
            angle += 360f;

        return angle;
    }

    private Vector3 DirectionToEuler(Vector3 dir)
    {
        dir.Normalize();
        float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float pitch = -Mathf.Asin(dir.y) * Mathf.Rad2Deg;
        return new Vector3(pitch, GetAngle(yaw), 0f);
    }

    public void LockCamera(bool value) => controllable = !value;

    public void LockOnObject(Transform obj)
    {
        controllable = false;
        objectLocked = obj;
    }

    public void UnlockObject()
    {
        controllable = true;
        objectLocked = null;
    }
}
