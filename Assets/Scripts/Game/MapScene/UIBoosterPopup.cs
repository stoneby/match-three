using qy;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIBoosterPopup : MonoBehaviour
{
    public BOOSTER_TYPE booster;
    public Text cost1;
    public Text title;
    public Image itemImage;
    public Text itemNum;
    public Text itemDes;
    public Text cost2;

    public bool clicking;

    private BuyBtnCallBcak callBack;
    private Popup popup;

    public void Init(BuyBtnCallBcak callback)
    {
        callBack = callback;
    }

    void Start()
    {
        title.text = LanguageManager.instance.GetValueByKey("200017");

        int amount = Configure.instance.package1Amount;
        itemNum.text = string.Format("×{0}", amount);
        switch (booster)
        {
            case BOOSTER_TYPE.BEGIN_FIVE_MOVES:
                cost1.text = Configure.instance.beginFiveMovesCost1.ToString();
                cost2.text = Configure.instance.beginFiveMovesCost2.ToString();
                break;
            case BOOSTER_TYPE.BEGIN_RAINBOW_BREAKER:
                cost1.text = string.Format("{0}",GameMainManager.Instance.configManager.propsConfig.GetItem("200003").price * amount);
                itemDes.text = LanguageManager.instance.GetValueByKey("210004");
                itemImage.sprite = Resources.Load(string.Format("{0}/item002", Configure.UIItemBase), typeof(Sprite)) as Sprite;
                itemImage.SetNativeSize();
                break;
            case BOOSTER_TYPE.BEGIN_BOMB_BREAKER:
                cost1.text = string.Format("{0}", GameMainManager.Instance.configManager.propsConfig.GetItem("200004").price * amount);
                itemDes.text = LanguageManager.instance.GetValueByKey("210003");
                itemImage.sprite = Resources.Load(string.Format("{0}/item003", Configure.UIItemBase), typeof(Sprite)) as Sprite;
                itemImage.SetNativeSize();
                break;
            case BOOSTER_TYPE.BEGIN_PLANE_BREAKER:
                cost1.text = string.Format("{0}", GameMainManager.Instance.configManager.propsConfig.GetItem("200005").price * amount);
                itemDes.text = LanguageManager.instance.GetValueByKey("210005");
                itemImage.sprite = Resources.Load(string.Format("{0}/item004", Configure.UIItemBase), typeof(Sprite)) as Sprite;
                itemImage.SetNativeSize();
                break;

            case BOOSTER_TYPE.SINGLE_BREAKER:
                cost1.text = string.Format("{0}", GameMainManager.Instance.configManager.propsConfig.GetItem("200006").price * amount);
                itemDes.text = LanguageManager.instance.GetValueByKey("210002");
                itemImage.sprite = Resources.Load(string.Format("{0}/item005", Configure.UIItemBase), typeof(Sprite)) as Sprite;
                itemImage.SetNativeSize();
                break;
            
            default:
                Debug.LogError("Not supported booster type for now: " + booster);
                itemDes.text = "Not supported booster type for now: " + booster;
                break;
        }

        popup = GetComponent<Popup>();
    }

    public void BuyButtonClick(int package)
    {
        if (clicking)
            return;

        clicking = true;
        StartCoroutine(ResetButtonClick());

        int cost = 0;
        int amount = Configure.instance.package1Amount;

        if (package == 1)
        {
            switch (booster)
            {
                case BOOSTER_TYPE.BEGIN_FIVE_MOVES:
                    cost = Configure.instance.beginFiveMovesCost1;
                    amount = Configure.instance.package1Amount;
                    break;
                case BOOSTER_TYPE.BEGIN_RAINBOW_BREAKER:
                    cost = GameMainManager.Instance.configManager.propsConfig.GetItem("200003").price * amount;
                    break;
                case BOOSTER_TYPE.BEGIN_BOMB_BREAKER:
                    cost = GameMainManager.Instance.configManager.propsConfig.GetItem("200004").price * amount;
                    break;
                case BOOSTER_TYPE.BEGIN_PLANE_BREAKER:
                    cost = GameMainManager.Instance.configManager.propsConfig.GetItem("200005").price * amount;
                    break;
                case BOOSTER_TYPE.SINGLE_BREAKER:
                    cost = GameMainManager.Instance.configManager.propsConfig.GetItem("200006").price * amount;
                    break;
                case BOOSTER_TYPE.ROW_BREAKER:
                    cost = Configure.instance.rowBreakerCost1;
                    amount = Configure.instance.package1Amount;
                    break;
                case BOOSTER_TYPE.COLUMN_BREAKER:
                    cost = Configure.instance.columnBreakerCost1;
                    amount = Configure.instance.package1Amount;
                    break;
                case BOOSTER_TYPE.RAINBOW_BREAKER:
                    cost = Configure.instance.rainbowBreakerCost1;
                    amount = Configure.instance.package1Amount;
                    break;
                case BOOSTER_TYPE.OVEN_BREAKER:
                    cost = Configure.instance.ovenBreakerCost1;
                    amount = Configure.instance.package1Amount;
                    break;
            }
        }

        var coin = GameMainManager.Instance.playerData.coinNum;
        if (cost <= coin)
        {
            AudioManager.instance.CoinPayAudio();

            switch (booster)
            {
                case BOOSTER_TYPE.BEGIN_FIVE_MOVES:
                    GameData.instance.SaveBeginFiveMoves(amount);
                    GameMainManager.Instance.playerModel.BuyProp("200007", amount);
                    break;
                case BOOSTER_TYPE.BEGIN_RAINBOW_BREAKER:
                    GameData.instance.SaveBeginRainbow(amount);
                    GameMainManager.Instance.playerModel.BuyProp("200003", amount);
                    break;
                case BOOSTER_TYPE.BEGIN_BOMB_BREAKER:
                    GameData.instance.SaveBeginBombBreaker(amount);
                    GameMainManager.Instance.playerModel.BuyProp("200004", amount);
                    break;
                case BOOSTER_TYPE.BEGIN_PLANE_BREAKER:
                    GameData.instance.SaveBeginBombBreaker(amount);
                    GameMainManager.Instance.playerModel.BuyProp("200005", amount);
                    break;
                case BOOSTER_TYPE.SINGLE_BREAKER:
                    GameData.instance.SaveSingleBreaker(amount);
                    GameMainManager.Instance.playerModel.BuyProp("200006", amount);
                    break;
                case BOOSTER_TYPE.ROW_BREAKER:
                    GameData.instance.SaveRowBreaker(amount);
                    break;
                case BOOSTER_TYPE.COLUMN_BREAKER:
                    GameData.instance.SaveColumnBreaker(amount);
                    break;
                case BOOSTER_TYPE.RAINBOW_BREAKER:
                    GameData.instance.SaveRainbowBreaker(amount);
                    break;
                case BOOSTER_TYPE.OVEN_BREAKER:
                    GameData.instance.SaveOvenBreaker(amount);
                    break;
            }

            var c = coin - cost;
            GameData.instance.SavePlayerCoin(c);
            var coinTextGo = GameObject.Find("coinText");
            if (coinTextGo != null)
            {
                coinTextGo.GetComponent<Text>().text = string.Format("{0}", c);
            }

            if (callBack != null)
            {
                callBack(amount);
            }

            popup.Close();

            var go = GameObject.Find("Board");
            if (go != null)
            {
                go.GetComponent<Board>().state = GAME_STATE.WAITING_USER_SWAP;
            }
        }
    }

    IEnumerator ResetButtonClick()
    {
        yield return new WaitForSeconds(1f);

        clicking = false;
    }

    public void ButtonClickAudio()
    {
        AudioManager.instance.ButtonClickAudio();
    }

    public void CloseButtonClick()
    {
        AudioManager.instance.ButtonClickAudio();

        // if booster is in game we re-set game state
        var go = GameObject.Find("Board");
        if (go != null)
        {
            go.GetComponent<Board>().state = GAME_STATE.WAITING_USER_SWAP;
        }
    }
}
