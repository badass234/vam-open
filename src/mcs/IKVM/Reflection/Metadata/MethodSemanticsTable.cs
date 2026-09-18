using System;
using System.Collections.Generic;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal sealed class MethodSemanticsTable : SortedTable<MethodSemanticsTable.Record>
	{
		internal struct Record : IRecord
		{
			internal short Semantics;

			internal int Method;

			internal int Association;

			int IRecord.SortKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EMethodSemanticsTable_002ERecord_003E_002EIRecord_002Eget_SortKey();

			int IRecord.FilterKey => IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EMethodSemanticsTable_002ERecord_003E_002EIRecord_002Eget_FilterKey();

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EMethodSemanticsTable_002ERecord_003E_002EIRecord_002Eget_SortKey()
			{
				return Association;
			}

			private int IKVM_002EReflection_002EMetadata_002ESortedTable_003CIKVM_002EReflection_002EMetadata_002EMethodSemanticsTable_002ERecord_003E_002EIRecord_002Eget_FilterKey()
			{
				return Association;
			}
		}

		internal const int Index = 24;

		internal const short Setter = 1;

		internal const short Getter = 2;

		internal const short Other = 4;

		internal const short AddOn = 8;

		internal const short RemoveOn = 16;

		internal const short Fire = 32;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Semantics = mr.ReadInt16();
				records[i].Method = mr.ReadMethodDef();
				records[i].Association = mr.ReadHasSemantics();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Semantics);
				mw.WriteMethodDef(records[i].Method);
				mw.WriteHasSemantics(records[i].Association);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc.AddFixed(2).WriteMethodDef().WriteHasSemantics()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				moduleBuilder.FixupPseudoToken(ref records[i].Method);
				int association = records[i].Association;
				switch (association >> 24)
				{
				case 20:
					association = ((association & 0xFFFFFF) << 1) | 0;
					break;
				case 23:
					association = ((association & 0xFFFFFF) << 1) | 1;
					break;
				default:
					throw new InvalidOperationException();
				}
				records[i].Association = association;
			}
			Sort();
		}

		internal MethodInfo GetMethod(Module module, int token, bool nonPublic, short semantics)
		{
			foreach (int item in Filter(token))
			{
				if ((records[item].Semantics & semantics) != 0)
				{
					MethodBase methodBase = module.ResolveMethod((6 << 24) + records[item].Method);
					if (nonPublic || methodBase.IsPublic)
					{
						return (MethodInfo)methodBase;
					}
				}
			}
			return null;
		}

		internal MethodInfo[] GetMethods(Module module, int token, bool nonPublic, short semantics)
		{
			List<MethodInfo> list = new List<MethodInfo>();
			foreach (int item in Filter(token))
			{
				if ((records[item].Semantics & semantics) != 0)
				{
					MethodInfo methodInfo = (MethodInfo)module.ResolveMethod((6 << 24) + records[item].Method);
					if (nonPublic || methodInfo.IsPublic)
					{
						list.Add(methodInfo);
					}
				}
			}
			return list.ToArray();
		}

		internal void ComputeFlags(Module module, int token, out bool isPublic, out bool isNonPrivate, out bool isStatic)
		{
			isPublic = false;
			isNonPrivate = false;
			isStatic = false;
			foreach (int item in Filter(token))
			{
				MethodBase methodBase = module.ResolveMethod((6 << 24) + records[item].Method);
				isPublic |= methodBase.IsPublic;
				isNonPrivate |= (methodBase.Attributes & MethodAttributes.MemberAccessMask) > MethodAttributes.Private;
				isStatic |= methodBase.IsStatic;
			}
		}
	}
}
