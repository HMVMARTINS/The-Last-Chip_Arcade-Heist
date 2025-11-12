using UnityEngine;

public class LockObject : InteractableObject
{
    [SerializeField]
    Transform lockUI;

    protected override void OnInteract()
    {
        lockUI.gameObject.SetActive(true);
    }

    protected override void OnDisinteract()
    {
        lockUI.gameObject.SetActive(false);
    }
}
