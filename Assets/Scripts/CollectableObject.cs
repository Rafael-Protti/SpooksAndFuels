using UnityEngine;

public class CollectableObject : MonoBehaviour
{
    public int itemCount = 1;

    [SerializeField] private PlayerItems.ItemType itemType = PlayerItems.ItemType.Wood;
    public PlayerItems.ItemType ItemType => itemType;

    public void Drop(Transform dropLocation)
    {
        transform.position = dropLocation.position;
        transform.rotation = dropLocation.rotation;
        transform.SetParent(null);
    }
}
