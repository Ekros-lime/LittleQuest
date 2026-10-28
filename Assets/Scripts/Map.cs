using System;
using System.Collections.Generic;
using UnityEngine;

namespace shortRPG.Map
{
    [Serializable]
    public struct ExitEntry
    {
        public string ExitID;
        public Vector2 Position;
        public float Width;
        public float Height;
        public string TargetMapID;
        public InteractiveType type;
        public bool InitiallyDisabled;
    }
    [Serializable]
    public struct EntranceEntry
    {
        public Vector2 Position;
        public string FromExitID;
    }

    public class Map : MonoBehaviour
    {
        public string ID;
        public List<ExitEntry> ExitEntries;
        public List<EntranceEntry> Entrances;

        public List<ItemInteractive> Items = new List<ItemInteractive>();
        public List<QuestInteractive> Quests = new List<QuestInteractive>();
        private List<Interactivable> interactivables = new List<Interactivable>();

        private readonly Dictionary<QuestInteractive, GameObject> questObjects =
            new Dictionary<QuestInteractive, GameObject>();
        private readonly Dictionary<ItemInteractive, GameObject> itemObjects =
            new Dictionary<ItemInteractive, GameObject>();

        private readonly HashSet<Interactivable> automaticInside = new HashSet<Interactivable>();
        private readonly HashSet<QuestInteractive> completedQuests = new HashSet<QuestInteractive>();

        public void RemovePickedItem(ItemInteractive item)
        {
            if (item == null) return;
            RemoveItemObject(item);
            interactivables.Remove(item);
            if (Items != null) Items.Remove(item);
        }

        public void FilterPickedItems(HashSet<string> pickedItemIDs)
        {
            if (Items != null)
                foreach (var item in Items)
                    if (item != null && pickedItemIDs.Contains(item.ItemID))
                        RemoveItemObject(item);
            interactivables.RemoveAll(entry => entry is ItemInteractive item &&
                pickedItemIDs.Contains(item.ItemID));
            if (Items != null)
                Items.RemoveAll(item => item != null && pickedItemIDs.Contains(item.ItemID));
        }

        private void GenerateItemObjects()
        {
            if (Items == null || Items.Count == 0) return;
            Transform container = transform.Find("Items");
            if (container == null)
            {
                container = new GameObject("Items").transform;
                container.SetParent(transform, false);
            }

            foreach (var item in Items)
            {
                if (item == null || itemObjects.ContainsKey(item)) continue;
                var itemObject = new GameObject("Item_" + item.ItemID);
                itemObject.transform.SetParent(container, false);
                // Positions are relative to the map, regardless of the container offset.
                itemObject.transform.position = transform.position +
                    new Vector3(item.position.x, item.position.y, 0f);
                var spriteRenderer = itemObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = item.sprite;
                spriteRenderer.sortingOrder = 1;
                itemObjects.Add(item, itemObject);
            }
        }

        private void GenerateQuestObjects()
        {
            if (Quests == null || Quests.Count == 0) return;
            Transform container = transform.Find("Quests");
            if (container == null)
            {
                container = new GameObject("Quests").transform;
                container.SetParent(transform, false);
            }

            foreach (var quest in Quests)
            {
                if (quest == null || questObjects.ContainsKey(quest)) continue;
                var questObject = new GameObject("Quest_" + questObjects.Count);
                questObject.transform.SetParent(container, false);
                questObject.transform.position = transform.position +
                    new Vector3(quest.position.x, quest.position.y, 0f);
                var spriteRenderer = questObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = quest.sprite;
                spriteRenderer.sortingOrder = 1;
                questObjects.Add(quest, questObject);
            }
        }

        private void RemoveItemObject(ItemInteractive item)
        {
            if (!itemObjects.TryGetValue(item, out GameObject itemObject)) return;
            if (itemObject != null)
            {
                itemObject.SetActive(false);
                Destroy(itemObject);
            }
            itemObjects.Remove(item);
        }

