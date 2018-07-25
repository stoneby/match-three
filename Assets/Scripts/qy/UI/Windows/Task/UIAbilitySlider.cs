using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIAbilitySlider : MonoBehaviour {


    public Slider slider;
    public Text changeText;

    private int value;
    private bool isInit = false;
    public void SetData(int value)
    {
        int change = value - this.value;
        this.value = value;
        slider.value = value / 100f;
        if(!isInit)
        {
            isInit = true;
        }else if(change!=0)
        {
            
            string str = Mathf.Abs(change).ToString();
            Debug.Log("=" + str + "=");
            changeText.text = (change>0?"+":"-") + str;
            changeText.gameObject.SetActive(true);
            changeText.transform.DOLocalMoveY(0, 1f).From().SetEase(Ease.OutCubic).OnComplete(() =>
            {
                changeText.gameObject.SetActive(false);
            });
        }
        

    }
}
