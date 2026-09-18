using System;
using System.Reflection;
using System.Text;

namespace Mono.CSharp
{
	public abstract class AParametersCollection
	{
		protected bool has_arglist;

		protected bool has_params;

		protected IParameterData[] parameters;

		protected TypeSpec[] types;

		public CallingConventions CallingConvention
		{
			get
			{
				if (!has_arglist)
				{
					return CallingConventions.Standard;
				}
				return CallingConventions.VarArgs;
			}
		}

		public int Count => parameters.Length;

		public TypeSpec ExtensionMethodType
		{
			get
			{
				if (Count == 0)
				{
					return null;
				}
				if (!FixedParameters[0].HasExtensionMethodModifier)
				{
					return null;
				}
				return types[0];
			}
		}

		public IParameterData[] FixedParameters => parameters;

		public bool HasArglist => has_arglist;

		public bool HasExtensionMethodType
		{
			get
			{
				if (Count == 0)
				{
					return false;
				}
				return FixedParameters[0].HasExtensionMethodModifier;
			}
		}

		public bool HasParams => has_params;

		public bool IsEmpty => parameters.Length == 0;

		public TypeSpec[] Types
		{
			get
			{
				return types;
			}
			set
			{
				types = value;
			}
		}

		public static ParameterAttributes GetParameterAttribute(Parameter.Modifier modFlags)
		{
			if ((modFlags & Parameter.Modifier.OUT) == 0)
			{
				return ParameterAttributes.None;
			}
			return ParameterAttributes.Out;
		}

		public Type[] GetMetaInfo()
		{
			Type[] array;
			if (has_arglist)
			{
				if (Count == 1)
				{
					return Type.EmptyTypes;
				}
				array = new Type[Count - 1];
			}
			else
			{
				if (Count == 0)
				{
					return Type.EmptyTypes;
				}
				array = new Type[Count];
			}
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Types[i].GetMetaInfo();
				if ((FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask) != Parameter.Modifier.NONE)
				{
					array[i] = array[i].MakeByRefType();
				}
			}
			return array;
		}

		public int GetParameterIndexByName(string name)
		{
			for (int i = 0; i < Count; i++)
			{
				if (parameters[i].Name == name)
				{
					return i;
				}
			}
			return -1;
		}

		public string GetSignatureForDocumentation()
		{
			if (IsEmpty)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder("(");
			for (int i = 0; i < Count; i++)
			{
				if (i != 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(types[i].GetSignatureForDocumentation());
				if ((parameters[i].ModFlags & Parameter.Modifier.RefOutMask) != Parameter.Modifier.NONE)
				{
					stringBuilder.Append("@");
				}
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		public string GetSignatureForError()
		{
			return GetSignatureForError("(", ")", Count);
		}

		public string GetSignatureForError(string start, string end, int count)
		{
			StringBuilder stringBuilder = new StringBuilder(start);
			for (int i = 0; i < count; i++)
			{
				if (i != 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(ParameterDesc(i));
			}
			stringBuilder.Append(end);
			return stringBuilder.ToString();
		}

		public static bool HasSameParameterDefaults(AParametersCollection a, AParametersCollection b)
		{
			if (a == null)
			{
				return b == null;
			}
			for (int i = 0; i < a.Count; i++)
			{
				if (a.FixedParameters[i].HasDefaultValue != b.FixedParameters[i].HasDefaultValue)
				{
					return false;
				}
			}
			return true;
		}

		public AParametersCollection Inflate(TypeParameterInflator inflator)
		{
			TypeSpec[] array = null;
			bool flag = false;
			for (int i = 0; i < Count; i++)
			{
				TypeSpec typeSpec = inflator.Inflate(types[i]);
				if (array == null)
				{
					if (typeSpec == types[i])
					{
						continue;
					}
					flag |= FixedParameters[i].HasDefaultValue;
					array = new TypeSpec[types.Length];
					Array.Copy(types, array, types.Length);
				}
				else
				{
					if (typeSpec == types[i])
					{
						continue;
					}
					flag |= FixedParameters[i].HasDefaultValue;
				}
				array[i] = typeSpec;
			}
			if (array == null)
			{
				return this;
			}
			AParametersCollection aParametersCollection = (AParametersCollection)MemberwiseClone();
			aParametersCollection.types = array;
			if (flag)
			{
				aParametersCollection.parameters = new IParameterData[Count];
				for (int j = 0; j < Count; j++)
				{
					IParameterData parameterData = FixedParameters[j];
					aParametersCollection.FixedParameters[j] = parameterData;
					if (!parameterData.HasDefaultValue)
					{
						continue;
					}
					Expression expression = parameterData.DefaultValue;
					if (array[j] != expression.Type)
					{
						Constant constant = expression as Constant;
						if (constant != null)
						{
							constant = Constant.ExtractConstantFromValue(array[j], constant.GetValue(), expression.Location);
							expression = ((constant != null) ? ((Expression)constant) : ((Expression)new DefaultValueExpression(new TypeExpression(array[j], expression.Location), expression.Location)));
						}
						else if (expression is DefaultValueExpression)
						{
							expression = new DefaultValueExpression(new TypeExpression(array[j], expression.Location), expression.Location);
						}
						aParametersCollection.FixedParameters[j] = new ParameterData(parameterData.Name, parameterData.ModFlags, expression);
					}
				}
			}
			return aParametersCollection;
		}

		public string ParameterDesc(int pos)
		{
			if (types == null || types[pos] == null)
			{
				return ((Parameter)FixedParameters[pos]).GetSignatureForError();
			}
			string signatureForError = types[pos].GetSignatureForError();
			if (FixedParameters[pos].HasExtensionMethodModifier)
			{
				return "this " + signatureForError;
			}
			Parameter.Modifier modifier = FixedParameters[pos].ModFlags & Parameter.Modifier.ModifierMask;
			if (modifier == Parameter.Modifier.NONE)
			{
				return signatureForError;
			}
			return Parameter.GetModifierSignature(modifier) + " " + signatureForError;
		}
	}
}
