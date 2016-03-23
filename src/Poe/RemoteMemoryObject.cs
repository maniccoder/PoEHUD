using PoeHUD.Framework;
using PoeHUD.Poe.RemoteMemoryObjects;

namespace PoeHUD.Poe
{
    public abstract class RemoteMemoryObject
    {
        public int Address { get; protected set; }
        protected TheGame Game { get; set; }
        protected Memory M { get; set; }

        protected Offsets Offsets => M.offsets;

        public T ReadObjectAt<T>(int offset) where T : RemoteMemoryObject, new() => ReadObject<T>(Address + offset);

        public T ReadObject<T>(int addressPointer) where T : RemoteMemoryObject, new() => new T { M = M, Address = M.ReadInt(addressPointer), Game = Game };

        public T GetObjectAt<T>(int offset) where T : RemoteMemoryObject, new() => GetObject<T>(Address + offset);

        public T GetObject<T>(int address) where T : RemoteMemoryObject, new() => new T { M = M, Address = address, Game = Game };

        public T AsObject<T>() where T : RemoteMemoryObject, new() => new T { M = M, Address = Address, Game = Game };

        public override bool Equals(object obj)
        {
            var remoteMemoryObject = obj as RemoteMemoryObject;
            return remoteMemoryObject != null && remoteMemoryObject.Address == Address;
        }

        public override int GetHashCode() => Address + GetType().Name.GetHashCode();
    }
}