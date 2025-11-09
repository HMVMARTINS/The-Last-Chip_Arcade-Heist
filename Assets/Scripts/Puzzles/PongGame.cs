using System.Collections;
using UnityEngine;

public class PongGame : InteractableGame
{
    [SerializeField]
    Transform background;

    [SerializeField]
    Transform ball;

    [SerializeField]
    Transform playerPiece;

    [SerializeField]
    Transform enemyPiece;

    Vector2 screenLimits;
    Vector2 pieceLimits;
    Vector2 ballLimits;

    Vector2 ballVelocity;

    [SerializeField]
    float ballSpeed = 0.5f;
    float defaultBallSpeed;

    [SerializeField]
    float ballAcceleration = 0.02f;

    [SerializeField]
    [Range(0, 1)]
    float ballControl = 0.5f;

    [SerializeField]
    float pieceSpeed = 0.1f;

    [SerializeField]
    [Range(0, 1)]
    float ballMomentum = 0.2f;

    [SerializeField]
    [Range(0, 0.4f)]
    float enemyHumaniseDistance;

    [SerializeField]
    Vector2 enemyHumaniseCooldown;
    float enemyTargetOffset;
    Coroutine enemyControlCoroutine;
    private Vector2 ballHistoryPosition;

    void OnEnable()
    {
        defaultBallSpeed = ballSpeed;
        base.OnEnable();
        StartGame();
    }

    void OnDisable()
    {
        StopGame();
        base.OnDisable();
    }

    private void StartGame() => StartCoroutine(InitializeGame());

    private void StopGame() => InitializeObjects();

    void Update()
    {
        // BallAcceleration();
        PlayerControl();
        EnemyControl();
        BallControl();
        CheckForGameOver();
        ballSpeed += ballAcceleration * Time.deltaTime;
    }

    private void PlayerControl()
    {
        int upward = Input.GetKey(KeyCode.W) ? 1 : 0;
        int downward = Input.GetKey(KeyCode.S) ? -1 : 0;

        float upLimit = screenLimits.y - pieceLimits.y;
        float downLimit = -screenLimits.y + pieceLimits.y;

        playerPiece.localPosition = new Vector3(
            playerPiece.localPosition.x,
            Mathf.Clamp(
                playerPiece.localPosition.y + (upward + downward) * Time.deltaTime * pieceSpeed,
                downLimit,
                upLimit
            ),
            playerPiece.localPosition.z
        );
    }

    private void EnemyControl()
    {
        float timeLeft = (screenLimits.x - 0.05f - ball.localPosition.x) / ballVelocity.x;

        float target;
        if (ballVelocity.x <= 0)
        { // only visual
            target = ball.localPosition.y + enemyTargetOffset;
        }
        else
        { // knows position
            target =
                ComputeFinalPosition(ball.localPosition, screenLimits, ballVelocity, timeLeft)
                + enemyTargetOffset;
        }
        float diff = Mathf.Clamp(
            (target - enemyPiece.localPosition.y) * 10,
            -pieceSpeed,
            pieceSpeed
        );

        float upLimit = screenLimits.y - pieceLimits.y;
        float downLimit = -screenLimits.y + pieceLimits.y;

        enemyPiece.localPosition = new Vector3(
            enemyPiece.localPosition.x,
            Mathf.Clamp(enemyPiece.localPosition.y + diff * Time.deltaTime, downLimit, upLimit),
            -0.001f
        );
    }

    private float ComputeFinalPosition(Vector2 pos, Vector2 limits, Vector2 velocity, float time)
    {
        float y_min = -limits.y + ballLimits.y;
        float y_max = limits.y - ballLimits.y;
        float H = y_max - y_min;

        float y = pos.y + velocity.y * time;
        float mod = (y - y_min) % (2 * H);
        if (mod < 0)
            mod += 2 * H;

        float finalY = y_min + H - Mathf.Abs(mod - H);

        return finalY;
    }

    private void BallControl()
    {
        float deltaY = ballVelocity.y * Time.deltaTime;
        float deltaX = ballVelocity.x * Time.deltaTime;
        float upLimit = screenLimits.y - ballLimits.y;
        float downLimit = -screenLimits.y + ballLimits.y;

        ball.localPosition += new Vector3(deltaX, Mathf.Clamp(deltaY, downLimit, upLimit), 0f);

        if (ball.localPosition.y + deltaY > upLimit || ball.localPosition.y + deltaY < downLimit)
            ShootBall(new Vector2(ballVelocity.x, -ballVelocity.y));

        if (IsPositionInPlayersArea(ball.localPosition, 0.05f)) // player area
        {
            // pieces collisions
            float playerDistance = Vector2.Distance(playerPiece.localPosition, ball.localPosition);
            float enemyDistance = Vector2.Distance(enemyPiece.localPosition, ball.localPosition);

            if (playerDistance < pieceLimits.y)
            {
                Vector2 dir = ball.localPosition - playerPiece.localPosition;
                dir.y *= ballControl;
                dir.x = Mathf.Abs(dir.x);
                ShootBall(dir);
            }
            else if (enemyDistance < pieceLimits.y)
            {
                Vector2 dir = ball.localPosition - enemyPiece.localPosition;
                dir.y *= ballControl;
                dir.x = -Mathf.Abs(dir.x);
                ShootBall(dir);
            }
        }
    }

    private bool IsPositionInPlayersArea(Vector2 pos, float radius)
    {
        if (
            pos.x - ballLimits.x < -screenLimits.x + radius
            || pos.x + ballLimits.x > screenLimits.x - radius
        )
            return true;
        else
            return false;
    }

    void CheckForGameOver()
    {
        if (
            ball.localPosition.x - ballLimits.x < -screenLimits.x
            || ball.localPosition.x + ballLimits.x > screenLimits.x
        )
            StartGame();
    }

    private void ShootBall(Vector2 direction)
    {
        ballVelocity = direction.normalized * ballSpeed;
        Debug.Log("Shoot: " + ballVelocity);
    }

    private IEnumerator InitializeGame(float initialDelay = 1f)
    {
        StopGame();
        yield return new WaitForSeconds(initialDelay);

        StartAI();

        float x = Random.Range(-1, -0.5f);
        float y = Random.Range(-0.5f, 0.5f);
        ShootBall(new Vector2(x, y));

        yield break;
    }

    private void InitializeObjects()
    {
        screenLimits = Vector2.one / 2;
        pieceLimits = new Vector2(0.05f, 0.1f);
        ballLimits = ball.localScale;

        ballSpeed = defaultBallSpeed;

        playerPiece.localPosition = new Vector3(-screenLimits.x + 0.05f, 0, -0.001f);
        enemyPiece.localPosition = new Vector3(screenLimits.x - 0.05f, 0, -0.001f);

        ball.localPosition = new Vector3(0, 0, -0.001f);

        ballVelocity = Vector2.zero;
    }

    private void StartAI() => enemyControlCoroutine = StartCoroutine(EnemyHumaniseControl());

    private void StopAI() => StopCoroutine(enemyControlCoroutine);

    private IEnumerator EnemyHumaniseControl()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Random.Range(enemyHumaniseCooldown.x, enemyHumaniseCooldown.y)
            );
            enemyTargetOffset = Random.Range(-enemyHumaniseDistance, enemyHumaniseDistance);
            yield return null;
        }
    }
}
