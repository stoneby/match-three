using UnityEngine;

[RequireComponent(typeof(SpriteAnimator))]
public class CFX_AutoDestructSpriteAnimation : CFX_AutoDestruct
{
    private SpriteAnimator animator;

    protected override void OnEnable()
    {
        animator = GetComponent<SpriteAnimator>();
        duration = animator.Duration;

        base.OnEnable();
    }
}
