﻿using March.Core.WindowManager;
using UnityEngine;
using UnityEngine.UI;
using qy;
using qy.config;
using GuideManager = March.Core.Guide.GuideManager;

public class Booster : MonoBehaviour 
{
    public static Booster instance = null;

	[Header("Other")]
	public Material m_material;
	public Image icon;
	public Image m_lockImage;
	public Image numBg;

    [Header("Board")]
    private Board board;

    [Header("Booster")]
    public GameObject singleBooster;

    [Header("Active")]
    public GameObject singleActive;

    [Header("Amount")]
    public Text singleAmount;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }
    }

	void Start ()
	{
	    board = GameObject.Find("Board").GetComponent<Board>();

        singleBooster.SetActive(true);
        PropItem prop = GameMainManager.Instance.playerData.GetPropItem("200006");
        singleAmount.text = prop!=null?prop.count.ToString():"0";

        // single breaker
        //todo:从表里读取
        if (LevelLoader.instance.level < 9)
        {
			m_lockImage.gameObject.SetActive (true);
		}
        Messenger.AddListener(ELocalMsgID.RefreshBaseData,refresh);
    }

    void OnDestroy()
    {
        Messenger.RemoveListener(ELocalMsgID.RefreshBaseData,refresh);
    }

    #region Single

    public void SingleBoosterClick()
    {
		if (LevelLoader.instance.level < 9) {
			WindowManager.instance.Show<UIAlertPopupWindow>().Init(LanguageManager.instance.GetValueByKey("210134"));
			return;
		}
        if (board.state != GAME_STATE.WAITING_USER_SWAP || board.lockSwap)
        {
            return;
        }

        AudioManager.instance.ButtonClickAudio();

        board.dropTime = 1;

        // check amount
        PropItem prop = GameMainManager.Instance.playerData.GetPropItem("200006");
        int count = prop != null ? prop.count : 0;
        if (count<=0)
        {
            // show booster popup
            ShowPopup(BOOSTER_TYPE.SINGLE_BREAKER);

            return;
        }

        if (board.booster == BOOSTER_TYPE.NONE)
        {
            ActiveBooster(BOOSTER_TYPE.SINGLE_BREAKER);
			GuideManager.instance.Hide ();
			GuideManager.instance.Show ();
        }
        else
        {
            CancelBooster(BOOSTER_TYPE.SINGLE_BREAKER);
        }
    }

    #endregion

