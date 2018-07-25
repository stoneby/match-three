using System;

[Serializable]
public class Target
{
    public TARGET_TYPE Type;
    public int Amount;
    public int color;

    public void DataToTargetType(int data)
    {
        switch (data)
        {
            case 1:
                Type = TARGET_TYPE.SCORE;
                break;
            case 2:
                Type = TARGET_TYPE.COOKIE;
                break;
            case 3:
                Type = TARGET_TYPE.MARSHMALLOW;
                break;
            case 4:
                Type = TARGET_TYPE.WAFFLE;
                break;
            case 5:
                Type = TARGET_TYPE.COLLECTIBLE;
                break;
            case 6:
                Type = TARGET_TYPE.COLUMN_ROW_BREAKER;
                break;
            case 7:
                Type = TARGET_TYPE.BOMB_BREAKER;
                break;
            case 8:
                Type = TARGET_TYPE.X_BREAKER;
                break;
            case 9:
                Type = TARGET_TYPE.CAGE;
                break;
            case 10:
                Type = TARGET_TYPE.RAINBOW;
                break;
            case 11:
                Type = TARGET_TYPE.GINGERBREAD;
                break;
            case 12:
                Type = TARGET_TYPE.CHOCOLATE;
                break;
            case 13:
                Type = TARGET_TYPE.ROCK_CANDY;
                break;
            case 14:
                Type = TARGET_TYPE.GRASS;
                break;
            case 15:
                Type = TARGET_TYPE.CHERRY;
                break;
            case 16:
                Type = TARGET_TYPE.PACKAGEBOX;
                break;
            case 17:
                Type = TARGET_TYPE.APPLEBOX;
                break;
            default:
                Type = TARGET_TYPE.NONE;
                break;
        }
    }

    public string ToPrefabStr()
    {
        switch (Type)
        {
            case TARGET_TYPE.NONE:
            case TARGET_TYPE.SCORE:
            case TARGET_TYPE.X_BREAKER:
            case TARGET_TYPE.GINGERBREAD:
            case TARGET_TYPE.WAFFLE:
            case TARGET_TYPE.CHOCOLATE:
                break;

            case TARGET_TYPE.COOKIE:
            case TARGET_TYPE.COLLECTIBLE:
                return string.Format("{0}_{1}", Type.ToString(), color).ToLower();

            case TARGET_TYPE.MARSHMALLOW:
            case TARGET_TYPE.RAINBOW:
            case TARGET_TYPE.APPLEBOX:
            case TARGET_TYPE.CHERRY:
            case TARGET_TYPE.ROCK_CANDY:
            case TARGET_TYPE.BOMB_BREAKER:
            case TARGET_TYPE.COLUMN_ROW_BREAKER:
                return Type.ToString().ToLower();

            case TARGET_TYPE.CAGE:
            case TARGET_TYPE.GRASS:
            case TARGET_TYPE.PACKAGEBOX:
                return Type.ToString().ToLower() + "_1";
        }
        throw new Exception("Not supported type-" + Type);
    }
}

