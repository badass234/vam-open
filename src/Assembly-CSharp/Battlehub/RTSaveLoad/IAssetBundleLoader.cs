namespace Battlehub.RTSaveLoad
{
	public interface IAssetBundleLoader
	{
		void Load(string name, AssetBundleEventHandler callback);
	}
}
