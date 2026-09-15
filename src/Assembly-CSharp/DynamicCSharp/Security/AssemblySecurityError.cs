namespace DynamicCSharp.Security
{
	public struct AssemblySecurityError
	{
		public string assemblyName;

		public string moduleName;

		public string securityMessage;

		public string securityType;

		public override string ToString()
		{
			return $"Security Check Failed ({securityType}) : [{assemblyName}, {moduleName}] : {securityMessage}";
		}
	}
}
