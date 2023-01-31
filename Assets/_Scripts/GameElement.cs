using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class GameElement : MonoBehaviour
    {
        public GameApplication app => GameObject.FindObjectOfType<GameApplication>();
    }
}