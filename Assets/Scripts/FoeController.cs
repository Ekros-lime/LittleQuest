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
        private float attackStartDistance = 0.5f;
        private float attackRange = 0.8f;
        private float attackPreparationTime = 0.8f;
        private float attackRecoveryTime = 0.2f;
        private bool showAttackRange;

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

        private void OnDrawGizmos()
        {
            DrawMoveDirection();
            DrawAttackRange();
        }

        #region Draw Debug
        private void DrawMoveDirection()
        {
            if (!Application.isPlaying) return;
            Rigidbody2D body = GetComponent<Rigidbody2D>();
            if (body == null || body.velocity.sqrMagnitude < 0.0001f) return;

            Vector3 direction = new Vector3(body.velocity.x, body.velocity.y, 0f).normalized;
            Vector3 origin = transform.position;
            Vector3 tip = origin + direction;
            Vector3 side = new Vector3(-direction.y, direction.x, 0f);
            Color previousColor = Gizmos.color;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, tip);
            Gizmos.DrawLine(tip, tip - direction * 0.2f + side * 0.1f);
            Gizmos.DrawLine(tip, tip - direction * 0.2f - side * 0.1f);
            Gizmos.color = previousColor;
        }

        private void DrawAttackRange()
        {
            if (!Application.isPlaying || !isLive || !showAttackRange) return;
            Color previousColor = Gizmos.color;
            Gizmos.color = Color.red;
            const int segments = 48;
            Vector3 origin = transform.position;
            Vector3 previous = origin + Vector3.right * attackRange;
            for (int i = 1; i <= segments; i++)
            {
                float angle = 2f * Mathf.PI * i / segments;
                Vector3 point = origin + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * attackRange;
                Gizmos.DrawLine(previous, point);
                previous = point;
            }
            Gizmos.color = previousColor;
        }
        #endregion

        private void OnDisable()
        {
            showAttackRange = false;
        }

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
                if (dist <= attackStartDistance)
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
                if (dist <= attackStartDistance)
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
            showAttackRange = true;
            yield return StartCoroutine(Timer(attackPreparationTime));
            Collider2D player = Physics2D.OverlapCircle(new Vector2(this.transform.position.x, this.transform.position.y), attackRange, LayerMask.GetMask("PlayerHitBox"));
            if (player != null)
            {
                player.gameObject.GetComponent<PlayerController>().TakeDamage(this.Attack);
                if (this == null) yield break;
                if (!gameObject.activeInHierarchy) yield break;
                if (!isLive) yield break;
                // Debug.Log($"{player.gameObject.name} Take {this.Attack} dmg !");
            }
            
            showAttackRange = false;
            yield return StartCoroutine(Timer(attackRecoveryTime));
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