﻿using March.Core.Guide;
using March.Scene;
using UnityEngine;
using UnityEngine.UI;

public class GuideWindowController : MonoBehaviour
{
    public GuideWindowData Data;

    public Button NextButton;

    private Text contentText;
    private Text nextButtonText;
    private Transform head;

    private RectTransform rectTransform;

    void Awake()
    {
        Initialize();

        FlushDataToUI();
    }

    void Initialize()
    {
        rectTransform = GetComponent<RectTransform>();

        contentText = transform.Find("Text").GetComponent<Text>();
        nextButtonText = transform.Find("NextButton/Text").GetComponent<Text>();
        NextButton = transform.Find("NextButton").GetComponent<Button>();
        head = transform.Find("Head");

        NextButton.onClick.AddListener(()=>GuideManager.instance.Hide());
    }

    [ContextMenu("UI to Data")]
    public void FlushUIToData()
    {
        Initialize();

        Data.Position = new Position(rectTransform.anchoredPosition);
        Data.HasNextButton = NextButton.gameObject.activeSelf;
        Data.HasHead = head.gameObject.activeSelf;
        Data.Content = contentText.text;
        Data.NextButtonContent = nextButtonText.text;
    }

    [ContextMenu("Data to UI")]
    public void FlushDataToUI()
    {
        Initialize();

        rectTransform.anchoredPosition = new Vector3(Data.Position.X, Data.Position.Y, Data.Position.Z);
        NextButton.gameObject.SetActive(Data.HasNextButton);
        head.gameObject.SetActive(Data.HasHead);
        contentText.text = Data.Content;
        nextButtonText.text = Data.NextButtonContent;
    }
}
