﻿using System;
using System.Collections.Generic;
using System.Linq;
using March.Core.Network;
using March.Core.Pay;
using UnityEngine;
using UnityEngine.UI;
using qy;
using qy.config;

public class UIShopPopupPlay : MonoBehaviour 
{
	public Text titleText;

    public Text coinAmount;

    public List<Text> coinTextList;
    public List<Text> costTextList;

    public GameObject ItemPrefab;

    private GridLayoutGroup itemGridLayout;
    //private exchange exchange;
    private IAP iapController;

    private PayActionHandler actionHandler = new PayActionHandler();

    void Awake()
    {
        itemGridLayout = transform.Find("ItemContainer").GetComponent<GridLayoutGroup>();
        iapController = GetComponent<IAP>();
    }

	void Start () 
    {
		titleText.text = LanguageManager.instance.GetValueByKey ("210009");

        UpdateCoinAmountLabel();

        //exchange = DefaultConfig.getInstance().GetConfigByType<exchange>();
        List<qy.config.ExchangeItem> list = GameMainManager.Instance.configManager.exchangeConfig.GetList();
		var index = 0;
        list.ForEach(exchangeItem =>
        {
            var item = Instantiate(ItemPrefab);
            item.transform.SetParent(itemGridLayout.transform, false);
			var goldsp = item.transform.Find("GoldspBig").GetComponent<Image>();
			goldsp.sprite = Resources.Load(string.Format("Sprites/Cookie/UI/General/sc_g_{0}", ++index), typeof(Sprite)) as Sprite;
			goldsp.SetNativeSize();
			var gold = item.transform.Find("Coin").GetComponent<Text>();
            gold.text = exchangeItem.gold.ToString();
            var price = item.transform.Find("Button/Image/Cost").GetComponent<Text>();
            price.text = string.Format("${0}", exchangeItem.dollar);
            var button = item.transform.Find("Button/Image").GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                actionHandler.PayData.exchangeId = exchangeItem.id;
                //NetManager.instance.SendRequest(actionHandler);
                GameMainManager.Instance.netManager.SendRequest(actionHandler);
                iapController.BuyProductID(exchangeItem.id);
            });
        });

        //iapController.ProductItems = exchange.ExchangeItem.ExchangeItemList.Select(item => item.ID).ToList();
        iapController.ProductItems = list.Select(item => item.id).ToList();
        iapController.InitializePurchasing();
    }

    public void ButtonClickAudio()
    {
        AudioManager.instance.ButtonClickAudio();
    }

    public void UpdateCoinAmountLabel()
    {
        coinAmount.text = GameData.instance.GetPlayerCoin().ToString();
    }
}
