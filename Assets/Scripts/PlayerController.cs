using UnityEngine;
using shortRPG.config;
using System.Collections;
using shortRPG.Foe;
using System;
using System.Collections.Generic;

namespace shortRPG.GameMain
{
    public class PlayerController : MonoBehaviour
    {
        public class Character
        {
            public string Name;
            public string Description;
            public float Speed;
            public float DodgeSpeed;
            public int Damage;
        }

        private Character m_Character;
        private bool isDodge = false;
        private bool isAttacking = false;
        private float c_speed;
        private Vector3 moveDir;
        public bool isLive = true;
        private int CurHp;
        private float attackRange = 1f;
        private float attackAngle = 45f;
        private float attackDisplayDuration = 0.2f;
        private float attackDisplayUntil;
        private Vector3 attackOrigin;
        private Vector2 attackDirection;

        private List<string> Inventry = new List<string>();


        public delegate void Handler();
        public event Handler DieHandler;
        public event Action<int> HpChanged;

        public Action interactive;

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
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, tip);
            Gizmos.DrawLine(tip, tip - direction * 0.2f + side * 0.1f);
            Gizmos.DrawLine(tip, tip - direction * 0.2f - side * 0.1f);
            Gizmos.color = previousColor;
        }

        private void DrawAttackRange()
        {
            if (!Application.isPlaying || !isLive || Time.time >= attackDisplayUntil) return;
            if (attackDirection.sqrMagnitude < 0.0001f) return;

            Color previousColor = Gizmos.color;
            Gizmos.color = Color.red;
            float startAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg
                - attackAngle * 0.5f;
            const int segments = 24;
            Vector3 previous = attackOrigin;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (startAngle + attackAngle * i / segments) * Mathf.Deg2Rad;
                Vector3 point = attackOrigin + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * attackRange;
                Gizmos.DrawLine(previous, point);
                previous = point;
            }
            Gizmos.DrawLine(previous, attackOrigin);
            Gizmos.color = previousColor;
        }
        #endregion

        #region mono
        private void Start()
        {
            TestInit();
        }

        private void Update()
        {
            if (!isDodge && !isAttacking)
            {
                GetInput();
            }
        }

        private void FixedUpdate()
        {
            Move();
        }
        #endregion

        public bool PickUp(string id)
        {
            if (string.IsNullOrEmpty(id) || Inventry.Contains(id)) return false;
            Inventry.Add(id);
            return true;
        }

        public bool TryConsumeItems(List<string> requiredIDs)
        {
            if (requiredIDs == null) return true;
            var required = new HashSet<string>(requiredIDs);
            foreach (string id in required)
                if (string.IsNullOrEmpty(id) || !Inventry.Contains(id)) return false;
            foreach (string id in required) RemoveItem(id);
            return true;
        }

        private void RemoveItem(string id)
        {
            if (this.Inventry.Contains(id))
            {
                this.Inventry.Remove(id);
            }
        }

        private void Move()
        {
            this.transform.GetComponent<Rigidbody2D>().velocity = 
                Vector3.Normalize(this.moveDir) * 
                GameConfig.SpeedModifier *
                this.c_speed;
        }
        
        private void GetInput()
        {
            // init
            moveDir = Vector3.zero;

            if (Input.GetKey(GameConfig.UpKey))
            {
                moveDir += Vector3.up;
            }

            if (Input.GetKey(GameConfig.DownKey))
            {
                moveDir += Vector3.down;
            }

            if (Input.GetKey(GameConfig.LeftKey))
            {
                moveDir += Vector3.left;
            }

            if (Input.GetKey(GameConfig.RightKey))
            {
                moveDir += Vector3.right;
            }

            if (Input.GetKeyDown(GameConfig.DodgeKey))
            {
                if (!isDodge)
                {
                    StartCoroutine(Dodge());
                }
            }

            if (Input.GetKeyDown(GameConfig.InteractiveKey))
            {
                this.interactive?.Invoke();
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!isAttacking)
                {
                    StartCoroutine(Atk());
                }
            }

            Input.GetMouseButtonDown(1);
        }

        IEnumerator Atk()
        {
            isAttacking = true;

            moveDir = Vector3.zero;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            Collider2D[] Foes = Physics2D.OverlapCircleAll(new Vector2(this.transform.position.x, this.transform.position.y), attackRange, LayerMask.GetMask("FoeHitBox"));
            Vector3 mousePosScreen = Input.mousePosition;
            Vector3 mouseP = Camera.main.ScreenToWorldPoint(mousePosScreen);
            Vector2 mousePos = new Vector2(mouseP.x, mouseP.y);

            Vector2 AtkDir = mousePos - new Vector2(this.transform.position.x, this.transform.position.y);

            attackOrigin = transform.position;
            attackDirection = AtkDir.normalized;
            attackDisplayUntil = Time.time + attackDisplayDuration;

            foreach (var foe in Foes)
            {
                Vector2 dir = foe.gameObject.transform.position - this.transform.position;
                if (Vector2.Angle(AtkDir, dir) <= attackAngle * 0.5f)
                {
                    foe.gameObject.GetComponent<FoeController>().TakeDamage(m_Character.Damage);
                }
            }

            yield return StartCoroutine(Timer(0.2f));

            isAttacking = false;
        }

        IEnumerator Dodge()
        {
            isDodge = true;
            this.c_speed = m_Character.DodgeSpeed;
            if (this.moveDir == Vector3.zero)
            {
                // this.transform.position += GameConfig.DodgeDist * Vector3.Normalize(this.moveDir);
                this.moveDir = Vector3.left;
            }

            yield return StartCoroutine(Timer(0.2f));

            this.c_speed = m_Character.Speed;
            this.isDodge = false;
        }

        IEnumerator Timer(float time)
        {
            while (time >= 0)
            {
                time -= Time.deltaTime;
                yield return null;
            }
        }

        public void TakeDamage(int dmg)
        {
            if (!isLive) return;
            if (dmg <= 0) return;

            CurHp = Mathf.Max(0, CurHp - dmg);
            HpChanged?.Invoke(CurHp);
            Debug.Log($"{this.gameObject.name} take damage {dmg}");

            if (CurHp <= 0 && isLive)
            {
                Die();
                Debug.Log($"Hp: {CurHp}");
            }
        }

        protected virtual void OnDead()
        {
            DieHandler?.Invoke();
        }

        public void Die()
        {
            // TODO: 暂时用这个先通信
            this.isLive = false;
            OnDead();
        }

        // sim init a character
        private void TestInit()
        {
            this.m_Character = new Character();
            this.m_Character.Speed = 1;
            this.m_Character.DodgeSpeed = 3;
            this.m_Character.Name = "TestChara";
            this.m_Character.Description = "This is a Test Chara";
            this.m_Character.Damage = 1;

            this.CurHp = 5;
            HpChanged?.Invoke(CurHp);
            this.c_speed = m_Character.Speed;
        }
    }
}