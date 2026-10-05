using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator animator;
    float swingSpeed;

    public void ChangeSwing(bool value, float multiplier)
    {
        animator.SetBool("isSwinging", value);
        animator.speed *=  multiplier;
        swingSpeed = animator.speed;
    }

    public void SwingOff()
    {
        ChangeSwing(false, 1/swingSpeed);
    }

    public bool CheckSwing()
    {
        return animator.GetBool("isSwinging");
    }
}