        public bool HasExit(string exitID)
        {
            if (string.IsNullOrEmpty(exitID)) return false;
            foreach (var entry in interactivables)
            {
                if (entry is ExitInteractive exit && exit.ExitID == exitID) return true;
            }
            return false;
        }

        public void EnableExit(string exitID)
        {
            foreach (var entry in interactivables)
            {
                if (entry is ExitInteractive exit && exit.ExitID == exitID)
                {
                    exit.Enabled = true;
                }
            }

        }

        public bool CanApplyEffects(QuestInteractive quest)
        {
            if (quest.Effects == null) return true;
            foreach (var effect in quest.Effects)
            {
                if (effect == null || effect.Type == QuestEffectType.None) continue;
                if (effect.Type != QuestEffectType.EnableExit || !HasExit(effect.TargetID))
                {
                    Debug.LogWarning($"Invalid or unsupported quest effect: {effect.Type}, {effect.TargetID}");
                    return false;
                }
            }
            return true;
        }

        public void ApplyEffects(QuestInteractive quest)
        {
            if (quest.Effects == null) return;
            foreach (var effect in quest.Effects)
            {
                if (effect != null && effect.Type == QuestEffectType.EnableExit)
                {
                    EnableExit(effect.TargetID);
                }
            }

        }

        public void RestoreCompletedQuests(HashSet<int> indices)
        {
            if (Quests == null) return;
            foreach (int index in indices)
            {
                if (index < 0 || index >= Quests.Count || Quests[index] == null) continue;
                CompleteQuest(Quests[index]);
                ApplyEffects(Quests[index]);
            }
        }

        public void CompleteQuest(QuestInteractive quest)
        {
            if (quest == null) return;
            completedQuests.Add(quest);
            if (questObjects.TryGetValue(quest, out GameObject questObject))
            {
                if (questObject != null)
                {
                    questObject.SetActive(false);
                    Destroy(questObject);
                }
                questObjects.Remove(quest);
            }
        }

        public Interactivable FindInteraction(Vector2 worldPosition, InteractiveType triggerType)
        {
            Vector2 localPosition = worldPosition - (Vector2)transform.position;
            Interactivable selected = null;
            float selectedDistance = float.PositiveInfinity;
            foreach (var entry in interactivables)
            {
                if (entry == null || entry.type != triggerType) continue;
                if (entry is ExitInteractive exit && !exit.Enabled) continue;
                if (entry is QuestInteractive quest && completedQuests.Contains(quest)) continue;
                bool inside = entry.Contains(localPosition);
                if (triggerType == InteractiveType.auto)
                {
                    if (!inside)
                    {
                        automaticInside.Remove(entry);
                        continue;
                    }
                    if (!automaticInside.Add(entry)) continue;
                }
                else if (!inside) continue;
                float distance = (entry.position - localPosition).sqrMagnitude;
                if (selected == null || entry.Layer < selected.Layer ||
                    (entry.Layer == selected.Layer && distance < selectedDistance))
                {
                    selected = entry;
                    selectedDistance = distance;
                }
            }
            return selected;
        }


        private GameObject MapPrefab;

        private void OnDrawGizmos()
        {
            DrawMapPoints();
            DrawItemInteractionRanges();
            DrawQuestInteractionRanges();
        }

        #region Draw Debug
        private void DrawItemInteractionRanges()
        {
            if (Items == null) return;

            Color previousColor = Gizmos.color;
            Gizmos.color = Color.yellow;
            const int segments = 48;
            foreach (var item in Items)
            {
                if (item == null) continue;
                Vector3 center = transform.position +
                    new Vector3(item.position.x, item.position.y, 0f);
                if (item.shape == InteractiveShape.Box)
                {
                    if (item.w < 0f || item.h < 0f) continue;
                    Gizmos.DrawWireCube(center, new Vector3(item.w, item.h, 0f));
                }
                else
                {
                    if (item.r < 0f) continue;
                    Vector3 previous = center + Vector3.right * item.r;
                    for (int i = 1; i <= segments; i++)
                    {
                        float angle = 2f * Mathf.PI * i / segments;
                        Vector3 point = center +
                            new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * item.r;
                        Gizmos.DrawLine(previous, point);
                        previous = point;
                    }
                }
            }
            Gizmos.color = previousColor;
        }

