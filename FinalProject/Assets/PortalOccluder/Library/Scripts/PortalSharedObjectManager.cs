using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CirrusPlay.PortalLibrary
{
    public class PortalSharedItemManager<T> where T : Component
    {
        private readonly IDictionary<T, PortalSharedItem<T>> items = new Dictionary<T, PortalSharedItem<T>>();

        public PortalSharedItem<T> Add(T item, Room room)
        {
            var result = FindOrCreate(item);
            result.Add(room);
            return result;
        }

        public PortalSharedItem<T> Add(T item, Portal portal)
        {
            var result = FindOrCreate(item);
            result.Add(portal);
            return result;
        }

        public IEnumerable<PortalSharedItem<T>> Add(IEnumerable<T> items, Room room)
        {
            var list = new List<PortalSharedItem<T>>();
            foreach (var item in items)
            {
                list.Add(Add(item, room));
            }
            return list.AsReadOnly();
        }

        public IEnumerable<PortalSharedItem<T>> Add(IEnumerable<T> items, Portal portal)
        {
            var list = new List<PortalSharedItem<T>>();
            foreach (var item in items)
            {
                list.Add(Add(item, portal));
            }
            return list.AsReadOnly();
        }

        public PortalSharedItem<T> Remove(T item, Room room)
        {
            PortalSharedItem<T> result = null;
            if (room != null && items.TryGetValue(item, out result))
            {
                result.Remove(room);
                CleanupItem(result);
            }
            return result;
        }

        public PortalSharedItem<T> Remove(T item, Portal portal)
        {
            PortalSharedItem<T> result = null;
            if (portal != null && items.TryGetValue(item, out result))
            {
                result.Remove(portal);
                CleanupItem(result);
            }
            return result;
        }

        public PortalSharedItem<T> FindOrCreate(T item)
        {
            PortalSharedItem<T> result;
            if (items.TryGetValue(item, out result))
                return result;
            else
                return items[item] = new PortalSharedItem<T>(item);
        }

        public PortalSharedItem<T> Find(T item)
        {
            PortalSharedItem<T> result;
            if (items.TryGetValue(item, out result))
                return result;
            else
                return null;
        }

        private void CleanupItem(PortalSharedItem<T> item)
        {
            if (item.References() <= 0)
                items.Remove(item.item);
        }
    }

    internal struct PortalSharedItemModifier
    {
        private readonly System.Func<bool> isVisible;
        public bool IsVisible() { return isVisible(); }

        public PortalSharedItemModifier(System.Func<bool> isVisible)
        {
            this.isVisible = isVisible;
        }
    }

    public class PortalSharedItem<T> where T : Component
    {
        public delegate bool VisibilityCallback();
        public readonly T item;
        private readonly Dictionary<object, PortalSharedItemModifier> containers = new Dictionary<object, PortalSharedItemModifier>();

        public PortalSharedItem(T item)
        {
            this.item = item;
        }

        public void Add(Room room)
        {
            containers[room] = new PortalSharedItemModifier(() => room.IsVisible());
        }

        public void Add(Portal portal)
        {
            containers[portal] = new PortalSharedItemModifier(() => portal.IsVisible());
        }

        public void Remove(Room room)
        {
            containers.Remove(room);
        }

        public void Remove(Portal portal)
        {
            containers.Remove(portal);
        }

        public int References()
        {
            return containers.Count;
        }

        public bool IsVisible()
        {
            foreach (var container in containers)
                if (container.Value.IsVisible())
                    return true;
            return false;
        }

    }
}