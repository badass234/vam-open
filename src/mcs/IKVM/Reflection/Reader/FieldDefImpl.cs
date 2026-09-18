using System;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Reader
{
	internal sealed class FieldDefImpl : FieldInfo
	{
		private readonly ModuleReader module;

		private readonly TypeDefImpl declaringType;

		private readonly int index;

		private FieldSignature lazyFieldSig;

		public override FieldAttributes Attributes => (FieldAttributes)module.Field.records[index].Flags;

		public override Type DeclaringType
		{
			get
			{
				if (!declaringType.IsModulePseudoType)
				{
					return declaringType;
				}
				return null;
			}
		}

		public override string Name => module.GetString(module.Field.records[index].Name);

		public override Module Module => module;

		public override int MetadataToken => (4 << 24) + index + 1;

		public override int __FieldRVA
		{
			get
			{
				SortedTable<FieldRVATable.Record>.Enumerator enumerator = module.FieldRVA.Filter(index + 1).GetEnumerator();
				if (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					return module.FieldRVA.records[current].RVA;
				}
				throw new InvalidOperationException();
			}
		}

		internal override FieldSignature FieldSignature => lazyFieldSig ?? (lazyFieldSig = FieldSignature.ReadSig(module, module.GetBlob(module.Field.records[index].Signature), declaringType));

		internal override bool IsBaked => true;

		internal FieldDefImpl(ModuleReader module, TypeDefImpl declaringType, int index)
		{
			this.module = module;
			this.declaringType = declaringType;
			this.index = index;
		}

		public override string ToString()
		{
			return base.FieldType.Name + " " + Name;
		}

		public override object GetRawConstantValue()
		{
			return module.Constant.GetRawConstantValue(module, MetadataToken);
		}

		public override void __GetDataFromRVA(byte[] data, int offset, int length)
		{
			int _FieldRVA = __FieldRVA;
			if (_FieldRVA == 0)
			{
				Array.Clear(data, offset, length);
			}
			else
			{
				module.__ReadDataFromRVA(_FieldRVA, data, offset, length);
			}
		}

		public override bool __TryGetFieldOffset(out int offset)
		{
			SortedTable<FieldLayoutTable.Record>.Enumerator enumerator = Module.FieldLayout.Filter(index + 1).GetEnumerator();
			if (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				offset = Module.FieldLayout.records[current].Offset;
				return true;
			}
			offset = 0;
			return false;
		}

		internal override int ImportTo(ModuleBuilder module)
		{
			return module.ImportMethodOrField(declaringType, Name, FieldSignature);
		}

		internal override int GetCurrentToken()
		{
			return MetadataToken;
		}
	}
}
