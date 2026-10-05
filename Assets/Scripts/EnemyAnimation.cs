using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void TurnAnimationOff()
    {
        animator.SetBool("takeHit", false);
    }

    public void Die()
    {
        transform.parent.GetComponent<GhostEnemy>().OnDefeat();
    }
}
