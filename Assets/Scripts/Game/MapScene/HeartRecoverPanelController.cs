﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class HeartRecoverPanelController : MonoBehaviour
{
    private Text countDownText;
    private Text tileText;
    private Text nextText;
    private Text starText;
    private Text allLifeText;

    private GameObject buyButton;
    private Text buyText;

    private Action onBuyAction;
    private Popup popup;

    void Awake()
    {
        countDownText = transform.Find("downTime_Text").GetComponent<Text>();
        tileText = transform.Find("title_Text").GetComponent<Text>();
        nextText = transform.Find("next_Text").GetComponent<Text>();
        starText = transform.Find("starNum_Text").GetComponent<Text>();
        allLifeText = transform.Find("all_life_text").GetComponent<Text>();

        buyButton = transform.Find("buyPanel").gameObject;
        buyText = buyButton.transform.Find("Button/iconNum_Text").GetComponent<Text>();

        popup = GetComponent<Popup>();
    }

    void Start()
    {
        tileText.text = LanguageManager.instance.GetValueByKey("200039");
		nextText.text = LanguageManager.instance.GetValueByKey("210151");

        buyButton.SetActive(qy.GameMainManager.Instance.playerData.heartNum == 0);
        buyText.text = qy.GameMainManager.Instance.configManager.settingConfig.livesPrice.ToString();
    }

    void Update()
    {
		var heartNum = qy.GameMainManager.Instance.playerData.heartNum;
		if (heartNum < 5)
        {
			allLifeText.gameObject.SetActive (false);
			if (heartNum == 0) {
				buyButton.gameObject.SetActive (true);
			}
            int t = qy.GameMainManager.Instance.playerData.countDown;
            TimeSpan ts = new TimeSpan(0, 0, t);
            countDownText.text = string.Format("{0}:{1}", ts.Minutes.ToString("D2"), ts.Seconds.ToString("D2"));
            starText.text = qy.GameMainManager.Instance.playerData.heartNum.ToString();
        }
        else
        {
            countDownText.gameObject.SetActive(false);
            nextText.gameObject.SetActive(false);
			allLifeText.gameObject.SetActive (true);

            allLifeText.text = LanguageManager.instance.GetValueByKey("200022");
        }
    }

    public void OnBuyHeart()
    {
        onBuyAction();
        popup.Close();
    }

    public void RegisterCallback(Action action)
    {
        onBuyAction = action;
    }
}
