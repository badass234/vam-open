using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Mono.CSharp.yyParser;

namespace Mono.CSharp
{
	public class Tokenizer : yyInput
	{
		private class KeywordEntry<T>
		{
			public readonly T Token;

			public KeywordEntry<T> Next;

			public readonly char[] Value;

			public KeywordEntry(string value, T token)
			{
				Value = value.ToCharArray();
				Token = token;
			}
		}

		private sealed class IdentifiersComparer : IEqualityComparer<char[]>
		{
			private readonly int length;

			public IdentifiersComparer(int length)
			{
				this.length = length;
			}

			public bool Equals(char[] x, char[] y)
			{
				for (int i = 0; i < length; i++)
				{
					if (x[i] != y[i])
					{
						return false;
					}
				}
				return true;
			}

			public int GetHashCode(char[] obj)
			{
				int num = 0;
				for (int i = 0; i < length; i++)
				{
					num = (num << 5) - num + obj[i];
				}
				return num;
			}
		}

		public class LocatedTokenBuffer
		{
			private readonly LocatedToken[] buffer;

			public int pos;

			public LocatedTokenBuffer()
			{
				buffer = new LocatedToken[0];
			}

			public LocatedTokenBuffer(LocatedToken[] buffer)
			{
				this.buffer = buffer ?? new LocatedToken[0];
			}

			public LocatedToken Create(SourceFile file, int row, int column)
			{
				return Create(null, file, row, column);
			}

			public LocatedToken Create(string value, SourceFile file, int row, int column)
			{
				LocatedToken locatedToken;
				if (pos >= buffer.Length)
				{
					locatedToken = new LocatedToken();
				}
				else
				{
					locatedToken = buffer[pos];
					if (locatedToken == null)
					{
						locatedToken = new LocatedToken();
						buffer[pos] = locatedToken;
					}
					pos++;
				}
				locatedToken.value = value;
				locatedToken.file = file;
				locatedToken.row = row;
				locatedToken.column = column;
				return locatedToken;
			}

			[Conditional("FULL_AST")]
			public void CreateOptional(SourceFile file, int row, int col, ref object token)
			{
				token = Create(file, row, col);
			}
		}

		public enum PreprocessorDirective
		{
			Invalid = 0,
			Region = 1,
			Endregion = 2,
			If = 2051,
			Endif = 4,
			Elif = 2053,
			Else = 6,
			Define = 2055,
			Undef = 2056,
			Error = 9,
			Warning = 10,
			Pragma = 1035,
			Line = 1036,
			CustomArgumentsParsing = 1024,
			RequiresArgument = 2048
		}

		private class Position
		{
			public int position;

			public int line;

			public int ref_line;

			public int col;

			public Location hidden;

			public int putback_char;

			public int previous_col;

			public Stack<int> ifstack;

			public int parsing_generic_less_than;

			public int current_token;

			public object val;

			public int parsing_string_interpolation;

			public Stack<bool> parsing_string_interpolation_quoted;

			public Position(Tokenizer t)
			{
				position = t.reader.Position;
				line = t.line;
				ref_line = t.ref_line;
				col = t.col;
				hidden = t.hidden_block_start;
				putback_char = t.putback_char;
				previous_col = t.previous_col;
				if (t.ifstack != null && t.ifstack.Count != 0)
				{
					int[] array = t.ifstack.ToArray();
					Array.Reverse(array);
					ifstack = new Stack<int>(array);
				}
				parsing_generic_less_than = t.parsing_generic_less_than;
				current_token = t.current_token;
				val = t.val;
				parsing_string_interpolation = t.parsing_string_interpolation;
				if (t.parsing_string_interpolation_quoted != null && t.parsing_string_interpolation_quoted.Count != 0)
				{
					bool[] array2 = t.parsing_string_interpolation_quoted.ToArray();
					Array.Reverse(array2);
					parsing_string_interpolation_quoted = new Stack<bool>(array2);
				}
			}
		}

		private readonly SeekableStreamReader reader;

		private readonly CompilationSourceFile source_file;

		private readonly CompilerContext context;

		private readonly Report Report;

		private SourceFile current_source;

		private Location hidden_block_start;

		private int ref_line = 1;

		private int line = 1;

		private int col;

		private int previous_col;

		private int current_token;

		private readonly int tab_size;

		private bool handle_get_set;

		private bool handle_remove_add;

		private bool handle_where;

		private bool lambda_arguments_parsing;

		private List<Location> escaped_identifiers;

		private int parsing_generic_less_than;

		private readonly bool doc_processing;

		private readonly LocatedTokenBuffer ltb;

		public int parsing_block;

		public bool query_parsing;

		public int parsing_type;

		public bool parsing_generic_declaration;

		public bool parsing_generic_declaration_doc;

		public int parsing_declaration;

		public bool parsing_attribute_section;

		public bool parsing_modifiers;

		public bool parsing_catch_when;

		private int parsing_string_interpolation;

		private Stack<bool> parsing_string_interpolation_quoted;

		public bool parsing_interpolation_format;

		public const int EvalStatementParserCharacter = 1048576;

		public const int EvalCompilationUnitParserCharacter = 1048577;

		public const int EvalUsingDeclarationsParserCharacter = 1048578;

		public const int DocumentationXref = 1048579;

		private const int UnicodeLS = 8232;

		private const int UnicodePS = 8233;

		private StringBuilder xml_comment_buffer;

		private XmlCommentState xml_doc_state;

		private bool tokens_seen;

		private bool generated;

		private bool any_token_seen;

		private static readonly KeywordEntry<int>[][] keywords;

		private static readonly KeywordEntry<PreprocessorDirective>[][] keywords_preprocessor;

		private static readonly HashSet<string> keyword_strings;

		private static readonly NumberStyles styles;

		private static readonly NumberFormatInfo csharp_format_info;

		private static readonly char[] pragma_warning;

		private static readonly char[] pragma_warning_disable;

		private static readonly char[] pragma_warning_restore;

		private static readonly char[] pragma_checksum;

		private static readonly char[] line_hidden;

		private static readonly char[] line_default;

		private static readonly char[] simple_whitespaces;

		public bool CompleteOnEOF;

		public int putback_char;

		private object val;

		private const int TAKING = 1;

		private const int ELSE_SEEN = 4;

		private const int PARENT_TAKING = 8;

		private const int REGION = 16;

		private Stack<int> ifstack;

		public const int MaxIdentifierLength = 512;

		public const int MaxNumberLength = 512;

		private readonly char[] id_builder;

		private readonly Dictionary<char[], string>[] identifiers;

		private readonly char[] number_builder;

		private int number_pos;

		private char[] value_builder = new char[64];

		private Stack<Position> position_stack = new Stack<Position>(2);

		public bool PropertyParsing
		{
			get
			{
				return handle_get_set;
			}
			set
			{
				handle_get_set = value;
			}
		}

		public bool EventParsing
		{
			get
			{
				return handle_remove_add;
			}
			set
			{
				handle_remove_add = value;
			}
		}

		public bool ConstraintsParsing
		{
			get
			{
				return handle_where;
			}
			set
			{
				handle_where = value;
			}
		}

		public XmlCommentState doc_state
		{
			get
			{
				return xml_doc_state;
			}
			set
			{
				if (value == XmlCommentState.Allowed)
				{
					check_incorrect_doc_comment();
					reset_doc_comment();
				}
				xml_doc_state = value;
			}
		}

		public int Line => ref_line;

		public Location Location => new Location(current_source, ref_line, col);

		public object Value => val;

		private void AddEscapedIdentifier(Location loc)
		{
			if (escaped_identifiers == null)
			{
				escaped_identifiers = new List<Location>();
			}
			escaped_identifiers.Add(loc);
		}

		public bool IsEscapedIdentifier(ATypeNameExpression name)
		{
			if (escaped_identifiers != null)
			{
				return escaped_identifiers.Contains(name.Location);
			}
			return false;
		}

		public Tokenizer(SeekableStreamReader input, CompilationSourceFile file, ParserSession session, Report report)
		{
			source_file = file;
			context = file.Compiler;
			current_source = file.SourceFile;
			identifiers = session.Identifiers;
			id_builder = session.IDBuilder;
			number_builder = session.NumberBuilder;
			ltb = new LocatedTokenBuffer(session.LocatedTokens);
			Report = report;
			reader = input;
			putback_char = -1;
			xml_comment_buffer = new StringBuilder();
			doc_processing = context.Settings.DocumentationFile != null;
			tab_size = context.Settings.TabSize;
		}

		public void PushPosition()
		{
			position_stack.Push(new Position(this));
		}

		public void PopPosition()
		{
			Position position = position_stack.Pop();
			reader.Position = position.position;
			ref_line = position.ref_line;
			line = position.line;
			col = position.col;
			hidden_block_start = position.hidden;
			putback_char = position.putback_char;
			previous_col = position.previous_col;
			ifstack = position.ifstack;
			parsing_generic_less_than = position.parsing_generic_less_than;
			parsing_string_interpolation = position.parsing_string_interpolation;
			parsing_string_interpolation_quoted = position.parsing_string_interpolation_quoted;
			current_token = position.current_token;
			val = position.val;
		}

		public void DiscardPosition()
		{
			position_stack.Pop();
		}

		private static void AddKeyword(string kw, int token)
		{
			keyword_strings.Add(kw);
			AddKeyword(keywords, kw, token);
		}

		private static void AddPreprocessorKeyword(string kw, PreprocessorDirective directive)
		{
			AddKeyword(keywords_preprocessor, kw, directive);
		}

		private static void AddKeyword<T>(KeywordEntry<T>[][] keywords, string kw, T token)
		{
			int length = kw.Length;
			if (keywords[length] == null)
			{
				keywords[length] = new KeywordEntry<T>[28];
			}
			int num = kw[0] - 95;
			KeywordEntry<T> keywordEntry = keywords[length][num];
			if (keywordEntry == null)
			{
				keywords[length][num] = new KeywordEntry<T>(kw, token);
				return;
			}
			while (keywordEntry.Next != null)
			{
				keywordEntry = keywordEntry.Next;
			}
			keywordEntry.Next = new KeywordEntry<T>(kw, token);
		}

