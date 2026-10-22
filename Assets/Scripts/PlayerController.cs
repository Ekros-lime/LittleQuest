using UnityEngine;
using shortRPG.config;
using System.Collections;

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
        }

        private Character m_Character;
        private bool isDodge = false;
        private float c_speed;
        private Vector3 moveDir;

        private void Awake()
        {
            TestInit();
        }

        private void Update()
        {
            if (!isDodge)
            {
                GetInput();
            }
        }

        private void FixedUpdate()
        {
            Move();
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
                isDodge = true;
                this.c_speed = m_Character.DodgeSpeed;
                if (this.moveDir == Vector3.zero)
                {
                    // this.transform.position += GameConfig.DodgeDist * Vector3.Normalize(this.moveDir);
                    this.moveDir = Vector3.left;
                }
                StartCoroutine(Timer(100));
            }

            Input.GetKeyDown(GameConfig.InteractiveKey);
            Input.GetMouseButtonDown(0);
            Input.GetMouseButtonDown(1);
        }

        IEnumerator Timer(int time)
        {
            while (time >= 0)
            {
                time -= 1;
                yield return null;
            }
            this.c_speed = m_Character.Speed;
            this.isDodge = false;
        }

        // sim init a character
        private void TestInit()
        {
            this.m_Character = new Character();
            this.m_Character.Speed = 1;
            this.m_Character.DodgeSpeed = 5;
            this.m_Character.Name = "TestChara";
            this.m_Character.Description = "This is a Test Chara";

            this.c_speed = m_Character.Speed;
        }
    }
}

