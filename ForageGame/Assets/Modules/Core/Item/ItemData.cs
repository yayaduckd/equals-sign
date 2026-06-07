using UnityEngine;

namespace TDK.ItemSystem
{
    public class ItemData : ScriptableObject
    {
        [SerializeField] protected string itemName;
        [SerializeField] protected string description;
        [SerializeField] protected Sprite sprite;

        public string GetId() => ItemServices.Instance.Database.GetId(this);
        public string GetName() => itemName;
        public string GetDescription() => description;
        public Sprite GetSprite() => sprite;

        // World item function; return true if executed successfully.
        public virtual bool TryWorldItemInteract() => throw new System.NotImplementedException();

        // Item function; return true if executed successfully.
        public virtual bool TryUse() => throw new System.NotImplementedException();
    }
}