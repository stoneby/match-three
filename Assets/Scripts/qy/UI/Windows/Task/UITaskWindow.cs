using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using qy.ui;
using qy;
using qy.config;
using UnityEngine.UI;
using March.Core.WindowManager;
public class UITaskWindow :  UIWindowBase{

    public override UIWindowData windowData
    {
        get
        {
            UIWindowData windowData = new UIWindowData
            {
                id = qy.ui.UISettings.UIWindowID.UITaskWindow,
                type = qy.ui.UISettings.UIWindowType.PopUp,
            };

            return windowData;
        }
    }

    public Image roleHeadImage;
    public Text roleNameText;

    public UIAbilitySlider disciplineSlider;
    public UIAbilitySlider loyaltySlider;
    public UIAbilitySlider wisdomSlider;


    public Text levelText;
    public Slider levelSlider;
    public Text levelProgressText;
    public Image taskImg;
    public Image propImg;
    public Text taskTitle;
    public Text taskDesText;
    public GameObjectPool propPool;
    public GameObjectPool selectPool;
    public Text tipsText;
    public Text doBtnText;
    public RectTransform missionTF;
    public RectTransform buttonsTF;
    public RectTransform actionBtn;

    private qy.PlayerData playerdata;
    private qy.config.QuestItem questItem;
    private bool selectBtnLock = false;

    protected override void StartShowWindow(object[] data)
    {
        selectBtnLock = false;
        qy.PlayerData player = data[0] as qy.PlayerData;
        playerdata = player;
        
        UpdatePanel();
        
    }

    private void UpdatePanel()
    {
        questItem = playerdata.GetQuest();

        var sp = March.Core.ResourceManager.ResourceManager.instance.Load<Sprite>(Configure.BundleStoryPerson,playerdata.role.headIcon);
        roleHeadImage.sprite = sp;
        GameUtils.Scaling(roleHeadImage.transform as RectTransform, new Vector2(sp.texture.width, sp.texture.height));

        //string headUrl = FilePathTools.GetPersonHeadPath(playerdata.role.headIcon);
        //AssetsManager.Instance.LoadAssetAsync<Sprite>(headUrl, (sp) =>
        //{
        //    roleHeadImage.sprite = sp;
        //    GameUtils.Scaling(roleHeadImage.transform as RectTransform,new Vector2(sp.texture.width,sp.texture.height));
        //});
        roleNameText.text = playerdata.role.name;
        levelText.text = playerdata.level.ToString();
        UpdateAbility(playerdata.ability);

        qy.config.LevelItem levelItem = GameMainManager.Instance.configManager.levelConfig.GetItem(playerdata.level);
        if(levelItem!=null)
        {
            float expProgress = playerdata.currExp / (float)levelItem.exp;
            levelSlider.value = expProgress;
            levelProgressText.text = (expProgress*100).ToString("f0")+"%";
        }else
        {
            levelSlider.value = 0;
            levelProgressText.text = "0%";
        }


        SetMainTask();

        if (!string.IsNullOrEmpty(questItem.bg))
        {
            taskImg.enabled = true;

            var sprite = March.Core.ResourceManager.ResourceManager.instance.Load<Sprite>(Configure.BundleStoryBackground,questItem.bg);
            taskImg.sprite = sprite;
            SetMiddle(taskImg);
        }
        else
        {
            taskImg.enabled = false;
        }
        if(!string.IsNullOrEmpty(questItem.propsImg))
        {
            propImg.enabled = true;

            var sprite = March.Core.ResourceManager.ResourceManager.instance.Load<Sprite>(Configure.BundleStoryBackground, questItem.propsImg);
            GameUtils.ScalingFixedWithHeight(propImg.transform as RectTransform, new Vector2(sprite.texture.width, sprite.texture.height));
            propImg.sprite = sprite;
        }else
        {
            propImg.enabled = false;
        }

        
        taskTitle.text = questItem.sectionName;
        taskDesText.text = questItem.sectionDes;


        
    }

    private void UpdateAbility(qy.config.Ability ability)
    {
        disciplineSlider.SetData(ability.discipline);
        loyaltySlider.SetData(ability.loyalty);
        wisdomSlider.SetData(ability.wisdom);

    }

    private void SetSelectTask()
    {
        buttonsTF.gameObject.SetActive(true);
        actionBtn.gameObject.SetActive(false);
        //taskImg.gameObject.SetActive(false);
        missionTF.anchorMin = new Vector2(0, 0.6f);
        propImg.enabled = false;
        SetMiddle(taskImg);
        selectPool.resetAllTarget();
        List<SelectItem> selects = questItem.selectList;
        foreach(SelectItem item in selects)
        {
            UISelectItem cell = selectPool.getIdleTarget<UISelectItem>();
            cell.SetData(item);
        }
    }

