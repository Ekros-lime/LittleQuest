using System;
using System.Collections;
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


        private GameObject MapPrefab;

        private void OnDrawGizmos()
        {
            DrawMapPoints();
        }

        private void DrawMapPoints()
        {
            Color previousColor = Gizmos.color;

            if (ExitEntries != null)
            {
                Gizmos.color = Color.red;
                foreach (var exit in ExitEntries)
                {
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

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            MapPrefab = this.gameObject;
        }
    }
}
