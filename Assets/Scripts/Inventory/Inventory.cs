using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private Collectable[] inventory = new Collectable[0];
    private int selectedSlot = -1;
    private int slotsCount;
    private int lastSlotSelected = 0;
    public int SelectedSlot => selectedSlot;
    public int SlotsCount => slotsCount;
    public Collectable HoldingItem => GetHoldingItem();
    public int LastSlotSelected => lastSlotSelected;

    public Inventory(int slotsCount)
    {
        inventory = new Collectable[slotsCount];
        this.slotsCount = slotsCount;
    }

    public bool SelectSlot(int slot)
    {
        if (slot >= slotsCount || slot < -1)
            return false;

        lastSlotSelected = SelectedSlot;
        selectedSlot = slot;

        return true;
    }

    private Collectable GetHoldingItem()
    {
        if (inventory.Length <= selectedSlot || selectedSlot < 0)
            return null;

        Debug.Log("Selected slot: " + selectedSlot);
        return inventory[selectedSlot];
    }

    public Collectable[] GetInventory() => inventory;

    public Collectable GetSlot(int slot) => inventory[slot];

    public int GetItemSlot(Collectable item)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == item)
                return i;
        }
        return -1;
    }

    public bool HasItem(Collectable item) => GetItemSlot(item) == -1 ? false : true;

    private int ItensCount()
    {
        int count = 0;
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] != null)
                count++;
        }
        return count;
    }

    public bool AddItem(Collectable item)
    {
        int itensCount = ItensCount();

        if (itensCount >= inventory.Length)
            return false;

        inventory[itensCount] = item;
        return true;
    }

    public bool RemoveItem(Collectable item)
    {
        if (!HasItem(item))
            return false;

        int slot = GetItemSlot(item);
        inventory[slot] = null;

        ReorganizeInventory();

        return true;
    }

    private void ReorganizeInventory()
    {
        List<Collectable> itens = new List<Collectable>();
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] != null)
                itens.Add(inventory[i]);
        }

        Collectable[] newInventory = new Collectable[slotsCount];

        for (int i = 0; i < itens.Count; i++)
            newInventory[i] = itens[i];

        inventory = newInventory;
    }
}
