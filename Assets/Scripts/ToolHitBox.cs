using UnityEngine;

public class ToolHitBox : MonoBehaviour
{
    public void ToggleBoxCollider(bool value)
    {
        GetComponent<BoxCollider>().enabled = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        CallToolItem(other);
    }

    private void CallToolItem(Collider other)
    {
        PlayerController.playerController.CurrentEquippedTool.OnCollisionDetected(other);

    }
}