    private void SetMainTask()
    {
        buttonsTF.gameObject.SetActive(false);
        actionBtn.gameObject.SetActive(true);
        missionTF.anchorMin = new Vector2(0,0.25f);
        
        propPool.resetAllTarget();

        bool isComplate = playerdata.ContainsComplateQuest(playerdata.questId);
        List<PropItem> props = new List<PropItem>();
        if (questItem.requireStar > 0)
        {
            props.Add(new PropItem()
            {
                id = "1",
                icon = "XingXing",
                count = questItem.requireStar,
            });
        }

        props.AddRange(questItem.requireItem);
        foreach (PropItem item in props)
        {
            UIPropCell cell = propPool.getIdleTarget<UIPropCell>();
            if (item.id == "1")
            {
                //设置显示星星数量
                cell.SetData(item, playerdata.starNum, isComplate);

            }
            else
            {
                PropItem haveProp = playerdata.GetPropItem(item.id);
                cell.SetData(item, haveProp == null ? 0 : haveProp.count, isComplate);
            }

        }
        
    }

    private void DoTask(string selectedID = "")
    {
        
        string storyID;
        PlayerModelErr err = GameMainManager.Instance.playerModel.QuestComplate(out storyID, selectedID);
        if(err == PlayerModelErr.NULL)
        {
            if(!string.IsNullOrEmpty(storyID)&&storyID!="0")
            {
                if(!string.IsNullOrEmpty(selectedID))
                {
                    StartCoroutine(ShowDialog(storyID));
                }else
                {
                    OnClickClose();
                    GameMainManager.Instance.uiManager.OpenWindow(qy.ui.UISettings.UIWindowID.UIDialogueWindow, storyID);
                }
                
                
            }else
            {
                GameMainManager.Instance.uiManager.OpenWindow(qy.ui.UISettings.UIWindowID.UITaskWindow,playerdata);
                //UpdatePanel();
            }

            UpdateAbility(playerdata.ability);
        }
        else
        {
            OnErrHandle(err);
        }
        
    }

    private IEnumerator ShowDialog(string storyID)
    {
        selectBtnLock = true;
        yield return new WaitForSeconds(1f);
        OnClickClose();
        GameMainManager.Instance.uiManager.OpenWindow(qy.ui.UISettings.UIWindowID.UIDialogueWindow, storyID);
    }

    private void OnErrHandle(PlayerModelErr err)
    {
        OnClickClose();
        if (err == PlayerModelErr.NOT_ENOUGH_PROP)
        {
            //MessageBox.Instance.Show(LanguageManager.instance.GetValueByKey("200010"));
            Messenger.Broadcast(ELocalMsgID.OpenLevelBeginPanel);
            /*
            Alert.Show(LanguageManager.instance.GetValueByKey("200010"), Alert.OK, (btn) => {
                Messenger.Broadcast(ELocalMsgID.OpenLevelBeginPanel);
                OnClickClose();
            });
            */
        }
        else if (err == PlayerModelErr.NOT_ENOUGH_STAR)
        {
            Messenger.Broadcast(ELocalMsgID.OpenLevelBeginPanel);
            /*
            Alert.Show(LanguageManager.instance.GetValueByKey("200011"), Alert.OK, (btn) => {
                Messenger.Broadcast(ELocalMsgID.OpenLevelBeginPanel);
                OnClickClose();
            });
            */
        }
       
    }

    public void OnClickDoBtnHandle()
    {
        if (questItem.type == qy.config.QuestItem.QuestType.Branch)
        {
            PlayerModelErr err = GameMainManager.Instance.playerModel.QuestComplateCondition();
            if(err == PlayerModelErr.NULL)
            {
                SetSelectTask();
            }else
            {
                OnErrHandle(err);
            }
            
        }else
        {
            DoTask();
        }
        
    }

    public void OnClickSelectBtn(UISelectItem btn)
    {
        if(!selectBtnLock)
        {
            DoTask(btn.data.id);
        }
       
    }

    //将图片固定宽度居中
    private void SetMiddle(Image image)
    {
        if(image!=null)
        {
            Sprite sp = image.sprite;
            if(sp!=null)
            {
                Vector2 spSize = new Vector2(sp.texture.width, sp.texture.height);
                Vector2 imgSize = GameUtils.GetSize(image.rectTransform);
                float h = imgSize.x / (spSize.x / spSize.y);
                float scaleH = imgSize.y / h;
                image.material.SetTextureScale("_MainTex", new Vector2(1, scaleH));
                image.material.SetTextureOffset("_MainTex", new Vector2(0, (1 - scaleH) / 2));
            }
            
        }
        
    }
}
