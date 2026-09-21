using UnityEngine;

public class CollectableObject : MonoBehaviour
{
    public enum ItemType
    {
        Coal,
        Ectoplasm,
        SuperEctoplasm
    }

    [SerializeField] private ItemType itemType = ItemType.Coal;

    private LocomotiveController locomotive;

    public void Start()
    {
        locomotive = GameObject.Find("Locomotive").GetComponent<LocomotiveController>();
    }

    public void AddCoal()
    {
        int amount = 0;
        if(itemType == ItemType.Coal){
            amount = 10;
        }

        if(itemType == ItemType.Ectoplasm){
            amount = 2;
        }

        if(itemType == ItemType.SuperEctoplasm){
            amount = 4;
            AddSpeedBoost();
        }

        locomotive.Refuel(amount);
    }

    public void AddSpeedBoost()
    {
        locomotive.ApplySpeedBoost(1.50f, 30f);
    }

    public void Drop(Transform dropLocation)
    {
        GetComponent<Rigidbody>().isKinematic = false;
        transform.position = dropLocation.position;
        transform.rotation = dropLocation.rotation;
        transform.SetParent(null);

        GetComponent<InteractableObject>().enabled = true;
    }
}
