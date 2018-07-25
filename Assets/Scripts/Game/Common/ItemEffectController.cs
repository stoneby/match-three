using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemEffectController : MonoBehaviour
{
    public Vector3 IdleRotate = new Vector3(0f, 0f, 50f);
    public Ease IdleEaseType = Ease.OutBack;
    public float IdleDuration = 1f;
    public float IdleCompleteDuration = 0.2f;
    public float IdleLoopInterval = 0.2f;

    private Item item;
    private Dictionary<string, GameObject> effectCache = new Dictionary<string, GameObject>();

    private Sequence idleSequence;
    private string effectKey;

    private void Awake()
    {
        item = GetComponent<Item>();
    }

    private void Start()
    {
        if (item.HasIdleAnimation())
            effectKey = item.ToIdleEffect();

        item.ChangeToSpecialHandler += OnChangeToSpecialHandler;
    }

    private void OnDestroy()
    {
        foreach (var effect in effectCache.Values)
            effect.SetActive(false);
    }

    private void OnChangeToSpecialHandler(object sender, EventArgs e)
    {
        effectKey = item.ToIdleEffect();
    }

    public void PlayIdleAnimation()
    {
        StopIdleAnimation();

        if (item.HasIdleAnimation())
        {
            if (!effectCache.ContainsKey(effectKey))
            {
                effectCache.Add(effectKey, CFX_SpawnSystem.GetNextObject(effectKey, item.transform));
            }
            effectCache[effectKey].transform.position = item.transform.position;
            effectCache[effectKey].SetActive(true);
        }
        else
        {
            idleSequence = DOTween.Sequence();
            idleSequence.Append(item.transform.DORotateQuaternion(Quaternion.Euler(IdleRotate), IdleDuration).SetEase(IdleEaseType));
            idleSequence.Append(item.transform.DORotateQuaternion(Quaternion.Euler(Vector3.zero), IdleCompleteDuration).SetEase(IdleEaseType));
            idleSequence.AppendInterval(IdleLoopInterval);
            idleSequence.SetLoops(-1);
            idleSequence.Play();
        }
    }

    public void StopIdleAnimation()
    {
        if (item.HasIdleAnimation())
        {
            if (effectCache.ContainsKey(effectKey))
            {
                effectCache[effectKey].SetActive(false);
            }
        }
        else
        {
            idleSequence.Kill();
            item.transform.DORotateQuaternion(Quaternion.Euler(Vector3.zero), IdleCompleteDuration).SetEase(IdleEaseType);
        }
    }
}
