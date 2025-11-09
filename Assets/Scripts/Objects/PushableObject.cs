using UnityEngine;

public class PushableObject : InteractableObject
{
    [SerializeField]
    PlayerReferencer playerReferencer;

    [SerializeField]
    float pushForce = 20f;

    [SerializeField]
    bool onePushOnly;

    protected override void OnInteract()
    {
        float force = pushForce;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        Vector3 forceVector =
            transform.position - playerReferencer.playerMovement.transform.position;
        gameObject
            .GetComponent<Rigidbody>()
            .AddForce(forceVector.normalized * force, ForceMode.Impulse);

        if (onePushOnly)
            SetInteractability(false);
    }

    protected override void OnDisinteract() { }
}
