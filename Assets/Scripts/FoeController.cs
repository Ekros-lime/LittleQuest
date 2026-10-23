using UnityEngine;

namespace shortRPG.Foe
{
    public class FoeController : MonoBehaviour
    {
        private float Speed;
        private int MaxHp;
        private int CurHp;
        private bool isLive = true;

        private void Awake()
        {
            Init();
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

        // sim init
        private void Init()
        {
            Speed = 0.6f;
            MaxHp = 5;
            CurHp = MaxHp;
        }
    }
}