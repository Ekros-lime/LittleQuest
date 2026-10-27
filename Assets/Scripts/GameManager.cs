using shortRPG.GameMain;
using System.Collections.Generic;
using UnityEngine;

namespace shortRPG
{
    public class GameManager : MonoBehaviour
    {
        public List<GameObject> Maps;
        private Dictionary<string, GameObject> _mapDict;
        public GameObject PlayerPrefab;

        private GameObject Player;

        private GameObject curMap;

        private void Awake()
        {
            Init();
        }

        private void Update()
        {

        }

        private void TransMap()
        {
            if (Player == null || curMap == null) return;
            var currentMap = curMap.GetComponent<shortRPG.Map.Map>();
            if (currentMap == null || currentMap.ExitEntries == null) return;

            Vector2 localPosition = Player.transform.position - curMap.transform.position;
            foreach (var exit in currentMap.ExitEntries)
            {
                Vector2 offset = localPosition - exit.Position;
                if (Mathf.Abs(offset.x) > exit.Width / 2f ||
                    Mathf.Abs(offset.y) > exit.Height / 2f) continue;

                if (string.IsNullOrEmpty(exit.ExitID))
                {
                    Debug.LogWarning("The selected map exit has no ExitID.");
                    return;
                }

                if (string.IsNullOrEmpty(exit.TargetMapID) ||
                    !_mapDict.TryGetValue(exit.TargetMapID, out GameObject targetPrefab))
                {
                    Debug.LogWarning($"Target map not found: {exit.TargetMapID}");
                    return;
                }

                var targetMap = targetPrefab.GetComponent<shortRPG.Map.Map>();
                if (targetMap == null || targetMap.Entrances == null) return;
                foreach (var entrance in targetMap.Entrances)
                {
                    if (entrance.FromExitID != exit.ExitID) continue;
                    Vector2 position = entrance.Position;
                    RemoveMap();
                    LoadMap(targetPrefab);
                    Vector3 spawn = curMap.transform.position + new Vector3(position.x, position.y, 0f);
                    spawn.z = Player.transform.position.z;
                    Player.transform.position = spawn;
                    return;
                }
                Debug.LogWarning($"Map {exit.TargetMapID} has no entrance from exit {exit.ExitID}.");
                return;
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
                // DestroyImmediate(this.curMap, true);
                Destroy(curMap);
            }

            this.curMap = null;
        }

        private void Init()
        {
            this._mapDict = new Dictionary<string, GameObject>();
            if (this.Maps != null)
            {
                foreach (var map in this.Maps)
                {
                    this._mapDict[map.GetComponent<shortRPG.Map.Map>().ID] = map;
                }
            }

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

            SubscribePlayerEvents();

        }

        public void SubscribePlayerEvents()
        {
            Player.GetComponent<PlayerController>().interactive += TransMap;
            Player.GetComponent<PlayerController>().DieHandler += HandlePlayerDie;
        }

        private void HandlePlayerDie()
        {
            Reload();
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
}