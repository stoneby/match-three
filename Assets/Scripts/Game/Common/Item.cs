using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using March.Core.Guide;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class Item : MonoBehaviour
{
    [Header("Parent")]
    public Board board;
    public Node node;

    [Header("Variables")]
    public int color;
    public ITEM_TYPE type;
    public ITEM_TYPE next = ITEM_TYPE.NONE;
    public BREAKER_EFFECT effect = BREAKER_EFFECT.NORMAL;

    [Header("Check")]
    public bool drag;
    public bool nextSound = true;
    public bool destroying;
    public bool dropping;
    public bool changing;

    [SerializeField] private int _beabletodestroy;

    public bool IsMatched;

    public int beAbleToDestroy
    {
        get
        {
            if (type == ITEM_TYPE.APPLEBOX)
            {
                return applebox.beAbleToDestroy;
            }

            return _beabletodestroy;
        }
        set
        {
            if (type == ITEM_TYPE.APPLEBOX)
            {
                applebox.beAbleToDestroy = value;
            }
            else
            {
                _beabletodestroy = value;
            }
        }
    }

    public Vector3 mousePostion = Vector3.zero;
    public Vector3 deltaPosition = Vector3.zero;
    public Vector3 swapDirection = Vector3.zero;

    [Header("Swap")]
    public Node neighborNode;
    public Item swapItem;

    [Header("Drop")]
    public List<Vector3> dropPath;

    public AppleBox applebox;

    [Header("Configuration")]
    public Vector3 CookieScale = new Vector3(1.8f, 1.8f, 1.8f);

    public enum DestroyBy
    {
        Cookie,
        Booster,
    }

    public DestroyBy DestroyByType;

    private SpriteRenderer spriteRenderer;
    private ItemEffectController effectController;
    private ITEM_TYPE planeTargetType = ITEM_TYPE.NONE;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = SortingLayers.Item;
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
    }

    private void Start()
    {
        effectController = GetComponent<ItemEffectController>();
    }

    void Update()
    {
        if (drag)
        {
            deltaPosition = mousePostion - GetMousePosition();
            if (swapDirection == Vector3.zero)
            {
                SwapDirection(deltaPosition);
            }
        }
    }

    public bool HasIdleAnimation()
    {
        return type == ITEM_TYPE.BOMB_BREAKER || type == ITEM_TYPE.COLUMN_BREAKER || type == ITEM_TYPE.ROW_BREAKER ||
               type == ITEM_TYPE.PLANE_BREAKER || type == ITEM_TYPE.RAINBOW;
    }

    public bool HasGenerateEffect()
    {
        return type == ITEM_TYPE.BOMB_BREAKER || type == ITEM_TYPE.COLUMN_BREAKER || type == ITEM_TYPE.ROW_BREAKER ||
               type == ITEM_TYPE.PLANE_BREAKER || type == ITEM_TYPE.RAINBOW;
    }

    #region Type

    public bool Movable()
    {
        if (
            type == ITEM_TYPE.CHOCOLATE_1_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_2_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_3_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_4_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_5_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_6_LAYER ||
            type == ITEM_TYPE.ROCK_CANDY ||
            type == ITEM_TYPE.APPLEBOX ||
            type == ITEM_TYPE.BLANK
        )
        {
            return false;
        }

        if (!node.IsInCurrentPage())
        {
            return false;
        }

        // cage
        if (node.cage != null)
        {
            if (node.cage.type == CAGE_TYPE.CAGE_1 || node.cage.type == CAGE_TYPE.CAGE_2)
            {
                return false;
            }
        }

        if (node.jelly != null)
        {
            if (node.jelly.type == JELLY_TYPE.JELLY_1 || node.jelly.type == JELLY_TYPE.JELLY_2 ||
                node.jelly.type == JELLY_TYPE.JELLY_3)
            {
                return false;
            }
        }

        if (node.packagebox != null)
        {
            if (node.packagebox.type != PACKAGEBOX_TYPE.NONE)
            {
                return false;
            }
        }

        return true;
    }

    public bool HasNodeLayer()
    {
        // cage
        if (node.cage != null)
        {
            if (node.cage.type == CAGE_TYPE.CAGE_1 || node.cage.type == CAGE_TYPE.CAGE_2)
            {
                return true;
            }
        }

        if (node.jelly != null)
        {
            if (node.jelly.type == JELLY_TYPE.JELLY_1 || node.jelly.type == JELLY_TYPE.JELLY_2 ||
                node.jelly.type == JELLY_TYPE.JELLY_3)
            {
                return true;
            }
        }

        if (node.packagebox != null)
        {
            if (node.packagebox.type != PACKAGEBOX_TYPE.NONE)
            {
                return true;
            }
        }

        return false;
    }

    public bool Droppable()
    {
        if (
            type == ITEM_TYPE.CHOCOLATE_1_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_2_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_3_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_4_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_5_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_6_LAYER ||
            type == ITEM_TYPE.ROCK_CANDY ||
            type == ITEM_TYPE.APPLEBOX ||
            type == ITEM_TYPE.BLANK
        )
        {
            return false;
        }

        if (node.m_bLockDropAndThrough)
        {
            return false;
        }

        // cage
        if (node.cage != null)
        {
            if (node.cage.type == CAGE_TYPE.CAGE_1 || node.cage.type == CAGE_TYPE.CAGE_2)
            {
                return false;
            }
        }

        if (node.jelly != null)
        {
            if (node.jelly.type == JELLY_TYPE.JELLY_1 || node.jelly.type == JELLY_TYPE.JELLY_2 ||
                node.jelly.type == JELLY_TYPE.JELLY_3)
            {
                return false;
            }
        }

        if (node.packagebox != null)
        {
            if (node.packagebox.type != PACKAGEBOX_TYPE.NONE)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 检查是否可交换 0 不检查方向 1上 2右 3下 4左
    /// </summary>
    /// <param name="derection"></param>
    /// <returns></returns>
    public bool Exchangeable(SWAP_DIRECTION direction)
    {
        if (
            type == ITEM_TYPE.CHOCOLATE_1_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_2_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_3_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_4_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_5_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_6_LAYER ||
            type == ITEM_TYPE.APPLEBOX ||
            type == ITEM_TYPE.ROCK_CANDY
        )
        {
            return false;
        }

        if (!node.IsInCurrentPage())
        {
            return false;
        }

        // cage
        if (node.cage != null)
        {
            if (node.cage.type == CAGE_TYPE.CAGE_1 || node.cage.type == CAGE_TYPE.CAGE_2)
            {
                return false;
            }
        }

        if (node.jelly != null)
        {
            if (node.jelly.type == JELLY_TYPE.JELLY_1 || node.jelly.type == JELLY_TYPE.JELLY_2 ||
                node.jelly.type == JELLY_TYPE.JELLY_3)
            {
                return false;
            }
        }

        if (node.ice != null)
        {
            if (node.ice.type == ICE_TYPE.ICE_1 || node.ice.type == ICE_TYPE.ICE_2)
            {
                return false;
            }
        }

        if (node.packagebox != null)
        {
            if (node.packagebox.type != PACKAGEBOX_TYPE.NONE)
            {
                return false;
            }
        }

        if (direction == SWAP_DIRECTION.NONE)
        {

        }
        else if (direction == SWAP_DIRECTION.TOP)
        {
            if (node.TopNeighbor() != null && node.TopNeighbor().bafflebottom != null)
            {
                return false;
            }
        }
        else if (direction == SWAP_DIRECTION.RIGHT)
        {
            if (node.baffleright != null)
            {
                return false;
            }
        }
        else if (direction == SWAP_DIRECTION.BOTTOM)
        {
            if (node.bafflebottom != null)
            {
                return false;
            }
        }
        else if (direction == SWAP_DIRECTION.LEFT)
        {
            if (node.LeftNeighbor() != null && node.LeftNeighbor().baffleright != null)
            {
                return false;
            }
        }
        else
        {
            Debug.Log("方向参数错误");
        }

        return true;
    }


    public bool Matchable()
    {
        if (type == ITEM_TYPE.BLANK ||
            type == ITEM_TYPE.CHOCOLATE_1_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_2_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_3_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_4_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_5_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_6_LAYER ||
            type == ITEM_TYPE.ROCK_CANDY ||
            type == ITEM_TYPE.MARSHMALLOW ||
            type == ITEM_TYPE.RAINBOW ||

            type == ITEM_TYPE.COLUMN_BREAKER ||
            type == ITEM_TYPE.ROW_BREAKER ||
            type == ITEM_TYPE.BOMB_BREAKER ||
            type == ITEM_TYPE.PLANE_BREAKER ||

            type == ITEM_TYPE.COLLECTIBLE_1 ||
            type == ITEM_TYPE.COLLECTIBLE_2 ||
            type == ITEM_TYPE.COLLECTIBLE_3 ||
            type == ITEM_TYPE.COLLECTIBLE_4 ||
            type == ITEM_TYPE.COLLECTIBLE_5 ||
            type == ITEM_TYPE.COLLECTIBLE_6 ||
            type == ITEM_TYPE.COLLECTIBLE_7 ||
            type == ITEM_TYPE.COLLECTIBLE_8 ||
            type == ITEM_TYPE.COLLECTIBLE_9 ||

            type == ITEM_TYPE.APPLEBOX

        )
        {
            return false;
        }

        if (!node.IsInCurrentPage())
        {
            return false;
        }

        if (node.jelly != null)
        {
            if (node.jelly.type == JELLY_TYPE.JELLY_1 || node.jelly.type == JELLY_TYPE.JELLY_2 ||
                node.jelly.type == JELLY_TYPE.JELLY_3)
            {
                return false;
            }
        }

        if (node.packagebox != null)
        {
            if (node.packagebox.type != PACKAGEBOX_TYPE.NONE)
            {
                return false;
            }
        }

        return true;
    }

    public bool Destroyable()
    {
        if (type == ITEM_TYPE.COLLECTIBLE_1 ||
            type == ITEM_TYPE.COLLECTIBLE_2 ||
            type == ITEM_TYPE.COLLECTIBLE_3 ||
            type == ITEM_TYPE.COLLECTIBLE_4 ||
            type == ITEM_TYPE.COLLECTIBLE_5 ||
            type == ITEM_TYPE.COLLECTIBLE_6 ||
            type == ITEM_TYPE.COLLECTIBLE_7 ||
            type == ITEM_TYPE.COLLECTIBLE_8 ||
            type == ITEM_TYPE.COLLECTIBLE_9 ||
            type == ITEM_TYPE.COLLECTIBLE_10 ||
            type == ITEM_TYPE.COLLECTIBLE_11 ||
            type == ITEM_TYPE.COLLECTIBLE_12 ||
            type == ITEM_TYPE.COLLECTIBLE_13 ||
            type == ITEM_TYPE.COLLECTIBLE_14 ||
            type == ITEM_TYPE.COLLECTIBLE_15 ||
            type == ITEM_TYPE.COLLECTIBLE_16 ||
            type == ITEM_TYPE.COLLECTIBLE_17 ||
            type == ITEM_TYPE.COLLECTIBLE_18 ||
            type == ITEM_TYPE.COLLECTIBLE_19 ||
            type == ITEM_TYPE.COLLECTIBLE_20)
        {
            return false;
        }

        if (!node.IsInCurrentPage())
        {
            return false;
        }

        return true;
    }

    public bool CanChangeToBubble()
    {
        if (Movable()
            && (IsCookie() || IsBreaker(type) || type == ITEM_TYPE.RAINBOW)
        )
        {
            return true;
        }
        return false;
    }

    public bool DoesMatchFlyingTarget(Target target)
    {
        if (IsCookie() && !HasNodeLayer())
            return target.Type == TARGET_TYPE.COOKIE && color == target.color;

        if (IsCollectible() && !HasNodeLayer())
            return target.Type == TARGET_TYPE.COLLECTIBLE;

        if (IsMarshmallow() && !HasNodeLayer())
            return target.Type == TARGET_TYPE.MARSHMALLOW;

        if (IsRainbow() && !HasNodeLayer())
            return target.Type == TARGET_TYPE.RAINBOW;

        if (IsBombBreaker(type) && !HasNodeLayer())
            return target.Type == TARGET_TYPE.BOMB_BREAKER;

        if ((IsColumnBreaker(type) || IsRowBreaker(type)) && !HasNodeLayer())
            return target.Type == TARGET_TYPE.COLUMN_ROW_BREAKER;

        if (applebox != null && applebox.appleNum > 0 && IsAppleBox())
            return target.Type == TARGET_TYPE.APPLEBOX;

        if (IsCherry())
            return target.Type == TARGET_TYPE.CHERRY;

        if (IsRockCandy())
            return target.Type == TARGET_TYPE.ROCK_CANDY;

        if (node.packagebox != null)
            return target.Type == TARGET_TYPE.PACKAGEBOX;

        if (node.cage != null)
            return target.Type == TARGET_TYPE.CAGE;

        // not supported yet.
        return false;
    }

    public bool IsCookie()
    {
        if (type == ITEM_TYPE.COOKIE_1 ||
            type == ITEM_TYPE.COOKIE_2 ||
            type == ITEM_TYPE.COOKIE_3 ||
            type == ITEM_TYPE.COOKIE_4 ||
            type == ITEM_TYPE.COOKIE_5 ||
            type == ITEM_TYPE.COOKIE_6)
        {
            return true;
        }

        return false;
    }

    public bool IsBlank()
    {
        return (type == ITEM_TYPE.BLANK);
    }

    public bool IsRainbow()
    {
        return type == ITEM_TYPE.RAINBOW;
    }

    public bool IsCollectible()
    {
        if (type == ITEM_TYPE.COLLECTIBLE_1 ||
            type == ITEM_TYPE.COLLECTIBLE_2 ||
            type == ITEM_TYPE.COLLECTIBLE_3 ||
            type == ITEM_TYPE.COLLECTIBLE_4 ||
            type == ITEM_TYPE.COLLECTIBLE_5 ||
            type == ITEM_TYPE.COLLECTIBLE_6 ||
            type == ITEM_TYPE.COLLECTIBLE_7 ||
            type == ITEM_TYPE.COLLECTIBLE_8 ||
            type == ITEM_TYPE.COLLECTIBLE_9 ||
            type == ITEM_TYPE.COLLECTIBLE_10 ||
            type == ITEM_TYPE.COLLECTIBLE_11 ||
            type == ITEM_TYPE.COLLECTIBLE_12 ||
            type == ITEM_TYPE.COLLECTIBLE_13 ||
            type == ITEM_TYPE.COLLECTIBLE_14 ||
            type == ITEM_TYPE.COLLECTIBLE_15 ||
            type == ITEM_TYPE.COLLECTIBLE_16 ||
            type == ITEM_TYPE.COLLECTIBLE_17 ||
            type == ITEM_TYPE.COLLECTIBLE_18 ||
            type == ITEM_TYPE.COLLECTIBLE_19 ||
            type == ITEM_TYPE.COLLECTIBLE_20)
        {
            return true;
        }

        return false;
    }

    public bool IsGingerbread()
    {
        if (type == ITEM_TYPE.GINGERBREAD_1 ||
            type == ITEM_TYPE.GINGERBREAD_2 ||
            type == ITEM_TYPE.GINGERBREAD_3 ||
            type == ITEM_TYPE.GINGERBREAD_4 ||
            type == ITEM_TYPE.GINGERBREAD_5 ||
            type == ITEM_TYPE.GINGERBREAD_6)
        {
            return true;
        }

        return false;
    }

    public bool IsMarshmallow()
    {
        if (type == ITEM_TYPE.MARSHMALLOW)
        {
            return true;
        }

        return false;
    }

    public bool IsAppleBox()
    {
        if (type == ITEM_TYPE.APPLEBOX)
        {
            return true;
        }

        return false;
    }

    public bool IsChocolate()
    {
        if (type == ITEM_TYPE.CHOCOLATE_1_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_2_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_3_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_4_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_5_LAYER ||
            type == ITEM_TYPE.CHOCOLATE_6_LAYER)
        {
            return true;
        }

        return false;
    }

    public bool IsRockCandy()
    {
        if (type == ITEM_TYPE.ROCK_CANDY)
        {
            return true;
        }

        return false;
    }

    public bool IsCherry()
    {
        if (type == ITEM_TYPE.CHERRY)
        {
            return true;
        }

        return false;
    }

    public ITEM_TYPE OriginCookieType()
    {
        var order = board.NodeOrder(node.i, node.j);

        return LevelLoader.instance.itemLayerData[order];
    }

    // check if breaker is row or column
    ITEM_TYPE GetColRowBreaker(ITEM_TYPE check, Vector3 direction)
    {
        //print("direction: " + direction);

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            //print("row");

            switch (check)
            {
                case ITEM_TYPE.COOKIE_1:
                case ITEM_TYPE.COOKIE_2:
                case ITEM_TYPE.COOKIE_3:
                case ITEM_TYPE.COOKIE_4:
                case ITEM_TYPE.COOKIE_5:
                case ITEM_TYPE.COOKIE_6:
                    return ITEM_TYPE.ROW_BREAKER;
                default:
                    return ITEM_TYPE.NONE;
            }
        }
        else
        {
            //print("colmn");

            switch (check)
            {
                case ITEM_TYPE.COOKIE_1:
                case ITEM_TYPE.COOKIE_2:
                case ITEM_TYPE.COOKIE_3:
                case ITEM_TYPE.COOKIE_4:
                case ITEM_TYPE.COOKIE_5:
                case ITEM_TYPE.COOKIE_6:
                    return ITEM_TYPE.COLUMN_BREAKER;
                default:
                    return ITEM_TYPE.NONE;
            }
        }
    }

    public bool IsBombBreaker(ITEM_TYPE check)
    {
        if (check == ITEM_TYPE.BOMB_BREAKER
        )
        {
            return true;
        }

        return false;
    }


    public bool IsColumnBreaker(ITEM_TYPE check)
    {
        if (check == ITEM_TYPE.COLUMN_BREAKER
        )
        {
            return true;
        }

        return false;
    }

    public bool IsRowBreaker(ITEM_TYPE check)
    {
        if (check == ITEM_TYPE.ROW_BREAKER
        )
        {
            return true;
        }

        return false;
    }

    public bool IsPlaneBreaker(ITEM_TYPE check)
    {
        if (check == ITEM_TYPE.PLANE_BREAKER
        )
        {
            return true;
        }

        return false;
    }

    public bool IsBreaker(ITEM_TYPE check)
    {
        if (IsBombBreaker(check) || IsColumnBreaker(check) || IsRowBreaker(check) || IsPlaneBreaker(check))
        {
            return true;
        }

        return false;
    }

    public ITEM_TYPE GetBombBreaker(ITEM_TYPE check)
    {
        switch (check)
        {
            case ITEM_TYPE.COOKIE_1:
            case ITEM_TYPE.COOKIE_2:
            case ITEM_TYPE.COOKIE_3:
            case ITEM_TYPE.COOKIE_4:
            case ITEM_TYPE.COOKIE_5:
            case ITEM_TYPE.COOKIE_6:
                return ITEM_TYPE.BOMB_BREAKER;
            default:
                return ITEM_TYPE.NONE;
        }
    }

    public ITEM_TYPE GetColumnBreaker(ITEM_TYPE check)
    {
        switch (check)
        {
            case ITEM_TYPE.COOKIE_1:
            case ITEM_TYPE.COOKIE_2:
            case ITEM_TYPE.COOKIE_3:
            case ITEM_TYPE.COOKIE_4:
            case ITEM_TYPE.COOKIE_5:
            case ITEM_TYPE.COOKIE_6:
                return ITEM_TYPE.COLUMN_BREAKER;
            default:
                return ITEM_TYPE.NONE;
        }
    }

    public ITEM_TYPE GetRowBreaker(ITEM_TYPE check)
    {
        switch (check)
        {
            case ITEM_TYPE.COOKIE_1:
            case ITEM_TYPE.COOKIE_2:
            case ITEM_TYPE.COOKIE_3:
            case ITEM_TYPE.COOKIE_4:
            case ITEM_TYPE.COOKIE_5:
            case ITEM_TYPE.COOKIE_6:
                return ITEM_TYPE.ROW_BREAKER;
            default:
                return ITEM_TYPE.NONE;
        }
    }

    public ITEM_TYPE GetPlaneBreaker(ITEM_TYPE check)
    {
        switch (check)
        {
            case ITEM_TYPE.COOKIE_1:
            case ITEM_TYPE.COOKIE_2:
            case ITEM_TYPE.COOKIE_3:
            case ITEM_TYPE.COOKIE_4:
            case ITEM_TYPE.COOKIE_5:
            case ITEM_TYPE.COOKIE_6:
                return ITEM_TYPE.PLANE_BREAKER;
            default:
                return ITEM_TYPE.NONE;
        }
    }

    public ITEM_TYPE GetCookie(ITEM_TYPE check)
    {
        switch (check)
        {
            case ITEM_TYPE.COOKIE_1:
                return ITEM_TYPE.COOKIE_1;
            case ITEM_TYPE.COOKIE_2:
                return ITEM_TYPE.COOKIE_2;
            case ITEM_TYPE.COOKIE_3:
                return ITEM_TYPE.COOKIE_3;
            case ITEM_TYPE.COOKIE_4:
                return ITEM_TYPE.COOKIE_4;
            case ITEM_TYPE.COOKIE_5:
                return ITEM_TYPE.COOKIE_5;
            case ITEM_TYPE.COOKIE_6:
                return ITEM_TYPE.COOKIE_6;
            default:
                return ITEM_TYPE.NONE;
        }
    }

    #endregion

    #region Swap

    // helper function to know the direction of swap
    public Vector3 GetMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    // calculate the direction
    void SwapDirection(Vector3 delta)
    {
        deltaPosition = delta;

        var direction = SWAP_DIRECTION.NONE;

        if (Vector3.Magnitude(deltaPosition) > 0.85f)
        {
            if (Mathf.Abs(deltaPosition.x) > Mathf.Abs(deltaPosition.y) && deltaPosition.x > 0) swapDirection.x = 1;
            else if (Mathf.Abs(deltaPosition.x) > Mathf.Abs(deltaPosition.y) && deltaPosition.x < 0)
                swapDirection.x = -1;
            else if (Mathf.Abs(deltaPosition.x) < Mathf.Abs(deltaPosition.y) && deltaPosition.y > 0)
                swapDirection.y = 1;
            else if (Mathf.Abs(deltaPosition.x) < Mathf.Abs(deltaPosition.y) && deltaPosition.y < 0)
                swapDirection.y = -1;

            bool hasbaffle = false;

            if (swapDirection.x > 0)
            {
                //Debug.Log("Left");
                direction = SWAP_DIRECTION.LEFT;

                if (node != null)
                {
                    if (node.LeftNeighbor() != null)
                    {
                        if (node.LeftNeighbor().item != null)
                        {
                            if (node.LeftNeighbor().item.Exchangeable(SWAP_DIRECTION.RIGHT))
                            {
                                neighborNode = node.LeftNeighbor();
                                if (neighborNode.baffleright != null)
                                {
                                    hasbaffle = true;
                                }
                            }
                        }
                    }
                }
            }
            else if (swapDirection.x < 0)
            {
                //Debug.Log("Right");
                direction = SWAP_DIRECTION.RIGHT;

                if (node != null)
                {
                    if (node.RightNeighbor() != null)
                    {
                        if (node.RightNeighbor().item != null)
                        {
                            if (node.RightNeighbor().item.Exchangeable(SWAP_DIRECTION.LEFT))
                            {
                                neighborNode = node.RightNeighbor();
                                if (node.baffleright != null)
                                {
                                    hasbaffle = true;
                                }
                            }
                        }
                    }
                }
            }
            else if (swapDirection.y > 0)
            {
                //Debug.Log("Bottom");
                direction = SWAP_DIRECTION.BOTTOM;

                if (node != null)
                {
                    if (node.BottomNeighbor() != null)
                    {
                        if (node.BottomNeighbor().item != null)
                        {
                            if (node.BottomNeighbor().item.Exchangeable(SWAP_DIRECTION.TOP))
                            {
                                neighborNode = node.BottomNeighbor();
                                if (node.bafflebottom != null)
                                {
                                    hasbaffle = true;
                                }
                            }
                        }
                    }
                }
            }
            else if (swapDirection.y < 0)
            {
                //Debug.Log("Top");
                direction = SWAP_DIRECTION.TOP;

                if (node != null)
                {
                    if (node.TopNeighbor() != null)
                    {
                        if (node.TopNeighbor().item != null)
                        {
                            if (node.TopNeighbor().item.Exchangeable(SWAP_DIRECTION.BOTTOM))
                            {
                                neighborNode = node.TopNeighbor();
                                if (neighborNode.bafflebottom != null)
                                {
                                    hasbaffle = true;
                                }
                            }
                        }
                    }
                }
            }

            if (neighborNode != null && neighborNode.item != null && CheckHelpSwapable(direction) && !hasbaffle)
            {
                swapItem = neighborNode.item;

                board.touchedItem = this;
                board.swappedItem = swapItem;

                board.CancelChoseItem();
                Swap();
            }
            else
            {
                // if no neighbor item we need to reset to be able to swap again
                Reset();
            }
        }
    }

    // swap animation
    public void Swap(bool forced = false)
    {
        if (swapDirection != Vector3.zero && neighborNode != null)
        {
            CookieGeneralEffect();
            swapItem.CookieGeneralEffect();

            iTween.MoveTo(gameObject, iTween.Hash(
                "position", swapItem.transform.position,
                "onstart", "OnStartSwap",
                "oncomplete", "OnCompleteSwap",
                "oncompleteparams", new Hashtable() { { "forced", forced } },
                "easetype", iTween.EaseType.linear,
                "time", Configure.instance.swapTime
            ));

            iTween.MoveTo(swapItem.gameObject, iTween.Hash(
                "position", transform.position,
                "easetype", iTween.EaseType.linear,
                "time", Configure.instance.swapTime
            ));
        }
    }

    public void OnStartSwap()
    {
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;

        AudioManager.instance.SwapAudio();

        board.lockSwap = true;

        if (HasIdleAnimation() || swapItem.HasIdleAnimation())
            board.HideHint();

        board.dropTime = 1;

        // hide help if need
        GuideManager.instance.Hide();
    }

    public void OnCompleteSwap(Hashtable args)
    {
        var forced = (bool)args["forced"];

        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 0;

        SwapItem();

        // after swap this.node = neighbor
        var matchesHere = (node.FindMatches() != null) ? node.FindMatches().Count : 0;
        var matchesAtNeighbor = (swapItem.node.FindMatches() != null) ? swapItem.node.FindMatches().Count : 0;
        var squareMatchesHere = (node.FindSquareMatches() != null) ? node.FindSquareMatches().Count : 0;
        var squareMatchesAtNeighbor =
            (swapItem.node.FindSquareMatches() != null) ? swapItem.node.FindSquareMatches().Count : 0;

        var special =
            type == ITEM_TYPE.RAINBOW && (swapItem.IsCookie() || IsBreaker(swapItem.type) ||
                                          swapItem.type == ITEM_TYPE.RAINBOW) ||
            swapItem.type == ITEM_TYPE.RAINBOW &&
            (IsCookie() || IsBreaker(type) || type == ITEM_TYPE.RAINBOW) ||
            IsBreaker(type) && (swapItem.IsCookie() || IsBreaker(swapItem.type) ||
                                swapItem.type == ITEM_TYPE.RAINBOW || swapItem.IsMarshmallow() ||
                                swapItem.IsCollectible() || swapItem.IsBlank()) || IsBreaker(swapItem.type) &&
            (IsCookie() || IsBreaker(type) || type == ITEM_TYPE.RAINBOW || IsMarshmallow() || IsCollectible());

        if (matchesHere <= 0 && matchesAtNeighbor <= 0 && squareMatchesHere <= 0 && squareMatchesAtNeighbor <= 0 &&
            special == false && Configure.instance.checkSwap && forced == false)
        {
            // swap back
            iTween.MoveTo(gameObject, iTween.Hash(
                "position", swapItem.transform.position,
                "onstart", "OnStartSwapBack",
                "oncomplete", "OnCompleteSwapBack",
                "easetype", iTween.EaseType.linear,
                "time", Configure.instance.swapTime
            ));

            iTween.MoveTo(swapItem.gameObject, iTween.Hash(
                "position", transform.position,
                "easetype", iTween.EaseType.linear,
                "time", Configure.instance.swapTime
            ));
        }
        else
        {
            board.HideHint();

            //泡沫增长
            board.needIncreaseBubble = true;

            // do not reduce move when forced swap
            if (forced == false)
            {
                board.DecreaseMoveLeft();
            }

            if (special)
            {
                //如果交换前的位置有草地 特殊块会使交换后的地块变为草地
                if ((IsBreaker(type) || type == ITEM_TYPE.RAINBOW) && swapItem.node.IsGrass())
                {
                    node.ChangeToGrass();
                }

                if ((swapItem.IsBreaker(swapItem.type) || swapItem.type == ITEM_TYPE.RAINBOW) && node.IsGrass())
                {
                    swapItem.node.ChangeToGrass();
                }

                board.InitPlaneRandom();

                RainbowDestroy(this, swapItem);
                PlaneSpecialDestroy(this, swapItem);
                TwoColRowBreakerDestroy(this, swapItem);
                TwoBombBreakerDestroy(this, swapItem);
                ColRowBreakerAndBombBreakerDestroy(this, swapItem);

                if ((swapItem.IsCookie() || swapItem.IsMarshmallow() || swapItem.IsCollectible() ||
                     swapItem.IsBlank()) &&
                    (type == ITEM_TYPE.ROW_BREAKER
                     || type == ITEM_TYPE.COLUMN_BREAKER
                     || type == ITEM_TYPE.BOMB_BREAKER
                    )
                )
                {
                    Destroy();
                    beAbleToDestroy--;
                    board.FindMatches();
                }

                if ((IsCookie() || IsMarshmallow() || IsCollectible()) &&
                    (swapItem.type == ITEM_TYPE.ROW_BREAKER
                     || swapItem.type == ITEM_TYPE.COLUMN_BREAKER
                     || swapItem.type == ITEM_TYPE.BOMB_BREAKER
                    )
                )
                {
                    swapItem.Destroy();
                    board.FindMatches();
                }
            }
            else
            {
                //todo:根据配置决定优先级
                //飞机生成的优先级最低在最上面
                if (squareMatchesHere == 4)
                {
                    next = ITEM_TYPE.PLANE_BREAKER;
                }

                if (squareMatchesAtNeighbor == 4)
                {
                    swapItem.next = ITEM_TYPE.PLANE_BREAKER;
                }

                if (matchesHere == 4)
                {
                    next = GetColRowBreaker(this.type, transform.position - swapItem.transform.position);
                }
                else if (matchesHere >= 5)
                {
                    next = ITEM_TYPE.RAINBOW;
                }

                if (matchesAtNeighbor == 4)
                {
                    swapItem.next = GetColRowBreaker(swapItem.type, transform.position - swapItem.transform.position);
                }
                else if (matchesAtNeighbor >= 5)
                {
                    swapItem.next = ITEM_TYPE.RAINBOW;
                }

                // find the matches to destroy (destroy match 3/4/5)
                // this function will not destroy special match such as rainbow swap with breaker etc.
                board.FindMatches();
            }

            // we reset here because the item will be destroy soon (the board is still lock)
            Reset();
        }
    }

    public void OnStartSwapBack()
    {
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;

        AudioManager.instance.SwapBackAudio();
    }

    public void OnCompleteSwapBack()
    {
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 0;

        SwapItemBack();

        // fix swap back wrong position cause by iTween
        transform.position = board.NodeLocalPosition(node.i, node.j) + board.transform.position;

        Reset();

        board.lockSwap = false;
    }

    public void SwapItem()
    {
        Node thisNode = node;

        thisNode.item = swapItem;
        neighborNode.item = this;

        this.node = neighborNode;
        swapItem.node = thisNode;

        this.gameObject.transform.SetParent(neighborNode.gameObject.transform);
        swapItem.gameObject.transform.SetParent(thisNode.gameObject.transform);
    }

    void SwapItemBack()
    {
        Node swapNode = swapItem.node;

        this.node.item = swapItem;
        swapNode.item = this;

        this.node = swapItem.node;
        swapItem.node = neighborNode;

        this.gameObject.transform.SetParent(swapNode.gameObject.transform);
        swapItem.gameObject.transform.SetParent(neighborNode.gameObject.transform);
    }

    public void ShowHint()
    {
        effectController.PlayIdleAnimation();
    }

    public void HideHint()
    {
        effectController.StopIdleAnimation();
    }

    // reset info after a swap
    public void Reset()
    {
        drag = false;

        swapDirection = Vector3.zero;

        neighborNode = null;

        swapItem = null;

        board.CancelChoseItem();
    }

    public bool CheckHelpSwapable(SWAP_DIRECTION direction)
    {
        if (GuideManager.instance.GuideEnabled)
        {
            var currentGuideData = GuideManager.instance.GuideManagerData.CurrentGuideData;
            var result = currentGuideData.ConditionList.Any(condition =>
                node.OrderOnBoard() == condition.NodeIndex && direction == condition.Direction);
            return result;
        }

        return true;
    }

    #endregion

    #region ColorAndAppear

    // after the board is generate we need to alter the color to make sure there is no "pre-matches" on the board
    public void GenerateColor(int except)
    {
        var colors = new List<int>();

        var usingColors = LevelLoader.instance.usingColors;

        for (int i = 0; i < usingColors.Count; i++)
        {
            int color = usingColors[i];

            bool generatable = true;
            Node neighbor = null;

            neighbor = node.TopNeighbor();
            if (neighbor != null)
            {
                if (neighbor.item != null)
                {
                    if (neighbor.item.color == color)
                    {
                        generatable = false;
                    }
                }
            }

            neighbor = node.LeftNeighbor();
            if (neighbor != null)
            {
                if (neighbor.item != null)
                {
                    if (neighbor.item.color == color)
                    {
                        generatable = false;
                    }
                }
            }

            neighbor = node.RightNeighbor();
            if (neighbor != null)
            {
                if (neighbor.item != null)
                {
                    if (neighbor.item.color == color)
                    {
                        generatable = false;
                    }
                }
            }

            if (generatable && color != except)
            {
                colors.Add(color);
            }
        } // end for

        // by default index is a random color
        int index = usingColors[Random.Range(0, usingColors.Count)];

        // if there is generatable colors then change index
        if (colors.Count > 0)
        {
            index = colors[Random.Range(0, colors.Count)];
        }

        // if the random in colors list is a except color then change the index
        if (index == except)
        {
            index = (index++) % usingColors.Count;
        }

        this.color = index;

        ChangeSpriteAndType(index);
    }

    public static float ScaleOutDuration = 0.3f;
    public static float ScaleInDuration = 0.3f;
    public static float ScaleStrength = 1.2f;

    public void ChangeSpriteAndType(int itemColor)
    {
        if (itemColor - 1 < 0 || itemColor - 1 >= board.CookiesList.Count)
            throw new Exception("Exception out of range with itemColor: " + itemColor + ", in range 1 to " + board.CookiesList.Count);

        spriteRenderer.sprite = board.CookiesList[itemColor - 1].GetComponent<SpriteRenderer>().sprite;
    }

    public void ChangeSprite(int itemColor)
    {
        var sq = DOTween.Sequence();
        sq.Append(transform.DOScale(Vector3.zero, ScaleOutDuration).SetEase(Ease.InBack)).
            AppendCallback(() => spriteRenderer.sprite = board.CookiesList[itemColor - 1].GetComponent<SpriteRenderer>().sprite).
            Append(transform.DOScale(CookieScale, ScaleInDuration).SetEase(Ease.OutBack));
        sq.Play();
    }

    public void ChangeToGingerbread(ITEM_TYPE check)
    {
        if (node.item.IsGingerbread())
            return;

        var upper = board.GetUpperItem(node);
        if (upper != null && upper.IsGingerbread())
            return;

        AudioManager.instance.GingerbreadAudio();

        CFX_SpawnSystem.GetNextObject(Configure.EffectGingerbreadDestroy, node.item.transform);

        GameObject prefab = null;
        switch (check)
        {
            case ITEM_TYPE.GINGERBREAD_1:
                prefab = Resources.Load(Configure.Gingerbread1()) as GameObject;
                check = ITEM_TYPE.GINGERBREAD_1;
                color = 1;
                break;
            case ITEM_TYPE.GINGERBREAD_2:
                prefab = Resources.Load(Configure.Gingerbread2()) as GameObject;
                check = ITEM_TYPE.GINGERBREAD_2;
                color = 2;
                break;
            case ITEM_TYPE.GINGERBREAD_3:
                prefab = Resources.Load(Configure.Gingerbread3()) as GameObject;
                check = ITEM_TYPE.GINGERBREAD_3;
                color = 3;
                break;
            case ITEM_TYPE.GINGERBREAD_4:
                prefab = Resources.Load(Configure.Gingerbread4()) as GameObject;
                check = ITEM_TYPE.GINGERBREAD_4;
                color = 4;
                break;
            case ITEM_TYPE.GINGERBREAD_5:
                prefab = Resources.Load(Configure.Gingerbread5()) as GameObject;
                check = ITEM_TYPE.GINGERBREAD_5;
                color = 5;
                break;
            case ITEM_TYPE.GINGERBREAD_6:
                prefab = Resources.Load(Configure.Gingerbread6()) as GameObject;
                check = ITEM_TYPE.GINGERBREAD_6;
                color = 6;
                break;
        }

        if (prefab != null)
        {
            type = check;
            effect = BREAKER_EFFECT.NORMAL;

            GetComponent<SpriteRenderer>().sprite = prefab.GetComponent<SpriteRenderer>().sprite;
        }
    }

    public void ChangeToRainbow()
    {
        ChangeToSpecial(ITEM_TYPE.RAINBOW);
    }

    public void ChangeToBombBreaker()
    {
        ChangeToSpecial(ITEM_TYPE.BOMB_BREAKER);
    }

    public void ChangeToColRowBreaker()
    {
        var columnBreaker = Random.Range(0, 2) == 0;
        var type = columnBreaker ? ITEM_TYPE.COLUMN_BREAKER : ITEM_TYPE.ROW_BREAKER;

        ChangeToSpecial(type);
    }

    public void ChangeToColBreaker()
    {
        ChangeToSpecial(ITEM_TYPE.COLUMN_BREAKER);
    }

    public void ChangeToRowBreaker()
    {
        ChangeToSpecial(ITEM_TYPE.ROW_BREAKER);
    }

    public void ChangeToPlaneBreaker()
    {
        ChangeToSpecial(ITEM_TYPE.PLANE_BREAKER);
    }

    public event EventHandler<EventArgs> ChangeToSpecialHandler;

    public string ToIdleEffect()
    {
        return Configure.Idle + type.ToString().ToLower();
    }

    public void ChangeToSpecial(ITEM_TYPE itemType)
    {
        type = itemType;
        color = 0;

        var itemTypeStr = ToIdleEffect();
        var prefab = Instantiate(CFX_SpawnSystem.GetNextObject(itemTypeStr, null, false));
        if (prefab == null)
            return;

        spriteRenderer.sprite = prefab.GetComponent<SpriteRenderer>().sprite;

        var generate = CFX_SpawnSystem.GetNextObject(Configure.EffectGeneratePlane, transform);

        if (ChangeToSpecialHandler != null)
            ChangeToSpecialHandler(this, new EventArgs());

        Debug.LogWarning("ChangeToSpecial: " + itemType);
    }

    public void SetRandomNextType()
    {
        var random = Random.Range(0, 4);
        switch (random)
        {
            case 0:
                next = ITEM_TYPE.COLUMN_BREAKER;
                break;
            case 1:
                next = ITEM_TYPE.ROW_BREAKER;
                break;
            case 2:
                next = ITEM_TYPE.BOMB_BREAKER;
                break;
            case 3:
                next = ITEM_TYPE.PLANE_BREAKER;
                break;
        }
    }

    #endregion

    #region Destroy

    public void Destroy(bool forced = false, bool isPlaneChangeExplode = false, DestroyBy destroyBy = DestroyBy.Cookie)
    {
        if (Destroyable() == false && forced == false)
            return;

        if (destroying)
            return;

        destroying = true;

        beAbleToDestroy--;

        if (board.state == GAME_STATE.PRE_WIN_AUTO_PLAYING)
        {
            board.WinGoldReward(this);
        }

        if (!isPlaneChangeExplode && node != null && node.cage != null)
        {
            node.CageExplode();
            return;
        }

        if (!isPlaneChangeExplode && node != null && node.ice != null)
        {
            node.IceExplode();
            return;
        }

        if (!isPlaneChangeExplode && node != null && node.jelly != null)
        {
            if (node.JellyExplode())
                return;
        }

        if (!isPlaneChangeExplode && node != null && node.packagebox != null)
        {
            node.PackageBoxExplode();
            return;
        }

        board.destroyingItems++;

        DestroyByType = destroyBy;

        Debug.LogWarning("=====> Item:Destroy() - " + node + ", board state:" + board.state.ToString() + ", animation started!!");

        // destroy animation
        var s = DOTween.Sequence();
        s.Append(transform.
            DOScale(Configure.instance.ItemDestroyScaleToMax * transform.localScale, Configure.instance.destroyTime / 4).
            OnStart(() => OnStartDestroy()));
        s.Append(transform.
            DOScale(transform.localScale, Configure.instance.destroyTime / 4).
            OnComplete(() => OnCompleteDestroy(isPlaneChangeExplode)));
        s.Play();
    }

    public void OnStartDestroy()
    {
        // destroy and check collect waffle
        if (node != null)
            node.WaffleExplode();

        // check collect 
        board.CollectItem(this);

        // destroy neighbor marshmallow/chocolate/rock candy/jelly
        if (type != ITEM_TYPE.BLANK)
            board.DestroyNeighborItems(this);

        // explosion effect
        if (effect == BREAKER_EFFECT.BOMB_ROWCOL_BREAKER)
        {
            // bomb + row / col breaker.
            BombColRowBreakerExplosion();
        }
        else if (effect == BREAKER_EFFECT.CROSS)
        {
            // row + col breaker.
            CrossBreakerExplosion();
        }
        else if (effect == BREAKER_EFFECT.BOMB_X_BREAKER)
        {
            BombXBreakerExplosion();
        }
        else if (effect == BREAKER_EFFECT.CROSS_X_BREAKER)
        {
            CrossXBreakerExplosion();
        }
        else if (effect == BREAKER_EFFECT.BIG_BOMB_BREAKER)
        {
            // two bombs.
            BigBombBreakerExplosion();
        }
        else if (effect == BREAKER_EFFECT.BIG_PLANE_BREAKER)
        {
            // two planes.
            BigPlaneBreakerExplosion();
        }
        else if (effect == BREAKER_EFFECT.PLANE_CHANGE_BREAKER)
        {
            // plane with bomb / col / row breaker.
            PlaneChangeBreakerExplosion();
        }
        else if (effect == BREAKER_EFFECT.NORMAL)
        {
            if (IsCookie())
            {
                CookieExplosion();
            }
            else if (IsGingerbread())
            {
                GingerbreadExplosion();
            }
            else if (IsMarshmallow())
            {
                MarshmallowExplosion();
            }
            else if (IsChocolate())
            {
                ChocolateExplosion();
            }
            else if (IsRockCandy())
            {
                RockCandyExplosion();
            }
            else if (IsCollectible())
            {
                CollectibleExplosion();
            }
            else if (IsBombBreaker(type))
            {
                BombBreakerExplosion();
            }
            else if (type == ITEM_TYPE.RAINBOW)
            {
                RainbowExplosion();
            }
            else if (IsColumnBreaker(type))
            {
                ColumnBreakerExplosion();
            }
            else if (IsRowBreaker(type))
            {
                RowBreakerExplosion();
            }
            else if (IsPlaneBreaker(type))
            {
                PlaneBreakerExplosion();
            }
            else if (IsAppleBox())
            {
                AppleBoxExplosion();
            }
        }
    }

    public void OnCompleteDestroy(bool isPlaneChangeExplode, DestroyBy destroyBy = DestroyBy.Cookie)
    {
        if (board.state == GAME_STATE.PRE_WIN_AUTO_PLAYING)
        {
            board.score += Configure.instance.finishedScoreItem * board.dropTime;
        }
        else
        {
            board.score += Configure.instance.scoreItem * board.dropTime;
        }

        if (next != ITEM_TYPE.NONE)
        {
            if (IsBombBreaker(next))
            {
                if (nextSound)
                    AudioManager.instance.BombBreakerAudio();
            }
            else if (IsRowBreaker(next) || IsColumnBreaker(next))
            {
                if (nextSound)
                    AudioManager.instance.ColRowBreakerAudio();
            }
            else if (next == ITEM_TYPE.RAINBOW)
            {
                if (nextSound)
                    AudioManager.instance.RainbowAudio();
            }

            // generate a item at position of the node
            node.GenerateItem(next);
        }
        else if (type == ITEM_TYPE.CHOCOLATE_2_LAYER)
        {
            // generate a new chocolate
            node.GenerateItem(ITEM_TYPE.CHOCOLATE_1_LAYER);

            // set position
            board.GetNode(node.i, node.j).item.gameObject.transform.localPosition =
                board.NodeLocalPosition(node.i, node.j);
        }
        else if (type == ITEM_TYPE.CHOCOLATE_3_LAYER)
        {
            node.GenerateItem(ITEM_TYPE.CHOCOLATE_2_LAYER);

            board.GetNode(node.i, node.j).item.gameObject.transform.localPosition =
                board.NodeLocalPosition(node.i, node.j);
        }
        else if (type == ITEM_TYPE.CHOCOLATE_4_LAYER)
        {
            node.GenerateItem(ITEM_TYPE.CHOCOLATE_3_LAYER);

            board.GetNode(node.i, node.j).item.gameObject.transform.localPosition =
                board.NodeLocalPosition(node.i, node.j);
        }
        else if (type == ITEM_TYPE.APPLEBOX)
        {
            applebox.TryToDestroyApple(this);
            return;
        }
        else if (!isPlaneChangeExplode)
        {
            node.GenerateItem(ITEM_TYPE.BLANK);
        }

        if (destroying)
        {
            Debug.LogWarning("=====> Item:OnCompleteDestroy() - " + node + ", board state:" + board.state.ToString() + ", animation started!!");

            var s = DOTween.Sequence();
            s.Append(transform.
                DOScale(Configure.instance.ItemDestroyScaleToMin * transform.localScale, Configure.instance.destroyTime / 2).
                OnComplete(() =>
                {
                    UnityEngine.Object.Destroy(gameObject);

                    board.destroyingItems--;
                    // there is a case when a item is dropping and it is destroyed by other call
                    if (dropping)
                        board.droppingItems--;
                }));
            s.Insert(0f, spriteRenderer.DOFade(0f, Configure.instance.destroyTime / 2).SetEase(Ease.InExpo));
            s.Play();
        }
    }

    public IEnumerator ResetDestroying()
    {
        yield return new WaitForSeconds(Configure.instance.destroyTime);
        destroying = false;
    }

    #endregion

    #region Explosion

    void CookieExplosion()
    {
        AudioManager.instance.CookieCrushAudio();
        CFX_SpawnSystem.GetNextObject(Configure.EffectCookieDestroy, transform);
    }

    void GingerbreadExplosion()
    {
        AudioManager.instance.GingerbreadExplodeAudio();
        CFX_SpawnSystem.GetNextObject(Configure.EffectGingerbreadDestroy, transform);
    }

    void MarshmallowExplosion()
    {
        AudioManager.instance.MarshmallowExplodeAudio();
        CFX_SpawnSystem.GetNextObject(Configure.EffectMarshmallow, transform);
    }

    void AppleBoxExplosion()
    {
        AudioManager.instance.MarshmallowExplodeAudio();
        CFX_SpawnSystem.GetNextObject(Configure.EffectAppleBox, transform);
    }

    public void ChocolateExplosion()
    {
        AudioManager.instance.ChocolateExplodeAudio();
        CFX_SpawnSystem.GetNextObject(Configure.EffectChocolate, transform);
    }

    public void RockCandyExplosion()
    {
        AudioManager.instance.RockCandyExplodeAudio();
        CFX_SpawnSystem.GetNextObject(Configure.EffectRockCandyDestroy, transform);
    }

    void CollectibleExplosion()
    {
        AudioManager.instance.CollectibleExplodeAudio();
    }

    void BombBreakerExplosion()
    {
        AudioManager.instance.BombExplodeAudio();

        BombBreakerDestroy(2);

        CFX_SpawnSystem.GetNextObject(Configure.EffectBomb, transform);
    }

    void RainbowExplosion()
    {
        AudioManager.instance.RainbowExplodeAudio();

        DestroyItemsSameColor(LevelLoader.instance.RandomColor());

        CFX_SpawnSystem.GetNextObject(Configure.EffectRainbow, transform);
    }

    void XBreakerExplosion()
    {
        AudioManager.instance.ColRowBreakerExplodeAudio();

        XBreakerDestroy();
    }

    void RocketExplosion(List<Node.Direction> directionList, Node.Direction deltaDirection)
    {
        if (directionList == null || directionList.Count == 0)
            throw new Exception("Direction list should not be null or empty.");

        AudioManager.instance.ColRowBreakerExplodeAudio();

        var pos = transform.position + new Vector3(deltaDirection.X * board.NodeSize(), deltaDirection.Y * board.NodeSize(), 0);
        CFX_SpawnSystem.GetNextObject(Configure.EffectColRow, pos);

        var lineList = new List<GameObject>();
        for (var i = 0; i < directionList.Count; ++i)
        {
            var line = CFX_SpawnSystem.GetNextObject(Configure.EffectColRowLine, pos, false);
            lineList.Add(line);
        }
        var lineDuration = lineList[0].GetComponent<CFX_AutoDestruct>().Duration;

        var sq = DOTween.Sequence();
        sq.AppendInterval(Configure.instance.destroyTime / 5).
            AppendCallback(() =>
            {
                for (var i = 0; i < lineList.Count; ++i)
                {
                    var line = lineList[i];
                    line.SetActive(true);
                    line.transform.DOMove(line.transform.position + new Vector3(directionList[i].X, directionList[i].Y, 0) * board.NodeSize() * 10, lineDuration);
                }

                if (directionList.Contains(Node.Direction.Left) || directionList.Contains(Node.Direction.Right))
                    RowDestroy(node.i + deltaDirection.Y);
                if (directionList.Contains(Node.Direction.Top) || directionList.Contains(Node.Direction.Bottom))
                    ColumnDestroy(node.j + deltaDirection.X);
            });
        sq.Play();
    }

    void ColumnBreakerExplosion()
    {
        RocketExplosion(new List<Node.Direction> { Node.Direction.Top, Node.Direction.Bottom }, Node.Direction.Zero);
    }

    void BombColRowBreakerExplosion()
    {
        AudioManager.instance.ColRowBreakerExplodeAudio();

        RocketExplosion(new List<Node.Direction> { Node.Direction.Top, Node.Direction.Bottom }, Node.Direction.Left);
        RocketExplosion(new List<Node.Direction> { Node.Direction.Top, Node.Direction.Bottom }, Node.Direction.Zero);
        RocketExplosion(new List<Node.Direction> { Node.Direction.Top, Node.Direction.Bottom }, Node.Direction.Right);

        RocketExplosion(new List<Node.Direction> { Node.Direction.Left, Node.Direction.Right }, Node.Direction.Top);
        RocketExplosion(new List<Node.Direction> { Node.Direction.Left, Node.Direction.Right }, Node.Direction.Zero);
        RocketExplosion(new List<Node.Direction> { Node.Direction.Left, Node.Direction.Right }, Node.Direction.Bottom);
    }

    void CrossBreakerExplosion()
    {
        RocketExplosion(new List<Node.Direction> { Node.Direction.Top, Node.Direction.Bottom, Node.Direction.Left, Node.Direction.Right }, Node.Direction.Zero);
    }

    void BigBombBreakerExplosion()
    {
        AudioManager.instance.BombExplodeAudio();

        var sq = DOTween.Sequence();
        sq.AppendInterval(Configure.instance.destroyTime / 5).
            AppendCallback(() => BombBreakerDestroy(3));
        sq.Play();

        CFX_SpawnSystem.GetNextObject(Configure.EffectDubbleBomb, transform);
    }

    void BigPlaneBreakerExplosion()
    {
        AudioManager.instance.BombExplodeAudio();

        PlaneDestroy(3);

        CFX_SpawnSystem.GetNextObject(Configure.EffectBigPlane, transform);
    }

    void PlaneChangeBreakerExplosion()
    {
        AudioManager.instance.BombExplodeAudio();

        PlaneDestroy();

        CFX_SpawnSystem.GetNextObject(Configure.EffectPlane, transform);
    }

    void RowBreakerExplosion()
    {
        RocketExplosion(new List<Node.Direction> { Node.Direction.Left, Node.Direction.Right }, Node.Direction.Zero);
    }

    void PlaneBreakerExplosion()
    {
        AudioManager.instance.ColRowBreakerExplodeAudio();
        PlaneDestroy();

        CFX_SpawnSystem.GetNextObject(Configure.EffectPlane, transform);
    }

    void BombXBreakerExplosion()
    {
        BombBreakerExplosion();

        XBreakerExplosion();
    }

    void CrossXBreakerExplosion()
    {
        CrossBreakerExplosion();

        XBreakerExplosion();
    }

    #endregion

    #region SpecialDestroy

    void BombBreakerDestroy(int range)
    {
        List<Item> items = board.ItemAround(node, range);

        var isgrass = node.IsGrass();

        foreach (var item in items)
        {
            if (item != null)
            {
                if (isgrass)
                {
                    item.node.ChangeToGrass();
                }

                if (item.node.bafflebottom != null)
                {
                    item.node.bafflebottom.DestroyBaffle();
                }

                if (item.node.baffleright != null)
                {
                    item.node.baffleright.DestroyBaffle();
                }

                item.Destroy(false, false, Item.DestroyBy.Booster);
            }
        }
    }

    void XBreakerDestroy()
    {
        var items = board.XCrossItems(node);

        foreach (var item in items)
        {
            if (item != null)
            {
                item.Destroy(false, false, Item.DestroyBy.Booster);
            }
        }
    }

    /// <summary>
    /// destroy all items with the same color (when this color swap with a rainbow)
    /// </summary>
    /// <param name="color">color to destory</param>
    public void DestroyItemsSameColor(int color)
    {
        List<Item> items = board.GetListItems();

        bool isgrass = node.IsGrass();

        foreach (Item item in items)
        {
            if (item != null)
            {
                if (item.color == color && item.Matchable())
                {
                    board.sameColorList.Add(item);
                }
            }
        }

        board.DestroySameColorList(this, isgrass);
    }

    // Rainbow swap with other item
    public void RainbowDestroy(Item thisItem, Item otherItem)
    {
        if (thisItem.Destroyable() == false || otherItem.Destroyable() == false)
        {
            return;
        }

        bool isgrass = node.IsGrass();

        if (thisItem.type == ITEM_TYPE.RAINBOW)
        {
            if (otherItem.IsCookie())
            {
                thisItem.DestroyItemsSameColor(otherItem.color);
                thisItem.type = ITEM_TYPE.NONE;

                thisItem.Destroy();
            }
            else if (otherItem.IsBombBreaker(otherItem.type) || otherItem.IsRowBreaker(otherItem.type) ||
                     otherItem.IsColumnBreaker(otherItem.type) || otherItem.IsPlaneBreaker(otherItem.type))
            {
                var mostColor = board.GetMostColor();
                board.ChangeItemsType(thisItem.transform.position, mostColor, otherItem.type, thisItem.node.IsGrass());

                thisItem.type = ITEM_TYPE.NONE;
                otherItem.type = ITEM_TYPE.NONE;

                thisItem.Destroy();
                otherItem.Destroy();
            }
            else if (otherItem.type == ITEM_TYPE.RAINBOW)
            {
                board.DoubleRainbowDestroy(isgrass);

                thisItem.type = ITEM_TYPE.NONE;
                otherItem.type = ITEM_TYPE.NONE;

                thisItem.Destroy();
                otherItem.Destroy();
            }
        }
        else if (otherItem.type == ITEM_TYPE.RAINBOW)
        {
            if (thisItem.IsCookie())
            {
                otherItem.DestroyItemsSameColor(thisItem.color);
                otherItem.type = ITEM_TYPE.NONE;

                otherItem.Destroy();
            }
            else if (thisItem.IsBombBreaker(thisItem.type) || thisItem.IsRowBreaker(thisItem.type) ||
                     thisItem.IsColumnBreaker(thisItem.type) || otherItem.IsPlaneBreaker(thisItem.type))
            {
                var mostColor = board.GetMostColor();

                board.ChangeItemsType(otherItem.transform.position, mostColor, thisItem.type, otherItem.node.IsGrass());

                thisItem.type = ITEM_TYPE.NONE;
                otherItem.type = ITEM_TYPE.NONE;

                otherItem.Destroy();
                thisItem.Destroy();
            }
            else if (thisItem.type == ITEM_TYPE.RAINBOW)
            {
                board.DoubleRainbowDestroy(isgrass);

                thisItem.type = ITEM_TYPE.NONE;
                otherItem.type = ITEM_TYPE.NONE;

                thisItem.Destroy();
                otherItem.Destroy();
            }
        }
    }

    void ColumnDestroy(int col = -1, bool isspecialforgrass = false)
    {
        var nodes = new List<Node>();

        if (col == -1)
        {
            nodes = board.ColumnNodes(node.j);
        }
        else
        {
            nodes = board.ColumnNodes(col);
        }

        var upNodes = new List<Node>();

        var downNodes = new List<Node>();

        for (int i = node.i; i >= 0; i--)
        {
            upNodes.Add(nodes[i]);
        }

        var isGrass = false;
        var findMarshmallow = false;
        foreach (var node in upNodes)
        {
            if (findMarshmallow)
            {
                break;
            }

            if (node != null)
            {
                if (node.IsGrass(isspecialforgrass))
                {
                    isGrass = true;
                }

                if (isGrass)
                {
                    node.ChangeToGrass();
                }

                if (node.bafflebottom != null)
                {
                    node.bafflebottom.DestroyBaffle();
                }

                if (node.item != null)
                {
                    if (node.item.type == ITEM_TYPE.MARSHMALLOW)
                    {
                        findMarshmallow = true;
                    }

                    node.item.Destroy(false, false, Item.DestroyBy.Booster);
                }
            }
        }

        for (int i = node.i; i < nodes.Count; i++)
        {
            downNodes.Add(nodes[i]);
        }

        isGrass = false;
        findMarshmallow = false;
        foreach (var node in downNodes)
        {
            if (findMarshmallow)
            {
                break;
            }

            if (node != null)
            {
                if (node.IsGrass(isspecialforgrass))
                {
                    isGrass = true;
                }

                if (isGrass)
                {
                    node.ChangeToGrass();
                }

                if (node.TopNeighbor() != null && node.TopNeighbor().bafflebottom != null)
                {
                    node.TopNeighbor().bafflebottom.DestroyBaffle();
                }

                if (node.item != null)
                {
                    if (node.item.type == ITEM_TYPE.MARSHMALLOW)
                    {
                        findMarshmallow = true;
                    }

                    node.item.Destroy(false, false, Item.DestroyBy.Booster);
                }
            }
        }
    }

    public void RowDestroy(int row = -1, bool isspecialforgrass = false)
    {
        var nodes = new List<Node>();

        if (row == -1)
        {
            nodes = board.RowNodes(node.i);
        }
        else
        {
            nodes = board.RowNodes(row);
        }

        var leftNodes = new List<Node>();
        var rightNodes = new List<Node>();

        for (int i = node.j; i >= 0; i--)
        {
            leftNodes.Add(nodes[i]);
        }

        var isGrass = false;
        var findMarshmallow = false;
        foreach (var node in leftNodes)
        {
            if (findMarshmallow)
            {
                break;
            }

            if (node != null)
            {
                if (node.IsGrass(isspecialforgrass))
                {
                    isGrass = true;
                }

                if (isGrass)
                {
                    node.ChangeToGrass();
                }

                if (node.baffleright != null)
                {
                    node.baffleright.DestroyBaffle();
                }

                if (node.item != null)
                {
                    if (node.item.type == ITEM_TYPE.MARSHMALLOW)
                    {
                        findMarshmallow = true;
                    }

                    node.item.Destroy(false, false, Item.DestroyBy.Booster);
                }
            }
        }

        for (int i = node.j; i < nodes.Count; i++)
        {
            rightNodes.Add(nodes[i]);
        }

        isGrass = false;
        findMarshmallow = false;
        foreach (var node in rightNodes)
        {

            if (findMarshmallow)
            {
                break;
            }

            if (node != null)
            {
                if (node.IsGrass(isspecialforgrass))
                {
                    isGrass = true;
                }

                if (isGrass)
                {
                    node.ChangeToGrass();
                }

                if (node.LeftNeighbor() != null && node.LeftNeighbor().baffleright != null)
                {
                    node.LeftNeighbor().baffleright.DestroyBaffle();
                }

                if (node.item != null)
                {
                    if (node.item.type == ITEM_TYPE.MARSHMALLOW)
                    {
                        findMarshmallow = true;
                    }

                    node.item.Destroy(false, false, Item.DestroyBy.Booster);
                }
            }
        }
    }

    public void PlaneSpecialDestroy(Item thisItem, Item otherItem)
    {
        if (thisItem.type == ITEM_TYPE.PLANE_BREAKER)
        {
            if (otherItem.IsCookie())
            {
                thisItem.Destroy();
                board.FindMatches();
            }
            else if (otherItem.IsBombBreaker(otherItem.type) || otherItem.IsRowBreaker(otherItem.type) ||
                     otherItem.IsColumnBreaker(otherItem.type))
            {
                planeTargetType = otherItem.type;
                thisItem.effect = BREAKER_EFFECT.PLANE_CHANGE_BREAKER;

                otherItem.type = ITEM_TYPE.NONE;

                thisItem.Destroy();
                board.FindMatches();
            }
            else if (otherItem.IsPlaneBreaker(otherItem.type))
            {
                thisItem.effect = BREAKER_EFFECT.BIG_PLANE_BREAKER;
                otherItem.type = ITEM_TYPE.NONE;

                if (thisItem.node.TopLeftNeighbor() != null && thisItem.node.TopLeftNeighbor().item != null)
                {
                    thisItem.node.TopLeftNeighbor().item.Destroy(false, false, Item.DestroyBy.Booster);
                }

                if (thisItem.node.TopRightNeighbor() != null && thisItem.node.TopRightNeighbor().item != null)
                {
                    thisItem.node.TopLeftNeighbor().item.Destroy(false, false, Item.DestroyBy.Booster);
                }

                if (thisItem.node.BottomLeftNeighbor() != null && thisItem.node.BottomLeftNeighbor().item != null)
                {
                    thisItem.node.TopLeftNeighbor().item.Destroy(false, false, Item.DestroyBy.Booster);
                }

                if (thisItem.node.BottomRightNeighbor() != null && thisItem.node.BottomRightNeighbor().item != null)
                {
                    thisItem.node.TopLeftNeighbor().item.Destroy(false, false, Item.DestroyBy.Booster);
                }

                otherItem.Destroy();
                thisItem.Destroy();
                board.FindMatches();
            }
            else
            {
                thisItem.Destroy();
                board.FindMatches();
            }
        }
        else if (otherItem.type == ITEM_TYPE.PLANE_BREAKER)
        {
            if (thisItem.IsCookie())
            {
                otherItem.Destroy();
                board.FindMatches();
            }
            else if (thisItem.IsBombBreaker(thisItem.type) || thisItem.IsRowBreaker(thisItem.type) ||
                     thisItem.IsColumnBreaker(thisItem.type))
            {
                planeTargetType = thisItem.type;
                thisItem.effect = BREAKER_EFFECT.PLANE_CHANGE_BREAKER;

                otherItem.type = ITEM_TYPE.NONE;

                thisItem.Destroy();
                board.FindMatches();
            }
            else if (thisItem.IsPlaneBreaker(thisItem.type))
            {
                thisItem.effect = BREAKER_EFFECT.BIG_PLANE_BREAKER;

                otherItem.type = ITEM_TYPE.NONE;

                if (thisItem.node.TopLeftNeighbor() != null && thisItem.node.TopLeftNeighbor().item != null)
                {
                    thisItem.node.TopLeftNeighbor().item.Destroy(false, false, Item.DestroyBy.Booster);
                }

                if (thisItem.node.TopRightNeighbor() != null && thisItem.node.TopRightNeighbor().item != null)
                {
                    thisItem.node.TopLeftNeighbor().item.Destroy(false, false, Item.DestroyBy.Booster);
                }

                if (thisItem.node.BottomLeftNeighbor() != null && thisItem.node.BottomLeftNeighbor().item != null)
                {
                    thisItem.node.TopLeftNeighbor().item.Destroy(false, false, Item.DestroyBy.Booster);
                }

                if (thisItem.node.BottomRightNeighbor() != null && thisItem.node.BottomRightNeighbor().item != null)
                {
                    thisItem.node.TopLeftNeighbor().item.Destroy(false, false, Item.DestroyBy.Booster);
                }

                otherItem.Destroy();

                thisItem.Destroy();
                board.FindMatches();
            }
            else
            {
                otherItem.Destroy();
                board.FindMatches();
            }
        }
    }

    public void PlaneDestroy(int planeNum = 1)
    {
        planeNum += board.planePlusNum;

        var isgrass = node.IsGrass();
        var items = board.ItemAround(node, 1);
        foreach (var item in items)
        {
            if (item != null)
            {
                if (isgrass)
                {
                    item.node.ChangeToGrass();
                }

                item.Destroy(false, false, Item.DestroyBy.Booster);
            }
        }

        if (node.TopNeighbor() != null && node.TopNeighbor().bafflebottom != null)
        {
            node.TopNeighbor().bafflebottom.DestroyBaffle();
        }

        if (node.LeftNeighbor() != null && node.LeftNeighbor().baffleright != null)
        {
            node.LeftNeighbor().baffleright.DestroyBaffle();
        }

        if (node.bafflebottom != null)
        {
            node.bafflebottom.DestroyBaffle();
        }

        if (node.baffleright != null)
        {
            node.baffleright.DestroyBaffle();
        }

        board.FindPlaneTarget(planeNum, (node.grass != null));

        if (board.rdmItems.Count < planeNum)
            throw new Exception("Target could not collect enough items to fullfill number of planes.");

        var gameObjectPlane = new List<GameObject>();

        for (var i = 0; i < planeNum; i++)
        {
            var planeTargetItem = board.rdmItems[board.rdmItems.Count - 1];
            planeTargetItem.beAbleToDestroy--;

            board.rdmItems.RemoveAt(board.rdmItems.Count - 1);

            gameObjectPlane.Add(Instantiate(CFX_SpawnSystem.GetNextObject(Configure.PlaneBreaker, null, false)));
            gameObjectPlane[i].SetActive(true);
            gameObjectPlane[i].transform.position = gameObject.transform.position;
            var planItem = gameObjectPlane[i].GetComponent<Item>();
            planItem.planeTargetType = planeTargetType;
            planItem.node = planeTargetItem.node;
            planItem.board = board;
            gameObjectPlane[i].GetComponent<SpriteRenderer>().sortingLayerName = SortingLayers.Effect;

            board.playingAnimation++;

            var sourceItem = planItem;
            var targetItem = planeTargetItem;
            var bezierPathConfig = gameObjectPlane[i].GetComponent<BezierPathConfigure>();
            var path = bezierPathConfig.GetPath(gameObjectPlane[i].transform.position, planeTargetItem.transform.position);
            gameObjectPlane[i].transform.DOPath(path.ToArray(), bezierPathConfig.Duration, bezierPathConfig.PlanePathType).SetEase(bezierPathConfig.EaseType)
                .OnComplete(() => onPlaneComplete(sourceItem, targetItem, isgrass));
        }
    }

    private void onPlaneComplete(Item sourceItem, Item targetItem, bool isgrass)
    {
        if (isgrass)
        {
            sourceItem.node.ChangeToGrass();
        }

        if (planeTargetType != ITEM_TYPE.NONE)
        {
            sourceItem.ChangeToSpecialType(planeTargetType);
        }
        else
        {
            if (targetItem != null)
                sourceItem.board.DestroyPlaneTargetList(targetItem, isgrass);
            UnityEngine.Object.Destroy(sourceItem.gameObject);
        }
        sourceItem.board.playingAnimation--;
    }

    private List<Item> FindPlaneTarget(int planeNum)
    {
        var rdmItems = new List<Item>();

        for (var i = 0; i < LevelLoader.instance.targetList.Count; ++i)
        {
            var targetLeft = board.targetLeftList[i];
            var target = LevelLoader.instance.targetList[i];
            if (targetLeft > 0 && target.Amount > 0)
            {
                var targetItemList = FindPlaneTarget(target.Type, target.color, rdmItems);
                rdmItems.AddRange(targetItemList);
                rdmItems.Randomize();
            }

            if (rdmItems.Count >= planeNum)
                return rdmItems;
        }

        FindPlaneGeneraltarget(rdmItems, planeNum);

        return rdmItems;
    }

    private List<Item> FindPlaneTarget(TARGET_TYPE targetType, int targetColor, List<Item> targetItems)
    {
        List<Item> allTargetItems = new List<Item>();

        for (int i = 0; i < LevelLoader.instance.row; i++)
        {
            for (int j = 0; j < LevelLoader.instance.column; j++)
            {
                Node node = board.GetNode(i, j);

                if (node != null &&
                    node.item != null &&
                    node.item.beAbleToDestroy > 0 &&
                    !node.item.destroying &&
                    !allTargetItems.Contains(node.item) &&
                    !targetItems.Contains(node.item) &&
                    node.IsInCurrentPage())
                {
                    Item item = node.item;

                    if (targetType == TARGET_TYPE.COLLECTIBLE)
                    {
                        if (item.IsCollectible() && item.color == targetColor)
                        {
                            List<Item> obstacle = new List<Item>();
                            List<Item> targettmp = new List<Item>();

                            for (int k = LevelLoader.instance.row - 1; k >= i + 1; k--)
                            {
                                if (board.GetNode(k, j) != null)
                                {
                                    var targetNode = board.GetNode(k, j);
                                    var targetItem = targetNode.item;
                                    if (targetItem != null && !targetItem.destroying)
                                    {
                                        if (!targetNode.CanDropIn())
                                        {
                                            obstacle.Add(targetItem);
                                        }
                                        else if (!targetItem.IsCollectible() && !targetNode.isNodeBlank())
                                        {
                                            targettmp.Add(targetItem);
                                        }
                                    }
                                }
                            }

                            if (obstacle.Count > 0)
                            {
                                allTargetItems.AddRange(obstacle);
                            }
                            else if (targettmp.Count > 0)
                            {
                                allTargetItems.AddRange(targettmp);
                            }
                        }
                    }
                    else if (targetType == TARGET_TYPE.CAGE)
                    {
                        if (node.cage != null)
                        {
                            allTargetItems.Add(item);
                        }
                    }
                    else if (targetType == TARGET_TYPE.ROCK_CANDY)
                    {
                        if (item.IsRockCandy())
                        {
                            allTargetItems.Add(item);
                        }
                    }
                    else if (this.node.grass != null && targetType == TARGET_TYPE.GRASS)
                    {
                        if (node.grass == null)
                        {
                            allTargetItems.Add(item);
                        }
                    }
                    else if (targetType == TARGET_TYPE.CHERRY)
                    {
                        if (item.IsCherry())
                        {
                            allTargetItems.Add(item);
                        }
                    }
                    else if (targetType == TARGET_TYPE.PACKAGEBOX)
                    {
                        if (node.packagebox != null)
                        {
                            allTargetItems.Add(item);
                        }
                    }
                    else if (targetType == TARGET_TYPE.APPLEBOX)
                    {
                        if (node.item.IsAppleBox())
                        {
                            allTargetItems.Add(item);
                        }
                    }
                    else if (targetType == TARGET_TYPE.MARSHMALLOW)
                    {
                        if (item.IsMarshmallow())
                        {
                            allTargetItems.Add(item);
                        }
                    }
                    else if (targetType == TARGET_TYPE.COOKIE)
                    {
                        if (item.IsCookie() && item.color == targetColor)
                        {
                            allTargetItems.Add(item);
                        }
                    }
                }
            }
        }

        return allTargetItems;
    }

    private void FindPlaneGeneraltarget(List<Item> targetItems, int planeNum)
    {
        List<Item> allSpecialTargetItems = new List<Item>();
        List<Item> allCookieTargetItems = new List<Item>();
        for (int i = 0; i < LevelLoader.instance.row; i++)
        {
            for (int j = 0; j < LevelLoader.instance.column; j++)
            {
                var node = board.GetNode(i, j);
                if (node != null && node.item != null && !targetItems.Contains(node.item) &&
                    node.item.beAbleToDestroy > 0)
                {
                    Item item = node.item;
                    if (item != this && item.type != ITEM_TYPE.BLANK && !item.Movable() && !allSpecialTargetItems.Contains(item))
                    {
                        allSpecialTargetItems.Add(item);
                    }
                    else if (item.IsCookie() && !allCookieTargetItems.Contains(item))
                    {
                        allCookieTargetItems.Add(item);
                    }
                }
            }
        }

        if (targetItems.Count <= planeNum)
        {
            allSpecialTargetItems.ForEach(item => targetItems.Add(allSpecialTargetItems[Random.Range(0, allSpecialTargetItems.Count)]));
        }

        if (targetItems.Count <= planeNum)
        {
            allCookieTargetItems.ForEach(item => targetItems.Add(allCookieTargetItems[Random.Range(0, allCookieTargetItems.Count)]));
        }
    }

    void TwoColRowBreakerDestroy(Item thisItem, Item otherItem)
    {
        if (thisItem == null || otherItem == null)
        {
            return;
        }

        if ((IsRowBreaker(thisItem.type) || IsColumnBreaker(thisItem.type)) &&
            (IsRowBreaker(otherItem.type) || IsColumnBreaker(otherItem.type)))
        {
            thisItem.effect = BREAKER_EFFECT.CROSS;
            otherItem.effect = BREAKER_EFFECT.NONE;

            thisItem.Destroy();
            otherItem.Destroy();

            board.FindMatches();
        }
    }

    void TwoBombBreakerDestroy(Item thisItem, Item otherItem)
    {
        if (thisItem == null || otherItem == null)
        {
            return;
        }

        if (IsBombBreaker(thisItem.type) && IsBombBreaker(otherItem.type))
        {
            thisItem.effect = BREAKER_EFFECT.BIG_BOMB_BREAKER;

            otherItem.type = ITEM_TYPE.NONE;

            thisItem.Destroy();
            otherItem.Destroy();

            board.FindMatches();
        }
    }

    void ColRowBreakerAndBombBreakerDestroy(Item thisItem, Item otherItem)
    {
        if (thisItem == null || otherItem == null)
        {
            return;
        }

        if (
            (IsRowBreaker(thisItem.type) || IsColumnBreaker(thisItem.type)) && IsBombBreaker(otherItem.type)
            || (IsRowBreaker(otherItem.type) || IsColumnBreaker(otherItem.type)) && IsBombBreaker(thisItem.type)
        )
        {
            thisItem.effect = BREAKER_EFFECT.BOMB_ROWCOL_BREAKER;

            otherItem.type = otherItem.GetCookie(otherItem.type);

            thisItem.ChangeToBig();
        }
    }


    #endregion

    #region Change

    void ChangeToSpecialType(ITEM_TYPE changeToType)
    {
        if (IsColumnBreaker(changeToType))
        {
            ChangeToColBreaker();
        }
        else if (IsRowBreaker(changeToType))
        {
            ChangeToRowBreaker();
        }
        else if (IsBombBreaker(changeToType))
        {
            ChangeToBombBreaker();
        }
        else if (IsPlaneBreaker(changeToType))
        {
            ChangeToPlaneBreaker();
        }

        beAbleToDestroy++;
        Destroy(false, true);
        board.FindMatches();
    }

    void ChangeToBig()
    {
        if (changing)
            return;

        changing = true;

        GetComponent<SpriteRenderer>().sortingLayerName = SortingLayers.Effect;

        gameObject.transform.DOScale(new Vector3(2.5f, 2.5f, 0), Configure.instance.changingTime).SetEase(Ease.Linear)
            .OnComplete(CompleteChangeToBig);
    }

    void CompleteChangeToBig()
    {
        Destroy();
        board.FindMatches();
    }

    #endregion

    #region Drop

    public void Drop()
    {
        if (dropping)
            return;
        dropping = true;

        if (dropPath.Count > 1)
        {
            board.droppingItems++;

            var dist = (transform.position.y - dropPath[0].y);


            var time = (transform.position.y - dropPath[dropPath.Count - 1].y) / board.NodeSize();

            // fix iTween interesting problems http://vanstrydonck.com/working-with-itween-paths/
            while (dist > 0.1f)
            {
                dist -= board.NodeSize();
                dropPath.Insert(0, dropPath[0] + new Vector3(0, board.NodeSize(), 0));
            }

            if (node.ice != null)
            {
                iTween.MoveTo(node.ice.gameObject, iTween.Hash(
                    "path", dropPath.ToArray(),
                    "movetopath", false,
                    "onstart", "OnStartDrop",
                    "oncomplete", "OnCompleteDrop",
                    "easetype", iTween.EaseType.linear,
                    "time", Configure.instance.dropTime * time
                ));
            }

            iTween.MoveTo(gameObject, iTween.Hash(
                "path", dropPath.ToArray(),
                "movetopath", false,
                "onstart", "OnStartDrop",
                "oncomplete", "OnCompleteDrop",
                "easetype", iTween.EaseType.linear,
                "time", Configure.instance.dropTime * time
            ));
        }
        else
        {
            Vector3 target = board.NodeLocalPosition(node.i, node.j) + board.transform.position;

            if (Mathf.Abs(transform.position.x - target.x) > 0.1f || Mathf.Abs(transform.position.y - target.y) > 0.1f)
            {
                board.droppingItems++;

                var time = (transform.position.y - target.y) / board.NodeSize();

                //print("target: " + target);

                //Debug.Log("Node i:"+node.i+" j:"+node.j+"Target"+target);

                if (node.ice != null)
                {
                    iTween.MoveTo(node.ice.gameObject, iTween.Hash(
                        "position", target,
                        "onstart", "OnStartDrop",
                        "oncomplete", "OnCompleteDrop",
                        "easetype", iTween.EaseType.linear,
                        "time", Configure.instance.dropTime * time
                    ));
                }

                iTween.MoveTo(gameObject, iTween.Hash(
                    "position", target,
                    "onstart", "OnStartDrop",
                    "oncomplete", "OnCompleteDrop",
                    "easetype", iTween.EaseType.linear,
                    "time", Configure.instance.dropTime * time
                ));
            }
            else
            {
                dropping = false;
            }
        }
    }

    public bool isNeedDrop()
    {
        if (dropPath.Count > 1)
        {
            return true;
        }

        Vector3 target = board.NodeLocalPosition(node.i, node.j) + board.transform.position;

        if (Mathf.Abs(transform.position.x - target.x) > 0.1f || Mathf.Abs(transform.position.y - target.y) > 0.1f)
        {
            return true;
        }

        return false;
    }


    public void OnStartDrop()
    {

    }

    public void OnCompleteDrop()
    {
        if (dropping)
        {
            AudioManager.instance.DropAudio();

            // reset
            dropPath.Clear();

            board.droppingItems--;

            // reset
            dropping = false;

            CookieGeneralEffect();
            if (node.BottomNeighbor() != null && node.BottomNeighbor().item != null)
            {
                node.BottomNeighbor().item.CookieGeneralEffect();
            }
        }
    }

    #endregion

    #region AnimationEffect

    public void CookieGeneralEffect()
    {
        if (IsCookie())
        {
            Sequence m_sequence = DOTween.Sequence();

            var scale = CookieScale;
            m_sequence.Append(transform.DOScale(new Vector3(scale.x + 0.15f, scale.y - 0.2f, scale.z), 0.15f))
                .Append(transform.DOScale(new Vector3(scale.x - 0.10f, scale.y + 0.03f, scale.z), 0.15f))
                .Append(transform.DOScale(new Vector3(scale.x + 0.05f, scale.y, scale.z), 0.024f))
                .Append(transform.DOScale(new Vector3(scale.x, scale.y, scale.z), 0.2f));
        }
    }

    #endregion
}
