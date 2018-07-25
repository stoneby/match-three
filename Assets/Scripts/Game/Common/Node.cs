using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class Node : MonoBehaviour
{
    [Serializable]
    public class Direction
    {
        public static Direction Left = new Direction { X = -1, Y = 0 };
        public static Direction Right = new Direction { X = 1, Y = 0 };
        public static Direction Top = new Direction { X = 0, Y = -1 };
        public static Direction Bottom = new Direction { X = 0, Y = 1 };
        public static Direction TopLeft = new Direction { X = -1, Y = -1 };
        public static Direction TopRight = new Direction { X = 1, Y = -1 };
        public static Direction BottomLeft = new Direction { X = -1, Y = 1 };
        public static Direction BottomRight = new Direction { X = 1, Y = 1 };

        public static Direction Zero = new Direction { X = 0, Y = 0 };

        public int X;
        public int Y;

        public Direction TurnLeft()
        {
            return new Direction(this * Top, this * Left);
        }

        public Direction TurnRight()
        {
            return new Direction(this * Bottom, this * Right);
        }

        public Direction TurnTop()
        {
            return this * -1;
        }

        public Direction TurnTopLeft()
        {
            return TurnLeft() + TurnTop();
        }

        public Direction TurnTopRight()
        {
            return TurnRight() + TurnTop();
        }

        public Direction TurnBottomLeft()
        {
            return this + TurnLeft();
        }

        public Direction TurnBottomRight()
        {
            return this + TurnRight();
        }

        public Direction()
        {
            X = Y = 0;
        }

        public Direction(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Direction operator +(Direction c1, Direction c2)
        {
            return new Direction(c1.X + c2.X, c1.Y + c2.Y);
        }

        public static Direction operator -(Direction c1, Direction c2)
        {
            return new Direction(c1.X - c2.X, c1.Y - c2.Y);
        }

        public static Direction operator *(Direction c1, int length)
        {
            return new Direction(c1.X * length, c1.Y * length);
        }

        public static int operator *(Direction c1, Direction c2)
        {
            return c1.X * c2.X + c1.Y * c2.Y;
        }

        public static bool operator ==(Direction c1, Direction c2)
        {
            return c1.X == c2.X && c1.Y == c2.Y;
        }

        public static bool operator !=(Direction c1, Direction c2)
        {
            return !(c1 == c2);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }

        public override bool Equals(object obj)
        {
            return this == obj as Direction;
        }
    }

    [Header("Variables")]
    public Board board;
    public Tile tile;
    public Waffle waffle;
    public Item item;
    public Cage cage;
    public Jelly jelly;
    public PackageBox packagebox;
    public Ice ice;
    public Grass grass;
    public Baffle baffleright;
    public Baffle bafflebottom;
    public GameObject ovenActive;
    [Header("")]
    public int i; // row of node
    public int j; // column of node
    /// <summary>
    /// lock drop and through attribute don't let item drop if page is not get yet
    /// </summary>
    public bool m_bLockDropAndThrough = true;

    [Header("Direction")]
    public Direction direction = Direction.Bottom;

    #region Neighbor

    public Node Neighbor(Direction dir)
    {
        return board.GetNode(i + dir.Y, j + dir.X);
    }

    public Node LeftNeighbor()
    {
        var newDirection = direction.TurnLeft();
        Assert.AreEqual(newDirection, Direction.Left);
        return Neighbor(newDirection);
    }

    public Node RightNeighbor()
    {
        var newDirection = direction.TurnRight();
        Assert.AreEqual(newDirection, Direction.Right);
        return Neighbor(newDirection);
    }

    public Node TopNeighbor()
    {
        var newDirection = direction.TurnTop();
        Assert.AreEqual(newDirection, Direction.Top);
        return Neighbor(newDirection);
    }

    public Node BottomNeighbor()
    {
        var newDirection = direction;
        Assert.AreEqual(newDirection, Direction.Bottom);
        return Neighbor(newDirection);
    }

    public Node TopLeftNeighbor()
    {
        var newDirection = direction.TurnTopLeft();
        Assert.AreEqual(newDirection, Direction.TopLeft);
        return Neighbor(newDirection);
    }

    public Node TopRightNeighbor()
    {
        var newDirection = direction.TurnTopRight();
        Assert.AreEqual(newDirection, Direction.TopRight);
        return Neighbor(newDirection);
    }

    public Node BottomLeftNeighbor()
    {
        var newDirection = direction.TurnBottomLeft();
        Assert.AreEqual(newDirection, Direction.BottomLeft);
        return Neighbor(newDirection);
    }

    public Node BottomRightNeighbor()
    {
        var newDirection = direction.TurnBottomRight();
        Assert.AreEqual(newDirection, Direction.BottomRight);
        return Neighbor(newDirection);
    }

    #endregion

    public override string ToString()
    {
        return string.Format("[Node]: i-{0}, j-{1}", i, j);
    }

    #region Item

    // Some how the function does not return a object. 
    // It always return a null pointer.
    public Item GenerateItem(ITEM_TYPE type)
    {
        Item item = null;

        switch (type)
        {
            case ITEM_TYPE.RAMDOM:
                GenerateRandomCookie();
                break;

            case ITEM_TYPE.BLANK:
            case ITEM_TYPE.RAINBOW:
            case ITEM_TYPE.COOKIE_1:
            case ITEM_TYPE.COOKIE_2:
            case ITEM_TYPE.COOKIE_3:
            case ITEM_TYPE.COOKIE_4:
            case ITEM_TYPE.COOKIE_5:
            case ITEM_TYPE.COOKIE_6:
            case ITEM_TYPE.COLUMN_BREAKER:
            case ITEM_TYPE.ROW_BREAKER:
            case ITEM_TYPE.BOMB_BREAKER:
            case ITEM_TYPE.PLANE_BREAKER:
            case ITEM_TYPE.MARSHMALLOW:
            case ITEM_TYPE.CHERRY:
                InstantiateItem(type);
                break;

            case ITEM_TYPE.GINGERBREAD_RANDOM:
                GenerateRandomGingerbread();
                break;

            case ITEM_TYPE.GINGERBREAD_1:
            case ITEM_TYPE.GINGERBREAD_2:
            case ITEM_TYPE.GINGERBREAD_3:
            case ITEM_TYPE.GINGERBREAD_4:
            case ITEM_TYPE.GINGERBREAD_5:
            case ITEM_TYPE.GINGERBREAD_6:

            case ITEM_TYPE.CHOCOLATE_1_LAYER:
            case ITEM_TYPE.CHOCOLATE_2_LAYER:
            case ITEM_TYPE.CHOCOLATE_3_LAYER:
            case ITEM_TYPE.CHOCOLATE_4_LAYER:
            case ITEM_TYPE.CHOCOLATE_5_LAYER:
            case ITEM_TYPE.CHOCOLATE_6_LAYER:
                InstantiateItem(type);
                break;

            case ITEM_TYPE.ROCK_CANDY_RANDOM:
                InstantiateItem(ITEM_TYPE.ROCK_CANDY);
                break;

            case ITEM_TYPE.ROCK_CANDY:

            case ITEM_TYPE.COLLECTIBLE_1:
            case ITEM_TYPE.COLLECTIBLE_2:
            case ITEM_TYPE.COLLECTIBLE_3:
            case ITEM_TYPE.COLLECTIBLE_4:
            case ITEM_TYPE.COLLECTIBLE_5:
            case ITEM_TYPE.COLLECTIBLE_6:
            case ITEM_TYPE.COLLECTIBLE_7:
            case ITEM_TYPE.COLLECTIBLE_8:
            case ITEM_TYPE.COLLECTIBLE_9:
            case ITEM_TYPE.COLLECTIBLE_10:
            case ITEM_TYPE.COLLECTIBLE_11:
            case ITEM_TYPE.COLLECTIBLE_12:
            case ITEM_TYPE.COLLECTIBLE_13:
            case ITEM_TYPE.COLLECTIBLE_14:
            case ITEM_TYPE.COLLECTIBLE_15:
            case ITEM_TYPE.COLLECTIBLE_16:
            case ITEM_TYPE.COLLECTIBLE_17:
            case ITEM_TYPE.COLLECTIBLE_18:
            case ITEM_TYPE.COLLECTIBLE_19:
            case ITEM_TYPE.COLLECTIBLE_20:

                InstantiateItem(type);
                break;
            case ITEM_TYPE.APPLEBOX:
                InstantiateAppleBox(type);
                break;
        }

        return item;
    }

    Item GenerateRandomCookie()
    {
        var type = LevelLoader.instance.RandomCookie();
        return InstantiateItem(type);
    }

    Item GenerateRandomGingerbread()
    {
        var type = LevelLoader.instance.RandomCookie();

        switch (type)
        {
            case ITEM_TYPE.COOKIE_1:
                InstantiateItem(ITEM_TYPE.GINGERBREAD_1);
                break;

            case ITEM_TYPE.COOKIE_2:
                InstantiateItem(ITEM_TYPE.GINGERBREAD_2);
                break;

            case ITEM_TYPE.COOKIE_3:
                InstantiateItem(ITEM_TYPE.GINGERBREAD_3);
                break;

            case ITEM_TYPE.COOKIE_4:
                InstantiateItem(ITEM_TYPE.GINGERBREAD_4);
                break;

            case ITEM_TYPE.COOKIE_5:
                InstantiateItem(ITEM_TYPE.GINGERBREAD_5);
                break;

            case ITEM_TYPE.COOKIE_6:
                InstantiateItem(ITEM_TYPE.GINGERBREAD_6);
                break;
        }

        return null;
    }

    // same as drop time.
    public float FadeDuration = 0.1f;

    Item InstantiateItem(ITEM_TYPE type)
    {
        var prefab = CFX_SpawnSystem.GetNextObject(type.ToString().ToLower(), null, false) as GameObject;
        if (prefab == null)
            return null;

        var tokens = type.ToString().Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
        int color;
        color = tokens.Length > 0 && int.TryParse(tokens[tokens.Length - 1], out color) ? color : 0;

        var piece = Instantiate(prefab) as GameObject;
        piece.transform.SetParent(gameObject.transform);
        piece.name = "Item";
        piece.transform.localPosition = board.NodeLocalPosition(i, j);
        piece.SetActive(true);

        item = piece.GetComponent<Item>();
        item.node = this;
        item.board = board;
        item.type = type;
        item.color = color;
        item.beAbleToDestroy = type == ITEM_TYPE.CHERRY ? 0 : 1;

        var effect = piece.GetComponent<ItemEffectController>();
        if (effect == null)
            piece.AddComponent<ItemEffectController>();

        if (item.HasGenerateEffect())
        {
            var sprite = item.GetComponent<SpriteRenderer>();
            sprite.DOFade(0f, 0.05f).OnComplete(() => sprite.DOFade(1f, FadeDuration));

            CFX_SpawnSystem.GetNextObject(Configure.EffectGeneratePrefab, item.transform);
        }

        return item;
    }

    void InstantiateAppleBox(ITEM_TYPE type)
    {
        if (RightNeighbor() == null || BottomNeighbor() == null || BottomRightNeighbor() == null)
        {
            Debug.Log("苹果箱位置摆放错误！");
        }

        GameObject applebox_1 = null;
        GameObject applebox_2 = null;
        GameObject applebox_3 = null;
        GameObject applebox_4 = null;
        int color = 0;

        switch (type)
        {
            case ITEM_TYPE.APPLEBOX:
                applebox_1 = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(APPLEBOX_TYPE.APPLEBOX_8), null, false)) as GameObject;
                applebox_2 = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(APPLEBOX_TYPE.NONE, true), null, false)) as GameObject;
                applebox_3 = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(APPLEBOX_TYPE.NONE, true), null, false)) as GameObject;
                applebox_4 = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(APPLEBOX_TYPE.NONE, true), null, false)) as GameObject;
                break;
        }

        if (applebox_1 != null)
        {
            applebox_1.transform.SetParent(gameObject.transform);
            applebox_1.name = "Item";
            applebox_1.transform.localPosition = board.NodeLocalPosition(i, j);
            applebox_1.GetComponent<Item>().node = this;
            applebox_1.GetComponent<Item>().board = board;
            applebox_1.GetComponent<Item>().type = type;
            applebox_1.GetComponent<Item>().color = color;
            applebox_1.GetComponent<Item>().applebox = applebox_1.GetComponent<AppleBox>();
            item = applebox_1.GetComponent<Item>();
            applebox_1.GetComponent<AppleBox>().item = applebox_1.GetComponent<Item>();
            applebox_1.GetComponent<AppleBox>().node = this;
            applebox_1.GetComponent<AppleBox>().board = board;
            applebox_1.GetComponent<AppleBox>().appleNum = 8;
            applebox_1.GetComponent<AppleBox>().beAbleToDestroy = 8;
            applebox_1.GetComponent<AppleBox>().status = APPLEBOX_TYPE.APPLEBOX_8;
            applebox_1.SetActive(true);

            applebox_2.transform.SetParent(RightNeighbor().gameObject.transform);
            applebox_2.name = "Item";
            applebox_2.transform.localPosition = board.NodeLocalPosition(i, j + 1);
            applebox_2.GetComponent<Item>().node = RightNeighbor();
            applebox_2.GetComponent<Item>().board = RightNeighbor().board;
            applebox_2.GetComponent<Item>().type = type;
            applebox_2.GetComponent<Item>().color = color;
            applebox_2.GetComponent<Item>().applebox = applebox_1.GetComponent<AppleBox>();
            RightNeighbor().item = applebox_2.GetComponent<Item>();
            applebox_2.SetActive(true);

            applebox_3.transform.SetParent(BottomNeighbor().gameObject.transform);
            applebox_3.name = "Item";
            applebox_3.transform.localPosition = board.NodeLocalPosition(i + 1, j);
            applebox_3.GetComponent<Item>().node = BottomNeighbor();
            applebox_3.GetComponent<Item>().board = BottomNeighbor().board;
            applebox_3.GetComponent<Item>().type = type;
            applebox_3.GetComponent<Item>().color = color;
            applebox_3.GetComponent<Item>().applebox = applebox_1.GetComponent<AppleBox>();
            BottomNeighbor().item = applebox_3.GetComponent<Item>();
            applebox_3.SetActive(true);

            applebox_4.transform.SetParent(BottomRightNeighbor().gameObject.transform);
            applebox_4.name = "Item";
            applebox_4.transform.localPosition = board.NodeLocalPosition(i, j + 1);
            applebox_4.GetComponent<Item>().node = BottomRightNeighbor();
            applebox_4.GetComponent<Item>().board = BottomRightNeighbor().board;
            applebox_4.GetComponent<Item>().type = type;
            applebox_4.GetComponent<Item>().color = color;
            applebox_4.GetComponent<Item>().applebox = applebox_1.GetComponent<AppleBox>();
            BottomRightNeighbor().item = applebox_4.GetComponent<Item>();
            applebox_4.SetActive(true);

            board.appleBoxes.Add(applebox_1.GetComponent<AppleBox>());
        }
    }

    #endregion

    #region Match

    public List<Item> FindSquareMatches()
    {
        var list = new List<Item>();
        if (item == null || !item.Matchable())
        {
            return null;
        }
        if (IsSameColorItem(TopNeighbor()) && IsSameColorItem(TopRightNeighbor()) && IsSameColorItem(RightNeighbor()))
        {
            list.Add(item);
            list.Add(TopNeighbor().item);
            list.Add(TopRightNeighbor().item);
            list.Add(RightNeighbor().item);
        }
        else if (IsSameColorItem(TopNeighbor()) && IsSameColorItem(TopLeftNeighbor()) && IsSameColorItem(LeftNeighbor()))
        {
            list.Add(item);
            list.Add(TopNeighbor().item);
            list.Add(TopLeftNeighbor().item);
            list.Add(LeftNeighbor().item);
        }
        else if (IsSameColorItem(RightNeighbor()) && IsSameColorItem(BottomRightNeighbor()) && IsSameColorItem(BottomNeighbor()))
        {
            list.Add(item);
            list.Add(RightNeighbor().item);
            list.Add(BottomRightNeighbor().item);
            list.Add(BottomNeighbor().item);
        }
        else if (IsSameColorItem(LeftNeighbor()) && IsSameColorItem(BottomLeftNeighbor()) && IsSameColorItem(BottomNeighbor()))
        {
            list.Add(item);
            list.Add(LeftNeighbor().item);
            list.Add(BottomLeftNeighbor().item);
            list.Add(BottomNeighbor().item);
        }

        return list;
    }

    private bool IsSameColorItem(Node checkNode)
    {
        if (checkNode == null || checkNode.item == null)
        {
            return false;
        }
        return checkNode.item.Matchable() && checkNode.item.color == item.color;
    }

    // find matches at a node
    public List<Item> FindMatches(FIND_DIRECTION direction = FIND_DIRECTION.NONE, int matches = 3)
    {
        var list = new List<Item>();
        var countedNodes = new Dictionary<int, Item>();

        if (item == null || !item.Matchable())
        {
            return null;
        }

        if (direction != FIND_DIRECTION.COLUMN)
        {
            countedNodes = FindMoreMatches(item.color, countedNodes, FIND_DIRECTION.ROW);
        }

        if (countedNodes.Count < matches)
        {
            countedNodes.Clear();
        }

        if (direction != FIND_DIRECTION.ROW)
        {
            countedNodes = FindMoreMatches(item.color, countedNodes, FIND_DIRECTION.COLUMN);
        }

        if (countedNodes.Count < matches)
        {
            countedNodes.Clear();
        }

        foreach (KeyValuePair<int, Item> entry in countedNodes)
        {
            list.Add(entry.Value);
        }

        return list;
    }

    // helper function to find matches
    Dictionary<int, Item> FindMoreMatches(int color, Dictionary<int, Item> countedNodes, FIND_DIRECTION direction)
    {
        if (item == null || item.destroying)
        {
            return countedNodes;
        }

        if (item.color == color && !countedNodes.ContainsValue(item) && item.Matchable() && item.node != null)
        {
            countedNodes.Add(item.node.OrderOnBoard(), item);

            if (direction == FIND_DIRECTION.ROW)
            {
                if (LeftNeighbor() != null)
                {
                    countedNodes = LeftNeighbor().FindMoreMatches(color, countedNodes, FIND_DIRECTION.ROW);
                }

                if (RightNeighbor() != null)
                {
                    countedNodes = RightNeighbor().FindMoreMatches(color, countedNodes, FIND_DIRECTION.ROW);
                }
            }
            else if (direction == FIND_DIRECTION.COLUMN)
            {
                if (TopNeighbor() != null)
                {
                    countedNodes = TopNeighbor().FindMoreMatches(color, countedNodes, FIND_DIRECTION.COLUMN);
                }

                if (BottomNeighbor() != null)
                {
                    countedNodes = BottomNeighbor().FindMoreMatches(color, countedNodes, FIND_DIRECTION.COLUMN);
                }
            }
        }

        return countedNodes;
    }

    #endregion

    #region Utility

    // return the order base on i and j
    public int OrderOnBoard()
    {
        return (i * LevelLoader.instance.column + j);
    }

    #endregion

    #region Type

    public bool IsInCurrentPage()
    {
        if (board.SkipPageChecking)
            return true;

        Vector2 centerCoord = LevelLoader.GetInstance().GetPageCoord(board.CurrentPage);
        int centerRow = (int)centerCoord.x;
        int centerCol = (int)centerCoord.y;

        int offset = 0;
        if (i > (LevelLoader.C_PageRow / 2 + 1))
            offset = 1;
        bool iUp = i >= centerRow - LevelLoader.C_PageRow / 2 - offset;
        bool iDown = i <= centerRow + LevelLoader.C_PageRow / 2;

        bool jL = j >= centerCol - LevelLoader.C_PageColumn / 2;
        bool jR = j <= centerCol + LevelLoader.C_PageColumn / 2;
        
        if (iUp && iDown && jL && jR)
        {
            return true;
        }
        return false;
    }
    
    public bool CanStoreItem()
    {
        if (tile != null)
        {
            if (tile.type == TILE_TYPE.DARK || tile.type == TILE_TYPE.LIGHT)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanGoThrough()
    {
        if (tile == null || tile.type == TILE_TYPE.NONE || m_bLockDropAndThrough)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool CanGenerateNewItem()
    {
        if (CanStoreItem() && CanDropIn() && !HasTopBaffle())
        {
            for (int row = i - 1; row >= 0; row--)
            {
                Node upNode = board.GetNode(row, j);

                if (upNode != null)
                {
                    if (upNode.CanGoThrough() == false)
                    {
                        return false;
                    }
                    else
                    {
                        if (upNode.item != null)
                        {
                            if (upNode.item.type != ITEM_TYPE.BLANK && upNode.item.Droppable() == false || !upNode.CanDropIn() || upNode.HasTopBaffle())
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CanChangeType()
    {
        if (cage != null
            || ice != null
            || jelly != null
            || packagebox != null
            || !IsInCurrentPage()
        )
        {
            return false;
        }
        return true;
    }

    public bool CanDropIn()
    {
        if (cage != null
            || ice != null
            || jelly != null
            || packagebox != null
            || m_bLockDropAndThrough
        )
        {
            return false;
        }
        return true;
    }

    public bool HasTopBaffle()
    {
        if (TopNeighbor() != null && TopNeighbor().bafflebottom != null)
        {
            return true;
        }
        return false;
    }

    public bool isNodeBlank()
    {
        if (ice != null || jelly != null || packagebox != null || cage != null)
        {
            return false;
        }

        if (item != null && item.type == ITEM_TYPE.BLANK)
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Node

    // get source node of an empty node
    public Node GetSourceNode()
    {
        Node source = null;

        // top
        Node top = board.GetNode(i - 1, j);
        if (top != null && top.bafflebottom == null)
        {
            if (top.tile != null && top.tile.type == TILE_TYPE.PASSTHROUGH || top.item != null && top.item.type == ITEM_TYPE.BLANK && top.CanGoThrough() && top.CanDropIn())
            {
                source = top.GetSourceNode();
            }
        }

        if (source != null)
        {
            return source;
        }

        // top left
        Node left = board.GetNode(i - 1, j - 1);
        if (left != null && left.bafflebottom == null)
        {
            if (this.LeftNeighbor() == null || (this.LeftNeighbor() != null && this.LeftNeighbor().baffleright == null))
            {
                if (left.tile != null && left.tile.type == TILE_TYPE.PASSTHROUGH || left.item != null && left.item.type == ITEM_TYPE.BLANK && left.CanGoThrough() && left.CanDropIn())
                {
                    source = left.GetSourceNode();
                }
                else
                {
                    if (left.item != null && left.item.type != ITEM_TYPE.BLANK && left.item.Movable())
                    {
                        source = left;
                    }
                }
            }
        }

        if (source != null)
        {
            return source;
        }

        // top right
        Node right = board.GetNode(i - 1, j + 1);
        if (right != null && right.bafflebottom == null)
        {
            if (this.baffleright == null)
            {
                if (right.tile != null && right.tile.type == TILE_TYPE.PASSTHROUGH || right.item != null && right.item.type == ITEM_TYPE.BLANK && right.CanGoThrough() &&
                    right.CanDropIn())
                {
                    source = right.GetSourceNode();
                }
                else
                {
                    if (right.item != null && right.item.type != ITEM_TYPE.BLANK && right.item.Movable())
                    {
                        source = right;
                    }
                }
            }
        }

        return source;
    }

    // get move path from an empty node to source node
    public List<Vector3> GetMovePath()
    {
        List<Vector3> path = new List<Vector3>();

        path.Add(board.NodeLocalPosition(i, j) + board.transform.position);

        // top
        Node top = board.GetNode(i - 1, j);
        if (top != null && top.bafflebottom == null)
        {
            if (top.item != null && top.item.type == ITEM_TYPE.BLANK && top.CanGoThrough() && top.CanDropIn())
            {
                if (top.GetSourceNode() != null)
                {
                    path.AddRange(top.GetMovePath());
                    return path;
                }
            }
        }

        // left
        Node left = board.GetNode(i - 1, j - 1);
        if (left != null && left.bafflebottom == null)
        {
            if (this.LeftNeighbor() == null || (this.LeftNeighbor() != null && this.LeftNeighbor().baffleright == null))
            {
                if (left.item != null && left.item.type == ITEM_TYPE.BLANK && left.CanGoThrough() && left.CanDropIn())
                {
                    if (left.GetSourceNode() != null)
                    {
                        path.AddRange(left.GetMovePath());
                        return path;
                    }
                }
                else
                {
                    if (left.item != null && left.item.type != ITEM_TYPE.BLANK && left.item.Movable())
                    {
                        return path;
                    }
                }
            }
        }

        // right
        Node right = board.GetNode(i - 1, j + 1);
        if (right != null && right.bafflebottom == null)
        {
            if (this.baffleright == null)
            {
                if (right.item != null && right.item.type == ITEM_TYPE.BLANK && right.CanGoThrough() && right.CanDropIn())
                {
                    if (right.GetSourceNode() != null)
                    {
                        path.AddRange(right.GetMovePath());
                        return path;
                    }
                }
                else
                {
                    if (right.item != null && right.item.type != ITEM_TYPE.BLANK && right.item.Movable())
                    {
                        return path;
                    }
                }
            }
        }

        return path;
    }

    #endregion

    #region Waffle

    public void WaffleExplode()
    {
        if (waffle != null && item != null & (item.IsCookie() || item.IsBreaker(item.type) || item.type == ITEM_TYPE.RAINBOW))
        {
            AudioManager.instance.WaffleExplodeAudio();
            if (waffle.type == WAFFLE_TYPE.WAFFLE_1)
            {
                Destroy(waffle.gameObject);
                waffle = null;
            }
            else
            {
                var prefab = CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(waffle.type - 1), null, false);
                waffle.gameObject.GetComponent<SpriteRenderer>().sprite = prefab.GetComponent<SpriteRenderer>().sprite;
                waffle.type = waffle.type - 1;
            }
        }
    }

    #endregion

    #region Cage

    public void CageExplode()
    {
        if (cage == null)
            return;

        var effect = CFX_SpawnSystem.GetNextObject((cage.type == CAGE_TYPE.CAGE_2) ? Configure.EffectCageNormalDestroy : Configure.EffectCageFinalDestroy, item.transform);
        var duration = effect.GetComponent<CFX_AutoDestruct>().Duration;
        AudioManager.instance.CageExplodeAudio();

        CollectCage();

        StartCoroutine(item.ResetDestroying());
    }

    private void CollectCage()
    {
        if (cage.type == CAGE_TYPE.CAGE_1)
        {
            board.CollectItem(item);

            Destroy(cage.gameObject);
            cage = null;
        }
        else if (cage.type == CAGE_TYPE.CAGE_2)
        {
            var prefab = CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(CAGE_TYPE.CAGE_1), null, false);
            cage.gameObject.GetComponent<SpriteRenderer>().sprite = prefab.GetComponent<SpriteRenderer>().sprite;
            cage.type = CAGE_TYPE.CAGE_1;
        }
    }

    #endregion

    #region Ice


    public void IceExplode()
    {
        if (ice == null)
            return;

        var effect = CFX_SpawnSystem.GetNextObject((ice.type == ICE_TYPE.ICE_2) ? Configure.EffectIceNormalDestroy : Configure.EffectIceFinalDestroy, item.transform);
        var duration = effect.GetComponent<CFX_AutoDestruct>().Duration;
        AudioManager.instance.CageExplodeAudio();

        CollectIce();
        StartCoroutine(item.ResetDestroying());
    }

    private void CollectIce()
    {
        if (ice.type == ICE_TYPE.ICE_1)
        {
            Destroy(ice.gameObject);
            ice = null;
        }
        else if (ice.type == ICE_TYPE.ICE_2)
        {
            var prefab = CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(ice.type), null, false);
            ice.gameObject.GetComponent<SpriteRenderer>().sprite = prefab.GetComponent<SpriteRenderer>().sprite;
            ice.type = ICE_TYPE.ICE_1;
        }
    }

    #endregion

    #region Jelly

    public bool JellyExplode()
    {
        if (jelly == null)
        {
            return false;
        }

        CFX_SpawnSystem.GetNextObject(Configure.EffectJelly, item.transform);
        AudioManager.instance.CageExplodeAudio();

        if (jelly.type == JELLY_TYPE.JELLY_1)
        {
            Destroy(jelly.gameObject);
            jelly = null;
            if (item != null && item.type == ITEM_TYPE.CHERRY)
                return false;
        }
        else
        {
            var prefab = CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(jelly.type - 1), null, false);
            jelly.gameObject.GetComponent<SpriteRenderer>().sprite = prefab.GetComponent<SpriteRenderer>().sprite;
            jelly.type = jelly.type - 1;
        }

        StartCoroutine(item.ResetDestroying());
        return true;
    }

    #endregion

    #region PackageBox

    public void PackageBoxExplode()
    {
        if (packagebox == null)
            return;

        var effect = CFX_SpawnSystem.GetNextObject((packagebox.type != PACKAGEBOX_TYPE.PACKAGEBOX_1) ? Configure.EffectPackageBoxNormal : Configure.EffectPackageBoxFinal, item.transform);
        var duration = effect.GetComponent<CFX_AutoDestruct>().Duration;
        AudioManager.instance.CageExplodeAudio();

        CollectPackageBox();

        StartCoroutine(item.ResetDestroying());
    }

    private void CollectPackageBox()
    {
        if (packagebox.type == PACKAGEBOX_TYPE.PACKAGEBOX_1)
        {
            board.CollectItem(item);

            Destroy(packagebox.gameObject);
            packagebox = null;
        }
        else
        {
            var prefab = CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(packagebox.type - 1), null, false);
            packagebox.gameObject.GetComponent<SpriteRenderer>().sprite = prefab.GetComponent<SpriteRenderer>().sprite;
            packagebox.type = packagebox.type - 1;
        }
    }

    #endregion


    #region Booster

    public void AddOvenBoosterActive()
    {
        ovenActive = Instantiate(Resources.Load(Configure.EffectBoosterActive)) as GameObject;

        ovenActive.transform.localPosition = board.NodeLocalPosition(i, j);
    }

    public void RemoveOvenBoosterActive()
    {
        Destroy(ovenActive);

        ovenActive = null;
    }

    #endregion


    #region Grass

    public void ChangeToGrass()
    {
        if (tile == null
            || (tile.type != TILE_TYPE.DARK && tile.type != TILE_TYPE.LIGHT)
            || (cage != null)
            || (jelly != null)
            || (packagebox != null)
            )
        {
            return;
        }

        if (grass == null)
        {
            var grassPrefab = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.GrassPrefab, null, false));
            if (grassPrefab)
            {
                grassPrefab.transform.SetParent(gameObject.transform);
                grassPrefab.name = "Grass";
                grassPrefab.transform.localPosition = board.NodeLocalPosition(i, j);
                grassPrefab.GetComponent<Grass>().type = 0;
                grassPrefab.GetComponent<Grass>().node = this;
                grassPrefab.SetActive(true);

                grass = grassPrefab.GetComponent<Grass>();

                int order = -1;

                for (int k = 0; k < LevelLoader.instance.targetList.Count; k++)
                {
                    if (LevelLoader.instance.targetList[k].Type == TARGET_TYPE.GRASS
                        && board.targetLeftList[k] > 0
                    )
                    {
                        board.targetLeftList[k]--;
                        order = k;
                        break;
                    }
                }
                if (order != -1)
                {
                    board.UITarget.UpdateTargetAmount(order);
                }
            }
        }
    }

    public bool IsGrass(bool special = false)
    {
        if (special)
        {
            return true;
        }
        if (grass != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region Destroy


    #endregion
}
