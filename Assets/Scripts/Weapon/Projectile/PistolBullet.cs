﻿using Game.Player;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Projectile))]
    public class PistolBullet : MonoBehaviour
    {
        [SerializeField] private Projectile _projectile;
        [SerializeField] private float _damage;
        
        private void Awake()
        {
            _projectile = GetComponent<Projectile>();
            _projectile.OnHit += HandleHit;
        }

        private void HandleHit(PlayerController player)
        {
            Debug.Log($"deal {_damage} to player");
            ReturnToPool();
        }
        
        private void ReturnToPool()
        {
            _projectile.ReturnToPool();
        }
        
        // check for wall / floor hits
        private void OnCollisionEnter2D(Collision2D col)
        {
            PlayerController player = col.gameObject.GetComponent<PlayerController>();
            if (player != null) return;
            
            // hits a wall / floor
            ReturnToPool();
        }
    }
}