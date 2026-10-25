using shortRPG.config;
using System.Security.Cryptography;
using UnityEngine;

namespace shortRPG.Foe
{
    public class FoeController : MonoBehaviour
    {
        private float Speed;
        private int MaxHp;
        private int CurHp;
        private bool isLive = true;
        private bool isTracking = false;

        //Temp
        public GameObject Player;

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            TrackPlayer();
        }

        private void FixedUpdate()
        {
            if (isTracking)
            {
                MoveToPlayer();
            }
        }

        public void TakeDamage(int dmg)
        {
            if (!isLive) return;
            if (dmg <= 0) return;

            CurHp -= dmg;
            Debug.Log($"{this.gameObject.name} take damage {dmg}");

            if (CurHp <= 0 && isLive)
            {
                Die();
            }
        }

        private void Die()
        {
            isLive = false;
            Debug.Log($"{this.gameObject.name} Die !!!");

            // 可以等待播放动画
            Destroy(this.gameObject);
        }

        private void TrackPlayer()
        {
            if (!isTracking)
            {
                Collider2D playCol = Physics2D.OverlapCircle(new Vector2(this.transform.position.x, this.transform.position.y), 2, LayerMask.GetMask("Player"));
                if (playCol != null)
                {
                    isTracking = true;
                }
            }
            else
            {
                Collider2D playCol = Physics2D.OverlapCircle(new Vector2(this.transform.position.x, this.transform.position.y), 4, LayerMask.GetMask("Player"));
                if (playCol == null)
                {
                    isTracking = false;

                    // temp velocity = 0, future, go back or ??
                    this.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                }
            }

        }

        private void MoveToPlayer()
        {
            Vector2 moveDir = new Vector2(Player.transform.position.x, Player.transform.position.y) - 
                new Vector2(this.transform.position.x, this.transform.position.y);

            this.transform.GetComponent<Rigidbody2D>().velocity =
                Vector3.Normalize(moveDir) *
                GameConfig.SpeedModifier *
                this.Speed;
        }

        private float DistToPlayer()
        {
            Vector2 vector = new Vector2(Player.transform.position.x, Player.transform.position.y) - 
                new Vector2(this.transform.position.x, this.transform.position.y);

            return vector.magnitude;
        }

        // sim init
        private void Init()
        {
            Speed = 0.6f;
            MaxHp = 5;
            CurHp = MaxHp;
        }
    }
}