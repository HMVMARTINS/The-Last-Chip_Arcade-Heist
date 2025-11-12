using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryVisualizer : MonoBehaviour
{
    [SerializeField]
    private Image[] slots = new Image[5]; // 0, 1, 2, 3, 4

    [SerializeField]
    private Image[] itens = new Image[5]; // 0, 1, 2, 3, 4

    [SerializeField]
    private Image selectorUI;

    [SerializeField]
    private Vector2 centerPosition;

    [SerializeField]
    private Vector2 slotSize;

    [SerializeField]
    private float spacingSize;

    [SerializeField]
    float animationSpeed;

    [SerializeField]
    PlayerReferencer playerReferencer;
    private Inventory Inventory => playerReferencer.Inventory;

    private Color defaultColor;

    void Awake() => defaultColor = slots[1].color;

    bool active = true;

    private void UpdateUI()
    {
        if (!active)
            return;

        int selectedSlot = Inventory.SelectedSlot;
        if (selectedSlot < 0)
            selectedSlot = Inventory.SlotsCount / 2; // auto reset

        for (int i = 0; i < slots.Length; i++)
        {
            int slotP = i - selectedSlot;

            float opacity = 1 - ((Mathf.Abs(selectedSlot - i) + 0.001f) / Inventory.SlotsCount);

            Vector2 slotPosition;
            slotPosition.x = centerPosition.x + slotP * slotSize.x + spacingSize * slotP;
            slotPosition.y = centerPosition.y;

            slots[i].transform.localPosition = Vector3.Lerp(
                slots[i].transform.localPosition,
                slotPosition,
                animationSpeed * Time.fixedDeltaTime
            );

            Color color = defaultColor;
            color.a *= opacity;
            slots[i].color = color;

            Collectable item = Inventory.GetSlot(i);

            if (item != null) //has item in slot
            {
                itens[i].transform.localPosition = slots[i].transform.localPosition;

                if (Inventory.SelectedSlot == i)
                    itens[i].color = Color.white;
                else
                    itens[i].color = color;

                itens[i].sprite = item.Sprite;
            }
            else
                itens[i].color = Color.clear;
        }

        Color a = Color.white;
        a.a = Inventory.SelectedSlot < 0 ? 0f : 1f;
        selectorUI.color = a;

        if (Inventory.SelectedSlot >= 0)
            selectorUI.transform.localPosition = slots[Inventory.SelectedSlot]
                .transform
                .localPosition;
    }

    void FixedUpdate() => UpdateUI();

    public void ActivateInventory()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].gameObject.SetActive(true);
            itens[i].gameObject.SetActive(true);
        }

        selectorUI.gameObject.SetActive(true);

        active = true;
    }

    public void DeactivateInventory()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].gameObject.SetActive(false);
            itens[i].gameObject.SetActive(false);
        }

        selectorUI.gameObject.SetActive(false);

        active = false;
    }
}
