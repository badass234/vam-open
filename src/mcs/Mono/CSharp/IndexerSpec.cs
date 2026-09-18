using System.Collections.Generic;
using System.Reflection;

namespace Mono.CSharp
{
	public class IndexerSpec : PropertySpec, IParametersMember, IInterfaceMemberSpec
	{
		private AParametersCollection parameters;

		public AParametersCollection Parameters => parameters;

		public IndexerSpec(TypeSpec declaringType, IMemberDefinition definition, TypeSpec memberType, AParametersCollection parameters, PropertyInfo info, Modifiers modifiers)
			: base(MemberKind.Indexer, declaringType, definition, memberType, info, modifiers)
		{
			this.parameters = parameters;
		}

		public override string GetSignatureForDocumentation()
		{
			return base.GetSignatureForDocumentation() + parameters.GetSignatureForDocumentation();
		}

		public override string GetSignatureForError()
		{
			return base.DeclaringType.GetSignatureForError() + ".this" + parameters.GetSignatureForError("[", "]", parameters.Count);
		}

		public override MemberSpec InflateMember(TypeParameterInflator inflator)
		{
			IndexerSpec obj = (IndexerSpec)base.InflateMember(inflator);
			obj.parameters = parameters.Inflate(inflator);
			return obj;
		}

		public override List<MissingTypeSpecReference> ResolveMissingDependencies(MemberSpec caller)
		{
			List<MissingTypeSpecReference> list = base.ResolveMissingDependencies(caller);
			TypeSpec[] types = parameters.Types;
			for (int i = 0; i < types.Length; i++)
			{
				List<MissingTypeSpecReference> missingDependencies = types[i].GetMissingDependencies(caller);
				if (missingDependencies != null)
				{
					if (list == null)
					{
						list = new List<MissingTypeSpecReference>();
					}
					list.AddRange(missingDependencies);
				}
			}
			return list;
		}
	}
}
