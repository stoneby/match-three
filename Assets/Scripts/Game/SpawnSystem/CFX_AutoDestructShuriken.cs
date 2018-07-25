using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class CFX_AutoDestructShuriken : CFX_AutoDestruct
{
    private ParticleSystem ps;

    protected override void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();
        duration = ps.main.duration;

        base.OnEnable();
    }
}
