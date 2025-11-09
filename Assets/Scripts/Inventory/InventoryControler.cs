using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryControler : MonoBehaviour
{
    [SerializeField]
    PlayerReferencer playerReferencer;
    Inventory Inventory => playerReferencer.Inventory;
    InventoryVisualizer inventoryVisualizer => playerReferencer.inventoryVisualizer;

    bool active = true;

    void Update()
    {
        if (!active)
            return;

        float scroll = Input.mouseScrollDelta.y;
        int selectedSlot = Inventory.SelectedSlot;

        if (Input.GetMouseButtonDown(2))
        {
            if (selectedSlot < 0)
                Inventory.SelectSlot(Inventory.LastSlotSelected);
            else
                Inventory.SelectSlot(-1);
        }

        if (selectedSlot < 0) // not holding any item
            return;

        if (scroll > 0)
            Inventory.SelectSlot(Mathf.Clamp(selectedSlot + 1, 0, Inventory.SlotsCount - 1));
        else if (scroll < 0)
            Inventory.SelectSlot(Mathf.Clamp(selectedSlot - 1, 0, Inventory.SlotsCount - 1));
    }

    public void ActivateInventory()
    {
        inventoryVisualizer.ActivateInventory();
        active = true;
    }

    public void DeactivateInventory()
    {
        inventoryVisualizer.DeactivateInventory();
        active = false;
    }
}
