using UnityEngine;

public class PlayerReferencer : MonoBehaviour
{
    [SerializeField]
    public PlayerMovement playerMovement;

    [SerializeField]
    public PlayerCameraControl cameraControl;

    [SerializeField]
    public PlayerInteractionControl interactionControl;

    [SerializeField]
    public InventoryVisualizer inventoryVisualizer;

    [SerializeField]
    public InventoryControler inventoryControler;

    [SerializeField]
    public Transform mapTransform;

    public Inventory Inventory => inventory;
    private Inventory inventory;

    void Awake() => inventory = new Inventory(5);

    public void ActivateUI()
    {
        inventoryControler.ActivateInventory();
        interactionControl.InteractionPointer.ShowPointer();
    }

    public void DeactivateUI()
    {
        inventoryControler.DeactivateInventory();
        interactionControl.InteractionPointer.HidePointer();
    }
}
