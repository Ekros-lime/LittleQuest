using shortRPG.config;
using shortRPG.GameMain;
using System.Collections;
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
        private int Attack = 1;
        // private bool isTracking = false;

        private FoeState state = FoeState.Idle;

        public enum FoeState
        {
            Idle,
            Tracking,
            AtkPre,
            Attacking
        };

        //Temp
        public GameObject Player;

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            CheckState();
        }

        private void FixedUpdate()
        {
            if (this.state == FoeState.AtkPre)
            {
                this.state = FoeState.Attacking;
                StartCoroutine(Atk());

            }

            if (this.state == FoeState.Tracking)
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

        // 检查转换到其他状态
        private void CheckState()
        {
            float dist = DistToPlayer();
            if (this.state == FoeState.Idle)
            {
                //Collider2D playCol = Physics2D.OverlapCircle(new Vector2(this.transform.position.x, this.transform.position.y), 2, LayerMask.GetMask("Player"));
                //if (playCol != null)
                //{
                //    this.state = FoeState.Tracking;
                //    return;
                //}
                if (dist <= 0.5f)
                {
                    this.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    this.state = FoeState.AtkPre;
                    return;
                }
                else if (dist <= 2)
                {
                    this.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    this.state = FoeState.Tracking;
                    return;
                }
            }
            else if (state == FoeState.Tracking)
            {
                //Collider2D playCol = Physics2D.OverlapCircle(new Vector2(this.transform.position.x, this.transform.position.y), 4, LayerMask.GetMask("Player"));
                //if (playCol == null)
                //{
                //    this.state = FoeState.Idle;

                //    // temp velocity = 0, future, go back or ??
                //    this.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                //    return;
                //}
                if (dist <= 0.5f)
                {
                    this.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    this.state = FoeState.AtkPre;
                    return;
                }
                else if (dist > 4f)
                {
                    this.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    this.state = FoeState.Idle;
                    return;
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

        IEnumerator Timer(float time)
        {
            while (time >= 0)
            {
                time -= Time.deltaTime;
                yield return null;
            }
        }
        IEnumerator Atk()
        {
            this.state = FoeState.Attacking;
            yield return StartCoroutine(Timer(0.8f));
            Collider2D player = Physics2D.OverlapCircle(new Vector2(this.transform.position.x, this.transform.position.y), 0.8f, LayerMask.GetMask("Player"));
            if (player != null)
            {
                player.gameObject.GetComponent<PlayerController>().TakeDamage(this.Attack);
                Debug.Log($"{player.gameObject.name} Take {this.Attack} dmg !");
            }
            
            yield return StartCoroutine(Timer(0.2f));
            this.state = FoeState.Tracking;
        }

        // sim init
        private void Init()
        {
            Speed = 0.6f;
            MaxHp = 5;
            CurHp = MaxHp;

            this.Player = GameObject.FindGameObjectWithTag("Player");
        }
    }
}