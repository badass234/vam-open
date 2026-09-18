using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Mono.CSharp
{
	public sealed class ReflectionImporter : MetadataImporter
	{
		public ReflectionImporter(ModuleContainer module, BuiltinTypes builtin)
			: base(module)
		{
			Initialize(builtin);
		}

		public override void AddCompiledType(TypeBuilder builder, TypeSpec spec)
		{
		}

		protected override MemberKind DetermineKindFromBaseType(Type baseType)
		{
			if (baseType == typeof(ValueType))
			{
				return MemberKind.Struct;
			}
			if (baseType == typeof(System.Enum))
			{
				return MemberKind.Enum;
			}
			if (baseType == typeof(MulticastDelegate))
			{
				return MemberKind.Delegate;
			}
			return MemberKind.Class;
		}

		protected override bool HasVolatileModifier(Type[] modifiers)
		{
			for (int i = 0; i < modifiers.Length; i++)
			{
				if (modifiers[i] == typeof(IsVolatile))
				{
					return true;
				}
			}
			return false;
		}

		public void ImportAssembly(Assembly assembly, RootNamespace targetNamespace)
		{
			GetAssemblyDefinition(assembly);
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				types = ex.Types;
			}
			ImportTypes(types, targetNamespace, true);
		}

		public ImportedModuleDefinition ImportModule(Module module, RootNamespace targetNamespace)
		{
			ImportedModuleDefinition importedModuleDefinition = new ImportedModuleDefinition(module);
			importedModuleDefinition.ReadAttributes();
			Type[] types;
			try
			{
				types = module.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				types = ex.Types;
			}
			ImportTypes(types, targetNamespace, false);
			return importedModuleDefinition;
		}

		private void Initialize(BuiltinTypes builtin)
		{
			compiled_types.Add(typeof(object), builtin.Object);
			compiled_types.Add(typeof(ValueType), builtin.ValueType);
			compiled_types.Add(typeof(System.Attribute), builtin.Attribute);
			compiled_types.Add(typeof(int), builtin.Int);
			compiled_types.Add(typeof(long), builtin.Long);
			compiled_types.Add(typeof(uint), builtin.UInt);
			compiled_types.Add(typeof(ulong), builtin.ULong);
			compiled_types.Add(typeof(byte), builtin.Byte);
			compiled_types.Add(typeof(sbyte), builtin.SByte);
			compiled_types.Add(typeof(short), builtin.Short);
			compiled_types.Add(typeof(ushort), builtin.UShort);
			compiled_types.Add(typeof(IEnumerator), builtin.IEnumerator);
			compiled_types.Add(typeof(IEnumerable), builtin.IEnumerable);
			compiled_types.Add(typeof(IDisposable), builtin.IDisposable);
			compiled_types.Add(typeof(char), builtin.Char);
			compiled_types.Add(typeof(string), builtin.String);
			compiled_types.Add(typeof(float), builtin.Float);
			compiled_types.Add(typeof(double), builtin.Double);
			compiled_types.Add(typeof(decimal), builtin.Decimal);
			compiled_types.Add(typeof(bool), builtin.Bool);
			compiled_types.Add(typeof(IntPtr), builtin.IntPtr);
			compiled_types.Add(typeof(UIntPtr), builtin.UIntPtr);
			compiled_types.Add(typeof(MulticastDelegate), builtin.MulticastDelegate);
			compiled_types.Add(typeof(System.Delegate), builtin.Delegate);
			compiled_types.Add(typeof(System.Enum), builtin.Enum);
			compiled_types.Add(typeof(Array), builtin.Array);
			compiled_types.Add(typeof(void), builtin.Void);
			compiled_types.Add(typeof(Type), builtin.Type);
			compiled_types.Add(typeof(Exception), builtin.Exception);
			compiled_types.Add(typeof(RuntimeFieldHandle), builtin.RuntimeFieldHandle);
			compiled_types.Add(typeof(RuntimeTypeHandle), builtin.RuntimeTypeHandle);
		}
	}
}