		static Tokenizer()
		{
			pragma_warning = "warning".ToCharArray();
			pragma_warning_disable = "disable".ToCharArray();
			pragma_warning_restore = "restore".ToCharArray();
			pragma_checksum = "checksum".ToCharArray();
			line_hidden = "hidden".ToCharArray();
			line_default = "default".ToCharArray();
			simple_whitespaces = new char[2] { ' ', '\t' };
			keyword_strings = new HashSet<string>();
			keywords = new KeywordEntry<int>[11][];
			AddKeyword("__arglist", 341);
			AddKeyword("__makeref", 361);
			AddKeyword("__reftype", 360);
			AddKeyword("__refvalue", 359);
			AddKeyword("abstract", 261);
			AddKeyword("as", 262);
			AddKeyword("add", 263);
			AddKeyword("base", 264);
			AddKeyword("bool", 265);
			AddKeyword("break", 266);
			AddKeyword("byte", 267);
			AddKeyword("case", 268);
			AddKeyword("catch", 269);
			AddKeyword("char", 270);
			AddKeyword("checked", 271);
			AddKeyword("class", 272);
			AddKeyword("const", 273);
			AddKeyword("continue", 274);
			AddKeyword("decimal", 275);
			AddKeyword("default", 276);
			AddKeyword("delegate", 277);
			AddKeyword("do", 278);
			AddKeyword("double", 279);
			AddKeyword("else", 280);
			AddKeyword("enum", 281);
			AddKeyword("event", 282);
			AddKeyword("explicit", 283);
			AddKeyword("extern", 284);
			AddKeyword("false", 285);
			AddKeyword("finally", 286);
			AddKeyword("fixed", 287);
			AddKeyword("float", 288);
			AddKeyword("for", 289);
			AddKeyword("foreach", 290);
			AddKeyword("goto", 291);
			AddKeyword("get", 368);
			AddKeyword("if", 292);
			AddKeyword("implicit", 293);
			AddKeyword("in", 294);
			AddKeyword("int", 295);
			AddKeyword("interface", 296);
			AddKeyword("internal", 297);
			AddKeyword("is", 298);
			AddKeyword("lock", 299);
			AddKeyword("long", 300);
			AddKeyword("namespace", 301);
			AddKeyword("new", 302);
			AddKeyword("null", 303);
			AddKeyword("object", 304);
			AddKeyword("operator", 305);
			AddKeyword("out", 306);
			AddKeyword("override", 307);
			AddKeyword("params", 308);
			AddKeyword("private", 309);
			AddKeyword("protected", 310);
			AddKeyword("public", 311);
			AddKeyword("readonly", 312);
			AddKeyword("ref", 313);
			AddKeyword("remove", 315);
			AddKeyword("return", 314);
			AddKeyword("sbyte", 316);
			AddKeyword("sealed", 317);
			AddKeyword("set", 369);
			AddKeyword("short", 318);
			AddKeyword("sizeof", 319);
			AddKeyword("stackalloc", 320);
			AddKeyword("static", 321);
			AddKeyword("string", 322);
			AddKeyword("struct", 323);
			AddKeyword("switch", 324);
			AddKeyword("this", 325);
			AddKeyword("throw", 326);
			AddKeyword("true", 327);
			AddKeyword("try", 328);
			AddKeyword("typeof", 329);
			AddKeyword("uint", 330);
			AddKeyword("ulong", 331);
			AddKeyword("unchecked", 332);
			AddKeyword("unsafe", 333);
			AddKeyword("ushort", 334);
			AddKeyword("using", 335);
			AddKeyword("virtual", 336);
			AddKeyword("void", 337);
			AddKeyword("volatile", 338);
			AddKeyword("while", 340);
			AddKeyword("partial", 342);
			AddKeyword("where", 339);
			AddKeyword("from", 344);
			AddKeyword("join", 346);
			AddKeyword("on", 347);
			AddKeyword("equals", 348);
			AddKeyword("select", 349);
			AddKeyword("group", 350);
			AddKeyword("by", 351);
			AddKeyword("let", 352);
			AddKeyword("orderby", 353);
			AddKeyword("ascending", 354);
			AddKeyword("descending", 355);
			AddKeyword("into", 356);
			AddKeyword("async", 362);
			AddKeyword("await", 363);
			AddKeyword("when", 365);
			keywords_preprocessor = new KeywordEntry<PreprocessorDirective>[10][];
			AddPreprocessorKeyword("region", PreprocessorDirective.Region);
			AddPreprocessorKeyword("endregion", PreprocessorDirective.Endregion);
			AddPreprocessorKeyword("if", PreprocessorDirective.If);
			AddPreprocessorKeyword("endif", PreprocessorDirective.Endif);
			AddPreprocessorKeyword("elif", PreprocessorDirective.Elif);
			AddPreprocessorKeyword("else", PreprocessorDirective.Else);
			AddPreprocessorKeyword("define", PreprocessorDirective.Define);
			AddPreprocessorKeyword("undef", PreprocessorDirective.Undef);
			AddPreprocessorKeyword("error", PreprocessorDirective.Error);
			AddPreprocessorKeyword("warning", PreprocessorDirective.Warning);
			AddPreprocessorKeyword("pragma", PreprocessorDirective.Pragma);
			AddPreprocessorKeyword("line", PreprocessorDirective.Line);
			csharp_format_info = NumberFormatInfo.InvariantInfo;
			styles = NumberStyles.Float;
		}

		private int GetKeyword(char[] id, int id_len)
		{
			if (id_len >= keywords.Length || keywords[id_len] == null)
			{
				return -1;
			}
			int num = id[0] - 95;
			if (num > 27)
			{
				return -1;
			}
			KeywordEntry<int> keywordEntry = keywords[id_len][num];
			if (keywordEntry == null)
			{
				return -1;
			}
			int num2;
			do
			{
				num2 = keywordEntry.Token;
				for (int i = 1; i < id_len; i++)
				{
					if (id[i] != keywordEntry.Value[i])
					{
						num2 = 0;
						keywordEntry = keywordEntry.Next;
						break;
					}
				}
			}
			while (num2 == 0 && keywordEntry != null);
			int num3;
			switch (num2)
			{
			case 0:
				return -1;
			case 368:
			case 369:
				if (!handle_get_set)
				{
					num2 = -1;
				}
				break;
			case 263:
			case 315:
				if (!handle_remove_add)
				{
					num2 = -1;
				}
				break;
			case 284:
				if (parsing_declaration == 0)
				{
					num2 = 358;
				}
				break;
			case 276:
				if (peek_token() == 379)
				{
					token();
					num2 = 426;
				}
				break;
			case 365:
				if (current_token != 269 && !parsing_catch_when)
				{
					num2 = -1;
				}
				break;
			case 339:
				if ((!handle_where || current_token == 379) && !query_parsing)
				{
					num2 = -1;
				}
				break;
			case 344:
			{
				if (query_parsing)
				{
					break;
				}
				if (lambda_arguments_parsing || parsing_block == 0)
				{
					num2 = -1;
					break;
				}
				PushPosition();
				parsing_generic_less_than = 1;
				int num5 = xtoken();
				if (num5 <= 295)
				{
					if (num5 <= 270)
					{
						if (num5 == 265 || num5 == 267 || num5 == 270)
						{
							goto IL_02fe;
						}
					}
					else if (num5 <= 279)
					{
						if (num5 == 275 || num5 == 279)
						{
							goto IL_02fe;
						}
					}
					else if (num5 == 288 || num5 == 295)
					{
						goto IL_02fe;
					}
				}
				else if (num5 <= 322)
				{
					if (num5 == 300 || num5 == 304 || num5 == 322)
					{
						goto IL_02fe;
					}
				}
				else if (num5 <= 331)
				{
					if (num5 == 330 || num5 == 331)
					{
						goto IL_02fe;
					}
				}
				else
				{
					if (num5 == 337)
					{
						Expression.Error_VoidInvalidInTheContext(Location, Report);
						goto IL_03a6;
					}
					if (num5 == 422)
					{
						goto IL_02fe;
					}
				}
				goto IL_0376;
			}
			case 346:
			case 347:
			case 348:
			case 349:
			case 350:
			case 351:
			case 352:
			case 353:
			case 354:
			case 355:
			case 356:
				if (!query_parsing)
				{
					num2 = -1;
				}
				break;
			case 301:
			case 335:
				check_incorrect_doc_comment();
				parsing_modifiers = false;
				break;
			case 342:
			{
				if (parsing_block > 0)
				{
					num2 = -1;
					break;
				}
				PushPosition();
				num3 = token();
				bool num4 = num3 == 272 || num3 == 323 || num3 == 296 || num3 == 337;
				PopPosition();
				if (num4)
				{
					if (num3 == 337)
					{
						if (context.Settings.Version <= LanguageVersion.ISO_2)
						{
							Report.FeatureIsNotAvailable(context, Location, "partial methods");
						}
					}
					else if (context.Settings.Version == LanguageVersion.ISO_1)
					{
						Report.FeatureIsNotAvailable(context, Location, "partial types");
					}
					return num2;
				}
				if (num3 < 370)
				{
					Report.Error(267, Location, "The `partial' modifier can be used only immediately before `class', `struct', `interface', or `void' keyword");
					return token();
				}
				id_builder[0] = 'p';
				id_builder[1] = 'a';
				id_builder[2] = 'r';
				id_builder[3] = 't';
				id_builder[4] = 'i';
				id_builder[5] = 'a';
				id_builder[6] = 'l';
				num2 = -1;
				break;
			}
			case 362:
				if (parsing_modifiers)
				{
					if (parsing_attribute_section || peek_token() == 375)
					{
						num2 = -1;
					}
				}
				else if (parsing_block > 0)
				{
					int num5 = peek_token();
					if (num5 != 277)
					{
						if (num5 != 422)
						{
							if (num5 != 423)
							{
								goto IL_0580;
							}
						}
						else
						{
							PushPosition();
							xtoken();
							if (xtoken() != 343)
							{
								PopPosition();
								goto IL_0580;
							}
							PopPosition();
						}
					}
				}
				else
				{
					num2 = -1;
				}
				goto IL_05b8;
			case 363:
				{
					if (parsing_block == 0)
					{
						num2 = -1;
					}
					break;
				}
				IL_0376:
				PopPosition();
				id_builder[0] = 'f';
				id_builder[1] = 'r';
				id_builder[2] = 'o';
				id_builder[3] = 'm';
				return -1;
				IL_02fe:
				num3 = xtoken();
				if (num3 == 380 || num3 == 378 || num3 == 348 || num3 == 385)
				{
					goto IL_0376;
				}
				num2 = 345;
				query_parsing = true;
				if (context.Settings.Version <= LanguageVersion.ISO_2)
				{
					Report.FeatureIsNotAvailable(context, Location, "query expressions");
				}
				goto IL_03a6;
				IL_0580:
				id_builder[0] = 'a';
				id_builder[1] = 's';
				id_builder[2] = 'y';
				id_builder[3] = 'n';
				id_builder[4] = 'c';
				num2 = -1;
				goto IL_05b8;
				IL_05b8:
				if (num2 == 362 && context.Settings.Version <= LanguageVersion.V_4)
				{
					Report.FeatureIsNotAvailable(context, Location, "asynchronous functions");
				}
				break;
				IL_03a6:
				PopPosition();
				break;
			}
			return num2;
		}

		private static PreprocessorDirective GetPreprocessorDirective(char[] id, int id_len)
		{
			if (id_len >= keywords_preprocessor.Length || keywords_preprocessor[id_len] == null)
			{
				return PreprocessorDirective.Invalid;
			}
			int num = id[0] - 95;
			if (num > 27)
			{
				return PreprocessorDirective.Invalid;
			}
			KeywordEntry<PreprocessorDirective> keywordEntry = keywords_preprocessor[id_len][num];
			if (keywordEntry == null)
			{
				return PreprocessorDirective.Invalid;
			}
			PreprocessorDirective preprocessorDirective = PreprocessorDirective.Invalid;
			do
			{
				preprocessorDirective = keywordEntry.Token;
				for (int i = 1; i < id_len; i++)
				{
					if (id[i] != keywordEntry.Value[i])
					{
						preprocessorDirective = PreprocessorDirective.Invalid;
						keywordEntry = keywordEntry.Next;
						break;
					}
				}
			}
			while (preprocessorDirective == PreprocessorDirective.Invalid && keywordEntry != null);
			return preprocessorDirective;
		}

		private static bool is_identifier_start_character(int c)
		{
			if ((c >= 97 && c <= 122) || (c >= 65 && c <= 90) || c == 95)
			{
				return true;
			}
			if (c < 128)
			{
				return false;
			}
			return is_identifier_start_character_slow_part((char)c);
		}

