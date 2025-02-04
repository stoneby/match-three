﻿using DG.Tweening;
using qy;
using qy.config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyLandingActivities : MonoBehaviour
{
    public Text m_titleText;
    public Text m_des;
    public Text m_btnText;

    public Transform m_continueBtn;

    private List<GameObject> target_golds = new List<GameObject>();
    private List<GameObject> target_items = new List<GameObject>();

    private int m_day;

    void Awake()
    {
        m_titleText.text = LanguageManager.instance.GetValueByKey("210137");
        m_des.text = LanguageManager.instance.GetValueByKey("210138");
        m_btnText.text = LanguageManager.instance.GetValueByKey("210136");
    }

    private void addGold(int i, string gold)
    {
        var go = transform.Find(string.Format("item{0}", i));
        var b_go = go.transform.Find("Button");
        string g = "gold";
        if (i == 7)
        {
            g += "0";
        }
        var gold_go = b_go.transform.Find(g);
        gold_go.gameObject.SetActive(true);
        var gold_num_go = gold_go.transform.Find("num");
        gold_num_go.GetComponent<Text>().text = gold;
    }

    //
    private void addItem(int i, string item)
    {
        var go = transform.Find(string.Format("item{0}", i));
        var b_go = go.transform.Find("Button");
        string i_ = "item";
        string n_ = "num";
        if (i == 7)
        {
            i_ += "1";
            n_ += "1";
        }
        var item_go = b_go.transform.Find(i_);
        item_go.gameObject.SetActive(true);

        string itemId = item.Split(':')[0];
        //GoodsItem goodsItem = DefaultConfig.getInstance().GetConfigByType<item>().GetItemByID(itemId);
        PropItem goodsItem = GameMainManager.Instance.configManager.propsConfig.GetItem(itemId);
        Sprite sp = Resources.Load(string.Format("Sprites/UI/{0}", goodsItem.icon), typeof(Sprite)) as Sprite;
        Image icon = item_go.GetComponent<Image>();
        icon.sprite = sp;
        icon.SetNativeSize();
        icon.GetComponent<Transform>().localScale = new Vector3(0.3f, 0.3f, 0.3f);
        int itemNum = int.Parse(item.Split(':')[1]);
        if (itemNum > 1)
        {
            var numText = b_go.transform.Find(n_);
            numText.gameObject.SetActive(true);
            numText.GetComponent<Text>().text = "×" + itemNum;
        }
    }

    private void getAward()
    {
        //NetManager.instance.saveDayAward("{}");
        qy.GameMainManager.Instance.netManager.SevenDayAward((ret, res) =>
        {

        });
    }

    public void Init(int day)
    {
        m_day = day;
        for (int i = 1; i < day; i++)
        {
            var go = transform.Find(string.Format("item{0}", i));
            var b_go = go.transform.Find("Button");
            var i_go = b_go.transform.Find("surface");
            i_go.gameObject.SetActive(false);
        }
        StartCoroutine(dismissDay());
    }

    IEnumerator dismissDay()
    {
        yield return new WaitForSeconds(0.5f);
        var go = transform.Find(string.Format("item{0}", m_day));
        var b_go = go.transform.Find("Button");
        var i_go = b_go.transform.Find("surface");
        var sele_go = b_go.transform.Find("sele");
        var day = i_go.transform.Find("day");
        day.transform.GetComponent<Text>().DOFade(0, 1f);
        if (m_day == 7)
        {
            var bg = i_go.transform.Find("bg");
            bg.transform.GetComponent<Image>().DOFade(0, 1f);
        }
        i_go.transform.DOLocalMove(new Vector3(0, -50, 0), 1f).OnComplete(() => m_continueBtn.gameObject.SetActive(true));
        i_go.transform.GetComponent<Image>().DOFade(0, 1f).OnComplete(() => sele_go.gameObject.SetActive(true));
    }

    private void setTargetGolds()
    {
        var go = transform.Find(string.Format("item{0}", m_day));
        var b_go = go.transform.Find("Button");
        string g = "gold";
        if (m_day == 7)
        {
            g += "0";
        }
        for (int i = 0; i < 5; i++)
        {
            var gold_go = b_go.transform.Find(g);
            gold_go.transform.Find("num").gameObject.SetActive(false);
            var cloneGold = Instantiate(gold_go.gameObject, GameObject.FindObjectOfType<Canvas>().transform);
            cloneGold.transform.position = gold_go.transform.position;
            target_golds.Add(cloneGold);
        }
    }

    private void setTargetItems()
    {
        var go = transform.Find(string.Format("item{0}", m_day));
        var b_go = go.transform.Find("Button");
        //string item = m_Items[m_day - 1];
        //int itemNum = int.Parse(item.Split(':')[1]);
        int itemNum = GameMainManager.Instance.configManager.settingConfig.GetSevenDayProp(m_day - 1)[0].count;
        string i_ = "item";
        if (m_day == 7)
        {
            i_ += "1";
        }
        for (int i = 0; i < itemNum; i++)
        {
            var item_go = b_go.transform.Find(i_);
            var cloneItem = Instantiate(item_go.gameObject, GameObject.FindObjectOfType<Canvas>().transform);
            cloneItem.transform.position = item_go.transform.position;
            target_items.Add(cloneItem);
        }
    }

    public void OnCloseClick()
    {
        getAward();//领奖并关闭界面
        GetComponent<Popup>().Close();

        //string golds = m_Golds[m_day - 1];
        string golds = GameMainManager.Instance.configManager.settingConfig.GetSevenDayGold(m_day - 1).ToString();
        if (m_day == 7)
        {
            setTargetGolds();
            setTargetItems();
        }
        else
        {
            if (golds != "0")
            {
                setTargetGolds();
            }
            else
            {
                setTargetItems();
            }
        }
        var canvas = GameObject.FindObjectOfType<Canvas>();
        var baseinfo = canvas.transform.Find("baseinfo");
        var coin = baseinfo.transform.Find("coin");
        var coinImg = coin.transform.Find("coinImg");
        var eliminate = canvas.transform.Find("eliminate");
        var coinImg_p = coinImg.transform.position;
        for (int i = 0; i < target_golds.Count; i++)
        {
            var gold = target_golds[i];
            gold.transform.DOMove(coinImg.transform.position, 1f).OnComplete(() =>
              Destroy(gold)
            );
            Vector3[] path = { new Vector3(gold.transform.position.x - 40 * i, gold.transform.position.y - 20 * i, 0), new Vector3(coinImg.transform.position.x, coinImg.transform.position.y, 0) };
            gold.transform.DOPath(path, 1f).OnComplete(() => Destroy(gold));
            //StartCoroutine(CollectItemAnim(gold, coinImg));
        }
        for (int j = 0; j < target_items.Count; j++)
        {
            var item = target_items[j];
            Vector3[] path = { new Vector3(item.transform.position.x + 200, item.transform.position.y + 100, 0), new Vector3(eliminate.transform.position.x, eliminate.transform.position.y, 0) };
            item.transform.DOPath(path, 1f).OnComplete(() => Destroy(item));
            //StartCoroutine(CollectItemAnim(item, eliminate));
        }
    }
}
