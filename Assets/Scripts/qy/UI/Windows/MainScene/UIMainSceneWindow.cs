﻿using March.Core.WindowManager;
using qy;
using qy.ui;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIMainSceneWindow : UIWindowBase
{
    public Text lifeText;
    public Text lifeCountDownText;
    public Text coinText;
    public Text starText;
    public Text levelText;
    public GameObject taskTips;

    public override UIWindowData windowData
    {
        get
        {
            UIWindowData windowData = new UIWindowData
            {
                id = qy.ui.UISettings.UIWindowID.UIMainSceneWindow,
                type = qy.ui.UISettings.UIWindowType.Fixed,
            };

            return windowData;
        }
    }
    private void Awake()
    {
        Messenger.AddListener(ELocalMsgID.RefreshBaseData, UpdateUI);
        Messenger.AddListener(ELocalMsgID.OpenLevelBeginPanel, OnClickLevelBtn);
        UpdateUI();
    }

    private void OnDestroy()
    {
        Messenger.RemoveListener(ELocalMsgID.RefreshBaseData, UpdateUI);
        Messenger.RemoveListener(ELocalMsgID.OpenLevelBeginPanel, OnClickLevelBtn);
    }

    private void Update()
    {
        UpdateCountDown();
    }
    protected override void StartShowWindow(object[] data)
    {
        base.StartShowWindow(data);
    }

    public void OnClickTaskBtn()
    {
        if (GameMainManager.Instance.playerData.GetRoleState(GameMainManager.Instance.playerData.role.id) != qy.PlayerData.RoleState.Normal)
            GameMainManager.Instance.uiManager.OpenWindow(qy.ui.UISettings.UIWindowID.UIEndingWindow, GameMainManager.Instance.playerData.GetQuest());
        else
            GameMainManager.Instance.uiManager.OpenWindow(qy.ui.UISettings.UIWindowID.UITaskWindow, GameMainManager.Instance.playerData);
    }

    public void OnClickLevelBtn()
    {
        GameMainManager.Instance.netManager.MakePointInEliminateClick((ret, res) => { });

        if (GameMainManager.Instance.playerData.heartNum < 1)
        {
            WindowManager.instance.Show<UIAlertPopupWindow>().Init(LanguageManager.instance.GetValueByKey("200025"));
        }
        //else if (GameMainManager.Instance.playerData.eliminateLevel > GameMainManager.Instance.configManager.settingConfig.max)
        //{
        //    WindowManager.instance.Show<UIAlertPopupWindow>().Init(LanguageManager.instance.GetValueByKey("200049"));
        //}
        else
        {
#if GAME_DEBUG
            Instantiate(Resources.Load<GameObject>(Configure.DebugLevelChoosePanelPath),
                ((MonoBehaviour)UIManager.Instance).transform);
#else
            WindowManager.instance.Show<BeginPopupWindow>();
#endif
        }
    }

    public void OnClickSettingBtn()
    {
        WindowManager.instance.Show<SettingPanelPopupWindow>();
    }

    public void OnClickAddHeartBtn()
    {
        var heartPanelController = WindowManager.instance.Show<HeartRecoveryPanelPopupWindow>().GetComponent<HeartRecoverPanelController>();
        heartPanelController.RegisterCallback(() =>
        {
            PlayerModelErr err = GameMainManager.Instance.playerModel.BuyHeart();
            if (err != PlayerModelErr.NULL)
            {
                Alert.Show(GameMainManager.Instance.playerModel.GetErrorDes(err));
            }
            else
            {
                UpdateUI();
            }

        });
    }

    public void OnClickAddGoldBtn()
    {
        WindowManager.instance.Show<ShopPopupPlayWindow>().GetComponent<HeartRecoverPanelController>();
    }

    private void UpdateUI()
    {
        lifeText.text = GameMainManager.Instance.playerData.heartNum.ToString();
        coinText.text = GameUtils.GetCurrencyString(GameMainManager.Instance.playerData.coinNum);
        starText.text = GameMainManager.Instance.playerData.starNum.ToString();
        levelText.text = GameMainManager.Instance.playerData.eliminateLevel.ToString();
    }

    private int lastTime = -1;

    private void UpdateCountDown()
    {
        int t = GameMainManager.Instance.playerData.countDown;

        if (t != lastTime)
        {
            TimeSpan ts = new TimeSpan(0, 0, t);
            lifeCountDownText.text = string.Format("{0}:{1}", ts.Minutes.ToString("D2"), ts.Seconds.ToString("D2")); ;
            lastTime = t;
            if (GameMainManager.Instance.playerData.heartNum >= GameMainManager.Instance.configManager.settingConfig.maxLives)
            {
                lifeCountDownText.text = LanguageManager.instance.GetValueByKey("200021");
            }
            else if (t <= 0)
            {
                GameMainManager.Instance.playerModel.UpdateHeart();
                UpdateUI();
            }
        }
    }
}
