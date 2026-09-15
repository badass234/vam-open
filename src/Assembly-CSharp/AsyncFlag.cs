public class AsyncFlag
{
	public string Name { get; set; }

	public bool Raised { get; protected set; }

	public AsyncFlag(string name)
	{
		Name = name;
	}

	public void Raise()
	{
		Raised = true;
	}

	public void Lower()
	{
		Raised = false;
	}
}
