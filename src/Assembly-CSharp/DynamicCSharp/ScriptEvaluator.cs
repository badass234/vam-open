using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DynamicCSharp
{
	public sealed class ScriptEvaluator
	{
		private static Dictionary<string, ScriptProxy> evalCache = new Dictionary<string, ScriptProxy>();

		private ScriptDomain domain;

		private TextAsset templateSource;

		private Dictionary<string, Delegate> bindingDelegates = new Dictionary<string, Delegate>();

		private List<Variable> bindingVariables = new List<Variable>();

		private List<string> usingStatements = new List<string>();

		private const string templateResource = "DynamicCSharp_EvalTemplate";

		private const string entryClass = "_EvalClass";

		private const string entryMethod = "_EvalEntry";

		private const string returnObject = "_returnVal";

		private const string tagUsingStatements = "[TAG_USINGSTATEMENTS]";

		private const string tagClassName = "[TAG_CLASSNAME]";

		private const string tagFieldStatements = "[TAG_FIELDSTATEMENTS]";

		private const string tagDelegateStatements = "[TAG_DELEGATESTATEMENTS]";

		private const string tagMethodName = "[TAG_METHODNAME]";

		private const string tagMethodBody = "[TAG_METHODBODY]";

		private const string tagUsing = "using";

		private const string tagSpace = " ";

		private const string tagSemiColon = ";";

		private const string tagComma = ",";

		private const string tagArrowL = "<";

		private const string tagArrowR = ">";

		public static bool outputGeneratedSourceIfDebug = true;

		public ScriptEvaluator(ScriptDomain domain = null)
		{
			if (domain == null)
			{
				domain = ScriptDomain.Active;
			}
			if (domain == null)
			{
				throw new ArgumentNullException("The specified domain was null and there are no active domains");
			}
			if (domain.CompilerService == null)
			{
				throw new ArgumentException("The specified domain does not have a compiler service registered. The compiler service is required by a ScriptEvaluator");
			}
			this.domain = domain;
		}

		public Variable BindVar(string name, object value = null)
		{
			return this.BindVar<object>(name, value);
		}

		public Variable<T> BindVar<T>(string name, T value = default(T))
		{
			foreach (Variable bindingVariable in bindingVariables)
			{
				if (bindingVariable.Name == name)
				{
					bindingVariable.Update(value);
					return bindingVariable as Variable<T>;
				}
			}
			Variable<T> variable = new Variable<T>(name, value);
			bindingVariables.Add(variable);
			if (variable.Value != null)
			{
				Type type = variable.Value.GetType();
				AddUsing(type.Namespace);
			}
			return variable;
		}

		public void BindDelegate(string name, Action action)
		{
			if (bindingDelegates.ContainsKey(name))
			{
				bindingDelegates[name] = action;
			}
			else
			{
				bindingDelegates.Add(name, action);
			}
		}

		public void BindDelegate<T>(string name, Action<T> action)
		{
			if (bindingDelegates.ContainsKey(name))
			{
				bindingDelegates[name] = action;
			}
			else
			{
				bindingDelegates.Add(name, action);
			}
		}

		public void BindDelegate<R>(string name, Func<R> func)
		{
			if (bindingDelegates.ContainsKey(name))
			{
				bindingDelegates[name] = func;
			}
			else
			{
				bindingDelegates.Add(name, func);
			}
		}

		public void BindDelegate<R, T>(string name, Func<T, R> func)
		{
			if (bindingDelegates.ContainsKey(name))
			{
				bindingDelegates[name] = func;
			}
			else
			{
				bindingDelegates.Add(name, func);
			}
		}

		public void ClearVarBindings()
		{
			bindingVariables.Clear();
		}

		public void ClearDelegateBindings()
		{
			bindingDelegates.Clear();
		}

		public void AddUsing(string namespaceName)
		{
			if (!usingStatements.Contains(namespaceName))
			{
				usingStatements.Add(namespaceName);
			}
		}

		public Variable Eval(string sourceCode)
		{
			return Eval<object>(sourceCode);
		}

		public Variable<T> Eval<T>(string sourceCode)
		{
			ScriptProxy scriptProxy = null;
			bool flag = evalCache.ContainsKey(sourceCode);
			if (flag)
			{
				scriptProxy = evalCache[sourceCode];
			}
			else
			{
				string source = BuildSourceAroundTemplate(sourceCode);
				ScriptType scriptType = domain.CompileAndLoadScriptSource(source);
				if (scriptType == null)
				{
					return null;
				}
				scriptProxy = scriptType.CreateInstance();
				if (scriptProxy == null)
				{
					return null;
				}
			}
			if (!flag)
			{
				evalCache.Add(sourceCode, scriptProxy);
			}
			BindProxyDelegates(scriptProxy);
			BindProxyVars(scriptProxy);
			object obj = new object();
			scriptProxy.Fields["_returnVal"] = obj;
			object obj2 = scriptProxy.SafeCall("_EvalEntry");
			UnbindProxyVars(scriptProxy);
			if (obj == obj2)
			{
				return new Variable<T>("_returnVal", default(T));
			}
			T data = default(T);
			try
			{
				data = (T)obj2;
			}
			catch (InvalidCastException)
			{
			}
			return new Variable<T>("_returnVal", data);
		}

		private void BindProxyVars(ScriptProxy proxy)
		{
			foreach (Variable bindingVariable in bindingVariables)
			{
				proxy.Fields[bindingVariable.Name] = bindingVariable.Value;
			}
		}

		private void UnbindProxyVars(ScriptProxy proxy)
		{
			foreach (Variable bindingVariable in bindingVariables)
			{
				bindingVariable.Update(proxy.Fields[bindingVariable.Name]);
			}
		}

		private void BindProxyDelegates(ScriptProxy proxy)
		{
			List<string> list = new List<string>(bindingDelegates.Keys);
			foreach (string item in list)
			{
				Delegate value = bindingDelegates[item];
				proxy.Fields[item] = value;
			}
		}

		private string BuildSourceAroundTemplate(string source)
		{
			string text = GetTemplateSource();
			text = text.Replace("[TAG_USINGSTATEMENTS]", GetUsingStatementsSource());
			text = text.Replace("[TAG_DELEGATESTATEMENTS]", GetDelegateStatementsSource());
			text = text.Replace("[TAG_FIELDSTATEMENTS]", GetFieldStatementsSource());
			text = text.Replace("[TAG_CLASSNAME]", "_EvalClass" + Guid.NewGuid().ToString("N"));
			text = text.Replace("[TAG_METHODNAME]", "_EvalEntry");
			return text.Replace("[TAG_METHODBODY]", source);
		}

		private string GetTemplateSource()
		{
			if (templateSource == null)
			{
				templateSource = Resources.Load<TextAsset>("DynamicCSharp_EvalTemplate");
			}
			return templateSource.text;
		}

		private string GetUsingStatementsSource()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string usingStatement in usingStatements)
			{
				stringBuilder.Append("using");
				stringBuilder.Append(" ");
				stringBuilder.Append(usingStatement);
				stringBuilder.Append(";");
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}

		private string GetDelegateStatementsSource()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, Delegate> bindingDelegate in bindingDelegates)
			{
				Delegate value = bindingDelegate.Value;
				MethodInfo method = value.Method;
				ParameterInfo[] parameters = method.GetParameters();
				Type returnType = method.ReturnType;
				Type[] array = new Type[parameters.Length];
				for (int i = 0; i < parameters.Length; i++)
				{
					array[i] = parameters[i].ParameterType;
				}
				if (returnType == typeof(void))
				{
					stringBuilder.Append(typeof(Action).FullName);
					if (array.Length > 0)
					{
						stringBuilder.Append("<");
						for (int j = 0; j < array.Length; j++)
						{
							string fullName = array[j].FullName;
							stringBuilder.Append(fullName);
							if (j < array.Length - 1)
							{
								stringBuilder.Append(",");
							}
						}
						stringBuilder.Append(">");
					}
					stringBuilder.Append(" ");
					stringBuilder.Append(bindingDelegate.Key);
					stringBuilder.Append(";");
					stringBuilder.AppendLine();
					continue;
				}
				stringBuilder.Append(typeof(Func<>).FullName.Replace("`1", string.Empty));
				stringBuilder.Append("<");
				if (array.Length > 0)
				{
					for (int k = 0; k < array.Length; k++)
					{
						string fullName2 = array[k].FullName;
						stringBuilder.Append(fullName2);
						stringBuilder.Append(",");
					}
				}
				string fullName3 = returnType.FullName;
				stringBuilder.Append(fullName3);
				stringBuilder.Append(">");
				stringBuilder.Append(" ");
				stringBuilder.Append(bindingDelegate.Key);
				stringBuilder.Append(";");
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}

		private string GetFieldStatementsSource()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Variable bindingVariable in bindingVariables)
			{
				object value = bindingVariable.Value;
				Type type = ((value != null) ? value.GetType() : typeof(object));
				string fullName = type.FullName;
				stringBuilder.Append(fullName);
				stringBuilder.Append(" ");
				stringBuilder.Append(bindingVariable.Name);
				stringBuilder.Append(";");
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}

		public static void ClearCache()
		{
			evalCache.Clear();
		}
	}
}
