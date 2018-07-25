using March.Core.WindowManager;
using qy;
using qy.config;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using GuideManager = March.Core.Guide.GuideManager;
using Random = UnityEngine.Random;
using System.Linq;

[Serializable]
public class DropCountConfig
{
    public int Count;
    public Type PopupType;
}

public class Board : MonoBehaviour
{
    /// <summary>
    /// container that include main camera and mask component
    /// </summary>
    [SerializeField]
    public GameObject m_CPageMoveContainer;
    
    [Header("Nodes")]
    public List<Node> nodes = new List<Node>();

    [Header("Board variables")]
    public GAME_STATE state;
    public bool lockSwap;
    public int moveLeft;
    public int dropTime;
    public int score;
    public int star;
    public List<int> targetLeftList;

    [Header("Booster")]
    public BOOSTER_TYPE booster;
    public List<Item> boosterItems = new List<Item>();
    public Item ovenTouchItem;

    [Header("Check")]
    public int destroyingItems;
    public int droppingItems;
    public int flyingItems;
    public int playingAnimation;
    public int matching;
    public int specialDestroying;
    public GAME_STATE originalStateInSDestroying;
    public bool isFirstMove;

    public bool needIncreaseBubble = false;

    [Header("")]
    public bool movingGingerbread;
    public bool generatingGingerbread;
    public bool skipGenerateGingerbread;
    public bool showingInspiringPopup;
    public int skipGingerbreadCount;

    [Header("Item Lists")]
    public List<Item> changingList;
    public List<Item> sameColorList;

    [Header("Swap")]
    public Item touchedItem;
    public Item swappedItem;
    public Item clickedItem;

    [Header("UI")]
    public UITarget UITarget;
    public UITop UITop;

    [Header("Hint")]
    public bool isHintShowing;
    public List<Item> hintItems = new List<Item>();

    [Header("AppleBox")]
    public List<AppleBox> appleBoxes = new List<AppleBox>();

    [Header("PlanePlus")]
    public int planePlusNum = 0;

    private Vector3 firstNodePosition;

    [Header("data")]
    public int allstep;
    public int FiveMoreTimes;
    public int minWinGold;
    public int winGold;

    [Header("Cache")]
    public List<GameObject> CookiesList;

    [Header("EditorMode")]
    public TextAsset LevelText;

    ///current page index
    private int m_ICurrentPageIndex;
    /// <summary>
    /// skip page check trigger, available before GenerateNoMatches called
    /// </summary>
    private bool m_bSkipPageChecking;

    public bool SkipPageChecking
    {
        get { return m_bSkipPageChecking; }
    }
    
    public int CurrentPage
    {
        get { return m_ICurrentPageIndex; }
    }

    void Awake()
    {
        if (LevelLoader.instance.level == 0)
        {
            LevelLoader.instance.LoadLevel();
        }
    }

    void Start()
    {
        state = GAME_STATE.PREPARING_LEVEL;
        moveLeft = LevelLoader.instance.moves;
        isFirstMove = true;
        FiveMoreTimes = 0;
        m_bSkipPageChecking = true;

        targetLeftList.Clear();
        for (int i = 0; i < LevelLoader.instance.targetList.Count; i++)
        {
            targetLeftList.Add(LevelLoader.instance.targetList[i].Amount);
        }

        allstep = 0;

        string itemid = (1000000 + LevelLoader.instance.level).ToString();
        //LevelItem levelconfig = LevelLoader.instance.LevelConfig.GetItemByID(itemid);
        MatchLevelItem levelconfig = GameMainManager.Instance.configManager.matchLevelConfig.GetItem(itemid);
        winGold = 0;
        if (levelconfig != null)
        {
            winGold = levelconfig.coin;
        }
        else
        {
            Debug.LogError("matchlevel表中找不当当前关卡配置！");
        }
        minWinGold = winGold;

        GenerateBoard();

        BeginBooster();

        // reset color of the random cookie to make sure there is no match in the original board
        GenerateNoMatches();

        // open target popup
        TargetPopup();
    }

    [ContextMenu("LoadLevel")]
    public void LoadLevel()
    {
        LevelLoader.instance.LoadLevel(LevelText.text);

        CleanupLevel();
        GenerateBoard();
    }

