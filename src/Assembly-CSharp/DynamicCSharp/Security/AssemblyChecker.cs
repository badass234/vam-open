using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace DynamicCSharp.Security
{
	internal sealed class AssemblyChecker
	{
		private AssemblySecurityError[] errors = new AssemblySecurityError[0];

		public AssemblySecurityError[] Errors => errors;

		public bool HasErrors => errors.Length > 0;

		public int ErrorCount => errors.Length;

		public void SecurityCheckAssembly(byte[] assemblyData)
		{
			ClearErrors();
			AssemblyDefinition assemblyDefinition = null;
			using (MemoryStream stream = new MemoryStream(assemblyData))
			{
				assemblyDefinition = AssemblyDefinition.ReadAssembly(stream);
			}
			foreach (ModuleDefinition module in assemblyDefinition.Modules)
			{
				SecurityCheckModule(assemblyDefinition.Name, module);
			}
		}

		private void SecurityCheckModule(AssemblyNameDefinition assemblyName, ModuleDefinition module)
		{
			foreach (ModuleReference moduleReference in module.ModuleReferences)
			{
				CreateError(assemblyName.Name, module.Name, "Unmanaged dll references not allowed: " + moduleReference.Name, "ModuleReference");
			}
			IEnumerable<Restriction> restrictions = DynamicCSharp.Settings.Restrictions;
			foreach (Restriction item in restrictions)
			{
				if (item.Mode == RestrictionMode.Exclusive)
				{
					if (!item.Verify(module))
					{
						CreateError(assemblyName.Name, module.Name, item.Message, item.GetType().Name);
					}
				}
				else
				{
					CreateError(assemblyName.Name, module.Name, "Inclusive security checking is not implemented", item.GetType().Name);
				}
			}
		}

		private void ClearErrors()
		{
			errors = new AssemblySecurityError[0];
		}

		private void CreateError(string assemblyName, string moduleName, string message, string type)
		{
			AssemblySecurityError assemblySecurityError = default(AssemblySecurityError);
			assemblySecurityError.assemblyName = assemblyName;
			assemblySecurityError.moduleName = moduleName;
			assemblySecurityError.securityMessage = message;
			assemblySecurityError.securityType = type;
			AssemblySecurityError assemblySecurityError2 = assemblySecurityError;
			Array.Resize(ref errors, errors.Length + 1);
			errors[errors.Length - 1] = assemblySecurityError2;
		}
	}
}
