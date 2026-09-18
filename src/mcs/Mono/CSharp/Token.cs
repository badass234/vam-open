namespace Mono.CSharp
{
	public class Token
	{
		public const int EOF = 257;

		public const int NONE = 258;

		public const int ERROR = 259;

		public const int FIRST_KEYWORD = 260;

		public const int ABSTRACT = 261;

		public const int AS = 262;

		public const int ADD = 263;

		public const int BASE = 264;

		public const int BOOL = 265;

		public const int BREAK = 266;

		public const int BYTE = 267;

		public const int CASE = 268;

		public const int CATCH = 269;

		public const int CHAR = 270;

		public const int CHECKED = 271;

		public const int CLASS = 272;

		public const int CONST = 273;

		public const int CONTINUE = 274;

		public const int DECIMAL = 275;

		public const int DEFAULT = 276;

		public const int DELEGATE = 277;

		public const int DO = 278;

		public const int DOUBLE = 279;

		public const int ELSE = 280;

		public const int ENUM = 281;

		public const int EVENT = 282;

		public const int EXPLICIT = 283;

		public const int EXTERN = 284;

		public const int FALSE = 285;

		public const int FINALLY = 286;

		public const int FIXED = 287;

		public const int FLOAT = 288;

		public const int FOR = 289;

		public const int FOREACH = 290;

		public const int GOTO = 291;

		public const int IF = 292;

		public const int IMPLICIT = 293;

		public const int IN = 294;

		public const int INT = 295;

		public const int INTERFACE = 296;

		public const int INTERNAL = 297;

		public const int IS = 298;

		public const int LOCK = 299;

		public const int LONG = 300;

		public const int NAMESPACE = 301;

		public const int NEW = 302;

		public const int NULL = 303;

		public const int OBJECT = 304;

		public const int OPERATOR = 305;

		public const int OUT = 306;

		public const int OVERRIDE = 307;

		public const int PARAMS = 308;

		public const int PRIVATE = 309;

		public const int PROTECTED = 310;

		public const int PUBLIC = 311;

		public const int READONLY = 312;

		public const int REF = 313;

		public const int RETURN = 314;

		public const int REMOVE = 315;

		public const int SBYTE = 316;

		public const int SEALED = 317;

		public const int SHORT = 318;

		public const int SIZEOF = 319;

		public const int STACKALLOC = 320;

		public const int STATIC = 321;

		public const int STRING = 322;

		public const int STRUCT = 323;

		public const int SWITCH = 324;

		public const int THIS = 325;

		public const int THROW = 326;

		public const int TRUE = 327;

		public const int TRY = 328;

		public const int TYPEOF = 329;

		public const int UINT = 330;

		public const int ULONG = 331;

		public const int UNCHECKED = 332;

		public const int UNSAFE = 333;

		public const int USHORT = 334;

		public const int USING = 335;

		public const int VIRTUAL = 336;

		public const int VOID = 337;

		public const int VOLATILE = 338;

		public const int WHERE = 339;

		public const int WHILE = 340;

		public const int ARGLIST = 341;

		public const int PARTIAL = 342;

		public const int ARROW = 343;

		public const int FROM = 344;

		public const int FROM_FIRST = 345;

		public const int JOIN = 346;

		public const int ON = 347;

		public const int EQUALS = 348;

		public const int SELECT = 349;

		public const int GROUP = 350;

		public const int BY = 351;

		public const int LET = 352;

		public const int ORDERBY = 353;

		public const int ASCENDING = 354;

		public const int DESCENDING = 355;

		public const int INTO = 356;

		public const int INTERR_NULLABLE = 357;

		public const int EXTERN_ALIAS = 358;

		public const int REFVALUE = 359;

		public const int REFTYPE = 360;

		public const int MAKEREF = 361;

		public const int ASYNC = 362;

		public const int AWAIT = 363;

		public const int INTERR_OPERATOR = 364;

		public const int WHEN = 365;

		public const int INTERPOLATED_STRING = 366;

		public const int INTERPOLATED_STRING_END = 367;

		public const int GET = 368;

		public const int SET = 369;

		public const int LAST_KEYWORD = 370;

		public const int OPEN_BRACE = 371;

		public const int CLOSE_BRACE = 372;

		public const int OPEN_BRACKET = 373;

		public const int CLOSE_BRACKET = 374;

		public const int OPEN_PARENS = 375;

		public const int CLOSE_PARENS = 376;

		public const int DOT = 377;

		public const int COMMA = 378;

		public const int COLON = 379;

		public const int SEMICOLON = 380;

		public const int TILDE = 381;

		public const int PLUS = 382;

		public const int MINUS = 383;

		public const int BANG = 384;

		public const int ASSIGN = 385;

		public const int OP_LT = 386;

		public const int OP_GT = 387;

		public const int BITWISE_AND = 388;

		public const int BITWISE_OR = 389;

		public const int STAR = 390;

		public const int PERCENT = 391;

		public const int DIV = 392;

		public const int CARRET = 393;

		public const int INTERR = 394;

		public const int DOUBLE_COLON = 395;

		public const int OP_INC = 396;

		public const int OP_DEC = 397;

		public const int OP_SHIFT_LEFT = 398;

		public const int OP_SHIFT_RIGHT = 399;

		public const int OP_LE = 400;

		public const int OP_GE = 401;

		public const int OP_EQ = 402;

		public const int OP_NE = 403;

		public const int OP_AND = 404;

		public const int OP_OR = 405;

		public const int OP_MULT_ASSIGN = 406;

		public const int OP_DIV_ASSIGN = 407;

		public const int OP_MOD_ASSIGN = 408;

		public const int OP_ADD_ASSIGN = 409;

		public const int OP_SUB_ASSIGN = 410;

		public const int OP_SHIFT_LEFT_ASSIGN = 411;

		public const int OP_SHIFT_RIGHT_ASSIGN = 412;

		public const int OP_AND_ASSIGN = 413;

		public const int OP_XOR_ASSIGN = 414;

		public const int OP_OR_ASSIGN = 415;

		public const int OP_PTR = 416;

		public const int OP_COALESCING = 417;

		public const int OP_GENERICS_LT = 418;

		public const int OP_GENERICS_LT_DECL = 419;

		public const int OP_GENERICS_GT = 420;

		public const int LITERAL = 421;

		public const int IDENTIFIER = 422;

		public const int OPEN_PARENS_LAMBDA = 423;

		public const int OPEN_PARENS_CAST = 424;

		public const int GENERIC_DIMENSION = 425;

		public const int DEFAULT_COLON = 426;

		public const int OPEN_BRACKET_EXPR = 427;

		public const int EVAL_STATEMENT_PARSER = 428;

		public const int EVAL_COMPILATION_UNIT_PARSER = 429;

		public const int EVAL_USING_DECLARATIONS_UNIT_PARSER = 430;

		public const int DOC_SEE = 431;

		public const int GENERATE_COMPLETION = 432;

		public const int COMPLETE_COMPLETION = 433;

		public const int UMINUS = 434;

		public const int yyErrorCode = 256;
	}
}
