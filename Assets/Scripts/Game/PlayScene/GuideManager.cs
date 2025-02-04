﻿using Assets.Scripts.Common;
using qy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace March.Core.Guide
{
    public class GuideManager : MonoSingleton<GuideManager>
    {
        public bool GuideEnabled
        {
            get
            {
                GuideManagerData.CurrentLevel = LevelLoader.instance.level;
                var isGuideComplete = PlayerPrefs.GetInt(string.Format("{0}_{1}", PlayerPrefEnums.PlayGuideComplete, GuideManagerData.CurrentLevel)) > 0;
                return GuideManagerData.CurrentGuideLevelData != null && !isGuideComplete;
            }
        }

        private const string GuidePrefab = "GuideGenerater";
        private GuideController guideController;
        private GuideCustomInterface guideCustomController;

        private const string RelativePath = "March/Data/Resources/PlayGuide";
        private const string LoadRelativePath = "PlayGuide";
        private const string GuideManagerPath = "GuideManager";
        private string guidePath;

        public GuideLevelManagerData GuideManagerData;

        private bool showing;

        protected override void Init()
        {
            LoadGuideConfig();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void GetRelativePath()
        {
            guidePath = string.Format("{0}/{1}", Application.dataPath, RelativePath);
        }

        [ContextMenu("Generate Guide Config")]
        public void GenerateGuideConfig()
        {
            GetRelativePath();

            if (GuideManagerData.GuideLevelList == null)
                GuideManagerData.GuideLevelList = new List<GuideLevelData>();

            GuideManagerData.GuideLevelList.Clear();
            foreach (var file in Directory.GetFiles(guidePath, "Guide_*_*.json", SearchOption.TopDirectoryOnly))
            {
                var fileInfor = new FileInfo(file);
                var tokens = fileInfor.Name.Replace(fileInfor.Extension, string.Empty)
                    .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                GuideManagerData.GuideLevelList.Add(new GuideLevelData
                {
                    Level = int.Parse(tokens[1]),
                    Step = int.Parse(tokens[2])
                });
            }

            var guideManagerStr = JsonUtility.ToJson(GuideManagerData);
            var path = string.Format("{0}/{1}.json", guidePath, GuideManagerPath);
            File.WriteAllText(path, guideManagerStr);

            Debug.LogWarning("Write data to file: " + path);
        }

        [ContextMenu("Read Guide Config")]
        public void ReadGuideConfig()
        {
            GetRelativePath();

            var path = string.Format("{0}/{1}.json", guidePath, GuideManagerPath);
            var guideManagerStr = File.ReadAllText(path);
            GuideManagerData = JsonUtility.FromJson<GuideLevelManagerData>(guideManagerStr);

            Debug.LogWarning("Read data from file: " + path);
        }

        private void LoadGuideConfig()
        {
            var path = string.Format("{0}/{1}", LoadRelativePath, GuideManagerPath);
            GuideManagerData = JsonUtility.FromJson<GuideLevelManagerData>(Resources.Load<TextAsset>(path).text);
        }

        public void Show()
        {
            if (GuideEnabled && !showing)
            {
                Debug.Log("Current Step:" + GuideManagerData.CurrentStep);

                showing = true;

                var guide = Instantiate(Resources.Load<GameObject>(GuidePrefab), GameObject.Find("Canvas").transform);
                guideController = guide.GetComponent<GuideController>();
                var path = string.Format("{0}/{1}", LoadRelativePath,
                    GuideManagerData.CurrentGuideLevelData.GuideDataPath);
                guideController.GuideText = Instantiate(Resources.Load(path)) as TextAsset;
                guideController.Generate();

                var customControllerName = string.Format("{0}Controller", guideController.name);
                var customControllerType = Type.GetType(string.Format("{0}, Assembly-CSharp", customControllerName));
                if (customControllerType != null)
                    guideCustomController = guideController.gameObject.AddComponent(customControllerType) as GuideCustomInterface;

                guideController.name = string.Format("Level {0} Step {1}", GuideManagerData.CurrentLevel,
                    GuideManagerData.CurrentStep);

                GuideManagerData.CurrentGuideData = guideController.GuideData;

                GameMainManager.Instance.netManager.MakePointInGuide(LevelLoader.instance.level, GuideManagerData.CurrentStep,
                    (ret, res) => { });
            }
        }

        public void Hide()
        {
            if (guideController == null || !guideController.gameObject.activeSelf)
                return;

            StartCoroutine("DoHide");
        }

        private IEnumerator DoHide()
        {
            if (guideCustomController != null)
                yield return guideCustomController.Hide();

            guideController.gameObject.SetActive(false);

            ++GuideManagerData.CurrentStep;

            if (GuideManagerData.IsFinalStep)
            {
                PlayerPrefs.SetInt(string.Format("{0}_{1}", PlayerPrefEnums.PlayGuideComplete, GuideManagerData.CurrentLevel), 1);
            }

            showing = false;
        }

        private void Initialize()
        {
            guideController = null;
            guideCustomController = null;
            showing = false;
            GuideManagerData.CurrentStep = 1;
        }

        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            if (scene.name.Equals("Play"))
            {
                Initialize();
            }
        }
    }
}