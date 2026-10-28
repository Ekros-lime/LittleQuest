using shortRPG.Map;
using System.Collections.Generic;
using UnityEngine;

namespace shortRPG
{
    public enum InteractiveShape
    {
        Circle,
        Box
    }
    public enum InteractiveType
    {
        auto,
        manual
    }
    [System.Serializable]
    public abstract class Interactivable
    {
        public Vector2 position;
        public InteractiveShape shape;
        public InteractiveType type;
        public int Layer;
        public float r;
        public float w;
        public float h;

        public bool Contains(Vector2 localPosition)
        {
            Vector2 offset = localPosition - position;
            if (shape == InteractiveShape.Circle)
                return r >= 0f && offset.sqrMagnitude <= r * r;
            return w >= 0f && h >= 0f &&
                Mathf.Abs(offset.x) <= w * 0.5f && Mathf.Abs(offset.y) <= h * 0.5f;
        }
    }

    [System.Serializable]
    public class ExitInteractive : Interactivable
    {
        public string ExitID;
        public string TargetMapID;
        public bool Enabled;

        public ExitInteractive(ExitEntry e)
        {
            this.Enabled = !e.InitiallyDisabled;
            this.type = e.type;
            this.ExitID = e.ExitID;
            this.position = e.Position;
            this.w = e.Width;
            this.h = e.Height;
            this.TargetMapID = e.TargetMapID;
            this.shape = InteractiveShape.Box;
            this.Layer = 5;
        }
    }

    [System.Serializable]
    public class ItemInteractive : Interactivable
    {
        public string ItemID;

        public Sprite sprite;

        public ItemInteractive() : this(Vector2.zero, string.Empty, 0.5f) { }

        public ItemInteractive(Vector2 pos, string id, float r)
        {
            this.ItemID = id;
            this.position = pos;
            this.r = r;
            this.shape = InteractiveShape.Circle;
            this.Layer = 1;
        }
    }

    public enum QuestEffectType
    {
        None,
        HideObject,
        EnableExit
    }

    [System.Serializable]
    public class QuestEffect
    {
        public QuestEffectType Type = QuestEffectType.None;
        public string TargetID;
    }

    [System.Serializable]
    public class QuestInteractive : Interactivable
    {
        public List<string> NeedItemID;
        public Sprite sprite;
        public List<QuestEffect> Effects = new List<QuestEffect>();
    }
}
