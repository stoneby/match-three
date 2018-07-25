using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIDialoguePerson : MonoBehaviour
{
    public SpriteAnimation mouth;

    private void Awake()
    {
        mouth.Loop = true;
        mouth.AutoPlay = false;
    }

    public void StartTalk()
    {
        mouth.Play();
    }

    public void StopTalk()
    {
        mouth.Stop();
    }

    public void Show()
    {
        transform.DOScale(1.1f, 0.5f);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        transform.DOScale(0.9f, 0.5f);
    }
}
