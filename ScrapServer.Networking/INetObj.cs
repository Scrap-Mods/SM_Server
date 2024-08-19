using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

public interface INetObj
{
    public abstract NetObjType NetObjType { get; }
    public abstract ControllerType ControllerType { get; }
    public uint Id { get; }

    public abstract void SerializeCreate(ref BitWriter writer);

    public void SerializeP(ref BitWriter writer)
    {
        if (this.NetObjType != NetObjType.Joint)
        {
            throw new NotSupportedException("Only Joints can be serialized as P updates");
        }
        throw new NotImplementedException();
    }

    public abstract void SerializeUpdate(ref BitWriter writer);

    public void SerializeRemove(ref BitWriter writer)
    {
        // Empty for most if not all NetObjs
    }
}
