using System.Collections;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    public string Location;

    public bool Loop;
    public float Duration = 0.05f;

    public bool AutoLoad;
    public bool AutoDestroy;

    private SpriteRenderer spriteRenderer;
    private Sprite[] sprites;
    private int frame;

    private bool isPlaying;

    void OnEnable()
    {
        if (AutoLoad)
            Play();
    }

    void OnDisable()
    {
        Reset();
    }

    public void Init()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (sprites == null)
            sprites = Resources.LoadAll<Sprite>(Location);
    }

    public void Reset()
    {
        frame = 0;
        isPlaying = false;
    }

    public void Play()
    {
        Init();

        if (isPlaying)
            return;

        isPlaying = true;
        StartCoroutine("DoPlay");
    }

    IEnumerator DoPlay()
    {
        do
        {
            spriteRenderer.sprite = sprites[frame];

            yield return new WaitForSeconds(Duration / sprites.Length);

            frame++;
            if (Loop)
                frame %= sprites.Length;
            else if (frame >= sprites.Length)
                break;
        } while (true);

        Reset();

        if (AutoDestroy)
            Destroy(gameObject);
    }

    public void Stop()
    {
        Init();

        Reset();

        StopCoroutine("DoPlay");
        spriteRenderer.sprite = sprites[0];
    }
}