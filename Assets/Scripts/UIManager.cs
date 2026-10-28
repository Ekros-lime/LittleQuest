using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace shortRPG.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject NormalPanel;
        [SerializeField] private RectTransform itemContainer;
        private readonly Dictionary<string, Image> itemIcons = new Dictionary<string, Image>();
        [SerializeField] private Sprite HpSprite;
        [SerializeField] private RectTransform hpContainer;
        private readonly List<Image> hpIcons = new List<Image>();

        public void ShowHp(int hp)
        {
            if (hpContainer == null)
            {
                if (NormalPanel == null)
                {
                    Transform panel = transform.Find("NormalPanel");
                    if (panel != null) NormalPanel = panel.gameObject;
                }
                Transform root = NormalPanel != null ? NormalPanel.transform : transform;
                foreach (var rect in root.GetComponentsInChildren<RectTransform>(true))
                {
                    if (rect.name != "Ctx_Hp") continue;
                    hpContainer = rect;
                    break;
                }
            }
            if (hpContainer == null || HpSprite == null)
            {
                Debug.LogWarning("Cannot display HP: missing Ctx_Hp or HpSprite.");
                return;
            }

            hp = Mathf.Max(0, hp);
            while (hpIcons.Count > hp)
            {
                int index = hpIcons.Count - 1;
                Image icon = hpIcons[index];
                hpIcons.RemoveAt(index);
                if (icon == null) continue;
                icon.gameObject.SetActive(false);
                Destroy(icon.gameObject);
            }
            while (hpIcons.Count < hp)
            {
                var iconObject = new GameObject("Hp_" + hpIcons.Count,
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconObject.transform.SetParent(hpContainer, false);
                var image = iconObject.GetComponent<Image>();
                image.sprite = HpSprite;
                image.preserveAspect = true;
                image.raycastTarget = false;
                hpIcons.Add(image);
            }
        }


        public void RemoveItem(string itemID)
        {
            if (!itemIcons.TryGetValue(itemID, out Image icon)) return;
            itemIcons.Remove(itemID);
            if (icon == null) return;
            icon.gameObject.SetActive(false);
            Destroy(icon.gameObject);
        }

        public void ShowItem(string itemID, Sprite sprite)
        {
            if (string.IsNullOrEmpty(itemID) || itemIcons.ContainsKey(itemID)) return;
            if (itemContainer == null)
            {
                if (NormalPanel == null)
                {
                    Transform panel = transform.Find("NormalPanel");
                    if (panel != null) NormalPanel = panel.gameObject;
                }
                if (NormalPanel != null)
                    itemContainer = NormalPanel.transform.Find("Ctx_Item") as RectTransform;
            }
            if (itemContainer == null || sprite == null)
            {
                Debug.LogWarning($"Cannot display item {itemID}: missing Ctx_Item or Sprite.");
                return;
            }

            var iconObject = new GameObject("Item_" + itemID, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = iconObject.GetComponent<RectTransform>();
            rect.SetParent(itemContainer, false);
            var image = iconObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            itemIcons.Add(itemID, image);
        }
    }
}
