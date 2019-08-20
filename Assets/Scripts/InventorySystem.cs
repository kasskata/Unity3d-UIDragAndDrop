namespace Assets.Scripts
{
    using System;
    using Core;
    using UnityEngine;
    using UnityEngine.UI;
    using Random = UnityEngine.Random;

    public class InventorySystem : MonoBehaviour
    {
        [Tooltip("DragAndDrop reference on the scene. You can have only one instance or you will have logic problems.")]
        public DragAndDrop dragAndDrop;

        [Tooltip("Chest slots reference where instantiate more slots with items inside.")]
        public Transform chestSlots;

        [Tooltip("Inventory slots reference.")]
        public Transform InventorySlots;

        [Tooltip("Item prefab reference from project folder.")]
        public DragElement itemPrefab;

        [Tooltip("Slot prefab reference from project folder.")]
        public DropObject slotPrefab;

        [Tooltip("Particles system triggered from dragging event.")]
        public ParticleSystem particles;

        [Tooltip("How many items you want to have in chest")]
        public int itemsCount;

        [Tooltip("How many slots want to have in inventory.")]
        public int slotsCount;

        [Tooltip("Sprite atlas source for item images.")]
        public AtlasLoader spriteAtlas;

        public void Start()
        {
            int i;
            // Instantiate inventory slots.
            for (i = 0; i < this.slotsCount; i++)
            {
                this.dragAndDrop.AddDropObject(this.InventorySlots, this.slotPrefab, $"Slot_{i}");
            }

            // Instantiate chest's item slots.
            for (i = 0; i < this.itemsCount; i++)
            {
                this.dragAndDrop.AddDropObject(this.chestSlots, this.slotPrefab, $"Slot_{i}");
            }

            // Instantiate chest's items randomly.
            for (i = 0; i < this.chestSlots.childCount; i++)
            {
                int randomInt = Random.Range(0, 10);
                if (randomInt >= 5)
                {
                   continue;
                }

                Transform slot = this.chestSlots.GetChild(i);
                Guid key = slot.GetComponent<DropObject>().Id;

                DragElement item;
                if (this.dragAndDrop.DropObjectsCache[key].isEmpty)
                {
                    item = this.dragAndDrop.AddDragElement(slot, this.itemPrefab, $"Item_{i}");
                    item.TransformCache.localPosition = Vector2.zero;
                }
                else
                {
                    item = this.dragAndDrop.DropObjectsCache[key].GetComponentInChildren<DragElement>();
                }

                Image image = item.GetComponent<Image>();
                image.sprite = GetRandomSprite();

            }
        }

        // Attached to DragAndDrop End Drag event. When new place is invalid return to last place.
        public void OnInvalidPlaceReturn()
        {
            if (this.dragAndDrop.HoveredDropObject == null)
                return;

            if (!this.dragAndDrop.HoveredDropObject.isEmpty)
            {
                BetweenGlobalPosition tweenPosition = this.dragAndDrop.SelectedDragElement.GetComponent<BetweenGlobalPosition>();
                tweenPosition.To = this.dragAndDrop.SelectedDragElement.TransformCache.position;
                tweenPosition.From = this.dragAndDrop.SelectedDragElement.LastPosition;
                tweenPosition.ResetToEnd();
                tweenPosition.OnFinish.AddListener(() =>
                {
                    tweenPosition.OnFinish.RemoveAllListeners();
                });

                tweenPosition.PlayReverse();
            }
        }

        // Attached to DragAndDrop Begin Drag event. Trigger all Between animations forward of empty slots.
        public void AnimateOnlyEmptySlots()
        {
            foreach (DropObject slot in this.dragAndDrop.DropObjectsCache.Values)
            {
                slot.GetComponentInChildren<BetweenColor>().Play(slot.isEmpty);
            }
        }

        // Attached to DragAndDrop End/Drop Drag event. Trigger all Between animations reverse of empty slots.
        public void ReverseAnimationOnlyEmptySlots()
        {
            foreach (DropObject slot in this.dragAndDrop.DropObjectsCache.Values)
            {
                if (slot.isEmpty)
                {
                    slot.GetComponentInChildren<BetweenColor>().PlayReverse();
                }
            }
        }

        // Attached to DragAndDrop Drop Outside event. Destroy selected item.
        public void DestroySelectedDragElement()
        {
            Destroy(this.dragAndDrop.SelectedDragElement.gameObject);
        }

        // Attached to DragAndDrop all events. Trigger particle system to emit or stop emitting and change parents.
        public void SetParticles(bool isActive)
        {
            if (isActive)
            {
                this.particles.transform.SetParent(this.dragAndDrop.SelectedDragElement.TransformCache);
                this.particles.transform.localPosition = Vector2.zero;
                if (!this.particles.isPlaying)
                {
                    this.particles.Play();
                }
            }
            else
            {
                this.particles.transform.SetParent(this.transform);
                this.particles.transform.localPosition = Vector2.zero;

                if (this.particles.isPlaying)
                {
                    this.particles.Stop();
                }
            }
        }

        private Sprite GetRandomSprite()
        {
            int index = Random.Range(0, this.spriteAtlas.Sprites.Length);
            return this.spriteAtlas.Get(index);
        }
    }
}
