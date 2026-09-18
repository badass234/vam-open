namespace Mono.CSharp
{
	public class LocatedToken
	{
		public int row;

		public int column;

		public string value;

		public SourceFile file;

		public Location Location => new Location(file, row, column);

		public string Value => value;

		public LocatedToken()
		{
		}

		public LocatedToken(string value, Location loc)
		{
			this.value = value;
			file = loc.SourceFile;
			row = loc.Row;
			column = loc.Column;
		}

		public override string ToString()
		{
			return $"Token '{Value}' at {row},{column}";
		}
	}
}
