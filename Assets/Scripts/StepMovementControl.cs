using UnityEngine;

public class StepMovementControl
{
    private bool leftFoot;
    public bool LeftFoot => leftFoot;
    private float animValue;
    public float AnimValue => animValue;
    private float stepDistance;
    public float StepDistance => stepDistance;
    public float StepAmmount => Mathf.Clamp(animValue / stepDistance, 0f, 1f);
    private float momentumDecay;
    private float lastDistanceMoved;
    public float LastDistanceMoved;

    public StepMovementControl(float stepDistance, float momentumDecay = 5f)
    {
        this.stepDistance = stepDistance;
        this.momentumDecay = momentumDecay;
    }

    public void Move(float distanceMoved, float deltaTime, bool sprinting)
    {
        animValue += distanceMoved;
        if (sprinting)
            animValue -= distanceMoved * 0.3f;

        if (animValue >= stepDistance)
        {
            animValue = animValue - stepDistance;
            leftFoot = !leftFoot;
        }
        if (distanceMoved / deltaTime <= 0.00000001f)
        {
            int sign = animValue > stepDistance / 2 ? 1 : -1;

            animValue = Mathf.Clamp(
                animValue + deltaTime * momentumDecay * sign,
                0.0001f,
                stepDistance
            );
        }
        LastDistanceMoved = distanceMoved;
    }
}
