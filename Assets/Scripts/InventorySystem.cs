namespace Assets.Scripts
{
    using Core;
    using UnityEngine;
    using UnityEngine.UI;

    public class InventorySystem : MonoBehaviour
    {
        public DragAndDrop dragAndDrop;
        public Transform itemContainer;
        public Transform slotContainer;

        public DragElement itemPrefab;
        public DropObject slotPrefab;
        public ParticleSystem particles;

        public int itemsCount;
        public int slotsCount;
        public AtlasLoader spriteAtlas;


        public void Awake()
        {
            for (int i = 0; i < this.slotsCount; i++)
            {
                //this.dragAndDrop.AddDropObject(this.itemContainer, $"Slot_{i}");
                DropObject go = Instantiate(this.slotPrefab, this.slotContainer);
                go.name = $"Slot_{i}";
            }

            for (int i = 0; i < this.itemsCount; i++)
            {
                //this.dragAndDrop.AddDropObject(this.itemContainer, $"Slot_{i}");
                DropObject slot = Instantiate(this.slotPrefab, this.itemContainer);
                slot.name = $"Slot_{i}";
            }

            for (int i = 0; i < this.itemsCount; i++)
            {
                DragElement item = Instantiate(this.itemPrefab, this.itemContainer.GetChild(i));
                item.transformCache.localPosition = Vector2.zero;
                item.name = $"Item_{i}";
                Image image = item.GetComponent<Image>();
                image.sprite = GetRandomSprite();
            }
        }

        public void OnInvalidPlaceReturn()
        {
            if (this.dragAndDrop.HoveredDropObject == null)
                return;

            if (!this.dragAndDrop.HoveredDropObject.IsEmpty)
            {
                BetweenGlobalPosition tweenPosition = this.dragAndDrop.SelectedDragElement.GetComponent<BetweenGlobalPosition>();
                tweenPosition.To = this.dragAndDrop.SelectedDragElement.transformCache.position;
                tweenPosition.From = this.dragAndDrop.SelectedDragElement.LastPosition;
                tweenPosition.ResetToEnd();
                tweenPosition.OnFinish.AddListener(() =>
                {
                    tweenPosition.OnFinish.RemoveAllListeners();
                });

                tweenPosition.PlayReverse();
            }
        }

        public void AnimateOnlyEmptySlots()
        {
            foreach (DropObject slot in this.dragAndDrop.DropObjects.Values)
            {
                slot.GetComponentInChildren<BetweenColor>().Play(slot.IsEmpty);
            }
        }

        public void StopAnimationOnlyEmptySlots()
        {
            foreach (DropObject slot in this.dragAndDrop.DropObjects.Values)
            {
                if (slot.IsEmpty)
                {
                    slot.GetComponentInChildren<BetweenColor>().PlayReverse();
                }
            }
        }

        public void DestroySelectedDragElement()
        {
            Destroy(this.dragAndDrop.SelectedDragElement.gameObject);
        }

        public void SetParticles(bool isActive)
        {
            if (isActive)
            {
                this.particles.transform.SetParent(this.dragAndDrop.SelectedDragElement.transformCache);
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
            int i = Random.Range(0, this.spriteAtlas.Sprites.Length);
            return this.spriteAtlas.Get(i);
        }
    }
}
