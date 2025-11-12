using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float playerSpeed;

    [SerializeField]
    private float playerMaxSpeed;

    [SerializeField]
    private float playerSprintSpeed;

    [SerializeField]
    private float playerCrawlSpeed;
    public float SprintSpeed => playerSprintSpeed;

    [SerializeField]
    float playerVelocityResponse;

    [SerializeField]
    Rigidbody rb;

    [SerializeField]
    PlayerCameraControl cameraControl;

    [SerializeField]
    float stepSize;
    Vector3 positionHistory;
    public StepMovementControl stepMovementControl;
    public bool isSprinting => Input.GetKey(KeyCode.LeftShift);

    [SerializeField]
    CapsuleCollider playerCollider;

    [SerializeField]
    float crawlHeight = 0.5f;
    private float defaultHeight;

    private float playerVelocity;
    bool autoCrawl;
    bool controllable = true;
    Vector3 autoPilotTarget = Vector3.zero;
    bool AutoPilotON => autoPilotTarget != Vector3.zero;

    private bool crawlingHistory;

    private NavMeshAgent agent;

    private Coroutine autonomousMovementCoroutine;

    private Vector2 GetInputs()
    {
        int Up = Input.GetKey(KeyCode.W) ? 1 : 0;
        int Down = Input.GetKey(KeyCode.S) ? -1 : 0;
        int Right = Input.GetKey(KeyCode.A) ? 1 : 0;
        int Left = Input.GetKey(KeyCode.D) ? -1 : 0;

        return new Vector2(Up + Down, Right + Left);
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        autoCrawl = true;
        defaultHeight = playerCollider.height;
        stepMovementControl = new StepMovementControl(stepSize);
    }

    void FixedUpdate()
    {
        if (!controllable)
        {
            UpdateStepControl();
            return;
        }

        Vector2 inputs = GetInputs();

        Quaternion camRot = Quaternion.Euler(0f, cameraControl.Direction, 0f);
        Vector3 inputDir = new Vector3(-inputs.y, 0f, inputs.x);

        Vector3 targetDirection =
            (inputDir.sqrMagnitude > 0.0001f) ? (camRot * inputDir.normalized) : Vector3.zero;

        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        bool crawling = Input.GetKey(KeyCode.LeftControl) || autoCrawl;

        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float speed = playerMaxSpeed;
        if (sprinting)
            speed = playerSprintSpeed;
        else if (crawling)
            speed = playerCrawlSpeed;

        Vector3 desiredVelocity = targetDirection * speed;

        Vector3 newVelocityXZ = Vector3.MoveTowards(
            currentVelocity,
            desiredVelocity,
            playerVelocityResponse * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector3(newVelocityXZ.x, rb.linearVelocity.y, newVelocityXZ.z);
        CrawlPlayer(crawling);
        UpdateStepControl();
    }

    void CrawlPlayer(bool crawling)
    {
        if (crawling == crawlingHistory)
            return;

        if (crawling)
            playerCollider.height = crawlHeight;
        else
            playerCollider.height = defaultHeight;

        crawlingHistory = crawling;
    }

    void UpdateStepControl()
    {
        Vector3 pos1 = new Vector3(positionHistory.x, 0, positionHistory.z);
        Vector3 pos2 = new Vector3(transform.position.x, 0, transform.position.z);

        float distance = Vector3.Distance(pos1, pos2);
        stepMovementControl.Move(distance, Time.deltaTime, isSprinting);
        positionHistory = transform.position;
    }

    void OnDrawGizmos()
    {
        Vector2 inputs = GetInputs();
        Quaternion camRot = Quaternion.Euler(0f, cameraControl.Direction, 0f);
        Vector3 inputDir = new Vector3(inputs.x, 0f, inputs.y);

        Vector3 targetDirection =
            (inputDir.sqrMagnitude > 0.0001f) ? (camRot * inputDir.normalized) : Vector3.zero;

        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 desiredVelocity = targetDirection * playerMaxSpeed;

        Vector3 newVelocityXZ = Vector3.MoveTowards(
            currentVelocity,
            desiredVelocity,
            playerVelocityResponse * Time.fixedDeltaTime
        );
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + desiredVelocity * 5f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, newVelocityXZ);
    }

    public void AutoCrawl(bool value) => autoCrawl = value;

    public void LockMovement(bool value)
    {
        controllable = !value;
        rb.isKinematic = value;
    }

    public void GoToPosition(Vector3 targetPosition) =>
        autonomousMovementCoroutine = StartCoroutine(AutoPilot(targetPosition));

    private IEnumerator AutoPilot(Vector3 targetPosition)
    {
        rb.linearVelocity = Vector2.zero;
        bool movLocked = controllable;
        autoPilotTarget = targetPosition;
        controllable = false;

        agent.enabled = true;

        agent.SetDestination(targetPosition);

        bool finished = false;

        while (!finished)
        {
            Vector3 difference = targetPosition - transform.position;
            float distance = Mathf.Abs(difference.x) + Mathf.Abs(difference.z);

            if (distance < 0.01f)
                finished = true;

            yield return null;
        }

        // Debug.Log("[AUTOPILOT] Arrived at destination.");

        agent.enabled = false;
        autoPilotTarget = Vector3.zero;
        LockMovement(!movLocked);
        yield break;
    }

    public void ForceStopAutoPilot()
    {
        StopCoroutine(autonomousMovementCoroutine);
        autoPilotTarget = Vector3.zero;
        agent.enabled = false;
    }
}
