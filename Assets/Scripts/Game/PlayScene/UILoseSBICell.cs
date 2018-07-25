using qy;
using UnityEngine;
using UnityEngine.UI;

public class UILoseSBICell : MonoBehaviour
{
    public Image itemIcon;
    public Text itemNum;

    public void Init(string itemId, int num)
    {
        itemIcon.gameObject.SetActive(true);

        var icon = GameMainManager.Instance.configManager.propsConfig.GetItem(itemId).icon;
        itemIcon.sprite = Resources.Load(string.Format("UI/{0}", icon), typeof(Sprite)) as Sprite;
		itemIcon.SetNativeSize ();
    }

	public void Init()
	{
		itemIcon.gameObject.SetActive(true);

		itemIcon.sprite = Resources.Load("UI/loseAdd", typeof(Sprite)) as Sprite;
		itemIcon.transform.localPosition = new Vector3 (20, 0, 0);
		itemIcon.SetNativeSize ();
	}
}
