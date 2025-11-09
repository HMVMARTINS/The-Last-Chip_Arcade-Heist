using UnityEngine;

public class Collectable : InteractableObject
{
    [SerializeField]
    string name;

    [SerializeField]
    Sprite sprite;

    [SerializeField]
    bool canBeDeleted = true;

    public Sprite Sprite => sprite;

    protected override void OnInteract()
    {
        gameObject.SetActive(false);
        // Destroy(gameObject);
    }

    protected override void OnDisinteract() { }
}
