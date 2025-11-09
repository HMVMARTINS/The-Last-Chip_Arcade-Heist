using System.Collections.Generic;
using UnityEngine;

public class InteractableGame : MonoBehaviour
{
    [SerializeField]
    PlayerReferencer playerReferencer;

    [SerializeField]
    List<GameObject> gameObjects = new List<GameObject>();

    public void OnEnable()
    {
        foreach (GameObject obj in gameObjects)
            obj.SetActive(true);

        playerReferencer.inventoryControler.DeactivateInventory();
    }

    public void OnDisable()
    {
        foreach (GameObject obj in gameObjects)
            obj.SetActive(false);

        playerReferencer.inventoryControler.ActivateInventory();
    }
}
