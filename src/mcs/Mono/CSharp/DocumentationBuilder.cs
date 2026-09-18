using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Mono.CSharp
{
	public class DocumentationBuilder
	{
		private readonly XmlDocument XmlDocumentation;

		private readonly ModuleContainer module;

		private readonly ModuleContainer doc_module;

		private XmlWriter XmlCommentOutput;

		private static readonly string line_head = Environment.NewLine + "            ";

		private Dictionary<string, XmlDocument> StoredDocuments = new Dictionary<string, XmlDocument>();

		private ParserSession session;

		private Report Report => module.Compiler.Report;

		public MemberName ParsedName { get; set; }

		public List<DocumentationParameter> ParsedParameters { get; set; }

		public TypeExpression ParsedBuiltinType { get; set; }

		public Operator.OpType? ParsedOperator { get; set; }

		public DocumentationBuilder(ModuleContainer module)
		{
			doc_module = new ModuleContainer(module.Compiler);
			doc_module.DocumentationBuilder = this;
			this.module = module;
			XmlDocumentation = new XmlDocument();
			XmlDocumentation.PreserveWhitespace = false;
		}

		private XmlNode GetDocCommentNode(MemberCore mc, string name)
		{
			XmlDocument xmlDocumentation = XmlDocumentation;
			try
			{
				XmlElement xmlElement = xmlDocumentation.CreateElement("member");
				xmlElement.SetAttribute("name", name);
				string text = (xmlElement.InnerXml = mc.DocComment);
				string[] array = text.Split('\n');
				int count = 0;
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i].TrimEnd();
					if (text2.Length > 0)
					{
						array[count++] = text2;
					}
				}
				xmlElement.InnerXml = line_head + string.Join(line_head, array, 0, count);
				return xmlElement;
			}
			catch (Exception ex)
			{
				Report.Warning(1570, 1, mc.Location, "XML documentation comment on `{0}' is not well-formed XML markup ({1})", mc.GetSignatureForError(), ex.Message);
				return xmlDocumentation.CreateComment($"FIXME: Invalid documentation markup was found for member {name}");
			}
		}

		public void GenerateDocumentationForMember(MemberCore mc)
		{
			string name = mc.DocCommentHeader + mc.GetSignatureForDocumentation();
			XmlNode docCommentNode = GetDocCommentNode(mc, name);
			XmlElement xmlElement = docCommentNode as XmlElement;
			if (xmlElement != null)
			{
				IParametersMember parametersMember = mc as IParametersMember;
				if (parametersMember != null)
				{
					CheckParametersComments(mc, parametersMember, xmlElement);
				}
				XmlNodeList xmlNodeList = docCommentNode.SelectNodes(".//include");
				if (xmlNodeList.Count > 0)
				{
					List<XmlNode> list = new List<XmlNode>(xmlNodeList.Count);
					foreach (XmlNode item in xmlNodeList)
					{
						list.Add(item);
					}
					foreach (XmlElement item2 in list)
					{
						if (!HandleInclude(mc, item2))
						{
							item2.ParentNode.RemoveChild(item2);
						}
					}
				}
				foreach (XmlElement item3 in docCommentNode.SelectNodes(".//see"))
				{
					HandleSee(mc, item3);
				}
				foreach (XmlElement item4 in docCommentNode.SelectNodes(".//seealso"))
				{
					HandleSeeAlso(mc, item4);
				}
				foreach (XmlElement item5 in docCommentNode.SelectNodes(".//exception"))
				{
					HandleException(mc, item5);
				}
				foreach (XmlElement item6 in docCommentNode.SelectNodes(".//typeparam"))
				{
					HandleTypeParam(mc, item6);
				}
				foreach (XmlElement item7 in docCommentNode.SelectNodes(".//typeparamref"))
				{
					HandleTypeParamRef(mc, item7);
				}
			}
			docCommentNode.WriteTo(XmlCommentOutput);
		}

		private bool HandleInclude(MemberCore mc, XmlElement el)
		{
			bool result = false;
			string attribute = el.GetAttribute("file");
			string attribute2 = el.GetAttribute("path");
			if (attribute == "")
			{
				Report.Warning(1590, 1, mc.Location, "Invalid XML `include' element. Missing `file' attribute");
				el.ParentNode.InsertBefore(el.OwnerDocument.CreateComment(" Include tag is invalid "), el);
				result = true;
			}
			else if (attribute2.Length == 0)
			{
				Report.Warning(1590, 1, mc.Location, "Invalid XML `include' element. Missing `path' attribute");
				el.ParentNode.InsertBefore(el.OwnerDocument.CreateComment(" Include tag is invalid "), el);
				result = true;
			}
			else
			{
				Exception ex = null;
				string text = Path.Combine(Path.GetDirectoryName(mc.Location.NameFullPath), attribute);
				XmlDocument value;
				if (!StoredDocuments.TryGetValue(text, out value))
				{
					try
					{
						value = new XmlDocument();
						value.Load(text);
						StoredDocuments.Add(text, value);
					}
					catch (Exception ex2)
					{
						ex = ex2;
						el.ParentNode.InsertBefore(el.OwnerDocument.CreateComment($" Badly formed XML in at comment file `{attribute}': cannot be included "), el);
					}
				}
				if (value != null)
				{
					try
					{
						XmlNodeList xmlNodeList = value.SelectNodes(attribute2);
						if (xmlNodeList.Count == 0)
						{
							el.ParentNode.InsertBefore(el.OwnerDocument.CreateComment(" No matching elements were found for the include tag embedded here. "), el);
							result = true;
						}
						foreach (XmlNode item in xmlNodeList)
						{
							el.ParentNode.InsertBefore(el.OwnerDocument.ImportNode(item, true), el);
						}
					}
					catch (Exception ex3)
					{
						ex = ex3;
						el.ParentNode.InsertBefore(el.OwnerDocument.CreateComment(" Failed to insert some or all of included XML "), el);
					}
				}
				if (ex != null)
				{
					Report.Warning(1589, 1, mc.Location, "Unable to include XML fragment `{0}' of file `{1}'. {2}", attribute2, attribute, ex.Message);
				}
			}
			return result;
		}

		private void HandleSee(MemberCore mc, XmlElement see)
		{
			HandleXrefCommon(mc, see);
		}

		private void HandleSeeAlso(MemberCore mc, XmlElement seealso)
		{
			HandleXrefCommon(mc, seealso);
		}

		private void HandleException(MemberCore mc, XmlElement seealso)
		{
			HandleXrefCommon(mc, seealso);
		}

		private static void HandleTypeParam(MemberCore mc, XmlElement node)
		{
			if (node.HasAttribute("name"))
			{
				string attribute = node.GetAttribute("name");
				if (mc.CurrentTypeParameters == null || mc.CurrentTypeParameters.Find(attribute) == null)
				{
					mc.Compiler.Report.Warning(1711, 2, mc.Location, "XML comment on `{0}' has a typeparam name `{1}' but there is no type parameter by that name", mc.GetSignatureForError(), attribute);
				}
			}
		}

		private static void HandleTypeParamRef(MemberCore mc, XmlElement node)
		{
			if (!node.HasAttribute("name"))
			{
				return;
			}
			string attribute = node.GetAttribute("name");
			MemberCore memberCore = mc;
			do
			{
				if (memberCore.CurrentTypeParameters != null && memberCore.CurrentTypeParameters.Find(attribute) != null)
				{
					return;
				}
				memberCore = memberCore.Parent;
			}
			while (memberCore != null);
			mc.Compiler.Report.Warning(1735, 2, mc.Location, "XML comment on `{0}' has a typeparamref name `{1}' that could not be resolved", mc.GetSignatureForError(), attribute);
		}

		private FullNamedExpression ResolveMemberName(IMemberContext context, MemberName mn)
		{
			if (mn.Left == null)
			{
				return context.LookupNamespaceOrType(mn.Name, mn.Arity, LookupMode.Probing, Location.Null);
			}
			FullNamedExpression fullNamedExpression = ResolveMemberName(context, mn.Left);
			NamespaceExpression namespaceExpression = fullNamedExpression as NamespaceExpression;
			if (namespaceExpression != null)
			{
				return namespaceExpression.LookupTypeOrNamespace(context, mn.Name, mn.Arity, LookupMode.Probing, Location.Null);
			}
			TypeExpr typeExpr = fullNamedExpression as TypeExpr;
			if (typeExpr != null)
			{
				TypeSpec typeSpec = MemberCache.FindNestedType(typeExpr.Type, mn.Name, mn.Arity);
				if (typeSpec != null)
				{
					return new TypeExpression(typeSpec, Location.Null);
				}
				return null;
			}
			return fullNamedExpression;
		}

		private void HandleXrefCommon(MemberCore mc, XmlElement xref)
		{
			string attribute = xref.GetAttribute("cref");
			if (!xref.HasAttribute("cref") || (attribute.Length > 2 && attribute[1] == ':'))
			{
				return;
			}
			attribute = attribute.Replace('{', '<').Replace('}', '>');
			Encoding encoding = module.Compiler.Settings.Encoding;
			MemoryStream stream = new MemoryStream(encoding.GetBytes(attribute));
			CompilationSourceFile file = new CompilationSourceFile(doc_module, mc.Location.SourceFile);
			Report report = new Report(doc_module.Compiler, new NullReportPrinter());
			if (session == null)
			{
				session = new ParserSession
				{
					UseJayGlobalArrays = true
				};
			}
			CSharpParser cSharpParser = new CSharpParser(new SeekableStreamReader(stream, encoding, session.StreamReaderBuffer), file, report, session);
			ParsedParameters = null;
			ParsedName = null;
			ParsedBuiltinType = null;
			ParsedOperator = null;
			cSharpParser.Lexer.putback_char = 1048579;
			cSharpParser.Lexer.parsing_generic_declaration_doc = true;
			cSharpParser.parse();
			if (report.Errors > 0)
			{
				Report.Warning(1584, 1, mc.Location, "XML comment on `{0}' has syntactically incorrect cref attribute `{1}'", mc.GetSignatureForError(), attribute);
				xref.SetAttribute("cref", "!:" + attribute);
				return;
			}
			FullNamedExpression fullNamedExpression = null;
			MemberSpec memberSpec = ((ParsedBuiltinType == null || (ParsedParameters != null && ParsedName == null)) ? null : ParsedBuiltinType.Type);
			if (ParsedName != null || ParsedOperator.HasValue)
			{
				TypeSpec typeSpec = null;
				string text = null;
				if (memberSpec == null)
				{
					if (ParsedOperator.HasValue)
					{
						typeSpec = mc.CurrentType;
					}
					else if (ParsedName.Left != null)
					{
						fullNamedExpression = ResolveMemberName(mc, ParsedName.Left);
						if (fullNamedExpression != null)
						{
							NamespaceExpression namespaceExpression = fullNamedExpression as NamespaceExpression;
							if (namespaceExpression != null)
							{
								fullNamedExpression = namespaceExpression.LookupTypeOrNamespace(mc, ParsedName.Name, ParsedName.Arity, LookupMode.Probing, Location.Null);
								if (fullNamedExpression != null)
								{
									memberSpec = fullNamedExpression.Type;
								}
							}
							else
							{
								typeSpec = fullNamedExpression.Type;
							}
						}
					}
					else
					{
						fullNamedExpression = ResolveMemberName(mc, ParsedName);
						if (fullNamedExpression == null)
						{
							typeSpec = mc.CurrentType;
						}
						else if (ParsedParameters == null)
						{
							memberSpec = fullNamedExpression.Type;
						}
						else if (fullNamedExpression.Type.MemberDefinition == mc.CurrentType.MemberDefinition)
						{
							text = Constructor.ConstructorName;
							typeSpec = fullNamedExpression.Type;
						}
					}
				}
				else
				{
					typeSpec = (TypeSpec)memberSpec;
					memberSpec = null;
				}
				if (ParsedParameters != null)
				{
					ReportPrinter printer = mc.Module.Compiler.Report.SetPrinter(new NullReportPrinter());
					try
					{
						DocumentationMemberContext context = new DocumentationMemberContext(mc, ParsedName ?? MemberName.Null);
						foreach (DocumentationParameter parsedParameter in ParsedParameters)
						{
							parsedParameter.Resolve(context);
						}
					}
					finally
					{
						mc.Module.Compiler.Report.SetPrinter(printer);
					}
				}
				if (typeSpec != null)
				{
					if (text == null)
					{
						text = (ParsedOperator.HasValue ? Operator.GetMetadataName(ParsedOperator.Value) : ParsedName.Name);
					}
					int num = ((ParsedOperator == Operator.OpType.Explicit || ParsedOperator == Operator.OpType.Implicit) ? (ParsedParameters.Count - 1) : ((ParsedParameters != null) ? ParsedParameters.Count : 0));
					int num2 = -1;
					do
					{
						IList<MemberSpec> list = MemberCache.FindMembers(typeSpec, text, true);
						if (list != null)
						{
							foreach (MemberSpec item in list)
							{
								if (ParsedName != null && item.Arity != ParsedName.Arity)
								{
									continue;
								}
								if (ParsedParameters != null)
								{
									IParametersMember parametersMember = item as IParametersMember;
									if (parametersMember == null || (item.Kind == MemberKind.Operator && !ParsedOperator.HasValue))
									{
										continue;
									}
									AParametersCollection parameters = parametersMember.Parameters;
									int i;
									for (i = 0; i < num; i++)
									{
										DocumentationParameter documentationParameter = ParsedParameters[i];
										if (i >= parameters.Count || documentationParameter == null || documentationParameter.TypeSpec == null || !TypeSpecComparer.Override.IsEqual(documentationParameter.TypeSpec, parameters.Types[i]) || (documentationParameter.Modifier & Parameter.Modifier.RefOutMask) != (parameters.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask))
										{
											if (i > num2)
											{
												num2 = i;
											}
											i = -1;
											break;
										}
									}
									if (i < 0)
									{
										continue;
									}
									if (ParsedOperator == Operator.OpType.Explicit || ParsedOperator == Operator.OpType.Implicit)
									{
										if (parametersMember.MemberType != ParsedParameters[num].TypeSpec)
										{
											num2 = num + 1;
											continue;
										}
									}
									else if (num != parameters.Count)
									{
										continue;
									}
								}
								if (memberSpec != null)
								{
									Report.Warning(419, 3, mc.Location, "Ambiguous reference in cref attribute `{0}'. Assuming `{1}' but other overloads including `{2}' have also matched", attribute, memberSpec.GetSignatureForError(), item.GetSignatureForError());
									break;
								}
								memberSpec = item;
							}
						}
						typeSpec = ((memberSpec != null) ? null : typeSpec.DeclaringType);
					}
					while (typeSpec != null);
					if (memberSpec == null && num2 >= 0)
					{
						for (int j = num2; j < num; j++)
						{
							Report.Warning(1580, 1, mc.Location, "Invalid type for parameter `{0}' in XML comment cref attribute `{1}'", (j + 1).ToString(), attribute);
						}
						if (num2 == num + 1)
						{
							Report.Warning(1581, 1, mc.Location, "Invalid return type in XML comment cref attribute `{0}'", attribute);
						}
					}
				}
			}
			if (memberSpec != null)
			{
				attribute = ((memberSpec != InternalType.Namespace) ? (GetMemberDocHead(memberSpec) + memberSpec.GetSignatureForDocumentation()) : ("N:" + fullNamedExpression.GetSignatureForError()));
			}
			else
			{
				Report.Warning(1574, 1, mc.Location, "XML comment on `{0}' has cref attribute `{1}' that could not be resolved", mc.GetSignatureForError(), attribute);
				attribute = "!:" + attribute;
			}
			xref.SetAttribute("cref", attribute);
		}

		private static string GetMemberDocHead(MemberSpec type)
		{
			if (type is FieldSpec)
			{
				return "F:";
			}
			if (type is MethodSpec)
			{
				return "M:";
			}
			if (type is EventSpec)
			{
				return "E:";
			}
			if (type is PropertySpec)
			{
				return "P:";
			}
			if (type is TypeSpec)
			{
				return "T:";
			}
			throw new NotImplementedException(type.GetType().ToString());
		}

		private void CheckParametersComments(MemberCore member, IParametersMember paramMember, XmlElement el)
		{
			HashSet<string> hashSet = null;
			foreach (XmlElement item in el.SelectNodes("param"))
			{
				string attribute = item.GetAttribute("name");
				if (attribute.Length != 0)
				{
					if (hashSet == null)
					{
						hashSet = new HashSet<string>();
					}
					if (attribute != "" && paramMember.Parameters.GetParameterIndexByName(attribute) < 0)
					{
						Report.Warning(1572, 2, member.Location, "XML comment on `{0}' has a param tag for `{1}', but there is no parameter by that name", member.GetSignatureForError(), attribute);
					}
					else if (hashSet.Contains(attribute))
					{
						Report.Warning(1571, 2, member.Location, "XML comment on `{0}' has a duplicate param tag for `{1}'", member.GetSignatureForError(), attribute);
					}
					else
					{
						hashSet.Add(attribute);
					}
				}
			}
			if (hashSet == null)
			{
				return;
			}
			IParameterData[] fixedParameters = paramMember.Parameters.FixedParameters;
			for (int i = 0; i < fixedParameters.Length; i++)
			{
				Parameter parameter = (Parameter)fixedParameters[i];
				if (!hashSet.Contains(parameter.Name) && !(parameter is ArglistParameter))
				{
					Report.Warning(1573, 4, member.Location, "Parameter `{0}' has no matching param tag in the XML comment for `{1}'", parameter.Name, member.GetSignatureForError());
				}
			}
		}

		public bool OutputDocComment(string asmfilename, string xmlFileName)
		{
			XmlTextWriter xmlTextWriter = null;
			try
			{
				xmlTextWriter = new XmlTextWriter(xmlFileName, null);
				xmlTextWriter.Indentation = 4;
				xmlTextWriter.Formatting = Formatting.Indented;
				xmlTextWriter.WriteStartDocument();
				xmlTextWriter.WriteStartElement("doc");
				xmlTextWriter.WriteStartElement("assembly");
				xmlTextWriter.WriteStartElement("name");
				xmlTextWriter.WriteString(Path.GetFileNameWithoutExtension(asmfilename));
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.WriteStartElement("members");
				XmlCommentOutput = xmlTextWriter;
				module.GenerateDocComment(this);
				xmlTextWriter.WriteFullEndElement();
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.WriteWhitespace(Environment.NewLine);
				xmlTextWriter.WriteEndDocument();
				return true;
			}
			catch (Exception ex)
			{
				Report.Error(1569, "Error generating XML documentation file `{0}' (`{1}')", xmlFileName, ex.Message);
				return false;
			}
			finally
			{
				xmlTextWriter?.Close();
			}
		}
	}
}
