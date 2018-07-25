using UnityEngine;
using UnityEngine.UI;

public class UITargetCell : MonoBehaviour
{
    public Image Image;
    public Text Amount;
    public GameObject TargetTick;

    public void Init(Target target, bool isTarget)
    {
        Image.gameObject.SetActive(true);
        TargetTick.gameObject.SetActive(false);

        Amount.text = target.Amount.ToString();

        var prefabStr = target.ToPrefabStr();
        GameObject prefab = CFX_SpawnSystem.GetNextObject(prefabStr, transform, false) as GameObject;
        if (prefab != null)
            Image.sprite = prefab.GetComponent<SpriteRenderer>().sprite;
    }
}
