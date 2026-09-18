namespace Mono.CSharp
{
	public class ComposedTypeSpecifier
	{
		public static readonly ComposedTypeSpecifier SingleDimension = new ComposedTypeSpecifier(1, Location.Null);

		public readonly int Dimension;

		public readonly Location Location;

		public bool IsNullable => Dimension == -1;

		public bool IsPointer => Dimension == -2;

		public ComposedTypeSpecifier Next { get; set; }

		public ComposedTypeSpecifier(int specifier, Location loc)
		{
			Dimension = specifier;
			Location = loc;
		}

		public static ComposedTypeSpecifier CreateArrayDimension(int dimension, Location loc)
		{
			return new ComposedTypeSpecifier(dimension, loc);
		}

		public static ComposedTypeSpecifier CreateNullable(Location loc)
		{
			return new ComposedTypeSpecifier(-1, loc);
		}

		public static ComposedTypeSpecifier CreatePointer(Location loc)
		{
			return new ComposedTypeSpecifier(-2, loc);
		}

		public string GetSignatureForError()
		{
			string text = (IsPointer ? "*" : (IsNullable ? "?" : ArrayContainer.GetPostfixSignature(Dimension)));
			if (Next == null)
			{
				return text;
			}
			return text + Next.GetSignatureForError();
		}
	}
}
