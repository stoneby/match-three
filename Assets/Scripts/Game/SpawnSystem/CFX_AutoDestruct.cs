using UnityEngine;

public class CFX_AutoDestruct : MonoBehaviour
{
    public bool OnlyDeactivate = true;

    public float Duration
    {
        get { return duration;}
        set
        {
            duration = value;
            CancelInvoke("DeadAction");
            Invoke("DeadAction", duration);
        }
    }

    protected float duration;

    protected virtual void OnEnable()
    {
        Invoke("DeadAction", Duration);
    }

    protected void DeadAction()
    {
        if (OnlyDeactivate)
        {
#if UNITY_3_5
			gameObject.SetActiveRecursively(false);
#else
            gameObject.SetActive(false);
#endif
        }
        else
            Destroy(gameObject);
    }

}