		private static bool is_identifier_part_character(char c)
		{
			if (c >= 'a' && c <= 'z')
			{
				return true;
			}
			if (c >= 'A' && c <= 'Z')
			{
				return true;
			}
			switch (c)
			{
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
			case '_':
				return true;
			default:
				if (c < '\u0080')
				{
					return false;
				}
				return is_identifier_part_character_slow_part(c);
			}
		}

		private static bool is_identifier_start_character_slow_part(char c)
		{
			switch (char.GetUnicodeCategory(c))
			{
			case UnicodeCategory.UppercaseLetter:
			case UnicodeCategory.LowercaseLetter:
			case UnicodeCategory.TitlecaseLetter:
			case UnicodeCategory.ModifierLetter:
			case UnicodeCategory.OtherLetter:
			case UnicodeCategory.LetterNumber:
				return true;
			default:
				return false;
			}
		}

		private static bool is_identifier_part_character_slow_part(char c)
		{
			switch (char.GetUnicodeCategory(c))
			{
			case UnicodeCategory.UppercaseLetter:
			case UnicodeCategory.LowercaseLetter:
			case UnicodeCategory.TitlecaseLetter:
			case UnicodeCategory.ModifierLetter:
			case UnicodeCategory.OtherLetter:
			case UnicodeCategory.NonSpacingMark:
			case UnicodeCategory.SpacingCombiningMark:
			case UnicodeCategory.DecimalDigitNumber:
			case UnicodeCategory.LetterNumber:
			case UnicodeCategory.ConnectorPunctuation:
				return true;
			case UnicodeCategory.Format:
				return c != '\ufeff';
			default:
				return false;
			}
		}

		public static bool IsKeyword(string s)
		{
			return keyword_strings.Contains(s);
		}

		private int TokenizeOpenParens()
		{
			current_token = -1;
			int num = 0;
			bool flag = false;
			bool flag2 = false;
			while (true)
			{
				int num2 = current_token;
				token();
				switch (current_token)
				{
				case 376:
					token();
					if (current_token == 343)
					{
						return 423;
					}
					if (flag)
					{
						if (current_token == 380)
						{
							return 375;
						}
						return 424;
					}
					if (flag2)
					{
						switch (current_token)
						{
						case 264:
						case 265:
						case 267:
						case 270:
						case 271:
						case 275:
						case 276:
						case 277:
						case 279:
						case 285:
						case 287:
						case 288:
						case 295:
						case 300:
						case 302:
						case 303:
						case 318:
						case 319:
						case 322:
						case 325:
						case 326:
						case 327:
						case 329:
						case 330:
						case 331:
						case 332:
						case 333:
						case 334:
						case 363:
						case 375:
						case 381:
						case 384:
						case 421:
						case 422:
							return 424;
						}
					}
					return 375;
				case 377:
				case 395:
					if (num2 == 422 || num2 == 420)
					{
						continue;
					}
					break;
				case 363:
				case 422:
					switch (num2)
					{
					case 377:
						if (num == 0)
						{
							flag = false;
							flag2 = true;
						}
						break;
					case -1:
					case 378:
					case 395:
					case 418:
						if (num == 0)
						{
							flag2 = true;
						}
						break;
					default:
						flag2 = (flag = false);
						break;
					}
					continue;
				case 265:
				case 267:
				case 270:
				case 275:
				case 279:
				case 288:
				case 295:
				case 300:
				case 304:
				case 316:
				case 318:
				case 322:
				case 330:
				case 331:
				case 334:
				case 337:
					if (num == 0)
					{
						flag = true;
					}
					continue;
				case 378:
					if (num == 0)
					{
						num = 100;
						flag2 = (flag = false);
					}
					continue;
				case 373:
				case 418:
					if (num++ == 0)
					{
						flag = true;
					}
					continue;
				case 374:
				case 420:
					num--;
					continue;
				case 357:
				case 390:
					if (num == 0)
					{
						flag = true;
					}
					continue;
				case 306:
				case 313:
					flag2 = (flag = false);
					continue;
				}
				break;
			}
			return 375;
		}

		public static bool IsValidIdentifier(string s)
		{
			if (s == null || s.Length == 0)
			{
				return false;
			}
			if (!is_identifier_start_character(s[0]))
			{
				return false;
			}
			for (int i = 1; i < s.Length; i++)
			{
				if (!is_identifier_part_character(s[i]))
				{
					return false;
				}
			}
			return true;
		}

		private bool parse_less_than(ref int genericDimension)
		{
			while (true)
			{
				int num = token();
				if (num == 373)
				{
					do
					{
						num = token();
						if (num == 257)
						{
							return true;
						}
					}
					while (num != 374);
					num = token();
				}
				else if (num == 294 || num == 306)
				{
					num = token();
				}
				switch (num)
				{
				case 420:
					genericDimension = 1;
					return true;
				case 294:
				case 306:
					return true;
				case 378:
					while (true)
					{
						genericDimension++;
						switch (token())
						{
						case 378:
							break;
						case 420:
							genericDimension++;
							return true;
						default:
							return false;
						}
					}
				default:
					return false;
				case 265:
				case 267:
				case 270:
				case 275:
				case 279:
				case 288:
				case 295:
				case 300:
				case 304:
				case 316:
				case 318:
				case 322:
				case 330:
				case 331:
				case 334:
				case 337:
				case 422:
					break;
				}
				while (true)
				{
					switch (token())
					{
					case 377:
					case 378:
					case 395:
						break;
					case 357:
					case 390:
						continue;
					case 420:
						return true;
					case 418:
						if (parse_less_than(ref genericDimension))
						{
							continue;
						}
						return false;
					case 373:
						while (true)
						{
							switch (token())
							{
							case 374:
								break;
							case 378:
								continue;
							default:
								return false;
							}
							break;
						}
						continue;
					default:
						return false;
					}
					break;
				}
			}
		}

		public int peek_token()
		{
			PushPosition();
			int result = token();
			PopPosition();
			return result;
		}

		private int TokenizePossibleNullableType()
		{
			if (parsing_block == 0 || parsing_type > 0)
			{
				return 357;
			}
			switch (peek_char())
			{
			case 63:
				get_char();
				return 417;
			case 46:
				return 364;
			case 44:
			case 59:
			case 62:
				return 357;
			case 42:
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
				return 394;
			default:
			{
				PushPosition();
				current_token = 258;
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4;
				switch (xtoken())
				{
				case 377:
				case 427:
					num4 = 364;
					break;
				case 285:
				case 302:
				case 303:
				case 325:
				case 327:
				case 366:
				case 421:
					num4 = 394;
					break;
				case 373:
				case 376:
				case 378:
				case 379:
				case 380:
				case 394:
				case 417:
				case 420:
					num4 = 357;
					break;
				case 375:
				case 423:
				case 424:
					num4 = -1;
					num++;
					break;
				case 418:
				case 419:
				case 425:
					num4 = -1;
					num2++;
					break;
				default:
					num4 = -1;
					break;
				}
				if (num4 == -1)
				{
					switch (xtoken())
					{
					case 294:
					case 371:
					case 378:
					case 380:
						num4 = 357;
						break;
					case 379:
						num4 = 394;
						break;
					case 375:
					case 423:
					case 424:
						num++;
						goto default;
					case 373:
					case 427:
						num3++;
						goto default;
					case 376:
						num--;
						goto default;
					case 418:
					case 419:
					case 425:
						num2++;
						goto default;
					default:
					{
						int num5 = 1;
						int num6 = 0;
						int num7 = 0;
						int num8;
						while ((num8 = xtoken()) != 257)
						{
							switch (num8)
							{
							case 371:
								num7++;
								continue;
							case 375:
							case 423:
							case 424:
								num++;
								continue;
							case 372:
								num7--;
								continue;
							case 418:
							case 419:
							case 425:
								num2++;
								continue;
							case 373:
							case 427:
								num3++;
								continue;
							case 374:
								num3--;
								continue;
							case 376:
								if (num > 0)
								{
									num--;
									continue;
								}
								PopPosition();
								return 357;
							case 420:
								if (num2 > 0)
								{
									num2--;
									continue;
								}
								PopPosition();
								return 357;
							}
							if (num7 != 0)
							{
								continue;
							}
							if (num8 == 380)
							{
								break;
							}
							if (num != 0)
							{
								continue;
							}
							switch (num8)
							{
							case 378:
								if (num2 != 0 || num3 != 0)
								{
									continue;
								}
								PopPosition();
								return 357;
							case 379:
								break;
							case 394:
								num5++;
								continue;
							default:
								continue;
							}
							if (++num6 == num5)
							{
								break;
							}
						}
						num4 = ((num6 != num5 && num7 == 0) ? 357 : 394);
						break;
					}
					}
				}
				PopPosition();
				return num4;
			}
			}
		}

		private bool decimal_digits(int c)
		{
			bool result = false;
			if (c != -1)
			{
				if (number_pos == 512)
				{
					Error_NumericConstantTooLong();
				}
				number_builder[number_pos++] = (char)c;
			}
			int num;
			while ((num = peek_char2()) != -1 && num >= 48 && num <= 57)
			{
				if (number_pos == 512)
				{
					Error_NumericConstantTooLong();
				}
				number_builder[number_pos++] = (char)num;
				get_char();
				result = true;
			}
			return result;
		}

		private static bool is_hex(int e)
		{
			if ((e < 48 || e > 57) && (e < 65 || e > 70))
			{
				if (e >= 97)
				{
					return e <= 102;
				}
				return false;
			}
			return true;
		}

		private static TypeCode real_type_suffix(int c)
		{
			switch (c)
			{
			case 70:
			case 102:
				return TypeCode.Single;
			case 68:
			case 100:
				return TypeCode.Double;
			case 77:
			case 109:
				return TypeCode.Decimal;
			default:
				return TypeCode.Empty;
			}
		}

		private ILiteralConstant integer_type_suffix(ulong ul, int c, Location loc)
		{
			bool flag = false;
			bool flag2 = false;
			if (c != -1)
			{
				bool flag3 = true;
				do
				{
					if (c <= 85)
					{
						if (c == 76)
						{
							goto IL_0051;
						}
						if (c == 85)
						{
							goto IL_0025;
						}
					}
					else
					{
						if (c == 108)
						{
							if (!flag)
							{
								Report.Warning(78, 4, Location, "The `l' suffix is easily confused with the digit `1' (use `L' for clarity)");
							}
							goto IL_0051;
						}
						if (c == 117)
						{
							goto IL_0025;
						}
					}
					flag3 = false;
					goto IL_0063;
					IL_0025:
					if (flag)
					{
						flag3 = false;
					}
					flag = true;
					get_char();
					goto IL_0063;
					IL_0051:
					if (flag2)
					{
						flag3 = false;
					}
					flag2 = true;
					get_char();
					goto IL_0063;
					IL_0063:
					c = peek_char();
				}
				while (flag3);
			}
			if (flag2 & flag)
			{
				return new ULongLiteral(context.BuiltinTypes, ul, loc);
			}
			if (flag)
			{
				if ((ul & 0xFFFFFFFF00000000uL) == 0L)
				{
					return new UIntLiteral(context.BuiltinTypes, (uint)ul, loc);
				}
				return new ULongLiteral(context.BuiltinTypes, ul, loc);
			}
			if (flag2)
			{
				if ((ul & 0x8000000000000000uL) != 0L)
				{
					return new ULongLiteral(context.BuiltinTypes, ul, loc);
				}
				return new LongLiteral(context.BuiltinTypes, (long)ul, loc);
			}
			if ((ul & 0xFFFFFFFF00000000uL) == 0L)
			{
				uint num = (uint)ul;
				if ((num & 0x80000000u) != 0)
				{
					return new UIntLiteral(context.BuiltinTypes, num, loc);
				}
				return new IntLiteral(context.BuiltinTypes, (int)num, loc);
			}
			if ((ul & 0x8000000000000000uL) != 0L)
			{
				return new ULongLiteral(context.BuiltinTypes, ul, loc);
			}
			return new LongLiteral(context.BuiltinTypes, (long)ul, loc);
		}

