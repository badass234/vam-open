using System;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Metadata
{
	internal abstract class Table
	{
		protected sealed class RowSizeCalc
		{
			private readonly MetadataWriter mw;

			private int size;

			internal int Value => size;

			internal RowSizeCalc(MetadataWriter mw)
			{
				this.mw = mw;
			}

			internal RowSizeCalc AddFixed(int size)
			{
				this.size += size;
				return this;
			}

			internal RowSizeCalc WriteStringIndex()
			{
				if (mw.bigStrings)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteGuidIndex()
			{
				if (mw.bigGuids)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteBlobIndex()
			{
				if (mw.bigBlobs)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteTypeDefOrRef()
			{
				if (mw.bigTypeDefOrRef)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteField()
			{
				if (mw.bigField)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteMethodDef()
			{
				if (mw.bigMethodDef)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteParam()
			{
				if (mw.bigParam)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteResolutionScope()
			{
				if (mw.bigResolutionScope)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteMemberRefParent()
			{
				if (mw.bigMemberRefParent)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteHasCustomAttribute()
			{
				if (mw.bigHasCustomAttribute)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteCustomAttributeType()
			{
				if (mw.bigCustomAttributeType)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteHasConstant()
			{
				if (mw.bigHasConstant)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteTypeDef()
			{
				if (mw.bigTypeDef)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteMethodDefOrRef()
			{
				if (mw.bigMethodDefOrRef)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteEvent()
			{
				if (mw.bigEvent)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteProperty()
			{
				if (mw.bigProperty)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteHasSemantics()
			{
				if (mw.bigHasSemantics)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteImplementation()
			{
				if (mw.bigImplementation)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteTypeOrMethodDef()
			{
				if (mw.bigTypeOrMethodDef)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteGenericParam()
			{
				if (mw.bigGenericParam)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteHasDeclSecurity()
			{
				if (mw.bigHasDeclSecurity)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteMemberForwarded()
			{
				if (mw.bigMemberForwarded)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteModuleRef()
			{
				if (mw.bigModuleRef)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteHasFieldMarshal()
			{
				if (mw.bigHasFieldMarshal)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}
		}

		internal bool Sorted;

		internal bool IsBig => RowCount > 65535;

		internal abstract int RowCount { get; set; }

		internal abstract void Write(MetadataWriter mw);

		internal abstract void Read(MetadataReader mr);

		internal int GetLength(MetadataWriter md)
		{
			return RowCount * GetRowSize(new RowSizeCalc(md));
		}

		protected abstract int GetRowSize(RowSizeCalc rsc);
	}
	internal abstract class Table<T> : Table
	{
		internal T[] records = Empty<T>.Array;

		protected int rowCount;

		internal sealed override int RowCount
		{
			get
			{
				return rowCount;
			}
			set
			{
				rowCount = value;
				records = new T[value];
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			throw new InvalidOperationException();
		}

		internal int AddRecord(T newRecord)
		{
			if (rowCount == records.Length)
			{
				Array.Resize(ref records, Math.Max(16, records.Length * 2));
			}
			records[rowCount++] = newRecord;
			return rowCount;
		}

		internal int AddVirtualRecord()
		{
			return ++rowCount;
		}

		internal override void Write(MetadataWriter mw)
		{
			throw new InvalidOperationException();
		}
	}
}
