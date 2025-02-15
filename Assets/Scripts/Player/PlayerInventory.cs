using UnityEngine;
using UnityEngine.Events;

namespace Game.Player
{
    /**
     * Represent the inventory of a player
     */
    [RequireComponent(typeof(PlayerController), typeof(PlayerStat))]
    public class PlayerInventory : MonoBehaviour
    {
        public UsableItem EquippedItem => _inventory.Item1;
        public UsableItem HeldItem => _inventory.Item2;
        public bool IsFull => _inventory.Item1 != null && _inventory.Item2 != null;

        /**
         * Invoked when an item is equipped,
         * either when pick up or switching between weapons
         */
        public event UnityAction<PlayerInventory> OnItemEquip = (_) => { };
        /**
         * Invoked when an item is held,
         * either when replaced on pick up or switching between weapons
         */
        public event UnityAction<PlayerInventory> OnItemHold = (_) => { };
        /**
         * Invoked when the items in the inventory is switched
         * Notice that it also trigger if an item is pick up when there's a spot
         * in the inventory and the player is holding an item
         */
        public event UnityAction<PlayerInventory> OnItemSwitch = (_) => { };
        /**
         * Invoked when picking up an item
         */
        public event UnityAction<PlayerInventory> OnItemPick = (_) => { };
        
        [SerializeField] private Transform _itemHolderTrans;
        private PlayerController _playerController;
        private PlayerStat _playerStat;
        private (UsableItem, UsableItem) _inventory;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            _playerStat = GetComponent<PlayerStat>();
            
            _playerController.OnItemUseDown += ItemUseDown;
            _playerController.OnItemUseUp += ItemUseUp;
            _playerController.OnSwitchItem += SwitchItem;
            _playerController.OnMovement += FlipItemHolder;
        }

        private void FlipItemHolder(Vector2 dir)
        {
            if (dir.x > 0)
                _itemHolderTrans.localScale = Vector3.left + Vector3.up;
            if (dir.x < 0)
                _itemHolderTrans.localScale = Vector3.right + Vector3.up;
        }

        private void SwitchItem()
        {
            bool equipped = false;
            bool held = false;
            if (EquippedItem != null)
            {
                EquippedItem.Hold(_playerStat.ID);
                EquippedItem.MakeInvisible();
                held = true;
            }

            if (HeldItem != null)
            {
                HeldItem.Equip(_playerStat.ID);
                HeldItem.MakeVisible();
                equipped = true;
            }
            
            _inventory = (_inventory.Item2, _inventory.Item1);
            OnItemSwitch.Invoke(this);
            if (equipped) OnItemEquip.Invoke(this);
            if (held) OnItemHold.Invoke(this);
        }

        private void ItemUseDown()
        {
            if (EquippedItem == null) return;
            EquippedItem.ItemUseDown(_playerStat.ID);
        }
        
        private void ItemUseUp()
        {
            if (EquippedItem == null) return;
            EquippedItem.ItemUseUp(_playerStat.ID);
        }

        private void ItemBreak(UsableItem item)
        {
            item.OnBreak -= ItemBreak;
            if (item == EquippedItem) _inventory.Item1 = null;
            if (item == HeldItem) _inventory.Item2 = null;
        }

        // Handle when picking up an item in three cases on the inventory
        // 1. (null, null):
        //      have no item at all, pick up and equip right away
        // 2. (null, item):
        //      currently not equipping any item, pick up and equip right away
        // 3. (item, null):
        //      try picking up item when equipping an item, switch the
        //      equipped item to inventory slot. And pick up and equip right away 
        // * Currently not auto picking up items when inventory is full
        private void PickUpItem(UsableItem item)
        {
            if (IsFull) return;
            item.PickUpBy(_itemHolderTrans);
            if (EquippedItem == null)
            {
                _inventory.Item1 = item;
                _inventory.Item1.Equip(_playerStat.ID);
                _inventory.Item1.OnBreak += ItemBreak;
                OnItemEquip.Invoke(this);
                return;
            }

            _inventory.Item2 = item;
            _inventory.Item2.OnBreak += ItemBreak;
            SwitchItem();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            UsableItem item = col.gameObject.GetComponent<UsableItem>();
            if (item == null) return;
            PickUpItem(item);
            OnItemPick.Invoke(this);
        }
    }
}