    [ContextMenu("CleanupLevel")]
    public void CleanupLevel()
    {
        firstNodePosition = Vector3.zero;
        nodes.Clear();
        for (var i = transform.childCount - 1; i >= 0; --i)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    void Update()
    {
        if (state == GAME_STATE.WAITING_USER_SWAP && lockSwap == false && moveLeft > 0)
        {
            if (needIncreaseBubble)
            {
                needIncreaseBubble = false;
                IncreaseBubble();
            }
            else
            {
                // no booster
                if (booster == BOOSTER_TYPE.NONE)
                {
                    // mouse down
                    if (Input.GetMouseButtonDown(0))
                    {
                        // hit the collier
                        Collider2D hit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        if (hit != null)
                        {
                            Item item = hit.gameObject.GetComponent<Item>();
                            if (item != null && item.node != null)
                            {
                                if (item.Exchangeable(SWAP_DIRECTION.NONE))
                                {
                                    ClickChoseItem(item);

                                }
                                else
                                {
                                    CancelChoseItem();
                                }
                            }
                            else
                            {
                                CancelChoseItem();
                            }
                        }
                        else
                        {
                            CancelChoseItem();
                        }
                    }
                    // mouse up
                    else if (Input.GetMouseButtonUp(0))
                    {
                        Collider2D hit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        if (hit != null)
                        {
                            var item = hit.gameObject.GetComponent<Item>();
                            if (item != null)
                            {
                                item.drag = false;
                                item.swapDirection = Vector3.zero;
                            }
                        }
                    }
                }
                // use booster
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Collider2D hit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        if (hit != null)
                        {
                            var item = hit.gameObject.GetComponent<Item>();
                            if (item != null)
                            {
                                DestroyBoosterItems(item);
                            }
                        }
                    }
                }
            }
        }
    }
    #region Board

    void GenerateBoard()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        nodes.Clear();
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var order = NodeOrder(i, j);

                GameObject node = Instantiate(Resources.Load(Configure.NodePrefab)) as GameObject;
                node.transform.SetParent(gameObject.transform, false);
                node.name = "Node " + order;
                node.GetComponent<Node>().board = this;
                node.GetComponent<Node>().i = i;
                node.GetComponent<Node>().j = j;

                nodes.Add(node.GetComponent<Node>());
            }
        }

        GenerateTileLayer();

        GenerateGrassLayer();

        GenerateWaffleLayer();

        GenerateItemLayer();

        GenerateJellyLayer();

        GeneratePackageBoxLayer();

        GenerateCageLayer();

        GenerateIceLayer();

        GenerateBaffleLayer();

        GenerateCollectibleBoxByColumn();
        GenerateCollectibleBoxByNode();

        initPage();
    }

    void GenerateTileLayer()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;
        var tileLayerData = LevelLoader.instance.tileLayerData;
        if (tileLayerData.Count == 0)
            return;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var order = NodeOrder(i, j);
                
                GameObject tile = null;
                if (tileLayerData[order] == TILE_TYPE.NONE || tileLayerData[order] == TILE_TYPE.PASSTHROUGH)
                    tile = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(TILE_TYPE.NONE, true), null, false)) as GameObject;

                if ((i % 2 + j % 2) % 2 == 0)
                {
                    if (tile == null)
                        tile = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(TILE_TYPE.LIGHT, true), null, false)) as GameObject;
                }
                else
                {
                    if (tile == null)
                        tile = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(TILE_TYPE.DARK, true), null, false)) as GameObject;
                }

                if (tile)
                {
                    tile.transform.SetParent(nodes[order].gameObject.transform);
                    tile.name = "Tile";

                    if (tile.GetComponent<SpriteRenderer>())
                        tile.GetComponent<SpriteRenderer>().sortingLayerName = SortingLayers.Tile;

                    tile.transform.localPosition = NodeLocalPosition(i, j);
                    tile.GetComponent<Tile>().type = tileLayerData[order];
                    tile.GetComponent<Tile>().node = nodes[order];
                    tile.SetActive(true);

                    nodes[order].tile = tile.GetComponent<Tile>();
                }
            }
        }
    }

    void GenerateGrassLayer()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;
        var grassLayerData = LevelLoader.instance.grassLayerData;
        if (grassLayerData.Count == 0)
            return;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var order = NodeOrder(i, j);

                GameObject grass = null;
                if (grassLayerData.Count == row * column)//兼容旧关卡格式
                {
                    switch (grassLayerData[order])
                    {
                        case GRASS_TYPE.GRASS_1:
                            grass = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.GrassPrefab, null, false));
                            break;
                        case GRASS_TYPE.NONE:
                            grass = null;
                            break;
                    }
                }
                if (grass)
                {
                    grass.transform.SetParent(nodes[order].gameObject.transform);
                    grass.name = "Grass";
                    grass.transform.localPosition = NodeLocalPosition(i, j);
                    grass.GetComponent<Grass>().type = 0;
                    grass.GetComponent<Grass>().node = nodes[order];
                    grass.SetActive(true);

                    nodes[order].grass = grass.GetComponent<Grass>();
                }
            }
        }
    }

    // waffle
    void GenerateWaffleLayer()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;
        var waffleLayerData = LevelLoader.instance.waffleLayerData;
        if (waffleLayerData.Count == 0)
            return;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var order = NodeOrder(i, j);

                GameObject waffle = null;

                switch (waffleLayerData[order])
                {
                    case WAFFLE_TYPE.WAFFLE_1:
                        waffle = Instantiate(Resources.Load(Configure.Waffle1)) as GameObject;
                        break;
                    case WAFFLE_TYPE.WAFFLE_2:
                        waffle = Instantiate(Resources.Load(Configure.Waffle2)) as GameObject;
                        break;
                    case WAFFLE_TYPE.WAFFLE_3:
                        waffle = Instantiate(Resources.Load(Configure.Waffle3)) as GameObject;
                        break;
                }

                if (waffle)
                {
                    waffle.transform.SetParent(nodes[order].gameObject.transform);
                    waffle.name = "Waffle";
                    waffle.transform.localPosition = NodeLocalPosition(i, j);
                    waffle.GetComponent<Waffle>().type = waffleLayerData[order];
                    waffle.GetComponent<Waffle>().node = nodes[order];

                    SpriteRenderer waffleRender = waffle.GetComponent<SpriteRenderer>();
                    waffleRender.sortingLayerName = SortingLayers.Waffle;
                    waffleRender.sortingOrder = 0;
                    waffleRender.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

                    nodes[order].waffle = waffle.GetComponent<Waffle>();
                }
            }
        }
    }

    void GenerateItemLayer()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;
        var itemLayerData = LevelLoader.instance.itemLayerData;
        if (itemLayerData.Count == 0)
            return;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var order = NodeOrder(i, j);

                if (nodes[order].CanStoreItem())
                {
                    nodes[order].GenerateItem(itemLayerData[order]);

                    // add mask
                    //var mask = Instantiate(Resources.Load(Configure.Mask())) as GameObject;
                    //mask.transform.SetParent(nodes[order].transform);
                    //mask.transform.localPosition = NodeLocalPosition(i, j);
                    //mask.name = "Mask";
                }
            }
        }
    }

    void GenerateCageLayer()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        int beabletodestroy = 1;
        var cageLayerData = LevelLoader.instance.cageLayerData;
        if (cageLayerData.Count == 0)
            return;


        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var order = NodeOrder(i, j);

                GameObject cage = null;
                if (cageLayerData[order] != CAGE_TYPE.NONE)
                    cage = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(cageLayerData[order]), null, false)) as GameObject;

                if (cage)
                {
                    cage.transform.SetParent(nodes[order].gameObject.transform);
                    cage.name = "Cage";
                    cage.transform.localPosition = NodeLocalPosition(i, j);
                    cage.GetComponent<Cage>().type = cageLayerData[order];
                    cage.GetComponent<Cage>().node = nodes[order];
                    cage.SetActive(true);

                    if (nodes[order].item != null)
                    {
                        nodes[order].item.beAbleToDestroy += beabletodestroy;
                    }

                    nodes[order].cage = cage.GetComponent<Cage>();
                }
            }
        }
    }

    void GenerateJellyLayer()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;
        int beabletodestroy = 1;
        var jellyLayerData = LevelLoader.instance.jellyLayerData;
        if (jellyLayerData.Count == 0)
            return;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var order = NodeOrder(i, j);

                GameObject jelly = null;

                if (jellyLayerData.Count == row * column)//兼容旧关卡格式
                {
                    if (jellyLayerData[order] != JELLY_TYPE.NONE)
                    {
                        jelly = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(jellyLayerData[order]), null, false)) as GameObject;
                        beabletodestroy = (int)jellyLayerData[order];
                    }
                }

                if (jelly)
                {
                    jelly.transform.SetParent(nodes[order].gameObject.transform);
                    jelly.name = "Jelly";
                    jelly.transform.localPosition = NodeLocalPosition(i, j);
                    jelly.GetComponent<Jelly>().type = jellyLayerData[order];
                    jelly.GetComponent<Jelly>().node = nodes[order];
                    jelly.SetActive(true);

                    if (nodes[order].item != null)
                    {
                        nodes[order].item.beAbleToDestroy += beabletodestroy;
                    }

                    nodes[order].jelly = jelly.GetComponent<Jelly>();
                }
            }
        }
    }

    void GeneratePackageBoxLayer()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;
        int beabletodestroy = 1;
        var packageboxLayerData = LevelLoader.instance.packageboxLayerData;
        if (packageboxLayerData.Count == 0)
            return;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var order = NodeOrder(i, j);

                GameObject packagebox = null;

                if (packageboxLayerData.Count == row * column)//兼容旧关卡格式
                {
                    if (packageboxLayerData[order] != PACKAGEBOX_TYPE.NONE)
                    {
                        packagebox = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(packageboxLayerData[order]), null, false));
                        beabletodestroy = (int)packageboxLayerData[order];
                    }
                }

                if (packagebox)
                {
                    packagebox.transform.SetParent(nodes[order].gameObject.transform);
                    packagebox.name = "PackageBox";
                    packagebox.transform.localPosition = NodeLocalPosition(i, j);
                    packagebox.GetComponent<PackageBox>().type = packageboxLayerData[order];
                    packagebox.GetComponent<PackageBox>().node = nodes[order];
                    packagebox.SetActive(true);

                    if (nodes[order].item != null)
                    {
                        nodes[order].item.beAbleToDestroy += beabletodestroy;
                    }

                    nodes[order].packagebox = packagebox.GetComponent<PackageBox>();
                }
            }
        }
    }

    void GenerateIceLayer()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;
        var iceLayerData = LevelLoader.instance.iceLayerData;
        if (iceLayerData.Count == 0)
            return;

        int beabletodestroy = 1;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var order = NodeOrder(i, j);

                GameObject ice = null;

                if (iceLayerData.Count == row * column) //兼容旧关卡格式
                {
                    if (iceLayerData[order] != ICE_TYPE.NONE)
                    {
                        ice = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(iceLayerData[order]), null, false));
                        beabletodestroy = (int)iceLayerData[order];
                    }
                }
                if (ice)
                {
                    ice.transform.SetParent(nodes[order].gameObject.transform);
                    ice.name = "Ice";
                    ice.transform.localPosition = NodeLocalPosition(i, j);
                    ice.GetComponent<Ice>().type = iceLayerData[order];
                    ice.GetComponent<Ice>().node = nodes[order];
                    ice.SetActive(true);

                    if (nodes[order].item != null)
                    {
                        nodes[order].item.beAbleToDestroy += beabletodestroy;
                    }

                    nodes[order].ice = ice.GetComponent<Ice>();
                }
            }
        }
    }

    void GenerateBaffleLayer()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        var baffleRightLayerData = LevelLoader.instance.baffleRightLayerData;
        if (baffleRightLayerData == null || baffleRightLayerData.Count != row * column)
            return;

        var baffleBottomLayerData = LevelLoader.instance.baffleBottomLayerData;
        if (baffleBottomLayerData == null || baffleBottomLayerData.Count != row * column)
            return;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var order = NodeOrder(i, j);
                GenerateBaffle(i, j, order, baffleRightLayerData, BAFFLE_TYPE.BAFFLE_RIGHT);
                GenerateBaffle(i, j, order, baffleBottomLayerData, BAFFLE_TYPE.BAFFLE_BOTTOM);
            }
        }
    }

    private void GenerateBaffle(int i, int j, int order, List<BAFFLE_TYPE> baffleLayerData, BAFFLE_TYPE baffleType)
    {
        if (baffleLayerData[order] != baffleType)
            return;

        var baffle = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(baffleLayerData[order]), null, false));
        baffle.transform.SetParent(nodes[order].gameObject.transform);
        baffle.name = baffleType.ToString();
        baffle.transform.localPosition = NodeLocalPosition(i, j);
        baffle.GetComponent<Baffle>().type = baffleLayerData[order];
        baffle.GetComponent<Baffle>().node = nodes[order];
        baffle.SetActive(true);

        if (baffleType == BAFFLE_TYPE.BAFFLE_RIGHT)
            nodes[order].baffleright = baffle.GetComponent<Baffle>();
        else if (baffleType == BAFFLE_TYPE.BAFFLE_BOTTOM)
            nodes[order].bafflebottom = baffle.GetComponent<Baffle>();
    }

    void GenerateCollectibleBoxByColumn()
    {
        bool hasCollectible = false;
        for (int i = 0; i < LevelLoader.instance.targetList.Count; i++)
        {
            if (LevelLoader.instance.targetList[i].Type == TARGET_TYPE.COLLECTIBLE)
            {
                hasCollectible = true;
                break;
            }
        }
        if (!hasCollectible)
            return;

        var row = LevelLoader.instance.row;

        foreach (var column in LevelLoader.instance.collectibleCollectColumnMarkers)
        {
            var node = GetNode(row - 1, column);

            if (node != null && node.CanStoreItem())
            {
                var box = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.CollectorPrefab, null, false)) as GameObject;
                if (box)
                {
                    box.transform.SetParent(node.gameObject.transform);
                    box.name = "Box";
                    box.transform.localPosition = NodeLocalPosition(node.i, node.j) + new Vector3(0, -1 * NodeSize() + 0.4f, 0);
                    box.transform.localScale = new Vector3(0.7f, 0.7f, 1.0f);
                    box.SetActive(true);
                }
            }
        }
    }

    void GenerateCollectibleBoxByNode()
    {
        bool hasCollectible = false;
        for (int i = 0; i < LevelLoader.instance.targetList.Count; i++)
        {
            if (LevelLoader.instance.targetList[i].Type == TARGET_TYPE.COLLECTIBLE)
            {
                hasCollectible = true;
                break;
            }
        }
        if (!hasCollectible)
        {
            return;
        }

        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var order = NodeOrder(i, j);

                if (LevelLoader.instance.collectibleCollectNodeMarkers.Contains(order))
                {
                    var node = GetNode(i, j);

                    if (node != null)
                    {
                        var box = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.CollectorPrefab, null, false)) as GameObject;
                        if (box)
                        {
                            box.transform.SetParent(node.gameObject.transform);
                            box.name = "Box";
                            box.transform.localPosition = NodeLocalPosition(node.i, node.j) + new Vector3(0, -1 * NodeSize() + 0.4f, 0);
                            box.transform.localScale = new Vector3(0.7f, 0.7f, 1.0f);
                            box.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Begin

    void BeginBooster()
    {
        for (int i = LevelLoader.instance.beginItemList.Count - 1; i >= 0; i--)
        {
            BoosterEffect(LevelLoader.instance.beginItemList[i]);
        }
    }

    public void BoosterEffect(string itemId)
    {
        var items = GetListItems();
        var cookies = new List<Item>();

        foreach (var item in items)
        {
            if (item != null && item.IsCookie() && item.Movable())
            {
                cookies.Add(item);
            }
        }
        var cookie = cookies[Random.Range(0, cookies.Count - 1)];

        if (itemId == "200003") //rainbow
        {
            cookie.ChangeToRainbow();
        }
        else if (itemId == "200004")
        {
            int rdmBomb = 0;
            int rdmRocket = 0;

            while (rdmRocket == rdmBomb)
            {
                rdmRocket = Random.Range(0, cookies.Count - 1);
                rdmBomb = Random.Range(0, cookies.Count - 1);
            }

            cookies[rdmBomb].ChangeToBombBreaker();
            cookies[rdmRocket].ChangeToColRowBreaker();
        }
        else if (itemId == "200005")
        {
            planePlusNum = 1;

        }
        else if (itemId == "200008")
        {
            cookie.ChangeToColRowBreaker();
        }
        else if (itemId == "200009")
        {
            cookie.ChangeToBombBreaker();
        }
        else if (itemId == "200010")
        {
            cookie.ChangeToPlaneBreaker();
        }
    }


    #endregion

    #region Utility
    Vector3 CalculateFirstNodePosition()
    {
        var width = NodeSize();
        var height = NodeSize();
        var column = LevelLoader.instance.column;
        var row = LevelLoader.instance.row;

        var offset = new Vector3(2, 0, 0);

        return (new Vector3(-((column - 1) * width / 2), (row - 1) * height / 2, 0) + offset);
    }

    public float NodeSize()
    {
        return 1.7f;
    }

    public Vector3 NodeLocalPosition(int i, int j)
    {
        var width = NodeSize();
        var height = NodeSize();

        if (firstNodePosition == Vector3.zero)
        {
            firstNodePosition = CalculateFirstNodePosition();
        }

        var x = firstNodePosition.x + j * width;
        var y = firstNodePosition.y - i * height;

        return new Vector3(x, y, 0);
    }

    public int NodeOrder(int i, int j)
    {
        return (i * LevelLoader.instance.column + j);
    }

    public Node GetNode(int row, int column)
    {
        if (row < 0 || row >= LevelLoader.instance.row || column < 0 || column >= LevelLoader.instance.column)
        {
            return null;
        }
        return nodes[row * LevelLoader.instance.column + column];
    }

    private Vector3 ColumnFirstItemPosition(int i, int j)
    {
        Node node = GetNode(i, j);

        if (node != null)
        {
            var item = node.item;
            if (item != null && item.type != ITEM_TYPE.BLANK)
                return item.gameObject.transform.position;

            return ColumnFirstItemPosition(i + 1, j);
        }
        return Vector3.zero;
    }

    public List<Item> GetListItems()
    {
        var items = new List<Item>();
        foreach (var node in nodes)
            if (node != null)
                items.Add(node.item);

        return items;
    }

    public List<Item> GetListItemsInCurrentPage()
    {
        var items = new List<Item>();
        foreach (var node in nodes)
            if (node != null && node.IsInCurrentPage())
                items.Add(node.item);

        return items;
    }

    public int GetMostColor()
    {
        var sameColorItems = new Dictionary<int, int>();

        foreach (var node in nodes)
        {
            if (node != null && node.item != null && node.item.IsCookie())
            {
                if (sameColorItems.ContainsKey(node.item.color))
                {
                    sameColorItems[node.item.color]++;
                }
                else
                {
                    sameColorItems.Add(node.item.color, 1);
                }
            }
        }
        int count = 0;

        int mostColor = 1;

        foreach (var sameItems in sameColorItems)
        {
            if (count < sameItems.Value)
            {
                count = sameItems.Value;
                mostColor = sameItems.Key;
            }
        }

        return mostColor;
    }

    #endregion

    #region Match

    // return the list of square matches on the board
    public List<List<Item>> GetSquareMatches()
    {
        var combines = new List<List<Item>>();

        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                if (GetNode(i, j) != null)
                {
                    List<Item> combine = GetNode(i, j).FindSquareMatches();

                    // combine can be null
                    if (combine != null)
                    {
                        if (combine.Count == 4)
                        {
                            combines.Add(combine);
                        }
                    }
                }
            }
        }

        return combines;
    }

    // re-generate the board to make sure there is no "pre-matches"
    void GenerateNoMatches()
    {
        //Debug.Log("Start generating matches");

        var combines = GetMatches();
        var squareCombines = GetSquareMatches();

        var runNum = 0;

        do
        {
            List<Item> rdmItems = new List<Item>();
            foreach (var combine in combines)
            {
                foreach (var item in combine)
                {
                    if (item != null)
                    {
                        if (!rdmItems.Contains(item))
                        {
                            rdmItems.Add(item);
                        }
                    }
                }
            }

            foreach (var combine in squareCombines)
            {
                foreach (var item in combine)
                {
                    if (item != null)
                    {
                        if (!rdmItems.Contains(item))
                        {
                            rdmItems.Add(item);
                        }
                    }
                }
            }

            // only re-generate color for random item
            int i = 0;
            foreach (var item in rdmItems)
            {
                if (item.OriginCookieType() == ITEM_TYPE.RAMDOM)
                {
                    item.GenerateColor(item.color + i);
                    i++;
                }
            }

            squareCombines = GetSquareMatches();
            combines = GetMatches();
            runNum++;
            if (runNum > 400)
            {
                Debug.Log("初始地图配置存在无法解决的初始可消除问题！！！ 检查关卡配置！！！");
                break;
            }

        } while (combines.Count > 0 || squareCombines.Count > 0);

        //gnerate no match is done,we should not skip page checking futher.
        m_bSkipPageChecking = false;
    }

    public List<List<Item>> GetMatches(FIND_DIRECTION direction = FIND_DIRECTION.NONE, int matches = 3)
    {
        var combines = new List<List<Item>>();

        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                if (GetNode(i, j) != null)
                {
                    var combine = GetNode(i, j).FindMatches(direction, matches);
                    if (combine != null)
                    {
                        if (combine.Count >= matches && !combines.Contains(combine))
                        {
                            combines.Add(combine);
                        }
                    }
                }
            }
        }

        return combines;
    }

    public void FindMatches()
    {
        StartCoroutine(DestroyMatches());
    }

    IEnumerator DestroyMatches()
    {
        matching++;

        while (true)
        {
            var destroyItemList = new List<Item>();
            var grasschangeList = new List<Node>();

            var combines = GetMatches();

            combines.Sort(delegate (List<Item> left, List<Item> right)
            {
                if (left == null && right == null)
                    return 0;
                else if (left == null)
                    return -1;
                else if (right == null)
                    return 1;
                else
                    return left.Count < right.Count ? -1 : 1;
            });

            combines.SelectMany(item => item).ToList().ForEach(item => item.IsMatched = false);

            foreach (var combine in combines)
            {
                if (!combine.Any(item => item.IsMatched))
                {
                    if (combine.Count == 3 && combines.Count > 3)
                    {
                        var itemList = SetBombBreakerCombine(GetMatches(FIND_DIRECTION.ROW));
                        combine.ForEach(item => item.IsMatched = true);
                        itemList.ForEach(item => item.IsMatched = true);
                    }
                    else if (combine.Count == 4)
                    {
                        SetColRowBreakerCombine(combine);
                        combine.ForEach(item => item.IsMatched = true);
                    }
                    else if (combine.Count >= 5)
                    {
                        SetRainbowCombine(combine);
                        combine.ForEach(item => item.IsMatched = true);
                    }
                }

                var isGrass = false;

                foreach (var item in combine)
                {
                    destroyItemList.Add(item);

                    if (item.node.grass != null)
                    {
                        isGrass = true;
                    }
                }
                if (isGrass)
                {
                    foreach (var item in combine)
                    {
                        if (!grasschangeList.Contains(item.node))
                        {
                            grasschangeList.Add(item.node);
                        }
                    }
                }

            } // end foreach combines

            var squareCombines = GetSquareMatches();

            foreach (var combine in squareCombines)
            {
                if (combine.Count == 4)
                {
                    SetPlaneCombine(combine);
                }
                bool isDestroy = false;
                foreach (var item in combine)
                {
                    if (!(item.next == ITEM_TYPE.NONE || item.next == ITEM_TYPE.PLANE_BREAKER))
                    {
                        isDestroy = true;
                    }
                }

                var isGrass = false;

                if (!isDestroy)
                {
                    foreach (var item in combine)
                    {
                        destroyItemList.Add(item);

                        if (item.node.grass != null)
                        {
                            isGrass = true;
                        }
                    }
                }

                if (isGrass)
                {
                    foreach (var item in combine)
                    {
                        if (!grasschangeList.Contains(item.node))
                        {
                            grasschangeList.Add(item.node);
                        }
                    }
                }
            }

            InitPlaneRandom();

            foreach (var item in destroyItemList)
            {
                item.Destroy();
            }

            foreach (var node in grasschangeList)
            {
                node.ChangeToGrass();
            }

            // wait until item destroy animation finish
            while (destroyingItems > 0 || playingAnimation > 0)
            {
                //Debug.Log("Destroying items");
                yield return new WaitForSeconds(0.1f);
            }

            // IMPORTANT: as describe in document Destroy is always delayed (but executed within the same frame).
            // So There is case destroyingItems = 0 BUT the item still exist that causes the GenerateNewItems function goes wrong
            yield return new WaitForEndOfFrame();

            for (int i = appleBoxes.Count - 1; i >= 0; i--)
            {
                appleBoxes[i].TryToDestroyBox();
            }

            yield return new WaitForEndOfFrame();

            Drop();

            while (droppingItems > 0)
                yield return new WaitForSeconds(0.1f);

            var collectionList = CollectCollectible();
            CollectCollectionItem(collectionList);

            if (collectionList.Count > 0)
            {
                yield return new WaitForSeconds(Configure.instance.destroyTime);

                Drop();

                while (droppingItems > 0)
                    yield return new WaitForSeconds(0.1f);
            }

            if (GetSquareMatches().Count <= 0 && GetMatches().Count <= 0)
                break;

            dropTime++;
        }

        // wait until all flying items fly to top bar
        while (flyingItems > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForEndOfFrame();

        if (matching > 1)
        {
            matching--;
            yield break;
        }

        // check if level complete
        if (state == GAME_STATE.WAITING_USER_SWAP)
        {
            if (moveLeft > 0)
            {
                if (IsLevelCompleted())
                {
                    StartCoroutine(PreWinAutoPlay());
                }
                else
                {
                    if (MoveGingerbread())
                    {
                        yield return new WaitForSeconds(Configure.instance.swapTime);

                        FindMatches();
                    }

                    if (GenerateGingerbread())
                    {
                        yield return new WaitForSeconds(0.2f);

                        FindMatches();
                    }

                    if (!GuideManager.instance.GuideEnabled)
                    {
                        CheckHint();
                    }
                    else
                    {
                        GuideManager.instance.Show();
                    }
                }
            }
            else if (moveLeft == 0)
            {
                if (IsLevelCompleted())
                {
                    SaveLevelInfo();

                    state = GAME_STATE.OPENING_POPUP;

                    WindowManager.instance.Show<WinPopupWindow>();

                    AudioManager.instance.PopupWinAudio();
                }
                else
                {
                    state = GAME_STATE.OPENING_POPUP;

                    WindowManager.instance.Show<LosePopupWindow>();

                    AudioManager.instance.PopupLoseAudio();
                }
            }
        }

        matching--;

        // if dropTime >= 3 we should show some text like: grate, amazing, etc.
        if (dropTime >= Configure.instance.encouragingPopup && state == GAME_STATE.WAITING_USER_SWAP && showingInspiringPopup == false)
        {
            ShowInspiringPopup();
        }

        //check should move to next page or not
        //have multiple page
        if (LevelLoader.instance.HaveMultiplePage() && m_ICurrentPageIndex < (LevelLoader.instance.GetPageCount() - 1))
        {
            if (ShouldTurnPage())
            {
                //move to next page
                yield return moveToPageByIndex(m_ICurrentPageIndex + 1);
                UnlockNodesDropInAttrInPage();
            }
        }

        yield return new WaitForSeconds(0.2f);
        lockSwap = false;
    }

    #endregion

    #region Drop

    void Drop()
    {
        SetDropTargets();

        GenerateNewItems(true, Vector3.zero);

        Move();

        DropItems();
    }

    void SetDropTargets()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        for (int j = 0; j < column; j++)
        {
            //need to enumerate rows from bottom to top
            for (int i = row - 1; i >= 0; i--)
            {
                Node node = GetNode(i, j);

                if (node != null)
                {
                    Item item = node.item;

                    if (item != null)
                    {
                        // start calculating new target for the node
                        if (item.Droppable())
                        {
                            Node target = node.BottomNeighbor();

                            if (target != null && target.CanGoThrough() && target.CanDropIn() && !target.HasTopBaffle())
                            {
                                if (target.item == null || (target.item != null && target.item.type == ITEM_TYPE.BLANK))
                                {
                                    // check rows below at this time GetNode(i + 1, j) = target
                                    for (int k = i + 2; k < row; k++)
                                    {
                                        if (GetNode(k, j) != null)
                                        {
                                            if (GetNode(k, j).item != null && GetNode(k, j).item.type == ITEM_TYPE.BLANK)
                                            {
                                                if (GetNode(k, j).CanStoreItem() && GetNode(k, j).CanDropIn() && !GetNode(k, j).HasTopBaffle())
                                                {
                                                    target = GetNode(k, j);
                                                }
                                            }

                                            // if a node can not go through we do not need to check bellow
                                            if (GetNode(k, j).CanGoThrough() == false || !GetNode(k, j).CanDropIn() || GetNode(k, j).HasTopBaffle())
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                if (GetNode(k, j).item != null && GetNode(k, j).item.type != ITEM_TYPE.BLANK)
                                                {
                                                    if (GetNode(k, j).item.Droppable() == false)
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                // after have the target we swap items on nodes
                                if ((target.item != null && target.item.type == ITEM_TYPE.BLANK) && target.ice == null && target.CanStoreItem())
                                {
                                    if (target.item != null && target.item.type == ITEM_TYPE.BLANK)
                                    {
                                        Destroy(target.item.gameObject);
                                    }
                                    target.item = item;
                                    target.item.gameObject.transform.SetParent(target.gameObject.transform);
                                    target.item.node = target;

                                    node.item = null;
                                    node.GenerateItem(ITEM_TYPE.BLANK);

                                    if (node.ice != null)
                                    {
                                        target.ice = node.ice;
                                        target.ice.gameObject.transform.SetParent(target.gameObject.transform);
                                        target.ice.node = target;

                                        node.ice = null;

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // after destroy and drop items then we generate new items
    void GenerateNewItems(bool IsDrop, Vector3 pos)
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        var marshmallowGenerated = false;

        for (int j = 0; j < column; j++)
        {
            var space = -1;

            var itemPos = Vector3.zero;

            for (int i = row - 1; i >= 0; i--)
            {
                if (GetNode(i, j) != null)
                {
                    if (((GetNode(i, j).item != null && GetNode(i, j).item.type == ITEM_TYPE.BLANK)) && GetNode(i, j).CanGenerateNewItem())
                    {
                        // if target is collectible the new item can be a collectible
                        var collectible = false;

                        // collectible is only generated on the highest row
                        if (i == 0)
                        {
                            // check if need to generate new collectible
                            if (CheckGenerateCollectible() != null &&
                                CheckGenerateCollectible().Count > 0 &&
                                (LevelLoader.instance.collectibleGenerateMarkers.Contains(j) || LevelLoader.instance.collectibleGenerateMarkers.Count == 0))
                            {
                                collectible = true;
                            }
                        }

                        // check if need to generate a new marshmallow
                        var marshmallow = false;

                        if (CheckGenerateMarshmallow())
                        {
                            marshmallow = true;
                        }

                        if (pos != Vector3.zero)
                        {
                            itemPos = pos + Vector3.up * NodeSize();
                        }
                        else
                        {
                            // calculate position of the new item
                            if (i > space)
                            {
                                space = i;
                            }

                            // can pass through node
                            var pass = 0;

                            for (int k = 0; k < row; k++)
                            {
                                var node = GetNode(k, j);

                                if (node != null && node.tile != null && node.tile.type == TILE_TYPE.PASSTHROUGH)
                                {
                                    pass++;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            itemPos = NodeLocalPosition(i, j) + Vector3.up * (space - pass + 1) * NodeSize();
                        }

                        if (GetNode(i, j).item != null && GetNode(i, j).item.type == ITEM_TYPE.BLANK)
                        {
                            Destroy(GetNode(i, j).item.gameObject);
                            GetNode(i, j).item = null;
                        }

                        if (collectible && Random.Range(0, 2) == 1)
                        {
                            GetNode(i, j).GenerateItem(CheckGenerateCollectible()[Random.Range(0, CheckGenerateCollectible().Count)]);
                        }
                        else if (marshmallow && Random.Range(0, 2) == 1 && marshmallowGenerated == false)
                        {
                            marshmallowGenerated = true;

                            GetNode(i, j).GenerateItem(ITEM_TYPE.MARSHMALLOW);
                        }
                        else
                        {
                            GetNode(i, j).GenerateItem(ITEM_TYPE.RAMDOM);
                        }

                        var newItem = GetNode(i, j).item;
                        if (newItem != null)
                        {
                            if (IsDrop)
                            {
                                newItem.gameObject.transform.localPosition = itemPos;
                            }
                            else
                            {
                                newItem.gameObject.transform.localPosition = NodeLocalPosition(i, j);
                            }
                        }
                    }
                }
            }
        }
    }

    void Move()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        for (int i = row - 1; i >= 0; i--)
        {
            for (int j = 0; j < column; j++)
            {
                Node node = GetNode(i, j);

                if (node != null)
                {
                    if (node.item != null && node.item.type == ITEM_TYPE.BLANK && node.CanStoreItem() && node.CanDropIn())
                    {
                        Node source = node.GetSourceNode();

                        if (source != null)
                        {
                            var pos = ColumnFirstItemPosition(0, source.j);

                            List<Vector3> path = node.GetMovePath();

                            if (source.transform.position != NodeLocalPosition(source.i, source.j))
                            {
                                path.Add(NodeLocalPosition(source.i, source.j) + transform.position);
                            }

                            if (node.item != null && node.item.type == ITEM_TYPE.BLANK && node.CanDropIn())
                            {
                                Destroy(node.item.gameObject);
                            }

                            node.item = source.item;
                            node.item.gameObject.transform.SetParent(node.gameObject.transform);
                            node.item.node = node;

                            source.item = null;
                            source.GenerateItem(ITEM_TYPE.BLANK);

                            if (source.ice != null)
                            {
                                node.ice = source.ice;
                                node.ice.gameObject.transform.SetParent(node.gameObject.transform);
                                node.ice.node = node;

                                source.ice = null;
                            }

                            if (path.Count > 1)
                            {
                                path.Reverse();

                                node.item.dropPath = path;
                            }
                            SetDropTargets();

                            GenerateNewItems(true, pos);
                        }
                    }
                }
            }
        }
    }

    void DropItems()
    {
        StartCoroutine(DropItems2());
    }

    IEnumerator DropItems2()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        for (int i = row - 1; i >= 0; i--)
        {
            bool hasDrop = false;
            for (int j = 0; j < column; j++)
            {
                if (GetNode(i, j) != null)
                {
                    if (GetNode(i, j).item != null)
                    {
                        if (GetNode(i, j).item.isNeedDrop())
                        {
                            hasDrop = true;
                            GetNode(i, j).item.Drop();
                        }
                    }
                }
            }
            if (hasDrop)
            {
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    #endregion

    #region Item

    // this function check all the items and set them to be bomb-breaker/x-breaker
    public List<Item> SetBombBreakerCombine(List<List<Item>> lists)
    {
        var itemList = new List<Item>();
        foreach (List<Item> list in lists)
        {
            foreach (Item item in list)
            {
                if (item != null && item.node != null)
                {
                    var columnList = item.node.FindMatches(FIND_DIRECTION.COLUMN);
                    if (columnList.Count > 2)
                    {
                        //todo : 优先级根据配置 改掉死代码
                        if (item.next == ITEM_TYPE.NONE || item.next == ITEM_TYPE.ROW_BREAKER || item.next == ITEM_TYPE.COLUMN_BREAKER || item.next == ITEM_TYPE.PLANE_BREAKER)
                        {
                            item.next = item.GetBombBreaker(item.type);
                            itemList.AddRange(columnList);
                            return itemList;
                        }
                    }
                }
            }
        }
        return itemList;
    }

    public void SetColRowBreakerCombine(List<Item> combine)
    {
        bool isSwap = false;

        //todo : 优先级根据配置 改掉死代码
        foreach (Item item in combine)
        {
            if (item.next != ITEM_TYPE.NONE)
            {
                isSwap = true;

                break;
            }
        }

        // next type is normal (drop then match) get first item in the combine
        if (!isSwap)
        {
            Item first = null;

            foreach (Item item in combine)
            {
                if (first == null)
                {
                    first = item;
                }
                else
                {
                    if (item.node.OrderOnBoard() < first.node.OrderOnBoard())
                    {
                        first = item;
                    }
                }
            }

            foreach (Item item in combine)
            {
                if (first.node.RightNeighbor())
                {
                    if (item.node.OrderOnBoard() == first.node.RightNeighbor().OrderOnBoard())
                    {
                        first.next = first.GetColumnBreaker(first.type);
                        break;
                    }
                }

                if (first.node.BottomNeighbor())
                {
                    if (item.node.OrderOnBoard() == first.node.BottomNeighbor().OrderOnBoard())
                    {
                        first.next = first.GetRowBreaker(first.type);
                        break;
                    }
                }
            }
        }
    }

    public void SetRainbowCombine(List<Item> combine)
    {

        var isSwap = false;
        foreach (Item item in combine)
        {
            if (item.next == ITEM_TYPE.RAINBOW)
            {
                isSwap = true;
                break;
            }
        }

        if (!isSwap)
        {
            Item first = null;

            foreach (Item item in combine)
            {
                if (first == null)
                {
                    first = item;
                }
                else
                {
                    if (item.node.OrderOnBoard() < first.node.OrderOnBoard())
                    {
                        first = item;
                    }
                }
            }

            foreach (Item item in combine)
            {
                if (first.node.RightNeighbor())
                {
                    if (item.node.OrderOnBoard() == first.node.RightNeighbor().OrderOnBoard())
                    {
                        combine[2].next = ITEM_TYPE.RAINBOW;
                        break;
                    }
                }

                if (first.node.BottomNeighbor())
                {
                    if (item.node.OrderOnBoard() == first.node.BottomNeighbor().OrderOnBoard())
                    {
                        first.next = ITEM_TYPE.RAINBOW;
                        break;
                    }
                }
            }
        }
    }

    public void SetPlaneCombine(List<Item> combine)
    {
        bool isSwap = false;
        //todo : 优先级根据配置 改掉死代码
        // rainbow优先级最高
        foreach (Item item in combine)
        {
            if (item.next != ITEM_TYPE.NONE)
            {
                isSwap = true;
                break;

            }
        }

        if (!isSwap)
        {
            Item first = null;

            foreach (Item item in combine)
            {
                if (first == null)
                {
                    first = item;
                }
                else
                {
                    if (item.node.OrderOnBoard() < first.node.OrderOnBoard())
                    {
                        first = item;
                    }
                }
            }
            first.next = first.GetPlaneBreaker(first.type);
        }
    }

    // return items around
    public List<Item> ItemAround(Node node, int range)
    {
        List<Item> items = new List<Item>();

        for (int i = node.i - range; i <= node.i + range; i++)
        {
            for (int j = node.j - range; j <= node.j + range; j++)
            {
                //跳过四个角点
                if (i == node.i - range && j == node.j - range
                    || i == node.i - range && j == node.j + range
                    || i == node.i + range && j == node.j - range
                    || i == node.i + range && j == node.j + range
                )
                {
                    continue;
                }
                if (GetNode(i, j) != null)
                {
                    items.Add(GetNode(i, j).item);
                }
            }
        }

        return items;
    }

    public List<Item> XCrossItems(Node node)
    {
        var items = new List<Item>();

        var row = LevelLoader.instance.row;

        for (int i = 0; i < row; i++)
        {
            if (i < node.i)
            {
                var crossLeft = GetNode(i, node.j - (node.i - i));
                var crossRight = GetNode(i, node.j + (node.i - i));

                if (crossLeft != null)
                {
                    if (crossLeft.item != null)
                    {
                        items.Add(crossLeft.item);
                    }
                }

                if (crossRight != null)
                {
                    if (crossRight.item != null)
                    {
                        items.Add(crossRight.item);
                    }
                }
            }
            else if (i == node.i)
            {
                if (node.item != null)
                {
                    items.Add(node.item);
                }
            }
            else if (i > node.i)
            {
                var crossLeft = GetNode(i, node.j - (i - node.i));
                var crossRight = GetNode(i, node.j + (i - node.i));

                if (crossLeft != null)
                {
                    if (crossLeft.item != null)
                    {
                        items.Add(crossLeft.item);
                    }
                }

                if (crossRight != null)
                {
                    if (crossRight.item != null)
                    {
                        items.Add(crossRight.item);
                    }
                }
            }
        }

        return items;
    }

    public List<Item> ColumnItems(int column)
    {
        var items = new List<Item>();

        var row = LevelLoader.instance.row;

        for (int i = 0; i < row; i++)
        {
            if (GetNode(i, column) != null)
            {
                items.Add(GetNode(i, column).item);
            }
        }

        return items;
    }

    public List<Item> RowItems(int row)
    {
        var items = new List<Item>();

        var column = LevelLoader.instance.column;

        for (int j = 0; j < column; j++)
        {
            if (GetNode(row, j) != null)
            {
                items.Add(GetNode(row, j).item);
            }
        }

        return items;
    }

    public List<Node> ColumnNodes(int column)
    {
        var nodes = new List<Node>();

        var row = LevelLoader.instance.row;

        for (int i = 0; i < row; i++)
        {
            if (GetNode(i, column) != null)
            {
                nodes.Add(GetNode(i, column));
            }
        }

        return nodes;
    }

    public List<Node> RowNodes(int row)
    {
        var nodes = new List<Node>();

        var column = LevelLoader.instance.column;

        for (int j = 0; j < column; j++)
        {
            if (GetNode(row, j) != null)
            {
                nodes.Add(GetNode(row, j));
            }
        }

        return nodes;
    }

    #endregion

    #region Destroy

    // destroy the whole board when swap 2 rainbow
    public void DoubleRainbowDestroy(bool isgrass)
    {
        StartCoroutine(DestroyWholeBoard(isgrass));
    }

    IEnumerator DestroyWholeBoard(bool isgrass)
    {
        var column = LevelLoader.instance.column;
        playingAnimation++;
        for (int i = 0; i < column; i++)
        {
            List<Item> items = ColumnItems(i);

            foreach (var item in items)
            {
                if (item != null && item.Destroyable())
                {
                    //item.type = item.GetCookie(item.type);
                    if (isgrass)
                    {
                        item.node.ChangeToGrass();
                    }

                    CFX_SpawnSystem.GetNextObject(Configure.EffectDoubleRainbow, item.transform);

                    item.Destroy();
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
        playingAnimation--;
        FindMatches();
    }

    public void DestroyPlaneTargetList(Item item, bool isgrass = false)
    {
        StartCoroutine(StartDestroyPlaneTargetList(item, isgrass));
    }

    IEnumerator StartDestroyPlaneTargetList(Item item, bool isgrass = false)
    {
        if (state != GAME_STATE.PRE_WIN_AUTO_PLAYING)
        {
            if (specialDestroying == 0)
            {
                originalStateInSDestroying = state;
            }
            specialDestroying++;
            state = GAME_STATE.DESTROYING_ITEMS;
        }

        if (item != null)
        {
            if (isgrass)
            {
                item.node.ChangeToGrass();
            }
            item.beAbleToDestroy++;
            item.Destroy();
        }

        if (state != GAME_STATE.PRE_WIN_AUTO_PLAYING)
        {
            specialDestroying--;
            if (specialDestroying == 0)
            {
                state = originalStateInSDestroying;
            }
        }

        while (destroyingItems > 0 || playingAnimation > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForEndOfFrame();

        Drop();

        while (droppingItems > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForEndOfFrame();
    }

    public void DestroyChangingList(bool isgrass = false)
    {
        StartCoroutine(StartDestroyChangingList(isgrass));
    }

    IEnumerator StartDestroyChangingList(bool isgrass = false)
    {
        if (state != GAME_STATE.PRE_WIN_AUTO_PLAYING)
        {
            if (specialDestroying == 0)
            {
                originalStateInSDestroying = state;
            }
            specialDestroying++;
            state = GAME_STATE.DESTROYING_ITEMS;
        }

        yield return new WaitForSeconds(0.5f);

        InitPlaneRandom();

        foreach (var item in changingList)
        {
            if (item != null && item.destroying == false)
            {
                if (isgrass)
                {
                    item.node.ChangeToGrass();
                }
                item.Destroy();
            }
        }

        while (destroyingItems > 0 || playingAnimation > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForEndOfFrame();

        Drop();

        while (droppingItems > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForEndOfFrame();

        changingList.Clear();
        if (state != GAME_STATE.PRE_WIN_AUTO_PLAYING)
        {
            specialDestroying--;
            if (specialDestroying == 0)
            {
                state = originalStateInSDestroying;
            }
        }
        FindMatches();
    }

    public void DestroySameColorList(Item source, bool isgrass = false)
    {
        StartCoroutine(StartDestroySameColorList(source.transform.position, isgrass));
    }

    public List<Item> rdmItems = new List<Item>();
    private bool rdmInit = false;

    public void InitPlaneRandom()
    {
        rdmItems.Clear();
        rdmInit = false;
    }

    public void FindPlaneTarget(int planeNum, bool isgrass)
    {
        if (rdmItems.Count == 0 && !rdmInit)
        {
            rdmInit = true;

            for (var i = 0; i < LevelLoader.instance.targetList.Count; ++i)
            {
                var targetLeft = targetLeftList[i];
                var target = LevelLoader.instance.targetList[i];
                if (targetLeft > 0 && target.Amount > 0)
                {
                    var targetItemList = FindPlaneTarget(target.Type, target.color, rdmItems, isgrass);
                    rdmItems.AddRange(targetItemList);
                    rdmItems.Randomize();
                }
            }
        }

        if (rdmItems.Count >= planeNum)
            return;

        FindPlaneGeneraltarget(rdmItems, planeNum, isgrass);
    }

    private void FindPlaneGeneraltarget(List<Item> targetItems, int planeNum, bool isgrass)
    {
        List<Item> allSpecialTargetItems = new List<Item>();
        List<Item> allCookieTargetItems = new List<Item>();
        for (int i = 0; i < LevelLoader.instance.row; i++)
        {
            for (int j = 0; j < LevelLoader.instance.column; j++)
            {
                var node = GetNode(i, j);
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

    private List<Item> FindPlaneTarget(TARGET_TYPE targetType, int targetColor, List<Item> targetItems, bool isgrass)
    {
        List<Item> allTargetItems = new List<Item>();

        for (int i = 0; i < LevelLoader.instance.row; i++)
        {
            for (int j = 0; j < LevelLoader.instance.column; j++)
            {
                Node node = GetNode(i, j);

                if (node != null && node.item != null && node.item.beAbleToDestroy > 0 && !node.item.destroying &&
                    !allTargetItems.Contains(node.item) && !targetItems.Contains(node.item))
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
                                if (GetNode(k, j) != null)
                                {
                                    var targetNode = GetNode(k, j);
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
                    else if (isgrass && targetType == TARGET_TYPE.GRASS)
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

    public bool RainbowDisplayOnce = true;
    public float RainbowLineDuration = 0.1f;
    public float RainbowLineInterval = 0.02f;

    IEnumerator StartDestroySameColorList(Vector3 source, bool isgrass = false)
    {
        playingAnimation++;

        if (state != GAME_STATE.PRE_WIN_AUTO_PLAYING)
        {
            if (specialDestroying == 0)
            {
                originalStateInSDestroying = state;
            }
            specialDestroying++;
            state = GAME_STATE.DESTROYING_ITEMS;
        }

        var effectRainbow = CFX_SpawnSystem.GetNextObject(Configure.EffectRainbow, source).GetComponent<CFX_AutoDestruct>();
        effectRainbow.Duration = !RainbowDisplayOnce
            ? sameColorList.Count * RainbowLineDuration + (sameColorList.Count - 1) * RainbowLineInterval
            : RainbowLineDuration;

        foreach (var item in sameColorList)
        {
            if (item != null && item.destroying == false)
            {
                var line = CFX_SpawnSystem.GetNextObject(Configure.EffectRainbowLine, source);
                line.transform.DOMove(item.transform.position, RainbowLineDuration);
                var effectLine = line.GetComponent<CFX_AutoDestruct>();
                effectLine.Duration = RainbowLineDuration;

                if (!RainbowDisplayOnce)
                    yield return new WaitForSeconds(RainbowLineInterval);

                CFX_SpawnSystem.GetNextObject(Configure.EffectCookieDestroy, item.transform);

                if (isgrass)
                {
                    item.node.ChangeToGrass();
                }

                StartCoroutine(ItemDestroy(item));

                if (!RainbowDisplayOnce)
                    yield return new WaitForSeconds(RainbowLineDuration);
            }
        }

        if (RainbowDisplayOnce)
            yield return new WaitForSeconds(RainbowLineDuration);

        sameColorList.Clear();
        if (state != GAME_STATE.PRE_WIN_AUTO_PLAYING)
        {
            specialDestroying--;
            if (specialDestroying == 0)
            {
                state = originalStateInSDestroying;
            }
        }
        playingAnimation--;

        FindMatches();
    }

    private IEnumerator ItemDestroy(Item item)
    {
        yield return new WaitForSeconds(RainbowLineDuration);
        item.Destroy();
    }

    public void DestroyNeighborItems(Item item)
    {
        if (!CanDestroyNeighbor(item))
            return;

        if (state == GAME_STATE.PRE_WIN_AUTO_PLAYING)
            return;

        if (item.DestroyByType == Item.DestroyBy.Booster)
        {
            DestroyRockCandy(item);
            DestroyJelly(item);
        }
        else if (item.DestroyByType == Item.DestroyBy.Cookie)
        {
            DestroyMarshmallow(item);
            DestroyAppleBox(item);
            DestroyRockCandy(item);
            DestroyJelly(item);
            DestroyPackageBox(item);
        }
    }

    private bool CanDestroyNeighbor(Item item)
    {
        return (item != null && item.IsCookie());
    }

    public void DestroyMarshmallow(Item item)
    {
        var marshmallows = new List<Item>();

        if (item.node.TopNeighbor() != null && item.node.TopNeighbor().bafflebottom == null && item.node.TopNeighbor().item != null && item.node.TopNeighbor().item.IsMarshmallow())
        {
            marshmallows.Add(item.node.TopNeighbor().item);
        }

        if (item.node.RightNeighbor() != null && item.node.baffleright == null && item.node.RightNeighbor().item != null && item.node.RightNeighbor().item.IsMarshmallow())
        {
            marshmallows.Add(item.node.RightNeighbor().item);
        }

        if (item.node.BottomNeighbor() != null && item.node.bafflebottom == null && item.node.BottomNeighbor().item != null && item.node.BottomNeighbor().item.IsMarshmallow())
        {
            marshmallows.Add(item.node.BottomNeighbor().item);
        }

        if (item.node.LeftNeighbor() != null && item.node.LeftNeighbor().baffleright == null && item.node.LeftNeighbor().item != null && item.node.LeftNeighbor().item.IsMarshmallow())
        {
            marshmallows.Add(item.node.LeftNeighbor().item);
        }

        foreach (var marshmallow in marshmallows)
        {
            marshmallow.Destroy();
        }
    }

    public void DestroyAppleBox(Item item)
    {

        var appleboxes = new List<Item>();

        if (item.node.TopNeighbor() != null && item.node.TopNeighbor().bafflebottom == null && item.node.TopNeighbor().item != null && item.node.TopNeighbor().item.IsAppleBox())
        {
            appleboxes.Add(item.node.TopNeighbor().item);
        }

        if (item.node.RightNeighbor() != null && item.node.baffleright == null && item.node.RightNeighbor().item != null && item.node.RightNeighbor().item.IsAppleBox())
        {
            appleboxes.Add(item.node.RightNeighbor().item);
        }

        if (item.node.BottomNeighbor() != null && item.node.bafflebottom == null && item.node.BottomNeighbor().item != null && item.node.BottomNeighbor().item.IsAppleBox())
        {
            appleboxes.Add(item.node.BottomNeighbor().item);
        }

        if (item.node.LeftNeighbor() != null && item.node.LeftNeighbor().baffleright == null && item.node.LeftNeighbor().item != null && item.node.LeftNeighbor().item.IsAppleBox())
        {
            appleboxes.Add(item.node.LeftNeighbor().item);
        }

        foreach (var applebox in appleboxes)
        {
            applebox.Destroy();
        }
    }

    public void DestroyChocolate(Item item)
    {

        var chocolates = new List<Item>();

        if (item.node.TopNeighbor() != null && item.node.TopNeighbor().bafflebottom == null && item.node.TopNeighbor().item != null && item.node.TopNeighbor().item.IsChocolate())
        {
            chocolates.Add(item.node.TopNeighbor().item);
        }

        if (item.node.RightNeighbor() != null && item.node.baffleright == null && item.node.RightNeighbor().item != null && item.node.RightNeighbor().item.IsChocolate())
        {
            chocolates.Add(item.node.RightNeighbor().item);
        }

        if (item.node.BottomNeighbor() != null && item.node.bafflebottom == null && item.node.BottomNeighbor().item != null && item.node.BottomNeighbor().item.IsChocolate())
        {
            chocolates.Add(item.node.BottomNeighbor().item);
        }

        if (item.node.LeftNeighbor() != null && item.node.LeftNeighbor().baffleright == null && item.node.LeftNeighbor().item != null && item.node.LeftNeighbor().item.IsChocolate())
        {
            chocolates.Add(item.node.LeftNeighbor().item);
        }

        foreach (var chocolate in chocolates)
        {
            chocolate.Destroy();
        }
    }

    public void DestroyJelly(Item item)
    {

        var items = new List<Item>();

        if (item.node.TopNeighbor() != null && item.node.TopNeighbor().bafflebottom == null && item.node.TopNeighbor().item != null && item.node.TopNeighbor().jelly != null)
        {
            items.Add(item.node.TopNeighbor().item);
        }

        if (item.node.RightNeighbor() != null && item.node.baffleright == null && item.node.RightNeighbor().item != null && item.node.RightNeighbor().jelly != null)
        {
            items.Add(item.node.RightNeighbor().item);
        }

        if (item.node.BottomNeighbor() != null && item.node.bafflebottom == null && item.node.BottomNeighbor().item != null && item.node.BottomNeighbor().jelly != null)
        {
            items.Add(item.node.BottomNeighbor().item);
        }

        if (item.node.LeftNeighbor() != null && item.node.LeftNeighbor().baffleright == null && item.node.LeftNeighbor().item != null && item.node.LeftNeighbor().jelly != null)
        {
            items.Add(item.node.LeftNeighbor().item);
        }

        foreach (var itemtmp in items)
        {
            itemtmp.Destroy();
        }
    }

    public void DestroyRockCandy(Item item)
    {

        var rocks = new List<Item>();

        if (item.node.TopNeighbor() != null && item.node.TopNeighbor().bafflebottom == null && item.node.TopNeighbor().item != null && item.node.TopNeighbor().item.IsRockCandy())
        {
            rocks.Add(item.node.TopNeighbor().item);
        }

        if (item.node.RightNeighbor() != null && item.node.baffleright == null && item.node.RightNeighbor().item != null && item.node.RightNeighbor().item.IsRockCandy())
        {
            rocks.Add(item.node.RightNeighbor().item);
        }

        if (item.node.BottomNeighbor() != null && item.node.bafflebottom == null && item.node.BottomNeighbor().item != null && item.node.BottomNeighbor().item.IsRockCandy())
        {
            rocks.Add(item.node.BottomNeighbor().item);
        }

        if (item.node.LeftNeighbor() != null && item.node.LeftNeighbor().baffleright == null && item.node.LeftNeighbor().item != null && item.node.LeftNeighbor().item.IsRockCandy())
        {
            rocks.Add(item.node.LeftNeighbor().item);
        }

        foreach (var rock in rocks)
        {
            needIncreaseBubble = false;
            rock.Destroy();
        }
    }

    public void DestroyPackageBox(Item item)
    {

        var packageboxes = new List<Item>();

        if (item.node.TopNeighbor() != null && item.node.TopNeighbor().bafflebottom == null && item.node.TopNeighbor().item != null && item.node.TopNeighbor().packagebox != null)
        {
            packageboxes.Add(item.node.TopNeighbor().item);
        }

        if (item.node.RightNeighbor() != null && item.node.baffleright == null && item.node.RightNeighbor().item != null && item.node.RightNeighbor().packagebox != null)
        {
            packageboxes.Add(item.node.RightNeighbor().item);
        }

        if (item.node.BottomNeighbor() != null && item.node.bafflebottom == null && item.node.BottomNeighbor().item != null && item.node.BottomNeighbor().packagebox != null)
        {
            packageboxes.Add(item.node.BottomNeighbor().item);
        }

        if (item.node.LeftNeighbor() != null && item.node.LeftNeighbor().baffleright == null && item.node.LeftNeighbor().item != null && item.node.LeftNeighbor().packagebox != null)
        {
            packageboxes.Add(item.node.LeftNeighbor().item);
        }

        foreach (var packagetmp in packageboxes)
        {
            packagetmp.Destroy();
        }
    }

    #endregion

    #region Collect

    // if item is the target to collect
    public void CollectItem(Item item)
    {
        GameObject flyingItem = null;
        var order = 0;
        Target target = null;

        for (int i = 0; i < LevelLoader.instance.targetList.Count; i++)
        {
            target = LevelLoader.instance.targetList[i];
            if (item.DoesMatchFlyingTarget(target) && targetLeftList[i] > 0)
            {
                targetLeftList[i]--;
                if (item.applebox != null)
                    item.applebox.appleNum--;
                flyingItem = Instantiate(CFX_SpawnSystem.GetNextObject(Configure.FlyingCookiePath, null, false));
                order = i;
                break;
            }
        }

        if (flyingItem != null)
        {
            flyingItem.transform.position = item.transform.position;
            flyingItem.name = "Flying " + item.name;
            flyingItem.layer = LayerMask.NameToLayer("On Top UI");
            flyingItem.SetActive(true);

            SpriteRenderer spriteRenderer = flyingItem.GetComponent<SpriteRenderer>();
            var prefab = CFX_SpawnSystem.GetNextObject(target.ToPrefabStr(), item.transform, false);
            if (prefab != null)
                spriteRenderer.sprite = prefab.GetComponent<SpriteRenderer>().sprite;

            var bezierConfig = flyingItem.GetComponent<BezierPathConfigure>() ?? flyingItem.AddComponent<BezierPathConfigure>();
            StartCoroutine(CollectItemAnim(flyingItem, order));
        }
    }

    private List<Item> CollectCollectible()
    {
        var collectionList = new List<Item>();

        bool hasCollectible = false;
        for (int i = 0; i < LevelLoader.instance.targetList.Count; i++)
        {
            if (LevelLoader.instance.targetList[i].Type == TARGET_TYPE.COLLECTIBLE)
            {
                hasCollectible = true;
                break;
            }
        }
        if (!hasCollectible)
            return collectionList;

        GetListItems().ForEach(item =>
        {
            var collectable = item != null &&
                item.node.i == LevelLoader.instance.row - 1 &&
                LevelLoader.instance.collectibleCollectColumnMarkers.Contains(item.node.j) &&
                item.IsCollectible() || item != null &&
                LevelLoader.instance.collectibleCollectNodeMarkers.Contains(NodeOrder(item.node.i, item.node.j)) &&
                item.IsCollectible();

            if (collectable)
                collectionList.Add(item);
        });

        return collectionList;
    }

    void CollectCollectionItem(List<Item> list)
    {
        list.ForEach(item =>
        {
            Debug.Log("收集到cake" + item.node.name + item.type);
            item.Destroy(true);
        });
    }

    // item fly to target
    public IEnumerator CollectItemAnim(GameObject source, int order)
    {
        yield return null;

        var target = UITarget.TargetCellList[order].gameObject;

        flyingItems++;

        source.transform.DOScale(CollectionScale, CollectionDuration);

        yield return new WaitForSeconds(CollectionDuration);

        var bezierPathConfig = source.GetComponent<BezierPathConfigure>();
        //adjust target position according to page move container's position
        Vector3 targetPos = target.transform.position + new Vector3(m_CPageMoveContainer.transform.position.x, m_CPageMoveContainer.transform.position.y, 0);

        var path = bezierPathConfig.GetPath(source.transform.position, targetPos);
        source.transform.DOPath(path.ToArray(), bezierPathConfig.Duration, bezierPathConfig.PlanePathType).SetEase(bezierPathConfig.EaseType);

        yield return new WaitForSeconds(bezierPathConfig.Duration);

        AudioManager.instance.CollectTargetAudio();

        UITarget.UpdateTargetAmount(order);

        Destroy(source);

        flyingItems--;
    }

    public Vector3 CollectionScale = new Vector3(0.8f, 0.8f, 0.8f);
    public float CollectionDuration = 0.2f;

    #endregion

    #region Popup

    void TargetPopup()
    {
        StartCoroutine(StartTargetPopup());
    }

    IEnumerator StartTargetPopup()
    {
        state = GAME_STATE.OPENING_POPUP;

        yield return new WaitForSeconds(0.5f);

        AudioManager.instance.PopupTargetAudio();

        var targetPopup = WindowManager.instance.Show<TargetPopupWindow>().GetComponent<Popup>();

        yield return new WaitForSeconds(targetPopup.Duration + 0.5f);

        yield return viewPages();
        //when viewing finish then unlock current page nodes drop in attribute
        UnlockNodesDropInAttrInPage();

        state = GAME_STATE.WAITING_USER_SWAP;

        if (!GuideManager.instance.GuideEnabled)
        {
            CheckHint();
        }
        else
        {
            GuideManager.instance.Show();
        }
    }

    IEnumerator Plus5MovesPopup()
    {
        Configure.instance.beginFiveMoves = false;

        WindowManager.instance.Show<Plus5MovesPopupWindow>();

        yield return new WaitForSeconds(1.0f);

        var popup = GameObject.Find("Plus5MovesPopup(Clone)");

        if (popup)
        {
            popup.GetComponent<Popup>().Close();
        }
    }

    void ShowInspiringPopup()
    {
        var encourage = (dropTime >= 6) ? 6 : dropTime;
        switch (encourage)
        {
            case 3:
                WindowManager.instance.Show<NicePopupWindow>();
                AudioManager.instance.niceAudio();
                break;
            case 4:
                WindowManager.instance.Show<GreatPopupWindow>();
                AudioManager.instance.greatAudio();
                break;
            case 5:
                WindowManager.instance.Show<ExcellentPopupWindow>();
                AudioManager.instance.exellentAudio();
                break;
            case 6:
                WindowManager.instance.Show<AmazingPopupWindow>();
                AudioManager.instance.amazingAudio();
                break;
        }
    }

    #endregion

    #region Complete

    bool IsLevelCompleted()
    {
        for (int i = 0; i < targetLeftList.Count; i++)
        {
            if (targetLeftList[i] != 0)
            {
                return false;
            }
        }
        return true;
    }

    // auto play the left moves when target is reached
    IEnumerator PreWinAutoPlay()
    {
        HideHint();

        // reset drop time
        dropTime = 1;

        state = GAME_STATE.OPENING_POPUP;

        yield return new WaitForSeconds(0.5f);

        var duration = WindowManager.instance.Show<CompletedPopupWindow>().GetComponent<Popup>().Duration;

        AudioManager.instance.PopupCompletedAudio();

        yield return new WaitForSeconds(duration + 0.5f);

        state = GAME_STATE.PRE_WIN_AUTO_PLAYING;

        DestroyAllObstacles();

        var items = GetRandomItemsInCurrentPage(moveLeft);

        foreach (var item in items)
        {
            item.SetRandomNextType();
            item.nextSound = false;

            DecreaseMoveLeft(true);

            var prefab = Instantiate(Resources.Load(Configure.StarGold())) as GameObject;
            prefab.transform.position = UITop.GetComponent<UITop>().movesText.gameObject.transform.position;

            var startPosition = prefab.transform.position;
            var endPosition = item.gameObject.transform.position;
            var bending = new Vector3(1, 1, 0);
            var timeToTravel = 0.2f;
            var timeStamp = Time.time;

            while (Time.time < timeStamp + timeToTravel)
            {
                var currentPos = Vector3.Lerp(startPosition, endPosition, (Time.time - timeStamp) / timeToTravel);

                currentPos.x += bending.x * Mathf.Sin(Mathf.Clamp01((Time.time - timeStamp) / timeToTravel) * Mathf.PI);
                currentPos.y += bending.y * Mathf.Sin(Mathf.Clamp01((Time.time - timeStamp) / timeToTravel) * Mathf.PI);
                currentPos.z += bending.z * Mathf.Sin(Mathf.Clamp01((Time.time - timeStamp) / timeToTravel) * Mathf.PI);

                prefab.transform.position = currentPos;

                yield return null;
            }

            Destroy(prefab);

            item.Destroy();

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);

        while (GetAllSpecialItemsInCurrentPage().Count > 0)
        {
            while (GetAllSpecialItemsInCurrentPage().Count > 0)
            {
                var specials = GetAllSpecialItemsInCurrentPage();

                var item = specials[Random.Range(0, specials.Count)];

                InitPlaneRandom();

                item.Destroy();

                while (destroyingItems > 0 || playingAnimation > 0)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                yield return new WaitForEndOfFrame();

                Drop();

                while (droppingItems > 0)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                yield return new WaitForEndOfFrame();
            }
            state = GAME_STATE.PRE_WIN_AUTO_PLAYING;

            yield return StartCoroutine(DestroyMatches());
        }

        while (destroyingItems > 0 || playingAnimation > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForEndOfFrame();

        while (droppingItems > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(0.5f);

        state = GAME_STATE.OPENING_POPUP;

        AudioManager.instance.PopupWinAudio();

        WindowManager.instance.Show<WinPopupWindow>();
    }

    public void WinGoldReward(Item item)
    {
        SettingConfig config = GameMainManager.Instance.configManager.settingConfig;

        int maxgold = config.maxgold;
        if (winGold > maxgold)
        {
            winGold = maxgold;
            return;
        }

        if (item.IsPlaneBreaker(item.type))
        {
            winGold += config.planebreaker;
            getWinGold(item, winGold);
        }
        else if (item.IsColumnBreaker(item.type))
        {
            winGold += config.columnbreaker;
            getWinGold(item, winGold);
        }
        else if (item.IsRowBreaker(item.type))
        {
            winGold += config.rowbreaker;
            getWinGold(item, winGold);
        }
        else if (item.IsBombBreaker(item.type))
        {
            winGold += config.bombbreaker;
            getWinGold(item, winGold);
        }
        else if (item.type == ITEM_TYPE.RAINBOW)
        {
            winGold += config.rainbow;
            getWinGold(item, winGold);
        }
        if (winGold > maxgold)
        {
            winGold = maxgold;
        }
    }

    private void getWinGold(Item item, int gold)
    {
    }

    private void DestroyAllObstacles()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var node = GetNode(i, j);
                if (node != null)
                {
                    if (node.ice != null)
                    {
                        Destroy(node.ice.gameObject);
                        node.ice = null;
                    }
                    if (node.jelly != null)
                    {
                        Destroy(node.jelly.gameObject);
                        node.jelly = null;
                    }
                    if (node.cage != null)
                    {
                        Destroy(node.cage.gameObject);
                        node.cage = null;
                    }
                    if (node.packagebox != null)
                    {
                        Destroy(node.packagebox.gameObject);
                        node.packagebox = null;
                    }
                }
            }
        }
    }

    List<Item> GetRandomItems(int number)
    {
        var avaiableItems = new List<Item>();
        var returnItems = new List<Item>();

        foreach (var item in GetListItems())
        {
            if (item != null)
            {
                if (item.node != null)
                {
                    if (item.IsCookie())
                    {
                        avaiableItems.Add(item);
                    }
                }
            }
        }

        while (returnItems.Count < number && avaiableItems.Count > 0)
        {
            var item = avaiableItems[Random.Range(0, avaiableItems.Count)];

            returnItems.Add(item);

            avaiableItems.Remove(item);
        }

        return returnItems;
    }

    List<Item> GetRandomItemsInCurrentPage(int number)
    {
        var avaiableItems = new List<Item>();
        var returnItems = new List<Item>();

        foreach (var item in GetListItemsInCurrentPage())
        {
            if (item != null)
            {
                if (item.node != null)
                {
                    if (item.IsCookie())
                    {
                        avaiableItems.Add(item);
                    }
                }
            }
        }

        while (returnItems.Count < number && avaiableItems.Count > 0)
        {
            var item = avaiableItems[Random.Range(0, avaiableItems.Count)];

            returnItems.Add(item);

            avaiableItems.Remove(item);
        }

        return returnItems;
    }

    List<Item> GetAllSpecialItems()
    {
        var specials = new List<Item>();

        foreach (var item in GetListItems())
        {
            if (item != null)
            {
                if (item.type == ITEM_TYPE.RAINBOW || item.IsColumnBreaker(item.type) || item.IsRowBreaker(item.type) || item.IsBombBreaker(item.type) || item.IsPlaneBreaker(item.type))
                {
                    specials.Add(item);
                }
            }
        }
        return specials;
    }

    /// <summary>
    /// get all special items in current page
    /// </summary>
    /// <returns></returns>
    List<Item> GetAllSpecialItemsInCurrentPage()
    {
        var specials = new List<Item>();

        foreach (var item in GetListItems())
        {
            if (item != null && item.node.IsInCurrentPage())
            {
                if (item.type == ITEM_TYPE.RAINBOW || item.IsColumnBreaker(item.type) || item.IsRowBreaker(item.type) || item.IsBombBreaker(item.type) || item.IsPlaneBreaker(item.type))
                {
                    specials.Add(item);
                }
            }
        }
        return specials;
    }

    public void SaveLevelInfo()
    {
        if (score < LevelLoader.instance.score1Star)
        {
            star = 0;
        }
        else if (LevelLoader.instance.score1Star <= score && score < LevelLoader.instance.score2Star)
        {
            star = 1;
        }
        else if (LevelLoader.instance.score2Star <= score && score < LevelLoader.instance.score3Star)
        {
            star = 2;
        }
        else if (score >= LevelLoader.instance.score2Star)
        {
            star = 3;
        }

        GameData.instance.SaveLevelStatistics(LevelLoader.instance.level, score, star);

        int openedLevel = GameData.instance.GetOpendedLevel();
        if (LevelLoader.instance.level == openedLevel)
        {
            if (openedLevel < Configure.instance.maxLevel)
            {
                GameData.instance.SaveOpendedLevel(openedLevel + 1);
            }
        }

        int coin = GameData.instance.GetPlayerCoin();
        if (star == 1)
        {
            GameData.instance.SavePlayerCoin(coin + Configure.instance.bonus1Star);
        }
        else if (star == 2)
        {
            GameData.instance.SavePlayerCoin(coin + Configure.instance.bonus2Star);
        }
        else if (star == 3)
        {
            GameData.instance.SavePlayerCoin(coin + Configure.instance.bonus3Star);
        }
    }

    #endregion

    #region Hint

    public void CheckHint()
    {
        if (!Configure.instance.showHint)
            return;

        if (moveLeft <= 0)
            return;

        HideHint();

        if (GetHintByRainbowItem() || GetHintByBreaker() || GetHintByColor())
        {
            StartCoroutine(ShowHint());
        }
        else
        {
            state = GAME_STATE.NO_MATCHES_REGENERATING;

            lockSwap = true;

            AudioManager.instance.PopupNoMatchesAudio();

            var popup = WindowManager.instance.Show<NoMatchesdPopupWindow>().GetComponent<Popup>();

            StartCoroutine(OnNoMatchesPoppedUp(popup.Duration));
        }
    }

    public float ScaleOutDuration = 0.1f;
    public float ScaleInDuration = 0.1f;

    private IEnumerator OnNoMatchesPoppedUp(float duration)
    {
        yield return new WaitForSeconds(duration);

        NoMoveRegenerate();

        yield return new WaitForSeconds(Item.ScaleInDuration + Item.ScaleOutDuration);

        while (GetHintByColor() == false)
        {
            NoMoveRegenerate();

            yield return new WaitForSeconds(Item.ScaleInDuration + Item.ScaleOutDuration);
        }

        yield return new WaitForSeconds(0.5f);

        state = GAME_STATE.WAITING_USER_SWAP;

        FindMatches();
    }

    public IEnumerator ShowHint()
    {
        if (isHintShowing)
            yield return null;

        isHintShowing = true;

        DisplayHint();

        if (Configure.instance.showHint == false)
        {
            yield break;
        }

        yield return new WaitForSeconds(Configure.instance.hintDelay);

        while (state != GAME_STATE.WAITING_USER_SWAP)
        {
            yield return new WaitForSeconds(0.1f);
        }

        while (lockSwap)
        {
            yield return new WaitForSeconds(0.1f);
        }

        foreach (var hintItem in hintItems)
        {
            if (hintItem == null)
                continue;
            hintItem.ShowHint();
        }

        // only wait if hint items run animation
        // if there is no item that mean the hint list is clean in clear hint function
        if (hintItems.Count > 0)
        {
            yield return new WaitForSeconds(1.5f);
        }
    }

    public void HideHint()
    {
        DisplayHint();

        isHintShowing = false;

        foreach (var hintItem in hintItems)
        {
            if (hintItem == null)
                continue;

            hintItem.HideHint();
        }
        hintItems.Clear();
    }

    public void DisplayHint()
    {
        Debug.LogWarning("Display Hint");
        foreach (var hintItem in hintItems)
        {
            Debug.LogWarning(hintItem.node);
        }
    }

    List<int> Shuffle(List<int> list)
    {
        System.Random rng = new System.Random();

        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }

    void CheckHintNode(Node node, int color, SWAP_DIRECTION direction)
    {
        if (node != null)
        {
            if (node.item != null && node.item.color == color)
            {
                if (direction == SWAP_DIRECTION.TOP
                    || direction == SWAP_DIRECTION.RIGHT
                    || direction == SWAP_DIRECTION.BOTTOM
                    || direction == SWAP_DIRECTION.LEFT
                    )
                {
                    if (node.item.Exchangeable(direction) && node.item.Matchable())
                    {
                        hintItems.Add(node.item);
                    }
                }
                else
                {
                    if (node.item.Matchable())
                    {
                        hintItems.Add(node.item);
                    }
                }
            }
        }
    }

    void NoMoveRegenerate()
    {
        foreach (var item in GetListItems())
        {
            if (item != null)
            {
                if (item.Exchangeable(SWAP_DIRECTION.NONE) && item.IsCookie())
                {
                    item.color = LevelLoader.instance.RandomColor();
                    item.type = ITEM_TYPE.COOKIE_1 + item.color - 1;
                    item.ChangeSprite(item.color);
                }
            }
        }
    }

    bool GetHintByColor()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        foreach (int color in Shuffle(LevelLoader.instance.usingColors))
        {
            for (int j = 0; j < column; j++)
            {
                for (int i = 0; i < row; i++)
                {
                    Node node = GetNode(i, j);

                    if (node != null)
                    {
                        if (node.item == null || !(node.item.Exchangeable(SWAP_DIRECTION.NONE)))
                        {
                            continue;
                        }

                        // o-o-x
                        //	   o
                        CheckHintNode(GetNode(i + 1, j), color, SWAP_DIRECTION.TOP);
                        CheckHintNode(GetNode(i, j - 1), color, SWAP_DIRECTION.NONE);
                        CheckHintNode(GetNode(i, j - 2), color, SWAP_DIRECTION.NONE);
                        if (hintItems.Count == 3)
                        {
                            return true;
                        }
                        else
                        {
                            hintItems.Clear();
                        }

                        //     o
                        // o-o x
                        CheckHintNode(GetNode(i - 1, j), color, SWAP_DIRECTION.BOTTOM);
                        CheckHintNode(GetNode(i, j - 1), color, SWAP_DIRECTION.NONE);
                        CheckHintNode(GetNode(i, j - 2), color, SWAP_DIRECTION.NONE);
                        if (hintItems.Count == 3)
                        {
                            return true;
                        }
                        else
                        {
                            hintItems.Clear();
                        }

                        // x o o
                        // o
                        CheckHintNode(GetNode(i + 1, j), color, SWAP_DIRECTION.TOP);
                        CheckHintNode(GetNode(i, j + 1), color, SWAP_DIRECTION.NONE);
                        CheckHintNode(GetNode(i, j + 2), color, SWAP_DIRECTION.NONE);
                        if (hintItems.Count == 3)
                        {
                            return true;
                        }
                        else
                        {
                            hintItems.Clear();
                        }

                        // o
                        // x o o
                        CheckHintNode(GetNode(i - 1, j), color, SWAP_DIRECTION.BOTTOM);
                        CheckHintNode(GetNode(i, j + 1), color, SWAP_DIRECTION.NONE);
                        CheckHintNode(GetNode(i, j + 2), color, SWAP_DIRECTION.NONE);
                        if (hintItems.Count == 3)
                        {
                            return true;
                        }
                        else
                        {
                            hintItems.Clear();
                        }

                        // o
                        // o
                        // x o
                        CheckHintNode(GetNode(i, j + 1), color, SWAP_DIRECTION.LEFT);
                        CheckHintNode(GetNode(i - 1, j), color, SWAP_DIRECTION.NONE);
                        CheckHintNode(GetNode(i - 2, j), color, SWAP_DIRECTION.NONE);
                        if (hintItems.Count == 3)
                        {
                            return true;
                        }
                        else
                        {
                            hintItems.Clear();
                        }

                        // x o
                        // o
                        // o
                        CheckHintNode(GetNode(i, j + 1), color, SWAP_DIRECTION.LEFT);
                        CheckHintNode(GetNode(i + 1, j), color, SWAP_DIRECTION.NONE);
                        CheckHintNode(GetNode(i + 2, j), color, SWAP_DIRECTION.NONE);
                        if (hintItems.Count == 3)
                        {
                            return true;
                        }
                        else
                        {
                            hintItems.Clear();
                        }

                        //	 o
                        //   o
                        // o x
                        CheckHintNode(GetNode(i, j - 1), color, SWAP_DIRECTION.RIGHT);
                        CheckHintNode(GetNode(i - 1, j), color, SWAP_DIRECTION.NONE);
                        CheckHintNode(GetNode(i - 2, j), color, SWAP_DIRECTION.NONE);
                        if (hintItems.Count == 3)
                        {
                            return true;
                        }
                        else
                        {
                            hintItems.Clear();
                        }

                        // o x
                        //   o
                        //   o
                        CheckHintNode(GetNode(i, j - 1), color, SWAP_DIRECTION.RIGHT);
                        CheckHintNode(GetNode(i + 1, j), color, SWAP_DIRECTION.NONE);
                        CheckHintNode(GetNode(i + 2, j), color, SWAP_DIRECTION.NONE);
                        if (hintItems.Count == 3)
                        {
                            return true;
                        }
                        else
                        {
                            hintItems.Clear();
                        }

                        // o-x-o-o
                        CheckHintNode(GetNode(i, j - 1), color, SWAP_DIRECTION.RIGHT);
                        CheckHintNode(GetNode(i, j + 1), color, SWAP_DIRECTION.NONE);
                        CheckHintNode(GetNode(i, j + 2), color, SWAP_DIRECTION.NONE);
                        if (hintItems.Count == 3)
                        {
                            return true;
                        }
                        else
                        {
                            hintItems.Clear();
                        }

                        // o-o-x-o
                        CheckHintNode(GetNode(i, j + 1), color, SWAP_DIRECTION.LEFT);
                        CheckHintNode(GetNode(i, j - 1), color, SWAP_DIRECTION.NONE);
                        CheckHintNode(GetNode(i, j - 2), color, SWAP_DIRECTION.NONE);
                        if (hintItems.Count == 3)
                        {
                            return true;
                        }
                        else
                        {
                            hintItems.Clear();
                        }

                        // o
                        // x
                        // o
                        // o
                        CheckHintNode(GetNode(i - 1, j), color, SWAP_DIRECTION.BOTTOM);
                        CheckHintNode(GetNode(i + 1, j), color, SWAP_DIRECTION.NONE);
                        CheckHintNode(GetNode(i + 2, j), color, SWAP_DIRECTION.NONE);
                        if (hintItems.Count == 3)
                        {
                            return true;
                        }
                        else
                        {
                            hintItems.Clear();
                        }

                        // o
                        // o
                        // x
                        // o
                        CheckHintNode(GetNode(i + 1, j), color, SWAP_DIRECTION.TOP);
                        CheckHintNode(GetNode(i - 1, j), color, SWAP_DIRECTION.NONE);
                        CheckHintNode(GetNode(i - 2, j), color, SWAP_DIRECTION.NONE);
                        if (hintItems.Count == 3)
                        {
                            return true;
                        }
                        else
                        {
                            hintItems.Clear();
                        }

                        //   o
                        // o x o
                        //   o
                        int h = 0;
                        int v = 0;
                        Node neighbor = null;

                        neighbor = node.LeftNeighbor();
                        if (neighbor != null)
                        {
                            if (neighbor.item != null && neighbor.item.Matchable() && neighbor.item.color == color)
                            {
                                hintItems.Add(neighbor.item);

                                h++;
                            }
                        }

                        neighbor = node.RightNeighbor();
                        if (neighbor != null)
                        {
                            if (neighbor.item != null && neighbor.item.Matchable() && neighbor.item.color == color)
                            {
                                hintItems.Add(neighbor.item);

                                h++;
                            }
                        }

                        neighbor = node.TopNeighbor();
                        if (neighbor != null)
                        {
                            if (neighbor.item != null && neighbor.item.Matchable() && neighbor.item.color == color)
                            {
                                hintItems.Add(neighbor.item);

                                v++;
                            }
                        }

                        neighbor = node.BottomNeighbor();
                        if (neighbor != null)
                        {
                            if (neighbor.item != null && neighbor.item.Matchable() && neighbor.item.color == color)
                            {
                                hintItems.Add(neighbor.item);

                                v++;
                            }
                        }

                        if (hintItems.Count == 3)
                        {
                            if (v > h && hintItems[0].node.item != null)
                            {
                                if (hintItems[0].node == node.LeftNeighbor() && hintItems[0].node.item.Exchangeable(SWAP_DIRECTION.RIGHT))
                                {
                                    return true;
                                }
                                else if (hintItems[0].node == node.RightNeighbor() &&
                                         hintItems[0].node.item.Exchangeable(SWAP_DIRECTION.LEFT))
                                {
                                    return true;
                                }
                                else
                                {
                                    hintItems.Clear();
                                }
                            }
                            else if (v < h && hintItems[2].node.item != null)
                            {
                                if (hintItems[2].node == node.TopNeighbor() && hintItems[2].node.item.Exchangeable(SWAP_DIRECTION.BOTTOM))
                                {
                                    return true;
                                }
                                else if (hintItems[2].node == node.BottomNeighbor() && hintItems[2].node.item.Exchangeable(SWAP_DIRECTION.TOP))
                                {
                                    return true;
                                }
                                else
                                {
                                    hintItems.Clear();
                                }
                            }
                            else
                            {
                                hintItems.Clear();
                            }
                        }
                        else if (hintItems.Count == 4)
                        {
                            if (hintItems[0].node.item.Exchangeable(SWAP_DIRECTION.RIGHT))
                            {
                                hintItems.RemoveAt(1);

                                return true;
                            }
                            else if (hintItems[1].node.item.Exchangeable(SWAP_DIRECTION.LEFT))
                            {
                                hintItems.RemoveAt(0);

                                return true;
                            }
                            else if (hintItems[2].node.item.Exchangeable(SWAP_DIRECTION.BOTTOM))
                            {
                                hintItems.RemoveAt(3);

                                return true;
                            }
                            else if (hintItems[3].node.item.Exchangeable(SWAP_DIRECTION.TOP))
                            {
                                hintItems.RemoveAt(2);

                                return true;
                            }
                            else
                            {
                                hintItems.Clear();
                            }
                        }
                        else
                        {
                            hintItems.Clear();
                        }
                    }
                }
            }
        }
        return false;
    }

    bool GetHintByRainbowItem()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                Node node = GetNode(i, j);
                if (node != null)
                {
                    if (node.item == null)
                    {
                        continue;
                    }

                    if (node.item.type == ITEM_TYPE.RAINBOW)
                    {
                        Node neighbor = null;

                        neighbor = node.LeftNeighbor();
                        if (neighbor != null)
                        {
                            if (neighbor.item != null && neighbor.item.Exchangeable(SWAP_DIRECTION.RIGHT))
                            {
                                hintItems.Add(node.item);

                                return true;
                            }
                        }

                        neighbor = node.RightNeighbor();
                        if (neighbor != null)
                        {
                            if (neighbor.item != null && neighbor.item.Exchangeable(SWAP_DIRECTION.LEFT))
                            {
                                hintItems.Add(node.item);

                                return true;
                            }
                        }

                        neighbor = node.TopNeighbor();
                        if (neighbor != null)
                        {
                            if (neighbor.item != null && neighbor.item.Exchangeable(SWAP_DIRECTION.BOTTOM))
                            {
                                hintItems.Add(node.item);

                                return true;
                            }
                        }

                        neighbor = node.BottomNeighbor();
                        if (neighbor != null)
                        {
                            if (neighbor.item != null && neighbor.item.Exchangeable(SWAP_DIRECTION.TOP))
                            {
                                hintItems.Add(node.item);

                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    bool GetHintByBreaker()
    {
        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                Node node = GetNode(i, j);
                if (node != null)
                {
                    if (node.item == null || !(node.item.Exchangeable(SWAP_DIRECTION.NONE)))
                    {
                        continue;
                    }

                    if (node.item.IsBreaker(node.item.type))
                    {
                        hintItems.Add(node.item);

                        return true;
                    }
                }
            }
        }

        return false;
    }

    #endregion

    #region Gingerbread

    bool GenerateGingerbread()
    {
        if (!IsGingerbreadTarget())
        {
            return false;
        }

        if (skipGenerateGingerbread)
        {
            return false;
        }

        // calculate the total gingerbread need to generate
        var needGenerate = 0;
        for (int i = 0; i < LevelLoader.instance.targetList.Count; i++)
        {
            if (LevelLoader.instance.targetList[i].Type == TARGET_TYPE.GINGERBREAD)
            {
                needGenerate += targetLeftList[i];
            }
        }

        if (needGenerate <= 0)
        {
            return false;
        }

        // check gingerbread on board
        var amount = GingerbreadOnBoard().Count;

        if (amount >= LevelLoader.instance.maxGingerbread)
        {
            return false;
        }

        // prevent multiple call
        if (generatingGingerbread)
        {
            return false;
        }

        // skip generate randomly
        if (Random.Range(0, 2) == 0 && skipGingerbreadCount < 2)
        {
            skipGingerbreadCount++;
            return false;
        }
        skipGingerbreadCount = 0;

        generatingGingerbread = true;

        // get node to generate gingerbread
        var row = LevelLoader.instance.row - 1;
        var column = LevelLoader.instance.gingerbreadMarkers[Random.Range(0, LevelLoader.instance.gingerbreadMarkers.Count)];

        var node = GetNode(row, column);
        if (node != null && node.item != null)
        {
            node.item.ChangeToGingerbread(LevelLoader.instance.RandomGingerbread());
            return true;
        }

        return false;
    }

    bool IsGingerbreadTarget()
    {

        for (int i = 0; i < LevelLoader.instance.targetList.Count; i++)
        {
            if (LevelLoader.instance.targetList[i].Type == TARGET_TYPE.GINGERBREAD)
            {
                return true;
            }
        }
        return false;
    }

    List<Item> GingerbreadOnBoard()
    {
        var list = new List<Item>();

        var items = GetListItems();

        foreach (var item in items)
        {
            if (item != null && item.IsGingerbread())
            {
                list.Add(item);
            }
        }

        return list;
    }

    bool MoveGingerbread()
    {
        if (IsGingerbreadTarget() == false)
        {
            return false;
        }

        // prevent multiple call
        if (movingGingerbread)
        {
            return false;
        }

        movingGingerbread = true;

        var isMoved = false;
        foreach (var gingerbread in GingerbreadOnBoard())
        {
            if (gingerbread != null)
            {
                var upper = GetUpperItem(gingerbread.node);

                if (upper != null && upper.node != null && upper.IsGingerbread() == false && gingerbread.node.cage == null && gingerbread.node.ice == null)
                {
                    var gingerbreadPosition = NodeLocalPosition(upper.node.i, upper.node.j);
                    var upperItemPosition = NodeLocalPosition(gingerbread.node.i, gingerbread.node.j);

                    gingerbread.neighborNode = upper.node;
                    gingerbread.swapItem = upper;

                    touchedItem = gingerbread;
                    swappedItem = upper;

                    gingerbread.SwapItem();

                    gingerbread.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;

                    // animation
                    iTween.MoveTo(gingerbread.gameObject, iTween.Hash(
                        "position", gingerbreadPosition,
                        "easetype", iTween.EaseType.linear,
                        "time", Configure.instance.swapTime
                    ));

                    iTween.MoveTo(upper.gameObject, iTween.Hash(
                        "position", upperItemPosition,
                        "easetype", iTween.EaseType.linear,
                        "time", Configure.instance.swapTime
                    ));
                }
                else if (upper == null || upper.node == null)
                {
                    AudioManager.instance.GingerbreadExplodeAudio();

                    gingerbread.color = LevelLoader.instance.RandomColor();

                    gingerbread.ChangeSpriteAndType(gingerbread.color);

                    // after changing a gingerbread to a cookie. skip generate one turn on generate call right after this function
                    skipGenerateGingerbread = true;
                }

                isMoved = true;
            }
        }

        return isMoved;
    }

    public Item GetUpperItem(Node node)
    {
        var top = node.TopNeighbor();

        if (top == null)
        {
            return null;
        }

        if (top.tile.type == TILE_TYPE.NONE || top.tile.type == TILE_TYPE.PASSTHROUGH)
        {
            return GetUpperItem(top);
        }

        if (top.item != null && top.item.Movable())
        {
            return top.item;
        }

        return node.item;
    }

    #endregion

    #region Booster

    void DestroyBoosterItems(Item boosterItem)
    {
        if (boosterItem == null)
        {
            return;
        }

        if (boosterItem.Destroyable() && booster != BOOSTER_TYPE.OVEN_BREAKER)
        {
            if (booster == BOOSTER_TYPE.RAINBOW_BREAKER && boosterItem.IsCookie() == false)
            {
                return;
            }

            lockSwap = true;

            switch (booster)
            {
                case BOOSTER_TYPE.SINGLE_BREAKER:
                    DestroySingleBooster(boosterItem);
                    break;
                case BOOSTER_TYPE.ROW_BREAKER:
                    StartCoroutine(DestroyRowBooster(boosterItem));
                    break;
                case BOOSTER_TYPE.COLUMN_BREAKER:
                    StartCoroutine(DestroyColumnBooster(boosterItem));
                    break;
                case BOOSTER_TYPE.RAINBOW_BREAKER:
                    StartCoroutine(DestroyRainbowBooster(boosterItem));
                    break;
            }

            Booster.instance.BoosterComplete();
        }

        if (boosterItem.Movable() && booster == BOOSTER_TYPE.OVEN_BREAKER)
        {
            StartCoroutine(DestroyOvenBooster(boosterItem));
        }
    }

    void DestroySingleBooster(Item boosterItem)
    {
        HideHint();

        AudioManager.instance.SingleBoosterAudio();

        boosterItem.Destroy();

        FindMatches();
    }

    IEnumerator DestroyRowBooster(Item boosterItem)
    {
        AudioManager.instance.RowBoosterAudio();

        // destroy a row
        var items = new List<Item>();

        items = RowItems(boosterItem.node.i);

        foreach (var item in items)
        {
            // this item maybe destroyed in other call
            if (item != null)
            {
                item.Destroy(false, false, Item.DestroyBy.Booster);
            }

            yield return new WaitForSeconds(0.1f);
        }

        FindMatches();
    }

    IEnumerator DestroyColumnBooster(Item boosterItem)
    {
        AudioManager.instance.ColumnBoosterAudio();

        // destroy a row
        var items = new List<Item>();

        items = ColumnItems(boosterItem.node.j);

        foreach (var item in items)
        {
            // this item maybe destroyed in other call
            if (item != null)
            {
                item.Destroy(false, false, Item.DestroyBy.Booster);
            }

            yield return new WaitForSeconds(0.1f);
        }

        FindMatches();
    }

    IEnumerator DestroyRainbowBooster(Item boosterItem)
    {
        AudioManager.instance.RainbowBoosterAudio();

        boosterItem.DestroyItemsSameColor(boosterItem.color);

        yield return new WaitForFixedUpdate();
    }

    IEnumerator DestroyOvenBooster(Item boosterItem)
    {
        if (ovenTouchItem == null)
        {
            ovenTouchItem = boosterItem;

            ovenTouchItem.node.AddOvenBoosterActive();

            AudioManager.instance.ButtonClickAudio();
        }
        else
        {
            // the same item
            if (ovenTouchItem.node.OrderOnBoard() == boosterItem.node.OrderOnBoard())
            {
                // remove active
                ovenTouchItem.node.RemoveOvenBoosterActive();

                ovenTouchItem = null;

                AudioManager.instance.ButtonClickAudio();
            }
            // swap
            else
            {
                lockSwap = true;

                boosterItem.node.AddOvenBoosterActive();

                AudioManager.instance.OvenBoosterAudio();

                AudioManager.instance.ButtonClickAudio();

                // animation
                iTween.MoveTo(ovenTouchItem.gameObject, iTween.Hash(
                    "position", boosterItem.gameObject.transform.position,
                    "easetype", iTween.EaseType.linear,
                    "time", Configure.instance.swapTime
                ));

                iTween.MoveTo(boosterItem.gameObject, iTween.Hash(
                    "position", ovenTouchItem.gameObject.transform.position,
                    "easetype", iTween.EaseType.linear,
                    "time", Configure.instance.swapTime
                ));

                yield return new WaitForSeconds(Configure.instance.swapTime);

                ovenTouchItem.node.RemoveOvenBoosterActive();
                boosterItem.node.RemoveOvenBoosterActive();

                var ovenTouchNode = ovenTouchItem.node;
                var boosterItemNode = boosterItem.node;

                // swap item
                ovenTouchNode.item = boosterItem;
                boosterItemNode.item = ovenTouchItem;

                // swap node
                ovenTouchItem.node = boosterItemNode;
                boosterItem.node = ovenTouchNode;

                // swap on hierarchy
                ovenTouchItem.gameObject.transform.SetParent(boosterItemNode.gameObject.transform);
                boosterItem.gameObject.transform.SetParent(ovenTouchNode.gameObject.transform);

                yield return new WaitForEndOfFrame();

                ovenTouchItem = null;

                Booster.instance.BoosterComplete();

                yield return new WaitForSeconds(0.1f);

                FindMatches();
            }
        }
        yield return new WaitForFixedUpdate();
    }

    #endregion 

    #region Collectible

    List<ITEM_TYPE> CheckGenerateCollectible()
    {
        bool hasCollectible = false;
        for (int i = 0; i < LevelLoader.instance.targetList.Count; i++)
        {
            if (LevelLoader.instance.targetList[i].Type == TARGET_TYPE.COLLECTIBLE)
            {
                hasCollectible = true;
                break;
            }
        }
        if (!hasCollectible)
        {
            return null;
        }

        var collectibles = new List<ITEM_TYPE>();

        if (CollectibleOnBoard() >= LevelLoader.instance.collectibleMaxOnBoard)
        {
            return null;
        }

        for (int j = 0; j < LevelLoader.instance.targetList.Count; j++)
        {
            TARGET_TYPE targetType = TARGET_TYPE.NONE;
            int targetColor = 0;
            int collectibleOnBoard = 0;
            int targetLeft = 0;

            targetType = LevelLoader.instance.targetList[j].Type;
            targetColor = LevelLoader.instance.targetList[j].color;
            collectibleOnBoard = CollectibleOnBoard(LevelLoader.instance.targetList[j].color);
            targetLeft = targetLeftList[j];

            if (targetType == TARGET_TYPE.COLLECTIBLE && collectibleOnBoard < targetLeft)
            {
                for (int k = 0; k < targetLeft - collectibleOnBoard; k++)
                {
                    collectibles.Add(ColorToCollectible(targetColor));
                }
            }
        }
        return collectibles;
    }

    ITEM_TYPE ColorToCollectible(int color)
    {
        switch (color)
        {
            case 1:
                return ITEM_TYPE.COLLECTIBLE_1;
            case 2:
                return ITEM_TYPE.COLLECTIBLE_2;
            case 3:
                return ITEM_TYPE.COLLECTIBLE_3;
            case 4:
                return ITEM_TYPE.COLLECTIBLE_4;
            case 5:
                return ITEM_TYPE.COLLECTIBLE_5;
            case 6:
                return ITEM_TYPE.COLLECTIBLE_6;
            case 7:
                return ITEM_TYPE.COLLECTIBLE_7;
            case 8:
                return ITEM_TYPE.COLLECTIBLE_8;
            case 9:
                return ITEM_TYPE.COLLECTIBLE_9;
            case 10:
                return ITEM_TYPE.COLLECTIBLE_10;
            case 11:
                return ITEM_TYPE.COLLECTIBLE_11;
            case 12:
                return ITEM_TYPE.COLLECTIBLE_12;
            case 13:
                return ITEM_TYPE.COLLECTIBLE_13;
            case 14:
                return ITEM_TYPE.COLLECTIBLE_14;
            case 15:
                return ITEM_TYPE.COLLECTIBLE_15;
            case 16:
                return ITEM_TYPE.COLLECTIBLE_16;
            case 17:
                return ITEM_TYPE.COLLECTIBLE_17;
            case 18:
                return ITEM_TYPE.COLLECTIBLE_18;
            case 19:
                return ITEM_TYPE.COLLECTIBLE_19;
            case 20:
                return ITEM_TYPE.COLLECTIBLE_20;
            default:
                return ITEM_TYPE.NONE;
        }
    }

    int CollectibleOnBoard(int color = 0)
    {
        int amount = 0;

        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var node = GetNode(i, j);

                if (node != null && node.item != null && node.item.IsCollectible())
                {
                    if (color == 0)
                    {
                        amount++;
                    }
                    else
                    {
                        if (node.item.color == color)
                        {
                            amount++;
                        }
                    }
                }
            }
        }

        return amount;
    }

    #endregion

    #region Marshmallow

    bool CheckGenerateMarshmallow()
    {
        bool hasMarshmallow = false;
        for (int i = 0; i < LevelLoader.instance.targetList.Count; i++)
        {
            if (LevelLoader.instance.targetList[i].Type == TARGET_TYPE.MARSHMALLOW)
            {
                hasMarshmallow = true;
                break;
            }
        }
        if (!hasMarshmallow)
        {
            return false;
        }

        var needGenerate = 0;

        for (int i = 0; i < LevelLoader.instance.targetList.Count; i++)
        {
            if (LevelLoader.instance.targetList[i].Type == TARGET_TYPE.MARSHMALLOW)
            {
                needGenerate += targetLeftList[i];
            }
        }

        if (needGenerate + LevelLoader.instance.marshmallowMoreThanTarget <= MarshmallowOnBoard())
        {
            return false;
        }

        return true;
    }

    int MarshmallowOnBoard()
    {
        int amount = 0;

        var row = LevelLoader.instance.row;
        var column = LevelLoader.instance.column;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var node = GetNode(i, j);

                if (node != null && node.item != null && node.item.IsMarshmallow())
                {
                    amount++;
                }
            }
        }

        return amount;
    }

    #endregion

    #region Itemclick

    public void ClickChoseItem(Item item)
    {
        var positiontmp = item.GetMousePosition();
        var swapItem = item;

        if (clickedItem != null)
        {
            //双击特殊块
            if (clickedItem == item && clickedItem.IsBreaker(clickedItem.type))
            {
                HideHint();

                CancelChoseItem();

                if (!item.CheckHelpSwapable(SWAP_DIRECTION.SELFCLICK))
                {
                    return;
                }

                needIncreaseBubble = true;

                movingGingerbread = false;
                generatingGingerbread = false;
                skipGenerateGingerbread = false;

                lockSwap = true;

                dropTime = 1;

                // hide help if need
                GuideManager.instance.Hide();

                DecreaseMoveLeft();

                item.Destroy();

                FindMatches();
                return;
            }
            //点击周围块
            if ((clickedItem.node.TopNeighbor() && item.node == clickedItem.node.TopNeighbor())
                || (clickedItem.node.LeftNeighbor() && item.node == clickedItem.node.LeftNeighbor())
                || (clickedItem.node.RightNeighbor() && item.node == clickedItem.node.RightNeighbor())
                || (clickedItem.node.BottomNeighbor() && item.node == clickedItem.node.BottomNeighbor()))
            {
                swapItem = clickedItem;
                positiontmp = clickedItem.gameObject.transform.position;
            }
            //点击其他块
            else
            {
                CancelChoseItem();

                if (item.type != ITEM_TYPE.BLANK)
                {
                    clickedItem = item;

                    CreateSelectedSprite(item);
                }
            }
        }
        else
        {
            if (item.type != ITEM_TYPE.BLANK)
            {
                clickedItem = item;

                CreateSelectedSprite(item);
            }
        }

        if (swapItem.type != ITEM_TYPE.BLANK)
        {
            swapItem.drag = true;
            swapItem.mousePostion = positiontmp;
            swapItem.deltaPosition = Vector3.zero;

            movingGingerbread = false;
            generatingGingerbread = false;
            skipGenerateGingerbread = false;
        }
    }

    private GameObject effectSelect;

    public void CancelChoseItem()
    {
        if (clickedItem != null)
        {
            if (effectSelect != null)
                effectSelect.SetActive(false);
            clickedItem = null;
        }
    }

    private void CreateSelectedSprite(Item item)
    {
        effectSelect = CFX_SpawnSystem.GetNextObject(Configure.EffectSelect, item.transform);
        effectSelect.transform.position = item.transform.position;
    }

    #endregion

    #region bubble

    public void IncreaseBubble()
    {
        List<Item> prepareToChange = new List<Item>();
        for (int i = 0; i < LevelLoader.instance.row; i++)
        {
            for (int j = 0; j < LevelLoader.instance.column; j++)
            {
                var order = NodeOrder(i, j);
                var item = nodes[order].item;
                if (item != null && item.type == ITEM_TYPE.ROCK_CANDY)
                {
                    var top = item.node.TopNeighbor();
                    var bottom = item.node.BottomNeighbor();
                    var left = item.node.LeftNeighbor();
                    var right = item.node.RightNeighbor();
                    if (top != null && top.item != null && top.item.CanChangeToBubble())
                    {
                        prepareToChange.Add(top.item);
                    }
                    if (bottom != null && bottom.item != null && bottom.item.CanChangeToBubble())
                    {
                        prepareToChange.Add(bottom.item);
                    }
                    if (left != null && left.item != null && left.item.CanChangeToBubble())
                    {
                        prepareToChange.Add(left.item);
                    }
                    if (right != null && right.item != null && right.item.CanChangeToBubble())
                    {
                        prepareToChange.Add(right.item);
                    }
                }
            }
        }
        if (prepareToChange.Count == 0)
            return;

        var rnd = Random.Range(0, prepareToChange.Count);
        ChangeToBubble(prepareToChange[rnd]);
    }

    public void ChangeToBubble(Item item)
    {
        for (int i = 0; i < LevelLoader.instance.targetList.Count; i++)
        {
            if (LevelLoader.instance.targetList[i].Type == TARGET_TYPE.ROCK_CANDY
                && targetLeftList[i] > 0
            )
            {
                targetLeftList[i]++;

                UITarget.UpdateTargetAmount(i);
                break;
            }
        }

        item.node.GenerateItem(ITEM_TYPE.ROCK_CANDY);
        Destroy(item.gameObject);
    }

    #endregion

    #region Change


    // change all items to column-breaker/row-breaker/bomb-breaker/x-breaker when swap a rainbow with a breaker
    public void ChangeItemsType(Vector3 source, int color, ITEM_TYPE changeToType, bool isgrass)
    {
        StartCoroutine(TryToChangeType(source, color, changeToType, isgrass));
    }

    IEnumerator TryToChangeType(Vector3 source, int color, ITEM_TYPE changeToType, bool isgrass)
    {
        List<Item> items = GetListItems();

        CFX_SpawnSystem.GetNextObject(Configure.EffectRainbow, source);

        foreach (var item in items)
        {
            if (item != null)
            {
                if (item.color == color && item.IsCookie() && item.Matchable())
                {
                    var line = CFX_SpawnSystem.GetNextObject(Configure.EffectRainbowLine, source);
                    line.transform.DOMove(item.transform.position, RainbowLineDuration);
                    var effectLine = line.GetComponent<CFX_AutoDestruct>();
                    effectLine.Duration = RainbowLineDuration;

                    StartCoroutine(DoChangeType(item, changeToType, changingList));

                    yield return new WaitForSeconds(RainbowLineInterval);
                }
            }
        }
        yield return new WaitForSeconds(RainbowLineDuration);

        DestroyChangingList(isgrass);
    }

    IEnumerator DoChangeType(Item item, ITEM_TYPE changeToType, List<Item> changingList)
    {
        yield return new WaitForSeconds(RainbowLineDuration);

        bool isChanged = true;
        if (item.node != null && item.node.cage != null)
        {
            item.node.CageExplode();
            isChanged = false;
        }
        if (item.node != null && item.node.ice != null)
        {
            item.node.IceExplode();
            isChanged = false;
        }
        if (item.node != null && item.node.jelly != null)
        {
            if (item.node.JellyExplode())
            {
                isChanged = false;
            }
        }

        if (item.node != null && item.node.packagebox != null)
        {
            item.node.PackageBoxExplode();
            isChanged = false;
        }

        if (item.IsColumnBreaker(changeToType) || item.IsRowBreaker(changeToType))
        {
            if (item.node.CanChangeType())
            {
                CollectItem(item);
                item.ChangeToColRowBreaker();
            }
        }
        else if (item.IsBombBreaker(changeToType))
        {
            if (item.node.CanChangeType())
            {
                CollectItem(item);
                item.ChangeToBombBreaker();
            }
        }
        else if (item.IsPlaneBreaker(changeToType))
        {
            if (item.node.CanChangeType())
            {
                CollectItem(item);
                item.ChangeToPlaneBreaker();
            }
        }

        if (isChanged)
            changingList.Add(item);
    }

    #endregion

    #region MoveLeft

    public void DecreaseMoveLeft(bool effect = false)
    {
        moveLeft--;
        allstep++;
        UITop.DecreaseMoves(effect);

        if (isFirstMove)
        {
            isFirstMove = false;
            GameMainManager.Instance.playerModel.StartLevel();

            int j = LevelLoader.instance.beginItemList.Count;
            for (int i = 0; i < j; i++)
            {
                string itemId = LevelLoader.instance.beginItemList[i];
                //NetManager.instance.userToolsToServer (itemId, "1");
                GameMainManager.Instance.playerModel.UseProp(itemId, 1);
            }
        }
    }

    #endregion


    #region page

    /// <summary>
    /// init level page
    /// </summary>
    void initPage()
    {
        LevelLoader loader = LevelLoader.instance;
        if (loader.GetPageCount() == 0)
        {
            m_ICurrentPageIndex = 0;
            return;
        }
        //last page
        m_ICurrentPageIndex = loader.GetPageCount() - 1;
        //set board position to last page coordinate's position
        Vector2 lastCoord = loader.GetPageCoord(m_ICurrentPageIndex);
        Vector3 boardPos = NodeLocalPosition((int)lastCoord.x, (int)lastCoord.y);
        Vector3 offset = new Vector3(2, 0, 0);
        m_CPageMoveContainer.transform.localPosition = boardPos - offset;
    }

    /// <summary>
    /// view page from last to first
    /// </summary>
    /// <returns></returns>
    IEnumerator viewPages()
    {
        if (!LevelLoader.instance.HaveMultiplePage())
        {
            yield break;
        }

        for (int i = m_ICurrentPageIndex; i > 0; --i)
        {
            int lastIndex = m_ICurrentPageIndex;
            PageMove(m_ICurrentPageIndex - 1);
            while (lastIndex == m_ICurrentPageIndex)
            {
                yield return 0;
            }
            yield return 0;
        }
    }

    /// <summary>
    /// move to index page,and play board move animation
    /// </summary>
    void PageMove(int index)
    {
        if (!LevelLoader.instance.HaveMultiplePage())
        {
            return;
        }
        //to coord
        Vector3 offset = new Vector3(2, 0, 0);
        LevelLoader loader = LevelLoader.instance;
        Vector2 toCoord = loader.GetPageCoord(index);
        Vector3 toPos = NodeLocalPosition((int)toCoord.x, (int)toCoord.y) - offset;

        iTween.MoveTo(m_CPageMoveContainer, iTween.Hash(
                "position", toPos,
                "oncomplete", "OnMovePageComplete",
                "oncompleteparams", index,
                "oncompletetarget", gameObject,
                "easetype", iTween.EaseType.linear,
                "time", 2f,
                "delay", 0.1f
            ));
    }

    /// <summary>
    /// move to page cor
    /// </summary>
    /// <param name="page index"></param>
    /// <returns></returns>
    IEnumerator moveToPageByIndex(int index)
    {
        int lastIndex = m_ICurrentPageIndex;
        PageMove(index);
        while (lastIndex == m_ICurrentPageIndex)
        {
            yield return 0;
        }
    }

    /// <summary>
    /// move page complete
    /// </summary>
    public void OnMovePageComplete(int index)
    {
        m_ICurrentPageIndex = index;
    }

    /// <summary>
    /// should turn page or not
    /// </summary>
    /// <returns></returns>
    public bool ShouldTurnPage()
    {
        //if there is no target item in current page then turn page
        for (int i = 0; i < nodes.Count; ++i)
        {
            if (nodes[i].IsInCurrentPage() && nodes[i].item != null)
            {
                for (int j = 0; j < LevelLoader.instance.targetList.Count; j++)
                {
                    if (nodes[i].item.DoesMatchFlyingTarget(LevelLoader.instance.targetList[j]))
                    {
                        //there still have target item in page
                        return false;
                    }
                }
            }

        }
        return true;
    }

    /// <summary>
    /// unlock Nodes drop in attribute which are in current page 
    /// </summary>
    public void UnlockNodesDropInAttrInPage()
    {
        Vector2 toCoord = LevelLoader.instance.GetPageCoord(m_ICurrentPageIndex);
        int centerRow = (int)toCoord.x;
        int centerCol = (int)toCoord.y;

        int beginRow = Mathf.Max(0, centerRow - LevelLoader.C_PageRow / 2);
        int endRow = centerRow + LevelLoader.C_PageRow / 2;
        int beginCol = Mathf.Max(0, centerCol - LevelLoader.C_PageColumn / 2);
        int endCol = centerCol + LevelLoader.C_PageColumn / 2;

        Node tempNode = null;
        for (int i = beginRow; i <= endRow; ++i)
        {
            for (int j = beginCol; j <= endCol; ++j)
            {
                tempNode = GetNode(i, j);
                //unlock dropin attribute
                if (tempNode != null)
                    tempNode.m_bLockDropAndThrough = false;
            }
        }

    }

    #endregion
}
