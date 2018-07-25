using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingProgressController : MonoBehaviour
{
    public int Partition;
    public float TweenDuration;

    [Range(0f, 1f)]
    public float MaxFillAmount;

    public Ease EaseType;

    public string Description
    {
        set { DescriptionText.text = value; }
    }

    public Image Bar;
    public Image Topping;
    public Image BarRight;
    public Text PercentageText;
    public Text DescriptionText;

    private Tweener currentTweener;

    private void Awake()
    {
        NotifyUI(0f);
    }

    private void GenerateTween(float start)
    {
        var end = start + 1f / Partition * (1 - start);

        if (end > MaxFillAmount)
            return;

        currentTweener = Bar.DOFillAmount(start + 1f / Partition * (1 - start), TweenDuration).SetEase(EaseType)
            .OnUpdate(() => NotifyUI(Bar.fillAmount)).OnComplete(() =>
             {
                 start = end;
                 GenerateTween(start);
             });
    }

    public void StopTween()
    {
        currentTweener.Kill();
        NotifyUI(1f);
    }

    public void NotifyUI(float percentage)
    {
        Bar.fillAmount = percentage;
        Topping.fillAmount = percentage;

        PercentageText.text = string.Format("{0:0.0}%", Bar.fillAmount * 100);

        BarRight.gameObject.SetActive(Bar.fillAmount >= 1f);
    }
}
