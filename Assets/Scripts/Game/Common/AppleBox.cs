using UnityEngine;

public class AppleBox : MonoBehaviour
{
    public Item item;
    public Node node;
    public Board board;
    public int beAbleToDestroy;
    public APPLEBOX_TYPE status;

    public int appleNum;

    public void TryToDestroyApple(Item sourceItem)
    {
        if (status > APPLEBOX_TYPE.APPLEBOX_0)
        {
            status = status - 1;

            var obj = CFX_SpawnSystem.GetNextObject(Configure.ToSpawnKey(status), null, false) as GameObject;
            if (obj != null)
                item.gameObject.GetComponent<SpriteRenderer>().sprite = obj.GetComponent<SpriteRenderer>().sprite;
        }

        StartCoroutine(sourceItem.ResetDestroying());
        sourceItem.board.destroyingItems--;
    }

    public void TryToDestroyBox()
    {
        if (status == APPLEBOX_TYPE.APPLEBOX_0)
        {
            Destroy(node.RightNeighbor().item.gameObject);
            node.RightNeighbor().item = null;
            node.RightNeighbor().GenerateItem(ITEM_TYPE.BLANK);

            Destroy(node.BottomNeighbor().item.gameObject);
            node.BottomNeighbor().item = null;
            node.BottomNeighbor().GenerateItem(ITEM_TYPE.BLANK);

            Destroy(node.BottomRightNeighbor().item.gameObject);
            node.BottomRightNeighbor().item = null;
            node.BottomRightNeighbor().GenerateItem(ITEM_TYPE.BLANK);

            node.item = null;
            node.GenerateItem(ITEM_TYPE.BLANK);
            node.board.appleBoxes.Remove(this);
            Destroy(gameObject);
        }
    }
}
