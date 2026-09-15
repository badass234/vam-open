namespace DynamicCSharp
{
	public static class RuntimeRestrictions
	{
		public static void AddRuntimeNamespaceRestriction(string namespaceName)
		{
			DynamicCSharp.Settings.AddRuntimeNamespaceRestriction(namespaceName);
		}

		public static void RemoveRuntimeNamespaceRestriction(string namespaceName)
		{
			DynamicCSharp.Settings.RemoveRuntimeNamespaceRestriction(namespaceName);
		}
	}
}
