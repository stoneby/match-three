public class Baffle : NodeLayer
{
    public BAFFLE_TYPE type;

    public void DestroyBaffle()
    {
        if (type == BAFFLE_TYPE.BAFFLE_BOTTOM)
        {
            node.bafflebottom = null;
        }
        else if (type == BAFFLE_TYPE.BAFFLE_RIGHT)
        {
            node.baffleright = null;
        }
        Destroy(gameObject);
    }
}