		private ILiteralConstant adjust_int(int c, Location loc)
		{
			try
			{
				if (number_pos > 9)
				{
					ulong num = (uint)(number_builder[0] - 48);
					for (int i = 1; i < number_pos; i++)
					{
						num = checked(num * 10 + (uint)(number_builder[i] - 48));
					}
					return integer_type_suffix(num, c, loc);
				}
				uint num2 = (uint)(number_builder[0] - 48);
				for (int j = 1; j < number_pos; j++)
				{
					num2 = checked(num2 * 10 + (uint)(number_builder[j] - 48));
				}
				return integer_type_suffix(num2, c, loc);
			}
			catch (OverflowException)
			{
				Error_NumericConstantTooLong();
				return new IntLiteral(context.BuiltinTypes, 0, loc);
			}
			catch (FormatException)
			{
				Report.Error(1013, Location, "Invalid number");
				return new IntLiteral(context.BuiltinTypes, 0, loc);
			}
		}

		private ILiteralConstant adjust_real(TypeCode t, Location loc)
		{
			string s = new string(number_builder, 0, number_pos);
			switch (t)
			{
			case TypeCode.Decimal:
				try
				{
					return new DecimalLiteral(context.BuiltinTypes, decimal.Parse(s, styles, csharp_format_info), loc);
				}
				catch (OverflowException)
				{
					Report.Error(594, Location, "Floating-point constant is outside the range of type `{0}'", "decimal");
					return new DecimalLiteral(context.BuiltinTypes, 0m, loc);
				}
			case TypeCode.Single:
				try
				{
					return new FloatLiteral(context.BuiltinTypes, float.Parse(s, styles, csharp_format_info), loc);
				}
				catch (OverflowException)
				{
					Report.Error(594, Location, "Floating-point constant is outside the range of type `{0}'", "float");
					return new FloatLiteral(context.BuiltinTypes, 0f, loc);
				}
			default:
				try
				{
					return new DoubleLiteral(context.BuiltinTypes, double.Parse(s, styles, csharp_format_info), loc);
				}
				catch (OverflowException)
				{
					Report.Error(594, loc, "Floating-point constant is outside the range of type `{0}'", "double");
					return new DoubleLiteral(context.BuiltinTypes, 0.0, loc);
				}
			}
		}

		private ILiteralConstant handle_hex(Location loc)
		{
			get_char();
			int num;
			while ((num = peek_char()) != -1 && is_hex(num))
			{
				number_builder[number_pos++] = (char)num;
				get_char();
			}
			string s = new string(number_builder, 0, number_pos);
			try
			{
				ulong ul = ((number_pos > 8) ? ulong.Parse(s, NumberStyles.HexNumber) : uint.Parse(s, NumberStyles.HexNumber));
				return integer_type_suffix(ul, peek_char(), loc);
			}
			catch (OverflowException)
			{
				Error_NumericConstantTooLong();
				return new IntLiteral(context.BuiltinTypes, 0, loc);
			}
			catch (FormatException)
			{
				Report.Error(1013, Location, "Invalid number");
				return new IntLiteral(context.BuiltinTypes, 0, loc);
			}
		}

		private int is_number(int c, bool dotLead)
		{
			number_pos = 0;
			Location location = Location;
			ILiteralConstant literalConstant;
			if (!dotLead)
			{
				if (c == 48)
				{
					int num = peek_char();
					if (num == 120 || num == 88)
					{
						literalConstant = (ILiteralConstant)(val = handle_hex(location));
						return 421;
					}
				}
				decimal_digits(c);
				c = peek_char();
			}
			bool flag = false;
			if (c == 46)
			{
				if (!dotLead)
				{
					get_char();
				}
				if (!decimal_digits(46))
				{
					putback(46);
					number_pos--;
					literalConstant = (ILiteralConstant)(val = adjust_int(-1, location));
					return 421;
				}
				flag = true;
				c = peek_char();
			}
			if (c == 101 || c == 69)
			{
				flag = true;
				get_char();
				if (number_pos == 512)
				{
					Error_NumericConstantTooLong();
				}
				number_builder[number_pos++] = (char)c;
				c = get_char();
				switch (c)
				{
				case 43:
					if (number_pos == 512)
					{
						Error_NumericConstantTooLong();
					}
					number_builder[number_pos++] = '+';
					c = -1;
					break;
				case 45:
					if (number_pos == 512)
					{
						Error_NumericConstantTooLong();
					}
					number_builder[number_pos++] = '-';
					c = -1;
					break;
				default:
					if (number_pos == 512)
					{
						Error_NumericConstantTooLong();
					}
					number_builder[number_pos++] = '+';
					break;
				}
				decimal_digits(c);
				c = peek_char();
			}
			TypeCode typeCode = real_type_suffix(c);
			if (typeCode == TypeCode.Empty && !flag)
			{
				literalConstant = adjust_int(c, location);
			}
			else
			{
				flag = true;
				if (typeCode != TypeCode.Empty)
				{
					get_char();
				}
				literalConstant = adjust_real(typeCode, location);
			}
			val = literalConstant;
			return 421;
		}

		private int getHex(int count, out int surrogate, out bool error)
		{
			int num = 0;
			int num2 = ((count != -1) ? count : 4);
			get_char();
			error = false;
			surrogate = 0;
			for (int i = 0; i < num2; i++)
			{
				int num3 = get_char();
				if (num3 >= 48 && num3 <= 57)
				{
					num3 -= 48;
				}
				else if (num3 >= 65 && num3 <= 70)
				{
					num3 = num3 - 65 + 10;
				}
				else
				{
					if (num3 < 97 || num3 > 102)
					{
						error = true;
						return 0;
					}
					num3 = num3 - 97 + 10;
				}
				num = num * 16 + num3;
				if (count == -1)
				{
					int num4 = peek_char();
					if (num4 == -1 || !is_hex((ushort)num4))
					{
						break;
					}
				}
			}
			if (num2 == 8)
			{
				if (num > 1114111)
				{
					error = true;
					return 0;
				}
				if (num >= 65536)
				{
					surrogate = (num - 65536) % 1024 + 56320;
					num = (num - 65536) / 1024 + 55296;
				}
			}
			return num;
		}

		private int escape(int c, out int surrogate)
		{
			int num = peek_char();
			if (c != 92)
			{
				surrogate = 0;
				return c;
			}
			int hex;
			switch (num)
			{
			case 97:
				hex = 7;
				break;
			case 98:
				hex = 8;
				break;
			case 110:
				hex = 10;
				break;
			case 116:
				hex = 9;
				break;
			case 118:
				hex = 11;
				break;
			case 114:
				hex = 13;
				break;
			case 92:
				hex = 92;
				break;
			case 102:
				hex = 12;
				break;
			case 48:
				hex = 0;
				break;
			case 34:
				hex = 34;
				break;
			case 39:
				hex = 39;
				break;
			case 120:
			{
				bool error;
				hex = getHex(-1, out surrogate, out error);
				if (!error)
				{
					return hex;
				}
				goto default;
			}
			case 85:
			case 117:
				return EscapeUnicode(num, out surrogate);
			default:
				surrogate = 0;
				Report.Error(1009, Location, "Unrecognized escape sequence `\\{0}'", ((char)num).ToString());
				return num;
			}
			get_char();
			surrogate = 0;
			return hex;
		}

		private int EscapeUnicode(int ch, out int surrogate)
		{
			bool error;
			ch = ((ch != 85) ? getHex(4, out surrogate, out error) : getHex(8, out surrogate, out error));
			if (error)
			{
				Report.Error(1009, Location, "Unrecognized escape sequence");
			}
			return ch;
		}

		private int get_char()
		{
			int num;
			if (putback_char != -1)
			{
				num = putback_char;
				putback_char = -1;
			}
			else
			{
				num = reader.Read();
			}
			if (num <= 13)
			{
				switch (num)
				{
				case 13:
					if (peek_char() == 10)
					{
						putback_char = -1;
					}
					num = 10;
					advance_line();
					break;
				case 10:
					advance_line();
					break;
				default:
					col++;
					break;
				}
			}
			else if (num >= 8232 && num <= 8233)
			{
				advance_line();
			}
			else
			{
				col++;
			}
			return num;
		}

		private void advance_line()
		{
			line++;
			ref_line++;
			previous_col = col;
			col = 0;
		}

		private int peek_char()
		{
			if (putback_char == -1)
			{
				putback_char = reader.Read();
			}
			return putback_char;
		}

		private int peek_char2()
		{
			if (putback_char != -1)
			{
				return putback_char;
			}
			return reader.Peek();
		}

		public void putback(int c)
		{
			if (putback_char != -1)
			{
				throw new InternalErrorException($"Secondary putback [{(char)putback_char}] putting back [{(char)c}] is not allowed", Location);
			}
			if (c == 10 || col == 0 || (c >= 8232 && c <= 8233))
			{
				line--;
				ref_line--;
				col = previous_col;
			}
			else
			{
				col--;
			}
			putback_char = c;
		}

		public bool advance()
		{
			if (peek_char() == -1)
			{
				return CompleteOnEOF;
			}
			return true;
		}

		public object value()
		{
			return val;
		}

		public int token()
		{
			current_token = xtoken();
			return current_token;
		}

		private int TokenizePreprocessorKeyword(out int c)
		{
			do
			{
				c = get_char();
			}
			while (c == 32 || c == 9);
			int result = 0;
			while (c != -1 && c >= 97 && c <= 122)
			{
				id_builder[result++] = (char)c;
				c = get_char();
				if (c != 92)
				{
					continue;
				}
				int num = peek_char();
				if (num != 85 && num != 117)
				{
					continue;
				}
				int surrogate;
				c = EscapeUnicode(c, out surrogate);
				if (surrogate != 0)
				{
					if (is_identifier_part_character((char)c))
					{
						id_builder[result++] = (char)c;
					}
					c = surrogate;
				}
			}
			return result;
		}

