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
        [SerializeField] private shortRPG.UI.UIManager uiManager;
        private readonly Dictionary<string, Sprite> itemSprites = new Dictionary<string, Sprite>();

        private GameObject Player;

        private GameObject curMap;
        private readonly HashSet<string> pickedItemIDs = new HashSet<string>();
        private readonly HashSet<string> inventoryItemIDs = new HashSet<string>();
        private readonly Dictionary<string, HashSet<int>> completedMapQuests =
            new Dictionary<string, HashSet<int>>();

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            HandleInteraction(InteractiveType.auto);
        }

        public event System.Action<string> ItemInteracted;

        public event System.Action<QuestInteractive> QuestInteracted;

        private void HandleInteraction()
        {
            HandleInteraction(InteractiveType.manual);
        }

        private void HandleInteraction(InteractiveType triggerType)
        {
            if (Player == null || curMap == null) return;
            if (!Player.GetComponent<PlayerController>().isLive) return;
            var map = curMap.GetComponent<shortRPG.Map.Map>();
            if (map == null) return;
            Interactivable selected = map.FindInteraction(Player.transform.position, triggerType);
            if (selected is ExitInteractive exit)
            {
                TransMap(exit);
            }
            else if (selected is ItemInteractive item)
            {
                var controller = Player.GetComponent<PlayerController>();
                if (controller == null || string.IsNullOrEmpty(item.ItemID)) return;
                controller.PickUp(item.ItemID);
                pickedItemIDs.Add(item.ItemID);
                inventoryItemIDs.Add(item.ItemID);
                map.RemovePickedItem(item);
                map.FilterPickedItems(pickedItemIDs);
                if (uiManager != null && itemSprites.TryGetValue(item.ItemID, out Sprite sprite))
                    uiManager.ShowItem(item.ItemID, sprite);
                Debug.Log($"Picked up item: {item.ItemID}");
                ItemInteracted?.Invoke(item.ItemID);
            }
            else if (selected is QuestInteractive quest)
            {
                var controller = Player.GetComponent<PlayerController>();
                if (!map.CanApplyEffects(quest)) return;
                if (!controller.TryConsumeItems(quest.NeedItemID))
                {
                    Debug.Log("Quest interaction failed: missing required items.");
                    return;
                }
                if (quest.NeedItemID != null)
                    foreach (string id in quest.NeedItemID)
                    {
                        inventoryItemIDs.Remove(id);
                        if (uiManager != null) uiManager.RemoveItem(id);
                    }
                map.CompleteQuest(quest);
                map.ApplyEffects(quest);
                if (!completedMapQuests.TryGetValue(map.ID, out HashSet<int> completed))
                {
                    completed = new HashSet<int>();
                    completedMapQuests.Add(map.ID, completed);
                }
                completed.Add(map.Quests.IndexOf(quest));
                Debug.Log("Quest interaction succeeded.");
                QuestInteracted?.Invoke(quest);
            }
        }

        private void TransMap(ExitInteractive exit)
        {
            if (Player == null || curMap == null) return;
            if (string.IsNullOrEmpty(exit.ExitID))
            {
                Debug.LogWarning("The selected map exit has no ExitID.");
                return;
            }
            if (string.IsNullOrEmpty(exit.TargetMapID) ||
                !_mapDict.TryGetValue(exit.TargetMapID, out GameObject targetPrefab) || targetPrefab == null)
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
        }

        public void LoadMap(GameObject map)
        {
            this.curMap = Instantiate(map);
            curMap.SetActive(true);
            curMap.transform.position = Vector3.zero;
            var mapData = curMap.GetComponent<shortRPG.Map.Map>();
            if (mapData != null)
            {
                mapData.FilterPickedItems(pickedItemIDs);
                if (completedMapQuests.TryGetValue(mapData.ID, out HashSet<int> completed))
                    mapData.RestoreCompletedQuests(completed);
            }
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
            if (uiManager == null) uiManager = FindObjectOfType<shortRPG.UI.UIManager>();
            itemSprites.Clear();
            this._mapDict = new Dictionary<string, GameObject>();
            if (this.Maps != null)
            {
                foreach (var map in this.Maps)
                {
                    if (map == null) continue;
                    var mapData = map.GetComponent<shortRPG.Map.Map>();
                    if (mapData == null) continue;
                    this._mapDict[mapData.ID] = map;
                    if (mapData.Items == null) continue;
                    foreach (var item in mapData.Items)
                    {
                        if (item == null || string.IsNullOrEmpty(item.ItemID)) continue;
                        if (itemSprites.TryGetValue(item.ItemID, out Sprite existing) &&
                            existing != null && item.sprite != null && existing != item.sprite)
                        {
                            Debug.LogWarning($"Different sprites configured for item: {item.ItemID}");
                            continue;
                        }
                        if (!itemSprites.ContainsKey(item.ItemID) || existing == null)
                            itemSprites[item.ItemID] = item.sprite;
                    }
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

            var controller = Player.GetComponent<PlayerController>();
            foreach (string itemID in inventoryItemIDs)
                controller.PickUp(itemID);
            SubscribePlayerEvents();

        }

        public void SubscribePlayerEvents()
        {
            Player.GetComponent<PlayerController>().interactive += HandleInteraction;
            Player.GetComponent<PlayerController>().DieHandler += HandlePlayerDie;
            Player.GetComponent<PlayerController>().HpChanged += HandleHpChanged;
        }

        private void HandleHpChanged(int hp)
        {
            if (uiManager != null) uiManager.ShowHp(hp);
        }

        private void HandlePlayerDie()
        {
            Reload();
        }

        private void RemovePlayer()
        {
            Player.GetComponent<PlayerController>().interactive -= HandleInteraction;
            Player.GetComponent<PlayerController>().DieHandler -= HandlePlayerDie;
            Player.GetComponent<PlayerController>().HpChanged -= HandleHpChanged;
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