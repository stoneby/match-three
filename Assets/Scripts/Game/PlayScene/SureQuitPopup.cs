﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using qy;

public class SureQuitPopup : MonoBehaviour
{
    public Text headText;
    public Text QuitBtnText;
    public Text ContinueBtnText;
    public Text showText;
    public GameObject loseTargetLayout;

    private Board board;

    void Start()
    {
		headText.text = LanguageManager.instance.GetValueByKey ("210152");
		QuitBtnText.text = LanguageManager.instance.GetValueByKey ("210154");
		ContinueBtnText.text = LanguageManager.instance.GetValueByKey ("210136");
		showText.text = LanguageManager.instance.GetValueByKey ("210153");

        board = GameObject.Find("Board").GetComponent<Board>();
        if (board != null)
        {
            for (int i = 0; i < board.targetLeftList.Count; i++)
            {
                if (board.targetLeftList[i] > 0)
                {
                    if (loseTargetLayout != null)
                    {
                        GameObject cell = Instantiate(Resources.Load(Configure.UITargetCellPrefab), loseTargetLayout.transform) as GameObject;

                        cell.GetComponent<UITargetCell>().Init(LevelLoader.instance.targetList[i], true);
                    }
                }
            }
        }
    }

    public void onQuitClick()
    {
        //NetManager.instance.eliminateLevelEnd(LevelLoader.instance.level, 0, board.allstep, 0);
        GameMainManager.Instance.playerModel.EndLevel(LevelLoader.instance.level, false, board.allstep, 0);
        SceneManager.LoadScene("main");
    }

    public void onContinueClick()
    {
        if (GameObject.Find("Board"))
        {
            GameObject.Find("Board").GetComponent<Board>().state = GAME_STATE.WAITING_USER_SWAP;
        }

        AudioManager.instance.ButtonClickAudio();
    }
}