		private PreprocessorDirective get_cmd_arg(out string arg)
		{
			tokens_seen = false;
			arg = "";
			int c;
			PreprocessorDirective preprocessorDirective = GetPreprocessorDirective(id_builder, TokenizePreprocessorKeyword(out c));
			if ((preprocessorDirective & PreprocessorDirective.CustomArgumentsParsing) != PreprocessorDirective.Invalid)
			{
				return preprocessorDirective;
			}
			while (c == 32 || c == 9)
			{
				c = get_char();
			}
			int num = (int)(preprocessorDirective & PreprocessorDirective.RequiresArgument);
			int num2 = 0;
			while (c != -1 && c != 10 && c != 8232 && c != 8233)
			{
				if (c == 92 && num >= 0)
				{
					if (num != 0)
					{
						num = 1;
						int num3 = peek_char();
						if (num3 == 85 || num3 == 117)
						{
							int surrogate;
							c = EscapeUnicode(c, out surrogate);
							if (surrogate != 0)
							{
								if (is_identifier_part_character((char)c))
								{
									if (num2 == value_builder.Length)
									{
										Array.Resize(ref value_builder, num2 * 2);
									}
									value_builder[num2++] = (char)c;
								}
								c = surrogate;
							}
						}
					}
					else
					{
						num = -1;
					}
				}
				else if (c == 47 && peek_char() == 47)
				{
					get_char();
					ReadToEndOfLine();
					break;
				}
				if (num2 == value_builder.Length)
				{
					Array.Resize(ref value_builder, num2 * 2);
				}
				value_builder[num2++] = (char)c;
				c = get_char();
			}
			if (num2 != 0)
			{
				if (num2 > 512)
				{
					arg = new string(value_builder, 0, num2);
				}
				else
				{
					arg = InternIdentifier(value_builder, num2);
				}
				arg = arg.Trim(simple_whitespaces);
			}
			return preprocessorDirective;
		}

		private bool PreProcessLine()
		{
			Location location = Location;
			int c;
			int num = TokenizePreprocessorKeyword(out c);
			if (num == line_default.Length)
			{
				if (!IsTokenIdentifierEqual(line_default))
				{
					return false;
				}
				current_source = source_file.SourceFile;
				if (!hidden_block_start.IsNull)
				{
					current_source.RegisterHiddenScope(hidden_block_start, location);
					hidden_block_start = Location.Null;
				}
				ref_line = line;
				return true;
			}
			if (num == line_hidden.Length)
			{
				if (!IsTokenIdentifierEqual(line_hidden))
				{
					return false;
				}
				if (hidden_block_start.IsNull)
				{
					hidden_block_start = location;
				}
				return true;
			}
			if (num != 0 || c < 48 || c > 57)
			{
				ReadToEndOfLine();
				return false;
			}
			int num2 = TokenizeNumber(c);
			if (num2 < 1)
			{
				ReadToEndOfLine();
				return num2 != 0;
			}
			c = get_char();
			if (c == 32)
			{
				do
				{
					c = get_char();
				}
				while (c == 32 || c == 9);
			}
			else if (c == 34)
			{
				c = 0;
			}
			if (c != 10 && c != 47 && c != 34 && c != 8232 && c != 8233)
			{
				ReadToEndOfLine();
				Report.Error(1578, location, "Filename, single-line comment or end-of-line expected");
				return true;
			}
			string text = null;
			if (c == 34)
			{
				text = TokenizeFileName(ref c);
				while (c == 32 || c == 9)
				{
					c = get_char();
				}
			}
			switch (c)
			{
			case 47:
				ReadSingleLineComment();
				break;
			default:
				ReadToEndOfLine();
				Error_EndLineExpected();
				return true;
			case 10:
			case 8232:
			case 8233:
				break;
			}
			if (text != null)
			{
				current_source = context.LookupFile(source_file, text);
				source_file.AddIncludeFile(current_source);
			}
			if (!hidden_block_start.IsNull)
			{
				current_source.RegisterHiddenScope(hidden_block_start, location);
				hidden_block_start = Location.Null;
			}
			ref_line = num2;
			return true;
		}

		private void PreProcessDefinition(bool is_define, string ident, bool caller_is_taking)
		{
			if (ident.Length == 0 || ident == "true" || ident == "false")
			{
				Report.Error(1001, Location, "Missing identifier to pre-processor directive");
				return;
			}
			if (ident.IndexOfAny(simple_whitespaces) != -1)
			{
				Error_EndLineExpected();
				return;
			}
			if (!is_identifier_start_character(ident[0]))
			{
				Report.Error(1001, Location, "Identifier expected: {0}", ident);
			}
			string text = ident.Substring(1);
			for (int i = 0; i < text.Length; i++)
			{
				if (!is_identifier_part_character(text[i]))
				{
					Report.Error(1001, Location, "Identifier expected: {0}", ident);
					return;
				}
			}
			if (!caller_is_taking)
			{
				return;
			}
			if (is_define)
			{
				if (!context.Settings.IsConditionalSymbolDefined(ident))
				{
					source_file.AddDefine(ident);
				}
			}
			else
			{
				source_file.AddUndefine(ident);
			}
		}

		private byte read_hex(out bool error)
		{
			int num = get_char();
			int num2;
			if (num >= 48 && num <= 57)
			{
				num2 = num - 48;
			}
			else if (num >= 65 && num <= 70)
			{
				num2 = num - 65 + 10;
			}
			else
			{
				if (num < 97 || num > 102)
				{
					error = true;
					return 0;
				}
				num2 = num - 97 + 10;
			}
			num2 *= 16;
			num = get_char();
			if (num >= 48 && num <= 57)
			{
				num2 += num - 48;
			}
			else if (num >= 65 && num <= 70)
			{
				num2 += num - 65 + 10;
			}
			else
			{
				if (num < 97 || num > 102)
				{
					error = true;
					return 0;
				}
				num2 += num - 97 + 10;
			}
			error = false;
			return (byte)num2;
		}

		private bool ParsePragmaChecksum()
		{
			int c = get_char();
			if (c != 34)
			{
				return false;
			}
			string name = TokenizeFileName(ref c);
			if (c != 32)
			{
				return false;
			}
			SourceFile sourceFile = context.LookupFile(source_file, name);
			if (get_char() != 34 || get_char() != 123)
			{
				return false;
			}
			byte[] array = new byte[16];
			int i;
			bool error;
			for (i = 0; i < 4; i++)
			{
				array[i] = read_hex(out error);
				if (error)
				{
					return false;
				}
			}
			if (get_char() != 45)
			{
				return false;
			}
			for (; i < 10; i++)
			{
				array[i] = read_hex(out error);
				if (error)
				{
					return false;
				}
				array[i++] = read_hex(out error);
				if (error)
				{
					return false;
				}
				if (get_char() != 45)
				{
					return false;
				}
			}
			for (; i < 16; i++)
			{
				array[i] = read_hex(out error);
				if (error)
				{
					return false;
				}
			}
			if (get_char() != 125 || get_char() != 34)
			{
				return false;
			}
			c = get_char();
			if (c != 32)
			{
				return false;
			}
			if (get_char() != 34)
			{
				return false;
			}
			List<byte> list = new List<byte>(16);
			Location location = Location;
			c = peek_char();
			while (c != 34 && c != -1)
			{
				list.Add(read_hex(out error));
				if (error)
				{
					return false;
				}
				c = peek_char();
			}
			if (c == 47)
			{
				ReadSingleLineComment();
			}
			else if (get_char() != 34)
			{
				return false;
			}
			if (context.Settings.GenerateDebugInfo)
			{
				byte[] array2 = list.ToArray();
				if (sourceFile.HasChecksum && !ArrayComparer.IsEqual(sourceFile.Checksum, array2))
				{
					Report.Warning(1697, 1, location, "Different checksum values specified for file `{0}'", sourceFile.Name);
				}
				sourceFile.SetChecksum(array, array2);
				current_source.AutoGenerated = true;
			}
			return true;
		}

		private bool IsTokenIdentifierEqual(char[] identifier)
		{
			for (int i = 0; i < identifier.Length; i++)
			{
				if (identifier[i] != id_builder[i])
				{
					return false;
				}
			}
			return true;
		}

		private bool ScanClosingInterpolationBrace()
		{
			PushPosition();
			bool? flag = null;
			int num = 0;
			do
			{
				switch (reader.Read())
				{
				case 34:
					num++;
					break;
				case -1:
					flag = false;
					break;
				case 125:
					if (num % 2 == 1)
					{
						flag = true;
					}
					break;
				}
			}
			while (!flag.HasValue);
			PopPosition();
			return flag.Value;
		}

		private int TokenizeNumber(int value)
		{
			number_pos = 0;
			decimal_digits(value);
			uint num = (uint)(number_builder[0] - 48);
			try
			{
				for (int i = 1; i < number_pos; i++)
				{
					num = checked(num * 10 + (uint)(number_builder[i] - 48));
				}
				return (int)num;
			}
			catch (OverflowException)
			{
				Error_NumericConstantTooLong();
				return -1;
			}
		}

		private string TokenizeFileName(ref int c)
		{
			StringBuilder stringBuilder = new StringBuilder();
			while (c != -1 && c != 10 && c != 8232 && c != 8233)
			{
				c = get_char();
				if (c == 34)
				{
					c = get_char();
					break;
				}
				stringBuilder.Append((char)c);
			}
			if (stringBuilder.Length == 0)
			{
				Report.Warning(1709, 1, Location, "Filename specified for preprocessor directive is empty");
			}
			return stringBuilder.ToString();
		}

		private int TokenizePragmaWarningIdentifier(ref int c, ref bool identifier)
		{
			if ((c >= 48 && c <= 57) || is_identifier_start_character(c))
			{
				int num;
				if (c >= 48 && c <= 57)
				{
					number_pos = 0;
					num = TokenizeNumber(c);
					c = get_char();
					if (c != 32 && c != 9 && c != 44 && c != 10 && c != -1 && c != 8232 && c != 8233)
					{
						return ReadPragmaWarningComment(c);
					}
				}
				else
				{
					int num2 = 0;
					num = -1;
					id_builder[num2++] = (char)c;
					while (c < 512)
					{
						c = reader.Read();
						id_builder[num2] = (char)c;
						if (c >= 48 && c <= 57)
						{
							if (num2 == 6 && id_builder[0] == 'C' && id_builder[1] == 'S')
							{
								num = 0;
								int num3 = 1000;
								for (int i = 0; i < 4; i++)
								{
									char c2 = id_builder[i + 2];
									if (c2 < '0' || c2 > '9')
									{
										num = -1;
										break;
									}
									num += (c2 - 48) * num3;
									num3 /= 10;
								}
							}
						}
						else if ((c < 97 || c > 122) && (c < 65 || c > 90) && c != 95)
						{
							break;
						}
						num2++;
					}
					if (num < 0)
					{
						identifier = true;
						num = num2;
					}
				}
				while (c == 32 || c == 9)
				{
					c = get_char();
				}
				if (c == 44)
				{
					c = get_char();
				}
				while (c == 32 || c == 9)
				{
					c = get_char();
				}
				return num;
			}
			return ReadPragmaWarningComment(c);
		}

		private int ReadPragmaWarningComment(int c)
		{
			if (c == 47)
			{
				ReadSingleLineComment();
			}
			else
			{
				Report.Warning(1692, 1, Location, "Invalid number");
				ReadToEndOfLine();
			}
			return -1;
		}

		private void ReadToEndOfLine()
		{
			int num;
			do
			{
				num = get_char();
			}
			while (num != -1 && num != 10 && num != 8232 && num != 8233);
		}

		private void ReadSingleLineComment()
		{
			if (peek_char() != 47)
			{
				Report.Warning(1696, 1, Location, "Single-line comment or end-of-line expected");
			}
			ReadToEndOfLine();
		}