        private void DrawQuestInteractionRanges()
        {
            if (Quests == null) return;

            Color previousColor = Gizmos.color;
            Gizmos.color = Color.magenta;
            const int segments = 48;
            foreach (var quest in Quests)
            {
                if (quest == null) continue;
                Vector3 center = transform.position +
                    new Vector3(quest.position.x, quest.position.y, 0f);
                if (completedQuests.Contains(quest))
                {
                    Gizmos.color = Color.red;
                    const float markerSize = 0.12f;
                    Vector3 diagonal = new Vector3(markerSize, markerSize, 0f);
                    Vector3 otherDiagonal = new Vector3(markerSize, -markerSize, 0f);
                    Gizmos.DrawLine(center - diagonal, center + diagonal);
                    Gizmos.DrawLine(center - otherDiagonal, center + otherDiagonal);
                    continue;
                }
                Gizmos.color = Color.magenta;
                if (quest.shape == InteractiveShape.Box)
                {
                    if (quest.w < 0f || quest.h < 0f) continue;
                    Gizmos.DrawWireCube(center, new Vector3(quest.w, quest.h, 0f));
                }
                else
                {
                    if (quest.r < 0f) continue;
                    Vector3 previous = center + Vector3.right * quest.r;
                    for (int i = 1; i <= segments; i++)
                    {
                        float angle = 2f * Mathf.PI * i / segments;
                        Vector3 point = center +
                            new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * quest.r;
                        Gizmos.DrawLine(previous, point);
                        previous = point;
                    }
                }
            }
            Gizmos.color = previousColor;
        }

        private void DrawMapPoints()
        {
            Color previousColor = Gizmos.color;

            if (ExitEntries != null)
            {
                foreach (var exit in ExitEntries)
                {
                    bool enabled = !exit.InitiallyDisabled;
                    if (Application.isPlaying)
                    {
                        foreach (var entry in interactivables)
                        {
                            if (!(entry is ExitInteractive runtimeExit) ||
                                runtimeExit.ExitID != exit.ExitID) continue;
                            enabled = runtimeExit.Enabled;
                            break;
                        }
                    }
                    Gizmos.color = enabled ? Color.red : Color.gray;
                    Vector3 center = transform.position +
                        new Vector3(exit.Position.x, exit.Position.y, 0f);
                    Gizmos.DrawWireCube(center, new Vector3(exit.Width, exit.Height, 0f));
                }
            }

            if (Entrances != null)
            {
                Gizmos.color = Color.green;
                foreach (var entrance in Entrances)
                {
                    Vector3 position = transform.position +
                        new Vector3(entrance.Position.x, entrance.Position.y, 0f);
                    Gizmos.DrawWireSphere(position, 0.1f);
                    Gizmos.DrawLine(position - Vector3.right * 0.15f,
                        position + Vector3.right * 0.15f);
                    Gizmos.DrawLine(position - Vector3.up * 0.15f,
                        position + Vector3.up * 0.15f);
                }
            }

            Gizmos.color = previousColor;
        }
        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            MapPrefab = this.gameObject;
            interactivables.Clear();
            if (ExitEntries != null)
                foreach (var exit in ExitEntries)
                    interactivables.Add(new ExitInteractive(exit));
            if (Items != null)
                foreach (var item in Items)
                    if (item != null) interactivables.Add(item);
            GenerateItemObjects();
            if (Quests != null)
                foreach (var quest in Quests)
                    if (quest != null) interactivables.Add(quest);
            GenerateQuestObjects();
        }
    }
}
