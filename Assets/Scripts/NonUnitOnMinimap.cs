using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NonUnitOnMinimap : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        if (all == null)
        {
            all = new Queue<NonUnitOnMinimap>[10];
            for(int i = 0; i < all.Length; ++i)
            {
                all[i] = new Queue<NonUnitOnMinimap>();
            }
        }
        all[renderPriority].Enqueue(this);
    }
    void Start()
    {
      if (all != null)
        {
            for (int i = 0; i < all.Length; ++i)
            {
                while (all[i].Count != 0)
                {
                    all[i].Dequeue().Init();
                }
            }
            all = null;
        }
    }
    void Init()
    {
        myTile = Instantiate(GameMaster.singleton.tilePrefab, GameMaster.singleton.RUtile.transform);
        sr = myTile.GetComponent<Image>();
        sr.color = GetComponent<MeshRenderer>().material.color;
        myTile.name = name;
        Vector2 thisSizeInUnits = new Vector2((scaleFactorFromShape) * transform.localScale.x, (scaleFactorFromShape) * transform.localScale.z);
        sr.transform.position = GameMaster.singleton.calculateTilePosition(gameObject);
        sr.transform.localScale = GameMaster.singleton.GetTileScale(thisSizeInUnits);
    }
    static Queue<NonUnitOnMinimap>[] all;
    // Update is called once per frame
    GameObject myTile;
    Image sr;
    static bool ru = true;
    public float scaleFactorFromShape=1;
    [Range(1,9)]
    public int renderPriority;

    void Update()
    {
    }
}
