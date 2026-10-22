using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shortRPG.config
{
    public static class GameConfig
    {
        public const KeyCode UpKey = KeyCode.W;
        public const KeyCode DownKey = KeyCode.S;
        public const KeyCode RightKey = KeyCode.D;
        public const KeyCode LeftKey = KeyCode.A;

        public const KeyCode InteractiveKey = KeyCode.E;
        public const KeyCode DodgeKey = KeyCode.Space;

        public const int SpeedModifier = 5;
        public const int DodgeDist = 2;
    }
}

