using System.IO;

namespace Subterannia.Core.Mechanics.Interfaces
{
    public interface ISerializable
    {
        void Serialize(Stream stream);
    }
}
