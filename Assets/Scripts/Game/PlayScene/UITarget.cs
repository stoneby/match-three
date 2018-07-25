using System.Collections.Generic;
using UnityEngine;

public class UITarget : MonoBehaviour
{
    public List<UITargetCell> TargetCellList;

    public GameObject TargetLayout;

    void Start()
    {
        for (int i = 0; i < LevelLoader.instance.targetList.Count; i++)
        {
            var go = Instantiate(Resources.Load(Configure.UITargetCellPrefab), TargetLayout.transform) as GameObject;
            go.GetComponent<UITargetCell>().Init(LevelLoader.instance.targetList[i], true);

            TargetCellList.Add(go.GetComponent<UITargetCell>());
        }
    }

    public void UpdateTargetAmount(int index)
    {
        if (index < TargetCellList.Count)
        {
            TargetCellList[index].Amount.text = GameObject.Find("Board").GetComponent<Board>().targetLeftList[index].ToString();
            if (int.Parse(TargetCellList[index].Amount.text) <= 0)
            {
                TargetCellList[index].Amount.gameObject.SetActive(false);
                TargetCellList[index].TargetTick.gameObject.SetActive(true);
            }
        }
    }
}
