using shortRPG.GameMain;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<GameObject> Maps;
    public GameObject PlayerPrefab;

    private GameObject Player;

    private GameObject curMap;

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        // TODO: 暂时检测这个
        if (!Player.GetComponent<PlayerController>().isLive)
        {
            Reload();
        }
    }

    private void TransMap()
    {
        Collider2D col = Physics2D.OverlapCircle(new Vector2(Player.transform.position.x, Player.transform.position.y), 0.2f, LayerMask.GetMask("MapExit"));
        if (col != null)
        {
            // 转换地图
            // TODO

            string exit = col.gameObject.name;

            int mapIndex = 0;
            if (exit == "To_A")
            {
                mapIndex = 0;
            }
            else if(exit == "To_B")
            {
                mapIndex = 1;
            }
            RemoveMap();
            LoadMap(this.Maps[mapIndex]);

            // 放置角色
            this.Player.transform.position = curMap.transform.Find("Entrances").transform.position; // TODO
        }
    }

    public void LoadMap(GameObject map)
    {
        this.curMap = Instantiate(map);
        curMap.SetActive(true);
        curMap.transform.position = Vector3.zero;
    }

    public void RemoveMap()
    {
        if (this.curMap != null)
        {
            this.curMap.SetActive(false);
            DestroyImmediate(this.curMap, true);
        }

        this.curMap = null;
    }

    private void Init()
    {
        LoadMap(Maps[1]);

        LoadPlayer();
    }

    private void OnDestroy()
    {
        
    }

    private void LoadPlayer()
    {
        if (this.PlayerPrefab == null) return;
        this.Player = Instantiate(this.PlayerPrefab);
        this.Player.transform.position = Vector3.zero;

        Player.GetComponent<PlayerController>().interactive += TransMap;
    }

    private void RemovePlayer()
    {
        Player.GetComponent<PlayerController>().interactive -= TransMap;
        Destroy(Player);
    }

    // TODO: 后续多项控制移动到此处
    private void Reload()
    {
        RemoveMap();
        RemovePlayer();

        // TODO: 后续记录复活地图，以及复活点
        LoadMap(Maps[1]);
        LoadPlayer();
    }
}