		private void ParsePragmaDirective()
		{
			int c;
			int num = TokenizePreprocessorKeyword(out c);
			if (num == pragma_warning.Length && IsTokenIdentifierEqual(pragma_warning))
			{
				num = TokenizePreprocessorKeyword(out c);
				if (num == pragma_warning_disable.Length)
				{
					bool flag = IsTokenIdentifierEqual(pragma_warning_disable);
					if (flag || IsTokenIdentifierEqual(pragma_warning_restore))
					{
						while (c == 32 || c == 9)
						{
							c = get_char();
						}
						Location location = Location;
						if (c == 10 || c == 47 || c == 8232 || c == 8233)
						{
							if (c == 47)
							{
								ReadSingleLineComment();
							}
							if (flag)
							{
								Report.RegisterWarningRegion(location).WarningDisable(location.Row);
							}
							else
							{
								Report.RegisterWarningRegion(location).WarningEnable(location.Row);
							}
							return;
						}
						int num2;
						do
						{
							bool identifier = false;
							num2 = TokenizePragmaWarningIdentifier(ref c, ref identifier);
							if (num2 > 0 && !identifier)
							{
								if (flag)
								{
									Report.RegisterWarningRegion(location).WarningDisable(location, num2, context.Report);
								}
								else
								{
									Report.RegisterWarningRegion(location).WarningEnable(location, num2, context);
								}
							}
						}
						while (num2 >= 0 && c != 10 && c != -1 && c != 8232 && c != 8233);
						return;
					}
				}
				Report.Warning(1634, 1, Location, "Expected disable or restore");
				ReadToEndOfLine();
			}
			else if (num == pragma_checksum.Length && IsTokenIdentifierEqual(pragma_checksum))
			{
				if (c != 32 || !ParsePragmaChecksum())
				{
					Report.Warning(1695, 1, Location, "Invalid #pragma checksum syntax. Expected \"filename\" \"{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}\" \"XXXX...\"");
				}
			}
			else
			{
				Report.Warning(1633, 1, Location, "Unrecognized #pragma directive");
				ReadToEndOfLine();
			}
		}

		private bool eval_val(string s)
		{
			if (s == "true")
			{
				return true;
			}
			if (s == "false")
			{
				return false;
			}
			return source_file.IsConditionalDefined(s);
		}

		private bool pp_primary(ref string s)
		{
			s = s.Trim();
			int length = s.Length;
			if (length > 0)
			{
				char c = s[0];
				if (c == '(')
				{
					s = s.Substring(1);
					bool result = pp_expr(ref s, false);
					if (s.Length > 0 && s[0] == ')')
					{
						s = s.Substring(1);
						return result;
					}
					Error_InvalidDirective();
					return false;
				}
				if (is_identifier_start_character(c))
				{
					for (int i = 1; i < length; i++)
					{
						c = s[i];
						if (!is_identifier_part_character(c))
						{
							bool result2 = eval_val(s.Substring(0, i));
							s = s.Substring(i);
							return result2;
						}
					}
					bool result3 = eval_val(s);
					s = "";
					return result3;
				}
			}
			Error_InvalidDirective();
			return false;
		}

		private bool pp_unary(ref string s)
		{
			s = s.Trim();
			int length = s.Length;
			if (length > 0)
			{
				if (s[0] == '!')
				{
					if (length > 1 && s[1] == '=')
					{
						Error_InvalidDirective();
						return false;
					}
					s = s.Substring(1);
					return !pp_primary(ref s);
				}
				return pp_primary(ref s);
			}
			Error_InvalidDirective();
			return false;
		}

		private bool pp_eq(ref string s)
		{
			bool flag = pp_unary(ref s);
			s = s.Trim();
			int length = s.Length;
			if (length > 0)
			{
				if (s[0] == '=')
				{
					if (length > 2 && s[1] == '=')
					{
						s = s.Substring(2);
						return flag == pp_unary(ref s);
					}
					Error_InvalidDirective();
					return false;
				}
				if (s[0] == '!' && length > 1 && s[1] == '=')
				{
					s = s.Substring(2);
					return flag != pp_unary(ref s);
				}
			}
			return flag;
		}

		private bool pp_and(ref string s)
		{
			bool flag = pp_eq(ref s);
			s = s.Trim();
			int length = s.Length;
			if (length > 0 && s[0] == '&')
			{
				if (length > 2 && s[1] == '&')
				{
					s = s.Substring(2);
					return flag & pp_and(ref s);
				}
				Error_InvalidDirective();
				return false;
			}
			return flag;
		}

		private bool pp_expr(ref string s, bool isTerm)
		{
			bool flag = pp_and(ref s);
			s = s.Trim();
			int length = s.Length;
			if (length > 0)
			{
				if (s[0] == '|')
				{
					if (length > 2 && s[1] == '|')
					{
						s = s.Substring(2);
						return flag | pp_expr(ref s, isTerm);
					}
					Error_InvalidDirective();
					return false;
				}
				if (isTerm)
				{
					Error_EndLineExpected();
					return false;
				}
			}
			return flag;
		}

		private bool eval(string s)
		{
			bool result = pp_expr(ref s, true);
			s = s.Trim();
			if (s.Length != 0)
			{
				return false;
			}
			return result;
		}

		private void Error_NumericConstantTooLong()
		{
			Report.Error(1021, Location, "Integral constant is too large");
		}

		private void Error_InvalidDirective()
		{
			Report.Error(1517, Location, "Invalid preprocessor directive");
		}

		private void Error_UnexpectedDirective(string extra)
		{
			Report.Error(1028, Location, "Unexpected processor directive ({0})", extra);
		}

		private void Error_TokensSeen()
		{
			Report.Error(1032, Location, "Cannot define or undefine preprocessor symbols after first token in file");
		}

		private void Eror_WrongPreprocessorLocation()
		{
			Report.Error(1040, Location, "Preprocessor directives must appear as the first non-whitespace character on a line");
		}

		private void Error_EndLineExpected()
		{
			Report.Error(1025, Location, "Single-line comment or end-of-line expected");
		}

		private void WarningMisplacedComment(Location loc)
		{
			if (doc_state != XmlCommentState.Error)
			{
				doc_state = XmlCommentState.Error;
				Report.Warning(1587, 2, loc, "XML comment is not placed on a valid language element");
			}
		}

		private bool ParsePreprocessingDirective(bool caller_is_taking)
		{
			bool flag = false;
			string arg;
			PreprocessorDirective preprocessorDirective = get_cmd_arg(out arg);
			switch (preprocessorDirective)
			{
			case PreprocessorDirective.Region:
				flag = true;
				arg = "true";
				goto case PreprocessorDirective.If;
			case PreprocessorDirective.Endregion:
				if (ifstack == null || ifstack.Count == 0)
				{
					Error_UnexpectedDirective("no #region for this #endregion");
					return true;
				}
				if ((ifstack.Pop() & 0x10) == 0)
				{
					Report.Error(1027, Location, "Expected `#endif' directive");
				}
				return caller_is_taking;
			case PreprocessorDirective.If:
			{
				if (ifstack == null)
				{
					ifstack = new Stack<int>(2);
				}
				int num3 = (flag ? 16 : 0);
				if (ifstack.Count == 0)
				{
					num3 |= 8;
				}
				else if ((ifstack.Peek() & 1) != 0)
				{
					num3 |= 8;
				}
				if (eval(arg) & caller_is_taking)
				{
					ifstack.Push(num3 | 1);
					return true;
				}
				ifstack.Push(num3);
				return false;
			}
			case PreprocessorDirective.Endif:
				if (ifstack == null || ifstack.Count == 0)
				{
					Error_UnexpectedDirective("no #if for this #endif");
					return true;
				}
				if ((ifstack.Pop() & 0x10) != 0)
				{
					Report.Error(1038, Location, "#endregion directive expected");
				}
				if (arg.Length != 0)
				{
					Error_EndLineExpected();
				}
				if (ifstack.Count == 0)
				{
					return true;
				}
				return (ifstack.Peek() & 1) != 0;
			case PreprocessorDirective.Elif:
			{
				if (ifstack == null || ifstack.Count == 0)
				{
					Error_UnexpectedDirective("no #if for this #elif");
					return true;
				}
				int num = ifstack.Pop();
				if ((num & 0x10) != 0)
				{
					Report.Error(1038, Location, "#endregion directive expected");
					return true;
				}
				if ((num & 4) != 0)
				{
					Error_UnexpectedDirective("#elif not valid after #else");
					return true;
				}
				if ((num & 1) != 0)
				{
					ifstack.Push(0);
					return false;
				}
				if (eval(arg) && (num & 8) != 0)
				{
					ifstack.Push(num | 1);
					return true;
				}
				ifstack.Push(num);
				return false;
			}
			case PreprocessorDirective.Else:
			{
				if (ifstack == null || ifstack.Count == 0)
				{
					Error_UnexpectedDirective("no #if for this #else");
					return true;
				}
				int num2 = ifstack.Peek();
				if ((num2 & 0x10) != 0)
				{
					Report.Error(1038, Location, "#endregion directive expected");
					return true;
				}
				if ((num2 & 4) != 0)
				{
					Error_UnexpectedDirective("#else within #else");
					return true;
				}
				ifstack.Pop();
				if (arg.Length != 0)
				{
					Error_EndLineExpected();
					return true;
				}
				bool flag2 = false;
				if ((num2 & 8) != 0)
				{
					flag2 = (num2 & 1) == 0;
					num2 = ((!flag2) ? (num2 & -2) : (num2 | 1));
				}
				ifstack.Push(num2 | 4);
				return flag2;
			}
			case PreprocessorDirective.Define:
				if (any_token_seen)
				{
					if (caller_is_taking)
					{
						Error_TokensSeen();
					}
					return caller_is_taking;
				}
				PreProcessDefinition(true, arg, caller_is_taking);
				return caller_is_taking;
			case PreprocessorDirective.Undef:
				if (any_token_seen)
				{
					if (caller_is_taking)
					{
						Error_TokensSeen();
					}
					return caller_is_taking;
				}
				PreProcessDefinition(false, arg, caller_is_taking);
				return caller_is_taking;
			case PreprocessorDirective.Invalid:
				Report.Error(1024, Location, "Wrong preprocessor directive");
				return true;
			default:
				if (!caller_is_taking)
				{
					return false;
				}
				switch (preprocessorDirective)
				{
				case PreprocessorDirective.Error:
					Report.Error(1029, Location, "#error: '{0}'", arg);
					return true;
				case PreprocessorDirective.Warning:
					Report.Warning(1030, 1, Location, "#warning: `{0}'", arg);
					return true;
				case PreprocessorDirective.Pragma:
					if (context.Settings.Version == LanguageVersion.ISO_1)
					{
						Report.FeatureIsNotAvailable(context, Location, "#pragma");
					}
					ParsePragmaDirective();
					return true;
				case PreprocessorDirective.Line:
				{
					Location location = Location;
					if (!PreProcessLine())
					{
						Report.Error(1576, location, "The line number specified for #line directive is missing or invalid");
					}
					return caller_is_taking;
				}
				default:
					throw new NotImplementedException(preprocessorDirective.ToString());
				}
			}
		}

