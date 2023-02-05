﻿using System;
using Game.Player;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Projectile))]
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private Projectile _projectile;
        [SerializeField] private Pickable _pickable;
        [SerializeField] private float _damage;
        [SerializeField] private Transform _visualTransform;
        
        private void Awake()
        {
            _projectile = GetComponent<Projectile>();
            _pickable = GetComponent<Pickable>();
            _projectile.OnHit += HandleHit;
        }

        private void Update()
        {
            // rotate arrow based on velocity
            Vector2 v = _pickable.Rigidbody.velocity;
            _visualTransform.rotation = Quaternion.AngleAxis(
                Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg, 
                Vector3.forward);
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