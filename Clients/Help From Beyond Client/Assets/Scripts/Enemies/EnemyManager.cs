using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class EnemyManager : NetworkBehaviour
    {
        public void OnEnemyDied(GhostEnemy enemy)
        {
        }
    }
}