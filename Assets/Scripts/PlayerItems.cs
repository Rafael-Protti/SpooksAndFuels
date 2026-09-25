using UnityEngine;
using static PlayerItems;

public class PlayerItems : MonoBehaviour
{
    public enum ItemType
    {
        Coal,
        Ectoplasm,
        SuperEctoplasm
    }

    public int coal = 0;
    public int ectoplasm = 0;
    public int superectoplasm = 0;

    public void AddItem(ItemType itemType ,int value)
    {
        if (itemType == ItemType.Coal)
        {
            coal += value;
        }

        if (itemType == ItemType.Ectoplasm)
        {
            ectoplasm += value;
        }

        if (itemType == ItemType.SuperEctoplasm)
        {
            superectoplasm += value;
        }

    }

}
