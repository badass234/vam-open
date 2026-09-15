namespace DynamicCSharp
{
	public interface IMemberProxy
	{
		object this[string name] { get; set; }
	}
}