		private int consume_string(bool quoted)
		{
			int num = 0;
			Location location = Location;
			if (quoted)
			{
				location -= 1;
			}
			while (true)
			{
				int num2;
				if (putback_char != -1)
				{
					num2 = putback_char;
					putback_char = -1;
				}
				else
				{
					num2 = reader.Read();
				}
				if (num2 == 34)
				{
					col++;
					if (quoted && peek_char() == 34)
					{
						if (num == value_builder.Length)
						{
							Array.Resize(ref value_builder, num * 2);
						}
						value_builder[num++] = (char)num2;
						get_char();
						continue;
					}
					ILiteralConstant literalConstant = new StringLiteral(context.BuiltinTypes, CreateStringFromBuilder(num), location);
					val = literalConstant;
					return 421;
				}
				if (num2 == 10 || num2 == 8232 || num2 == 8233)
				{
					if (!quoted)
					{
						Report.Error(1010, Location, "Newline in constant");
						advance_line();
						if (num > 1 && value_builder[num - 1] == '\r')
						{
							num--;
						}
						val = new StringLiteral(context.BuiltinTypes, new string(value_builder, 0, num), location);
						return 421;
					}
					advance_line();
				}
				else if (num2 == 92 && !quoted)
				{
					col++;
					int surrogate;
					num2 = escape(num2, out surrogate);
					if (num2 == -1)
					{
						return 259;
					}
					if (surrogate != 0)
					{
						if (num == value_builder.Length)
						{
							Array.Resize(ref value_builder, num * 2);
						}
						value_builder[num++] = (char)num2;
						num2 = surrogate;
					}
				}
				else
				{
					if (num2 == -1)
					{
						break;
					}
					col++;
				}
				if (num == value_builder.Length)
				{
					Array.Resize(ref value_builder, num * 2);
				}
				value_builder[num++] = (char)num2;
			}
			Report.Error(1039, Location, "Unterminated string literal");
			return 257;
		}

		private int consume_identifier(int s)
		{
			int result = consume_identifier(s, false);
			if (doc_state == XmlCommentState.Allowed)
			{
				doc_state = XmlCommentState.NotAllowed;
			}
			return result;
		}

		private int consume_identifier(int c, bool quoted)
		{
			int num = 0;
			int num2 = col;
			if (quoted)
			{
				num2--;
			}
			if (c == 92)
			{
				int surrogate;
				c = escape(c, out surrogate);
				if (!quoted && !is_identifier_start_character(c))
				{
					if (surrogate == 0)
					{
						Report.Error(1056, Location, "Unexpected character `\\{0}'", c.ToString("x4"));
						return 259;
					}
					id_builder[num++] = (char)c;
					c = surrogate;
				}
			}
			id_builder[num++] = (char)c;
			try
			{
				while (true)
				{
					c = reader.Read();
					if ((c < 97 || c > 122) && (c < 65 || c > 90))
					{
						switch (c)
						{
						case 48:
						case 49:
						case 50:
						case 51:
						case 52:
						case 53:
						case 54:
						case 55:
						case 56:
						case 57:
						case 95:
							break;
						default:
							if (c < 128)
							{
								if (c == 92)
								{
									int surrogate2;
									c = escape(c, out surrogate2);
									if (is_identifier_part_character((char)c))
									{
										id_builder[num++] = (char)c;
										continue;
									}
									if (surrogate2 != 0)
									{
										c = surrogate2;
										continue;
									}
									if (c == 65279)
									{
										putback_char = c;
										continue;
									}
									Report.Error(1056, Location, "Unexpected character `\\{0}'", c.ToString("x4"));
									return 259;
								}
							}
							else if (is_identifier_part_character_slow_part((char)c))
							{
								id_builder[num++] = (char)c;
								continue;
							}
							putback_char = c;
							goto end_IL_007c;
						}
					}
					id_builder[num++] = (char)c;
					continue;
					end_IL_007c:
					break;
				}
			}
			catch (IndexOutOfRangeException)
			{
				Report.Error(645, Location, "Identifier too long (limit is 512 chars)");
				num--;
				col += num;
			}
			col += num - 1;
			if (id_builder[0] >= '_' && !quoted)
			{
				int keyword = GetKeyword(id_builder, num);
				if (keyword != -1)
				{
					val = ltb.Create((keyword == 363) ? "await" : null, current_source, ref_line, num2);
					return keyword;
				}
			}
			string text = InternIdentifier(id_builder, num);
			val = ltb.Create(text, current_source, ref_line, num2);
			if (quoted && parsing_attribute_section)
			{
				AddEscapedIdentifier(((LocatedToken)val).Location);
			}
			return 422;
		}

		private string InternIdentifier(char[] charBuffer, int length)
		{
			Dictionary<char[], string> dictionary = identifiers[length];
			string result;
			if (dictionary != null)
			{
				if (dictionary.TryGetValue(charBuffer, out result))
				{
					return result;
				}
			}
			else
			{
				dictionary = new Dictionary<char[], string>((length > 20) ? 10 : 100, new IdentifiersComparer(length));
				identifiers[length] = dictionary;
			}
			char[] array = new char[length];
			Array.Copy(charBuffer, array, length);
			result = new string(charBuffer, 0, length);
			dictionary.Add(array, result);
			return result;
		}

		public int xtoken()
		{
			if (parsing_interpolation_format)
			{
				return TokenizeInterpolationFormat();
			}
			bool flag = false;
			int num;
			while ((num = get_char()) != -1)
			{
				switch (num)
				{
				case 9:
					col = (col - 1 + tab_size) / tab_size * tab_size;
					continue;
				case 92:
					tokens_seen = true;
					return consume_identifier(num);
				case 123:
					val = ltb.Create(current_source, ref_line, col);
					return 371;
				case 125:
					if (parsing_string_interpolation > 0)
					{
						parsing_string_interpolation--;
						bool quoted = parsing_string_interpolation_quoted != null && parsing_string_interpolation_quoted.Count > 0 && parsing_string_interpolation_quoted.Pop();
						return TokenizeInterpolatedString(quoted);
					}
					val = ltb.Create(current_source, ref_line, col);
					return 372;
				case 91:
					if (doc_state == XmlCommentState.Allowed)
					{
						doc_state = XmlCommentState.NotAllowed;
					}
					val = ltb.Create(current_source, ref_line, col);
					if (parsing_block == 0 || lambda_arguments_parsing)
					{
						return 373;
					}
					switch (peek_char())
					{
					case 44:
					case 93:
						return 373;
					case 10:
					case 11:
					case 12:
					case 13:
					case 32:
					case 47:
					case 8232:
					case 8233:
					{
						int num3 = peek_token();
						if (num3 == 378 || num3 == 374)
						{
							return 373;
						}
						return 427;
					}
					default:
						return 427;
					}
				case 93:
					return 374;
				case 40:
					val = ltb.Create(current_source, ref_line, col);
					if (parsing_block != 0 && !lambda_arguments_parsing)
					{
						switch (current_token)
						{
						case 276:
						case 277:
						case 289:
						case 290:
						case 292:
						case 324:
						case 329:
						case 335:
						case 340:
						case 420:
						case 422:
							return 375;
						default:
							switch (peek_char())
							{
							case 34:
							case 39:
							case 40:
							case 48:
							case 49:
								return 375;
							default:
							{
								lambda_arguments_parsing = true;
								PushPosition();
								int num2 = TokenizeOpenParens();
								PopPosition();
								lambda_arguments_parsing = false;
								return num2;
							}
							}
						}
					}
					return 375;
				case 41:
					return 376;
				case 44:
					return 378;
				case 59:
					return 380;
				case 126:
					val = ltb.Create(current_source, ref_line, col);
					return 381;
				case 63:
					val = ltb.Create(current_source, ref_line, col);
					return TokenizePossibleNullableType();
				case 60:
					val = ltb.Create(current_source, ref_line, col);
					if (parsing_generic_less_than++ > 0)
					{
						return 418;
					}
					return TokenizeLessThan();
				case 62:
				{
					val = ltb.Create(current_source, ref_line, col);
					int num2 = peek_char();
					if (num2 == 61)
					{
						get_char();
						return 401;
					}
					if (parsing_generic_less_than > 1 || (parsing_generic_less_than == 1 && num2 != 62))
					{
						parsing_generic_less_than--;
						return 420;
					}
					if (num2 == 62)
					{
						get_char();
						num2 = peek_char();
						if (num2 == 61)
						{
							get_char();
							return 412;
						}
						return 399;
					}
					return 387;
				}
				case 43:
				{
					val = ltb.Create(current_source, ref_line, col);
					int num2;
					switch (peek_char())
					{
					case 43:
						num2 = 396;
						break;
					case 61:
						num2 = 409;
						break;
					default:
						return 382;
					}
					get_char();
					return num2;
				}
				case 45:
				{
					val = ltb.Create(current_source, ref_line, col);
					int num2;
					switch (peek_char())
					{
					case 45:
						num2 = 397;
						break;
					case 61:
						num2 = 410;
						break;
					case 62:
						num2 = 416;
						break;
					default:
						return 383;
					}
					get_char();
					return num2;
				}
				case 33:
					val = ltb.Create(current_source, ref_line, col);
					if (peek_char() == 61)
					{
						get_char();
						return 403;
					}
					return 384;
				case 61:
					val = ltb.Create(current_source, ref_line, col);
					switch (peek_char())
					{
					case 61:
						get_char();
						return 402;
					case 62:
						get_char();
						return 343;
					default:
						return 385;
					}
				case 38:
					val = ltb.Create(current_source, ref_line, col);
					switch (peek_char())
					{
					case 38:
						get_char();
						return 404;
					case 61:
						get_char();
						return 413;
					default:
						return 388;
					}
				case 124:
					val = ltb.Create(current_source, ref_line, col);
					switch (peek_char())
					{
					case 124:
						get_char();
						return 405;
					case 61:
						get_char();
						return 415;
					default:
						return 389;
					}
				case 42:
					val = ltb.Create(current_source, ref_line, col);
					if (peek_char() == 61)
					{
						get_char();
						return 406;
					}
					return 390;
				case 47:
				{
					int num2 = peek_char();
					if (num2 == 61)
					{
						val = ltb.Create(current_source, ref_line, col);
						get_char();
						return 407;
					}
					if (num2 == 47)
					{
						if (parsing_string_interpolation > 0)
						{
							Report.Error(8077, Location, "A single-line comment may not be used in an interpolated string");
							goto case 125;
						}
						get_char();
						if (doc_processing)
						{
							if (peek_char() == 47)
							{
								get_char();
								if ((num2 = peek_char()) != 47)
								{
									if (doc_state == XmlCommentState.Allowed)
									{
										handle_one_line_xml_comment();
									}
									else if (doc_state == XmlCommentState.NotAllowed)
									{
										WarningMisplacedComment(Location - 3);
									}
								}
							}
							else if (xml_comment_buffer.Length > 0)
							{
								doc_state = XmlCommentState.NotAllowed;
							}
						}
						ReadToEndOfLine();
						any_token_seen |= tokens_seen;
						tokens_seen = false;
						flag = false;
						continue;
					}
					if (num2 == 42)
					{
						get_char();
						bool flag2 = false;
						if (doc_processing && peek_char() == 42)
						{
							get_char();
							if (peek_char() == 47)
							{
								get_char();
								continue;
							}
							if (doc_state == XmlCommentState.Allowed)
							{
								flag2 = true;
							}
							else if (doc_state == XmlCommentState.NotAllowed)
							{
								WarningMisplacedComment(Location - 2);
							}
						}
						int current_comment_start = 0;
						if (flag2)
						{
							current_comment_start = xml_comment_buffer.Length;
							xml_comment_buffer.Append(Environment.NewLine);
						}
						while ((num2 = get_char()) != -1)
						{
							if (num2 == 42 && peek_char() == 47)
							{
								get_char();
								flag = true;
								break;
							}
							if (flag2)
							{
								xml_comment_buffer.Append((char)num2);
							}
							if (num2 == 10 || num2 == 8232 || num2 == 8233)
							{
								any_token_seen |= tokens_seen;
								tokens_seen = false;
								flag = false;
							}
						}
						if (!flag)
						{
							Report.Error(1035, Location, "End-of-file found, '*/' expected");
						}
						if (flag2)
						{
							update_formatted_doc_comment(current_comment_start);
						}
						continue;
					}
					val = ltb.Create(current_source, ref_line, col);
					return 392;
				}
				case 37:
					val = ltb.Create(current_source, ref_line, col);
					if (peek_char() == 61)
					{
						get_char();
						return 408;
					}
					return 391;
				case 94:
					val = ltb.Create(current_source, ref_line, col);
					if (peek_char() == 61)
					{
						get_char();
						return 414;
					}
					return 393;
				case 58:
					val = ltb.Create(current_source, ref_line, col);
					if (peek_char() == 58)
					{
						get_char();
						return 395;
					}
					return 379;
				case 48:
				case 49:
				case 50:
				case 51:
				case 52:
				case 53:
				case 54:
				case 55:
				case 56:
				case 57:
					tokens_seen = true;
					return is_number(num, false);
				case 10:
				case 8232:
				case 8233:
					any_token_seen |= tokens_seen;
					tokens_seen = false;
					flag = false;
					continue;
				case 46:
				{
					tokens_seen = true;
					int num2 = peek_char();
					if (num2 >= 48 && num2 <= 57)
					{
						return is_number(num, true);
					}
					return 377;
				}
				case 35:
				{
					if (tokens_seen | flag)
					{
						Eror_WrongPreprocessorLocation();
						return 259;
					}
					if (ParsePreprocessingDirective(true))
					{
						continue;
					}
					bool flag3 = false;
					while ((num = get_char()) != -1)
					{
						if (col == 1)
						{
							flag3 = true;
						}
						else if (!flag3)
						{
							continue;
						}
						if (num != 32 && num != 9 && num != 10 && num != 12 && num != 11 && num != 8232 && num != 8233)
						{
							if (num == 35 && ParsePreprocessingDirective(false))
							{
								break;
							}
							flag3 = false;
						}
					}
					if (num != -1)
					{
						tokens_seen = false;
						continue;
					}
					return 257;
				}
				case 34:
					if (parsing_string_interpolation > 0 && !ScanClosingInterpolationBrace())
					{
						parsing_string_interpolation = 0;
						Report.Error(8076, Location, "Missing close delimiter `}' for interpolated expression");
						val = new StringLiteral(context.BuiltinTypes, "", Location);
						return 367;
					}
					return consume_string(false);
				case 39:
					return TokenizeBackslash();
				case 64:
					num = get_char();
					if (num == 34)
					{
						tokens_seen = true;
						return consume_string(true);
					}
					if (is_identifier_start_character(num))
					{
						return consume_identifier(num, true);
					}
					Report.Error(1646, Location, "Keyword, identifier, or string expected after verbatim specifier: @");
					return 259;
				case 36:
					switch (peek_char())
					{
					case 34:
						get_char();
						return TokenizeInterpolatedString(false);
					case 64:
						get_char();
						if (peek_char() == 34)
						{
							get_char();
							return TokenizeInterpolatedString(true);
						}
						break;
					}
					break;
				case 1048576:
					return 428;
				case 1048577:
					return 429;
				case 1048578:
					return 430;
				case 1048579:
					return 431;
				case 0:
				case 11:
				case 12:
				case 32:
				case 160:
				case 65279:
					continue;
				}
				if (is_identifier_start_character(num))
				{
					tokens_seen = true;
					return consume_identifier(num);
				}
				if (!char.IsWhiteSpace((char)num))
				{
					Report.Error(1056, Location, "Unexpected character `{0}'", ((char)num).ToString());
				}
			}
			if (CompleteOnEOF)
			{
				if (generated)
				{
					return 433;
				}
				generated = true;
				return 432;
			}
			return 257;
		}

