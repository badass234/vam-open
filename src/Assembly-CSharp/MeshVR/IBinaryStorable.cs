using System.IO;

namespace MeshVR
{
	public interface IBinaryStorable
	{
		bool LoadFromBinaryFile(string path);

		bool LoadFromBinaryReader(BinaryReader reader);

		bool StoreToBinaryFile(string path);

		bool StoreToBinaryWriter(BinaryWriter writer);
	}
}
