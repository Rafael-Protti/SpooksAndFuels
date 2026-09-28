using UnityEngine;

public class ToolAnimation : MonoBehaviour
{
    public PlayerAnimation playerAnimation;

    public void SwingOff()
    {
        if (playerAnimation == null) return;

        playerAnimation.SwingOff();
        PlayerController.playerController.toolHitBox.ToggleBoxCollider(false);
    }
}
