namespace Mono.CSharp
{
	public class RootContext
	{
		private static ModuleContainer root;

		public static ModuleContainer ToplevelTypes
		{
			get
			{
				return root;
			}
			set
			{
				root = value;
			}
		}
	}
}