		private int TokenizeBackslash()
		{
			Location location = Location;
			int num = get_char();
			tokens_seen = true;
			switch (num)
			{
			case 39:
				val = new CharLiteral(context.BuiltinTypes, (char)num, location);
				Report.Error(1011, location, "Empty character literal");
				return 421;
			case 10:
			case 8232:
			case 8233:
				Report.Error(1010, location, "Newline in constant");
				return 259;
			default:
			{
				int surrogate;
				num = escape(num, out surrogate);
				if (num == -1)
				{
					return 259;
				}
				if (surrogate != 0)
				{
					throw new NotImplementedException();
				}
				ILiteralConstant literalConstant = new CharLiteral(context.BuiltinTypes, (char)num, location);
				val = literalConstant;
				num = get_char();
				if (num != 39)
				{
					Report.Error(1012, location, "Too many characters in character literal");
					while ((num = get_char()) != -1 && num != 10 && num != 39 && num != 8232 && num != 8233)
					{
					}
				}
				return 421;
			}
			}
		}

		private int TokenizeLessThan()
		{
			PushPosition();
			int genericDimension = 0;
			if (parse_less_than(ref genericDimension))
			{
				int result;
				if (parsing_generic_declaration && (parsing_generic_declaration_doc || token() != 377))
				{
					result = 419;
				}
				else
				{
					if (genericDimension > 0)
					{
						val = genericDimension;
						DiscardPosition();
						return 425;
					}
					result = 418;
				}
				PopPosition();
				return result;
			}
			PopPosition();
			parsing_generic_less_than = 0;
			switch (peek_char())
			{
			case 60:
			{
				get_char();
				int result = peek_char();
				if (result == 61)
				{
					get_char();
					return 411;
				}
				return 398;
			}
			case 61:
				get_char();
				return 400;
			default:
				return 386;
			}
		}

		private int TokenizeInterpolatedString(bool quoted)
		{
			int num = 0;
			Location location = Location;
			while (true)
			{
				int num2 = get_char();
				switch (num2)
				{
				case 34:
					if (quoted && peek_char() == 34)
					{
						get_char();
						break;
					}
					val = new StringLiteral(context.BuiltinTypes, CreateStringFromBuilder(num), location);
					return 367;
				case 123:
					if (peek_char() == 123)
					{
						value_builder[num++] = (char)num2;
						get_char();
						break;
					}
					parsing_string_interpolation++;
					if (quoted && parsing_string_interpolation_quoted == null)
					{
						parsing_string_interpolation_quoted = new Stack<bool>();
					}
					if (parsing_string_interpolation_quoted != null)
					{
						parsing_string_interpolation_quoted.Push(quoted);
					}
					val = new StringLiteral(context.BuiltinTypes, CreateStringFromBuilder(num), location);
					return 366;
				case 92:
				{
					if (quoted)
					{
						break;
					}
					col++;
					int surrogate;
					num2 = escape(num2, out surrogate);
					switch (num2)
					{
					case -1:
						return 259;
					case 123:
					case 125:
						Report.Error(8087, Location, "A `{0}' character may only be escaped by doubling `{0}{0}' in an interpolated string", ((char)num2).ToString());
						break;
					}
					if (surrogate != 0)
					{
						if (num == value_builder.Length)
						{
							Array.Resize(ref value_builder, num * 2);
						}
						if (num == value_builder.Length)
						{
							Array.Resize(ref value_builder, num * 2);
						}
						value_builder[num++] = (char)num2;
						num2 = surrogate;
					}
					break;
				}
				case -1:
					return 257;
				}
				col++;
				if (num == value_builder.Length)
				{
					Array.Resize(ref value_builder, num * 2);
				}
				value_builder[num++] = (char)num2;
			}
		}

		private int TokenizeInterpolationFormat()
		{
			int num = 0;
			int num2 = 0;
			while (true)
			{
				int num3 = get_char();
				switch (num3)
				{
				case 123:
					num2++;
					break;
				case 125:
					if (num2 == 0)
					{
						putback_char = num3;
						if (num == 0)
						{
							Report.Error(8089, Location, "Empty interpolated expression format specifier");
						}
						else if (Array.IndexOf(simple_whitespaces, value_builder[num - 1]) >= 0)
						{
							Report.Error(8088, Location, "A interpolated expression format specifier may not contain trailing whitespace");
						}
						val = CreateStringFromBuilder(num);
						return 421;
					}
					num2--;
					break;
				case 92:
				{
					col++;
					int surrogate;
					num3 = escape(num3, out surrogate);
					switch (num3)
					{
					case -1:
						return 259;
					case 123:
					case 125:
						Report.Error(8087, Location, "A `{0}' character may only be escaped by doubling `{0}{0}' in an interpolated string", ((char)num3).ToString());
						break;
					}
					if (surrogate != 0)
					{
						if (num == value_builder.Length)
						{
							Array.Resize(ref value_builder, num * 2);
						}
						value_builder[num++] = (char)num3;
						num3 = surrogate;
					}
					break;
				}
				case -1:
					return 257;
				}
				col++;
				value_builder[num++] = (char)num3;
			}
		}

		private string CreateStringFromBuilder(int pos)
		{
			if (pos == 0)
			{
				return string.Empty;
			}
			if (pos <= 4)
			{
				return InternIdentifier(value_builder, pos);
			}
			return new string(value_builder, 0, pos);
		}

		private void handle_one_line_xml_comment()
		{
			int num;
			while ((num = peek_char()) == 32)
			{
				get_char();
			}
			while ((num = peek_char()) != -1 && num != 10 && num != 13)
			{
				xml_comment_buffer.Append((char)get_char());
			}
			if (num == 13 || num == 10)
			{
				xml_comment_buffer.Append(Environment.NewLine);
			}
		}

		private void update_formatted_doc_comment(int current_comment_start)
		{
			int num = xml_comment_buffer.Length - current_comment_start;
			string[] array = xml_comment_buffer.ToString(current_comment_start, num).Replace("\r", "").Split('\n');
			for (int i = 1; i < array.Length; i++)
			{
				string text = array[i];
				int num2 = text.IndexOf('*');
				string text2 = null;
				if (num2 < 0)
				{
					if (i < array.Length - 1)
					{
						return;
					}
					text2 = text;
				}
				else
				{
					text2 = text.Substring(0, num2);
				}
				string text3 = text2;
				for (int j = 0; j < text3.Length; j++)
				{
					if (text3[j] != ' ')
					{
						return;
					}
				}
				array[i] = text.Substring(num2 + 1);
			}
			xml_comment_buffer.Remove(current_comment_start, num);
			xml_comment_buffer.Insert(current_comment_start, string.Join(Environment.NewLine, array));
		}

		public void check_incorrect_doc_comment()
		{
			if (xml_comment_buffer.Length > 0)
			{
				WarningMisplacedComment(Location);
			}
		}

		public string consume_doc_comment()
		{
			if (xml_comment_buffer.Length > 0)
			{
				string result = xml_comment_buffer.ToString();
				reset_doc_comment();
				return result;
			}
			return null;
		}

		private void reset_doc_comment()
		{
			xml_comment_buffer.Length = 0;
		}

		public void cleanup()
		{
			if (ifstack != null && ifstack.Count >= 1)
			{
				if ((ifstack.Pop() & 0x10) != 0)
				{
					Report.Error(1038, Location, "#endregion directive expected");
				}
				else
				{
					Report.Error(1027, Location, "Expected `#endif' directive");
				}
			}
		}
	}
}
