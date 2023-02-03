using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreGame
{
    public class GameElement : MonoBehaviour
    {
        public GameApplication App => GameObject.FindObjectOfType<GameApplication>();
    }
}