using UnityEngine;
using UnityEngine.UI;

public class UITop : MonoBehaviour
{
    public Text levelText;
    public Text movesText;

    int moves;

    void Start()
    {
        levelText.text = LanguageManager.instance.GetValueByKey("200014") + LevelLoader.instance.level;

        moves = LevelLoader.instance.moves;

        if (Configure.instance.beginFiveMoves == true)
        {
            moves += Configure.instance.plusMoves;
        }

        movesText.text = moves.ToString();
    }

    public void DecreaseMoves(bool effect = false)
    {
        if (effect)
            CFX_SpawnSystem.GetNextObject(Configure.EffectRing, movesText.gameObject.transform);

        if (moves > 0)
        {
            moves--;
            movesText.text = moves.ToString();
        }
    }

    public void Set5Moves(int addsteps)
    {
        CFX_SpawnSystem.GetNextObject(Configure.EffectRing, movesText.gameObject.transform);

        moves += addsteps;

        movesText.text = moves.ToString();
    }
}