//    #region Row
//
//    public void RowBoosterClick()
//    {
//        if (board.state != GAME_STATE.WAITING_USER_SWAP || board.lockSwap == true)
//        {
//            return;
//        }
//
//        AudioManager.instance.ButtonClickAudio();
//
//        board.dropTime = 1;
//
//        // hide help
//        if (LevelLoader.instance.level == 12)
//        {
//            // hide step 1
//            Help.instance.Hide();
//
//            // show step 2
//            if (Help.instance.step == 1)
//            {
//                var prefab = Instantiate(Resources.Load(Configure.Level12Step2())) as GameObject;
//                prefab.name = "Level 12 Step 2";
//
//                prefab.gameObject.transform.SetParent(Help.instance.gameObject.transform);
//                prefab.GetComponent<RectTransform>().localScale = Vector3.one;
//
//                Help.instance.current = prefab;
//
//                Help.instance.step = 2;
//            }
//        }
//
//        // check amount
//
//        if (GameData.instance.GetRowBreaker() <= 0)
//        {
//            // show booster popup
//            ShowPopup(BOOSTER_TYPE.ROW_BREAKER);
//
//            return;
//        }
//
//        if (board.booster == BOOSTER_TYPE.NONE)
//        {
//            ActiveBooster(BOOSTER_TYPE.ROW_BREAKER);
//        }
//        else
//        {
//            CancelBooster(BOOSTER_TYPE.ROW_BREAKER);
//        }
//    }
//
//    #endregion
//
//    #region Column
//
//    public void ColumnBoosterClick()
//    {
//        if (board.state != GAME_STATE.WAITING_USER_SWAP || board.lockSwap == true)
//        {
//            return;
//        }
//
//        AudioManager.instance.ButtonClickAudio();
//
//        board.dropTime = 1;
//
//        // hide help
//        if (LevelLoader.instance.level == 15)
//        {
//            // hide step 1
//            Help.instance.Hide();
//
//            // show step 2
//            if (Help.instance.step == 1)
//            {
//                var prefab = Instantiate(Resources.Load(Configure.Level15Step2())) as GameObject;
//                prefab.name = "Level 15 Step 2";
//
//                prefab.gameObject.transform.SetParent(Help.instance.gameObject.transform);
//                prefab.GetComponent<RectTransform>().localScale = Vector3.one;
//
//                Help.instance.current = prefab;
//
//                Help.instance.step = 2;
//            }
//        }
//
//        // check amount
//
//        if (GameData.instance.GetColumnBreaker() <= 0)
//        {
//            // show booster popup
//            ShowPopup(BOOSTER_TYPE.COLUMN_BREAKER);
//
//            return;
//        }
//
//        if (board.booster == BOOSTER_TYPE.NONE)
//        {
//            ActiveBooster(BOOSTER_TYPE.COLUMN_BREAKER);
//        }
//        else
//        {
//            CancelBooster(BOOSTER_TYPE.COLUMN_BREAKER);
//        }
//    }
//
//    #endregion
//
//    #region Rainbow
//
//    public void RainbowBoosterClick()
//    {
//        if (board.state != GAME_STATE.WAITING_USER_SWAP || board.lockSwap == true)
//        {
//            return;
//        }
//
//        AudioManager.instance.ButtonClickAudio();
//
//        board.dropTime = 1;
//
//        // hide help
//        if (LevelLoader.instance.level == 18)
//        {
//            // hide step 1
//            Help.instance.Hide();
//
//            // show step 2
//            if (Help.instance.step == 1)
//            {
//                var prefab = Instantiate(Resources.Load(Configure.Level18Step2())) as GameObject;
//                prefab.name = "Level 18 Step 2";
//
//                prefab.gameObject.transform.SetParent(Help.instance.gameObject.transform);
//                prefab.GetComponent<RectTransform>().localScale = Vector3.one;
//
//                Help.instance.current = prefab;
//
//                Help.instance.step = 2;
//            }
//        }
//
//        // check amount
//
//        if (GameData.instance.GetRainbowBreaker() <= 0)
//        {
//            // show booster popup
//            ShowPopup(BOOSTER_TYPE.RAINBOW_BREAKER);
//
//            return;
//        }
//
//        if (board.booster == BOOSTER_TYPE.NONE)
//        {
//            ActiveBooster(BOOSTER_TYPE.RAINBOW_BREAKER);
//        }
//        else
//        {
//            CancelBooster(BOOSTER_TYPE.RAINBOW_BREAKER);
//        }
//    }
//
//    #endregion
//
//    #region Oven
//
//    public void OvenBoosterClick()
//    {
//        if (board.state != GAME_STATE.WAITING_USER_SWAP || board.lockSwap == true)
//        {
//            return;
//        }
//
//        AudioManager.instance.ButtonClickAudio();
//
//        board.dropTime = 0;
//
//        // hide help
//        if (LevelLoader.instance.level == 25)
//        {
//            // hide step 1
//            Help.instance.Hide();
//
//            // show step 2
//            if (Help.instance.step == 1)
//            {
//                var prefab = Instantiate(Resources.Load(Configure.Level25Step2())) as GameObject;
//                prefab.name = "Level 25 Step 2";
//
//                prefab.gameObject.transform.SetParent(Help.instance.gameObject.transform);
//                prefab.GetComponent<RectTransform>().localScale = Vector3.one;
//
//                Help.instance.current = prefab;
//
//                Help.instance.step = 2;
//            }
//        }
//
//        // check amount
//
//        if (GameData.instance.GetOvenBreaker() <= 0)
//        {
//            // show booster popup
//            ShowPopup(BOOSTER_TYPE.OVEN_BREAKER);
//
//            return;
//        }
//
//        if (board.booster == BOOSTER_TYPE.NONE)
//        {
//            ActiveBooster(BOOSTER_TYPE.OVEN_BREAKER);
//        }
//        else
//        {
//            CancelBooster(BOOSTER_TYPE.OVEN_BREAKER);
//        }
//    }
//
//    #endregion

    #region Complete

    public void BoosterComplete()
    {
        if (board.booster == BOOSTER_TYPE.SINGLE_BREAKER)
        {
            CancelBooster(BOOSTER_TYPE.SINGLE_BREAKER);
			GuideManager.instance.Hide ();
            // reduce amount
            PropItem prop = GameMainManager.Instance.playerData.GetPropItem("200006");
            int count = prop != null ? prop.count : 0;
            if (count > 0)
            {
                Debug.Log(count);

                var amount = count - 1;

                //NetManager.instance.userToolsToServer("200006", "1");
                GameMainManager.Instance.playerModel.UseProp("200006",1);
                // change text

                singleAmount.text = amount.ToString();
            }
        }
    }

    #endregion

    #region Popup

    public void ShowPopup(BOOSTER_TYPE check)
    {
        if (check == BOOSTER_TYPE.SINGLE_BREAKER)
        {
            board.state = GAME_STATE.OPENING_POPUP;

            var booster = WindowManager.instance.Show<SingleBoosterPopupWindow>().GetComponent<UIBoosterPopup>();
            booster.booster = BOOSTER_TYPE.SINGLE_BREAKER;
        }
    }

    #endregion

    #region Booster

    public void ActiveBooster(BOOSTER_TYPE check)
    {
        if (check == BOOSTER_TYPE.SINGLE_BREAKER)
        {
            board.booster = BOOSTER_TYPE.SINGLE_BREAKER;

            singleActive.SetActive(true);

            // interactable
//            rowActive.transform.parent.GetComponent<AnimatedButton>().interactable = false;
//            columnActive.transform.parent.GetComponent<AnimatedButton>().interactable = false;
//            rainbowActive.transform.parent.GetComponent<AnimatedButton>().interactable = false;
//            ovenActive.transform.parent.GetComponent<AnimatedButton>().interactable = false;
        }
    }

    public void CancelBooster(BOOSTER_TYPE check)
    {
        board.booster = BOOSTER_TYPE.NONE;

        if (check == BOOSTER_TYPE.SINGLE_BREAKER)
        {
            singleActive.SetActive(false);

            // interactable
//            rowActive.transform.parent.GetComponent<AnimatedButton>().interactable = true;
//            columnActive.transform.parent.GetComponent<AnimatedButton>().interactable = true;
//            rainbowActive.transform.parent.GetComponent<AnimatedButton>().interactable = true;
//            ovenActive.transform.parent.GetComponent<AnimatedButton>().interactable = true;
        }
    }

    #endregion

    #region refresh

    public void refresh()
    {
        PropItem prop = GameMainManager.Instance.playerData.GetPropItem("200006");
        int count = prop != null ? prop.count : 0;
        singleAmount.text = count.ToString();
    }


    #endregion
}
