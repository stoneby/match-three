﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using qy.ui;
using UnityEngine.UI;
using qy.config;
using qy;

public class UIRoleWindow : UIWindowBase {

    //public AutoScrollView scrollView;
    public GameObjectPool pool;
    public Image head;
    public Slider discipline;
    public Slider wisdomSlider;
    public Slider loyaltySlider;
    public Text roleName;
    public Text introduction;
    public GameObject stateGO;
    public Text stateText;

    private string selectedRoleID;

    public override UIWindowData windowData
    {
        get
        {
            UIWindowData windowData = new UIWindowData
            {
                id = qy.ui.UISettings.UIWindowID.UIRoleWindow,
                type = qy.ui.UISettings.UIWindowType.Fixed,
            };

            return windowData;
        }
    }

    private void Awake()
    {
        //scrollView.onSelected += OnSelected;

        Messenger.AddListener(ELocalMsgID.RefreshBaseData, UpdateUI);
    }

    private void OnDestroy()
    {
        //scrollView.onSelected -= OnSelected;
        Messenger.RemoveListener(ELocalMsgID.RefreshBaseData, UpdateUI);
    }

    public void OnSelected(BaseItemView item,bool isOn)
    {
        if(isOn)
        {
            UIRoleWindowHead roleItem = item as UIRoleWindowHead;
            SetPanel(roleItem.data);
            selectedRoleID = roleItem.data.id;
        }
       
    }
    

    protected override void StartShowWindow(object[] data)
    {
        UpdateUI();
    }
    private void UpdateUI()
    {
        List<RoleItem> roles = GameMainManager.Instance.configManager.roleConfig.GetRoleList();
        pool.resetAllTarget();
        for(int i=0;i<roles.Count;i++ )
        {
            RoleItem item = roles[i];
            UIRoleWindowHead headItem = pool.getIdleTarget<UIRoleWindowHead>();
            headItem.SetData(item);
            Toggle toggle = headItem.toggle;
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((isOn) =>
            {
                OnSelected(headItem,isOn);
            });
            if (i == 0)
            {
                toggle.isOn = true;
            }
            
        }
        //scrollView.SetData(roles);
    }
    private void SetPanel(RoleItem item)
    {
        var sp =
            March.Core.ResourceManager.ResourceManager.instance.Load<Sprite>(Configure.BundleStoryPerson,
                item.headIcon);
        head.sprite = sp;
        head.type = Image.Type.Simple;
        GameUtils.Scaling(head.transform as RectTransform, new Vector2(sp.texture.width, sp.texture.height));

        //string headUrl = FilePathTools.GetPersonHeadPath(item.headIcon);
        //AssetsManager.Instance.LoadAssetAsync<Sprite>(headUrl, (sp) =>
        //{
        //    head.sprite = sp;
        //    head.type = Image.Type.Simple;
        //    GameUtils.Scaling(head.transform as RectTransform,new Vector2(sp.texture.width,sp.texture.height));
        //});

        loyaltySlider.value = item.ability.loyalty/100f;
        wisdomSlider.value = item.ability.wisdom / 100f;
        discipline.value = item.ability.discipline / 100f;
        roleName.text = item.name;
        introduction.text = item.introduction;

        qy.PlayerData.RoleState state = GameMainManager.Instance.playerData.GetRoleState(item.id);
        switch(state)
        {
            case qy.PlayerData.RoleState.Dide:
                stateGO.SetActive(true);
                stateText.text = "已死亡";
                break;
            case qy.PlayerData.RoleState.Pass:
                stateGO.SetActive(true);
                stateText.text = "已通关";
                break;
            default:
                stateGO.SetActive(false);
                break;
        }
    }

    public void OnClickStartBtnHandle()
    {
        qy.PlayerData.RoleState state = GameMainManager.Instance.playerData.GetRoleState(selectedRoleID);
        if (state == qy.PlayerData.RoleState.Dide)
        {
            GameMainManager.Instance.uiManager.OpenWindow(qy.ui.UISettings.UIWindowID.UICallBackWindow, selectedRoleID);
        }
        else
        {
            GameMainManager.Instance.playerModel.StartGameWithRole(selectedRoleID);
            GameMainManager.Instance.uiManager.OpenWindow(qy.ui.UISettings.UIWindowID.UIMainSceneWindow);
            OnClickClose();
            
        }
    }

    public void OnClickHead(UIRoleWindowHead head)
    {
        RoleItem selectRole = head.data;
        List<RoleItem> roles = GameMainManager.Instance.configManager.roleConfig.GetRoleList();
        for(int i =0;i<roles.Count;i++)
        {
            RoleItem role = roles[i];
            if (role.id == selectRole.id)
            {
                //scrollView.SetSelected(i);
                break;
            }
        }

    }
}
