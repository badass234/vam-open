using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Mono.CSharp.Linq;
using Mono.CSharp.Nullable;
using Mono.CSharp.yyParser;
using Mono.CSharp.yydebug;

namespace Mono.CSharp
{
	public class CSharpParser
	{
		[Flags]
		private enum ParameterModifierType
		{
			Ref = 2,
			Out = 4,
			This = 8,
			Params = 0x10,
			Arglist = 0x20,
			DefaultValue = 0x40,
			All = Ref | Out | This | Params | Arglist | DefaultValue,
			PrimaryConstructor = 0x56
		}

		private class YYRules : MarshalByRefObject
		{
			public static readonly string[] yyRule = new string[1110]
			{
				"$accept : compilation_unit", "compilation_unit : outer_declaration opt_EOF", "$$1 :", "compilation_unit : interactive_parsing $$1 opt_EOF", "compilation_unit : documentation_parsing", "outer_declaration : opt_extern_alias_directives opt_using_directives", "outer_declaration : opt_extern_alias_directives opt_using_directives namespace_or_type_declarations opt_attributes", "outer_declaration : opt_extern_alias_directives opt_using_directives attribute_sections", "outer_declaration : error", "opt_EOF :",
				"opt_EOF : EOF", "extern_alias_directives : extern_alias_directive", "extern_alias_directives : extern_alias_directives extern_alias_directive", "extern_alias_directive : EXTERN_ALIAS IDENTIFIER IDENTIFIER SEMICOLON", "extern_alias_directive : EXTERN_ALIAS error", "using_directives : using_directive", "using_directives : using_directives using_directive", "using_directive : using_namespace", "using_namespace : USING opt_static namespace_or_type_expr SEMICOLON", "using_namespace : USING opt_static IDENTIFIER ASSIGN namespace_or_type_expr SEMICOLON",
				"using_namespace : USING error", "opt_static :", "opt_static : STATIC", "$$2 :", "$$3 :", "namespace_declaration : opt_attributes NAMESPACE namespace_name $$2 OPEN_BRACE $$3 opt_extern_alias_directives opt_using_directives opt_namespace_or_type_declarations CLOSE_BRACE opt_semicolon_error", "namespace_declaration : opt_attributes NAMESPACE namespace_name", "opt_semicolon_error :", "opt_semicolon_error : SEMICOLON", "opt_semicolon_error : error",
				"namespace_name : IDENTIFIER", "namespace_name : namespace_name DOT IDENTIFIER", "namespace_name : error", "opt_semicolon :", "opt_semicolon : SEMICOLON", "opt_comma :", "opt_comma : COMMA", "opt_using_directives :", "opt_using_directives : using_directives", "opt_extern_alias_directives :",
				"opt_extern_alias_directives : extern_alias_directives", "opt_namespace_or_type_declarations :", "opt_namespace_or_type_declarations : namespace_or_type_declarations", "namespace_or_type_declarations : namespace_or_type_declaration", "namespace_or_type_declarations : namespace_or_type_declarations namespace_or_type_declaration", "namespace_or_type_declaration : type_declaration", "namespace_or_type_declaration : namespace_declaration", "namespace_or_type_declaration : attribute_sections CLOSE_BRACE", "type_declaration : class_declaration", "type_declaration : struct_declaration",
				"type_declaration : interface_declaration", "type_declaration : enum_declaration", "type_declaration : delegate_declaration", "opt_attributes :", "opt_attributes : attribute_sections", "attribute_sections : attribute_section", "attribute_sections : attribute_sections attribute_section", "$$4 :", "attribute_section : OPEN_BRACKET $$4 attribute_section_cont", "$$5 :",
				"attribute_section_cont : attribute_target COLON $$5 attribute_list opt_comma CLOSE_BRACKET", "attribute_section_cont : attribute_list opt_comma CLOSE_BRACKET", "attribute_section_cont : IDENTIFIER error", "attribute_section_cont : error", "attribute_target : IDENTIFIER", "attribute_target : EVENT", "attribute_target : RETURN", "attribute_list : attribute", "attribute_list : attribute_list COMMA attribute", "$$6 :",
				"attribute : attribute_name $$6 opt_attribute_arguments", "attribute_name : namespace_or_type_expr", "opt_attribute_arguments :", "opt_attribute_arguments : OPEN_PARENS attribute_arguments CLOSE_PARENS", "attribute_arguments :", "attribute_arguments : positional_or_named_argument", "attribute_arguments : named_attribute_argument", "attribute_arguments : attribute_arguments COMMA positional_or_named_argument", "attribute_arguments : attribute_arguments COMMA named_attribute_argument", "positional_or_named_argument : expression",
				"positional_or_named_argument : named_argument", "positional_or_named_argument : error", "$$7 :", "named_attribute_argument : IDENTIFIER ASSIGN $$7 expression", "named_argument : identifier_inside_body COLON opt_named_modifier named_argument_expr", "named_argument_expr : expression_or_error", "opt_named_modifier :", "opt_named_modifier : REF", "opt_named_modifier : OUT", "opt_class_member_declarations :",
				"opt_class_member_declarations : class_member_declarations", "class_member_declarations : class_member_declaration", "class_member_declarations : class_member_declarations class_member_declaration", "class_member_declaration : constant_declaration", "class_member_declaration : field_declaration", "class_member_declaration : method_declaration", "class_member_declaration : property_declaration", "class_member_declaration : event_declaration", "class_member_declaration : indexer_declaration", "class_member_declaration : operator_declaration",
				"class_member_declaration : constructor_declaration", "class_member_declaration : primary_constructor_body", "class_member_declaration : destructor_declaration", "class_member_declaration : type_declaration", "class_member_declaration : attributes_without_members", "class_member_declaration : incomplete_member", "class_member_declaration : error", "$$8 :", "primary_constructor_body : OPEN_BRACE $$8 opt_statement_list block_end", "$$9 :",
				"$$10 :", "$$11 :", "$$12 :", "$$13 :", "struct_declaration : opt_attributes opt_modifiers opt_partial STRUCT $$9 type_declaration_name $$10 opt_primary_parameters opt_class_base opt_type_parameter_constraints_clauses $$11 OPEN_BRACE $$12 opt_class_member_declarations CLOSE_BRACE $$13 opt_semicolon", "struct_declaration : opt_attributes opt_modifiers opt_partial STRUCT error", "$$14 :", "constant_declaration : opt_attributes opt_modifiers CONST type IDENTIFIER $$14 constant_initializer opt_constant_declarators SEMICOLON", "constant_declaration : opt_attributes opt_modifiers CONST type error", "opt_constant_declarators :",
				"opt_constant_declarators : constant_declarators", "constant_declarators : constant_declarator", "constant_declarators : constant_declarators constant_declarator", "constant_declarator : COMMA IDENTIFIER constant_initializer", "$$15 :", "constant_initializer : ASSIGN $$15 constant_initializer_expr", "constant_initializer : error", "constant_initializer_expr : constant_expression", "constant_initializer_expr : array_initializer", "$$16 :",
				"field_declaration : opt_attributes opt_modifiers member_type IDENTIFIER $$16 opt_field_initializer opt_field_declarators SEMICOLON", "$$17 :", "field_declaration : opt_attributes opt_modifiers FIXED simple_type IDENTIFIER $$17 fixed_field_size opt_fixed_field_declarators SEMICOLON", "field_declaration : opt_attributes opt_modifiers FIXED simple_type error SEMICOLON", "opt_field_initializer :", "$$18 :", "opt_field_initializer : ASSIGN $$18 variable_initializer", "opt_field_declarators :", "opt_field_declarators : field_declarators", "field_declarators : field_declarator",
				"field_declarators : field_declarators field_declarator", "field_declarator : COMMA IDENTIFIER", "$$19 :", "field_declarator : COMMA IDENTIFIER ASSIGN $$19 variable_initializer", "opt_fixed_field_declarators :", "opt_fixed_field_declarators : fixed_field_declarators", "fixed_field_declarators : fixed_field_declarator", "fixed_field_declarators : fixed_field_declarators fixed_field_declarator", "fixed_field_declarator : COMMA IDENTIFIER fixed_field_size", "$$20 :",
				"fixed_field_size : OPEN_BRACKET $$20 expression CLOSE_BRACKET", "fixed_field_size : OPEN_BRACKET error", "variable_initializer : expression", "variable_initializer : array_initializer", "variable_initializer : error", "$$21 :", "method_declaration : method_header $$21 method_body_expression_block", "$$22 :", "$$23 :", "method_header : opt_attributes opt_modifiers member_type method_declaration_name OPEN_PARENS $$22 opt_formal_parameter_list CLOSE_PARENS $$23 opt_type_parameter_constraints_clauses",
				"$$24 :", "$$25 :", "$$26 :", "method_header : opt_attributes opt_modifiers PARTIAL VOID $$24 method_declaration_name OPEN_PARENS $$25 opt_formal_parameter_list CLOSE_PARENS $$26 opt_type_parameter_constraints_clauses", "method_header : opt_attributes opt_modifiers member_type modifiers method_declaration_name OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS", "method_header : opt_attributes opt_modifiers member_type method_declaration_name error", "method_body_expression_block : method_body", "method_body_expression_block : expression_block", "method_body : block", "method_body : SEMICOLON",
				"$$27 :", "expression_block : ARROW $$27 expression SEMICOLON", "opt_formal_parameter_list :", "opt_formal_parameter_list : formal_parameter_list", "formal_parameter_list : fixed_parameters", "formal_parameter_list : fixed_parameters COMMA parameter_array", "formal_parameter_list : fixed_parameters COMMA arglist_modifier", "formal_parameter_list : parameter_array COMMA error", "formal_parameter_list : fixed_parameters COMMA parameter_array COMMA error", "formal_parameter_list : arglist_modifier COMMA error",
				"formal_parameter_list : fixed_parameters COMMA ARGLIST COMMA error", "formal_parameter_list : parameter_array", "formal_parameter_list : arglist_modifier", "formal_parameter_list : error", "fixed_parameters : fixed_parameter", "fixed_parameters : fixed_parameters COMMA fixed_parameter", "fixed_parameter : opt_attributes opt_parameter_modifier parameter_type identifier_inside_body", "fixed_parameter : opt_attributes opt_parameter_modifier parameter_type identifier_inside_body OPEN_BRACKET CLOSE_BRACKET", "fixed_parameter : attribute_sections error", "fixed_parameter : opt_attributes opt_parameter_modifier parameter_type error",
				"$$28 :", "fixed_parameter : opt_attributes opt_parameter_modifier parameter_type identifier_inside_body ASSIGN $$28 constant_expression", "opt_parameter_modifier :", "opt_parameter_modifier : parameter_modifiers", "parameter_modifiers : parameter_modifier", "parameter_modifiers : parameter_modifiers parameter_modifier", "parameter_modifier : REF", "parameter_modifier : OUT", "parameter_modifier : THIS", "parameter_array : opt_attributes params_modifier type IDENTIFIER",
				"parameter_array : opt_attributes params_modifier type IDENTIFIER ASSIGN constant_expression", "parameter_array : opt_attributes params_modifier type error", "params_modifier : PARAMS", "params_modifier : PARAMS parameter_modifier", "params_modifier : PARAMS params_modifier", "arglist_modifier : ARGLIST", "$$29 :", "$$30 :", "$$31 :", "$$32 :",
				"property_declaration : opt_attributes opt_modifiers member_type member_declaration_name $$29 OPEN_BRACE $$30 accessor_declarations $$31 CLOSE_BRACE $$32 opt_property_initializer", "$$33 :", "property_declaration : opt_attributes opt_modifiers member_type member_declaration_name $$33 expression_block", "opt_property_initializer :", "$$34 :", "opt_property_initializer : ASSIGN $$34 property_initializer SEMICOLON", "property_initializer : expression", "property_initializer : array_initializer", "$$35 :", "$$36 :",
				"indexer_declaration : opt_attributes opt_modifiers member_type indexer_declaration_name OPEN_BRACKET $$35 opt_formal_parameter_list CLOSE_BRACKET $$36 indexer_body", "indexer_body : OPEN_BRACE accessor_declarations CLOSE_BRACE", "indexer_body : expression_block", "accessor_declarations : get_accessor_declaration", "accessor_declarations : get_accessor_declaration accessor_declarations", "accessor_declarations : set_accessor_declaration", "accessor_declarations : set_accessor_declaration accessor_declarations", "accessor_declarations : error", "$$37 :", "get_accessor_declaration : opt_attributes opt_modifiers GET $$37 accessor_body",
				"$$38 :", "set_accessor_declaration : opt_attributes opt_modifiers SET $$38 accessor_body", "accessor_body : block", "accessor_body : SEMICOLON", "accessor_body : error", "$$39 :", "$$40 :", "$$41 :", "$$42 :", "interface_declaration : opt_attributes opt_modifiers opt_partial INTERFACE $$39 type_declaration_name $$40 opt_class_base opt_type_parameter_constraints_clauses $$41 OPEN_BRACE opt_interface_member_declarations CLOSE_BRACE $$42 opt_semicolon",
				"interface_declaration : opt_attributes opt_modifiers opt_partial INTERFACE error", "opt_interface_member_declarations :", "opt_interface_member_declarations : interface_member_declarations", "interface_member_declarations : interface_member_declaration", "interface_member_declarations : interface_member_declarations interface_member_declaration", "interface_member_declaration : constant_declaration", "interface_member_declaration : field_declaration", "interface_member_declaration : method_declaration", "interface_member_declaration : property_declaration", "interface_member_declaration : event_declaration",
				"interface_member_declaration : indexer_declaration", "interface_member_declaration : operator_declaration", "interface_member_declaration : constructor_declaration", "interface_member_declaration : type_declaration", "$$43 :", "operator_declaration : opt_attributes opt_modifiers operator_declarator $$43 method_body_expression_block", "operator_type : type_expression_or_array", "operator_type : VOID", "$$44 :", "operator_declarator : operator_type OPERATOR overloadable_operator OPEN_PARENS $$44 opt_formal_parameter_list CLOSE_PARENS",
				"operator_declarator : conversion_operator_declarator", "overloadable_operator : BANG", "overloadable_operator : TILDE", "overloadable_operator : OP_INC", "overloadable_operator : OP_DEC", "overloadable_operator : TRUE", "overloadable_operator : FALSE", "overloadable_operator : PLUS", "overloadable_operator : MINUS", "overloadable_operator : STAR",
				"overloadable_operator : DIV", "overloadable_operator : PERCENT", "overloadable_operator : BITWISE_AND", "overloadable_operator : BITWISE_OR", "overloadable_operator : CARRET", "overloadable_operator : OP_SHIFT_LEFT", "overloadable_operator : OP_SHIFT_RIGHT", "overloadable_operator : OP_EQ", "overloadable_operator : OP_NE", "overloadable_operator : OP_GT",
				"overloadable_operator : OP_LT", "overloadable_operator : OP_GE", "overloadable_operator : OP_LE", "overloadable_operator : IS", "$$45 :", "conversion_operator_declarator : IMPLICIT OPERATOR type OPEN_PARENS $$45 opt_formal_parameter_list CLOSE_PARENS", "$$46 :", "conversion_operator_declarator : EXPLICIT OPERATOR type OPEN_PARENS $$46 opt_formal_parameter_list CLOSE_PARENS", "conversion_operator_declarator : IMPLICIT error", "conversion_operator_declarator : EXPLICIT error",
				"constructor_declaration : constructor_declarator constructor_body", "$$47 :", "$$48 :", "constructor_declarator : opt_attributes opt_modifiers IDENTIFIER $$47 OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS $$48 opt_constructor_initializer", "constructor_body : block_prepared", "constructor_body : SEMICOLON", "opt_constructor_initializer :", "opt_constructor_initializer : constructor_initializer", "$$49 :", "constructor_initializer : COLON BASE OPEN_PARENS $$49 opt_argument_list CLOSE_PARENS",
				"$$50 :", "constructor_initializer : COLON THIS OPEN_PARENS $$50 opt_argument_list CLOSE_PARENS", "constructor_initializer : COLON error", "constructor_initializer : error", "$$51 :", "destructor_declaration : opt_attributes opt_modifiers TILDE $$51 IDENTIFIER OPEN_PARENS CLOSE_PARENS method_body", "$$52 :", "event_declaration : opt_attributes opt_modifiers EVENT type member_declaration_name $$52 opt_event_initializer opt_event_declarators SEMICOLON", "$$53 :", "$$54 :",
				"event_declaration : opt_attributes opt_modifiers EVENT type member_declaration_name OPEN_BRACE $$53 event_accessor_declarations $$54 CLOSE_BRACE", "event_declaration : opt_attributes opt_modifiers EVENT type error", "opt_event_initializer :", "$$55 :", "opt_event_initializer : ASSIGN $$55 event_variable_initializer", "opt_event_declarators :", "opt_event_declarators : event_declarators", "event_declarators : event_declarator", "event_declarators : event_declarators event_declarator", "event_declarator : COMMA IDENTIFIER",
				"$$56 :", "event_declarator : COMMA IDENTIFIER ASSIGN $$56 event_variable_initializer", "$$57 :", "event_variable_initializer : $$57 variable_initializer", "event_accessor_declarations : add_accessor_declaration remove_accessor_declaration", "event_accessor_declarations : remove_accessor_declaration add_accessor_declaration", "event_accessor_declarations : add_accessor_declaration", "event_accessor_declarations : remove_accessor_declaration", "event_accessor_declarations : error", "$$58 :",
				"add_accessor_declaration : opt_attributes opt_modifiers ADD $$58 event_accessor_block", "$$59 :", "remove_accessor_declaration : opt_attributes opt_modifiers REMOVE $$59 event_accessor_block", "event_accessor_block : opt_semicolon", "event_accessor_block : block", "attributes_without_members : attribute_sections CLOSE_BRACE", "incomplete_member : opt_attributes opt_modifiers member_type CLOSE_BRACE", "$$60 :", "$$61 :", "$$62 :",
				"enum_declaration : opt_attributes opt_modifiers ENUM type_declaration_name opt_enum_base $$60 OPEN_BRACE $$61 opt_enum_member_declarations $$62 CLOSE_BRACE opt_semicolon", "opt_enum_base :", "opt_enum_base : COLON type", "opt_enum_base : COLON error", "opt_enum_member_declarations :", "opt_enum_member_declarations : enum_member_declarations", "opt_enum_member_declarations : enum_member_declarations COMMA", "enum_member_declarations : enum_member_declaration", "enum_member_declarations : enum_member_declarations COMMA enum_member_declaration", "enum_member_declaration : opt_attributes IDENTIFIER",
				"$$63 :", "enum_member_declaration : opt_attributes IDENTIFIER $$63 ASSIGN constant_expression", "enum_member_declaration : opt_attributes IDENTIFIER error", "enum_member_declaration : attributes_without_members", "$$64 :", "$$65 :", "$$66 :", "delegate_declaration : opt_attributes opt_modifiers DELEGATE member_type type_declaration_name OPEN_PARENS $$64 opt_formal_parameter_list CLOSE_PARENS $$65 opt_type_parameter_constraints_clauses $$66 SEMICOLON", "opt_nullable :", "opt_nullable : INTERR_NULLABLE",
				"namespace_or_type_expr : member_name", "namespace_or_type_expr : qualified_alias_member IDENTIFIER opt_type_argument_list", "namespace_or_type_expr : qualified_alias_member IDENTIFIER generic_dimension", "member_name : simple_name_expr", "member_name : namespace_or_type_expr DOT IDENTIFIER opt_type_argument_list", "member_name : namespace_or_type_expr DOT IDENTIFIER generic_dimension", "simple_name_expr : IDENTIFIER opt_type_argument_list", "simple_name_expr : IDENTIFIER generic_dimension", "opt_type_argument_list :", "opt_type_argument_list : OP_GENERICS_LT type_arguments OP_GENERICS_GT",
				"opt_type_argument_list : OP_GENERICS_LT error", "type_arguments : type", "type_arguments : type_arguments COMMA type", "$$67 :", "type_declaration_name : IDENTIFIER $$67 opt_type_parameter_list", "member_declaration_name : method_declaration_name", "method_declaration_name : type_declaration_name", "method_declaration_name : explicit_interface IDENTIFIER opt_type_parameter_list", "indexer_declaration_name : THIS", "indexer_declaration_name : explicit_interface THIS",
				"explicit_interface : IDENTIFIER opt_type_argument_list DOT", "explicit_interface : qualified_alias_member IDENTIFIER opt_type_argument_list DOT", "explicit_interface : explicit_interface IDENTIFIER opt_type_argument_list DOT", "opt_type_parameter_list :", "opt_type_parameter_list : OP_GENERICS_LT_DECL type_parameters OP_GENERICS_GT", "type_parameters : type_parameter", "type_parameters : type_parameters COMMA type_parameter", "type_parameter : opt_attributes opt_type_parameter_variance IDENTIFIER", "type_parameter : error", "type_and_void : type_expression_or_array",
				"type_and_void : VOID", "member_type : type_and_void", "type : type_expression_or_array", "type : void_invalid", "simple_type : type_expression", "simple_type : void_invalid", "parameter_type : type_expression_or_array", "parameter_type : VOID", "type_expression_or_array : type_expression", "type_expression_or_array : type_expression rank_specifiers",
				"type_expression : namespace_or_type_expr opt_nullable", "type_expression : namespace_or_type_expr pointer_stars", "type_expression : builtin_type_expression", "void_invalid : VOID", "builtin_type_expression : builtin_types opt_nullable", "builtin_type_expression : builtin_types pointer_stars", "builtin_type_expression : VOID pointer_stars", "type_list : base_type_name", "type_list : type_list COMMA base_type_name", "base_type_name : type",
				"builtin_types : OBJECT", "builtin_types : STRING", "builtin_types : BOOL", "builtin_types : DECIMAL", "builtin_types : FLOAT", "builtin_types : DOUBLE", "builtin_types : integral_type", "integral_type : SBYTE", "integral_type : BYTE", "integral_type : SHORT",
				"integral_type : USHORT", "integral_type : INT", "integral_type : UINT", "integral_type : LONG", "integral_type : ULONG", "integral_type : CHAR", "primary_expression : type_name_expression", "primary_expression : literal", "primary_expression : array_creation_expression", "primary_expression : parenthesized_expression",
				"primary_expression : default_value_expression", "primary_expression : invocation_expression", "primary_expression : element_access", "primary_expression : this_access", "primary_expression : base_access", "primary_expression : post_increment_expression", "primary_expression : post_decrement_expression", "primary_expression : object_or_delegate_creation_expression", "primary_expression : anonymous_type_expression", "primary_expression : typeof_expression",
				"primary_expression : sizeof_expression", "primary_expression : checked_expression", "primary_expression : unchecked_expression", "primary_expression : pointer_member_access", "primary_expression : anonymous_method_expression", "primary_expression : undocumented_expressions", "primary_expression : interpolated_string", "type_name_expression : simple_name_expr", "type_name_expression : IDENTIFIER GENERATE_COMPLETION", "type_name_expression : member_access",
				"literal : boolean_literal", "literal : LITERAL", "literal : NULL", "boolean_literal : TRUE", "boolean_literal : FALSE", "interpolated_string : INTERPOLATED_STRING interpolations INTERPOLATED_STRING_END", "interpolated_string : INTERPOLATED_STRING_END", "interpolations : interpolation", "interpolations : interpolations INTERPOLATED_STRING interpolation", "interpolation : expression",
				"interpolation : expression COMMA expression", "$$68 :", "interpolation : expression COLON $$68 LITERAL", "$$69 :", "interpolation : expression COMMA expression COLON $$69 LITERAL", "open_parens_any : OPEN_PARENS", "open_parens_any : OPEN_PARENS_CAST", "close_parens : CLOSE_PARENS", "close_parens : COMPLETE_COMPLETION", "parenthesized_expression : OPEN_PARENS expression CLOSE_PARENS",
				"parenthesized_expression : OPEN_PARENS expression COMPLETE_COMPLETION", "member_access : primary_expression DOT identifier_inside_body opt_type_argument_list", "member_access : primary_expression DOT identifier_inside_body generic_dimension", "member_access : primary_expression INTERR_OPERATOR DOT identifier_inside_body opt_type_argument_list", "member_access : builtin_types DOT identifier_inside_body opt_type_argument_list", "member_access : BASE DOT identifier_inside_body opt_type_argument_list", "member_access : AWAIT DOT identifier_inside_body opt_type_argument_list", "member_access : qualified_alias_member identifier_inside_body opt_type_argument_list", "member_access : qualified_alias_member identifier_inside_body generic_dimension", "member_access : primary_expression DOT GENERATE_COMPLETION",
				"member_access : primary_expression DOT IDENTIFIER GENERATE_COMPLETION", "member_access : builtin_types DOT GENERATE_COMPLETION", "member_access : builtin_types DOT IDENTIFIER GENERATE_COMPLETION", "invocation_expression : primary_expression open_parens_any opt_argument_list close_parens", "invocation_expression : primary_expression open_parens_any argument_list error", "invocation_expression : primary_expression open_parens_any error", "opt_object_or_collection_initializer :", "opt_object_or_collection_initializer : object_or_collection_initializer", "object_or_collection_initializer : OPEN_BRACE opt_member_initializer_list close_brace_or_complete_completion", "object_or_collection_initializer : OPEN_BRACE member_initializer_list COMMA CLOSE_BRACE",
				"opt_member_initializer_list :", "opt_member_initializer_list : member_initializer_list", "member_initializer_list : member_initializer", "member_initializer_list : member_initializer_list COMMA member_initializer", "member_initializer_list : member_initializer_list error", "member_initializer : IDENTIFIER ASSIGN initializer_value", "member_initializer : AWAIT ASSIGN initializer_value", "member_initializer : GENERATE_COMPLETION", "member_initializer : non_assignment_expression opt_COMPLETE_COMPLETION", "member_initializer : OPEN_BRACE expression_list CLOSE_BRACE",
				"member_initializer : OPEN_BRACKET_EXPR argument_list CLOSE_BRACKET ASSIGN initializer_value", "member_initializer : OPEN_BRACE CLOSE_BRACE", "initializer_value : expression", "initializer_value : object_or_collection_initializer", "opt_argument_list :", "opt_argument_list : argument_list", "argument_list : argument_or_named_argument", "argument_list : argument_list COMMA argument", "argument_list : argument_list COMMA named_argument", "argument_list : argument_list COMMA error",
				"argument_list : COMMA error", "argument : expression", "argument : non_simple_argument", "argument_or_named_argument : argument", "argument_or_named_argument : named_argument", "non_simple_argument : REF variable_reference", "non_simple_argument : REF declaration_expression", "non_simple_argument : OUT variable_reference", "non_simple_argument : OUT declaration_expression", "non_simple_argument : ARGLIST OPEN_PARENS argument_list CLOSE_PARENS",
				"non_simple_argument : ARGLIST OPEN_PARENS CLOSE_PARENS", "declaration_expression : OPEN_PARENS declaration_expression CLOSE_PARENS", "declaration_expression : variable_type identifier_inside_body", "declaration_expression : variable_type identifier_inside_body ASSIGN expression", "variable_reference : expression", "element_access : primary_expression OPEN_BRACKET_EXPR expression_list_arguments CLOSE_BRACKET", "element_access : primary_expression INTERR_OPERATOR OPEN_BRACKET_EXPR expression_list_arguments CLOSE_BRACKET", "element_access : primary_expression OPEN_BRACKET_EXPR expression_list_arguments error", "element_access : primary_expression OPEN_BRACKET_EXPR error", "expression_list : expression_or_error",
				"expression_list : expression_list COMMA expression_or_error", "expression_list_arguments : expression_list_argument", "expression_list_arguments : expression_list_arguments COMMA expression_list_argument", "expression_list_argument : expression", "expression_list_argument : named_argument", "this_access : THIS", "base_access : BASE OPEN_BRACKET_EXPR expression_list_arguments CLOSE_BRACKET", "base_access : BASE OPEN_BRACKET error", "post_increment_expression : primary_expression OP_INC", "post_decrement_expression : primary_expression OP_DEC",
				"object_or_delegate_creation_expression : NEW new_expr_type open_parens_any opt_argument_list CLOSE_PARENS opt_object_or_collection_initializer", "object_or_delegate_creation_expression : NEW new_expr_type object_or_collection_initializer", "array_creation_expression : NEW new_expr_type OPEN_BRACKET_EXPR expression_list CLOSE_BRACKET opt_rank_specifier opt_array_initializer", "array_creation_expression : NEW new_expr_type rank_specifiers opt_array_initializer", "array_creation_expression : NEW rank_specifier array_initializer", "array_creation_expression : NEW new_expr_type OPEN_BRACKET CLOSE_BRACKET OPEN_BRACKET_EXPR error CLOSE_BRACKET", "array_creation_expression : NEW new_expr_type error", "$$70 :", "new_expr_type : $$70 simple_type", "anonymous_type_expression : NEW OPEN_BRACE anonymous_type_parameters_opt_comma CLOSE_BRACE",
				"anonymous_type_expression : NEW OPEN_BRACE GENERATE_COMPLETION", "anonymous_type_parameters_opt_comma : anonymous_type_parameters_opt", "anonymous_type_parameters_opt_comma : anonymous_type_parameters COMMA", "anonymous_type_parameters_opt :", "anonymous_type_parameters_opt : anonymous_type_parameters", "anonymous_type_parameters : anonymous_type_parameter", "anonymous_type_parameters : anonymous_type_parameters COMMA anonymous_type_parameter", "anonymous_type_parameters : COMPLETE_COMPLETION", "anonymous_type_parameters : anonymous_type_parameter COMPLETE_COMPLETION", "anonymous_type_parameter : identifier_inside_body ASSIGN variable_initializer",
				"anonymous_type_parameter : identifier_inside_body", "anonymous_type_parameter : member_access", "anonymous_type_parameter : error", "opt_rank_specifier :", "opt_rank_specifier : rank_specifiers", "rank_specifiers : rank_specifier", "rank_specifiers : rank_specifier rank_specifiers", "rank_specifier : OPEN_BRACKET CLOSE_BRACKET", "rank_specifier : OPEN_BRACKET dim_separators CLOSE_BRACKET", "dim_separators : COMMA",
				"dim_separators : dim_separators COMMA", "opt_array_initializer :", "opt_array_initializer : array_initializer", "array_initializer : OPEN_BRACE CLOSE_BRACE", "array_initializer : OPEN_BRACE variable_initializer_list opt_comma CLOSE_BRACE", "variable_initializer_list : variable_initializer", "variable_initializer_list : variable_initializer_list COMMA variable_initializer", "typeof_expression : TYPEOF open_parens_any typeof_type_expression CLOSE_PARENS", "typeof_type_expression : type_and_void", "typeof_type_expression : error",
				"generic_dimension : GENERIC_DIMENSION", "qualified_alias_member : IDENTIFIER DOUBLE_COLON", "sizeof_expression : SIZEOF open_parens_any type CLOSE_PARENS", "sizeof_expression : SIZEOF open_parens_any type error", "checked_expression : CHECKED open_parens_any expression CLOSE_PARENS", "checked_expression : CHECKED error", "unchecked_expression : UNCHECKED open_parens_any expression CLOSE_PARENS", "unchecked_expression : UNCHECKED error", "pointer_member_access : primary_expression OP_PTR IDENTIFIER opt_type_argument_list", "$$71 :",
				"anonymous_method_expression : DELEGATE opt_anonymous_method_signature $$71 block", "$$72 :", "anonymous_method_expression : ASYNC DELEGATE opt_anonymous_method_signature $$72 block", "opt_anonymous_method_signature :", "opt_anonymous_method_signature : anonymous_method_signature", "$$73 :", "anonymous_method_signature : OPEN_PARENS $$73 opt_formal_parameter_list CLOSE_PARENS", "default_value_expression : DEFAULT open_parens_any type CLOSE_PARENS", "unary_expression : primary_expression", "unary_expression : BANG prefixed_unary_expression",
				"unary_expression : TILDE prefixed_unary_expression", "unary_expression : OPEN_PARENS_CAST type CLOSE_PARENS prefixed_unary_expression", "unary_expression : AWAIT prefixed_unary_expression", "unary_expression : BANG error", "unary_expression : TILDE error", "unary_expression : OPEN_PARENS_CAST type CLOSE_PARENS error", "unary_expression : AWAIT error", "prefixed_unary_expression : unary_expression", "prefixed_unary_expression : PLUS prefixed_unary_expression", "prefixed_unary_expression : MINUS prefixed_unary_expression",
				"prefixed_unary_expression : OP_INC prefixed_unary_expression", "prefixed_unary_expression : OP_DEC prefixed_unary_expression", "prefixed_unary_expression : STAR prefixed_unary_expression", "prefixed_unary_expression : BITWISE_AND prefixed_unary_expression", "prefixed_unary_expression : PLUS error", "prefixed_unary_expression : MINUS error", "prefixed_unary_expression : OP_INC error", "prefixed_unary_expression : OP_DEC error", "prefixed_unary_expression : STAR error", "prefixed_unary_expression : BITWISE_AND error",
				"multiplicative_expression : prefixed_unary_expression", "multiplicative_expression : multiplicative_expression STAR prefixed_unary_expression", "multiplicative_expression : multiplicative_expression DIV prefixed_unary_expression", "multiplicative_expression : multiplicative_expression PERCENT prefixed_unary_expression", "multiplicative_expression : multiplicative_expression STAR error", "multiplicative_expression : multiplicative_expression DIV error", "multiplicative_expression : multiplicative_expression PERCENT error", "additive_expression : multiplicative_expression", "additive_expression : additive_expression PLUS multiplicative_expression", "additive_expression : additive_expression MINUS multiplicative_expression",
				"additive_expression : additive_expression PLUS error", "additive_expression : additive_expression MINUS error", "additive_expression : additive_expression AS type", "additive_expression : additive_expression IS pattern_type_expr opt_identifier", "additive_expression : additive_expression IS pattern_expr", "additive_expression : additive_expression AS error", "additive_expression : additive_expression IS error", "additive_expression : AWAIT IS type", "additive_expression : AWAIT AS type", "pattern_type_expr : variable_type",
				"pattern_expr : literal", "pattern_expr : PLUS prefixed_unary_expression", "pattern_expr : MINUS prefixed_unary_expression", "pattern_expr : sizeof_expression", "pattern_expr : default_value_expression", "pattern_expr : OPEN_PARENS_CAST type CLOSE_PARENS prefixed_unary_expression", "pattern_expr : STAR", "pattern_expr : pattern_expr_invocation", "pattern_expr : pattern_property", "pattern_expr_invocation : type_name_expression OPEN_PARENS opt_pattern_list CLOSE_PARENS",
				"pattern_property : type_name_expression OPEN_BRACE pattern_property_list CLOSE_BRACE", "pattern_property_list : pattern_property_entry", "pattern_property_list : pattern_property_list COMMA pattern_property_entry", "pattern_property_entry : identifier_inside_body IS pattern", "pattern : pattern_expr", "pattern : pattern_type_expr opt_identifier", "opt_pattern_list :", "opt_pattern_list : pattern_list", "pattern_list : pattern_argument", "pattern_list : pattern_list COMMA pattern_argument",
				"pattern_argument : pattern", "pattern_argument : IDENTIFIER COLON pattern", "shift_expression : additive_expression", "shift_expression : shift_expression OP_SHIFT_LEFT additive_expression", "shift_expression : shift_expression OP_SHIFT_RIGHT additive_expression", "shift_expression : shift_expression OP_SHIFT_LEFT error", "shift_expression : shift_expression OP_SHIFT_RIGHT error", "relational_expression : shift_expression", "relational_expression : relational_expression OP_LT shift_expression", "relational_expression : relational_expression OP_GT shift_expression",
				"relational_expression : relational_expression OP_LE shift_expression", "relational_expression : relational_expression OP_GE shift_expression", "relational_expression : relational_expression OP_LT error", "relational_expression : relational_expression OP_GT error", "relational_expression : relational_expression OP_LE error", "relational_expression : relational_expression OP_GE error", "equality_expression : relational_expression", "equality_expression : equality_expression OP_EQ relational_expression", "equality_expression : equality_expression OP_NE relational_expression", "equality_expression : equality_expression OP_EQ error",
				"equality_expression : equality_expression OP_NE error", "and_expression : equality_expression", "and_expression : and_expression BITWISE_AND equality_expression", "and_expression : and_expression BITWISE_AND error", "exclusive_or_expression : and_expression", "exclusive_or_expression : exclusive_or_expression CARRET and_expression", "exclusive_or_expression : exclusive_or_expression CARRET error", "inclusive_or_expression : exclusive_or_expression", "inclusive_or_expression : inclusive_or_expression BITWISE_OR exclusive_or_expression", "inclusive_or_expression : inclusive_or_expression BITWISE_OR error",
				"conditional_and_expression : inclusive_or_expression", "conditional_and_expression : conditional_and_expression OP_AND inclusive_or_expression", "conditional_and_expression : conditional_and_expression OP_AND error", "conditional_or_expression : conditional_and_expression", "conditional_or_expression : conditional_or_expression OP_OR conditional_and_expression", "conditional_or_expression : conditional_or_expression OP_OR error", "null_coalescing_expression : conditional_or_expression", "null_coalescing_expression : conditional_or_expression OP_COALESCING null_coalescing_expression", "conditional_expression : null_coalescing_expression", "conditional_expression : null_coalescing_expression INTERR expression COLON expression",
				"conditional_expression : null_coalescing_expression INTERR expression error", "conditional_expression : null_coalescing_expression INTERR expression COLON error", "conditional_expression : null_coalescing_expression INTERR expression COLON CLOSE_BRACE", "assignment_expression : prefixed_unary_expression ASSIGN expression", "assignment_expression : prefixed_unary_expression OP_MULT_ASSIGN expression", "assignment_expression : prefixed_unary_expression OP_DIV_ASSIGN expression", "assignment_expression : prefixed_unary_expression OP_MOD_ASSIGN expression", "assignment_expression : prefixed_unary_expression OP_ADD_ASSIGN expression", "assignment_expression : prefixed_unary_expression OP_SUB_ASSIGN expression", "assignment_expression : prefixed_unary_expression OP_SHIFT_LEFT_ASSIGN expression",
				"assignment_expression : prefixed_unary_expression OP_SHIFT_RIGHT_ASSIGN expression", "assignment_expression : prefixed_unary_expression OP_AND_ASSIGN expression", "assignment_expression : prefixed_unary_expression OP_OR_ASSIGN expression", "assignment_expression : prefixed_unary_expression OP_XOR_ASSIGN expression", "lambda_parameter_list : lambda_parameter", "lambda_parameter_list : lambda_parameter_list COMMA lambda_parameter", "lambda_parameter : parameter_modifier parameter_type identifier_inside_body", "lambda_parameter : parameter_type identifier_inside_body", "lambda_parameter : IDENTIFIER", "lambda_parameter : AWAIT",
				"opt_lambda_parameter_list :", "opt_lambda_parameter_list : lambda_parameter_list", "$$74 :", "lambda_expression_body : $$74 expression", "lambda_expression_body : block", "lambda_expression_body : error", "expression_or_error : expression", "expression_or_error : error", "$$75 :", "lambda_expression : IDENTIFIER ARROW $$75 lambda_expression_body",
				"$$76 :", "lambda_expression : AWAIT ARROW $$76 lambda_expression_body", "$$77 :", "lambda_expression : ASYNC identifier_inside_body ARROW $$77 lambda_expression_body", "$$78 :", "$$79 :", "lambda_expression : OPEN_PARENS_LAMBDA $$78 opt_lambda_parameter_list CLOSE_PARENS ARROW $$79 lambda_expression_body", "$$80 :", "$$81 :", "lambda_expression : ASYNC OPEN_PARENS_LAMBDA $$80 opt_lambda_parameter_list CLOSE_PARENS ARROW $$81 lambda_expression_body",
				"expression : assignment_expression", "expression : non_assignment_expression", "non_assignment_expression : conditional_expression", "non_assignment_expression : lambda_expression", "non_assignment_expression : query_expression", "non_assignment_expression : ARGLIST", "undocumented_expressions : REFVALUE OPEN_PARENS non_assignment_expression COMMA type CLOSE_PARENS", "undocumented_expressions : REFTYPE open_parens_any expression CLOSE_PARENS", "undocumented_expressions : MAKEREF open_parens_any expression CLOSE_PARENS", "constant_expression : expression",
				"boolean_expression : expression", "opt_primary_parameters :", "opt_primary_parameters : primary_parameters", "primary_parameters : OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS", "opt_primary_parameters_with_class_base :", "opt_primary_parameters_with_class_base : class_base", "opt_primary_parameters_with_class_base : primary_parameters", "opt_primary_parameters_with_class_base : primary_parameters class_base", "$$82 :", "opt_primary_parameters_with_class_base : primary_parameters class_base OPEN_PARENS $$82 opt_argument_list CLOSE_PARENS",
				"$$83 :", "$$84 :", "$$85 :", "$$86 :", "class_declaration : opt_attributes opt_modifiers opt_partial CLASS $$83 type_declaration_name $$84 opt_primary_parameters_with_class_base opt_type_parameter_constraints_clauses $$85 OPEN_BRACE opt_class_member_declarations CLOSE_BRACE $$86 opt_semicolon", "opt_partial :", "opt_partial : PARTIAL", "opt_modifiers :", "opt_modifiers : modifiers", "modifiers : modifier",
				"modifiers : modifiers modifier", "modifier : NEW", "modifier : PUBLIC", "modifier : PROTECTED", "modifier : INTERNAL", "modifier : PRIVATE", "modifier : ABSTRACT", "modifier : SEALED", "modifier : STATIC", "modifier : READONLY",
				"modifier : VIRTUAL", "modifier : OVERRIDE", "modifier : EXTERN", "modifier : VOLATILE", "modifier : UNSAFE", "modifier : ASYNC", "opt_class_base :", "opt_class_base : class_base", "class_base : COLON type_list", "class_base : COLON type_list error",
				"opt_type_parameter_constraints_clauses :", "opt_type_parameter_constraints_clauses : type_parameter_constraints_clauses", "type_parameter_constraints_clauses : type_parameter_constraints_clause", "type_parameter_constraints_clauses : type_parameter_constraints_clauses type_parameter_constraints_clause", "type_parameter_constraints_clause : WHERE IDENTIFIER COLON type_parameter_constraints", "type_parameter_constraints_clause : WHERE IDENTIFIER error", "type_parameter_constraints : type_parameter_constraint", "type_parameter_constraints : type_parameter_constraints COMMA type_parameter_constraint", "type_parameter_constraint : type", "type_parameter_constraint : NEW OPEN_PARENS CLOSE_PARENS",
				"type_parameter_constraint : CLASS", "type_parameter_constraint : STRUCT", "opt_type_parameter_variance :", "opt_type_parameter_variance : type_parameter_variance", "type_parameter_variance : OUT", "type_parameter_variance : IN", "$$87 :", "block : OPEN_BRACE $$87 opt_statement_list block_end", "block_end : CLOSE_BRACE", "block_end : COMPLETE_COMPLETION",
				"$$88 :", "block_prepared : OPEN_BRACE $$88 opt_statement_list CLOSE_BRACE", "opt_statement_list :", "opt_statement_list : statement_list", "statement_list : statement", "statement_list : statement_list statement", "statement : block_variable_declaration", "statement : valid_declaration_statement", "statement : labeled_statement", "statement : error",
				"interactive_statement_list : interactive_statement", "interactive_statement_list : interactive_statement_list interactive_statement", "interactive_statement : block_variable_declaration", "interactive_statement : interactive_valid_declaration_statement", "interactive_statement : labeled_statement", "valid_declaration_statement : block", "valid_declaration_statement : empty_statement", "valid_declaration_statement : expression_statement", "valid_declaration_statement : selection_statement", "valid_declaration_statement : iteration_statement",
				"valid_declaration_statement : jump_statement", "valid_declaration_statement : try_statement", "valid_declaration_statement : checked_statement", "valid_declaration_statement : unchecked_statement", "valid_declaration_statement : lock_statement", "valid_declaration_statement : using_statement", "valid_declaration_statement : unsafe_statement", "valid_declaration_statement : fixed_statement", "interactive_valid_declaration_statement : block", "interactive_valid_declaration_statement : empty_statement",
				"interactive_valid_declaration_statement : interactive_expression_statement", "interactive_valid_declaration_statement : selection_statement", "interactive_valid_declaration_statement : iteration_statement", "interactive_valid_declaration_statement : jump_statement", "interactive_valid_declaration_statement : try_statement", "interactive_valid_declaration_statement : checked_statement", "interactive_valid_declaration_statement : unchecked_statement", "interactive_valid_declaration_statement : lock_statement", "interactive_valid_declaration_statement : using_statement", "interactive_valid_declaration_statement : unsafe_statement",
				"interactive_valid_declaration_statement : fixed_statement", "embedded_statement : valid_declaration_statement", "embedded_statement : block_variable_declaration", "embedded_statement : labeled_statement", "embedded_statement : error", "empty_statement : SEMICOLON", "$$89 :", "labeled_statement : identifier_inside_body COLON $$89 statement", "variable_type : variable_type_simple", "variable_type : variable_type_simple rank_specifiers",
				"variable_type_simple : type_name_expression opt_nullable", "variable_type_simple : type_name_expression pointer_stars", "variable_type_simple : builtin_type_expression", "variable_type_simple : void_invalid", "pointer_stars : pointer_star", "pointer_stars : pointer_star pointer_stars", "pointer_star : STAR", "identifier_inside_body : IDENTIFIER", "identifier_inside_body : AWAIT", "$$90 :",
				"block_variable_declaration : variable_type identifier_inside_body $$90 opt_local_variable_initializer opt_variable_declarators SEMICOLON", "$$91 :", "block_variable_declaration : CONST variable_type identifier_inside_body $$91 const_variable_initializer opt_const_declarators SEMICOLON", "opt_local_variable_initializer :", "opt_local_variable_initializer : ASSIGN block_variable_initializer", "opt_local_variable_initializer : error", "opt_variable_declarators :", "opt_variable_declarators : variable_declarators", "opt_using_or_fixed_variable_declarators :", "opt_using_or_fixed_variable_declarators : variable_declarators",
				"variable_declarators : variable_declarator", "variable_declarators : variable_declarators variable_declarator", "variable_declarator : COMMA identifier_inside_body", "variable_declarator : COMMA identifier_inside_body ASSIGN block_variable_initializer", "const_variable_initializer :", "const_variable_initializer : ASSIGN constant_initializer_expr", "opt_const_declarators :", "opt_const_declarators : const_declarators", "const_declarators : const_declarator", "const_declarators : const_declarators const_declarator",
				"const_declarator : COMMA identifier_inside_body ASSIGN constant_initializer_expr", "block_variable_initializer : variable_initializer", "block_variable_initializer : STACKALLOC simple_type OPEN_BRACKET_EXPR expression CLOSE_BRACKET", "block_variable_initializer : STACKALLOC simple_type", "expression_statement : statement_expression SEMICOLON", "expression_statement : statement_expression COMPLETE_COMPLETION", "expression_statement : statement_expression CLOSE_BRACE", "interactive_expression_statement : interactive_statement_expression SEMICOLON", "interactive_expression_statement : interactive_statement_expression COMPLETE_COMPLETION", "statement_expression : expression",
				"interactive_statement_expression : expression", "interactive_statement_expression : error", "selection_statement : if_statement", "selection_statement : switch_statement", "if_statement : IF open_parens_any boolean_expression CLOSE_PARENS embedded_statement", "if_statement : IF open_parens_any boolean_expression CLOSE_PARENS embedded_statement ELSE embedded_statement", "if_statement : IF open_parens_any boolean_expression error", "$$92 :", "switch_statement : SWITCH open_parens_any expression CLOSE_PARENS OPEN_BRACE $$92 opt_switch_sections CLOSE_BRACE", "switch_statement : SWITCH open_parens_any expression error",
				"opt_switch_sections :", "opt_switch_sections : switch_sections", "switch_sections : switch_section", "switch_sections : switch_sections switch_section", "switch_sections : error", "switch_section : switch_labels statement_list", "switch_labels : switch_label", "switch_labels : switch_labels switch_label", "switch_label : CASE constant_expression COLON", "switch_label : CASE constant_expression error",
				"switch_label : DEFAULT_COLON", "iteration_statement : while_statement", "iteration_statement : do_statement", "iteration_statement : for_statement", "iteration_statement : foreach_statement", "while_statement : WHILE open_parens_any boolean_expression CLOSE_PARENS embedded_statement", "while_statement : WHILE open_parens_any boolean_expression error", "do_statement : DO embedded_statement WHILE open_parens_any boolean_expression CLOSE_PARENS SEMICOLON", "do_statement : DO embedded_statement error", "do_statement : DO embedded_statement WHILE open_parens_any boolean_expression error",
				"$$93 :", "for_statement : FOR open_parens_any $$93 for_statement_cont", "$$94 :", "for_statement_cont : opt_for_initializer SEMICOLON $$94 for_condition_and_iterator_part embedded_statement", "for_statement_cont : error", "$$95 :", "for_condition_and_iterator_part : opt_for_condition SEMICOLON $$95 for_iterator_part", "for_condition_and_iterator_part : opt_for_condition close_parens_close_brace", "for_iterator_part : opt_for_iterator CLOSE_PARENS", "for_iterator_part : opt_for_iterator CLOSE_BRACE",
				"close_parens_close_brace : CLOSE_PARENS", "close_parens_close_brace : CLOSE_BRACE", "opt_for_initializer :", "opt_for_initializer : for_initializer", "$$96 :", "for_initializer : variable_type identifier_inside_body $$96 opt_local_variable_initializer opt_variable_declarators", "for_initializer : statement_expression_list", "opt_for_condition :", "opt_for_condition : boolean_expression", "opt_for_iterator :",
				"opt_for_iterator : for_iterator", "for_iterator : statement_expression_list", "statement_expression_list : statement_expression", "statement_expression_list : statement_expression_list COMMA statement_expression", "foreach_statement : FOREACH open_parens_any type error", "foreach_statement : FOREACH open_parens_any type identifier_inside_body error", "$$97 :", "foreach_statement : FOREACH open_parens_any type identifier_inside_body IN expression CLOSE_PARENS $$97 embedded_statement", "jump_statement : break_statement", "jump_statement : continue_statement",
				"jump_statement : goto_statement", "jump_statement : return_statement", "jump_statement : throw_statement", "jump_statement : yield_statement", "break_statement : BREAK SEMICOLON", "continue_statement : CONTINUE SEMICOLON", "continue_statement : CONTINUE error", "goto_statement : GOTO identifier_inside_body SEMICOLON", "goto_statement : GOTO CASE constant_expression SEMICOLON", "goto_statement : GOTO DEFAULT SEMICOLON",
				"return_statement : RETURN opt_expression SEMICOLON", "return_statement : RETURN expression error", "return_statement : RETURN error", "throw_statement : THROW opt_expression SEMICOLON", "throw_statement : THROW expression error", "throw_statement : THROW error", "yield_statement : identifier_inside_body RETURN opt_expression SEMICOLON", "yield_statement : identifier_inside_body RETURN expression error", "yield_statement : identifier_inside_body BREAK SEMICOLON", "opt_expression :",
				"opt_expression : expression", "try_statement : TRY block catch_clauses", "try_statement : TRY block FINALLY block", "try_statement : TRY block catch_clauses FINALLY block", "try_statement : TRY block error", "catch_clauses : catch_clause", "catch_clauses : catch_clauses catch_clause", "opt_identifier :", "opt_identifier : identifier_inside_body", "catch_clause : CATCH opt_catch_filter block",
				"$$98 :", "catch_clause : CATCH open_parens_any type opt_identifier CLOSE_PARENS $$98 opt_catch_filter_or_error", "catch_clause : CATCH open_parens_any error", "opt_catch_filter_or_error : opt_catch_filter block_prepared", "opt_catch_filter_or_error : error", "opt_catch_filter :", "$$99 :", "opt_catch_filter : WHEN $$99 open_parens_any expression CLOSE_PARENS", "checked_statement : CHECKED block", "unchecked_statement : UNCHECKED block",
				"$$100 :", "unsafe_statement : UNSAFE $$100 block", "lock_statement : LOCK open_parens_any expression CLOSE_PARENS embedded_statement", "lock_statement : LOCK open_parens_any expression error", "$$101 :", "$$102 :", "fixed_statement : FIXED open_parens_any variable_type identifier_inside_body $$101 using_or_fixed_variable_initializer opt_using_or_fixed_variable_declarators CLOSE_PARENS $$102 embedded_statement", "$$103 :", "$$104 :", "using_statement : USING open_parens_any variable_type identifier_inside_body $$103 using_initialization CLOSE_PARENS $$104 embedded_statement",
				"using_statement : USING open_parens_any expression CLOSE_PARENS embedded_statement", "using_statement : USING open_parens_any expression error", "using_initialization : using_or_fixed_variable_initializer opt_using_or_fixed_variable_declarators", "using_initialization : error", "using_or_fixed_variable_initializer :", "using_or_fixed_variable_initializer : ASSIGN variable_initializer", "query_expression : first_from_clause query_body", "query_expression : nested_from_clause query_body", "query_expression : first_from_clause COMPLETE_COMPLETION", "query_expression : nested_from_clause COMPLETE_COMPLETION",
				"first_from_clause : FROM_FIRST identifier_inside_body IN expression", "first_from_clause : FROM_FIRST type identifier_inside_body IN expression", "nested_from_clause : FROM identifier_inside_body IN expression", "nested_from_clause : FROM type identifier_inside_body IN expression", "$$105 :", "from_clause : FROM identifier_inside_body IN $$105 expression_or_error", "$$106 :", "from_clause : FROM type identifier_inside_body IN $$106 expression_or_error", "query_body : query_body_clauses select_or_group_clause opt_query_continuation", "query_body : select_or_group_clause opt_query_continuation",
				"query_body : query_body_clauses COMPLETE_COMPLETION", "query_body : query_body_clauses error", "query_body : error", "$$107 :", "select_or_group_clause : SELECT $$107 expression_or_error", "$$108 :", "$$109 :", "select_or_group_clause : GROUP $$108 expression_or_error $$109 by_expression", "by_expression : BY expression_or_error", "by_expression : error",
				"query_body_clauses : query_body_clause", "query_body_clauses : query_body_clauses query_body_clause", "query_body_clause : from_clause", "query_body_clause : let_clause", "query_body_clause : where_clause", "query_body_clause : join_clause", "query_body_clause : orderby_clause", "$$110 :", "let_clause : LET identifier_inside_body ASSIGN $$110 expression_or_error", "$$111 :",
				"where_clause : WHERE $$111 expression_or_error", "$$112 :", "$$113 :", "$$114 :", "join_clause : JOIN identifier_inside_body IN $$112 expression_or_error ON $$113 expression_or_error EQUALS $$114 expression_or_error opt_join_into", "$$115 :", "$$116 :", "$$117 :", "join_clause : JOIN type identifier_inside_body IN $$115 expression_or_error ON $$116 expression_or_error EQUALS $$117 expression_or_error opt_join_into", "opt_join_into :",
				"opt_join_into : INTO identifier_inside_body", "$$118 :", "orderby_clause : ORDERBY $$118 orderings", "orderings : order_by", "$$119 :", "orderings : order_by COMMA $$119 orderings_then_by", "orderings_then_by : then_by", "$$120 :", "orderings_then_by : orderings_then_by COMMA $$120 then_by", "order_by : expression",
				"order_by : expression ASCENDING", "order_by : expression DESCENDING", "then_by : expression", "then_by : expression ASCENDING", "then_by : expression DESCENDING", "opt_query_continuation :", "$$121 :", "opt_query_continuation : INTO identifier_inside_body $$121 query_body", "interactive_parsing : EVAL_STATEMENT_PARSER EOF", "interactive_parsing : EVAL_USING_DECLARATIONS_UNIT_PARSER using_directives opt_COMPLETE_COMPLETION",
				"$$122 :", "interactive_parsing : EVAL_STATEMENT_PARSER $$122 interactive_statement_list opt_COMPLETE_COMPLETION", "interactive_parsing : EVAL_COMPILATION_UNIT_PARSER interactive_compilation_unit", "interactive_compilation_unit : opt_extern_alias_directives opt_using_directives", "interactive_compilation_unit : opt_extern_alias_directives opt_using_directives namespace_or_type_declarations", "opt_COMPLETE_COMPLETION :", "opt_COMPLETE_COMPLETION : COMPLETE_COMPLETION", "close_brace_or_complete_completion : CLOSE_BRACE", "close_brace_or_complete_completion : COMPLETE_COMPLETION", "documentation_parsing : DOC_SEE doc_cref",
				"doc_cref : doc_type_declaration_name opt_doc_method_sig", "doc_cref : builtin_types opt_doc_method_sig", "doc_cref : VOID opt_doc_method_sig", "doc_cref : builtin_types DOT IDENTIFIER opt_doc_method_sig", "doc_cref : doc_type_declaration_name DOT THIS", "$$123 :", "doc_cref : doc_type_declaration_name DOT THIS OPEN_BRACKET $$123 opt_doc_parameters CLOSE_BRACKET", "doc_cref : EXPLICIT OPERATOR type opt_doc_method_sig", "doc_cref : IMPLICIT OPERATOR type opt_doc_method_sig", "doc_cref : OPERATOR overloadable_operator opt_doc_method_sig",
				"doc_type_declaration_name : type_declaration_name", "doc_type_declaration_name : doc_type_declaration_name DOT type_declaration_name", "opt_doc_method_sig :", "$$124 :", "opt_doc_method_sig : OPEN_PARENS $$124 opt_doc_parameters CLOSE_PARENS", "opt_doc_parameters :", "opt_doc_parameters : doc_parameters", "doc_parameters : doc_parameter", "doc_parameters : doc_parameters COMMA doc_parameter", "doc_parameter : opt_parameter_modifier parameter_type"
			};

			public static string getRule(int index)
			{
				return yyRule[index];
			}
		}

		private class OperatorDeclaration
		{
			public readonly Operator.OpType optype;

			public readonly FullNamedExpression ret_type;

			public readonly Location location;

			public OperatorDeclaration(Operator.OpType op, FullNamedExpression ret_type, Location location)
			{
				optype = op;
				this.ret_type = ret_type;
				this.location = location;
			}
		}

		private static readonly object ModifierNone = 0;

		private NamespaceContainer current_namespace;

		private TypeContainer current_container;

		private TypeDefinition current_type;

		private PropertyBase current_property;

		private EventProperty current_event;

		private EventField current_event_field;

		private FieldBase current_field;

		private Block current_block;

		private BlockVariable current_variable;

		private Delegate current_delegate;

		private AnonymousMethodExpression current_anonymous_method;

		private ParametersCompiled current_local_parameters;

		private bool parsing_anonymous_method;

		private bool async_block;

		private Stack<object> oob_stack;

		private int yacc_verbose_flag;

		public bool UnexpectedEOF;

		private readonly CompilationSourceFile file;

		private string tmpComment;

		private string enumTypeComment;

		private string current_attr_target;

		private ParameterModifierType valid_param_mod;

		private bool default_parameter_used;

		public Class InteractiveResult;

		public Undo undo;

		private bool? interactive_async;

		private Stack<QueryBlock> linq_clause_blocks;

		private ModuleContainer module;

		private readonly CompilerContext compiler;

		private readonly LanguageVersion lang_version;

		private readonly bool doc_support;

		private readonly CompilerSettings settings;

		private readonly Report report;

		private List<Parameter> parameters_bucket;

		private LocationsBag lbag;

		private List<Tuple<Modifiers, Location>> mod_locations;

		private Stack<Location> location_stack;

		public TextWriter ErrorOutput = Console.Out;

		public int eof_token;

		public yyDebug debug;

		protected const int yyFinal = 7;

		protected static readonly string[] yyNames = new string[435]
		{
			"end-of-file", null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, "EOF", "NONE", "ERROR",
			"FIRST_KEYWORD", "ABSTRACT", "AS", "ADD", "BASE", "BOOL", "BREAK", "BYTE", "CASE", "CATCH",
			"CHAR", "CHECKED", "CLASS", "CONST", "CONTINUE", "DECIMAL", "DEFAULT", "DELEGATE", "DO", "DOUBLE",
			"ELSE", "ENUM", "EVENT", "EXPLICIT", "EXTERN", "FALSE", "FINALLY", "FIXED", "FLOAT", "FOR",
			"FOREACH", "GOTO", "IF", "IMPLICIT", "IN", "INT", "INTERFACE", "INTERNAL", "IS", "LOCK",
			"LONG", "NAMESPACE", "NEW", "NULL", "OBJECT", "OPERATOR", "OUT", "OVERRIDE", "PARAMS", "PRIVATE",
			"PROTECTED", "PUBLIC", "READONLY", "REF", "RETURN", "REMOVE", "SBYTE", "SEALED", "SHORT", "SIZEOF",
			"STACKALLOC", "STATIC", "STRING", "STRUCT", "SWITCH", "THIS", "THROW", "TRUE", "TRY", "TYPEOF",
			"UINT", "ULONG", "UNCHECKED", "UNSAFE", "USHORT", "USING", "VIRTUAL", "VOID", "VOLATILE", "WHERE",
			"WHILE", "ARGLIST", "PARTIAL", "ARROW", "FROM", "FROM_FIRST", "JOIN", "ON", "EQUALS", "SELECT",
			"GROUP", "BY", "LET", "ORDERBY", "ASCENDING", "DESCENDING", "INTO", "INTERR_NULLABLE", "EXTERN_ALIAS", "REFVALUE",
			"REFTYPE", "MAKEREF", "ASYNC", "AWAIT", "INTERR_OPERATOR", "WHEN", "INTERPOLATED_STRING", "INTERPOLATED_STRING_END", "GET", "SET",
			"LAST_KEYWORD", "OPEN_BRACE", "CLOSE_BRACE", "OPEN_BRACKET", "CLOSE_BRACKET", "OPEN_PARENS", "CLOSE_PARENS", "DOT", "COMMA", "COLON",
			"SEMICOLON", "TILDE", "PLUS", "MINUS", "BANG", "ASSIGN", "OP_LT", "OP_GT", "BITWISE_AND", "BITWISE_OR",
			"STAR", "PERCENT", "DIV", "CARRET", "INTERR", "DOUBLE_COLON", "OP_INC", "OP_DEC", "OP_SHIFT_LEFT", "OP_SHIFT_RIGHT",
			"OP_LE", "OP_GE", "OP_EQ", "OP_NE", "OP_AND", "OP_OR", "OP_MULT_ASSIGN", "OP_DIV_ASSIGN", "OP_MOD_ASSIGN", "OP_ADD_ASSIGN",
			"OP_SUB_ASSIGN", "OP_SHIFT_LEFT_ASSIGN", "OP_SHIFT_RIGHT_ASSIGN", "OP_AND_ASSIGN", "OP_XOR_ASSIGN", "OP_OR_ASSIGN", "OP_PTR", "OP_COALESCING", "OP_GENERICS_LT", "OP_GENERICS_LT_DECL",
			"OP_GENERICS_GT", "LITERAL", "IDENTIFIER", "OPEN_PARENS_LAMBDA", "OPEN_PARENS_CAST", "GENERIC_DIMENSION", "DEFAULT_COLON", "OPEN_BRACKET_EXPR", "EVAL_STATEMENT_PARSER", "EVAL_COMPILATION_UNIT_PARSER",
			"EVAL_USING_DECLARATIONS_UNIT_PARSER", "DOC_SEE", "GENERATE_COMPLETION", "COMPLETE_COMPLETION", "UMINUS"
		};

		private int yyExpectingState;

		protected int yyMax;

		private static int[] global_yyStates;

		private static object[] global_yyVals;

		protected bool use_global_stacks;

		private object[] yyVals;

		private object yyVal;

		private int yyToken;

		private int yyTop;

		private static readonly short[] yyLhs = new short[1110]
		{
			-1, 0, 4, 0, 0, 1, 1, 1, 1, 2,
			2, 11, 11, 12, 12, 13, 13, 14, 15, 15,
			15, 16, 16, 20, 21, 18, 18, 23, 23, 23,
			19, 19, 19, 24, 24, 25, 25, 7, 7, 6,
			6, 22, 22, 8, 8, 26, 26, 26, 27, 27,
			27, 27, 27, 9, 9, 10, 10, 35, 33, 38,
			34, 34, 34, 34, 36, 36, 36, 37, 37, 42,
			39, 40, 41, 41, 43, 43, 43, 43, 43, 44,
			44, 44, 48, 45, 47, 51, 50, 50, 50, 53,
			53, 54, 54, 55, 55, 55, 55, 55, 55, 55,
			55, 55, 55, 55, 55, 55, 55, 69, 64, 74,
			76, 79, 80, 81, 29, 29, 84, 56, 56, 85,
			85, 86, 86, 87, 89, 83, 83, 88, 88, 94,
			57, 98, 57, 57, 93, 101, 93, 95, 95, 102,
			102, 103, 104, 103, 99, 99, 105, 105, 106, 107,
			97, 97, 100, 100, 100, 110, 58, 113, 114, 108,
			115, 116, 117, 108, 108, 108, 109, 109, 119, 119,
			122, 120, 112, 112, 123, 123, 123, 123, 123, 123,
			123, 123, 123, 123, 124, 124, 127, 127, 127, 127,
			130, 127, 128, 128, 131, 131, 132, 132, 132, 125,
			125, 125, 133, 133, 133, 126, 135, 137, 138, 140,
			59, 141, 59, 139, 143, 139, 142, 142, 145, 147,
			61, 146, 146, 136, 136, 136, 136, 136, 151, 148,
			152, 149, 150, 150, 150, 153, 154, 155, 157, 30,
			30, 156, 156, 158, 158, 159, 159, 159, 159, 159,
			159, 159, 159, 159, 161, 62, 162, 162, 165, 160,
			160, 164, 164, 164, 164, 164, 164, 164, 164, 164,
			164, 164, 164, 164, 164, 164, 164, 164, 164, 164,
			164, 164, 164, 164, 167, 166, 168, 166, 166, 166,
			63, 171, 173, 169, 170, 170, 172, 172, 177, 175,
			178, 175, 175, 175, 179, 65, 181, 60, 184, 185,
			60, 60, 180, 187, 180, 182, 182, 188, 188, 189,
			190, 189, 191, 186, 183, 183, 183, 183, 183, 195,
			192, 196, 193, 194, 194, 66, 67, 198, 200, 201,
			31, 197, 197, 197, 199, 199, 199, 202, 202, 203,
			204, 203, 203, 203, 205, 206, 207, 32, 208, 208,
			17, 17, 17, 209, 209, 209, 213, 213, 211, 211,
			211, 214, 214, 216, 73, 134, 111, 111, 144, 144,
			217, 217, 217, 215, 215, 218, 218, 219, 219, 221,
			221, 92, 82, 82, 96, 96, 129, 129, 163, 163,
			223, 223, 223, 222, 226, 226, 226, 228, 228, 229,
			227, 227, 227, 227, 227, 227, 227, 230, 230, 230,
			230, 230, 230, 230, 230, 230, 231, 231, 231, 231,
			231, 231, 231, 231, 231, 231, 231, 231, 231, 231,
			231, 231, 231, 231, 231, 231, 231, 232, 232, 232,
			233, 233, 233, 254, 254, 252, 252, 255, 255, 256,
			256, 257, 256, 258, 256, 259, 259, 260, 260, 235,
			235, 253, 253, 253, 253, 253, 253, 253, 253, 253,
			253, 253, 253, 237, 237, 237, 262, 262, 263, 263,
			264, 264, 266, 266, 266, 267, 267, 267, 267, 267,
			267, 267, 268, 268, 176, 176, 261, 261, 261, 261,
			261, 273, 273, 272, 272, 274, 274, 274, 274, 274,
			274, 276, 276, 276, 275, 238, 238, 238, 238, 271,
			271, 278, 278, 279, 279, 239, 240, 240, 241, 242,
			243, 243, 234, 234, 234, 234, 234, 284, 280, 244,
			244, 285, 285, 286, 286, 287, 287, 287, 287, 288,
			288, 288, 288, 281, 281, 224, 224, 283, 283, 289,
			289, 282, 282, 91, 91, 290, 290, 245, 291, 291,
			212, 210, 246, 246, 247, 247, 248, 248, 249, 293,
			250, 294, 250, 292, 292, 296, 295, 236, 297, 297,
			297, 297, 297, 297, 297, 297, 297, 298, 298, 298,
			298, 298, 298, 298, 298, 298, 298, 298, 298, 298,
			299, 299, 299, 299, 299, 299, 299, 300, 300, 300,
			300, 300, 300, 300, 300, 300, 300, 300, 300, 301,
			303, 303, 303, 303, 303, 303, 303, 303, 303, 304,
			305, 307, 307, 308, 309, 309, 306, 306, 310, 310,
			311, 311, 312, 312, 312, 312, 312, 313, 313, 313,
			313, 313, 313, 313, 313, 313, 314, 314, 314, 314,
			314, 315, 315, 315, 316, 316, 316, 317, 317, 317,
			318, 318, 318, 319, 319, 319, 320, 320, 321, 321,
			321, 321, 321, 322, 322, 322, 322, 322, 322, 322,
			322, 322, 322, 322, 323, 323, 324, 324, 324, 324,
			325, 325, 327, 326, 326, 326, 52, 52, 329, 328,
			330, 328, 331, 328, 332, 333, 328, 334, 335, 328,
			46, 46, 269, 269, 269, 269, 251, 251, 251, 90,
			337, 75, 75, 338, 339, 339, 339, 339, 341, 339,
			342, 343, 344, 345, 28, 72, 72, 71, 71, 118,
			118, 346, 346, 346, 346, 346, 346, 346, 346, 346,
			346, 346, 346, 346, 346, 346, 77, 77, 340, 340,
			78, 78, 347, 347, 348, 348, 349, 349, 350, 350,
			350, 350, 220, 220, 351, 351, 352, 121, 70, 70,
			353, 174, 68, 68, 354, 354, 355, 355, 355, 355,
			359, 359, 360, 360, 360, 357, 357, 357, 357, 357,
			357, 357, 357, 357, 357, 357, 357, 357, 361, 361,
			361, 361, 361, 361, 361, 361, 361, 361, 361, 361,
			361, 375, 375, 375, 375, 362, 376, 358, 277, 277,
			377, 377, 377, 377, 225, 225, 378, 49, 49, 380,
			356, 383, 356, 379, 379, 379, 381, 381, 387, 387,
			386, 386, 388, 388, 382, 382, 384, 384, 389, 389,
			390, 385, 385, 385, 363, 363, 363, 374, 374, 391,
			392, 392, 364, 364, 393, 393, 393, 396, 394, 394,
			395, 395, 397, 397, 397, 398, 399, 399, 400, 400,
			400, 365, 365, 365, 365, 401, 401, 402, 402, 402,
			406, 403, 409, 405, 405, 412, 408, 408, 411, 411,
			413, 413, 407, 407, 416, 415, 415, 410, 410, 414,
			414, 418, 417, 417, 404, 404, 419, 404, 366, 366,
			366, 366, 366, 366, 420, 421, 421, 422, 422, 422,
			423, 423, 423, 424, 424, 424, 425, 425, 425, 426,
			426, 367, 367, 367, 367, 427, 427, 302, 302, 428,
			431, 428, 428, 430, 430, 429, 432, 429, 368, 369,
			433, 372, 370, 370, 435, 436, 373, 438, 439, 371,
			371, 371, 437, 437, 434, 434, 336, 336, 336, 336,
			440, 440, 442, 442, 444, 443, 445, 443, 441, 441,
			441, 441, 441, 449, 447, 450, 452, 447, 451, 451,
			446, 446, 453, 453, 453, 453, 453, 458, 454, 459,
			455, 460, 461, 462, 456, 464, 465, 466, 456, 463,
			463, 468, 457, 467, 471, 467, 470, 473, 470, 469,
			469, 469, 472, 472, 472, 448, 474, 448, 3, 3,
			475, 3, 3, 476, 476, 270, 270, 265, 265, 5,
			477, 477, 477, 477, 477, 481, 477, 477, 477, 477,
			478, 478, 479, 482, 479, 480, 480, 483, 483, 484
		};

		private static readonly short[] yyLen = new short[1110]
		{
			2, 2, 0, 3, 1, 2, 4, 3, 1, 0,
			1, 1, 2, 4, 2, 1, 2, 1, 4, 6,
			2, 0, 1, 0, 0, 11, 3, 0, 1, 1,
			1, 3, 1, 0, 1, 0, 1, 0, 1, 0,
			1, 0, 1, 1, 2, 1, 1, 2, 1, 1,
			1, 1, 1, 0, 1, 1, 2, 0, 3, 0,
			6, 3, 2, 1, 1, 1, 1, 1, 3, 0,
			3, 1, 0, 3, 0, 1, 1, 3, 3, 1,
			1, 1, 0, 4, 4, 1, 0, 1, 1, 0,
			1, 1, 2, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 0, 4, 0,
			0, 0, 0, 0, 17, 5, 0, 9, 5, 0,
			1, 1, 2, 3, 0, 3, 1, 1, 1, 0,
			8, 0, 9, 6, 0, 0, 3, 0, 1, 1,
			2, 2, 0, 5, 0, 1, 1, 2, 3, 0,
			4, 2, 1, 1, 1, 0, 3, 0, 0, 10,
			0, 0, 0, 12, 8, 5, 1, 1, 1, 1,
			0, 4, 0, 1, 1, 3, 3, 3, 5, 3,
			5, 1, 1, 1, 1, 3, 4, 6, 2, 4,
			0, 7, 0, 1, 1, 2, 1, 1, 1, 4,
			6, 4, 1, 2, 2, 1, 0, 0, 0, 0,
			12, 0, 6, 0, 0, 4, 1, 1, 0, 0,
			10, 3, 1, 1, 2, 1, 2, 1, 0, 5,
			0, 5, 1, 1, 1, 0, 0, 0, 0, 15,
			5, 0, 1, 1, 2, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 0, 5, 1, 1, 0, 7,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 0, 7, 0, 7, 2, 2,
			2, 0, 0, 9, 1, 1, 0, 1, 0, 6,
			0, 6, 2, 1, 0, 8, 0, 9, 0, 0,
			10, 5, 0, 0, 3, 0, 1, 1, 2, 2,
			0, 5, 0, 2, 2, 2, 1, 1, 1, 0,
			5, 0, 5, 1, 1, 2, 4, 0, 0, 0,
			12, 0, 2, 2, 0, 1, 2, 1, 3, 2,
			0, 5, 3, 1, 0, 0, 0, 13, 0, 1,
			1, 3, 3, 1, 4, 4, 2, 2, 0, 3,
			2, 1, 3, 0, 3, 1, 1, 3, 1, 2,
			3, 4, 4, 0, 3, 1, 3, 3, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
			2, 2, 1, 1, 2, 2, 2, 1, 3, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 2, 1,
			1, 1, 1, 1, 1, 3, 1, 1, 3, 1,
			3, 0, 4, 0, 6, 1, 1, 1, 1, 3,
			3, 4, 4, 5, 4, 4, 4, 3, 3, 3,
			4, 3, 4, 4, 4, 3, 0, 1, 3, 4,
			0, 1, 1, 3, 2, 3, 3, 1, 2, 3,
			5, 2, 1, 1, 0, 1, 1, 3, 3, 3,
			2, 1, 1, 1, 1, 2, 2, 2, 2, 4,
			3, 3, 2, 4, 1, 4, 5, 4, 3, 1,
			3, 1, 3, 1, 1, 1, 4, 3, 2, 2,
			6, 3, 7, 4, 3, 7, 3, 0, 2, 4,
			3, 1, 2, 0, 1, 1, 3, 1, 2, 3,
			1, 1, 1, 0, 1, 1, 2, 2, 3, 1,
			2, 0, 1, 2, 4, 1, 3, 4, 1, 1,
			1, 2, 4, 4, 4, 2, 4, 2, 4, 0,
			4, 0, 5, 0, 1, 0, 4, 4, 1, 2,
			2, 4, 2, 2, 2, 4, 2, 1, 2, 2,
			2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
			1, 3, 3, 3, 3, 3, 3, 1, 3, 3,
			3, 3, 3, 4, 3, 3, 3, 3, 3, 1,
			1, 2, 2, 1, 1, 4, 1, 1, 1, 4,
			4, 1, 3, 3, 1, 2, 0, 1, 1, 3,
			1, 3, 1, 3, 3, 3, 3, 1, 3, 3,
			3, 3, 3, 3, 3, 3, 1, 3, 3, 3,
			3, 1, 3, 3, 1, 3, 3, 1, 3, 3,
			1, 3, 3, 1, 3, 3, 1, 3, 1, 5,
			4, 5, 5, 3, 3, 3, 3, 3, 3, 3,
			3, 3, 3, 3, 1, 3, 3, 2, 1, 1,
			0, 1, 0, 2, 1, 1, 1, 1, 0, 4,
			0, 4, 0, 5, 0, 0, 7, 0, 0, 8,
			1, 1, 1, 1, 1, 1, 6, 4, 4, 1,
			1, 0, 1, 3, 0, 1, 1, 2, 0, 6,
			0, 0, 0, 0, 15, 0, 1, 0, 1, 1,
			2, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 0, 1, 2, 3,
			0, 1, 1, 2, 4, 3, 1, 3, 1, 3,
			1, 1, 0, 1, 1, 1, 0, 4, 1, 1,
			0, 4, 0, 1, 1, 2, 1, 1, 1, 1,
			1, 2, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 0, 4, 1, 2,
			2, 2, 1, 1, 1, 2, 1, 1, 1, 0,
			6, 0, 7, 0, 2, 1, 0, 1, 0, 1,
			1, 2, 2, 4, 0, 2, 0, 1, 1, 2,
			4, 1, 5, 2, 2, 2, 2, 2, 2, 1,
			1, 1, 1, 1, 5, 7, 4, 0, 8, 4,
			0, 1, 1, 2, 1, 2, 1, 2, 3, 3,
			1, 1, 1, 1, 1, 5, 4, 7, 3, 6,
			0, 4, 0, 5, 1, 0, 4, 2, 2, 2,
			1, 1, 0, 1, 0, 5, 1, 0, 1, 0,
			1, 1, 1, 3, 4, 5, 0, 9, 1, 1,
			1, 1, 1, 1, 2, 2, 2, 3, 4, 3,
			3, 3, 2, 3, 3, 2, 4, 4, 3, 0,
			1, 3, 4, 5, 3, 1, 2, 0, 1, 3,
			0, 7, 3, 2, 1, 0, 0, 5, 2, 2,
			0, 3, 5, 4, 0, 0, 10, 0, 0, 9,
			5, 4, 2, 1, 0, 2, 2, 2, 2, 2,
			4, 5, 4, 5, 0, 5, 0, 6, 3, 2,
			2, 2, 1, 0, 3, 0, 0, 5, 2, 1,
			1, 2, 1, 1, 1, 1, 1, 0, 5, 0,
			3, 0, 0, 0, 12, 0, 0, 0, 13, 0,
			2, 0, 3, 1, 0, 4, 1, 0, 4, 1,
			2, 2, 1, 2, 2, 0, 0, 4, 2, 3,
			0, 4, 2, 2, 3, 0, 1, 1, 1, 2,
			2, 2, 2, 4, 3, 0, 7, 4, 4, 3,
			1, 3, 0, 0, 4, 0, 1, 1, 3, 2
		};

		private static readonly short[] yyDefRed = new short[1716]
		{
			0, 8, 0, 0, 0, 0, 0, 0, 0, 2,
			4, 0, 0, 11, 14, 0, 1078, 0, 0, 1082,
			0, 0, 15, 17, 412, 418, 425, 413, 415, 0,
			414, 0, 421, 423, 410, 0, 417, 419, 411, 422,
			424, 420, 0, 373, 1100, 0, 416, 1089, 0, 10,
			1, 0, 0, 0, 12, 0, 901, 0, 0, 0,
			0, 0, 0, 0, 0, 454, 0, 0, 0, 0,
			0, 0, 0, 452, 0, 0, 0, 535, 0, 453,
			0, 0, 0, 1000, 0, 0, 0, 745, 0, 0,
			0, 0, 0, 0, 0, 0, 456, 806, 0, 855,
			0, 0, 0, 0, 0, 0, 0, 0, 451, 0,
			734, 0, 900, 0, 838, 0, 447, 863, 862, 0,
			0, 0, 427, 428, 429, 430, 431, 432, 433, 434,
			435, 436, 437, 438, 439, 440, 441, 442, 443, 444,
			445, 446, 449, 450, 741, 0, 607, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 742,
			740, 743, 744, 822, 824, 0, 820, 823, 839, 841,
			842, 843, 844, 845, 846, 847, 848, 849, 850, 840,
			0, 0, 902, 903, 921, 922, 923, 924, 958, 959,
			960, 961, 962, 963, 0, 0, 0, 20, 22, 0,
			1086, 16, 1079, 0, 0, 266, 283, 265, 262, 267,
			268, 261, 280, 279, 272, 273, 269, 271, 270, 274,
			263, 264, 275, 276, 282, 281, 277, 278, 0, 1103,
			1092, 0, 0, 1091, 0, 1090, 3, 57, 0, 0,
			0, 46, 43, 45, 48, 49, 50, 51, 52, 55,
			13, 0, 0, 0, 964, 585, 465, 466, 998, 0,
			0, 0, 0, 0, 0, 0, 0, 966, 965, 0,
			595, 589, 594, 854, 899, 825, 852, 851, 853, 826,
			827, 828, 829, 830, 831, 832, 833, 834, 835, 836,
			837, 0, 0, 0, 930, 0, 0, 0, 868, 867,
			0, 0, 0, 0, 0, 0, 0, 0, 972, 0,
			0, 0, 0, 426, 0, 0, 0, 975, 0, 0,
			0, 0, 587, 999, 0, 0, 0, 866, 406, 0,
			0, 0, 0, 0, 0, 392, 360, 0, 363, 393,
			0, 402, 0, 0, 0, 0, 0, 0, 0, 737,
			0, 606, 0, 0, 730, 0, 0, 602, 0, 0,
			457, 0, 0, 604, 600, 614, 608, 615, 609, 603,
			599, 619, 613, 618, 612, 616, 610, 617, 611, 728,
			581, 0, 580, 448, 366, 367, 0, 0, 0, 0,
			0, 856, 0, 359, 0, 404, 405, 0, 0, 538,
			539, 0, 0, 0, 860, 861, 869, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			1081, 821, 859, 0, 897, 898, 1032, 1049, 0, 0,
			1033, 1035, 0, 1061, 1018, 1016, 1042, 0, 0, 1040,
			1043, 1044, 1045, 1046, 1019, 1017, 0, 0, 0, 0,
			0, 0, 1099, 0, 0, 374, 0, 0, 1101, 0,
			0, 44, 776, 782, 774, 0, 771, 781, 775, 773,
			772, 779, 777, 778, 784, 780, 783, 785, 0, 0,
			769, 47, 56, 537, 0, 533, 534, 0, 0, 531,
			0, 871, 0, 0, 0, 928, 0, 896, 894, 895,
			0, 0, 0, 749, 0, 969, 967, 750, 0, 0,
			562, 0, 0, 550, 557, 0, 0, 0, 551, 0,
			0, 567, 569, 0, 546, 0, 0, 0, 0, 0,
			541, 0, 544, 548, 395, 394, 971, 970, 0, 0,
			974, 973, 984, 0, 0, 0, 985, 579, 0, 389,
			578, 0, 0, 1001, 0, 0, 865, 0, 0, 400,
			401, 0, 0, 0, 399, 0, 0, 0, 620, 0,
			0, 591, 0, 732, 638, 637, 0, 0, 0, 461,
			0, 455, 819, 0, 0, 814, 816, 817, 818, 469,
			470, 0, 370, 371, 0, 197, 196, 198, 0, 719,
			0, 0, 0, 396, 0, 714, 0, 0, 978, 0,
			0, 0, 477, 478, 0, 481, 0, 0, 0, 0,
			479, 0, 0, 528, 0, 485, 0, 0, 0, 0,
			511, 514, 0, 0, 506, 513, 512, 0, 703, 704,
			705, 706, 707, 708, 709, 710, 711, 713, 712, 624,
			621, 626, 623, 625, 622, 635, 632, 636, 0, 0,
			646, 0, 0, 0, 0, 0, 639, 0, 634, 647,
			648, 630, 0, 631, 0, 665, 0, 0, 666, 0,
			672, 0, 673, 0, 674, 0, 675, 0, 679, 0,
			680, 0, 683, 0, 686, 0, 689, 0, 692, 0,
			695, 0, 697, 0, 566, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 1031, 1030, 0, 1041, 0, 1029,
			0, 18, 1097, 1098, 0, 0, 194, 0, 0, 1107,
			388, 0, 0, 0, 385, 1093, 1095, 63, 65, 66,
			0, 0, 58, 0, 0, 67, 69, 32, 30, 0,
			0, 0, 766, 0, 770, 475, 0, 536, 0, 584,
			0, 597, 183, 205, 0, 0, 0, 173, 0, 0,
			0, 184, 590, 0, 1004, 934, 0, 952, 931, 0,
			943, 0, 954, 0, 968, 906, 0, 1003, 0, 0,
			549, 0, 558, 568, 570, 0, 0, 0, 0, 497,
			0, 0, 492, 0, 0, 727, 726, 529, 0, 572,
			543, 0, 0, 154, 573, 152, 153, 575, 0, 583,
			582, 909, 0, 996, 0, 0, 982, 0, 986, 577,
			586, 1011, 0, 1007, 926, 0, 0, 1022, 0, 361,
			362, 1020, 0, 0, 747, 748, 0, 0, 0, 725,
			724, 731, 0, 476, 0, 0, 458, 808, 809, 807,
			815, 729, 0, 369, 717, 0, 0, 0, 605, 601,
			977, 976, 857, 482, 474, 0, 0, 480, 471, 472,
			588, 527, 525, 0, 524, 517, 518, 0, 515, 516,
			0, 510, 467, 468, 483, 484, 0, 875, 0, 0,
			641, 642, 0, 0, 0, 988, 633, 700, 0, 1050,
			1024, 0, 1051, 0, 1034, 1036, 1047, 0, 1062, 0,
			1028, 1076, 0, 1109, 195, 1104, 0, 805, 804, 0,
			803, 0, 384, 0, 62, 59, 0, 0, 0, 0,
			0, 0, 391, 0, 760, 0, 0, 88, 87, 0,
			532, 0, 0, 0, 0, 0, 188, 596, 0, 0,
			0, 0, 0, 944, 932, 0, 955, 0, 0, 1002,
			559, 556, 0, 501, 0, 0, 0, 1087, 1088, 488,
			494, 0, 498, 0, 0, 0, 0, 0, 0, 907,
			0, 992, 0, 989, 983, 1010, 0, 925, 364, 365,
			1023, 1021, 0, 592, 0, 733, 723, 463, 462, 372,
			716, 715, 735, 473, 526, 0, 0, 520, 0, 509,
			508, 507, 0, 891, 874, 0, 0, 0, 880, 0,
			0, 0, 651, 0, 0, 654, 0, 660, 0, 658,
			701, 702, 699, 0, 1026, 0, 1055, 0, 0, 1070,
			1071, 1064, 0, 19, 1108, 387, 386, 0, 0, 68,
			61, 0, 70, 31, 24, 0, 0, 337, 0, 240,
			0, 115, 0, 84, 85, 885, 127, 128, 0, 0,
			0, 888, 203, 204, 0, 0, 0, 0, 176, 185,
			177, 179, 929, 0, 0, 0, 0, 0, 953, 0,
			0, 502, 503, 496, 499, 495, 0, 489, 493, 0,
			564, 0, 530, 540, 487, 576, 574, 0, 0, 0,
			1013, 0, 0, 746, 738, 0, 0, 521, 0, 519,
			0, 0, 870, 881, 645, 0, 650, 0, 0, 655,
			649, 0, 1025, 0, 0, 0, 1039, 0, 1037, 1048,
			0, 1077, 1096, 0, 81, 0, 0, 75, 76, 79,
			80, 0, 354, 343, 342, 0, 761, 236, 110, 0,
			872, 889, 189, 0, 201, 0, 0, 0, 927, 1015,
			0, 0, 0, 948, 0, 0, 956, 905, 0, 545,
			542, 914, 0, 920, 0, 0, 912, 0, 916, 0,
			990, 1012, 1008, 0, 464, 736, 523, 0, 0, 653,
			652, 661, 659, 1027, 1052, 0, 1038, 0, 0, 1066,
			0, 82, 73, 0, 0, 0, 338, 0, 0, 0,
			0, 0, 190, 0, 180, 178, 1005, 945, 933, 941,
			940, 935, 937, 0, 500, 0, 908, 913, 0, 917,
			997, 0, 0, 739, 0, 883, 0, 1056, 1073, 1074,
			1067, 60, 0, 77, 78, 0, 0, 0, 0, 0,
			0, 0, 755, 0, 787, 0, 752, 890, 187, 0,
			200, 0, 0, 957, 919, 918, 994, 0, 991, 1009,
			892, 0, 0, 0, 83, 0, 0, 355, 0, 0,
			353, 339, 0, 347, 0, 409, 0, 407, 0, 0,
			762, 0, 792, 237, 0, 191, 1006, 936, 0, 0,
			950, 810, 993, 1053, 0, 1068, 0, 0, 0, 335,
			0, 0, 753, 789, 0, 758, 0, 0, 793, 0,
			111, 939, 938, 0, 0, 1057, 29, 28, 25, 356,
			352, 0, 0, 348, 408, 0, 795, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 34, 340, 0, 800,
			0, 801, 798, 0, 796, 106, 107, 0, 103, 0,
			0, 91, 93, 94, 95, 96, 97, 98, 99, 100,
			101, 102, 104, 105, 155, 0, 0, 253, 245, 246,
			247, 248, 249, 250, 251, 252, 0, 0, 243, 112,
			811, 0, 1054, 0, 357, 351, 759, 0, 0, 0,
			0, 763, 92, 0, 295, 290, 294, 0, 238, 244,
			0, 1060, 1058, 799, 797, 0, 0, 0, 0, 0,
			0, 0, 0, 304, 0, 0, 254, 0, 0, 260,
			0, 170, 169, 156, 166, 167, 168, 0, 0, 0,
			108, 0, 0, 289, 0, 0, 288, 0, 160, 0,
			0, 378, 336, 0, 376, 0, 0, 0, 0, 0,
			0, 0, 0, 764, 0, 239, 113, 118, 116, 311,
			0, 375, 0, 0, 0, 0, 131, 0, 0, 0,
			0, 0, 0, 165, 157, 0, 0, 0, 218, 0,
			379, 0, 255, 0, 0, 0, 0, 308, 0, 286,
			133, 0, 284, 0, 0, 0, 135, 0, 380, 0,
			0, 207, 212, 0, 0, 0, 377, 258, 171, 114,
			126, 124, 0, 0, 313, 0, 0, 0, 0, 0,
			161, 0, 292, 0, 0, 0, 0, 139, 0, 0,
			0, 0, 381, 382, 0, 0, 0, 0, 0, 121,
			328, 0, 309, 0, 0, 322, 0, 0, 0, 317,
			0, 151, 0, 0, 0, 0, 146, 0, 0, 305,
			0, 136, 0, 130, 140, 158, 164, 227, 0, 208,
			0, 0, 219, 0, 125, 0, 117, 122, 0, 0,
			0, 324, 0, 325, 314, 0, 0, 307, 318, 287,
			0, 0, 132, 147, 285, 0, 303, 0, 293, 297,
			142, 0, 0, 0, 224, 226, 0, 259, 123, 329,
			331, 310, 0, 0, 323, 320, 150, 148, 162, 302,
			0, 0, 0, 159, 228, 230, 209, 0, 222, 220,
			0, 0, 322, 0, 298, 300, 143, 0, 0, 0,
			0, 333, 334, 330, 332, 321, 163, 0, 0, 234,
			233, 232, 229, 231, 214, 210, 221, 0, 0, 0,
			299, 301, 216, 217, 0, 215
		};

		protected static readonly short[] yyDgoto = new short[485]
		{
			7, 8, 50, 9, 51, 10, 11, 52, 238, 784,
			785, 12, 13, 53, 22, 23, 199, 332, 241, 769,
			960, 1181, 1316, 1368, 1691, 957, 242, 243, 244, 245,
			246, 247, 248, 249, 762, 479, 763, 764, 1078, 765,
			766, 1082, 958, 1176, 1177, 1178, 274, 651, 1282, 113,
			969, 1093, 827, 1399, 1400, 1401, 1402, 1403, 1404, 1405,
			1406, 1407, 1408, 1409, 1410, 1411, 1412, 1413, 603, 1439,
			879, 498, 773, 1494, 1092, 1295, 1249, 1293, 1330, 1380,
			1450, 1535, 1325, 1562, 1536, 1587, 1588, 1589, 1095, 1585,
			1096, 836, 961, 1547, 1521, 1575, 553, 1568, 1541, 1604,
			1043, 1573, 1576, 1577, 1672, 1605, 1606, 1602, 1414, 1473,
			1443, 1495, 786, 1549, 1651, 1518, 1608, 1683, 499, 1474,
			1475, 275, 1504, 787, 788, 789, 790, 791, 744, 621,
			1299, 745, 746, 975, 1497, 1526, 1619, 1580, 1653, 1705,
			1689, 1527, 1714, 1709, 1498, 1553, 1679, 1656, 1620, 1621,
			1702, 1687, 1688, 1090, 1248, 1359, 1426, 1478, 1427, 1428,
			1466, 1501, 1467, 335, 228, 1584, 1469, 1569, 1566, 1415,
			1445, 1490, 1648, 1610, 1342, 1649, 652, 1697, 1698, 1489,
			1565, 1538, 1597, 1592, 1563, 1629, 1634, 1595, 1598, 1599,
			1682, 1635, 1593, 1594, 1693, 1680, 1681, 1087, 1185, 1321,
			1287, 1350, 1322, 1323, 1371, 1245, 1347, 1384, 395, 336,
			115, 384, 385, 116, 614, 475, 231, 1513, 753, 754,
			949, 962, 117, 340, 442, 328, 341, 312, 1326, 1327,
			46, 120, 313, 122, 123, 124, 125, 126, 127, 128,
			129, 130, 131, 132, 133, 134, 135, 136, 137, 138,
			139, 140, 141, 142, 143, 359, 360, 875, 1145, 259,
			914, 832, 1133, 1122, 820, 999, 821, 822, 1123, 144,
			202, 828, 654, 655, 656, 905, 906, 145, 508, 509,
			305, 1131, 830, 443, 307, 537, 538, 539, 540, 543,
			838, 571, 271, 514, 866, 272, 513, 146, 147, 148,
			149, 1054, 926, 1055, 689, 690, 1056, 1051, 1052, 1057,
			1058, 1059, 150, 151, 152, 153, 154, 155, 156, 157,
			158, 159, 160, 624, 625, 626, 871, 872, 161, 611,
			596, 868, 386, 1146, 592, 1223, 162, 528, 1290, 1291,
			1294, 1375, 1088, 1247, 1357, 1470, 500, 1331, 1332, 1393,
			1394, 950, 361, 1363, 604, 605, 276, 277, 278, 165,
			166, 167, 279, 280, 281, 282, 283, 284, 285, 286,
			287, 288, 289, 290, 179, 291, 631, 180, 329, 919,
			657, 1046, 972, 780, 1099, 1044, 1047, 1201, 1048, 1100,
			1101, 292, 181, 182, 183, 1214, 1137, 1215, 1216, 1217,
			1218, 184, 185, 186, 187, 798, 521, 799, 1204, 1117,
			1205, 1337, 1302, 1262, 1338, 800, 1116, 801, 1340, 1263,
			188, 189, 190, 191, 192, 193, 314, 565, 566, 845,
			1308, 1271, 1010, 325, 1115, 982, 1301, 1142, 1016, 1272,
			194, 455, 195, 456, 1063, 1163, 457, 458, 739, 730,
			731, 1168, 1067, 459, 460, 461, 462, 463, 1068, 725,
			1065, 1276, 1364, 1432, 1165, 1312, 1383, 938, 733, 939,
			1238, 1170, 1239, 1313, 1072, 17, 19, 47, 48, 230,
			747, 953, 473, 748, 749
		};

		protected static readonly short[] yySindex = new short[1716]
		{
			-104, 0, -145, -119, 138, 196, 19722, 0, 313, 0,
			0, 196, 138, 0, 0, 255, 0, 8816, 196, 0,
			250, -32, 0, 0, 0, 0, 0, 0, 0, 273,
			0, 292, 0, 0, 0, 12227, 0, 0, 0, 0,
			0, 0, 278, 0, 0, 560, 0, 0, 832, 0,
			0, 313, 296, 196, 0, 360, 0, 342, 397, -171,
			19138, -110, -251, 316, 8977, 0, -251, -251, -251, 86,
			-251, -251, -6, 0, 11168, -251, -251, 0, 11329, 0,
			422, -251, -160, 0, -251, 448, -251, 0, 9477, 9477,
			669, -251, -251, 57, 11492, 18023, 0, 0, 18023, 0,
			12543, 12678, 12813, 12948, 13083, 13218, 13353, 13488, 0, 192,
			0, 10243, 0, 247, 0, -35, 0, 0, 0, 467,
			476, 432, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, -35, 0, 1040, 907, 245,
			614, 682, 815, 474, 483, 608, 651, -272, 673, 0,
			0, 0, 0, 0, 0, 4218, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			726, 185, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 172, 310, 296, 0, 0, 717,
			0, 0, 0, 10243, 10243, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 278, 0,
			0, 697, 722, 0, -209, 0, 0, 0, 296, 12849,
			898, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 901, -35, 18163, 0, 0, 0, 0, 0, 18023,
			-189, -143, 877, 795, 555, 476, -35, 0, 0, 10243,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 293, 231, 19138, 0, 10243, 18023, 818, 0, 0,
			844, 18023, 18023, 7064, 735, -134, 821, 10243, 0, 11492,
			192, 975, 881, 0, 870, 10243, 18023, 0, 1008, 930,
			480, 1429, 0, 0, 18023, 422, 17463, 0, 0, 448,
			18023, 636, 570, 984, -35, 0, 0, 905, 0, 0,
			726, 0, 432, 1025, -35, 18023, 18023, 18023, 316, 0,
			987, 0, 10243, 10243, 0, 12408, -35, 0, 913, 939,
			0, 9138, 79, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 1582, 0, 0, 0, 0, 19602, 636, 969, 970,
			18023, 0, 660, 0, 182, 0, 0, 290, 187, 0,
			0, 938, 11627, 9460, 0, 0, 0, 18023, 18023, 18023,
			18023, 18023, 18023, 18023, 18023, 18023, 18023, 18023, 13623, 13758,
			13893, 4963, 16188, 14028, 14163, 14298, 14433, 14568, 14703, 14838,
			14973, 15108, 15243, 15378, 15513, 15648, 15783, 15918, 18583, 18023,
			0, 0, 0, 726, 0, 0, 0, 0, 9477, 9477,
			0, 0, -35, 0, 0, 0, 0, 326, 1018, 0,
			0, 0, 0, 0, 0, 0, 296, 898, 530, 387,
			278, 278, 0, 737, -100, 0, 278, 991, 0, -188,
			12849, 0, 0, 0, 0, -121, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 331, 12984,
			0, 0, 0, 0, 992, 0, 0, 1022, 764, 0,
			1035, 0, 1036, 217, 422, 0, -251, 0, 0, 0,
			-35, 10387, -154, 0, 1033, 0, 0, 0, -174, 122,
			0, 795, 555, 0, 0, 1031, 0, 1048, 0, 1043,
			993, 0, 0, 771, 0, 9740, 782, 11788, 821, 17323,
			0, 9904, 0, 0, 0, 0, 0, 0, 126, 168,
			0, 0, 0, 275, 422, 388, 0, 0, 448, 0,
			0, 1046, 1051, 0, 177, -35, 0, 197, 1007, 0,
			0, 18023, 1136, 660, 0, 18023, 1137, 1054, 0, 1058,
			1059, 0, 19602, 0, 0, 0, 91, 992, 18023, 0,
			18023, 0, 0, -255, 9138, 0, 0, 0, 0, 0,
			0, 91, 0, 0, 376, 0, 0, 0, 448, 0,
			636, -35, 10404, 0, 1060, 0, 1061, 16053, 0, 1180,
			1064, 9138, 0, 0, 1009, 0, 992, -35, 18163, 1013,
			0, 660, 992, 0, 210, 0, 17603, 17603, 1065, 1183,
			0, 0, 269, -180, 0, 0, 0, -114, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 18711, 18711,
			0, 10243, 639, 0, 0, 0, 0, -35, 0, 0,
			0, 0, 907, 0, 907, 0, 12273, 245, 0, 245,
			0, 614, 0, 614, 0, 614, 0, 614, 0, 682,
			0, 682, 0, 815, 0, 474, 0, 483, 0, 608,
			0, 651, 0, -42, 0, 11788, 1162, -35, 1163, -35,
			11788, 11788, 1074, 18023, 0, 0, 1018, 0, -35, 0,
			1039, 0, 0, 0, 10404, 737, 0, 1086, 1085, 0,
			0, 69, 296, 443, 0, 0, 0, 0, 0, 0,
			-196, 1088, 0, 1087, 1089, 0, 0, 0, 0, 1093,
			11024, 1049, 0, 439, 0, 0, 789, 0, 18163, 0,
			1090, 0, 0, 0, 734, 61, 1098, 0, 1099, 1100,
			1101, 0, 0, 18023, 0, 0, -35, 0, 0, 1096,
			0, 1103, 0, -129, 0, 0, 8977, 0, 8977, 10548,
			0, 16407, 0, 0, 0, 10711, 10846, 551, 17323, 0,
			-23, 58, 0, 1050, 1055, 0, 0, 0, 812, 0,
			0, 1108, 1111, 0, 0, 0, 0, 0, 1114, 0,
			0, 0, 1115, 0, 5452, 422, 0, 422, 0, 0,
			0, 0, 8977, 0, 0, 8977, 660, 0, 18023, 0,
			0, 0, 18023, 10243, 0, 0, 422, 1117, 91, 0,
			0, 0, 18023, 0, 1116, 1073, 0, 0, 0, 0,
			0, 0, 10243, 0, 0, -35, 19602, 1153, 0, 0,
			0, 0, 0, 0, 0, 992, 830, 0, 0, 0,
			0, 0, 0, 17603, 0, 0, 0, -35, 0, 0,
			17183, 0, 0, 0, 0, 0, 10065, 0, 10226, 1119,
			0, 0, 1122, -35, 18839, 0, 0, 0, 11007, 0,
			0, 1210, 0, 1211, 0, 0, 0, 960, 0, 1128,
			0, 0, 501, 0, 0, 0, 737, 0, 0, 1091,
			0, -100, 0, 737, 0, 0, 1039, 1133, 1134, 1097,
			1139, 1049, 0, 1129, 0, 1255, 1256, 0, 0, 11788,
			0, 17743, 1140, 734, 10404, 10243, 0, 0, 434, 1265,
			1266, 203, 1138, 0, 0, 18023, 0, 18023, 1244, 0,
			0, 0, 17883, 0, 519, 17883, 838, 0, 0, 0,
			0, 9600, 0, 1271, 726, 11788, 1157, 10548, 1158, 0,
			-251, 0, -35, 0, 0, 0, -67, 0, 0, 0,
			0, 0, 1155, 0, 1186, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 1156, 1151, 0, 856, 0,
			0, 0, 10243, 0, 0, -35, 1159, 1119, 0, 18711,
			1242, 685, 0, 487, -35, 0, 1165, 0, 1164, 0,
			0, 0, 0, 11788, 0, 11788, 0, 63, 11788, 0,
			0, 0, 481, 0, 0, 0, 0, 1171, 1039, 0,
			0, 11949, 0, 0, 0, 1177, 5615, 0, 1049, 0,
			1049, 0, 1049, 0, 0, 0, 0, 0, -35, 1168,
			1140, 0, 0, 0, -144, -120, 1175, 1181, 0, 0,
			0, 0, 0, 1178, 10548, 1119, -114, 18023, 0, 1179,
			8977, 0, 0, 0, 0, 0, 1182, 0, 0, 1188,
			0, 821, 0, 0, 0, 0, 0, -179, 18023, 1184,
			0, 1119, 1187, 0, 0, 1144, 91, 0, 18023, 0,
			1142, 1185, 0, 0, 0, 18948, 0, -35, 18948, 0,
			0, 18839, 0, 11788, 1219, 11788, 0, 11788, 0, 0,
			18023, 0, 0, 1089, 0, 566, 903, 0, 0, 0,
			0, 138, 0, 0, 0, 1200, 0, 0, 0, 1190,
			0, 0, 0, 502, 0, 1191, 1316, 1323, 0, 0,
			1119, 1204, 1119, 0, 8977, 665, 0, 0, 17883, 0,
			0, 0, 18023, 0, 1209, -195, 0, 8653, 0, 1206,
			0, 0, 0, 91, 0, 0, 0, 18023, 10226, 0,
			0, 0, 0, 0, 0, 1236, 0, 979, 1208, 0,
			1213, 0, 0, 11949, 196, 217, 0, 873, 1205, 1214,
			17743, 1216, 0, 18023, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 8977, 0, 99, 0, 0, 9138, 0,
			0, -69, 8977, 0, 1217, 0, 11788, 0, 0, 0,
			0, 0, 18023, 0, 0, 296, 1212, 296, 217, 10243,
			1205, 1253, 0, 1253, 0, 1205, 0, 0, 0, 18023,
			0, 8977, 18023, 0, 0, 0, 0, 1222, 0, 0,
			0, 1247, 11788, 18023, 0, 296, 1224, 0, 1176, 982,
			0, 0, 1221, 0, 1225, 0, 113, 0, 1227, 1189,
			0, 1253, 0, 0, 1253, 0, 0, 0, 879, 1103,
			0, 0, 0, 0, 1252, 0, 41, 1253, 1357, 0,
			1243, 296, 0, 0, 10243, 0, 103, 1245, 0, 1246,
			0, 0, 0, 9138, 11788, 0, 0, 0, 0, 0,
			0, 1229, 1239, 0, 0, 17323, 0, 19790, -161, 296,
			1249, 1251, 1268, 11788, 1248, 18023, 0, 0, 1257, 0,
			1250, 0, 0, 1254, 0, 0, 0, 12984, 0, 1259,
			-161, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, -237, 12984, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 1263, 296, 0, 0,
			0, -35, 0, 1268, 0, 0, 0, 1260, 19790, 9138,
			19381, 0, 0, 606, 0, 0, 0, 19449, 0, 0,
			-161, 0, 0, 0, 0, -255, 10243, 10243, 258, 10243,
			415, 448, 1290, 0, 636, 17084, 0, 1324, 0, 0,
			1239, 0, 0, 0, 0, 0, 0, 17119, 1239, 1267,
			0, -117, -116, 0, 10243, -81, 0, 10243, 0, 1215,
			1269, 0, 0, 156, 0, 115, 1081, 0, 1270, 1220,
			174, 606, 12227, 0, 18023, 0, 0, 0, 0, 0,
			156, 0, 1275, 1226, 1272, 1261, 0, 1278, 1232, 1281,
			217, 1273, 1280, 0, 0, 1284, 1292, 1318, 0, 992,
			0, 952, 0, 1289, 1285, 1239, -62, 0, 1282, 0,
			0, 1293, 0, 1294, 1296, 1297, 0, 1301, 0, 217,
			217, 0, 0, 217, 1291, 1298, 0, 0, 0, 0,
			0, 0, 1304, 221, 0, 1305, 217, 1428, 1308, 217,
			0, -208, 0, 10548, 1276, 1307, 1301, 0, 1312, 1313,
			249, 1319, 0, 0, 217, 17743, 1279, 1310, 1304, 0,
			0, 12984, 0, 296, 296, 0, 1283, 1315, 1305, 0,
			1321, 0, 18023, 1288, 1320, 1308, 0, 1326, 217, 0,
			116, 0, 1322, 0, 0, 0, 0, 0, 12984, 0,
			249, 249, 0, 1327, 0, -62, 0, 0, 270, 1339,
			12984, 0, 12984, 0, 0, 10548, 1328, 0, 0, 0,
			1342, 1293, 0, 0, 0, 1343, 0, 282, 0, 0,
			0, 1253, 1004, 1346, 0, 0, -211, 0, 0, 0,
			0, 0, 1406, 1459, 0, 0, 0, 0, 0, 0,
			1350, 1351, 10548, 0, 0, 0, 0, 249, 0, 0,
			652, 652, 0, 1253, 0, 0, 0, 112, 112, 1345,
			1355, 0, 0, 0, 0, 0, 0, 17323, 17323, 0,
			0, 0, 0, 0, 0, 0, 0, 1352, 1356, 17743,
			0, 0, 0, 0, 1354, 0
		};

		protected static readonly short[] yyRindex = new short[1716]
		{
			3692, 0, 0, 9299, 3692, 0, 0, 0, 1735, 0,
			0, 3833, 2158, 0, 0, 0, 0, 0, 3833, 0,
			1314, 51, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 1737, 0, 0, 1737, 0, 0, 1737, 0,
			0, 1735, 3880, 3739, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 1367, 0, 0, 0, 0, 0, 0,
			0, 0, 11185, 0, 1359, 0, 0, 0, 1359, 0,
			0, 0, 0, 0, 0, 2220, 0, 0, 0, 0,
			0, 0, 0, 0, 301, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 5840,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 6373,
			5777, 4376, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 6481, 6661, 7066,
			7345, 1617, 7985, 8129, 8273, 8417, 5065, 1888, 4598, 0,
			0, 0, 0, 0, 0, 51, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			6553, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 3943, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 1737, 0,
			0, 66, 0, 0, 0, 0, 0, 0, 3986, 662,
			4029, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 4636, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 1368, 0, 0, 0, 0, 0, 0,
			4799, 1362, 0, 0, 0, 0, 0, 0, 1362, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 3121,
			0, 95, 3271, 0, 0, 0, 0, 0, 0, 0,
			3421, 0, 3271, 0, 0, 0, 0, 0, 1367, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 1015, 0,
			0, 143, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 1370, 1990, 0, 0,
			1359, 0, 4636, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 272, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 2354, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 4761, 0,
			0, 0, 0, 0, 0, 0, 4092, 4139, 650, 0,
			1737, 1737, 0, 9921, 45, 0, 1737, 1743, 0, 0,
			158, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 457, 19062,
			0, 0, 0, 0, 4636, 0, 0, 0, 0, 0,
			0, 0, 0, 19489, 0, 0, 0, 0, 0, 0,
			0, 1364, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 253, 755, 0, 0, 264, 850, 0, 0, 1376,
			751, 0, 0, 0, 0, 228, 0, 0, 5288, 1373,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 1381, 0, 2670, 0, 0, 311, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 2971, 0, 0, 0, 0, 0, 0,
			0, 0, 1370, 0, 0, 0, 18303, 4636, 0, 0,
			0, 0, 0, 0, 265, 0, 0, 0, 0, 0,
			0, 18303, 0, 0, 0, 0, 0, 0, 87, 0,
			648, 0, 0, 0, 1379, 0, 0, 0, 0, 1362,
			0, 0, 0, 0, 4473, 0, 4636, 0, 0, 4309,
			0, 4636, 5451, 0, 0, 0, 0, 0, -173, 0,
			0, 0, 0, 279, 0, 0, 0, 922, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 5937, 6046, 6155, 6264, 0, 6733, 0, 0,
			0, 0, 6841, 0, 6913, 0, 0, 7172, 0, 7244,
			0, 7417, 0, 7518, 0, 7590, 0, 7691, 0, 7841,
			0, 7913, 0, 8057, 0, 8201, 0, 8345, 0, 8489,
			0, 5228, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 4761, 0, 0, 0,
			0, 0, 0, 0, 0, 11805, 0, 0, 927, 0,
			0, 1334, 16573, 0, 0, 0, 0, 0, 0, 0,
			799, 701, 0, 0, 1384, 0, 0, 0, 0, 2525,
			0, 0, 0, 0, 0, 0, 12110, 0, 0, 0,
			929, 0, 0, 0, 11966, 19642, 0, 0, 944, 948,
			949, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 1382, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 1392, 0, 0, 0, 0, 0, 7750, 0, 0,
			0, 271, 0, 97, 4962, 0, 0, 0, 0, 0,
			0, 0, 1389, 0, 0, 0, 0, 0, 1396, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 2971, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 18303, 0,
			0, 0, 0, 0, 1020, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 4636, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 1390,
			0, 0, 0, 0, 1393, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, -182, 0, 357,
			0, 0, 0, 0, 0, 0, 11966, 0, 0, 0,
			0, 45, 0, 10082, 0, 0, 1398, 0, 882, 0,
			0, 0, 0, 1402, 0, 1358, 1361, 0, 0, 0,
			0, 0, 1394, 17200, 0, 0, 0, 0, 19682, 0,
			0, 0, 950, 0, 0, 0, 0, 0, 2841, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 5125, 0, 5614, 1404, 0, 0,
			0, 0, 1403, 0, 0, 0, 950, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, -153, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 1401, 0, 0,
			0, 0, 0, 2493, 746, 0, 0, 0, 1409, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 961, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			1407, 0, 0, 0, 0, 0, 964, 967, 0, 0,
			0, 0, 0, 0, 0, 1410, 922, 716, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 5288, 0, 0, 0, 0, 0, 1416, 0, 0,
			0, 1410, 0, 0, 0, 0, 18303, 0, 0, 0,
			825, 859, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 1384, 0, 16408, 0, 0, 0, 0,
			0, 19838, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 791, 0, 867, 0, 0, 0, 0,
			1413, 0, 1390, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 1418, 0, 0, 0, 0,
			0, 0, 0, 18303, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, -170, 395, 0,
			0, 0, 0, 0, 19881, 19489, 0, 545, 624, 471,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, -139, 0,
			0, 1381, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 19924, 0, 384, 19489, 0,
			629, 1420, 0, 1420, 0, 624, 0, 0, 0, 0,
			0, 0, 891, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 19967, 0, 0, 0, 16846,
			0, 0, 1423, 0, 0, 0, 567, 0, 632, 0,
			0, 621, 0, 0, 1420, 0, 0, 0, 0, 897,
			0, 0, 0, 0, 0, 0, 3786, 1417, 688, 0,
			0, 407, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 1431, 0, 0, 0, 0, 0, 0,
			0, 0, 3590, 0, 0, 1373, 0, 0, 16682, 16928,
			0, 0, 465, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 601, 0, 0, 0, 19245, 0, 0,
			16764, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 19313, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 17010, 0, 0,
			0, 0, 0, 465, 0, 0, 0, 0, 0, 143,
			457, 0, 0, 0, 0, 0, 0, 457, 0, 0,
			16682, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 5774, 459, 0, 1799, 0, 0, 0, 17052, 0,
			3590, 0, 0, 0, 0, 0, 0, 0, 3590, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, -1, 0, 525, 0, 663, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			709, 0, 725, 0, 0, 0, 0, 0, 0, 0,
			19489, 966, 0, 0, 0, 0, 0, 0, 0, 1424,
			0, 546, 0, 0, 0, 3590, 0, 0, 971, 0,
			0, 0, 0, 0, 0, 0, 0, 1425, 0, 19489,
			19489, 0, 0, 19529, 0, 0, 0, 0, 0, 0,
			0, 0, 1426, 2200, 0, 1427, 19489, 18443, 1430, 19489,
			0, 0, 0, 0, 0, 0, 1433, 0, 0, 0,
			20047, 0, 0, 0, 19489, 0, 0, 0, 1435, 0,
			0, 332, 0, 1767, 12714, 0, 0, 0, 1436, 0,
			0, 0, 0, 0, 0, 1437, 0, 0, 19489, 0,
			694, 0, 983, 0, 0, 0, 0, 0, 1026, 0,
			18984, 20009, 0, 0, 0, 0, 0, 0, 0, 0,
			1485, 0, 1541, 0, 0, 0, 989, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 622, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 20047, 0, 0,
			12444, 12579, 0, 622, 0, 0, 0, 0, 0, 16318,
			0, 0, 0, 0, 0, 0, 0, 1373, 1373, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};

		protected static readonly short[] yyGindex = new short[485]
		{
			0, 0, 1758, 0, 0, 0, 4, -14, -184, -47,
			-43, 0, 1808, 1806, 869, 0, 0, -168, 0, 0,
			0, 0, 0, 0, -1285, -793, -222, -627, 0, 0,
			0, 0, 0, -229, 0, 0, 0, 744, 0, 872,
			0, 0, 0, 0, 580, 582, -17, -221, 0, -46,
			0, 0, 813, 379, 0, 426, -685, -649, -584, -581,
			-560, -525, -520, -514, 0, 0, -1190, 0, -1292, 0,
			375, -1297, 0, 78, 0, 0, 0, 536, -1243, 0,
			0, 0, 431, 209, 0, 0, 0, 248, -1197, 0,
			-279, -305, -332, 0, 0, 0, -1002, 194, 0, 0,
			-537, 0, 0, 261, 0, 0, 234, 0, 0, 339,
			0, -529, -1137, 0, 0, 0, 0, 0, -483, 274,
			-1439, -10, 0, 0, 0, 863, 866, 875, 1062, -567,
			0, 0, -334, 883, 368, 0, -896, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			176, 0, 0, 0, 0, 0, 0, 0, 0, 433,
			0, 0, 0, -306, 361, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 444, 0, -539, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 183, 0, 0, 268,
			0, 0, 277, 281, 181, 0, 0, 0, 0, 0,
			0, 0, 0, 517, 0, 0, 0, 0, -84, 0,
			337, -367, -359, 1446, 0, 341, 0, -449, 0, 924,
			0, 1558, 1173, -304, -277, -83, 201, 868, 0, 526,
			0, -41, 854, -398, 0, 0, -387, 0, 0, 0,
			0, 0, 0, 0, 0, 0, -364, 0, 0, 0,
			0, 0, 0, -264, 0, 0, 1299, 0, 0, 39,
			0, -362, 0, -278, 0, 0, 0, 884, -941, -319,
			-135, 1067, 0, 968, 0, 1258, -583, 38, -343, 1112,
			0, 0, 758, 1819, 0, 0, 0, 0, 1082, 0,
			0, 0, 1544, 0, 0, 0, 0, 0, 1503, 973,
			974, 1474, -124, 1475, 0, 0, 0, 0, 745, 29,
			0, 740, 858, 977, 1470, 1472, 1473, 1471, 1477, 0,
			1479, 0, 0, 0, 1023, 1332, -577, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, -309, 671, 0,
			-886, 0, 0, 0, 0, 0, -479, 0, 611, 0,
			473, 0, 0, 0, 727, -575, -15, -348, -11, 0,
			1778, 0, 62, 0, 75, 101, 111, 124, 127, 167,
			171, 173, 175, 180, 0, -762, 0, 0, 0, 829,
			0, 747, 0, 0, 0, 718, -328, 806, -972, 0,
			848, -499, 0, 0, 0, 0, 0, 0, 736, 0,
			733, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 653, 0, 0,
			0, 0, 0, 0, 0, 0, -30, 0, 1387, 686,
			0, 0, 0, 0, 937, 0, 0, 0, 0, 0,
			0, -177, 0, 0, 0, 0, 0, 1501, 1223, 0,
			0, 0, 0, 1505, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 527, 0, 0, 0, 0, 0, 0,
			0, 0, 664, 0, 0, 0, 0, 0, 0, 17,
			1028, 0, 0, 0, 1029
		};

		protected static readonly short[] yyTable = new short[20417]
		{
			112, 552, 163, 555, 196, 239, 164, 114, 18, 240,
			831, 502, 466, 607, 837, 569, 481, 524, 465, 265,
			774, 577, 797, 300, 683, 632, 587, 550, 548, 880,
			440, 469, 506, 633, 881, 684, 396, 404, 405, 536,
			1150, 653, 333, 343, 988, 1008, 989, 350, 319, 258,
			1333, 1085, 622, 1297, 1125, 885, 892, 311, 685, 644,
			954, 318, 233, 584, 909, 235, 383, 255, 757, 392,
			320, 1381, 323, 1212, 1069, 1153, 915, 1211, 358, 168,
			623, 362, 805, 745, 44, 255, 1072, 1387, 1552, 1212,
			1015, 1360, 169, 1017, 758, 1395, 322, 1320, 266, 406,
			1440, 269, 802, 522, 1369, 293, 294, 295, 1286, 301,
			302, 14, 1192, 322, 315, 316, 477, 877, 170, 1447,
			321, 324, 544, 326, 256, 330, 759, 986, 171, 915,
			346, 347, 1471, 437, 1341, 767, 1194, 775, 16, 1507,
			1509, 172, 917, 1444, 173, 438, 267, 1455, 112, 239,
			163, 1324, 1, 467, 164, 114, 750, 1069, 6, 403,
			1677, 1320, 1069, 97, 1069, 987, 724, 1069, 1069, 1072,
			1069, 1069, 1472, 257, 1072, 1515, 1072, 943, 878, 1072,
			1072, 506, 1072, 1072, 174, 1503, 256, 1306, 175, 1140,
			176, 480, 177, 1505, 1560, 467, 1069, 178, 916, 380,
			97, 745, 806, 745, 256, 745, 504, 507, 1072, 298,
			1396, 97, 237, 43, 927, 256, 859, 1678, 118, 298,
			511, 522, 381, 522, 860, 522, 823, 168, 1153, 382,
			873, 1213, 256, 915, 760, 257, 505, 545, 502, 546,
			169, 256, 510, 829, 481, 472, 576, 1213, 579, 580,
			1559, 1069, 265, 257, 2, 373, 607, 535, 622, 396,
			745, 118, 265, 1072, 257, 118, 170, 1264, 299, 894,
			268, 918, 990, 237, 898, 900, 171, 15, 299, 523,
			522, 257, 899, 607, 527, 529, 623, 915, 582, 172,
			257, 1025, 173, 547, 1628, 896, 843, 1366, 586, 559,
			324, 768, 1195, 20, 403, 1508, 1510, 572, 1085, 574,
			597, 761, 478, 527, 1000, 573, 623, 976, 1114, 1166,
			1035, 1652, 383, 1561, 3, 4, 5, 6, 298, 589,
			590, 520, 174, 1662, 348, 1663, 175, 928, 176, 53,
			177, 1516, 373, 397, 549, 178, 606, 869, 636, 997,
			608, 53, 641, 1085, 296, 1304, 507, 507, 1207, 1376,
			630, 1292, 297, 947, 575, 303, 118, 304, 1699, 1353,
			373, 1523, 1646, 629, 373, 948, 368, 129, 807, 129,
			1240, 265, 839, 1545, 129, 505, 650, 299, 1624, 867,
			658, 659, 660, 661, 662, 663, 664, 665, 666, 667,
			668, 200, 726, 728, 1328, 383, 732, 1104, 1673, 383,
			998, 944, 1578, 1579, 1167, 6, 1581, 506, 373, 239,
			298, 1367, 723, 467, 841, 337, 337, 751, 446, 1600,
			767, 752, 1607, 851, 237, 767, 1001, 383, 623, 767,
			1696, 383, 1258, 383, 383, 383, 383, 1623, 337, 298,
			397, 383, 368, 854, 767, 609, 996, 1485, 368, 1112,
			686, 397, 97, 397, 569, 397, 901, 53, 368, 1085,
			1135, 1645, 368, 782, 794, 1085, 803, 1590, 1305, 299,
			349, 767, 1377, 97, 981, 368, 1118, 742, 743, 1018,
			1524, 1354, 1700, 755, 118, 1647, 2, 1019, 808, 1530,
			767, 1303, 840, 507, 792, 1617, 197, 421, 299, 397,
			1309, 447, 610, 389, 1483, 812, 448, 368, 449, 334,
			344, 450, 451, 502, 452, 453, 683, 118, 1033, 853,
			826, 20, 650, 1659, 835, 379, 337, 684, 1669, 1336,
			337, 337, 388, 422, 842, 298, 1670, 536, 1038, 515,
			298, 380, 622, 852, 846, 793, 502, 506, 783, 796,
			685, 390, 118, 1484, 857, 444, 446, 868, 861, 1225,
			49, 198, 942, 855, 381, 884, 812, 1199, 203, 1113,
			623, 874, 734, 358, 902, 1660, 870, 380, 778, 606,
			237, 895, 507, 608, 237, 767, 1531, 204, 404, 405,
			490, 870, 844, 517, 634, 454, 337, 1671, 770, 639,
			381, 518, 771, 1063, 635, 868, 606, 382, 445, 640,
			608, 505, 237, 118, 383, 868, 391, 423, 424, 904,
			904, 868, 337, 516, 470, 471, 560, 813, 868, 1102,
			843, 925, 560, 491, 337, 912, 1273, 767, 504, 447,
			256, 1065, 337, 229, 448, 505, 449, 563, 337, 450,
			451, 490, 452, 453, 519, 447, 1097, 637, 623, 237,
			448, 1486, 449, 772, 847, 450, 451, 55, 452, 453,
			868, 931, 823, 933, 907, 907, 868, 390, 1002, 337,
			337, 270, 941, 880, 1418, 1040, 1063, 560, 813, 257,
			512, 1063, 913, 1063, 491, 504, 1063, 1063, 826, 1063,
			1063, 964, 505, 826, 826, 251, 937, 638, 337, 252,
			1487, 1059, 118, 337, 1654, 1655, 522, 1130, 1134, 765,
			1419, 766, 507, 390, 1065, 965, 562, 446, 555, 1065,
			250, 1065, 1418, 464, 1065, 1065, 558, 1065, 1065, 563,
			983, 1398, 1417, 765, 882, 766, 344, 683, 337, 735,
			683, 505, 966, 683, 578, 535, 564, 741, 684, 253,
			265, 684, 507, 1398, 684, 1106, 527, 254, 1419, 346,
			765, 1690, 766, 594, 595, 337, 337, 1200, 761, 393,
			1063, 685, 835, 97, 685, 1420, 883, 685, 1421, 826,
			1417, 650, 383, 797, 1059, 118, 53, 237, 1203, 1059,
			751, 1059, 613, 1200, 1059, 1059, 337, 1059, 1059, 1422,
			447, 951, 327, 1398, 393, 448, 829, 449, 1065, 53,
			450, 451, 118, 452, 453, 1013, 1388, 1014, 327, 1030,
			397, 1020, 751, 1420, 394, 1021, 1421, 118, 118, 963,
			751, 256, 676, 398, 1423, 1026, 1023, 327, 870, 1424,
			1180, 1036, 433, 952, 507, 1425, 1158, 1422, 375, 607,
			507, 121, 399, 400, 45, 1251, 434, 1050, 578, 727,
			729, 1073, 380, 265, 754, 119, 362, 1252, 1139, 383,
			201, 1124, 401, 650, 379, 1171, 375, 1005, 1059, 650,
			257, 835, 1423, 402, 751, 381, 788, 1424, 752, 379,
			761, 1062, 382, 1425, 121, 740, 754, 383, 121, 383,
			607, 383, 201, 368, 383, 380, 383, 393, 119, 337,
			1159, 383, 119, 1265, 767, 229, 995, 232, 788, 767,
			794, 907, 788, 767, 794, 1097, 380, 578, 381, 1471,
			380, 1241, 826, 1511, 523, 382, 342, 342, 767, 337,
			327, 380, 686, 786, 791, 790, 925, 1525, 756, 381,
			1119, 757, 794, 381, 1300, 1121, 382, 97, 1121, 342,
			382, 794, 1496, 383, 381, 767, 1472, 383, 826, 1543,
			835, 382, 791, 790, 1496, 786, 393, 435, 383, 1151,
			756, 791, 790, 757, 767, 368, 211, 118, 925, 118,
			923, 368, 425, 426, 924, 607, 1500, 774, 337, 121,
			1335, 368, 1180, 97, 718, 368, 718, 368, 1500, 327,
			368, 380, 1386, 119, 206, 507, 1611, 1259, 368, 1085,
			615, 1260, 973, 615, 345, 1261, 826, 616, 826, 1138,
			616, 826, 1189, 118, 381, 436, 118, 1156, 1193, 617,
			349, 382, 617, 1157, 1179, 296, 349, 439, 427, 428,
			368, 342, 342, 350, 296, 71, 71, 337, 381, 71,
			373, 337, 429, 430, 373, 382, 368, 373, 947, 373,
			502, 607, 947, 481, 373, 967, 947, 835, 1664, 304,
			527, 1315, 968, 306, 118, 306, 1435, 337, 1465, 541,
			306, 1050, 922, 542, 265, 1477, 474, 265, 987, 368,
			265, 1219, 987, 555, 987, 118, 1522, 867, 373, 555,
			368, 1226, 368, 867, 1468, 1686, 870, 342, 777, 468,
			867, 1468, 778, 1522, 476, 813, 826, 121, 826, 814,
			826, 368, 368, 1237, 348, 555, 824, 503, 1707, 1708,
			542, 119, 1554, 342, 1555, 186, 1186, 186, 1187, 186,
			1188, 368, 356, 368, 368, 342, 368, 368, 64, 368,
			121, 337, 368, 342, 1229, 1244, 1004, 1231, 867, 342,
			1005, 1121, 551, 686, 119, 523, 686, 507, 525, 686,
			337, 893, 606, 893, 1034, 893, 608, 229, 778, 234,
			1274, 835, 1126, 870, 449, 121, 916, 431, 432, 337,
			342, 342, 561, 337, 526, 449, 1179, 449, 561, 119,
			1285, 556, 1149, 523, 916, 882, 523, 882, 239, 882,
			1318, 199, 467, 199, 1319, 199, 449, 449, 1288, 342,
			557, 1361, 1289, 606, 342, 1362, 72, 608, 394, 826,
			72, 339, 339, 949, 560, 1314, 449, 949, 239, 951,
			501, 237, 467, 951, 449, 1012, 682, 449, 581, 1242,
			1097, 1243, 523, 561, 339, 701, 703, 705, 707, 342,
			119, 598, 599, 337, 1022, 826, 1237, 418, 419, 420,
			873, 1106, 873, 1106, 1318, 600, 601, 884, 1319, 884,
			561, 337, 337, 1029, 1069, 1070, 342, 342, 174, 585,
			174, 118, 181, 182, 181, 182, 1014, 583, 1014, 507,
			593, 1397, 1416, 1278, 1279, 1319, 752, 74, 205, 74,
			205, 175, 482, 175, 134, 627, 134, 826, 606, 312,
			628, 312, 608, 1397, 1349, 237, 118, 1319, 650, 118,
			642, 141, 118, 141, 756, 483, 826, 319, 523, 319,
			381, 474, 1674, 1675, 738, 121, 339, 339, 484, 337,
			1416, 459, 459, 486, 752, 1451, 460, 460, 487, 119,
			488, 489, 490, 491, 767, 767, 692, 694, 492, 697,
			699, 776, 493, 1397, 1713, 118, 1105, 1319, 709, 711,
			381, 779, 781, 804, 494, 337, 809, 495, 118, 496,
			810, 811, 849, 337, 606, 407, 812, 850, 608, 856,
			858, 862, 863, 1476, 864, 865, 890, 887, 886, 911,
			910, 893, 339, 497, 891, 897, 408, 409, 410, 411,
			412, 413, 414, 415, 416, 417, 930, 932, 121, 936,
			342, 387, 945, 946, 118, 578, 955, 956, 339, 118,
			959, 43, 119, 118, 977, 971, 984, 978, 979, 980,
			554, 985, 1003, 200, 1006, 121, 1009, 1534, 339, 916,
			342, 1476, 1007, 1024, 1028, 1027, 1032, 1045, 1049, 119,
			121, 121, 118, 1510, 1064, 1066, 1071, 1080, 1086, 1081,
			1084, 1089, 1091, 1075, 119, 119, 1591, 1184, 1098, 1083,
			752, 1110, 1111, 1114, 1120, 339, 339, 1129, 545, 1144,
			1136, 1143, 1147, 1618, 338, 338, 1148, 752, 929, 1152,
			1155, 1160, 1161, 934, 935, 1172, 1630, 1632, 1190, 342,
			752, 752, 1182, 1196, 339, 1206, 835, 338, 1198, 1197,
			1220, 1476, 1209, 1222, 118, 1224, 1234, 1208, 523, 1227,
			1228, 1246, 1254, 1618, 1618, 1250, 1253, 752, 752, 1255,
			1256, 1266, 1270, 1277, 1289, 1640, 1280, 1281, 1317, 1288,
			1298, 1310, 1329, 1341, 339, 1343, 1346, 357, 1348, 1351,
			1365, 1352, 1355, 364, 366, 368, 370, 372, 374, 376,
			378, 1356, 342, 1370, 1385, 1372, 1378, 1379, 835, 1386,
			1429, 339, 339, 1430, 1431, 1437, 337, 1488, 1434, 1502,
			1618, 1441, 1438, 1436, 752, 1448, 1453, 1519, 342, 1506,
			118, 1540, 1529, 1528, 1520, 338, 1537, 1539, 1531, 338,
			338, 507, 507, 1542, 1510, 835, 1544, 1548, 1546, 1550,
			121, 1471, 121, 1551, 1557, 1558, 1567, 1564, 1582, 1570,
			1692, 1692, 1571, 1572, 119, 1583, 119, 1701, 1701, 1574,
			650, 650, 1586, 1596, 1601, 567, 1603, 1613, 1615, 1616,
			1626, 337, 1712, 1622, 24, 1637, 25, 1639, 1612, 26,
			1642, 1625, 1644, 1657, 27, 1636, 121, 1650, 28, 121,
			1641, 1661, 342, 1665, 337, 338, 1666, 30, 1676, 1668,
			119, 1660, 1659, 119, 32, 1684, 1685, 1706, 1710, 33,
			1704, 342, 1711, 34, 1715, 9, 21, 1102, 593, 979,
			553, 338, 980, 1094, 942, 36, 720, 37, 554, 504,
			342, 38, 995, 338, 342, 721, 802, 121, 35, 39,
			40, 338, 946, 41, 552, 505, 568, 338, 35, 656,
			876, 119, 36, 341, 886, 337, 36, 337, 682, 987,
			235, 877, 1094, 109, 337, 657, 878, 887, 910, 879,
			911, 790, 119, 337, 337, 345, 337, 790, 338, 338,
			767, 368, 1499, 812, 767, 137, 119, 315, 1392, 236,
			144, 21, 357, 138, 1499, 120, 316, 145, 1132, 1499,
			54, 337, 1173, 1283, 337, 1284, 1442, 338, 1079, 1479,
			1480, 1334, 338, 1499, 1658, 1667, 1627, 1614, 612, 1643,
			1532, 1107, 342, 342, 1108, 1609, 974, 24, 588, 25,
			1512, 387, 26, 1109, 339, 1499, 1103, 27, 357, 1446,
			1449, 28, 1694, 1533, 1703, 1695, 1638, 338, 1373, 1392,
			30, 1633, 1556, 676, 1631, 1076, 1162, 32, 1164, 570,
			1374, 1169, 33, 994, 1041, 1128, 34, 1481, 1482, 1210,
			970, 306, 591, 991, 338, 338, 687, 688, 36, 876,
			37, 1232, 1230, 713, 38, 908, 715, 719, 717, 1031,
			342, 1454, 39, 40, 721, 1514, 41, 722, 1517, 85,
			1296, 670, 672, 674, 867, 338, 588, 588, 588, 588,
			588, 588, 588, 588, 588, 588, 588, 588, 588, 588,
			588, 588, 1358, 441, 1268, 1202, 1275, 1221, 1191, 1257,
			1269, 1267, 848, 1141, 342, 1339, 676, 1307, 736, 940,
			1452, 676, 737, 676, 676, 676, 676, 676, 676, 676,
			676, 676, 676, 676, 121, 1074, 1233, 1345, 1235, 0,
			1236, 1077, 0, 676, 676, 0, 0, 0, 119, 676,
			368, 676, 0, 676, 0, 676, 676, 676, 0, 0,
			0, 0, 0, 0, 387, 676, 676, 0, 0, 682,
			676, 676, 682, 0, 0, 682, 0, 339, 0, 676,
			676, 676, 676, 119, 0, 0, 119, 0, 53, 119,
			0, 0, 0, 0, 676, 0, 339, 0, 338, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 588, 0,
			676, 53, 0, 0, 0, 339, 0, 0, 121, 0,
			368, 0, 0, 0, 53, 0, 0, 0, 338, 53,
			0, 121, 119, 0, 53, 0, 53, 53, 53, 53,
			0, 0, 53, 368, 53, 119, 0, 0, 53, 1311,
			0, 0, 0, 0, 0, 0, 368, 0, 0, 0,
			53, 368, 0, 53, 368, 53, 368, 0, 368, 368,
			368, 368, 0, 0, 0, 0, 368, 121, 0, 0,
			368, 0, 121, 0, 368, 1344, 121, 338, 0, 53,
			889, 119, 368, 0, 0, 368, 119, 368, 0, 326,
			119, 0, 0, 0, 696, 0, 0, 0, 339, 0,
			0, 0, 0, 0, 0, 121, 368, 342, 40, 0,
			0, 368, 0, 0, 0, 0, 0, 0, 0, 119,
			0, 368, 368, 0, 291, 0, 368, 1382, 0, 0,
			0, 920, 921, 0, 0, 0, 338, 0, 0, 368,
			338, 0, 0, 0, 0, 0, 1433, 0, 0, 357,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 554, 338, 121, 0, 0,
			403, 368, 342, 0, 0, 0, 0, 696, 0, 0,
			0, 119, 696, 0, 696, 696, 696, 696, 696, 696,
			696, 696, 696, 696, 696, 342, 368, 0, 0, 0,
			0, 0, 368, 0, 696, 696, 0, 0, 0, 339,
			696, 0, 696, 0, 696, 0, 696, 696, 696, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 696, 0, 0, 0, 0, 0, 368, 0,
			338, 0, 0, 121, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 342, 119, 342, 338,
			0, 0, 0, 0, 0, 342, 0, 0, 357, 0,
			0, 696, 0, 0, 342, 342, 0, 342, 338, 368,
			0, 0, 338, 368, 368, 0, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 368, 0, 0,
			0, 0, 342, 368, 565, 342, 368, 368, 0, 0,
			0, 368, 368, 368, 368, 368, 368, 368, 368, 368,
			368, 0, 368, 368, 0, 0, 368, 368, 368, 368,
			368, 0, 0, 368, 368, 0, 0, 0, 368, 368,
			368, 368, 368, 368, 368, 368, 0, 0, 0, 0,
			0, 0, 338, 0, 0, 0, 0, 368, 0, 0,
			368, 0, 368, 0, 368, 40, 0, 368, 0, 40,
			338, 338, 0, 368, 0, 0, 0, 0, 0, 0,
			40, 0, 0, 0, 0, 40, 0, 0, 0, 40,
			0, 0, 40, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 40, 40, 0, 0, 0, 40,
			40, 53, 339, 53, 0, 40, 0, 40, 40, 40,
			40, 0, 0, 0, 0, 40, 403, 0, 0, 40,
			0, 40, 403, 0, 53, 0, 0, 0, 338, 0,
			0, 40, 0, 40, 40, 0, 40, 53, 0, 0,
			40, 0, 53, 0, 588, 0, 0, 53, 0, 53,
			53, 53, 53, 0, 0, 53, 0, 53, 403, 0,
			40, 53, 0, 0, 338, 26, 0, 339, 0, 0,
			40, 40, 338, 53, 0, 0, 53, 0, 53, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			339, 0, 1154, 0, 0, 0, 0, 0, 0, 403,
			0, 0, 53, 403, 403, 0, 403, 403, 403, 403,
			403, 403, 403, 403, 403, 403, 403, 0, 0, 0,
			0, 0, 0, 403, 0, 0, 403, 403, 0, 0,
			0, 403, 403, 403, 403, 403, 403, 0, 403, 403,
			403, 0, 403, 403, 0, 0, 403, 403, 403, 403,
			565, 339, 0, 403, 403, 565, 565, 0, 403, 403,
			403, 403, 403, 403, 403, 403, 0, 0, 0, 339,
			339, 0, 554, 0, 0, 0, 0, 403, 565, 0,
			403, 0, 403, 0, 403, 0, 0, 403, 0, 0,
			0, 565, 565, 403, 0, 0, 565, 339, 0, 565,
			339, 565, 0, 565, 565, 565, 565, 0, 0, 0,
			981, 565, 0, 0, 0, 565, 0, 0, 0, 565,
			0, 0, 0, 0, 0, 0, 0, 565, 0, 0,
			565, 0, 565, 565, 0, 0, 0, 565, 565, 0,
			565, 565, 565, 565, 565, 565, 565, 565, 565, 565,
			565, 0, 0, 0, 0, 0, 565, 565, 565, 0,
			565, 565, 0, 0, 0, 565, 565, 0, 565, 565,
			565, 565, 565, 565, 565, 338, 565, 565, 0, 565,
			565, 565, 565, 565, 565, 565, 565, 565, 565, 0,
			565, 565, 565, 565, 565, 565, 565, 565, 565, 565,
			565, 565, 565, 565, 565, 565, 565, 565, 565, 565,
			565, 565, 0, 0, 565, 0, 565, 0, 565, 0,
			0, 565, 26, 0, 0, 0, 26, 565, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 26, 0, 0,
			338, 0, 26, 0, 0, 0, 26, 0, 0, 26,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 26, 26, 338, 0, 0, 26, 26, 0, 0,
			0, 0, 26, 0, 26, 26, 26, 26, 0, 0,
			0, 904, 26, 0, 0, 0, 26, 0, 26, 0,
			368, 0, 0, 0, 0, 0, 368, 368, 26, 0,
			0, 26, 0, 26, 368, 0, 368, 26, 368, 368,
			368, 368, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 368, 338, 0, 338, 26, 0, 368,
			368, 0, 0, 338, 0, 0, 23, 26, 26, 0,
			0, 0, 338, 338, 0, 338, 0, 0, 0, 368,
			0, 0, 0, 0, 0, 368, 0, 368, 0, 0,
			368, 0, 0, 0, 0, 0, 981, 981, 0, 0,
			338, 0, 0, 338, 981, 981, 981, 981, 981, 0,
			981, 981, 0, 981, 981, 981, 981, 981, 981, 981,
			981, 0, 0, 0, 0, 981, 0, 981, 981, 981,
			981, 981, 981, 0, 0, 981, 0, 0, 0, 981,
			981, 368, 981, 981, 981, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 981, 0, 981, 0, 981, 981,
			0, 0, 981, 0, 981, 981, 981, 981, 981, 981,
			981, 981, 981, 981, 981, 981, 0, 981, 0, 0,
			981, 981, 0, 0, 981, 981, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 981,
			981, 981, 981, 981, 0, 0, 981, 981, 0, 0,
			0, 981, 981, 0, 0, 981, 0, 0, 0, 0,
			981, 981, 981, 981, 981, 0, 0, 0, 981, 0,
			981, 0, 0, 0, 0, 0, 981, 981, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 981, 981, 981, 981, 0, 981, 904, 904, 0,
			0, 0, 0, 981, 0, 904, 904, 904, 904, 904,
			0, 904, 904, 0, 904, 904, 904, 904, 904, 904,
			904, 864, 0, 0, 0, 0, 904, 0, 904, 904,
			904, 904, 904, 904, 0, 0, 904, 0, 0, 0,
			904, 904, 0, 904, 904, 904, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 904, 0, 904, 0, 904,
			904, 0, 0, 904, 0, 904, 904, 904, 904, 904,
			904, 904, 904, 904, 904, 904, 904, 0, 904, 0,
			0, 904, 904, 0, 0, 904, 904, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			904, 904, 904, 904, 904, 0, 0, 904, 904, 0,
			0, 0, 904, 904, 0, 0, 904, 0, 0, 0,
			0, 904, 904, 904, 904, 904, 0, 368, 0, 904,
			0, 904, 368, 368, 0, 0, 0, 904, 904, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 368, 0, 0, 0, 0,
			0, 0, 904, 904, 904, 904, 0, 904, 368, 368,
			0, 358, 0, 368, 904, 0, 368, 0, 368, 0,
			368, 368, 368, 368, 0, 0, 0, 0, 368, 0,
			0, 0, 368, 0, 0, 0, 368, 0, 0, 0,
			0, 0, 0, 0, 368, 0, 0, 368, 0, 368,
			368, 0, 0, 0, 368, 368, 0, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 368, 368, 0,
			0, 0, 0, 368, 368, 0, 0, 368, 368, 0,
			0, 0, 368, 368, 368, 368, 368, 368, 368, 368,
			368, 368, 0, 368, 368, 0, 0, 368, 368, 368,
			368, 368, 0, 0, 368, 368, 0, 0, 0, 368,
			368, 368, 368, 368, 368, 368, 368, 864, 0, 0,
			0, 0, 864, 864, 0, 0, 0, 0, 368, 0,
			0, 368, 0, 368, 0, 368, 0, 0, 368, 0,
			0, 0, 0, 0, 368, 864, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 864, 864,
			0, 398, 0, 864, 0, 0, 864, 0, 864, 0,
			864, 864, 864, 864, 0, 0, 0, 0, 864, 0,
			0, 0, 864, 0, 0, 0, 864, 0, 0, 0,
			0, 0, 0, 0, 864, 0, 0, 864, 0, 864,
			864, 0, 0, 0, 864, 864, 0, 864, 864, 864,
			864, 864, 864, 864, 864, 864, 864, 864, 0, 0,
			0, 0, 0, 864, 864, 0, 0, 864, 864, 0,
			0, 0, 864, 864, 864, 864, 864, 864, 0, 864,
			864, 864, 0, 864, 864, 0, 0, 864, 864, 864,
			864, 0, 0, 0, 864, 864, 0, 0, 0, 864,
			864, 864, 864, 864, 864, 864, 864, 358, 0, 0,
			0, 0, 358, 358, 0, 0, 0, 0, 864, 0,
			0, 864, 0, 864, 0, 864, 0, 0, 864, 0,
			0, 0, 0, 0, 864, 358, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 358, 358,
			0, 0, 0, 358, 0, 0, 358, 0, 358, 0,
			358, 358, 358, 358, 0, 0, 0, 0, 358, 0,
			33, 0, 358, 0, 0, 0, 358, 0, 0, 0,
			0, 0, 0, 0, 358, 0, 0, 358, 0, 358,
			358, 0, 0, 0, 358, 358, 0, 358, 358, 358,
			358, 358, 358, 358, 358, 358, 358, 358, 0, 0,
			0, 0, 0, 358, 358, 0, 0, 358, 358, 0,
			0, 0, 358, 358, 358, 358, 358, 358, 0, 358,
			358, 358, 0, 358, 358, 0, 0, 358, 358, 358,
			358, 0, 0, 0, 358, 358, 0, 0, 0, 358,
			358, 358, 358, 358, 358, 358, 358, 398, 0, 0,
			0, 0, 398, 398, 0, 0, 0, 0, 358, 0,
			0, 358, 39, 358, 0, 358, 0, 0, 358, 0,
			0, 0, 0, 0, 358, 398, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 398, 398,
			0, 0, 0, 398, 0, 0, 398, 0, 398, 0,
			398, 398, 398, 398, 0, 0, 0, 0, 398, 38,
			0, 0, 398, 0, 0, 0, 398, 0, 0, 0,
			0, 0, 0, 0, 398, 0, 0, 398, 0, 398,
			398, 0, 0, 0, 398, 398, 0, 398, 398, 398,
			398, 398, 398, 398, 398, 398, 398, 398, 0, 0,
			0, 0, 0, 398, 398, 0, 27, 398, 398, 0,
			0, 0, 398, 398, 0, 398, 398, 398, 0, 398,
			398, 398, 0, 398, 398, 0, 0, 398, 398, 398,
			398, 0, 0, 0, 398, 398, 0, 0, 0, 398,
			398, 398, 398, 398, 398, 398, 398, 0, 0, 0,
			0, 0, 0, 37, 0, 0, 0, 0, 398, 0,
			0, 398, 0, 398, 0, 0, 33, 33, 0, 0,
			0, 33, 0, 0, 398, 33, 0, 33, 0, 0,
			33, 0, 33, 33, 0, 33, 0, 33, 0, 33,
			0, 33, 33, 33, 33, 0, 0, 33, 33, 0,
			5, 0, 0, 33, 0, 33, 33, 33, 0, 0,
			33, 33, 33, 0, 33, 0, 0, 33, 0, 33,
			33, 33, 33, 0, 0, 0, 33, 33, 33, 0,
			0, 33, 33, 33, 0, 0, 0, 0, 0, 0,
			33, 33, 0, 33, 33, 0, 33, 33, 33, 0,
			0, 0, 33, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 1083, 0, 0, 0, 0, 0, 39,
			0, 0, 33, 39, 0, 0, 0, 0, 0, 0,
			0, 33, 33, 33, 39, 0, 0, 0, 0, 39,
			0, 33, 0, 39, 0, 0, 39, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 53, 0, 39, 39,
			0, 0, 0, 39, 39, 0, 38, 0, 0, 39,
			38, 39, 39, 39, 39, 0, 0, 0, 0, 39,
			0, 38, 33, 39, 0, 39, 38, 0, 0, 0,
			38, 0, 0, 38, 0, 39, 0, 39, 39, 7,
			39, 0, 0, 0, 39, 38, 38, 0, 0, 0,
			38, 38, 0, 27, 0, 0, 38, 27, 38, 38,
			38, 38, 0, 0, 39, 0, 38, 0, 27, 0,
			38, 0, 38, 27, 0, 39, 0, 27, 0, 0,
			27, 0, 38, 0, 0, 38, 0, 38, 0, 0,
			0, 38, 27, 27, 0, 0, 0, 27, 27, 0,
			37, 0, 1084, 27, 37, 27, 27, 27, 27, 0,
			0, 38, 0, 27, 0, 37, 0, 27, 0, 27,
			37, 38, 38, 0, 37, 0, 0, 37, 0, 27,
			0, 0, 27, 0, 27, 0, 0, 0, 27, 37,
			37, 0, 0, 0, 37, 37, 0, 5, 0, 54,
			37, 53, 37, 37, 37, 37, 0, 0, 27, 0,
			37, 0, 53, 0, 37, 0, 37, 53, 27, 27,
			0, 53, 0, 0, 53, 0, 37, 0, 0, 37,
			0, 37, 0, 0, 0, 37, 53, 53, 0, 0,
			0, 53, 53, 0, 0, 0, 0, 53, 0, 53,
			53, 53, 53, 0, 0, 37, 0, 53, 0, 0,
			1083, 53, 0, 53, 53, 0, 37, 0, 0, 0,
			0, 0, 0, 53, 0, 53, 53, 0, 53, 0,
			53, 0, 53, 0, 53, 0, 0, 53, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 53,
			53, 0, 53, 53, 53, 53, 0, 53, 0, 0,
			53, 0, 53, 53, 53, 53, 0, 0, 53, 0,
			53, 0, 0, 53, 53, 0, 53, 53, 0, 0,
			53, 0, 0, 0, 0, 0, 53, 0, 0, 53,
			0, 53, 53, 53, 0, 53, 7, 53, 53, 0,
			54, 0, 0, 53, 0, 53, 53, 53, 53, 0,
			0, 54, 0, 53, 0, 53, 54, 53, 0, 53,
			54, 0, 0, 54, 0, 0, 0, 0, 0, 53,
			0, 0, 53, 0, 53, 54, 54, 0, 53, 0,
			54, 54, 0, 0, 0, 0, 54, 0, 54, 54,
			54, 54, 0, 0, 0, 0, 54, 0, 53, 1084,
			54, 0, 54, 53, 0, 0, 0, 0, 0, 0,
			0, 0, 54, 0, 53, 54, 0, 54, 0, 53,
			0, 54, 0, 53, 0, 0, 53, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 53, 53,
			0, 54, 0, 53, 53, 0, 54, 0, 0, 53,
			54, 53, 53, 53, 53, 0, 0, 0, 0, 53,
			0, 54, 0, 53, 0, 53, 54, 0, 0, 0,
			54, 0, 0, 54, 0, 53, 0, 0, 53, 0,
			53, 0, 0, 0, 53, 54, 54, 0, 0, 0,
			54, 54, 0, 0, 0, 0, 54, 0, 54, 54,
			54, 54, 0, 0, 53, 0, 54, 0, 0, 0,
			54, 0, 54, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 54, 0, 56, 54, 0, 54, 0, 0,
			0, 54, 57, 24, 58, 25, 0, 0, 26, 59,
			0, 60, 61, 27, 62, 63, 64, 28, 0, 0,
			0, 54, 0, 65, 0, 66, 30, 67, 68, 69,
			70, 0, 0, 32, 0, 0, 0, 71, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 74, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 76, 77, 78, 79, 80, 81, 39, 40,
			82, 83, 41, 84, 0, 85, 0, 0, 86, 87,
			0, 0, 88, 89, 0, 867, 0, 0, 0, 0,
			0, 867, 0, 0, 0, 0, 0, 90, 91, 92,
			93, 94, 0, 0, 95, 96, 0, 0, 0, 97,
			0, 0, 0, 98, 0, 0, 0, 0, 99, 100,
			101, 102, 103, 0, 0, 0, 104, 867, 105, 0,
			0, 0, 0, 0, 106, 107, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 426, 0, 0, 0, 0, 0, 426, 108,
			109, 110, 111, 0, 0, 0, 0, 0, 867, 0,
			0, 200, 0, 867, 0, 867, 867, 867, 867, 867,
			867, 867, 867, 867, 867, 867, 867, 0, 0, 0,
			0, 0, 867, 867, 426, 867, 867, 0, 0, 0,
			867, 867, 867, 867, 867, 867, 867, 867, 867, 867,
			0, 867, 867, 0, 867, 867, 867, 867, 867, 867,
			867, 867, 867, 867, 0, 867, 867, 867, 867, 867,
			867, 867, 867, 867, 867, 867, 867, 867, 867, 867,
			867, 867, 867, 867, 867, 867, 867, 867, 0, 867,
			0, 867, 0, 867, 867, 867, 867, 0, 0, 358,
			426, 0, 867, 0, 0, 0, 0, 0, 426, 358,
			426, 426, 426, 426, 426, 0, 426, 0, 426, 426,
			0, 426, 426, 426, 426, 426, 0, 426, 426, 426,
			426, 867, 426, 426, 426, 426, 426, 426, 426, 426,
			426, 426, 426, 426, 426, 426, 426, 426, 426, 426,
			426, 426, 426, 426, 0, 0, 0, 0, 358, 0,
			426, 0, 0, 426, 0, 0, 0, 0, 0, 426,
			0, 0, 867, 0, 0, 0, 0, 867, 0, 867,
			867, 867, 867, 867, 867, 867, 867, 867, 867, 867,
			867, 0, 0, 0, 0, 0, 867, 867, 0, 867,
			867, 0, 0, 0, 867, 867, 867, 867, 867, 867,
			867, 867, 867, 867, 698, 867, 867, 0, 867, 867,
			867, 867, 867, 867, 867, 867, 867, 867, 0, 867,
			867, 867, 867, 867, 867, 867, 867, 867, 867, 867,
			867, 867, 867, 867, 867, 867, 867, 867, 867, 867,
			867, 867, 368, 0, 0, 867, 0, 867, 368, 0,
			867, 0, 0, 0, 0, 0, 867, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 368, 0, 0, 698, 0, 0,
			0, 0, 698, 0, 698, 698, 698, 698, 698, 698,
			698, 698, 698, 698, 698, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 698, 698, 0, 0, 0, 0,
			698, 0, 698, 0, 698, 368, 698, 698, 698, 0,
			368, 0, 368, 368, 368, 368, 368, 368, 368, 368,
			368, 368, 368, 368, 0, 0, 0, 0, 0, 368,
			368, 0, 368, 368, 0, 0, 0, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 1075, 368, 368,
			0, 368, 368, 368, 368, 368, 368, 368, 368, 368,
			368, 698, 368, 368, 368, 368, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 368, 368, 368,
			368, 368, 368, 368, 0, 368, 0, 0, 368, 0,
			368, 368, 0, 368, 0, 0, 0, 0, 0, 368,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 368, 0, 0,
			1075, 0, 0, 0, 0, 1075, 0, 1075, 1075, 1075,
			1075, 1075, 1075, 1075, 1075, 1075, 1075, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 1075, 1075, 0,
			0, 0, 0, 1075, 0, 1075, 0, 1075, 368, 1075,
			1075, 1075, 0, 368, 0, 368, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 0, 0, 0,
			0, 0, 368, 368, 0, 368, 368, 0, 0, 0,
			0, 368, 368, 368, 368, 368, 368, 368, 368, 368,
			0, 368, 368, 0, 368, 368, 368, 368, 368, 368,
			368, 368, 368, 368, 1075, 368, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 0, 567, 675,
			0, 368, 0, 368, 567, 0, 368, 0, 24, 0,
			25, 0, 368, 26, 0, 0, 0, 0, 27, 0,
			0, 0, 28, 0, 0, 0, 0, 0, 0, 0,
			0, 30, 0, 0, 0, 0, 0, 0, 32, 0,
			567, 0, 0, 33, 0, 0, 0, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 0, 0, 0, 38, 0, 0, 0, 0,
			0, 0, 0, 39, 40, 0, 0, 41, 0, 0,
			85, 567, 0, 0, 0, 0, 567, 0, 567, 567,
			567, 567, 567, 567, 567, 567, 567, 567, 567, 0,
			0, 693, 0, 0, 0, 0, 567, 0, 567, 567,
			0, 0, 0, 567, 567, 567, 567, 567, 567, 567,
			567, 567, 567, 0, 567, 567, 0, 567, 567, 567,
			567, 567, 567, 567, 567, 567, 567, 0, 567, 567,
			567, 567, 567, 567, 567, 567, 567, 567, 567, 567,
			567, 567, 567, 567, 567, 567, 567, 567, 567, 567,
			0, 563, 0, 0, 0, 387, 567, 563, 0, 0,
			0, 0, 0, 0, 0, 567, 0, 0, 0, 0,
			0, 0, 0, 0, 693, 0, 0, 0, 0, 693,
			0, 693, 693, 693, 693, 693, 693, 693, 693, 693,
			693, 693, 0, 563, 0, 0, 0, 0, 0, 0,
			0, 693, 693, 0, 0, 0, 0, 693, 0, 693,
			0, 693, 0, 693, 693, 693, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 693,
			0, 0, 0, 0, 563, 0, 0, 0, 0, 563,
			693, 563, 563, 563, 563, 563, 563, 563, 563, 563,
			563, 563, 693, 0, 694, 0, 0, 0, 0, 563,
			0, 563, 563, 0, 0, 0, 563, 563, 693, 563,
			563, 563, 563, 563, 563, 563, 0, 563, 563, 0,
			563, 563, 563, 563, 563, 563, 563, 563, 563, 563,
			0, 563, 563, 563, 563, 563, 563, 563, 563, 563,
			563, 563, 563, 563, 563, 563, 563, 563, 563, 563,
			563, 563, 563, 0, 571, 0, 0, 0, 0, 563,
			571, 0, 563, 0, 0, 0, 0, 0, 563, 0,
			0, 0, 0, 0, 0, 0, 0, 694, 0, 0,
			0, 0, 694, 0, 694, 694, 694, 694, 694, 694,
			694, 694, 694, 694, 694, 0, 571, 0, 0, 0,
			0, 0, 0, 0, 694, 694, 0, 0, 0, 0,
			694, 0, 694, 0, 694, 0, 694, 694, 694, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 694, 0, 0, 0, 0, 571, 0, 0,
			0, 0, 571, 694, 571, 571, 571, 571, 571, 571,
			571, 571, 571, 571, 571, 694, 0, 0, 0, 0,
			0, 0, 571, 0, 571, 571, 0, 0, 0, 0,
			571, 694, 571, 571, 571, 571, 571, 571, 571, 0,
			571, 571, 0, 571, 571, 571, 571, 571, 571, 571,
			571, 571, 571, 0, 571, 571, 571, 571, 571, 571,
			571, 571, 571, 571, 571, 571, 571, 571, 571, 571,
			571, 571, 571, 571, 571, 571, 0, 368, 1011, 0,
			0, 0, 571, 368, 0, 571, 0, 24, 0, 25,
			0, 571, 26, 0, 0, 0, 0, 27, 0, 0,
			0, 28, 0, 0, 0, 0, 0, 0, 0, 0,
			30, 0, 0, 0, 0, 0, 0, 32, 0, 368,
			0, 0, 33, 0, 0, 0, 34, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 36, 0,
			37, 0, 0, 0, 38, 0, 0, 0, 0, 0,
			0, 0, 39, 40, 0, 0, 41, 0, 0, 85,
			368, 0, 0, 0, 0, 368, 0, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 368, 0, 0,
			0, 0, 0, 0, 0, 368, 0, 368, 368, 0,
			0, 0, 0, 368, 0, 368, 368, 368, 368, 368,
			368, 368, 0, 368, 368, 0, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 0, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 368, 368, 0,
			486, 1183, 0, 0, 387, 368, 486, 0, 368, 0,
			24, 0, 25, 0, 368, 26, 0, 0, 0, 0,
			27, 0, 0, 0, 28, 0, 0, 0, 0, 0,
			0, 0, 0, 30, 0, 0, 0, 0, 0, 0,
			32, 0, 486, 0, 0, 33, 0, 0, 0, 34,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 36, 0, 37, 0, 0, 0, 38, 0, 0,
			0, 0, 0, 0, 0, 39, 40, 0, 0, 41,
			0, 0, 85, 486, 0, 0, 0, 0, 486, 0,
			486, 486, 486, 486, 486, 486, 486, 486, 486, 486,
			486, 0, 0, 0, 0, 0, 0, 0, 486, 0,
			486, 486, 0, 0, 0, 0, 486, 0, 486, 486,
			486, 486, 486, 486, 486, 0, 486, 486, 0, 486,
			486, 486, 486, 486, 486, 486, 486, 486, 486, 0,
			486, 486, 486, 486, 486, 486, 486, 486, 486, 486,
			486, 486, 486, 486, 486, 486, 486, 486, 486, 486,
			486, 486, 0, 598, 0, 390, 0, 387, 486, 598,
			0, 486, 0, 0, 0, 0, 0, 486, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 390, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 390, 0, 0, 0, 598, 390, 0, 0, 257,
			0, 390, 0, 390, 390, 390, 390, 0, 0, 0,
			0, 390, 0, 0, 0, 390, 368, 0, 0, 390,
			0, 0, 368, 0, 0, 0, 867, 390, 0, 0,
			390, 0, 390, 0, 0, 0, 598, 0, 0, 0,
			0, 598, 0, 598, 598, 598, 598, 598, 598, 598,
			598, 598, 598, 598, 0, 0, 390, 0, 368, 0,
			0, 0, 0, 598, 598, 0, 390, 0, 0, 598,
			0, 598, 0, 598, 867, 598, 598, 598, 0, 598,
			598, 0, 598, 598, 598, 598, 598, 598, 598, 598,
			598, 598, 0, 0, 0, 598, 598, 598, 598, 598,
			598, 598, 598, 598, 598, 598, 598, 598, 598, 598,
			598, 598, 598, 358, 598, 0, 390, 368, 0, 358,
			0, 0, 0, 368, 368, 0, 0, 0, 0, 0,
			598, 0, 368, 368, 368, 368, 368, 368, 368, 867,
			368, 0, 368, 368, 0, 368, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 358, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 368, 0, 0,
			0, 0, 368, 0, 368, 0, 0, 368, 0, 0,
			0, 0, 0, 368, 0, 0, 358, 0, 0, 0,
			0, 358, 0, 358, 358, 358, 358, 358, 358, 358,
			358, 358, 358, 358, 0, 0, 0, 0, 0, 0,
			358, 426, 640, 358, 358, 0, 0, 0, 640, 358,
			358, 358, 0, 358, 426, 358, 358, 358, 0, 358,
			358, 0, 0, 358, 358, 358, 358, 0, 0, 0,
			358, 358, 0, 426, 426, 358, 358, 358, 358, 358,
			358, 358, 358, 0, 640, 0, 0, 0, 0, 0,
			0, 0, 0, 426, 358, 0, 0, 0, 0, 358,
			0, 426, 0, 0, 426, 0, 0, 0, 0, 0,
			358, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 640, 0, 0, 0, 0,
			640, 0, 640, 640, 640, 640, 640, 640, 640, 640,
			640, 640, 640, 0, 0, 0, 0, 0, 0, 0,
			427, 644, 640, 640, 0, 0, 0, 644, 640, 0,
			640, 427, 640, 427, 640, 640, 640, 0, 640, 640,
			0, 0, 640, 640, 640, 640, 0, 0, 0, 640,
			640, 0, 427, 427, 640, 640, 640, 640, 640, 640,
			640, 640, 0, 644, 0, 0, 0, 0, 0, 0,
			0, 0, 427, 640, 0, 0, 0, 0, 0, 0,
			427, 0, 0, 427, 0, 0, 0, 0, 0, 640,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 644, 0, 0, 0, 0, 644,
			0, 644, 644, 644, 644, 644, 644, 644, 644, 644,
			644, 644, 0, 0, 0, 0, 0, 0, 0, 430,
			643, 644, 644, 0, 0, 0, 643, 644, 0, 644,
			430, 644, 430, 644, 644, 644, 0, 644, 644, 0,
			0, 644, 644, 644, 644, 0, 0, 0, 644, 644,
			0, 430, 430, 644, 644, 644, 644, 644, 644, 644,
			644, 0, 643, 0, 0, 0, 0, 0, 0, 0,
			0, 430, 644, 0, 0, 0, 0, 0, 0, 430,
			0, 0, 430, 0, 0, 0, 0, 0, 644, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 643, 0, 0, 0, 0, 643, 0,
			643, 643, 643, 643, 643, 643, 643, 643, 643, 643,
			643, 0, 0, 0, 0, 0, 0, 0, 440, 358,
			643, 643, 0, 0, 0, 358, 643, 0, 643, 440,
			643, 440, 643, 643, 643, 0, 643, 643, 0, 0,
			643, 643, 643, 643, 0, 0, 0, 643, 643, 0,
			440, 440, 643, 643, 643, 643, 643, 643, 643, 643,
			0, 358, 0, 0, 0, 0, 0, 0, 0, 0,
			440, 643, 0, 0, 0, 0, 0, 0, 440, 0,
			0, 440, 0, 0, 0, 0, 0, 643, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 358, 0, 0, 0, 0, 358, 0, 358,
			358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
			0, 0, 0, 0, 0, 0, 358, 620, 0, 358,
			358, 0, 0, 620, 0, 358, 358, 358, 0, 358,
			0, 358, 358, 358, 0, 358, 358, 0, 0, 358,
			358, 358, 358, 0, 0, 0, 358, 358, 0, 0,
			0, 358, 358, 358, 358, 358, 358, 358, 358, 620,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			358, 0, 0, 0, 0, 358, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 358, 0, 0, 858,
			0, 0, 0, 0, 0, 858, 0, 0, 0, 0,
			620, 0, 0, 0, 0, 620, 0, 620, 620, 620,
			620, 620, 620, 620, 620, 620, 620, 620, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 620, 620, 0,
			0, 858, 0, 620, 0, 620, 0, 620, 0, 620,
			620, 620, 0, 620, 620, 0, 0, 620, 620, 620,
			620, 620, 620, 620, 620, 620, 0, 0, 0, 620,
			620, 620, 620, 620, 620, 620, 620, 0, 0, 0,
			0, 0, 858, 0, 0, 0, 0, 858, 620, 858,
			858, 858, 858, 858, 858, 858, 858, 858, 858, 858,
			0, 0, 0, 0, 620, 0, 858, 627, 0, 858,
			858, 0, 0, 627, 0, 858, 0, 858, 0, 858,
			0, 858, 858, 858, 0, 858, 858, 0, 0, 858,
			858, 858, 858, 0, 0, 0, 858, 858, 0, 0,
			0, 858, 858, 858, 858, 858, 858, 858, 858, 627,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			858, 0, 0, 0, 0, 858, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 858, 0, 0, 987,
			0, 0, 0, 0, 0, 987, 0, 0, 0, 0,
			627, 0, 0, 0, 0, 627, 0, 627, 627, 627,
			627, 627, 627, 627, 627, 627, 627, 627, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 627, 627, 0,
			0, 987, 0, 627, 0, 627, 0, 627, 0, 627,
			627, 627, 0, 627, 627, 0, 0, 627, 627, 627,
			627, 0, 0, 0, 627, 627, 0, 0, 0, 627,
			627, 627, 627, 627, 627, 627, 627, 0, 0, 0,
			0, 0, 987, 0, 0, 0, 0, 987, 627, 987,
			987, 987, 987, 987, 987, 987, 987, 987, 987, 987,
			0, 0, 0, 0, 627, 0, 0, 628, 0, 987,
			987, 0, 0, 628, 0, 987, 0, 987, 0, 987,
			0, 987, 987, 987, 0, 987, 987, 0, 0, 987,
			987, 987, 987, 0, 0, 0, 987, 987, 0, 0,
			0, 987, 987, 987, 987, 987, 987, 987, 987, 628,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			987, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 987, 0, 0, 629,
			0, 0, 0, 0, 0, 629, 0, 0, 0, 0,
			628, 0, 0, 0, 0, 628, 0, 628, 628, 628,
			628, 628, 628, 628, 628, 628, 628, 628, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 628, 628, 0,
			0, 629, 0, 628, 0, 628, 0, 628, 0, 628,
			628, 628, 0, 628, 628, 0, 0, 628, 628, 628,
			628, 0, 0, 0, 628, 628, 0, 0, 0, 628,
			628, 628, 628, 628, 628, 628, 628, 0, 0, 0,
			0, 0, 629, 0, 0, 0, 0, 629, 628, 629,
			629, 629, 629, 629, 629, 629, 629, 629, 629, 629,
			0, 0, 0, 0, 628, 0, 0, 0, 0, 629,
			629, 0, 0, 0, 0, 629, 0, 629, 0, 629,
			0, 629, 629, 629, 0, 629, 629, 0, 0, 629,
			629, 629, 629, 0, 0, 0, 629, 629, 0, 0,
			0, 629, 629, 629, 629, 629, 629, 629, 629, 0,
			530, 0, 662, 0, 0, 0, 0, 0, 57, 24,
			629, 25, 0, 0, 26, 260, 0, 0, 0, 27,
			62, 63, 0, 28, 0, 0, 629, 0, 0, 65,
			0, 0, 30, 0, 0, 0, 0, 0, 0, 32,
			0, 0, 0, 0, 33, 0, 72, 73, 34, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			36, 0, 37, 75, 0, 0, 38, 0, 0, 77,
			0, 79, 0, 81, 39, 40, 261, 0, 41, 0,
			0, 0, 0, 0, 0, 662, 0, 0, 0, 0,
			662, 0, 662, 662, 662, 662, 662, 662, 662, 662,
			662, 662, 662, 90, 91, 92, 262, 531, 663, 0,
			95, 96, 662, 662, 0, 0, 0, 0, 662, 98,
			662, 0, 662, 0, 662, 662, 662, 0, 0, 0,
			0, 0, 662, 662, 662, 662, 0, 0, 0, 662,
			662, 0, 0, 0, 662, 662, 662, 662, 662, 662,
			662, 662, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 662, 0, 108, 532, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 533, 534, 0, 662,
			664, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 663, 0, 0, 0, 0, 663, 0, 663, 663,
			663, 663, 663, 663, 663, 663, 663, 663, 663, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 663, 663,
			0, 0, 0, 0, 663, 0, 663, 0, 663, 0,
			663, 663, 663, 0, 0, 0, 0, 0, 663, 663,
			663, 663, 0, 0, 0, 663, 663, 0, 0, 0,
			663, 663, 663, 663, 663, 663, 663, 663, 0, 0,
			0, 0, 0, 664, 0, 0, 0, 0, 664, 663,
			664, 664, 664, 664, 664, 664, 664, 664, 664, 664,
			664, 667, 0, 0, 0, 663, 0, 0, 0, 0,
			664, 664, 0, 0, 0, 0, 664, 0, 664, 0,
			664, 0, 664, 664, 664, 0, 0, 0, 0, 0,
			664, 664, 664, 664, 0, 0, 0, 664, 664, 0,
			0, 0, 664, 664, 664, 664, 664, 664, 664, 664,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 664, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 668, 0, 0, 0, 664, 0, 0,
			0, 0, 0, 0, 667, 0, 0, 0, 0, 667,
			0, 667, 667, 667, 667, 667, 667, 667, 667, 667,
			667, 667, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 667, 667, 0, 0, 0, 0, 667, 0, 667,
			0, 667, 0, 667, 667, 667, 0, 0, 0, 0,
			0, 667, 667, 667, 667, 0, 0, 0, 667, 667,
			0, 0, 0, 0, 0, 667, 667, 667, 667, 667,
			667, 0, 0, 0, 0, 0, 668, 0, 0, 0,
			0, 668, 667, 668, 668, 668, 668, 668, 668, 668,
			668, 668, 668, 668, 669, 0, 0, 0, 667, 0,
			0, 0, 0, 668, 668, 0, 0, 0, 0, 668,
			0, 668, 0, 668, 0, 668, 668, 668, 0, 0,
			0, 0, 0, 668, 668, 668, 668, 0, 0, 0,
			668, 668, 0, 0, 0, 0, 0, 668, 668, 668,
			668, 668, 668, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 668, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 670, 0, 0, 0,
			668, 0, 0, 0, 0, 0, 0, 669, 0, 0,
			0, 0, 669, 0, 669, 669, 669, 669, 669, 669,
			669, 669, 669, 669, 669, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 669, 669, 0, 0, 0, 0,
			669, 0, 669, 0, 669, 0, 669, 669, 669, 0,
			0, 0, 0, 0, 669, 669, 669, 669, 0, 0,
			0, 669, 669, 0, 0, 0, 0, 0, 669, 669,
			669, 669, 669, 669, 0, 0, 0, 0, 0, 670,
			0, 0, 0, 0, 670, 669, 670, 670, 670, 670,
			670, 670, 670, 670, 670, 670, 670, 671, 0, 0,
			0, 669, 0, 0, 0, 0, 670, 670, 0, 0,
			0, 0, 670, 0, 670, 0, 670, 0, 670, 670,
			670, 0, 0, 0, 0, 0, 670, 670, 670, 670,
			0, 0, 0, 670, 670, 0, 0, 0, 0, 0,
			670, 670, 670, 670, 670, 670, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 368, 670, 0, 0,
			0, 0, 368, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 670, 0, 0, 0, 0, 0, 0,
			671, 0, 0, 0, 0, 671, 0, 671, 671, 671,
			671, 671, 671, 671, 671, 671, 671, 671, 368, 0,
			0, 0, 0, 0, 0, 0, 0, 671, 671, 0,
			0, 0, 0, 671, 0, 671, 0, 671, 0, 671,
			671, 671, 0, 0, 0, 0, 0, 671, 671, 671,
			671, 0, 0, 0, 671, 671, 0, 0, 0, 0,
			0, 671, 671, 671, 671, 671, 671, 677, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 671, 0,
			0, 0, 0, 0, 368, 0, 0, 0, 0, 0,
			0, 0, 368, 0, 671, 368, 0, 368, 368, 0,
			0, 0, 368, 368, 0, 0, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 0, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 368, 368, 0, 678,
			0, 0, 0, 0, 368, 0, 0, 368, 0, 0,
			677, 0, 0, 368, 0, 677, 0, 677, 677, 677,
			677, 677, 677, 677, 677, 677, 677, 677, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 677, 677, 0,
			0, 0, 0, 677, 0, 677, 0, 677, 0, 677,
			677, 677, 0, 0, 0, 0, 0, 0, 0, 677,
			677, 0, 0, 0, 677, 677, 0, 0, 0, 0,
			0, 681, 0, 677, 677, 677, 677, 0, 0, 0,
			0, 0, 678, 0, 0, 0, 0, 678, 677, 678,
			678, 678, 678, 678, 678, 678, 678, 678, 678, 678,
			0, 0, 0, 0, 677, 0, 0, 0, 0, 678,
			678, 0, 0, 0, 0, 678, 0, 678, 0, 678,
			0, 678, 678, 678, 0, 0, 0, 0, 0, 0,
			0, 678, 678, 0, 0, 0, 678, 678, 0, 0,
			0, 0, 0, 682, 0, 678, 678, 678, 678, 0,
			0, 0, 0, 0, 681, 0, 0, 0, 0, 681,
			678, 681, 681, 681, 681, 681, 681, 681, 681, 681,
			681, 681, 0, 0, 0, 0, 678, 0, 0, 0,
			0, 681, 681, 0, 0, 0, 0, 681, 0, 681,
			0, 681, 0, 681, 681, 681, 0, 0, 0, 0,
			0, 0, 0, 681, 681, 0, 0, 0, 681, 681,
			0, 0, 0, 0, 0, 684, 0, 0, 0, 681,
			681, 0, 0, 0, 0, 0, 682, 0, 0, 0,
			0, 682, 681, 682, 682, 682, 682, 682, 682, 682,
			682, 682, 682, 682, 0, 0, 0, 0, 681, 0,
			0, 0, 0, 682, 682, 0, 0, 0, 0, 682,
			0, 682, 0, 682, 0, 682, 682, 682, 0, 0,
			0, 0, 0, 0, 0, 682, 682, 0, 0, 0,
			682, 682, 0, 0, 0, 0, 0, 685, 0, 0,
			0, 682, 682, 0, 0, 0, 0, 0, 684, 0,
			0, 0, 0, 684, 682, 684, 684, 684, 684, 684,
			684, 684, 684, 684, 684, 684, 0, 0, 0, 0,
			682, 0, 0, 0, 0, 684, 684, 0, 0, 0,
			0, 684, 0, 684, 0, 684, 0, 684, 684, 684,
			0, 0, 0, 0, 0, 0, 0, 0, 684, 0,
			0, 0, 684, 684, 0, 0, 0, 0, 0, 687,
			0, 0, 0, 684, 684, 0, 0, 0, 0, 0,
			685, 0, 0, 0, 0, 685, 684, 685, 685, 685,
			685, 685, 685, 685, 685, 685, 685, 685, 0, 0,
			0, 0, 684, 0, 0, 0, 0, 685, 685, 0,
			0, 0, 0, 685, 0, 685, 0, 685, 0, 685,
			685, 685, 0, 0, 0, 0, 0, 0, 0, 0,
			685, 0, 0, 0, 685, 685, 0, 0, 0, 0,
			0, 688, 0, 0, 0, 685, 685, 0, 0, 0,
			0, 0, 687, 0, 0, 0, 0, 687, 685, 687,
			687, 687, 687, 687, 687, 687, 687, 687, 687, 687,
			0, 0, 0, 0, 685, 0, 0, 0, 0, 687,
			687, 0, 0, 0, 0, 687, 0, 687, 0, 687,
			0, 687, 687, 687, 0, 0, 0, 0, 0, 0,
			0, 0, 687, 0, 0, 0, 0, 687, 0, 0,
			0, 0, 0, 690, 0, 0, 0, 687, 687, 0,
			0, 0, 0, 0, 688, 0, 0, 0, 0, 688,
			687, 688, 688, 688, 688, 688, 688, 688, 688, 688,
			688, 688, 0, 0, 0, 0, 687, 0, 0, 0,
			0, 688, 688, 0, 0, 0, 0, 688, 0, 688,
			0, 688, 0, 688, 688, 688, 0, 0, 0, 0,
			0, 0, 0, 0, 688, 0, 0, 0, 0, 688,
			0, 0, 0, 0, 0, 691, 0, 0, 0, 688,
			688, 0, 0, 0, 0, 0, 690, 0, 0, 0,
			0, 690, 688, 690, 690, 690, 690, 690, 690, 690,
			690, 690, 690, 690, 0, 0, 0, 0, 688, 0,
			0, 0, 0, 690, 690, 0, 0, 0, 0, 690,
			0, 690, 0, 690, 0, 690, 690, 690, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 690, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 690, 690, 0, 0, 0, 0, 0, 691, 0,
			0, 0, 0, 691, 690, 691, 691, 691, 691, 691,
			691, 691, 691, 691, 691, 691, 0, 0, 0, 0,
			690, 0, 0, 0, 0, 691, 691, 0, 0, 0,
			0, 691, 0, 691, 0, 691, 0, 691, 691, 691,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 691, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 691, 691, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 691, 0, 0, 602,
			0, 0, 0, 0, 0, 0, 0, 57, 24, 58,
			25, 1212, 691, 26, 59, 0, 60, 61, 27, 62,
			63, 64, 28, 0, 0, 0, 0, 0, 65, 0,
			66, 30, 67, 68, 69, 70, 0, 0, 32, 0,
			0, 0, 71, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 74, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 76, 77, 78,
			79, 80, 81, 39, 40, 82, 83, 41, 84, 0,
			85, 0, 0, 86, 87, 0, 0, 88, 89, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 93, 94, 0, 0, 95,
			96, 0, 0, 0, 97, 0, 0, 0, 98, 0,
			0, 0, 0, 99, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 0, 0, 0, 0, 0, 106,
			107, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 56, 0, 108, 109, 110, 111, 0, 1213,
			57, 24, 58, 25, 0, 0, 26, 59, 0, 60,
			61, 27, 62, 63, 64, 28, 0, 0, 0, 0,
			0, 65, 0, 66, 30, 67, 68, 69, 70, 0,
			0, 32, 0, 0, 0, 71, 33, 0, 72, 73,
			34, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			74, 0, 36, 0, 37, 75, 0, 0, 38, 0,
			76, 77, 78, 79, 80, 81, 39, 40, 82, 83,
			41, 84, 0, 85, 0, 0, 86, 87, 0, 0,
			88, 89, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 90, 91, 92, 93, 94,
			0, 0, 95, 96, 0, 0, 0, 97, 0, 0,
			0, 98, 0, 0, 0, 0, 99, 100, 101, 102,
			103, 0, 0, 0, 104, 0, 105, 0, 0, 0,
			0, 0, 106, 107, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 273, 0, 0, 0, 108, 109, 110,
			111, 57, 24, 58, 25, 0, 0, 26, 59, 0,
			60, 61, 27, 62, 63, 64, 28, 0, 0, 0,
			0, 0, 65, 0, 66, 30, 67, 68, 69, 70,
			0, 0, 32, 0, 0, 0, 71, 33, 0, 72,
			73, 34, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 74, 0, 36, 0, 37, 75, 0, 0, 38,
			0, 76, 77, 78, 79, 80, 81, 39, 40, 82,
			83, 41, 84, 0, 85, 0, 0, 86, 87, 0,
			0, 88, 89, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 90, 91, 92, 93,
			94, 0, 0, 95, 96, 0, 0, 0, 97, 0,
			0, 0, 98, 0, 0, 0, 0, 99, 100, 101,
			102, 103, 0, 0, 0, 104, 0, 105, 0, 0,
			0, 0, 0, 106, 107, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 602, 0, 0, 0, 108, 109,
			110, 111, 57, 24, 58, 25, 0, 0, 26, 59,
			0, 60, 61, 27, 62, 63, 64, 28, 0, 0,
			0, 0, 0, 65, 0, 66, 30, 67, 68, 69,
			70, 0, 0, 32, 0, 0, 0, 71, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 74, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 76, 77, 78, 79, 80, 81, 39, 40,
			82, 83, 41, 84, 0, 85, 0, 0, 86, 87,
			0, 0, 88, 89, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			93, 94, 0, 0, 95, 96, 0, 0, 0, 97,
			0, 0, 0, 98, 0, 0, 0, 0, 99, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 0,
			0, 0, 0, 0, 106, 107, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 1080, 0, 0, 0, 108,
			109, 110, 111, 1080, 1080, 1080, 1080, 0, 0, 1080,
			1080, 0, 1080, 1080, 1080, 1080, 1080, 1080, 1080, 0,
			0, 0, 0, 0, 1080, 0, 1080, 1080, 1080, 1080,
			1080, 1080, 0, 0, 1080, 0, 0, 0, 1080, 1080,
			0, 1080, 1080, 1080, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 1080, 0, 1080, 0, 1080, 1080, 0,
			0, 1080, 0, 1080, 1080, 1080, 1080, 1080, 1080, 1080,
			1080, 1080, 1080, 1080, 1080, 0, 1080, 0, 0, 1080,
			1080, 0, 0, 1080, 1080, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 1080, 1080,
			1080, 1080, 1080, 0, 0, 1080, 1080, 0, 0, 0,
			1080, 0, 0, 0, 1080, 0, 0, 0, 0, 1080,
			1080, 1080, 1080, 1080, 0, 0, 0, 1080, 0, 1080,
			0, 0, 0, 0, 0, 1080, 1080, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 645, 0, 0, 0,
			1080, 1080, 1080, 1080, 57, 24, 0, 25, 0, 0,
			26, 260, 0, 0, 0, 27, 62, 63, 0, 28,
			0, 0, 24, 0, 25, 65, 0, 26, 30, 0,
			0, 0, 27, 0, 0, 32, 28, 0, 0, 0,
			33, 0, 72, 73, 34, 30, 646, 0, 0, 0,
			0, 0, 32, 647, 0, 0, 36, 33, 37, 75,
			0, 34, 38, 0, 0, 77, 0, 79, 0, 81,
			39, 40, 261, 36, 41, 37, 0, 0, 0, 38,
			0, 648, 0, 0, 88, 89, 0, 39, 40, 0,
			0, 41, 0, 0, 85, 0, 0, 0, 0, 90,
			91, 92, 93, 94, 0, 0, 95, 96, 0, 0,
			0, 0, 0, 0, 0, 98, 0, 0, 649, 0,
			298, 100, 101, 102, 103, 0, 0, 0, 104, 0,
			105, 0, 0, 0, 0, 0, 106, 107, 0, 0,
			0, 0, 0, 0, 57, 24, 0, 25, 0, 0,
			26, 260, 0, 0, 0, 27, 62, 63, 0, 28,
			0, 108, 109, 110, 111, 65, 0, 0, 30, 0,
			0, 0, 0, 0, 0, 32, 0, 0, 0, 331,
			33, 0, 72, 73, 34, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 36, 0, 37, 75,
			0, 0, 38, 0, 0, 77, 0, 79, 0, 81,
			39, 40, 261, 0, 41, 0, 0, 0, 0, 0,
			0, 87, 0, 0, 88, 89, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 90,
			91, 92, 93, 815, 0, 0, 95, 96, 0, 0,
			0, 816, 1127, 0, 0, 98, 0, 0, 0, 0,
			0, 100, 101, 102, 103, 0, 0, 0, 104, 0,
			105, 0, 0, 0, 0, 0, 106, 107, 0, 0,
			0, 0, 0, 0, 57, 24, 0, 25, 0, 0,
			26, 260, 0, 0, 0, 27, 62, 63, 0, 28,
			0, 108, 817, 110, 111, 65, 0, 818, 30, 0,
			0, 0, 819, 0, 0, 32, 0, 0, 0, 0,
			33, 0, 72, 73, 34, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 36, 0, 37, 75,
			0, 0, 38, 0, 0, 77, 0, 79, 0, 81,
			39, 40, 261, 0, 41, 0, 0, 0, 0, 0,
			0, 87, 0, 0, 88, 89, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 90,
			91, 92, 93, 815, 0, 0, 95, 96, 0, 0,
			0, 816, 0, 0, 0, 98, 0, 0, 0, 0,
			0, 100, 101, 102, 103, 0, 0, 0, 104, 0,
			105, 0, 0, 0, 0, 0, 106, 107, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			833, 108, 817, 110, 111, 0, 0, 818, 57, 24,
			0, 25, 819, 0, 26, 260, 0, 0, 0, 27,
			62, 63, 0, 28, 0, 0, 192, 0, 192, 65,
			0, 192, 30, 0, 0, 0, 192, 0, 0, 32,
			192, 0, 0, 0, 33, 0, 72, 73, 34, 192,
			0, 0, 0, 0, 0, 0, 192, 0, 0, 0,
			36, 192, 37, 75, 0, 192, 38, 0, 0, 77,
			0, 79, 0, 81, 39, 40, 261, 192, 41, 192,
			0, 0, 0, 192, 0, 87, 0, 0, 88, 89,
			0, 192, 192, 0, 0, 192, 0, 0, 192, 0,
			0, 0, 0, 90, 91, 92, 93, 309, 0, 0,
			95, 96, 0, 0, 0, 551, 834, 0, 0, 98,
			0, 0, 0, 0, 0, 100, 101, 102, 103, 0,
			0, 0, 104, 0, 105, 0, 0, 1105, 0, 0,
			106, 107, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 1039, 0, 0, 0, 108, 310, 110, 111, 57,
			24, 0, 25, 0, 0, 26, 260, 0, 0, 0,
			27, 62, 63, 192, 28, 0, 0, 192, 0, 192,
			65, 0, 192, 30, 0, 0, 0, 192, 0, 0,
			32, 192, 0, 0, 0, 33, 0, 72, 73, 34,
			192, 646, 0, 0, 0, 0, 0, 192, 647, 0,
			0, 36, 192, 37, 75, 0, 192, 38, 0, 0,
			77, 0, 79, 0, 81, 39, 40, 261, 192, 41,
			192, 0, 0, 0, 192, 0, 648, 0, 0, 88,
			89, 0, 192, 192, 0, 0, 192, 0, 0, 192,
			0, 0, 0, 0, 90, 91, 92, 93, 94, 0,
			0, 95, 96, 0, 0, 0, 0, 0, 0, 0,
			98, 0, 0, 0, 0, 0, 100, 101, 102, 103,
			0, 0, 0, 104, 0, 105, 1105, 0, 0, 0,
			0, 106, 107, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 833, 0, 0, 0, 108, 109, 110, 111,
			57, 24, 0, 25, 0, 0, 26, 260, 0, 0,
			0, 27, 62, 63, 192, 28, 0, 0, 24, 0,
			25, 65, 0, 26, 30, 0, 0, 0, 27, 0,
			0, 32, 28, 0, 0, 0, 33, 0, 72, 73,
			34, 30, 0, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 36, 33, 37, 75, 1042, 34, 38, 0,
			0, 77, 0, 79, 0, 81, 39, 40, 261, 36,
			41, 37, 0, 0, 0, 38, 0, 87, 0, 0,
			88, 89, 0, 39, 40, 0, 0, 41, 0, 0,
			85, 0, 0, 0, 0, 90, 91, 92, 93, 309,
			0, 0, 95, 96, 0, 0, 0, 551, 0, 0,
			0, 98, 0, 0, 0, 0, 0, 100, 101, 102,
			103, 0, 0, 0, 104, 0, 105, 0, 0, 0,
			0, 0, 106, 107, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 795, 0, 0, 0, 108, 310, 110,
			111, 57, 24, 0, 25, 0, 0, 26, 260, 0,
			0, 0, 27, 62, 63, 387, 28, 0, 0, 24,
			0, 25, 65, 0, 26, 30, 0, 0, 0, 27,
			0, 0, 32, 28, 0, 0, 0, 33, 0, 72,
			73, 34, 30, 0, 0, 0, 0, 0, 0, 32,
			0, 0, 0, 36, 33, 37, 75, 0, 34, 38,
			0, 0, 77, 0, 79, 0, 81, 39, 40, 261,
			36, 41, 37, 0, 85, 0, 38, 0, 87, 0,
			0, 88, 89, 0, 39, 40, 0, 0, 41, 0,
			0, 618, 0, 0, 0, 0, 90, 91, 92, 93,
			309, 0, 0, 95, 96, 0, 0, 0, 0, 0,
			0, 0, 98, 0, 0, 0, 0, 0, 100, 101,
			102, 103, 0, 0, 0, 104, 0, 105, 0, 0,
			0, 0, 0, 106, 107, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 833, 0, 0, 0, 108, 310,
			110, 111, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 387, 28, 0, 0,
			0, 0, 0, 65, 0, 0, 30, 0, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 0, 0, 0, 0, 87,
			0, 0, 88, 89, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			93, 309, 0, 0, 95, 96, 0, 0, 0, 551,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 0,
			0, 0, 0, 0, 106, 107, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 351, 0, 108,
			310, 110, 111, 352, 0, 57, 24, 0, 25, 0,
			0, 26, 260, 0, 0, 0, 27, 62, 63, 0,
			28, 0, 0, 0, 0, 0, 65, 0, 0, 30,
			0, 0, 0, 0, 0, 0, 32, 0, 0, 353,
			0, 33, 0, 72, 73, 34, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 36, 0, 37,
			75, 0, 0, 38, 0, 0, 77, 0, 79, 0,
			81, 39, 40, 261, 0, 41, 0, 0, 0, 0,
			0, 0, 0, 0, 354, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			90, 91, 92, 262, 355, 0, 0, 95, 96, 0,
			0, 0, 0, 0, 0, 0, 98, 0, 356, 0,
			0, 0, 100, 101, 102, 103, 992, 0, 0, 104,
			0, 105, 825, 0, 0, 0, 0, 106, 107, 0,
			57, 24, 0, 25, 0, 0, 26, 260, 0, 0,
			0, 27, 62, 63, 0, 28, 0, 0, 0, 0,
			0, 65, 108, 264, 30, 111, 0, 0, 0, 0,
			0, 32, 0, 0, 0, 0, 33, 0, 72, 73,
			34, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 36, 0, 37, 75, 0, 0, 38, 0,
			0, 77, 0, 79, 0, 81, 39, 40, 261, 0,
			41, 0, 0, 0, 0, 0, 0, 87, 0, 0,
			88, 89, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 90, 91, 92, 93, 309,
			0, 0, 95, 96, 0, 0, 0, 0, 993, 0,
			0, 98, 0, 0, 0, 0, 0, 100, 101, 102,
			103, 0, 0, 0, 104, 0, 105, 0, 0, 0,
			0, 0, 106, 107, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 1060, 0, 0, 0, 108, 310, 110,
			111, 57, 24, 0, 25, 0, 0, 26, 260, 0,
			0, 0, 27, 62, 63, 0, 28, 0, 0, 24,
			0, 25, 65, 0, 26, 30, 0, 0, 0, 27,
			0, 0, 32, 28, 0, 0, 0, 33, 0, 72,
			73, 34, 30, 0, 0, 0, 0, 0, 0, 32,
			0, 0, 0, 36, 33, 37, 75, 0, 34, 38,
			0, 0, 77, 0, 79, 0, 81, 39, 40, 261,
			36, 41, 37, 0, 0, 0, 38, 0, 87, 0,
			0, 88, 89, 0, 39, 40, 0, 0, 41, 0,
			0, 568, 0, 0, 0, 0, 90, 91, 92, 93,
			309, 0, 0, 95, 96, 0, 0, 0, 0, 1061,
			0, 0, 98, 0, 0, 0, 0, 0, 100, 101,
			102, 103, 0, 0, 0, 104, 0, 105, 0, 0,
			0, 0, 0, 106, 107, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 308, 0, 0, 0, 108, 310,
			110, 111, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 387, 28, 0, 0,
			547, 0, 547, 65, 0, 547, 30, 0, 0, 0,
			547, 0, 0, 32, 547, 0, 0, 0, 33, 0,
			72, 73, 34, 547, 0, 0, 0, 0, 0, 0,
			547, 0, 0, 0, 36, 547, 37, 75, 0, 547,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 547, 41, 547, 0, 0, 0, 547, 0, 87,
			0, 0, 88, 89, 0, 547, 547, 0, 0, 547,
			0, 0, 547, 0, 0, 0, 0, 90, 91, 92,
			93, 309, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 0,
			0, 0, 0, 0, 106, 107, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 317, 0, 0, 0, 108,
			310, 110, 111, 57, 24, 0, 25, 0, 0, 26,
			260, 0, 0, 0, 27, 62, 63, 547, 28, 0,
			0, 0, 0, 0, 65, 0, 0, 30, 0, 0,
			0, 0, 0, 0, 32, 0, 0, 0, 0, 33,
			0, 72, 73, 34, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 36, 0, 37, 75, 0,
			0, 38, 0, 0, 77, 0, 79, 0, 81, 39,
			40, 261, 0, 41, 0, 0, 0, 0, 0, 0,
			87, 0, 0, 88, 89, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 90, 91,
			92, 93, 309, 0, 0, 95, 96, 0, 0, 0,
			0, 0, 0, 0, 98, 0, 0, 0, 0, 0,
			100, 101, 102, 103, 0, 0, 0, 104, 0, 105,
			0, 0, 0, 0, 0, 106, 107, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 351, 0,
			108, 310, 110, 111, 352, 0, 57, 24, 0, 25,
			0, 0, 26, 260, 0, 0, 0, 27, 62, 63,
			0, 28, 0, 0, 0, 0, 0, 65, 0, 0,
			30, 0, 0, 0, 0, 0, 0, 32, 0, 0,
			353, 0, 33, 0, 72, 73, 34, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 36, 0,
			37, 75, 0, 0, 38, 0, 0, 77, 0, 79,
			0, 81, 39, 40, 261, 0, 41, 0, 0, 0,
			0, 0, 0, 0, 0, 354, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 90, 91, 92, 262, 355, 0, 0, 95, 96,
			0, 0, 0, 0, 0, 0, 0, 98, 0, 356,
			0, 0, 0, 100, 101, 102, 103, 0, 0, 0,
			104, 0, 105, 643, 0, 0, 0, 0, 106, 107,
			0, 57, 24, 0, 25, 0, 0, 26, 260, 0,
			0, 0, 27, 62, 63, 0, 28, 0, 0, 0,
			0, 0, 65, 108, 264, 30, 111, 0, 0, 0,
			0, 0, 32, 0, 0, 0, 0, 33, 0, 72,
			73, 34, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 36, 0, 37, 75, 0, 0, 38,
			0, 0, 77, 0, 79, 0, 81, 39, 40, 261,
			0, 41, 0, 0, 0, 0, 0, 0, 87, 0,
			0, 88, 89, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 90, 91, 92, 93,
			94, 0, 0, 95, 96, 0, 0, 0, 0, 0,
			0, 0, 98, 0, 0, 0, 0, 0, 100, 101,
			102, 103, 0, 0, 0, 104, 0, 105, 0, 0,
			0, 0, 0, 106, 107, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 825, 0, 0, 0, 108, 109,
			110, 111, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			193, 0, 193, 65, 0, 193, 30, 0, 0, 0,
			193, 0, 0, 32, 193, 0, 0, 0, 33, 0,
			72, 73, 34, 193, 0, 0, 0, 0, 0, 0,
			193, 0, 0, 0, 36, 193, 37, 75, 0, 193,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 193, 41, 193, 0, 0, 0, 193, 0, 87,
			0, 0, 88, 89, 0, 193, 193, 0, 0, 193,
			0, 0, 193, 0, 0, 0, 0, 90, 91, 92,
			93, 309, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 0,
			0, 0, 0, 0, 106, 107, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 1174, 0, 0, 0, 108,
			310, 110, 111, 57, 24, 0, 25, 0, 0, 26,
			260, 0, 0, 0, 27, 62, 63, 193, 28, 0,
			0, 192, 0, 192, 65, 0, 192, 30, 0, 0,
			0, 192, 0, 0, 32, 192, 0, 0, 0, 33,
			0, 72, 73, 34, 192, 0, 0, 0, 0, 0,
			0, 192, 0, 0, 0, 36, 192, 37, 75, 0,
			192, 38, 0, 0, 77, 0, 79, 0, 81, 39,
			40, 261, 192, 41, 192, 0, 0, 0, 192, 0,
			87, 0, 0, 88, 89, 0, 192, 192, 0, 0,
			192, 0, 0, 192, 0, 0, 0, 0, 90, 91,
			92, 93, 94, 0, 0, 95, 96, 0, 0, 0,
			0, 0, 0, 0, 98, 0, 0, 0, 0, 0,
			100, 101, 102, 103, 0, 0, 0, 104, 0, 105,
			0, 0, 0, 0, 0, 106, 107, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 86, 0, 0, 0,
			108, 1175, 110, 111, 86, 86, 0, 86, 0, 0,
			86, 86, 0, 0, 0, 86, 86, 86, 192, 86,
			0, 0, 0, 0, 0, 86, 0, 0, 86, 0,
			0, 0, 0, 0, 0, 86, 0, 0, 0, 0,
			86, 0, 86, 86, 86, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 86, 0, 86, 86,
			0, 0, 86, 0, 0, 86, 0, 86, 0, 86,
			86, 86, 86, 0, 86, 0, 0, 0, 0, 0,
			0, 86, 0, 0, 86, 86, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 86,
			86, 86, 86, 86, 0, 0, 86, 86, 0, 0,
			0, 0, 0, 0, 0, 86, 0, 0, 0, 0,
			0, 86, 86, 86, 86, 0, 0, 0, 86, 0,
			86, 0, 0, 0, 0, 0, 86, 86, 0, 0,
			0, 0, 205, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 206, 0, 0, 0, 351,
			0, 86, 86, 86, 86, 352, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 207, 0, 0, 0, 65, 0,
			0, 30, 0, 0, 0, 0, 0, 0, 32, 0,
			0, 353, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 208, 209,
			210, 211, 0, 212, 213, 214, 215, 216, 217, 218,
			219, 0, 0, 220, 221, 222, 223, 224, 225, 226,
			227, 0, 90, 91, 92, 262, 355, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			356, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 351, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 33, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 33, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 33, 41, 0, 0, 0, 33, 0, 0, 0,
			0, 33, 0, 33, 33, 33, 33, 0, 0, 33,
			0, 33, 0, 0, 0, 33, 0, 90, 91, 92,
			262, 355, 0, 0, 95, 96, 0, 33, 0, 0,
			33, 0, 33, 98, 0, 356, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 363,
			0, 0, 0, 0, 106, 107, 33, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 33, 33, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			33, 0, 33, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 33, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 33, 41, 0, 0,
			0, 33, 0, 0, 0, 0, 33, 0, 33, 33,
			33, 33, 0, 0, 0, 0, 33, 0, 0, 0,
			33, 0, 90, 91, 92, 262, 355, 0, 0, 95,
			96, 0, 33, 0, 0, 33, 0, 33, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 365, 0, 0, 0, 0, 106,
			107, 33, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 33, 33, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 53, 0, 53, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 53, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 53, 41, 0, 0, 0, 53, 0, 0, 0,
			0, 53, 0, 53, 53, 53, 53, 0, 0, 0,
			0, 53, 0, 0, 0, 53, 0, 90, 91, 92,
			262, 355, 0, 0, 95, 96, 0, 53, 0, 0,
			53, 0, 53, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 367,
			0, 0, 0, 0, 106, 107, 53, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 327, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			482, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 483, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 484, 41, 0, 0,
			485, 486, 0, 0, 0, 0, 487, 0, 488, 489,
			490, 491, 0, 0, 0, 0, 492, 0, 0, 0,
			493, 0, 90, 91, 92, 262, 355, 0, 0, 95,
			96, 0, 494, 0, 0, 495, 0, 496, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 369, 0, 0, 0, 0, 106,
			107, 497, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 482, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 483, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 484, 41, 0, 0, 0, 486, 0, 0, 0,
			0, 487, 0, 488, 489, 490, 491, 0, 0, 0,
			0, 492, 0, 0, 0, 493, 0, 90, 91, 92,
			262, 355, 0, 0, 95, 96, 0, 494, 0, 0,
			495, 0, 496, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 371,
			0, 0, 0, 0, 106, 107, 497, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 355, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 373, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 355, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 375,
			0, 0, 0, 0, 106, 107, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 355, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 377, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 355, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 669,
			0, 0, 0, 0, 106, 107, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 355, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 671, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 355, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 673,
			0, 0, 0, 0, 106, 107, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 355, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 691, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 355, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 693,
			0, 0, 0, 0, 106, 107, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 355, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 695, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 696, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 698,
			0, 0, 0, 0, 106, 107, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 696, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 700, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 696, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 702,
			0, 0, 0, 0, 106, 107, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 696, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 704, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 696, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 706,
			0, 0, 0, 0, 106, 107, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 696, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 708, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 696, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 710,
			0, 0, 0, 0, 106, 107, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 696, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 712, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 696, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 714,
			0, 0, 0, 0, 106, 107, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 696, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 716, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 696, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 718,
			0, 0, 0, 0, 106, 107, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 696, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 720, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 696, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 100,
			101, 102, 103, 0, 0, 0, 104, 0, 105, 888,
			0, 0, 0, 0, 106, 107, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 0, 0, 0, 0, 65, 108,
			264, 30, 111, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 355, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 677, 0, 0, 0, 0, 106,
			107, 0, 57, 24, 0, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 108, 264, 30, 111, 0, 0,
			0, 0, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 85, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 263, 0, 0, 95, 96, 0, 0, 0, 0,
			0, 0, 0, 98, 0, 0, 0, 0, 0, 0,
			678, 679, 0, 0, 213, 0, 0, 0, 680, 213,
			0, 0, 0, 213, 0, 213, 0, 0, 213, 0,
			213, 213, 0, 213, 0, 213, 0, 213, 0, 213,
			213, 213, 213, 0, 0, 213, 213, 0, 0, 108,
			264, 213, 681, 213, 213, 213, 0, 0, 213, 0,
			213, 0, 213, 0, 0, 213, 0, 213, 213, 213,
			213, 0, 0, 0, 213, 213, 213, 0, 0, 213,
			213, 213, 0, 0, 0, 0, 0, 0, 213, 213,
			0, 213, 213, 0, 213, 213, 213, 0, 0, 0,
			213, 0, 0, 530, 0, 0, 0, 0, 0, 0,
			368, 57, 24, 0, 25, 0, 0, 26, 260, 0,
			213, 0, 27, 62, 63, 0, 28, 0, 0, 213,
			213, 213, 65, 0, 0, 30, 0, 0, 0, 213,
			0, 0, 32, 0, 0, 0, 368, 33, 0, 72,
			73, 34, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 36, 0, 37, 75, 0, 0, 38,
			0, 0, 77, 0, 79, 0, 81, 39, 40, 261,
			213, 41, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 90, 91, 92, 262,
			531, 0, 368, 95, 96, 0, 0, 0, 0, 0,
			0, 0, 98, 368, 368, 368, 368, 867, 0, 0,
			368, 368, 0, 0, 368, 368, 368, 368, 368, 368,
			368, 368, 368, 0, 368, 368, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 368, 368, 368, 368,
			368, 368, 368, 368, 368, 368, 0, 0, 108, 532,
			0, 0, 368, 0, 54, 368, 54, 0, 54, 0,
			54, 0, 0, 54, 0, 54, 54, 0, 54, 0,
			54, 0, 54, 0, 54, 54, 54, 54, 0, 0,
			54, 54, 0, 0, 0, 0, 54, 54, 54, 54,
			54, 0, 0, 54, 0, 54, 0, 54, 0, 54,
			54, 0, 54, 54, 54, 54, 0, 0, 54, 54,
			54, 54, 0, 0, 54, 54, 54, 0, 0, 0,
			0, 0, 0, 54, 54, 0, 54, 54, 0, 54,
			54, 54, 0, 0, 0, 54, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 54, 0, 0, 0, 0,
			0, 54, 54, 53, 0, 0, 0, 53, 0, 53,
			0, 0, 53, 0, 53, 53, 0, 53, 0, 53,
			0, 53, 0, 53, 53, 53, 53, 0, 0, 53,
			53, 0, 0, 0, 0, 53, 0, 53, 53, 53,
			0, 0, 53, 0, 53, 0, 53, 0, 0, 53,
			0, 53, 53, 53, 53, 54, 0, 0, 53, 53,
			53, 0, 0, 53, 53, 53, 0, 0, 0, 0,
			0, 0, 53, 53, 0, 53, 53, 0, 53, 53,
			53, 0, 0, 0, 53, 53, 0, 0, 0, 53,
			0, 53, 0, 0, 53, 0, 53, 53, 0, 53,
			0, 53, 0, 53, 53, 53, 53, 53, 53, 0,
			0, 53, 53, 0, 89, 0, 0, 53, 0, 53,
			53, 53, 0, 53, 53, 0, 53, 0, 53, 0,
			0, 53, 0, 53, 53, 53, 53, 0, 0, 0,
			53, 53, 53, 0, 0, 53, 53, 53, 0, 0,
			0, 0, 0, 0, 53, 53, 0, 53, 53, 0,
			53, 53, 53, 0, 53, 0, 53, 54, 0, 0,
			0, 54, 0, 54, 0, 0, 54, 0, 54, 54,
			0, 54, 0, 54, 0, 54, 53, 54, 54, 54,
			54, 0, 0, 54, 54, 0, 90, 0, 0, 54,
			0, 54, 54, 54, 0, 53, 54, 0, 54, 0,
			54, 0, 0, 54, 0, 54, 54, 54, 54, 0,
			0, 0, 54, 54, 54, 0, 0, 54, 54, 54,
			0, 0, 0, 0, 0, 0, 54, 54, 0, 54,
			54, 0, 54, 54, 54, 0, 53, 0, 54, 53,
			0, 0, 0, 53, 0, 53, 0, 0, 53, 0,
			53, 53, 0, 53, 0, 53, 0, 53, 54, 53,
			53, 53, 53, 0, 0, 53, 53, 0, 0, 0,
			0, 53, 0, 53, 53, 53, 0, 54, 53, 0,
			53, 0, 53, 0, 0, 53, 0, 53, 53, 53,
			53, 0, 0, 0, 53, 53, 53, 0, 0, 53,
			53, 53, 0, 0, 0, 0, 0, 0, 53, 53,
			0, 53, 53, 0, 53, 53, 53, 0, 54, 0,
			53, 53, 0, 0, 0, 53, 0, 53, 0, 0,
			53, 0, 53, 53, 0, 53, 0, 53, 0, 53,
			53, 53, 53, 53, 53, 0, 0, 53, 53, 0,
			241, 0, 0, 53, 0, 53, 53, 53, 0, 0,
			53, 0, 53, 389, 53, 0, 0, 53, 0, 53,
			53, 53, 53, 0, 0, 0, 53, 53, 53, 0,
			0, 53, 53, 53, 0, 0, 389, 0, 0, 0,
			53, 53, 0, 53, 53, 482, 53, 53, 53, 389,
			53, 0, 53, 0, 389, 0, 0, 256, 0, 389,
			0, 389, 389, 389, 389, 0, 0, 0, 483, 389,
			0, 0, 53, 389, 0, 0, 0, 389, 0, 0,
			482, 484, 242, 0, 0, 389, 486, 0, 389, 0,
			389, 487, 0, 488, 489, 490, 491, 0, 0, 0,
			0, 492, 0, 483, 0, 493, 0, 0, 0, 1491,
			0, 0, 0, 0, 389, 0, 484, 494, 0, 0,
			495, 486, 496, 0, 389, 0, 487, 0, 488, 489,
			490, 491, 53, 0, 0, 0, 492, 0, 0, 0,
			493, 0, 0, 0, 1491, 0, 497, 57, 24, 0,
			25, 0, 494, 26, 260, 495, 1492, 496, 27, 62,
			63, 0, 28, 0, 0, 202, 0, 202, 65, 0,
			202, 30, 0, 0, 389, 202, 0, 0, 32, 202,
			0, 497, 0, 33, 0, 72, 73, 34, 202, 646,
			0, 0, 0, 0, 0, 202, 647, 0, 0, 36,
			202, 37, 75, 0, 202, 38, 1493, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 202, 41, 202, 0,
			0, 0, 202, 0, 648, 0, 0, 88, 89, 0,
			202, 202, 0, 0, 202, 0, 0, 202, 0, 0,
			0, 1493, 90, 91, 92, 93, 94, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 1037,
			0, 649, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 0, 0, 0, 0, 0, 106,
			107, 0, 0, 0, 0, 0, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 108, 109, 110, 111, 65, 0,
			0, 30, 0, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 202, 33, 0, 72, 73, 34, 0, 646,
			0, 0, 0, 0, 0, 0, 647, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 648, 0, 0, 88, 89, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 93, 94, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 649, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 0, 0, 0, 0, 0, 106,
			107, 0, 0, 0, 0, 0, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 108, 109, 110, 111, 65, 0,
			0, 30, 0, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			85, 0, 0, 0, 87, 0, 0, 88, 89, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 93, 309, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 0, 0, 0, 0, 0, 106,
			107, 0, 0, 0, 0, 0, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 108, 310, 110, 111, 65, 0,
			0, 30, 0, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			85, 0, 0, 0, 87, 0, 0, 88, 89, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 93, 309, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 903, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 0, 0, 0, 0, 0, 106,
			107, 0, 0, 0, 0, 0, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 108, 310, 110, 111, 65, 0,
			0, 30, 0, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 87, 0, 0, 88, 89, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 93, 309, 0, 0, 95,
			96, 0, 0, 0, 551, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 0, 0, 0, 0, 0, 106,
			107, 0, 0, 0, 0, 0, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 108, 310, 110, 111, 65, 0,
			0, 30, 0, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 87, 0, 0, 88, 89, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 93, 309, 0, 0, 95,
			96, 0, 0, 0, 545, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 0, 0, 0, 0, 0, 106,
			107, 0, 0, 0, 0, 0, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 108, 310, 110, 111, 65, 0,
			0, 30, 0, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 87, 0, 0, 88, 89, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 93, 309, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 0, 0, 0, 0, 0, 106,
			107, 0, 0, 0, 0, 0, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 108, 310, 110, 111, 65, 0,
			0, 30, 0, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 87, 0, 0, 88, 89, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 93, 94, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 0, 0, 0, 0, 0, 106,
			107, 0, 0, 0, 0, 0, 0, 722, 722, 0,
			722, 0, 0, 722, 722, 0, 0, 0, 722, 722,
			722, 0, 722, 0, 108, 109, 110, 111, 722, 0,
			0, 722, 0, 0, 0, 0, 0, 0, 722, 0,
			0, 0, 0, 722, 0, 722, 722, 722, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 722,
			0, 722, 722, 0, 0, 722, 0, 0, 722, 0,
			722, 0, 722, 722, 722, 722, 0, 722, 0, 0,
			0, 0, 0, 0, 722, 0, 0, 722, 722, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 722, 722, 722, 722, 722, 0, 0, 722,
			722, 0, 0, 0, 0, 0, 0, 0, 722, 0,
			0, 0, 0, 0, 722, 722, 722, 722, 0, 0,
			0, 722, 0, 722, 0, 0, 0, 0, 0, 722,
			722, 0, 0, 0, 0, 0, 0, 149, 149, 0,
			149, 0, 0, 149, 149, 0, 0, 0, 149, 149,
			149, 0, 149, 0, 722, 722, 722, 722, 149, 0,
			0, 149, 0, 0, 0, 0, 0, 0, 149, 0,
			0, 0, 0, 149, 0, 149, 149, 149, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 149,
			0, 149, 149, 0, 0, 149, 0, 0, 149, 0,
			149, 0, 149, 149, 149, 149, 0, 149, 0, 0,
			0, 0, 0, 0, 149, 0, 0, 149, 149, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 149, 149, 149, 149, 149, 0, 0, 149,
			149, 0, 0, 0, 0, 0, 0, 0, 149, 0,
			0, 0, 0, 0, 149, 149, 149, 149, 0, 0,
			0, 149, 0, 149, 0, 0, 0, 0, 0, 149,
			149, 0, 0, 0, 0, 0, 0, 57, 24, 0,
			25, 0, 0, 26, 260, 0, 0, 0, 27, 62,
			63, 0, 28, 0, 149, 149, 149, 149, 65, 0,
			0, 30, 0, 0, 0, 0, 0, 0, 32, 0,
			0, 0, 0, 33, 0, 72, 73, 34, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 36,
			0, 37, 75, 0, 0, 38, 0, 0, 77, 0,
			79, 0, 81, 39, 40, 261, 0, 41, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 90, 91, 92, 262, 696, 0, 0, 95,
			96, 0, 0, 0, 0, 0, 0, 0, 98, 0,
			0, 0, 0, 0, 100, 101, 102, 103, 0, 0,
			0, 104, 0, 105, 0, 57, 24, 0, 25, 106,
			107, 26, 260, 0, 0, 0, 27, 62, 63, 0,
			28, 0, 0, 0, 0, 0, 65, 0, 0, 30,
			0, 0, 0, 0, 108, 264, 32, 111, 0, 0,
			0, 33, 0, 72, 73, 34, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 36, 0, 37,
			75, 0, 0, 38, 0, 0, 77, 0, 79, 0,
			81, 39, 40, 261, 0, 41, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			90, 91, 92, 262, 355, 0, 0, 95, 96, 0,
			0, 0, 0, 0, 0, 0, 98, 0, 0, 0,
			0, 0, 100, 101, 102, 103, 0, 0, 0, 104,
			0, 105, 0, 57, 24, 0, 25, 106, 107, 26,
			260, 0, 0, 0, 27, 62, 63, 0, 28, 0,
			0, 0, 0, 0, 65, 0, 0, 30, 0, 0,
			0, 0, 108, 264, 32, 111, 0, 0, 0, 33,
			0, 72, 73, 34, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 36, 0, 37, 75, 0,
			0, 38, 0, 0, 77, 0, 79, 0, 81, 39,
			40, 261, 0, 41, 0, 0, 85, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 90, 91,
			92, 262, 263, 0, 0, 95, 96, 0, 0, 0,
			0, 0, 57, 24, 98, 25, 0, 0, 26, 260,
			0, 678, 679, 27, 62, 63, 0, 28, 0, 680,
			0, 0, 0, 65, 0, 0, 30, 0, 0, 0,
			0, 0, 0, 32, 0, 53, 0, 0, 33, 0,
			72, 73, 34, 0, 0, 0, 0, 0, 0, 0,
			108, 1053, 0, 681, 36, 0, 37, 75, 53, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 53, 41, 0, 0, 85, 53, 0, 0, 0,
			0, 53, 0, 53, 53, 53, 53, 0, 0, 0,
			0, 53, 0, 0, 0, 53, 0, 90, 91, 92,
			262, 263, 0, 0, 95, 96, 0, 53, 0, 0,
			53, 0, 53, 98, 0, 768, 0, 768, 0, 768,
			678, 679, 768, 0, 768, 768, 0, 768, 680, 768,
			0, 768, 0, 768, 768, 768, 53, 0, 0, 768,
			768, 0, 53, 53, 0, 768, 223, 768, 768, 0,
			0, 0, 768, 0, 0, 0, 768, 0, 0, 108,
			264, 0, 681, 0, 0, 0, 0, 768, 768, 0,
			768, 0, 0, 0, 768, 768, 0, 0, 0, 0,
			0, 0, 768, 768, 0, 0, 768, 0, 0, 768,
			0, 0, 57, 24, 768, 25, 0, 0, 26, 260,
			0, 0, 0, 27, 62, 63, 0, 28, 0, 0,
			0, 0, 0, 65, 0, 0, 30, 0, 0, 0,
			768, 768, 0, 32, 0, 0, 0, 0, 33, 0,
			72, 73, 34, 768, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 36, 0, 37, 75, 0, 0,
			38, 0, 0, 77, 0, 79, 0, 81, 39, 40,
			261, 0, 41, 0, 0, 85, 0, 0, 0, 0,
			0, 0, 0, 0, 768, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 90, 91, 92,
			262, 263, 0, 0, 95, 96, 0, 0, 0, 0,
			767, 0, 767, 98, 0, 767, 0, 767, 767, 0,
			767, 0, 767, 0, 767, 0, 767, 767, 767, 0,
			0, 0, 767, 767, 0, 0, 0, 0, 767, 0,
			767, 767, 0, 0, 0, 767, 0, 0, 0, 767,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 108,
			264, 767, 0, 767, 0, 0, 0, 767, 767, 0,
			0, 0, 0, 0, 0, 767, 767, 0, 767, 767,
			767, 0, 767, 767, 0, 767, 767, 767, 767, 0,
			767, 0, 767, 0, 767, 767, 767, 0, 0, 0,
			767, 767, 0, 0, 0, 0, 767, 0, 767, 767,
			0, 0, 0, 767, 0, 0, 0, 767, 0, 0,
			0, 0, 0, 0, 0, 0, 767, 0, 0, 767,
			0, 767, 0, 0, 0, 767, 767, 0, 0, 0,
			0, 0, 0, 767, 767, 0, 24, 767, 25, 0,
			767, 26, 0, 0, 1456, 767, 27, 0, 770, 0,
			28, 0, 771, 1457, 1458, 0, 0, 767, 1459, 30,
			0, 0, 0, 0, 1460, 0, 32, 0, 0, 0,
			0, 33, 0, 0, 0, 34, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 36, 0, 37,
			0, 0, 0, 38, 0, 0, 0, 0, 0, 0,
			0, 39, 40, 0, 24, 41, 25, 0, 1461, 26,
			0, 0, 1456, 1462, 27, 0, 770, 0, 28, 0,
			771, 1457, 1458, 0, 0, 767, 1459, 30, 0, 0,
			0, 0, 1460, 0, 32, 0, 0, 0, 0, 33,
			0, 0, 0, 34, 53, 0, 53, 0, 0, 53,
			0, 0, 1463, 0, 53, 36, 0, 37, 53, 0,
			0, 38, 0, 0, 0, 0, 0, 53, 0, 39,
			40, 0, 0, 41, 53, 0, 1461, 0, 0, 53,
			0, 1462, 0, 53, 53, 53, 53, 53, 0, 53,
			0, 0, 53, 1464, 53, 53, 0, 53, 53, 0,
			0, 53, 0, 0, 53, 0, 0, 53, 0, 53,
			53, 0, 0, 53, 53, 0, 53, 0, 0, 53,
			0, 0, 0, 53, 0, 53, 0, 53, 0, 0,
			0, 0, 53, 0, 0, 53, 0, 53, 0, 0,
			0, 53, 0, 0, 53, 0, 0, 0, 0, 53,
			53, 0, 0, 53, 0, 172, 53, 24, 0, 25,
			0, 1464, 26, 0, 0, 0, 0, 27, 0, 0,
			0, 28, 0, 0, 0, 0, 0, 0, 0, 0,
			30, 0, 0, 0, 0, 0, 0, 32, 0, 0,
			0, 0, 33, 172, 0, 0, 34, 54, 615, 54,
			0, 53, 54, 0, 0, 616, 0, 54, 36, 0,
			37, 54, 0, 0, 38, 0, 0, 617, 0, 0,
			54, 0, 39, 40, 0, 0, 41, 54, 0, 618,
			0, 0, 54, 0, 0, 0, 54, 53, 54, 53,
			54, 53, 53, 0, 0, 54, 0, 53, 54, 0,
			54, 53, 0, 0, 54, 619, 0, 54, 0, 0,
			53, 0, 54, 54, 0, 0, 54, 53, 0, 54,
			0, 0, 53, 0, 0, 0, 53, 24, 53, 25,
			53, 0, 26, 0, 0, 53, 0, 27, 53, 0,
			53, 28, 0, 0, 53, 29, 0, 53, 0, 0,
			30, 0, 53, 53, 0, 31, 53, 32, 0, 53,
			0, 0, 33, 0, 620, 0, 34, 35, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 36, 0,
			37, 0, 0, 0, 38, 0, 0, 0, 0, 0,
			0, 0, 39, 40, 0, 24, 41, 25, 0, 42,
			26, 0, 1389, 0, 54, 27, 0, 0, 0, 28,
			0, 0, 0, 0, 0, 0, 0, 0, 30, 0,
			0, 0, 0, 0, 0, 32, 0, 0, 0, 0,
			33, 0, 1390, 0, 34, 0, 0, 0, 0, 39,
			0, 0, 0, 0, 53, 0, 36, 0, 37, 0,
			39, 0, 38, 1391, 0, 39, 0, 0, 0, 39,
			39, 40, 39, 0, 41, 0, 0, 85, 0, 0,
			0, 0, 0, 0, 39, 39, 0, 0, 0, 39,
			39, 0, 37, 0, 43, 39, 0, 39, 39, 39,
			39, 0, 0, 37, 0, 39, 0, 0, 37, 39,
			0, 39, 37, 0, 0, 37, 0, 0, 0, 0,
			0, 39, 0, 39, 39, 0, 39, 37, 37, 0,
			39, 0, 37, 37, 0, 53, 0, 0, 37, 0,
			37, 37, 37, 37, 0, 0, 53, 0, 37, 0,
			39, 53, 37, 0, 37, 53, 0, 0, 53, 0,
			39, 39, 387, 0, 37, 0, 0, 37, 0, 37,
			53, 53, 0, 37, 0, 53, 53, 0, 53, 0,
			0, 53, 0, 53, 53, 53, 53, 0, 0, 53,
			0, 53, 0, 37, 53, 53, 0, 53, 53, 0,
			0, 53, 0, 37, 37, 0, 0, 53, 0, 0,
			53, 0, 53, 53, 53, 0, 53, 0, 53, 53,
			53, 0, 0, 0, 53, 0, 53, 53, 53, 53,
			0, 0, 0, 0, 53, 0, 53, 0, 53, 0,
			53, 0, 0, 53, 0, 0, 41, 0, 0, 0,
			53, 0, 0, 53, 0, 53, 53, 0, 53, 53,
			0, 53, 0, 0, 0, 0, 53, 0, 53, 53,
			53, 53, 0, 0, 0, 0, 53, 0, 0, 53,
			53, 53, 0, 0, 0, 0, 0, 0, 0, 42,
			0, 0, 53, 0, 53, 53, 0, 53, 0, 53,
			0, 0, 0, 0, 53, 0, 53, 53, 53, 53,
			0, 0, 0, 0, 53, 0, 0, 0, 53, 0,
			0, 53, 0, 0, 0, 0, 0, 53, 53, 0,
			53, 225, 0, 53, 0, 53, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 53,
			0, 0, 0, 0, 0, 53, 53
		};

		protected static readonly short[] yyCheck = new short[20417]
		{
			17, 306, 17, 307, 18, 52, 17, 17, 4, 52,
			549, 240, 196, 361, 551, 321, 238, 296, 195, 60,
			499, 330, 521, 69, 422, 392, 345, 305, 305, 604,
			165, 199, 253, 392, 611, 422, 119, 121, 121, 303,
			1042, 403, 88, 89, 806, 838, 808, 93, 78, 59,
			1293, 0, 386, 1250, 995, 622, 631, 74, 422, 402,
			256, 78, 45, 340, 647, 48, 0, 256, 256, 115,
			80, 1363, 82, 268, 256, 1047, 256, 256, 95, 17,
			386, 98, 256, 256, 6, 256, 256, 1372, 1527, 268,
			852, 1334, 17, 855, 282, 256, 256, 1287, 60, 145,
			1397, 62, 256, 256, 1347, 66, 67, 68, 1245, 70,
			71, 256, 256, 256, 75, 76, 325, 372, 17, 1416,
			81, 82, 256, 84, 375, 86, 314, 256, 17, 268,
			91, 92, 343, 405, 371, 256, 256, 504, 257, 256,
			256, 17, 256, 380, 17, 417, 256, 1439, 165, 196,
			165, 1288, 256, 196, 165, 165, 256, 339, 0, 120,
			371, 1351, 344, 371, 346, 294, 443, 349, 350, 339,
			352, 353, 380, 424, 344, 256, 346, 744, 433, 349,
			350, 402, 352, 353, 17, 1470, 375, 256, 17, 256,
			17, 238, 17, 1478, 256, 238, 378, 17, 378, 395,
			371, 374, 376, 376, 375, 378, 252, 253, 378, 363,
			371, 371, 373, 422, 256, 375, 583, 1656, 17, 363,
			266, 374, 418, 376, 583, 378, 545, 165, 1200, 425,
			597, 426, 375, 372, 422, 424, 253, 371, 467, 373,
			165, 375, 259, 548, 466, 228, 329, 426, 332, 332,
			1535, 433, 293, 424, 358, 256, 604, 303, 592, 342,
			433, 60, 303, 433, 424, 64, 165, 1208, 422, 636,
			380, 385, 809, 373, 641, 642, 165, 422, 422, 296,
			433, 424, 641, 631, 301, 302, 592, 426, 334, 165,
			424, 868, 165, 427, 1591, 638, 365, 256, 344, 316,
			261, 422, 422, 335, 265, 422, 422, 324, 257, 326,
			356, 479, 234, 330, 256, 325, 622, 256, 385, 256,
			903, 1618, 256, 385, 428, 429, 430, 431, 363, 346,
			347, 293, 165, 1630, 277, 1632, 165, 379, 165, 294,
			165, 422, 343, 256, 305, 165, 361, 256, 394, 372,
			361, 306, 398, 256, 268, 256, 402, 403, 1120, 256,
			390, 1247, 276, 294, 326, 371, 165, 373, 256, 256,
			371, 256, 256, 390, 375, 306, 377, 378, 256, 380,
			1173, 422, 256, 1520, 385, 402, 403, 422, 1585, 294,
			407, 408, 409, 410, 411, 412, 413, 414, 415, 416,
			417, 433, 448, 449, 1290, 339, 452, 974, 1651, 343,
			433, 745, 1549, 1550, 351, 257, 1553, 638, 419, 466,
			363, 380, 439, 466, 256, 88, 89, 474, 256, 1566,
			272, 474, 1569, 256, 373, 277, 378, 371, 744, 281,
			1683, 375, 1204, 377, 378, 379, 380, 1584, 111, 363,
			363, 385, 357, 256, 296, 376, 818, 1459, 363, 256,
			422, 374, 371, 376, 770, 378, 256, 422, 373, 372,
			1007, 1608, 377, 256, 520, 378, 522, 256, 379, 422,
			423, 323, 379, 371, 793, 390, 985, 470, 471, 856,
			375, 378, 380, 476, 293, 379, 358, 856, 376, 325,
			342, 1263, 376, 549, 514, 256, 256, 262, 422, 422,
			1272, 339, 433, 266, 256, 372, 344, 422, 346, 88,
			89, 349, 350, 752, 352, 353, 924, 326, 895, 575,
			547, 335, 549, 263, 551, 343, 199, 924, 256, 1301,
			203, 204, 111, 298, 376, 363, 264, 811, 910, 256,
			363, 395, 886, 376, 564, 516, 785, 778, 341, 521,
			924, 314, 361, 305, 581, 380, 256, 266, 585, 1146,
			257, 321, 740, 376, 418, 621, 433, 1114, 305, 376,
			886, 598, 256, 600, 374, 315, 596, 395, 378, 604,
			373, 637, 638, 604, 373, 263, 422, 305, 682, 682,
			372, 611, 563, 372, 422, 433, 269, 325, 277, 422,
			418, 380, 281, 256, 432, 314, 631, 425, 433, 432,
			631, 638, 373, 422, 432, 372, 379, 382, 383, 646,
			647, 378, 295, 340, 203, 204, 372, 372, 385, 973,
			365, 687, 378, 372, 307, 376, 1223, 315, 376, 339,
			375, 256, 315, 375, 344, 376, 346, 269, 321, 349,
			350, 433, 352, 353, 433, 339, 971, 377, 974, 373,
			344, 256, 346, 342, 286, 349, 350, 422, 352, 353,
			379, 727, 1001, 729, 646, 647, 433, 376, 823, 352,
			353, 375, 738, 1268, 1379, 916, 339, 433, 433, 424,
			269, 344, 433, 346, 433, 433, 349, 350, 725, 352,
			353, 272, 433, 730, 731, 373, 733, 427, 381, 377,
			305, 256, 521, 386, 1620, 1621, 295, 1004, 1006, 272,
			1379, 272, 778, 422, 339, 296, 256, 256, 1042, 344,
			380, 346, 1427, 433, 349, 350, 315, 352, 353, 269,
			796, 1378, 1379, 296, 378, 296, 372, 1155, 421, 433,
			1158, 778, 323, 1161, 377, 811, 286, 380, 1155, 427,
			811, 1158, 818, 1400, 1161, 341, 793, 380, 1427, 372,
			323, 1677, 323, 352, 353, 448, 449, 1115, 956, 357,
			433, 1155, 809, 371, 1158, 1379, 420, 1161, 1379, 816,
			1427, 818, 256, 1302, 339, 604, 422, 373, 1117, 344,
			339, 346, 381, 1141, 349, 350, 479, 352, 353, 1379,
			339, 378, 390, 1450, 357, 344, 1131, 346, 433, 422,
			349, 350, 631, 352, 353, 845, 1375, 847, 390, 885,
			364, 858, 371, 1427, 377, 862, 1427, 646, 647, 771,
			379, 375, 421, 377, 1379, 872, 866, 390, 868, 1379,
			1081, 907, 388, 420, 910, 1379, 379, 1427, 343, 1217,
			916, 17, 396, 397, 6, 373, 393, 923, 377, 448,
			449, 380, 395, 924, 339, 17, 903, 385, 1012, 343,
			21, 372, 416, 910, 343, 1072, 371, 378, 433, 916,
			424, 918, 1427, 427, 951, 418, 339, 1427, 951, 343,
			1078, 928, 425, 1427, 60, 385, 371, 371, 64, 432,
			1268, 375, 53, 377, 378, 395, 380, 357, 60, 592,
			1054, 385, 64, 1212, 272, 375, 385, 377, 371, 277,
			339, 903, 375, 281, 343, 1250, 395, 377, 418, 343,
			395, 385, 969, 1482, 971, 425, 88, 89, 296, 622,
			390, 395, 924, 339, 343, 343, 1012, 1496, 339, 418,
			987, 339, 371, 418, 1253, 992, 425, 371, 995, 111,
			425, 380, 1465, 432, 418, 323, 380, 432, 1005, 1518,
			1007, 425, 371, 371, 1477, 371, 357, 389, 432, 1045,
			371, 380, 380, 371, 342, 357, 343, 806, 1054, 808,
			371, 363, 398, 399, 375, 1363, 1465, 1496, 681, 165,
			1299, 373, 1243, 371, 376, 377, 378, 377, 1477, 390,
			380, 395, 380, 165, 371, 1081, 1573, 372, 390, 961,
			306, 376, 308, 306, 375, 380, 1063, 313, 1065, 1010,
			313, 1068, 1098, 852, 418, 404, 855, 372, 1104, 325,
			372, 425, 325, 378, 1081, 371, 378, 394, 386, 387,
			422, 203, 204, 385, 380, 374, 375, 740, 418, 378,
			371, 744, 400, 401, 375, 425, 377, 378, 372, 380,
			1319, 1439, 376, 1315, 385, 306, 380, 1114, 1635, 373,
			1117, 1285, 313, 378, 903, 380, 1385, 770, 1440, 374,
			385, 1157, 681, 378, 1155, 1447, 419, 1158, 372, 364,
			1161, 1138, 376, 372, 378, 924, 1493, 372, 419, 378,
			375, 1148, 377, 378, 1440, 1672, 1146, 269, 374, 422,
			385, 1447, 378, 1510, 422, 374, 1163, 293, 1165, 378,
			1167, 396, 397, 1170, 277, 1459, 374, 256, 1697, 1698,
			378, 293, 1529, 295, 1531, 374, 1088, 376, 1090, 378,
			1092, 416, 377, 374, 375, 307, 377, 378, 379, 424,
			326, 844, 427, 315, 1155, 1181, 374, 1158, 433, 321,
			378, 1208, 371, 1155, 326, 1212, 1158, 1243, 380, 1161,
			863, 376, 1217, 378, 374, 380, 1217, 375, 378, 377,
			1227, 1228, 374, 1223, 364, 361, 378, 402, 403, 882,
			352, 353, 372, 886, 380, 375, 1243, 377, 378, 361,
			1244, 256, 376, 1250, 378, 376, 1253, 378, 1285, 380,
			1287, 374, 1285, 376, 1287, 378, 396, 397, 375, 381,
			380, 372, 379, 1268, 386, 376, 374, 1268, 377, 1276,
			378, 88, 89, 372, 256, 1282, 416, 376, 1315, 372,
			372, 373, 1315, 376, 424, 844, 422, 427, 294, 376,
			1585, 378, 1299, 433, 111, 427, 428, 429, 430, 421,
			422, 378, 379, 956, 863, 1312, 1313, 390, 391, 392,
			378, 374, 380, 376, 1351, 366, 367, 378, 1351, 380,
			380, 974, 975, 882, 354, 355, 448, 449, 374, 294,
			376, 1120, 374, 374, 376, 376, 376, 422, 378, 1375,
			343, 1378, 1379, 354, 355, 1378, 1379, 376, 374, 378,
			376, 374, 261, 376, 378, 376, 380, 1364, 1363, 378,
			380, 380, 1363, 1400, 372, 373, 1155, 1400, 1375, 1158,
			422, 378, 1161, 380, 373, 284, 1383, 378, 1385, 380,
			418, 419, 368, 369, 356, 521, 203, 204, 297, 1042,
			1427, 366, 367, 302, 1427, 1431, 366, 367, 307, 521,
			309, 310, 311, 312, 368, 369, 423, 424, 317, 425,
			426, 379, 321, 1450, 1709, 1204, 975, 1450, 431, 432,
			418, 376, 376, 380, 333, 1078, 385, 336, 1217, 338,
			372, 378, 376, 1086, 1439, 385, 433, 376, 1439, 422,
			294, 294, 378, 1443, 376, 376, 256, 376, 378, 256,
			375, 432, 269, 362, 380, 432, 406, 407, 408, 409,
			410, 411, 412, 413, 414, 415, 294, 294, 604, 385,
			592, 422, 376, 378, 1263, 377, 379, 378, 295, 1268,
			377, 422, 604, 1272, 376, 385, 380, 378, 378, 378,
			307, 378, 427, 433, 376, 631, 371, 1504, 315, 378,
			622, 1501, 378, 376, 421, 379, 343, 378, 376, 631,
			646, 647, 1301, 422, 294, 294, 378, 374, 379, 375,
			371, 256, 256, 422, 646, 647, 1563, 1086, 378, 422,
			1563, 256, 256, 385, 280, 352, 353, 256, 371, 343,
			372, 376, 376, 1580, 88, 89, 385, 1580, 725, 380,
			298, 376, 378, 730, 731, 374, 1593, 1594, 380, 681,
			1593, 1594, 375, 378, 381, 376, 1573, 111, 380, 378,
			376, 1571, 374, 376, 1363, 421, 347, 385, 1585, 427,
			385, 371, 256, 1620, 1621, 385, 385, 1620, 1621, 256,
			376, 372, 376, 347, 379, 1602, 378, 374, 376, 375,
			374, 374, 339, 371, 421, 348, 372, 94, 422, 378,
			348, 376, 375, 100, 101, 102, 103, 104, 105, 106,
			107, 422, 744, 256, 385, 372, 371, 371, 1635, 380,
			371, 448, 449, 372, 356, 375, 1289, 337, 380, 305,
			1677, 372, 378, 376, 1677, 372, 376, 422, 770, 372,
			1439, 380, 422, 373, 375, 199, 371, 375, 422, 203,
			204, 1697, 1698, 375, 422, 1672, 375, 377, 385, 375,
			806, 343, 808, 371, 375, 380, 373, 385, 377, 375,
			1680, 1681, 376, 376, 806, 377, 808, 1687, 1688, 378,
			1697, 1698, 378, 378, 256, 256, 378, 380, 376, 376,
			380, 1354, 1709, 374, 265, 380, 267, 376, 422, 270,
			380, 422, 376, 376, 275, 422, 852, 385, 279, 855,
			422, 372, 844, 385, 1377, 269, 374, 288, 372, 376,
			852, 315, 263, 855, 295, 375, 375, 372, 376, 300,
			385, 863, 376, 304, 380, 0, 422, 0, 371, 380,
			372, 295, 380, 0, 380, 316, 376, 318, 372, 376,
			882, 322, 371, 307, 886, 376, 422, 903, 374, 330,
			331, 315, 380, 334, 372, 376, 337, 321, 372, 376,
			380, 903, 374, 371, 380, 1438, 372, 1440, 924, 376,
			422, 380, 969, 422, 1447, 376, 376, 380, 372, 376,
			372, 371, 924, 1456, 1457, 372, 1459, 380, 352, 353,
			315, 377, 1465, 372, 263, 380, 380, 380, 1377, 51,
			380, 5, 309, 380, 1477, 380, 380, 380, 1005, 1482,
			12, 1484, 1078, 1243, 1487, 1243, 1400, 381, 956, 1450,
			1455, 1295, 386, 1496, 1625, 1641, 1588, 1576, 256, 1605,
			1501, 978, 974, 975, 978, 1571, 784, 265, 345, 267,
			1482, 422, 270, 978, 681, 1518, 973, 275, 355, 1415,
			1427, 279, 1681, 1502, 1688, 1682, 1598, 421, 1351, 1438,
			288, 1594, 1531, 256, 1593, 951, 1063, 295, 1065, 321,
			1354, 1068, 300, 816, 916, 1001, 304, 1456, 1457, 1131,
			778, 72, 348, 811, 448, 449, 422, 422, 316, 600,
			318, 1161, 1157, 433, 322, 647, 434, 436, 435, 886,
			1042, 1438, 330, 331, 437, 1484, 334, 438, 1487, 337,
			1249, 418, 419, 420, 592, 479, 423, 424, 425, 426,
			427, 428, 429, 430, 431, 432, 433, 434, 435, 436,
			437, 438, 1331, 165, 1217, 1116, 1228, 1141, 1100, 1202,
			1217, 1215, 565, 1016, 1086, 1302, 339, 1271, 457, 736,
			1433, 344, 457, 346, 347, 348, 349, 350, 351, 352,
			353, 354, 355, 356, 1120, 946, 1163, 1313, 1165, -1,
			1167, 953, -1, 366, 367, -1, -1, -1, 1120, 372,
			0, 374, -1, 376, -1, 378, 379, 380, -1, -1,
			-1, -1, -1, -1, 422, 388, 389, -1, -1, 1155,
			393, 394, 1158, -1, -1, 1161, -1, 844, -1, 402,
			403, 404, 405, 1155, -1, -1, 1158, -1, 261, 1161,
			-1, -1, -1, -1, 417, -1, 863, -1, 592, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 545, -1,
			433, 284, -1, -1, -1, 882, -1, -1, 1204, -1,
			261, -1, -1, -1, 297, -1, -1, -1, 622, 302,
			-1, 1217, 1204, -1, 307, -1, 309, 310, 311, 312,
			-1, -1, 315, 284, 317, 1217, -1, -1, 321, 1276,
			-1, -1, -1, -1, -1, -1, 297, -1, -1, -1,
			333, 302, -1, 336, 305, 338, 307, -1, 309, 310,
			311, 312, -1, -1, -1, -1, 317, 1263, -1, -1,
			321, -1, 1268, -1, 325, 1312, 1272, 681, -1, 362,
			627, 1263, 333, -1, -1, 336, 1268, 338, -1, 372,
			1272, -1, -1, -1, 256, -1, -1, -1, 975, -1,
			-1, -1, -1, -1, -1, 1301, 357, 1289, 0, -1,
			-1, 362, -1, -1, -1, -1, -1, -1, -1, 1301,
			-1, 372, 373, -1, 375, -1, 377, 1364, -1, -1,
			-1, 678, 679, -1, -1, -1, 740, -1, -1, 390,
			744, -1, -1, -1, -1, -1, 1383, -1, -1, 696,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 1042, 770, 1363, -1, -1,
			0, 422, 1354, -1, -1, -1, -1, 339, -1, -1,
			-1, 1363, 344, -1, 346, 347, 348, 349, 350, 351,
			352, 353, 354, 355, 356, 1377, 256, -1, -1, -1,
			-1, -1, 262, -1, 366, 367, -1, -1, -1, 1086,
			372, -1, 374, -1, 376, -1, 378, 379, 380, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 394, -1, -1, -1, -1, -1, 298, -1,
			844, -1, -1, 1439, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 1438, 1439, 1440, 863,
			-1, -1, -1, -1, -1, 1447, -1, -1, 815, -1,
			-1, 433, -1, -1, 1456, 1457, -1, 1459, 882, 339,
			-1, -1, 886, 343, 344, -1, 346, 347, 348, 349,
			350, 351, 352, 353, 354, 355, 356, 357, -1, -1,
			-1, -1, 1484, 363, 0, 1487, 366, 367, -1, -1,
			-1, 371, 372, 373, 374, 375, 376, 377, 378, 379,
			380, -1, 382, 383, -1, -1, 386, 387, 388, 389,
			390, -1, -1, 393, 394, -1, -1, -1, 398, 399,
			400, 401, 402, 403, 404, 405, -1, -1, -1, -1,
			-1, -1, 956, -1, -1, -1, -1, 417, -1, -1,
			420, -1, 422, -1, 424, 257, -1, 427, -1, 261,
			974, 975, -1, 433, -1, -1, -1, -1, -1, -1,
			272, -1, -1, -1, -1, 277, -1, -1, -1, 281,
			-1, -1, 284, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 296, 297, -1, -1, -1, 301,
			302, 261, 1289, 263, -1, 307, -1, 309, 310, 311,
			312, -1, -1, -1, -1, 317, 256, -1, -1, 321,
			-1, 323, 262, -1, 284, -1, -1, -1, 1042, -1,
			-1, 333, -1, 335, 336, -1, 338, 297, -1, -1,
			342, -1, 302, -1, 1001, -1, -1, 307, -1, 309,
			310, 311, 312, -1, -1, 315, -1, 317, 298, -1,
			362, 321, -1, -1, 1078, 0, -1, 1354, -1, -1,
			372, 373, 1086, 333, -1, -1, 336, -1, 338, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			1377, -1, 1049, -1, -1, -1, -1, -1, -1, 339,
			-1, -1, 362, 343, 344, -1, 346, 347, 348, 349,
			350, 351, 352, 353, 354, 355, 356, -1, -1, -1,
			-1, -1, -1, 363, -1, -1, 366, 367, -1, -1,
			-1, 371, 372, 373, 374, 375, 376, -1, 378, 379,
			380, -1, 382, 383, -1, -1, 386, 387, 388, 389,
			256, 1438, -1, 393, 394, 261, 262, -1, 398, 399,
			400, 401, 402, 403, 404, 405, -1, -1, -1, 1456,
			1457, -1, 1459, -1, -1, -1, -1, 417, 284, -1,
			420, -1, 422, -1, 424, -1, -1, 427, -1, -1,
			-1, 297, 298, 433, -1, -1, 302, 1484, -1, 305,
			1487, 307, -1, 309, 310, 311, 312, -1, -1, -1,
			0, 317, -1, -1, -1, 321, -1, -1, -1, 325,
			-1, -1, -1, -1, -1, -1, -1, 333, -1, -1,
			336, -1, 338, 339, -1, -1, -1, 343, 344, -1,
			346, 347, 348, 349, 350, 351, 352, 353, 354, 355,
			356, -1, -1, -1, -1, -1, 362, 363, 364, -1,
			366, 367, -1, -1, -1, 371, 372, -1, 374, 375,
			376, 377, 378, 379, 380, 1289, 382, 383, -1, 385,
			386, 387, 388, 389, 390, 391, 392, 393, 394, -1,
			396, 397, 398, 399, 400, 401, 402, 403, 404, 405,
			406, 407, 408, 409, 410, 411, 412, 413, 414, 415,
			416, 417, -1, -1, 420, -1, 422, -1, 424, -1,
			-1, 427, 257, -1, -1, -1, 261, 433, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 272, -1, -1,
			1354, -1, 277, -1, -1, -1, 281, -1, -1, 284,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 296, 297, 1377, -1, -1, 301, 302, -1, -1,
			-1, -1, 307, -1, 309, 310, 311, 312, -1, -1,
			-1, 0, 317, -1, -1, -1, 321, -1, 323, -1,
			357, -1, -1, -1, -1, -1, 363, 364, 333, -1,
			-1, 336, -1, 338, 371, -1, 373, 342, 375, 376,
			377, 378, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 390, 1438, -1, 1440, 362, -1, 396,
			397, -1, -1, 1447, -1, -1, 371, 372, 373, -1,
			-1, -1, 1456, 1457, -1, 1459, -1, -1, -1, 416,
			-1, -1, -1, -1, -1, 422, -1, 424, -1, -1,
			427, -1, -1, -1, -1, -1, 256, 257, -1, -1,
			1484, -1, -1, 1487, 264, 265, 266, 267, 268, -1,
			270, 271, -1, 273, 274, 275, 276, 277, 278, 279,
			280, -1, -1, -1, -1, 285, -1, 287, 288, 289,
			290, 291, 292, -1, -1, 295, -1, -1, -1, 299,
			300, 0, 302, 303, 304, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 314, -1, 316, -1, 318, 319,
			-1, -1, 322, -1, 324, 325, 326, 327, 328, 329,
			330, 331, 332, 333, 334, 335, -1, 337, -1, -1,
			340, 341, -1, -1, 344, 345, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 359,
			360, 361, 362, 363, -1, -1, 366, 367, -1, -1,
			-1, 371, 372, -1, -1, 375, -1, -1, -1, -1,
			380, 381, 382, 383, 384, -1, -1, -1, 388, -1,
			390, -1, -1, -1, -1, -1, 396, 397, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 421, 422, 423, 424, -1, 426, 256, 257, -1,
			-1, -1, -1, 433, -1, 264, 265, 266, 267, 268,
			-1, 270, 271, -1, 273, 274, 275, 276, 277, 278,
			279, 0, -1, -1, -1, -1, 285, -1, 287, 288,
			289, 290, 291, 292, -1, -1, 295, -1, -1, -1,
			299, 300, -1, 302, 303, 304, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 314, -1, 316, -1, 318,
			319, -1, -1, 322, -1, 324, 325, 326, 327, 328,
			329, 330, 331, 332, 333, 334, 335, -1, 337, -1,
			-1, 340, 341, -1, -1, 344, 345, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			359, 360, 361, 362, 363, -1, -1, 366, 367, -1,
			-1, -1, 371, 372, -1, -1, 375, -1, -1, -1,
			-1, 380, 381, 382, 383, 384, -1, 256, -1, 388,
			-1, 390, 261, 262, -1, -1, -1, 396, 397, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 284, -1, -1, -1, -1,
			-1, -1, 421, 422, 423, 424, -1, 426, 297, 298,
			-1, 0, -1, 302, 433, -1, 305, -1, 307, -1,
			309, 310, 311, 312, -1, -1, -1, -1, 317, -1,
			-1, -1, 321, -1, -1, -1, 325, -1, -1, -1,
			-1, -1, -1, -1, 333, -1, -1, 336, -1, 338,
			339, -1, -1, -1, 343, 344, -1, 346, 347, 348,
			349, 350, 351, 352, 353, 354, 355, 356, 357, -1,
			-1, -1, -1, 362, 363, -1, -1, 366, 367, -1,
			-1, -1, 371, 372, 373, 374, 375, 376, 377, 378,
			379, 380, -1, 382, 383, -1, -1, 386, 387, 388,
			389, 390, -1, -1, 393, 394, -1, -1, -1, 398,
			399, 400, 401, 402, 403, 404, 405, 256, -1, -1,
			-1, -1, 261, 262, -1, -1, -1, -1, 417, -1,
			-1, 420, -1, 422, -1, 424, -1, -1, 427, -1,
			-1, -1, -1, -1, 433, 284, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 297, 298,
			-1, 0, -1, 302, -1, -1, 305, -1, 307, -1,
			309, 310, 311, 312, -1, -1, -1, -1, 317, -1,
			-1, -1, 321, -1, -1, -1, 325, -1, -1, -1,
			-1, -1, -1, -1, 333, -1, -1, 336, -1, 338,
			339, -1, -1, -1, 343, 344, -1, 346, 347, 348,
			349, 350, 351, 352, 353, 354, 355, 356, -1, -1,
			-1, -1, -1, 362, 363, -1, -1, 366, 367, -1,
			-1, -1, 371, 372, 373, 374, 375, 376, -1, 378,
			379, 380, -1, 382, 383, -1, -1, 386, 387, 388,
			389, -1, -1, -1, 393, 394, -1, -1, -1, 398,
			399, 400, 401, 402, 403, 404, 405, 256, -1, -1,
			-1, -1, 261, 262, -1, -1, -1, -1, 417, -1,
			-1, 420, -1, 422, -1, 424, -1, -1, 427, -1,
			-1, -1, -1, -1, 433, 284, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 297, 298,
			-1, -1, -1, 302, -1, -1, 305, -1, 307, -1,
			309, 310, 311, 312, -1, -1, -1, -1, 317, -1,
			0, -1, 321, -1, -1, -1, 325, -1, -1, -1,
			-1, -1, -1, -1, 333, -1, -1, 336, -1, 338,
			339, -1, -1, -1, 343, 344, -1, 346, 347, 348,
			349, 350, 351, 352, 353, 354, 355, 356, -1, -1,
			-1, -1, -1, 362, 363, -1, -1, 366, 367, -1,
			-1, -1, 371, 372, 373, 374, 375, 376, -1, 378,
			379, 380, -1, 382, 383, -1, -1, 386, 387, 388,
			389, -1, -1, -1, 393, 394, -1, -1, -1, 398,
			399, 400, 401, 402, 403, 404, 405, 256, -1, -1,
			-1, -1, 261, 262, -1, -1, -1, -1, 417, -1,
			-1, 420, 0, 422, -1, 424, -1, -1, 427, -1,
			-1, -1, -1, -1, 433, 284, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 297, 298,
			-1, -1, -1, 302, -1, -1, 305, -1, 307, -1,
			309, 310, 311, 312, -1, -1, -1, -1, 317, 0,
			-1, -1, 321, -1, -1, -1, 325, -1, -1, -1,
			-1, -1, -1, -1, 333, -1, -1, 336, -1, 338,
			339, -1, -1, -1, 343, 344, -1, 346, 347, 348,
			349, 350, 351, 352, 353, 354, 355, 356, -1, -1,
			-1, -1, -1, 362, 363, -1, 0, 366, 367, -1,
			-1, -1, 371, 372, -1, 374, 375, 376, -1, 378,
			379, 380, -1, 382, 383, -1, -1, 386, 387, 388,
			389, -1, -1, -1, 393, 394, -1, -1, -1, 398,
			399, 400, 401, 402, 403, 404, 405, -1, -1, -1,
			-1, -1, -1, 0, -1, -1, -1, -1, 417, -1,
			-1, 420, -1, 422, -1, -1, 256, 257, -1, -1,
			-1, 261, -1, -1, 433, 265, -1, 267, -1, -1,
			270, -1, 272, 273, -1, 275, -1, 277, -1, 279,
			-1, 281, 282, 283, 284, -1, -1, 287, 288, -1,
			0, -1, -1, 293, -1, 295, 296, 297, -1, -1,
			300, 301, 302, -1, 304, -1, -1, 307, -1, 309,
			310, 311, 312, -1, -1, -1, 316, 317, 318, -1,
			-1, 321, 322, 323, -1, -1, -1, -1, -1, -1,
			330, 331, -1, 333, 334, -1, 336, 337, 338, -1,
			-1, -1, 342, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 0, -1, -1, -1, -1, -1, 257,
			-1, -1, 362, 261, -1, -1, -1, -1, -1, -1,
			-1, 371, 372, 373, 272, -1, -1, -1, -1, 277,
			-1, 381, -1, 281, -1, -1, 284, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 0, -1, 296, 297,
			-1, -1, -1, 301, 302, -1, 257, -1, -1, 307,
			261, 309, 310, 311, 312, -1, -1, -1, -1, 317,
			-1, 272, 422, 321, -1, 323, 277, -1, -1, -1,
			281, -1, -1, 284, -1, 333, -1, 335, 336, 0,
			338, -1, -1, -1, 342, 296, 297, -1, -1, -1,
			301, 302, -1, 257, -1, -1, 307, 261, 309, 310,
			311, 312, -1, -1, 362, -1, 317, -1, 272, -1,
			321, -1, 323, 277, -1, 373, -1, 281, -1, -1,
			284, -1, 333, -1, -1, 336, -1, 338, -1, -1,
			-1, 342, 296, 297, -1, -1, -1, 301, 302, -1,
			257, -1, 0, 307, 261, 309, 310, 311, 312, -1,
			-1, 362, -1, 317, -1, 272, -1, 321, -1, 323,
			277, 372, 373, -1, 281, -1, -1, 284, -1, 333,
			-1, -1, 336, -1, 338, -1, -1, -1, 342, 296,
			297, -1, -1, -1, 301, 302, -1, 257, -1, 0,
			307, 261, 309, 310, 311, 312, -1, -1, 362, -1,
			317, -1, 272, -1, 321, -1, 323, 277, 372, 373,
			-1, 281, -1, -1, 284, -1, 333, -1, -1, 336,
			-1, 338, -1, -1, -1, 342, 296, 297, -1, -1,
			-1, 301, 302, -1, -1, -1, -1, 307, -1, 309,
			310, 311, 312, -1, -1, 362, -1, 317, -1, -1,
			257, 321, -1, 323, 261, -1, 373, -1, -1, -1,
			-1, -1, -1, 333, -1, 272, 336, -1, 338, -1,
			277, -1, 342, -1, 281, -1, -1, 284, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 296,
			297, -1, 362, 257, 301, 302, -1, 261, -1, -1,
			307, -1, 309, 310, 311, 312, -1, -1, 272, -1,
			317, -1, -1, 277, 321, -1, 323, 281, -1, -1,
			284, -1, -1, -1, -1, -1, 333, -1, -1, 336,
			-1, 338, 296, 297, -1, 342, 257, 301, 302, -1,
			261, -1, -1, 307, -1, 309, 310, 311, 312, -1,
			-1, 272, -1, 317, -1, 362, 277, 321, -1, 323,
			281, -1, -1, 284, -1, -1, -1, -1, -1, 333,
			-1, -1, 336, -1, 338, 296, 297, -1, 342, -1,
			301, 302, -1, -1, -1, -1, 307, -1, 309, 310,
			311, 312, -1, -1, -1, -1, 317, -1, 362, 257,
			321, -1, 323, 261, -1, -1, -1, -1, -1, -1,
			-1, -1, 333, -1, 272, 336, -1, 338, -1, 277,
			-1, 342, -1, 281, -1, -1, 284, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 296, 297,
			-1, 362, -1, 301, 302, -1, 257, -1, -1, 307,
			261, 309, 310, 311, 312, -1, -1, -1, -1, 317,
			-1, 272, -1, 321, -1, 323, 277, -1, -1, -1,
			281, -1, -1, 284, -1, 333, -1, -1, 336, -1,
			338, -1, -1, -1, 342, 296, 297, -1, -1, -1,
			301, 302, -1, -1, -1, -1, 307, -1, 309, 310,
			311, 312, -1, -1, 362, -1, 317, -1, -1, -1,
			321, -1, 323, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 333, -1, 256, 336, -1, 338, -1, -1,
			-1, 342, 264, 265, 266, 267, -1, -1, 270, 271,
			-1, 273, 274, 275, 276, 277, 278, 279, -1, -1,
			-1, 362, -1, 285, -1, 287, 288, 289, 290, 291,
			292, -1, -1, 295, -1, -1, -1, 299, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 314, -1, 316, -1, 318, 319, -1, -1,
			322, -1, 324, 325, 326, 327, 328, 329, 330, 331,
			332, 333, 334, 335, -1, 337, -1, -1, 340, 341,
			-1, -1, 344, 345, -1, 256, -1, -1, -1, -1,
			-1, 262, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, 371,
			-1, -1, -1, 375, -1, -1, -1, -1, 380, 381,
			382, 383, 384, -1, -1, -1, 388, 298, 390, -1,
			-1, -1, -1, -1, 396, 397, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 256, -1, -1, -1, -1, -1, 262, 421,
			422, 423, 424, -1, -1, -1, -1, -1, 339, -1,
			-1, 433, -1, 344, -1, 346, 347, 348, 349, 350,
			351, 352, 353, 354, 355, 356, 357, -1, -1, -1,
			-1, -1, 363, 364, 298, 366, 367, -1, -1, -1,
			371, 372, 373, 374, 375, 376, 377, 378, 379, 380,
			-1, 382, 383, -1, 385, 386, 387, 388, 389, 390,
			391, 392, 393, 394, -1, 396, 397, 398, 399, 400,
			401, 402, 403, 404, 405, 406, 407, 408, 409, 410,
			411, 412, 413, 414, 415, 416, 417, 418, -1, 256,
			-1, 422, -1, 424, 425, 262, 427, -1, -1, 363,
			364, -1, 433, -1, -1, -1, -1, -1, 372, 373,
			374, 375, 376, 377, 378, -1, 380, -1, 382, 383,
			-1, 385, 386, 387, 388, 389, -1, 391, 392, 393,
			394, 298, 396, 397, 398, 399, 400, 401, 402, 403,
			404, 405, 406, 407, 408, 409, 410, 411, 412, 413,
			414, 415, 416, 417, -1, -1, -1, -1, 422, -1,
			424, -1, -1, 427, -1, -1, -1, -1, -1, 433,
			-1, -1, 339, -1, -1, -1, -1, 344, -1, 346,
			347, 348, 349, 350, 351, 352, 353, 354, 355, 356,
			357, -1, -1, -1, -1, -1, 363, 364, -1, 366,
			367, -1, -1, -1, 371, 372, 373, 374, 375, 376,
			377, 378, 379, 380, 256, 382, 383, -1, 385, 386,
			387, 388, 389, 390, 391, 392, 393, 394, -1, 396,
			397, 398, 399, 400, 401, 402, 403, 404, 405, 406,
			407, 408, 409, 410, 411, 412, 413, 414, 415, 416,
			417, 418, 256, -1, -1, 422, -1, 424, 262, -1,
			427, -1, -1, -1, -1, -1, 433, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 298, -1, -1, 339, -1, -1,
			-1, -1, 344, -1, 346, 347, 348, 349, 350, 351,
			352, 353, 354, 355, 356, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 366, 367, -1, -1, -1, -1,
			372, -1, 374, -1, 376, 339, 378, 379, 380, -1,
			344, -1, 346, 347, 348, 349, 350, 351, 352, 353,
			354, 355, 356, 357, -1, -1, -1, -1, -1, 363,
			364, -1, 366, 367, -1, -1, -1, 371, 372, 373,
			374, 375, 376, 377, 378, 379, 380, 256, 382, 383,
			-1, 385, 386, 387, 388, 389, 390, 391, 392, 393,
			394, 433, 396, 397, 398, 399, 400, 401, 402, 403,
			404, 405, 406, 407, 408, 409, 410, 411, 412, 413,
			414, 415, 416, 417, -1, 256, -1, -1, 422, -1,
			424, 262, -1, 427, -1, -1, -1, -1, -1, 433,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 298, -1, -1,
			339, -1, -1, -1, -1, 344, -1, 346, 347, 348,
			349, 350, 351, 352, 353, 354, 355, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 366, 367, -1,
			-1, -1, -1, 372, -1, 374, -1, 376, 339, 378,
			379, 380, -1, 344, -1, 346, 347, 348, 349, 350,
			351, 352, 353, 354, 355, 356, 357, -1, -1, -1,
			-1, -1, 363, 364, -1, 366, 367, -1, -1, -1,
			-1, 372, 373, 374, 375, 376, 377, 378, 379, 380,
			-1, 382, 383, -1, 385, 386, 387, 388, 389, 390,
			391, 392, 393, 394, 433, 396, 397, 398, 399, 400,
			401, 402, 403, 404, 405, 406, 407, 408, 409, 410,
			411, 412, 413, 414, 415, 416, 417, -1, 256, 256,
			-1, 422, -1, 424, 262, -1, 427, -1, 265, -1,
			267, -1, 433, 270, -1, -1, -1, -1, 275, -1,
			-1, -1, 279, -1, -1, -1, -1, -1, -1, -1,
			-1, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			298, -1, -1, 300, -1, -1, -1, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, -1, -1, -1, 322, -1, -1, -1, -1,
			-1, -1, -1, 330, 331, -1, -1, 334, -1, -1,
			337, 339, -1, -1, -1, -1, 344, -1, 346, 347,
			348, 349, 350, 351, 352, 353, 354, 355, 356, -1,
			-1, 256, -1, -1, -1, -1, 364, -1, 366, 367,
			-1, -1, -1, 371, 372, 373, 374, 375, 376, 377,
			378, 379, 380, -1, 382, 383, -1, 385, 386, 387,
			388, 389, 390, 391, 392, 393, 394, -1, 396, 397,
			398, 399, 400, 401, 402, 403, 404, 405, 406, 407,
			408, 409, 410, 411, 412, 413, 414, 415, 416, 417,
			-1, 256, -1, -1, -1, 422, 424, 262, -1, -1,
			-1, -1, -1, -1, -1, 433, -1, -1, -1, -1,
			-1, -1, -1, -1, 339, -1, -1, -1, -1, 344,
			-1, 346, 347, 348, 349, 350, 351, 352, 353, 354,
			355, 356, -1, 298, -1, -1, -1, -1, -1, -1,
			-1, 366, 367, -1, -1, -1, -1, 372, -1, 374,
			-1, 376, -1, 378, 379, 380, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 394,
			-1, -1, -1, -1, 339, -1, -1, -1, -1, 344,
			405, 346, 347, 348, 349, 350, 351, 352, 353, 354,
			355, 356, 417, -1, 256, -1, -1, -1, -1, 364,
			-1, 366, 367, -1, -1, -1, 371, 372, 433, 374,
			375, 376, 377, 378, 379, 380, -1, 382, 383, -1,
			385, 386, 387, 388, 389, 390, 391, 392, 393, 394,
			-1, 396, 397, 398, 399, 400, 401, 402, 403, 404,
			405, 406, 407, 408, 409, 410, 411, 412, 413, 414,
			415, 416, 417, -1, 256, -1, -1, -1, -1, 424,
			262, -1, 427, -1, -1, -1, -1, -1, 433, -1,
			-1, -1, -1, -1, -1, -1, -1, 339, -1, -1,
			-1, -1, 344, -1, 346, 347, 348, 349, 350, 351,
			352, 353, 354, 355, 356, -1, 298, -1, -1, -1,
			-1, -1, -1, -1, 366, 367, -1, -1, -1, -1,
			372, -1, 374, -1, 376, -1, 378, 379, 380, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 394, -1, -1, -1, -1, 339, -1, -1,
			-1, -1, 344, 405, 346, 347, 348, 349, 350, 351,
			352, 353, 354, 355, 356, 417, -1, -1, -1, -1,
			-1, -1, 364, -1, 366, 367, -1, -1, -1, -1,
			372, 433, 374, 375, 376, 377, 378, 379, 380, -1,
			382, 383, -1, 385, 386, 387, 388, 389, 390, 391,
			392, 393, 394, -1, 396, 397, 398, 399, 400, 401,
			402, 403, 404, 405, 406, 407, 408, 409, 410, 411,
			412, 413, 414, 415, 416, 417, -1, 256, 256, -1,
			-1, -1, 424, 262, -1, 427, -1, 265, -1, 267,
			-1, 433, 270, -1, -1, -1, -1, 275, -1, -1,
			-1, 279, -1, -1, -1, -1, -1, -1, -1, -1,
			288, -1, -1, -1, -1, -1, -1, 295, -1, 298,
			-1, -1, 300, -1, -1, -1, 304, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 316, -1,
			318, -1, -1, -1, 322, -1, -1, -1, -1, -1,
			-1, -1, 330, 331, -1, -1, 334, -1, -1, 337,
			339, -1, -1, -1, -1, 344, -1, 346, 347, 348,
			349, 350, 351, 352, 353, 354, 355, 356, -1, -1,
			-1, -1, -1, -1, -1, 364, -1, 366, 367, -1,
			-1, -1, -1, 372, -1, 374, 375, 376, 377, 378,
			379, 380, -1, 382, 383, -1, 385, 386, 387, 388,
			389, 390, 391, 392, 393, 394, -1, 396, 397, 398,
			399, 400, 401, 402, 403, 404, 405, 406, 407, 408,
			409, 410, 411, 412, 413, 414, 415, 416, 417, -1,
			256, 256, -1, -1, 422, 424, 262, -1, 427, -1,
			265, -1, 267, -1, 433, 270, -1, -1, -1, -1,
			275, -1, -1, -1, 279, -1, -1, -1, -1, -1,
			-1, -1, -1, 288, -1, -1, -1, -1, -1, -1,
			295, -1, 298, -1, -1, 300, -1, -1, -1, 304,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 316, -1, 318, -1, -1, -1, 322, -1, -1,
			-1, -1, -1, -1, -1, 330, 331, -1, -1, 334,
			-1, -1, 337, 339, -1, -1, -1, -1, 344, -1,
			346, 347, 348, 349, 350, 351, 352, 353, 354, 355,
			356, -1, -1, -1, -1, -1, -1, -1, 364, -1,
			366, 367, -1, -1, -1, -1, 372, -1, 374, 375,
			376, 377, 378, 379, 380, -1, 382, 383, -1, 385,
			386, 387, 388, 389, 390, 391, 392, 393, 394, -1,
			396, 397, 398, 399, 400, 401, 402, 403, 404, 405,
			406, 407, 408, 409, 410, 411, 412, 413, 414, 415,
			416, 417, -1, 256, -1, 261, -1, 422, 424, 262,
			-1, 427, -1, -1, -1, -1, -1, 433, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 284, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 297, -1, -1, -1, 298, 302, -1, -1, 305,
			-1, 307, -1, 309, 310, 311, 312, -1, -1, -1,
			-1, 317, -1, -1, -1, 321, 256, -1, -1, 325,
			-1, -1, 262, -1, -1, -1, 266, 333, -1, -1,
			336, -1, 338, -1, -1, -1, 339, -1, -1, -1,
			-1, 344, -1, 346, 347, 348, 349, 350, 351, 352,
			353, 354, 355, 356, -1, -1, 362, -1, 298, -1,
			-1, -1, -1, 366, 367, -1, 372, -1, -1, 372,
			-1, 374, -1, 376, 314, 378, 379, 380, -1, 382,
			383, -1, 385, 386, 387, 388, 389, 390, 391, 392,
			393, 394, -1, -1, -1, 398, 399, 400, 401, 402,
			403, 404, 405, 406, 407, 408, 409, 410, 411, 412,
			413, 414, 415, 256, 417, -1, 422, 357, -1, 262,
			-1, -1, -1, 363, 364, -1, -1, -1, -1, -1,
			433, -1, 372, 373, 374, 375, 376, 377, 378, 379,
			380, -1, 382, 383, -1, 385, 386, 387, 388, 389,
			390, 391, 392, 393, 394, 298, 396, 397, 398, 399,
			400, 401, 402, 403, 404, 405, 406, 407, 408, 409,
			410, 411, 412, 413, 414, 415, 416, 417, -1, -1,
			-1, -1, 422, -1, 424, -1, -1, 427, -1, -1,
			-1, -1, -1, 433, -1, -1, 339, -1, -1, -1,
			-1, 344, -1, 346, 347, 348, 349, 350, 351, 352,
			353, 354, 355, 356, -1, -1, -1, -1, -1, -1,
			363, 364, 256, 366, 367, -1, -1, -1, 262, 372,
			373, 374, -1, 376, 377, 378, 379, 380, -1, 382,
			383, -1, -1, 386, 387, 388, 389, -1, -1, -1,
			393, 394, -1, 396, 397, 398, 399, 400, 401, 402,
			403, 404, 405, -1, 298, -1, -1, -1, -1, -1,
			-1, -1, -1, 416, 417, -1, -1, -1, -1, 422,
			-1, 424, -1, -1, 427, -1, -1, -1, -1, -1,
			433, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 339, -1, -1, -1, -1,
			344, -1, 346, 347, 348, 349, 350, 351, 352, 353,
			354, 355, 356, -1, -1, -1, -1, -1, -1, -1,
			364, 256, 366, 367, -1, -1, -1, 262, 372, -1,
			374, 375, 376, 377, 378, 379, 380, -1, 382, 383,
			-1, -1, 386, 387, 388, 389, -1, -1, -1, 393,
			394, -1, 396, 397, 398, 399, 400, 401, 402, 403,
			404, 405, -1, 298, -1, -1, -1, -1, -1, -1,
			-1, -1, 416, 417, -1, -1, -1, -1, -1, -1,
			424, -1, -1, 427, -1, -1, -1, -1, -1, 433,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 339, -1, -1, -1, -1, 344,
			-1, 346, 347, 348, 349, 350, 351, 352, 353, 354,
			355, 356, -1, -1, -1, -1, -1, -1, -1, 364,
			256, 366, 367, -1, -1, -1, 262, 372, -1, 374,
			375, 376, 377, 378, 379, 380, -1, 382, 383, -1,
			-1, 386, 387, 388, 389, -1, -1, -1, 393, 394,
			-1, 396, 397, 398, 399, 400, 401, 402, 403, 404,
			405, -1, 298, -1, -1, -1, -1, -1, -1, -1,
			-1, 416, 417, -1, -1, -1, -1, -1, -1, 424,
			-1, -1, 427, -1, -1, -1, -1, -1, 433, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 339, -1, -1, -1, -1, 344, -1,
			346, 347, 348, 349, 350, 351, 352, 353, 354, 355,
			356, -1, -1, -1, -1, -1, -1, -1, 364, 256,
			366, 367, -1, -1, -1, 262, 372, -1, 374, 375,
			376, 377, 378, 379, 380, -1, 382, 383, -1, -1,
			386, 387, 388, 389, -1, -1, -1, 393, 394, -1,
			396, 397, 398, 399, 400, 401, 402, 403, 404, 405,
			-1, 298, -1, -1, -1, -1, -1, -1, -1, -1,
			416, 417, -1, -1, -1, -1, -1, -1, 424, -1,
			-1, 427, -1, -1, -1, -1, -1, 433, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 339, -1, -1, -1, -1, 344, -1, 346,
			347, 348, 349, 350, 351, 352, 353, 354, 355, 356,
			-1, -1, -1, -1, -1, -1, 363, 256, -1, 366,
			367, -1, -1, 262, -1, 372, 373, 374, -1, 376,
			-1, 378, 379, 380, -1, 382, 383, -1, -1, 386,
			387, 388, 389, -1, -1, -1, 393, 394, -1, -1,
			-1, 398, 399, 400, 401, 402, 403, 404, 405, 298,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			417, -1, -1, -1, -1, 422, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 433, -1, -1, 256,
			-1, -1, -1, -1, -1, 262, -1, -1, -1, -1,
			339, -1, -1, -1, -1, 344, -1, 346, 347, 348,
			349, 350, 351, 352, 353, 354, 355, 356, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 366, 367, -1,
			-1, 298, -1, 372, -1, 374, -1, 376, -1, 378,
			379, 380, -1, 382, 383, -1, -1, 386, 387, 388,
			389, 390, 391, 392, 393, 394, -1, -1, -1, 398,
			399, 400, 401, 402, 403, 404, 405, -1, -1, -1,
			-1, -1, 339, -1, -1, -1, -1, 344, 417, 346,
			347, 348, 349, 350, 351, 352, 353, 354, 355, 356,
			-1, -1, -1, -1, 433, -1, 363, 256, -1, 366,
			367, -1, -1, 262, -1, 372, -1, 374, -1, 376,
			-1, 378, 379, 380, -1, 382, 383, -1, -1, 386,
			387, 388, 389, -1, -1, -1, 393, 394, -1, -1,
			-1, 398, 399, 400, 401, 402, 403, 404, 405, 298,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			417, -1, -1, -1, -1, 422, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 433, -1, -1, 256,
			-1, -1, -1, -1, -1, 262, -1, -1, -1, -1,
			339, -1, -1, -1, -1, 344, -1, 346, 347, 348,
			349, 350, 351, 352, 353, 354, 355, 356, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 366, 367, -1,
			-1, 298, -1, 372, -1, 374, -1, 376, -1, 378,
			379, 380, -1, 382, 383, -1, -1, 386, 387, 388,
			389, -1, -1, -1, 393, 394, -1, -1, -1, 398,
			399, 400, 401, 402, 403, 404, 405, -1, -1, -1,
			-1, -1, 339, -1, -1, -1, -1, 344, 417, 346,
			347, 348, 349, 350, 351, 352, 353, 354, 355, 356,
			-1, -1, -1, -1, 433, -1, -1, 256, -1, 366,
			367, -1, -1, 262, -1, 372, -1, 374, -1, 376,
			-1, 378, 379, 380, -1, 382, 383, -1, -1, 386,
			387, 388, 389, -1, -1, -1, 393, 394, -1, -1,
			-1, 398, 399, 400, 401, 402, 403, 404, 405, 298,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			417, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 433, -1, -1, 256,
			-1, -1, -1, -1, -1, 262, -1, -1, -1, -1,
			339, -1, -1, -1, -1, 344, -1, 346, 347, 348,
			349, 350, 351, 352, 353, 354, 355, 356, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 366, 367, -1,
			-1, 298, -1, 372, -1, 374, -1, 376, -1, 378,
			379, 380, -1, 382, 383, -1, -1, 386, 387, 388,
			389, -1, -1, -1, 393, 394, -1, -1, -1, 398,
			399, 400, 401, 402, 403, 404, 405, -1, -1, -1,
			-1, -1, 339, -1, -1, -1, -1, 344, 417, 346,
			347, 348, 349, 350, 351, 352, 353, 354, 355, 356,
			-1, -1, -1, -1, 433, -1, -1, -1, -1, 366,
			367, -1, -1, -1, -1, 372, -1, 374, -1, 376,
			-1, 378, 379, 380, -1, 382, 383, -1, -1, 386,
			387, 388, 389, -1, -1, -1, 393, 394, -1, -1,
			-1, 398, 399, 400, 401, 402, 403, 404, 405, -1,
			256, -1, 256, -1, -1, -1, -1, -1, 264, 265,
			417, 267, -1, -1, 270, 271, -1, -1, -1, 275,
			276, 277, -1, 279, -1, -1, 433, -1, -1, 285,
			-1, -1, 288, -1, -1, -1, -1, -1, -1, 295,
			-1, -1, -1, -1, 300, -1, 302, 303, 304, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			316, -1, 318, 319, -1, -1, 322, -1, -1, 325,
			-1, 327, -1, 329, 330, 331, 332, -1, 334, -1,
			-1, -1, -1, -1, -1, 339, -1, -1, -1, -1,
			344, -1, 346, 347, 348, 349, 350, 351, 352, 353,
			354, 355, 356, 359, 360, 361, 362, 363, 256, -1,
			366, 367, 366, 367, -1, -1, -1, -1, 372, 375,
			374, -1, 376, -1, 378, 379, 380, -1, -1, -1,
			-1, -1, 386, 387, 388, 389, -1, -1, -1, 393,
			394, -1, -1, -1, 398, 399, 400, 401, 402, 403,
			404, 405, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 417, -1, 421, 422, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 432, 433, -1, 433,
			256, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 339, -1, -1, -1, -1, 344, -1, 346, 347,
			348, 349, 350, 351, 352, 353, 354, 355, 356, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 366, 367,
			-1, -1, -1, -1, 372, -1, 374, -1, 376, -1,
			378, 379, 380, -1, -1, -1, -1, -1, 386, 387,
			388, 389, -1, -1, -1, 393, 394, -1, -1, -1,
			398, 399, 400, 401, 402, 403, 404, 405, -1, -1,
			-1, -1, -1, 339, -1, -1, -1, -1, 344, 417,
			346, 347, 348, 349, 350, 351, 352, 353, 354, 355,
			356, 256, -1, -1, -1, 433, -1, -1, -1, -1,
			366, 367, -1, -1, -1, -1, 372, -1, 374, -1,
			376, -1, 378, 379, 380, -1, -1, -1, -1, -1,
			386, 387, 388, 389, -1, -1, -1, 393, 394, -1,
			-1, -1, 398, 399, 400, 401, 402, 403, 404, 405,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 417, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 256, -1, -1, -1, 433, -1, -1,
			-1, -1, -1, -1, 339, -1, -1, -1, -1, 344,
			-1, 346, 347, 348, 349, 350, 351, 352, 353, 354,
			355, 356, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 366, 367, -1, -1, -1, -1, 372, -1, 374,
			-1, 376, -1, 378, 379, 380, -1, -1, -1, -1,
			-1, 386, 387, 388, 389, -1, -1, -1, 393, 394,
			-1, -1, -1, -1, -1, 400, 401, 402, 403, 404,
			405, -1, -1, -1, -1, -1, 339, -1, -1, -1,
			-1, 344, 417, 346, 347, 348, 349, 350, 351, 352,
			353, 354, 355, 356, 256, -1, -1, -1, 433, -1,
			-1, -1, -1, 366, 367, -1, -1, -1, -1, 372,
			-1, 374, -1, 376, -1, 378, 379, 380, -1, -1,
			-1, -1, -1, 386, 387, 388, 389, -1, -1, -1,
			393, 394, -1, -1, -1, -1, -1, 400, 401, 402,
			403, 404, 405, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 417, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 256, -1, -1, -1,
			433, -1, -1, -1, -1, -1, -1, 339, -1, -1,
			-1, -1, 344, -1, 346, 347, 348, 349, 350, 351,
			352, 353, 354, 355, 356, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 366, 367, -1, -1, -1, -1,
			372, -1, 374, -1, 376, -1, 378, 379, 380, -1,
			-1, -1, -1, -1, 386, 387, 388, 389, -1, -1,
			-1, 393, 394, -1, -1, -1, -1, -1, 400, 401,
			402, 403, 404, 405, -1, -1, -1, -1, -1, 339,
			-1, -1, -1, -1, 344, 417, 346, 347, 348, 349,
			350, 351, 352, 353, 354, 355, 356, 256, -1, -1,
			-1, 433, -1, -1, -1, -1, 366, 367, -1, -1,
			-1, -1, 372, -1, 374, -1, 376, -1, 378, 379,
			380, -1, -1, -1, -1, -1, 386, 387, 388, 389,
			-1, -1, -1, 393, 394, -1, -1, -1, -1, -1,
			400, 401, 402, 403, 404, 405, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 256, 417, -1, -1,
			-1, -1, 262, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 433, -1, -1, -1, -1, -1, -1,
			339, -1, -1, -1, -1, 344, -1, 346, 347, 348,
			349, 350, 351, 352, 353, 354, 355, 356, 298, -1,
			-1, -1, -1, -1, -1, -1, -1, 366, 367, -1,
			-1, -1, -1, 372, -1, 374, -1, 376, -1, 378,
			379, 380, -1, -1, -1, -1, -1, 386, 387, 388,
			389, -1, -1, -1, 393, 394, -1, -1, -1, -1,
			-1, 400, 401, 402, 403, 404, 405, 256, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 417, -1,
			-1, -1, -1, -1, 364, -1, -1, -1, -1, -1,
			-1, -1, 372, -1, 433, 375, -1, 377, 378, -1,
			-1, -1, 382, 383, -1, -1, 386, 387, 388, 389,
			390, 391, 392, 393, 394, -1, 396, 397, 398, 399,
			400, 401, 402, 403, 404, 405, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 416, 417, -1, 256,
			-1, -1, -1, -1, 424, -1, -1, 427, -1, -1,
			339, -1, -1, 433, -1, 344, -1, 346, 347, 348,
			349, 350, 351, 352, 353, 354, 355, 356, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 366, 367, -1,
			-1, -1, -1, 372, -1, 374, -1, 376, -1, 378,
			379, 380, -1, -1, -1, -1, -1, -1, -1, 388,
			389, -1, -1, -1, 393, 394, -1, -1, -1, -1,
			-1, 256, -1, 402, 403, 404, 405, -1, -1, -1,
			-1, -1, 339, -1, -1, -1, -1, 344, 417, 346,
			347, 348, 349, 350, 351, 352, 353, 354, 355, 356,
			-1, -1, -1, -1, 433, -1, -1, -1, -1, 366,
			367, -1, -1, -1, -1, 372, -1, 374, -1, 376,
			-1, 378, 379, 380, -1, -1, -1, -1, -1, -1,
			-1, 388, 389, -1, -1, -1, 393, 394, -1, -1,
			-1, -1, -1, 256, -1, 402, 403, 404, 405, -1,
			-1, -1, -1, -1, 339, -1, -1, -1, -1, 344,
			417, 346, 347, 348, 349, 350, 351, 352, 353, 354,
			355, 356, -1, -1, -1, -1, 433, -1, -1, -1,
			-1, 366, 367, -1, -1, -1, -1, 372, -1, 374,
			-1, 376, -1, 378, 379, 380, -1, -1, -1, -1,
			-1, -1, -1, 388, 389, -1, -1, -1, 393, 394,
			-1, -1, -1, -1, -1, 256, -1, -1, -1, 404,
			405, -1, -1, -1, -1, -1, 339, -1, -1, -1,
			-1, 344, 417, 346, 347, 348, 349, 350, 351, 352,
			353, 354, 355, 356, -1, -1, -1, -1, 433, -1,
			-1, -1, -1, 366, 367, -1, -1, -1, -1, 372,
			-1, 374, -1, 376, -1, 378, 379, 380, -1, -1,
			-1, -1, -1, -1, -1, 388, 389, -1, -1, -1,
			393, 394, -1, -1, -1, -1, -1, 256, -1, -1,
			-1, 404, 405, -1, -1, -1, -1, -1, 339, -1,
			-1, -1, -1, 344, 417, 346, 347, 348, 349, 350,
			351, 352, 353, 354, 355, 356, -1, -1, -1, -1,
			433, -1, -1, -1, -1, 366, 367, -1, -1, -1,
			-1, 372, -1, 374, -1, 376, -1, 378, 379, 380,
			-1, -1, -1, -1, -1, -1, -1, -1, 389, -1,
			-1, -1, 393, 394, -1, -1, -1, -1, -1, 256,
			-1, -1, -1, 404, 405, -1, -1, -1, -1, -1,
			339, -1, -1, -1, -1, 344, 417, 346, 347, 348,
			349, 350, 351, 352, 353, 354, 355, 356, -1, -1,
			-1, -1, 433, -1, -1, -1, -1, 366, 367, -1,
			-1, -1, -1, 372, -1, 374, -1, 376, -1, 378,
			379, 380, -1, -1, -1, -1, -1, -1, -1, -1,
			389, -1, -1, -1, 393, 394, -1, -1, -1, -1,
			-1, 256, -1, -1, -1, 404, 405, -1, -1, -1,
			-1, -1, 339, -1, -1, -1, -1, 344, 417, 346,
			347, 348, 349, 350, 351, 352, 353, 354, 355, 356,
			-1, -1, -1, -1, 433, -1, -1, -1, -1, 366,
			367, -1, -1, -1, -1, 372, -1, 374, -1, 376,
			-1, 378, 379, 380, -1, -1, -1, -1, -1, -1,
			-1, -1, 389, -1, -1, -1, -1, 394, -1, -1,
			-1, -1, -1, 256, -1, -1, -1, 404, 405, -1,
			-1, -1, -1, -1, 339, -1, -1, -1, -1, 344,
			417, 346, 347, 348, 349, 350, 351, 352, 353, 354,
			355, 356, -1, -1, -1, -1, 433, -1, -1, -1,
			-1, 366, 367, -1, -1, -1, -1, 372, -1, 374,
			-1, 376, -1, 378, 379, 380, -1, -1, -1, -1,
			-1, -1, -1, -1, 389, -1, -1, -1, -1, 394,
			-1, -1, -1, -1, -1, 256, -1, -1, -1, 404,
			405, -1, -1, -1, -1, -1, 339, -1, -1, -1,
			-1, 344, 417, 346, 347, 348, 349, 350, 351, 352,
			353, 354, 355, 356, -1, -1, -1, -1, 433, -1,
			-1, -1, -1, 366, 367, -1, -1, -1, -1, 372,
			-1, 374, -1, 376, -1, 378, 379, 380, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 394, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 404, 405, -1, -1, -1, -1, -1, 339, -1,
			-1, -1, -1, 344, 417, 346, 347, 348, 349, 350,
			351, 352, 353, 354, 355, 356, -1, -1, -1, -1,
			433, -1, -1, -1, -1, 366, 367, -1, -1, -1,
			-1, 372, -1, 374, -1, 376, -1, 378, 379, 380,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 394, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 404, 405, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 417, -1, -1, 256,
			-1, -1, -1, -1, -1, -1, -1, 264, 265, 266,
			267, 268, 433, 270, 271, -1, 273, 274, 275, 276,
			277, 278, 279, -1, -1, -1, -1, -1, 285, -1,
			287, 288, 289, 290, 291, 292, -1, -1, 295, -1,
			-1, -1, 299, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 314, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, 324, 325, 326,
			327, 328, 329, 330, 331, 332, 333, 334, 335, -1,
			337, -1, -1, 340, 341, -1, -1, 344, 345, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, 371, -1, -1, -1, 375, -1,
			-1, -1, -1, 380, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, -1, -1, -1, -1, -1, 396,
			397, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 256, -1, 421, 422, 423, 424, -1, 426,
			264, 265, 266, 267, -1, -1, 270, 271, -1, 273,
			274, 275, 276, 277, 278, 279, -1, -1, -1, -1,
			-1, 285, -1, 287, 288, 289, 290, 291, 292, -1,
			-1, 295, -1, -1, -1, 299, 300, -1, 302, 303,
			304, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			314, -1, 316, -1, 318, 319, -1, -1, 322, -1,
			324, 325, 326, 327, 328, 329, 330, 331, 332, 333,
			334, 335, -1, 337, -1, -1, 340, 341, -1, -1,
			344, 345, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 359, 360, 361, 362, 363,
			-1, -1, 366, 367, -1, -1, -1, 371, -1, -1,
			-1, 375, -1, -1, -1, -1, 380, 381, 382, 383,
			384, -1, -1, -1, 388, -1, 390, -1, -1, -1,
			-1, -1, 396, 397, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 256, -1, -1, -1, 421, 422, 423,
			424, 264, 265, 266, 267, -1, -1, 270, 271, -1,
			273, 274, 275, 276, 277, 278, 279, -1, -1, -1,
			-1, -1, 285, -1, 287, 288, 289, 290, 291, 292,
			-1, -1, 295, -1, -1, -1, 299, 300, -1, 302,
			303, 304, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 314, -1, 316, -1, 318, 319, -1, -1, 322,
			-1, 324, 325, 326, 327, 328, 329, 330, 331, 332,
			333, 334, 335, -1, 337, -1, -1, 340, 341, -1,
			-1, 344, 345, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 359, 360, 361, 362,
			363, -1, -1, 366, 367, -1, -1, -1, 371, -1,
			-1, -1, 375, -1, -1, -1, -1, 380, 381, 382,
			383, 384, -1, -1, -1, 388, -1, 390, -1, -1,
			-1, -1, -1, 396, 397, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 256, -1, -1, -1, 421, 422,
			423, 424, 264, 265, 266, 267, -1, -1, 270, 271,
			-1, 273, 274, 275, 276, 277, 278, 279, -1, -1,
			-1, -1, -1, 285, -1, 287, 288, 289, 290, 291,
			292, -1, -1, 295, -1, -1, -1, 299, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 314, -1, 316, -1, 318, 319, -1, -1,
			322, -1, 324, 325, 326, 327, 328, 329, 330, 331,
			332, 333, 334, 335, -1, 337, -1, -1, 340, 341,
			-1, -1, 344, 345, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, 371,
			-1, -1, -1, 375, -1, -1, -1, -1, 380, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, -1,
			-1, -1, -1, -1, 396, 397, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 256, -1, -1, -1, 421,
			422, 423, 424, 264, 265, 266, 267, -1, -1, 270,
			271, -1, 273, 274, 275, 276, 277, 278, 279, -1,
			-1, -1, -1, -1, 285, -1, 287, 288, 289, 290,
			291, 292, -1, -1, 295, -1, -1, -1, 299, 300,
			-1, 302, 303, 304, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 314, -1, 316, -1, 318, 319, -1,
			-1, 322, -1, 324, 325, 326, 327, 328, 329, 330,
			331, 332, 333, 334, 335, -1, 337, -1, -1, 340,
			341, -1, -1, 344, 345, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 359, 360,
			361, 362, 363, -1, -1, 366, 367, -1, -1, -1,
			371, -1, -1, -1, 375, -1, -1, -1, -1, 380,
			381, 382, 383, 384, -1, -1, -1, 388, -1, 390,
			-1, -1, -1, -1, -1, 396, 397, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 256, -1, -1, -1,
			421, 422, 423, 424, 264, 265, -1, 267, -1, -1,
			270, 271, -1, -1, -1, 275, 276, 277, -1, 279,
			-1, -1, 265, -1, 267, 285, -1, 270, 288, -1,
			-1, -1, 275, -1, -1, 295, 279, -1, -1, -1,
			300, -1, 302, 303, 304, 288, 306, -1, -1, -1,
			-1, -1, 295, 313, -1, -1, 316, 300, 318, 319,
			-1, 304, 322, -1, -1, 325, -1, 327, -1, 329,
			330, 331, 332, 316, 334, 318, -1, -1, -1, 322,
			-1, 341, -1, -1, 344, 345, -1, 330, 331, -1,
			-1, 334, -1, -1, 337, -1, -1, -1, -1, 359,
			360, 361, 362, 363, -1, -1, 366, 367, -1, -1,
			-1, -1, -1, -1, -1, 375, -1, -1, 378, -1,
			363, 381, 382, 383, 384, -1, -1, -1, 388, -1,
			390, -1, -1, -1, -1, -1, 396, 397, -1, -1,
			-1, -1, -1, -1, 264, 265, -1, 267, -1, -1,
			270, 271, -1, -1, -1, 275, 276, 277, -1, 279,
			-1, 421, 422, 423, 424, 285, -1, -1, 288, -1,
			-1, -1, -1, -1, -1, 295, -1, -1, -1, 422,
			300, -1, 302, 303, 304, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 316, -1, 318, 319,
			-1, -1, 322, -1, -1, 325, -1, 327, -1, 329,
			330, 331, 332, -1, 334, -1, -1, -1, -1, -1,
			-1, 341, -1, -1, 344, 345, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 359,
			360, 361, 362, 363, -1, -1, 366, 367, -1, -1,
			-1, 371, 372, -1, -1, 375, -1, -1, -1, -1,
			-1, 381, 382, 383, 384, -1, -1, -1, 388, -1,
			390, -1, -1, -1, -1, -1, 396, 397, -1, -1,
			-1, -1, -1, -1, 264, 265, -1, 267, -1, -1,
			270, 271, -1, -1, -1, 275, 276, 277, -1, 279,
			-1, 421, 422, 423, 424, 285, -1, 427, 288, -1,
			-1, -1, 432, -1, -1, 295, -1, -1, -1, -1,
			300, -1, 302, 303, 304, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 316, -1, 318, 319,
			-1, -1, 322, -1, -1, 325, -1, 327, -1, 329,
			330, 331, 332, -1, 334, -1, -1, -1, -1, -1,
			-1, 341, -1, -1, 344, 345, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 359,
			360, 361, 362, 363, -1, -1, 366, 367, -1, -1,
			-1, 371, -1, -1, -1, 375, -1, -1, -1, -1,
			-1, 381, 382, 383, 384, -1, -1, -1, 388, -1,
			390, -1, -1, -1, -1, -1, 396, 397, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			256, 421, 422, 423, 424, -1, -1, 427, 264, 265,
			-1, 267, 432, -1, 270, 271, -1, -1, -1, 275,
			276, 277, -1, 279, -1, -1, 265, -1, 267, 285,
			-1, 270, 288, -1, -1, -1, 275, -1, -1, 295,
			279, -1, -1, -1, 300, -1, 302, 303, 304, 288,
			-1, -1, -1, -1, -1, -1, 295, -1, -1, -1,
			316, 300, 318, 319, -1, 304, 322, -1, -1, 325,
			-1, 327, -1, 329, 330, 331, 332, 316, 334, 318,
			-1, -1, -1, 322, -1, 341, -1, -1, 344, 345,
			-1, 330, 331, -1, -1, 334, -1, -1, 337, -1,
			-1, -1, -1, 359, 360, 361, 362, 363, -1, -1,
			366, 367, -1, -1, -1, 371, 372, -1, -1, 375,
			-1, -1, -1, -1, -1, 381, 382, 383, 384, -1,
			-1, -1, 388, -1, 390, -1, -1, 376, -1, -1,
			396, 397, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 256, -1, -1, -1, 421, 422, 423, 424, 264,
			265, -1, 267, -1, -1, 270, 271, -1, -1, -1,
			275, 276, 277, 422, 279, -1, -1, 265, -1, 267,
			285, -1, 270, 288, -1, -1, -1, 275, -1, -1,
			295, 279, -1, -1, -1, 300, -1, 302, 303, 304,
			288, 306, -1, -1, -1, -1, -1, 295, 313, -1,
			-1, 316, 300, 318, 319, -1, 304, 322, -1, -1,
			325, -1, 327, -1, 329, 330, 331, 332, 316, 334,
			318, -1, -1, -1, 322, -1, 341, -1, -1, 344,
			345, -1, 330, 331, -1, -1, 334, -1, -1, 337,
			-1, -1, -1, -1, 359, 360, 361, 362, 363, -1,
			-1, 366, 367, -1, -1, -1, -1, -1, -1, -1,
			375, -1, -1, -1, -1, -1, 381, 382, 383, 384,
			-1, -1, -1, 388, -1, 390, 374, -1, -1, -1,
			-1, 396, 397, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 256, -1, -1, -1, 421, 422, 423, 424,
			264, 265, -1, 267, -1, -1, 270, 271, -1, -1,
			-1, 275, 276, 277, 422, 279, -1, -1, 265, -1,
			267, 285, -1, 270, 288, -1, -1, -1, 275, -1,
			-1, 295, 279, -1, -1, -1, 300, -1, 302, 303,
			304, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, 316, 300, 318, 319, 320, 304, 322, -1,
			-1, 325, -1, 327, -1, 329, 330, 331, 332, 316,
			334, 318, -1, -1, -1, 322, -1, 341, -1, -1,
			344, 345, -1, 330, 331, -1, -1, 334, -1, -1,
			337, -1, -1, -1, -1, 359, 360, 361, 362, 363,
			-1, -1, 366, 367, -1, -1, -1, 371, -1, -1,
			-1, 375, -1, -1, -1, -1, -1, 381, 382, 383,
			384, -1, -1, -1, 388, -1, 390, -1, -1, -1,
			-1, -1, 396, 397, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 256, -1, -1, -1, 421, 422, 423,
			424, 264, 265, -1, 267, -1, -1, 270, 271, -1,
			-1, -1, 275, 276, 277, 422, 279, -1, -1, 265,
			-1, 267, 285, -1, 270, 288, -1, -1, -1, 275,
			-1, -1, 295, 279, -1, -1, -1, 300, -1, 302,
			303, 304, 288, -1, -1, -1, -1, -1, -1, 295,
			-1, -1, -1, 316, 300, 318, 319, -1, 304, 322,
			-1, -1, 325, -1, 327, -1, 329, 330, 331, 332,
			316, 334, 318, -1, 337, -1, 322, -1, 341, -1,
			-1, 344, 345, -1, 330, 331, -1, -1, 334, -1,
			-1, 337, -1, -1, -1, -1, 359, 360, 361, 362,
			363, -1, -1, 366, 367, -1, -1, -1, -1, -1,
			-1, -1, 375, -1, -1, -1, -1, -1, 381, 382,
			383, 384, -1, -1, -1, 388, -1, 390, -1, -1,
			-1, -1, -1, 396, 397, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 256, -1, -1, -1, 421, 422,
			423, 424, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, 422, 279, -1, -1,
			-1, -1, -1, 285, -1, -1, 288, -1, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, -1, -1, -1, -1, 341,
			-1, -1, 344, 345, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, 371,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, -1,
			-1, -1, -1, -1, 396, 397, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 256, -1, 421,
			422, 423, 424, 262, -1, 264, 265, -1, 267, -1,
			-1, 270, 271, -1, -1, -1, 275, 276, 277, -1,
			279, -1, -1, -1, -1, -1, 285, -1, -1, 288,
			-1, -1, -1, -1, -1, -1, 295, -1, -1, 298,
			-1, 300, -1, 302, 303, 304, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 316, -1, 318,
			319, -1, -1, 322, -1, -1, 325, -1, 327, -1,
			329, 330, 331, 332, -1, 334, -1, -1, -1, -1,
			-1, -1, -1, -1, 343, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			359, 360, 361, 362, 363, -1, -1, 366, 367, -1,
			-1, -1, -1, -1, -1, -1, 375, -1, 377, -1,
			-1, -1, 381, 382, 383, 384, 385, -1, -1, 388,
			-1, 390, 256, -1, -1, -1, -1, 396, 397, -1,
			264, 265, -1, 267, -1, -1, 270, 271, -1, -1,
			-1, 275, 276, 277, -1, 279, -1, -1, -1, -1,
			-1, 285, 421, 422, 288, 424, -1, -1, -1, -1,
			-1, 295, -1, -1, -1, -1, 300, -1, 302, 303,
			304, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 316, -1, 318, 319, -1, -1, 322, -1,
			-1, 325, -1, 327, -1, 329, 330, 331, 332, -1,
			334, -1, -1, -1, -1, -1, -1, 341, -1, -1,
			344, 345, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 359, 360, 361, 362, 363,
			-1, -1, 366, 367, -1, -1, -1, -1, 372, -1,
			-1, 375, -1, -1, -1, -1, -1, 381, 382, 383,
			384, -1, -1, -1, 388, -1, 390, -1, -1, -1,
			-1, -1, 396, 397, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 256, -1, -1, -1, 421, 422, 423,
			424, 264, 265, -1, 267, -1, -1, 270, 271, -1,
			-1, -1, 275, 276, 277, -1, 279, -1, -1, 265,
			-1, 267, 285, -1, 270, 288, -1, -1, -1, 275,
			-1, -1, 295, 279, -1, -1, -1, 300, -1, 302,
			303, 304, 288, -1, -1, -1, -1, -1, -1, 295,
			-1, -1, -1, 316, 300, 318, 319, -1, 304, 322,
			-1, -1, 325, -1, 327, -1, 329, 330, 331, 332,
			316, 334, 318, -1, -1, -1, 322, -1, 341, -1,
			-1, 344, 345, -1, 330, 331, -1, -1, 334, -1,
			-1, 337, -1, -1, -1, -1, 359, 360, 361, 362,
			363, -1, -1, 366, 367, -1, -1, -1, -1, 372,
			-1, -1, 375, -1, -1, -1, -1, -1, 381, 382,
			383, 384, -1, -1, -1, 388, -1, 390, -1, -1,
			-1, -1, -1, 396, 397, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 256, -1, -1, -1, 421, 422,
			423, 424, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, 422, 279, -1, -1,
			265, -1, 267, 285, -1, 270, 288, -1, -1, -1,
			275, -1, -1, 295, 279, -1, -1, -1, 300, -1,
			302, 303, 304, 288, -1, -1, -1, -1, -1, -1,
			295, -1, -1, -1, 316, 300, 318, 319, -1, 304,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, 316, 334, 318, -1, -1, -1, 322, -1, 341,
			-1, -1, 344, 345, -1, 330, 331, -1, -1, 334,
			-1, -1, 337, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, -1,
			-1, -1, -1, -1, 396, 397, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 256, -1, -1, -1, 421,
			422, 423, 424, 264, 265, -1, 267, -1, -1, 270,
			271, -1, -1, -1, 275, 276, 277, 422, 279, -1,
			-1, -1, -1, -1, 285, -1, -1, 288, -1, -1,
			-1, -1, -1, -1, 295, -1, -1, -1, -1, 300,
			-1, 302, 303, 304, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 316, -1, 318, 319, -1,
			-1, 322, -1, -1, 325, -1, 327, -1, 329, 330,
			331, 332, -1, 334, -1, -1, -1, -1, -1, -1,
			341, -1, -1, 344, 345, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 359, 360,
			361, 362, 363, -1, -1, 366, 367, -1, -1, -1,
			-1, -1, -1, -1, 375, -1, -1, -1, -1, -1,
			381, 382, 383, 384, -1, -1, -1, 388, -1, 390,
			-1, -1, -1, -1, -1, 396, 397, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 256, -1,
			421, 422, 423, 424, 262, -1, 264, 265, -1, 267,
			-1, -1, 270, 271, -1, -1, -1, 275, 276, 277,
			-1, 279, -1, -1, -1, -1, -1, 285, -1, -1,
			288, -1, -1, -1, -1, -1, -1, 295, -1, -1,
			298, -1, 300, -1, 302, 303, 304, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 316, -1,
			318, 319, -1, -1, 322, -1, -1, 325, -1, 327,
			-1, 329, 330, 331, 332, -1, 334, -1, -1, -1,
			-1, -1, -1, -1, -1, 343, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 359, 360, 361, 362, 363, -1, -1, 366, 367,
			-1, -1, -1, -1, -1, -1, -1, 375, -1, 377,
			-1, -1, -1, 381, 382, 383, 384, -1, -1, -1,
			388, -1, 390, 256, -1, -1, -1, -1, 396, 397,
			-1, 264, 265, -1, 267, -1, -1, 270, 271, -1,
			-1, -1, 275, 276, 277, -1, 279, -1, -1, -1,
			-1, -1, 285, 421, 422, 288, 424, -1, -1, -1,
			-1, -1, 295, -1, -1, -1, -1, 300, -1, 302,
			303, 304, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 316, -1, 318, 319, -1, -1, 322,
			-1, -1, 325, -1, 327, -1, 329, 330, 331, 332,
			-1, 334, -1, -1, -1, -1, -1, -1, 341, -1,
			-1, 344, 345, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 359, 360, 361, 362,
			363, -1, -1, 366, 367, -1, -1, -1, -1, -1,
			-1, -1, 375, -1, -1, -1, -1, -1, 381, 382,
			383, 384, -1, -1, -1, 388, -1, 390, -1, -1,
			-1, -1, -1, 396, 397, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 256, -1, -1, -1, 421, 422,
			423, 424, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			265, -1, 267, 285, -1, 270, 288, -1, -1, -1,
			275, -1, -1, 295, 279, -1, -1, -1, 300, -1,
			302, 303, 304, 288, -1, -1, -1, -1, -1, -1,
			295, -1, -1, -1, 316, 300, 318, 319, -1, 304,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, 316, 334, 318, -1, -1, -1, 322, -1, 341,
			-1, -1, 344, 345, -1, 330, 331, -1, -1, 334,
			-1, -1, 337, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, -1,
			-1, -1, -1, -1, 396, 397, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 256, -1, -1, -1, 421,
			422, 423, 424, 264, 265, -1, 267, -1, -1, 270,
			271, -1, -1, -1, 275, 276, 277, 422, 279, -1,
			-1, 265, -1, 267, 285, -1, 270, 288, -1, -1,
			-1, 275, -1, -1, 295, 279, -1, -1, -1, 300,
			-1, 302, 303, 304, 288, -1, -1, -1, -1, -1,
			-1, 295, -1, -1, -1, 316, 300, 318, 319, -1,
			304, 322, -1, -1, 325, -1, 327, -1, 329, 330,
			331, 332, 316, 334, 318, -1, -1, -1, 322, -1,
			341, -1, -1, 344, 345, -1, 330, 331, -1, -1,
			334, -1, -1, 337, -1, -1, -1, -1, 359, 360,
			361, 362, 363, -1, -1, 366, 367, -1, -1, -1,
			-1, -1, -1, -1, 375, -1, -1, -1, -1, -1,
			381, 382, 383, 384, -1, -1, -1, 388, -1, 390,
			-1, -1, -1, -1, -1, 396, 397, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 256, -1, -1, -1,
			421, 422, 423, 424, 264, 265, -1, 267, -1, -1,
			270, 271, -1, -1, -1, 275, 276, 277, 422, 279,
			-1, -1, -1, -1, -1, 285, -1, -1, 288, -1,
			-1, -1, -1, -1, -1, 295, -1, -1, -1, -1,
			300, -1, 302, 303, 304, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 316, -1, 318, 319,
			-1, -1, 322, -1, -1, 325, -1, 327, -1, 329,
			330, 331, 332, -1, 334, -1, -1, -1, -1, -1,
			-1, 341, -1, -1, 344, 345, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 359,
			360, 361, 362, 363, -1, -1, 366, 367, -1, -1,
			-1, -1, -1, -1, -1, 375, -1, -1, -1, -1,
			-1, 381, 382, 383, 384, -1, -1, -1, 388, -1,
			390, -1, -1, -1, -1, -1, 396, 397, -1, -1,
			-1, -1, 285, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 298, -1, -1, -1, 256,
			-1, 421, 422, 423, 424, 262, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, 327, -1, -1, -1, 285, -1,
			-1, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			-1, 298, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, 381, 382,
			383, 384, -1, 386, 387, 388, 389, 390, 391, 392,
			393, -1, -1, 396, 397, 398, 399, 400, 401, 402,
			403, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			377, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, 261, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, 284, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, 297, 334, -1, -1, -1, 302, -1, -1, -1,
			-1, 307, -1, 309, 310, 311, 312, -1, -1, 315,
			-1, 317, -1, -1, -1, 321, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, 333, -1, -1,
			336, -1, 338, 375, -1, 377, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, 362, 264, 265, -1,
			267, -1, -1, 270, 271, -1, 372, 373, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			261, -1, 263, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, 284, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, 297, 334, -1, -1,
			-1, 302, -1, -1, -1, -1, 307, -1, 309, 310,
			311, 312, -1, -1, -1, -1, 317, -1, -1, -1,
			321, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, 333, -1, -1, 336, -1, 338, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, 362, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, 372, 373, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, 261, -1, 263, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, 284, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, 297, 334, -1, -1, -1, 302, -1, -1, -1,
			-1, 307, -1, 309, 310, 311, 312, -1, -1, -1,
			-1, 317, -1, -1, -1, 321, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, 333, -1, -1,
			336, -1, 338, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, 362, 264, 265, -1,
			267, -1, -1, 270, 271, -1, 372, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			261, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, 284, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, 297, 334, -1, -1,
			301, 302, -1, -1, -1, -1, 307, -1, 309, 310,
			311, 312, -1, -1, -1, -1, 317, -1, -1, -1,
			321, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, 333, -1, -1, 336, -1, 338, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, 362, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, 261, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, 284, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, 297, 334, -1, -1, -1, 302, -1, -1, -1,
			-1, 307, -1, 309, 310, 311, 312, -1, -1, -1,
			-1, 317, -1, -1, -1, 321, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, 333, -1, -1,
			336, -1, 338, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, 362, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, 381,
			382, 383, 384, -1, -1, -1, 388, -1, 390, 256,
			-1, -1, -1, -1, 396, 397, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, -1, -1, -1, -1, 285, 421,
			422, 288, 424, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, 256, -1, -1, -1, -1, 396,
			397, -1, 264, 265, -1, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, 421, 422, 288, 424, -1, -1,
			-1, -1, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, 337, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			-1, -1, -1, 375, -1, -1, -1, -1, -1, -1,
			382, 383, -1, -1, 256, -1, -1, -1, 390, 261,
			-1, -1, -1, 265, -1, 267, -1, -1, 270, -1,
			272, 273, -1, 275, -1, 277, -1, 279, -1, 281,
			282, 283, 284, -1, -1, 287, 288, -1, -1, 421,
			422, 293, 424, 295, 296, 297, -1, -1, 300, -1,
			302, -1, 304, -1, -1, 307, -1, 309, 310, 311,
			312, -1, -1, -1, 316, 317, 318, -1, -1, 321,
			322, 323, -1, -1, -1, -1, -1, -1, 330, 331,
			-1, 333, 334, -1, 336, 337, 338, -1, -1, -1,
			342, -1, -1, 256, -1, -1, -1, -1, -1, -1,
			262, 264, 265, -1, 267, -1, -1, 270, 271, -1,
			362, -1, 275, 276, 277, -1, 279, -1, -1, 371,
			372, 373, 285, -1, -1, 288, -1, -1, -1, 381,
			-1, -1, 295, -1, -1, -1, 298, 300, -1, 302,
			303, 304, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 316, -1, 318, 319, -1, -1, 322,
			-1, -1, 325, -1, 327, -1, 329, 330, 331, 332,
			422, 334, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 359, 360, 361, 362,
			363, -1, 364, 366, 367, -1, -1, -1, -1, -1,
			-1, -1, 375, 375, 376, 377, 378, 379, -1, -1,
			382, 383, -1, -1, 386, 387, 388, 389, 390, 391,
			392, 393, 394, -1, 396, 397, 398, 399, 400, 401,
			402, 403, 404, 405, 406, 407, 408, 409, 410, 411,
			412, 413, 414, 415, 416, 417, -1, -1, 421, 422,
			-1, -1, 424, -1, 261, 427, 263, -1, 265, -1,
			267, -1, -1, 270, -1, 272, 273, -1, 275, -1,
			277, -1, 279, -1, 281, 282, 283, 284, -1, -1,
			287, 288, -1, -1, -1, -1, 293, 294, 295, 296,
			297, -1, -1, 300, -1, 302, -1, 304, -1, 306,
			307, -1, 309, 310, 311, 312, -1, -1, 315, 316,
			317, 318, -1, -1, 321, 322, 323, -1, -1, -1,
			-1, -1, -1, 330, 331, -1, 333, 334, -1, 336,
			337, 338, -1, -1, -1, 342, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 362, -1, -1, -1, -1,
			-1, 368, 369, 261, -1, -1, -1, 265, -1, 267,
			-1, -1, 270, -1, 272, 273, -1, 275, -1, 277,
			-1, 279, -1, 281, 282, 283, 284, -1, -1, 287,
			288, -1, -1, -1, -1, 293, -1, 295, 296, 297,
			-1, -1, 300, -1, 302, -1, 304, -1, -1, 307,
			-1, 309, 310, 311, 312, 422, -1, -1, 316, 317,
			318, -1, -1, 321, 322, 323, -1, -1, -1, -1,
			-1, -1, 330, 331, -1, 333, 334, -1, 336, 337,
			338, -1, -1, -1, 342, 261, -1, -1, -1, 265,
			-1, 267, -1, -1, 270, -1, 272, 273, -1, 275,
			-1, 277, -1, 279, 362, 281, 282, 283, 284, -1,
			-1, 287, 288, -1, 372, -1, -1, 293, -1, 295,
			296, 297, -1, 381, 300, -1, 302, -1, 304, -1,
			-1, 307, -1, 309, 310, 311, 312, -1, -1, -1,
			316, 317, 318, -1, -1, 321, 322, 323, -1, -1,
			-1, -1, -1, -1, 330, 331, -1, 333, 334, -1,
			336, 337, 338, -1, 422, -1, 342, 261, -1, -1,
			-1, 265, -1, 267, -1, -1, 270, -1, 272, 273,
			-1, 275, -1, 277, -1, 279, 362, 281, 282, 283,
			284, -1, -1, 287, 288, -1, 372, -1, -1, 293,
			-1, 295, 296, 297, -1, 381, 300, -1, 302, -1,
			304, -1, -1, 307, -1, 309, 310, 311, 312, -1,
			-1, -1, 316, 317, 318, -1, -1, 321, 322, 323,
			-1, -1, -1, -1, -1, -1, 330, 331, -1, 333,
			334, -1, 336, 337, 338, -1, 422, -1, 342, 261,
			-1, -1, -1, 265, -1, 267, -1, -1, 270, -1,
			272, 273, -1, 275, -1, 277, -1, 279, 362, 281,
			282, 283, 284, -1, -1, 287, 288, -1, -1, -1,
			-1, 293, -1, 295, 296, 297, -1, 381, 300, -1,
			302, -1, 304, -1, -1, 307, -1, 309, 310, 311,
			312, -1, -1, -1, 316, 317, 318, -1, -1, 321,
			322, 323, -1, -1, -1, -1, -1, -1, 330, 331,
			-1, 333, 334, -1, 336, 337, 338, -1, 422, -1,
			342, 261, -1, -1, -1, 265, -1, 267, -1, -1,
			270, -1, 272, 273, -1, 275, -1, 277, -1, 279,
			362, 281, 282, 283, 284, -1, -1, 287, 288, -1,
			372, -1, -1, 293, -1, 295, 296, 297, -1, -1,
			300, -1, 302, 261, 304, -1, -1, 307, -1, 309,
			310, 311, 312, -1, -1, -1, 316, 317, 318, -1,
			-1, 321, 322, 323, -1, -1, 284, -1, -1, -1,
			330, 331, -1, 333, 334, 261, 336, 337, 338, 297,
			422, -1, 342, -1, 302, -1, -1, 305, -1, 307,
			-1, 309, 310, 311, 312, -1, -1, -1, 284, 317,
			-1, -1, 362, 321, -1, -1, -1, 325, -1, -1,
			261, 297, 372, -1, -1, 333, 302, -1, 336, -1,
			338, 307, -1, 309, 310, 311, 312, -1, -1, -1,
			-1, 317, -1, 284, -1, 321, -1, -1, -1, 325,
			-1, -1, -1, -1, 362, -1, 297, 333, -1, -1,
			336, 302, 338, -1, 372, -1, 307, -1, 309, 310,
			311, 312, 422, -1, -1, -1, 317, -1, -1, -1,
			321, -1, -1, -1, 325, -1, 362, 264, 265, -1,
			267, -1, 333, 270, 271, 336, 372, 338, 275, 276,
			277, -1, 279, -1, -1, 265, -1, 267, 285, -1,
			270, 288, -1, -1, 422, 275, -1, -1, 295, 279,
			-1, 362, -1, 300, -1, 302, 303, 304, 288, 306,
			-1, -1, -1, -1, -1, 295, 313, -1, -1, 316,
			300, 318, 319, -1, 304, 322, 422, -1, 325, -1,
			327, -1, 329, 330, 331, 332, 316, 334, 318, -1,
			-1, -1, 322, -1, 341, -1, -1, 344, 345, -1,
			330, 331, -1, -1, 334, -1, -1, 337, -1, -1,
			-1, 422, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, 376,
			-1, 378, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, -1, -1, -1, -1, -1, 396,
			397, -1, -1, -1, -1, -1, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, 421, 422, 423, 424, 285, -1,
			-1, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, 422, 300, -1, 302, 303, 304, -1, 306,
			-1, -1, -1, -1, -1, -1, 313, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, 341, -1, -1, 344, 345, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, 378, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, -1, -1, -1, -1, -1, 396,
			397, -1, -1, -1, -1, -1, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, 421, 422, 423, 424, 285, -1,
			-1, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			337, -1, -1, -1, 341, -1, -1, 344, 345, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, -1, -1, -1, -1, -1, 396,
			397, -1, -1, -1, -1, -1, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, 421, 422, 423, 424, 285, -1,
			-1, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			337, -1, -1, -1, 341, -1, -1, 344, 345, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, -1, -1, -1, -1, -1, 396,
			397, -1, -1, -1, -1, -1, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, 421, 422, 423, 424, 285, -1,
			-1, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, 341, -1, -1, 344, 345, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, 371, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, -1, -1, -1, -1, -1, 396,
			397, -1, -1, -1, -1, -1, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, 421, 422, 423, 424, 285, -1,
			-1, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, 341, -1, -1, 344, 345, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, 371, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, -1, -1, -1, -1, -1, 396,
			397, -1, -1, -1, -1, -1, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, 421, 422, 423, 424, 285, -1,
			-1, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, 341, -1, -1, 344, 345, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, -1, -1, -1, -1, -1, 396,
			397, -1, -1, -1, -1, -1, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, 421, 422, 423, 424, 285, -1,
			-1, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, 341, -1, -1, 344, 345, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, -1, -1, -1, -1, -1, 396,
			397, -1, -1, -1, -1, -1, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, 421, 422, 423, 424, 285, -1,
			-1, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, 341, -1, -1, 344, 345, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, -1, -1, -1, -1, -1, 396,
			397, -1, -1, -1, -1, -1, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, 421, 422, 423, 424, 285, -1,
			-1, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, 341, -1, -1, 344, 345, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, -1, -1, -1, -1, -1, 396,
			397, -1, -1, -1, -1, -1, -1, 264, 265, -1,
			267, -1, -1, 270, 271, -1, -1, -1, 275, 276,
			277, -1, 279, -1, 421, 422, 423, 424, 285, -1,
			-1, 288, -1, -1, -1, -1, -1, -1, 295, -1,
			-1, -1, -1, 300, -1, 302, 303, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 316,
			-1, 318, 319, -1, -1, 322, -1, -1, 325, -1,
			327, -1, 329, 330, 331, 332, -1, 334, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 359, 360, 361, 362, 363, -1, -1, 366,
			367, -1, -1, -1, -1, -1, -1, -1, 375, -1,
			-1, -1, -1, -1, 381, 382, 383, 384, -1, -1,
			-1, 388, -1, 390, -1, 264, 265, -1, 267, 396,
			397, 270, 271, -1, -1, -1, 275, 276, 277, -1,
			279, -1, -1, -1, -1, -1, 285, -1, -1, 288,
			-1, -1, -1, -1, 421, 422, 295, 424, -1, -1,
			-1, 300, -1, 302, 303, 304, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 316, -1, 318,
			319, -1, -1, 322, -1, -1, 325, -1, 327, -1,
			329, 330, 331, 332, -1, 334, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			359, 360, 361, 362, 363, -1, -1, 366, 367, -1,
			-1, -1, -1, -1, -1, -1, 375, -1, -1, -1,
			-1, -1, 381, 382, 383, 384, -1, -1, -1, 388,
			-1, 390, -1, 264, 265, -1, 267, 396, 397, 270,
			271, -1, -1, -1, 275, 276, 277, -1, 279, -1,
			-1, -1, -1, -1, 285, -1, -1, 288, -1, -1,
			-1, -1, 421, 422, 295, 424, -1, -1, -1, 300,
			-1, 302, 303, 304, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 316, -1, 318, 319, -1,
			-1, 322, -1, -1, 325, -1, 327, -1, 329, 330,
			331, 332, -1, 334, -1, -1, 337, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 359, 360,
			361, 362, 363, -1, -1, 366, 367, -1, -1, -1,
			-1, -1, 264, 265, 375, 267, -1, -1, 270, 271,
			-1, 382, 383, 275, 276, 277, -1, 279, -1, 390,
			-1, -1, -1, 285, -1, -1, 288, -1, -1, -1,
			-1, -1, -1, 295, -1, 261, -1, -1, 300, -1,
			302, 303, 304, -1, -1, -1, -1, -1, -1, -1,
			421, 422, -1, 424, 316, -1, 318, 319, 284, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, 297, 334, -1, -1, 337, 302, -1, -1, -1,
			-1, 307, -1, 309, 310, 311, 312, -1, -1, -1,
			-1, 317, -1, -1, -1, 321, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, 333, -1, -1,
			336, -1, 338, 375, -1, 263, -1, 265, -1, 267,
			382, 383, 270, -1, 272, 273, -1, 275, 390, 277,
			-1, 279, -1, 281, 282, 283, 362, -1, -1, 287,
			288, -1, 368, 369, -1, 293, 372, 295, 296, -1,
			-1, -1, 300, -1, -1, -1, 304, -1, -1, 421,
			422, -1, 424, -1, -1, -1, -1, 315, 316, -1,
			318, -1, -1, -1, 322, 323, -1, -1, -1, -1,
			-1, -1, 330, 331, -1, -1, 334, -1, -1, 337,
			-1, -1, 264, 265, 342, 267, -1, -1, 270, 271,
			-1, -1, -1, 275, 276, 277, -1, 279, -1, -1,
			-1, -1, -1, 285, -1, -1, 288, -1, -1, -1,
			368, 369, -1, 295, -1, -1, -1, -1, 300, -1,
			302, 303, 304, 381, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, 316, -1, 318, 319, -1, -1,
			322, -1, -1, 325, -1, 327, -1, 329, 330, 331,
			332, -1, 334, -1, -1, 337, -1, -1, -1, -1,
			-1, -1, -1, -1, 422, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 359, 360, 361,
			362, 363, -1, -1, 366, 367, -1, -1, -1, -1,
			265, -1, 267, 375, -1, 270, -1, 272, 273, -1,
			275, -1, 277, -1, 279, -1, 281, 282, 283, -1,
			-1, -1, 287, 288, -1, -1, -1, -1, 293, -1,
			295, 296, -1, -1, -1, 300, -1, -1, -1, 304,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 421,
			422, 316, -1, 318, -1, -1, -1, 322, 323, -1,
			-1, -1, -1, -1, -1, 330, 331, -1, 265, 334,
			267, -1, 337, 270, -1, 272, 273, 342, 275, -1,
			277, -1, 279, -1, 281, 282, 283, -1, -1, -1,
			287, 288, -1, -1, -1, -1, 293, -1, 295, 296,
			-1, -1, -1, 300, -1, -1, -1, 304, -1, -1,
			-1, -1, -1, -1, -1, -1, 381, -1, -1, 316,
			-1, 318, -1, -1, -1, 322, 323, -1, -1, -1,
			-1, -1, -1, 330, 331, -1, 265, 334, 267, -1,
			337, 270, -1, -1, 273, 342, 275, -1, 277, -1,
			279, -1, 281, 282, 283, -1, -1, 422, 287, 288,
			-1, -1, -1, -1, 293, -1, 295, -1, -1, -1,
			-1, 300, -1, -1, -1, 304, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 316, -1, 318,
			-1, -1, -1, 322, -1, -1, -1, -1, -1, -1,
			-1, 330, 331, -1, 265, 334, 267, -1, 337, 270,
			-1, -1, 273, 342, 275, -1, 277, -1, 279, -1,
			281, 282, 283, -1, -1, 422, 287, 288, -1, -1,
			-1, -1, 293, -1, 295, -1, -1, -1, -1, 300,
			-1, -1, -1, 304, 265, -1, 267, -1, -1, 270,
			-1, -1, 381, -1, 275, 316, -1, 318, 279, -1,
			-1, 322, -1, -1, -1, -1, -1, 288, -1, 330,
			331, -1, -1, 334, 295, -1, 337, -1, -1, 300,
			-1, 342, -1, 304, 265, 306, 267, 308, -1, 270,
			-1, -1, 313, 422, 275, 316, -1, 318, 279, -1,
			-1, 322, -1, -1, 325, -1, -1, 288, -1, 330,
			331, -1, -1, 334, 295, -1, 337, -1, -1, 300,
			-1, -1, -1, 304, -1, 306, -1, 308, -1, -1,
			-1, -1, 313, -1, -1, 316, -1, 318, -1, -1,
			-1, 322, -1, -1, 325, -1, -1, -1, -1, 330,
			331, -1, -1, 334, -1, 376, 337, 265, -1, 267,
			-1, 422, 270, -1, -1, -1, -1, 275, -1, -1,
			-1, 279, -1, -1, -1, -1, -1, -1, -1, -1,
			288, -1, -1, -1, -1, -1, -1, 295, -1, -1,
			-1, -1, 300, 374, -1, -1, 304, 265, 306, 267,
			-1, 422, 270, -1, -1, 313, -1, 275, 316, -1,
			318, 279, -1, -1, 322, -1, -1, 325, -1, -1,
			288, -1, 330, 331, -1, -1, 334, 295, -1, 337,
			-1, -1, 300, -1, -1, -1, 304, 265, 306, 267,
			308, 422, 270, -1, -1, 313, -1, 275, 316, -1,
			318, 279, -1, -1, 322, 363, -1, 325, -1, -1,
			288, -1, 330, 331, -1, -1, 334, 295, -1, 337,
			-1, -1, 300, -1, -1, -1, 304, 265, 306, 267,
			308, -1, 270, -1, -1, 313, -1, 275, 316, -1,
			318, 279, -1, -1, 322, 283, -1, 325, -1, -1,
			288, -1, 330, 331, -1, 293, 334, 295, -1, 337,
			-1, -1, 300, -1, 422, -1, 304, 305, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 316, -1,
			318, -1, -1, -1, 322, -1, -1, -1, -1, -1,
			-1, -1, 330, 331, -1, 265, 334, 267, -1, 337,
			270, -1, 272, -1, 422, 275, -1, -1, -1, 279,
			-1, -1, -1, -1, -1, -1, -1, -1, 288, -1,
			-1, -1, -1, -1, -1, 295, -1, -1, -1, -1,
			300, -1, 302, -1, 304, -1, -1, -1, -1, 261,
			-1, -1, -1, -1, 422, -1, 316, -1, 318, -1,
			272, -1, 322, 323, -1, 277, -1, -1, -1, 281,
			330, 331, 284, -1, 334, -1, -1, 337, -1, -1,
			-1, -1, -1, -1, 296, 297, -1, -1, -1, 301,
			302, -1, 261, -1, 422, 307, -1, 309, 310, 311,
			312, -1, -1, 272, -1, 317, -1, -1, 277, 321,
			-1, 323, 281, -1, -1, 284, -1, -1, -1, -1,
			-1, 333, -1, 335, 336, -1, 338, 296, 297, -1,
			342, -1, 301, 302, -1, 261, -1, -1, 307, -1,
			309, 310, 311, 312, -1, -1, 272, -1, 317, -1,
			362, 277, 321, -1, 323, 281, -1, -1, 284, -1,
			372, 373, 422, -1, 333, -1, -1, 336, -1, 338,
			296, 297, -1, 342, -1, 301, 302, -1, 261, -1,
			-1, 307, -1, 309, 310, 311, 312, -1, -1, 272,
			-1, 317, -1, 362, 277, 321, -1, 323, 281, -1,
			-1, 284, -1, 372, 373, -1, -1, 333, -1, -1,
			336, -1, 338, 296, 297, -1, 342, -1, 301, 302,
			261, -1, -1, -1, 307, -1, 309, 310, 311, 312,
			-1, -1, -1, -1, 317, -1, 362, -1, 321, -1,
			323, -1, -1, 284, -1, -1, 372, -1, -1, -1,
			333, -1, -1, 336, -1, 338, 297, -1, 261, 342,
			-1, 302, -1, -1, -1, -1, 307, -1, 309, 310,
			311, 312, -1, -1, -1, -1, 317, -1, -1, 362,
			321, 284, -1, -1, -1, -1, -1, -1, -1, 372,
			-1, -1, 333, -1, 297, 336, -1, 338, -1, 302,
			-1, -1, -1, -1, 307, -1, 309, 310, 311, 312,
			-1, -1, -1, -1, 317, -1, -1, -1, 321, -1,
			-1, 362, -1, -1, -1, -1, -1, 368, 369, -1,
			333, 372, -1, 336, -1, 338, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, 362,
			-1, -1, -1, -1, -1, 368, 369
		};

		private Tokenizer lexer;

		public Tokenizer Lexer => lexer;

		public void yyerror(string message)
		{
			yyerror(message, null);
		}

		public void yyerror(string message, string[] expected)
		{
			if (yacc_verbose_flag > 0 && expected != null && expected.Length != 0)
			{
				ErrorOutput.Write(message + ", expecting");
				for (int i = 0; i < expected.Length; i++)
				{
					ErrorOutput.Write(" " + expected[i]);
				}
				ErrorOutput.WriteLine();
			}
			else
			{
				ErrorOutput.WriteLine(message);
			}
		}

		public static string yyname(int token)
		{
			if (token < 0 || token > yyNames.Length)
			{
				return "[illegal]";
			}
			string result;
			if ((result = yyNames[token]) != null)
			{
				return result;
			}
			return "[unknown]";
		}

		protected int[] yyExpectingTokens(int state)
		{
			int num = 0;
			bool[] array = new bool[yyNames.Length];
			int num2;
			int i;
			if ((num2 = yySindex[state]) != 0)
			{
				for (i = ((num2 < 0) ? (-num2) : 0); i < yyNames.Length && num2 + i < yyTable.Length; i++)
				{
					if (yyCheck[num2 + i] == i && !array[i] && yyNames[i] != null)
					{
						num++;
						array[i] = true;
					}
				}
			}
			if ((num2 = yyRindex[state]) != 0)
			{
				for (i = ((num2 < 0) ? (-num2) : 0); i < yyNames.Length && num2 + i < yyTable.Length; i++)
				{
					if (yyCheck[num2 + i] == i && !array[i] && yyNames[i] != null)
					{
						num++;
						array[i] = true;
					}
				}
			}
			int[] array2 = new int[num];
			num2 = (i = 0);
			while (num2 < num)
			{
				if (array[i])
				{
					array2[num2++] = i;
				}
				i++;
			}
			return array2;
		}

		protected string[] yyExpecting(int state)
		{
			int[] array = yyExpectingTokens(state);
			string[] array2 = new string[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i++] = yyNames[array[i]];
			}
			return array2;
		}

		public object yyparse(yyInput yyLex, object yyd)
		{
			debug = (yyDebug)yyd;
			return yyparse(yyLex);
		}

		protected object yyDefault(object first)
		{
			return first;
		}

		public object yyparse(yyInput yyLex)
		{
			if (yyMax <= 0)
			{
				yyMax = 256;
			}
			int num = 0;
			yyVal = null;
			yyToken = -1;
			int num2 = 0;
			int[] array;
			if (use_global_stacks && global_yyStates != null)
			{
				yyVals = global_yyVals;
				array = global_yyStates;
			}
			else
			{
				yyVals = new object[yyMax];
				array = new int[yyMax];
				if (use_global_stacks)
				{
					global_yyVals = yyVals;
					global_yyStates = array;
				}
			}
			yyTop = 0;
			while (true)
			{
				if (yyTop >= array.Length)
				{
					Array.Resize(ref array, array.Length + yyMax);
					Array.Resize(ref yyVals, yyVals.Length + yyMax);
				}
				array[yyTop] = num;
				yyVals[yyTop] = yyVal;
				if (debug != null)
				{
					debug.push(num, yyVal);
				}
				while (true)
				{
					int num3;
					if ((num3 = yyDefRed[num]) == 0)
					{
						if (yyToken < 0)
						{
							yyToken = (yyLex.advance() ? yyLex.token() : 0);
							if (debug != null)
							{
								debug.lex(num, yyToken, yyname(yyToken), yyLex.value());
							}
						}
						if ((num3 = yySindex[num]) != 0 && (num3 += yyToken) >= 0 && num3 < yyTable.Length && yyCheck[num3] == yyToken)
						{
							if (debug != null)
							{
								debug.shift(num, yyTable[num3], num2 - 1);
							}
							num = yyTable[num3];
							yyVal = yyLex.value();
							yyToken = -1;
							if (num2 > 0)
							{
								num2--;
							}
							break;
						}
						if ((num3 = yyRindex[num]) == 0 || (num3 += yyToken) < 0 || num3 >= yyTable.Length || yyCheck[num3] != yyToken)
						{
							switch (num2)
							{
							case 0:
								yyExpectingState = num;
								if (debug != null)
								{
									debug.error("syntax error");
								}
								if (yyToken == 0 || yyToken == eof_token)
								{
									throw new yyUnexpectedEof();
								}
								break;
							case 1:
							case 2:
								break;
							case 3:
								goto IL_0320;
							default:
								goto IL_037d;
							}
							num2 = 3;
							while ((num3 = yySindex[array[yyTop]]) == 0 || (num3 += 256) < 0 || num3 >= yyTable.Length || yyCheck[num3] != 256)
							{
								if (debug != null)
								{
									debug.pop(array[yyTop]);
								}
								if (--yyTop < 0)
								{
									if (debug != null)
									{
										debug.reject();
									}
									throw new yyException("irrecoverable syntax error");
								}
							}
							if (debug != null)
							{
								debug.shift(array[yyTop], yyTable[num3], 3);
							}
							num = yyTable[num3];
							yyVal = yyLex.value();
							break;
						}
						num3 = yyTable[num3];
					}
					goto IL_037d;
					IL_0320:
					if (yyToken == 0)
					{
						if (debug != null)
						{
							debug.reject();
						}
						throw new yyException("irrecoverable syntax error at end-of-file");
					}
					if (debug != null)
					{
						debug.discard(num, yyToken, yyname(yyToken), yyLex.value());
					}
					yyToken = -1;
					continue;
					IL_037d:
					int num4 = yyTop + 1 - yyLen[num3];
					if (debug != null)
					{
						debug.reduce(num, array[num4 - 1], num3, YYRules.getRule(num3), yyLen[num3]);
					}
					yyVal = ((num4 > yyTop) ? null : yyVals[num4]);
					switch (num3)
					{
					case 1:
						Lexer.check_incorrect_doc_comment();
						break;
					case 2:
						Lexer.CompleteOnEOF = false;
						break;
					case 6:
						case_6();
						break;
					case 7:
						module.AddAttributes((Attributes)yyVals[0 + yyTop], current_namespace);
						break;
					case 8:
						case_8();
						break;
					case 13:
						case_13();
						break;
					case 14:
						Error_SyntaxError(yyToken);
						break;
					case 17:
						case_17();
						break;
					case 18:
						case_18();
						break;
					case 19:
						case_19();
						break;
					case 20:
						case_20();
						break;
					case 23:
						case_23();
						break;
					case 24:
						case_24();
						break;
					case 25:
						case_25();
						break;
					case 26:
						case_26();
						break;
					case 29:
						case_29();
						break;
					case 30:
						case_30();
						break;
					case 31:
						case_31();
						break;
					case 32:
						case_32();
						break;
					case 45:
						case_45();
						break;
					case 46:
						current_namespace.DeclarationFound = true;
						break;
					case 47:
						case_47();
						break;
					case 55:
						case_55();
						break;
					case 56:
						case_56();
						break;
					case 57:
						case_57();
						break;
					case 58:
						case_58();
						break;
					case 59:
						case_59();
						break;
					case 60:
						case_60();
						break;
					case 61:
						case_61();
						break;
					case 62:
						case_62();
						break;
					case 63:
						case_63();
						break;
					case 64:
						case_64();
						break;
					case 65:
						yyVal = "event";
						break;
					case 66:
						yyVal = "return";
						break;
					case 67:
						yyVal = new List<Attribute>(4) { (Attribute)yyVals[0 + yyTop] };
						break;
					case 68:
						case_68();
						break;
					case 69:
						lexer.parsing_block++;
						break;
					case 70:
						case_70();
						break;
					case 72:
						yyVal = null;
						break;
					case 73:
						yyVal = yyVals[-1 + yyTop];
						break;
					case 74:
						yyVal = null;
						break;
					case 75:
						case_75();
						break;
					case 76:
						case_76();
						break;
					case 77:
						case_77();
						break;
					case 78:
						case_78();
						break;
					case 79:
						yyVal = new Argument((Expression)yyVals[0 + yyTop]);
						break;
					case 81:
						case_81();
						break;
					case 82:
						lexer.parsing_block++;
						break;
					case 83:
						case_83();
						break;
					case 84:
						case_84();
						break;
					case 86:
						yyVal = null;
						break;
					case 87:
						yyVal = Argument.AType.Ref;
						break;
					case 88:
						yyVal = Argument.AType.Out;
						break;
					case 91:
						case_91();
						break;
					case 92:
						case_92();
						break;
					case 106:
						case_106();
						break;
					case 107:
						case_107();
						break;
					case 108:
						case_108();
						break;
					case 110:
						case_110();
						break;
					case 111:
						case_111();
						break;
					case 112:
						case_112();
						break;
					case 113:
						case_113();
						break;
					case 114:
						case_114();
						break;
					case 115:
						Error_SyntaxError(yyToken);
						break;
					case 116:
						case_116();
						break;
					case 117:
						case_117();
						break;
					case 118:
						case_118();
						break;
					case 121:
						current_field.AddDeclarator((FieldDeclarator)yyVals[0 + yyTop]);
						break;
					case 122:
						current_field.AddDeclarator((FieldDeclarator)yyVals[0 + yyTop]);
						break;
					case 123:
						case_123();
						break;
					case 124:
						lexer.parsing_block++;
						break;
					case 125:
						case_125();
						break;
					case 126:
						case_126();
						break;
					case 129:
						case_129();
						break;
					case 130:
						case_130();
						break;
					case 131:
						case_131();
						break;
					case 132:
						case_132();
						break;
					case 133:
						report.Error(1641, GetLocation(yyVals[-1 + yyTop]), "A fixed size buffer field must have the array size specifier after the field name");
						break;
					case 135:
						case_135();
						break;
					case 136:
						case_136();
						break;
					case 139:
						current_field.AddDeclarator((FieldDeclarator)yyVals[0 + yyTop]);
						break;
					case 140:
						current_field.AddDeclarator((FieldDeclarator)yyVals[0 + yyTop]);
						break;
					case 141:
						case_141();
						break;
					case 142:
						lexer.parsing_block++;
						break;
					case 143:
						case_143();
						break;
					case 146:
						current_field.AddDeclarator((FieldDeclarator)yyVals[0 + yyTop]);
						break;
					case 147:
						current_field.AddDeclarator((FieldDeclarator)yyVals[0 + yyTop]);
						break;
					case 148:
						case_148();
						break;
					case 149:
						lexer.parsing_block++;
						break;
					case 150:
						case_150();
						break;
					case 151:
						case_151();
						break;
					case 154:
						case_154();
						break;
					case 155:
						case_155();
						break;
					case 156:
						case_156();
						break;
					case 157:
						valid_param_mod = ParameterModifierType.All;
						break;
					case 158:
						case_158();
						break;
					case 159:
						case_159();
						break;
					case 160:
						lexer.parsing_generic_declaration = true;
						break;
					case 161:
						case_161();
						break;
					case 162:
						lexer.ConstraintsParsing = true;
						break;
					case 163:
						case_163();
						break;
					case 164:
						case_164();
						break;
					case 165:
						case_165();
						break;
					case 169:
						yyVal = null;
						break;
					case 170:
						case_170();
						break;
					case 171:
						case_171();
						break;
					case 172:
						yyVal = ParametersCompiled.EmptyReadOnlyParameters;
						break;
					case 174:
						case_174();
						break;
					case 175:
						case_175();
						break;
					case 176:
						case_176();
						break;
					case 177:
						case_177();
						break;
					case 178:
						case_178();
						break;
					case 179:
						case_179();
						break;
					case 180:
						case_180();
						break;
					case 181:
						yyVal = new ParametersCompiled((Parameter)yyVals[0 + yyTop]);
						break;
					case 182:
						yyVal = new ParametersCompiled(new Parameter[1]
						{
							new ArglistParameter(GetLocation(yyVals[0 + yyTop]))
						}, true);
						break;
					case 183:
						case_183();
						break;
					case 184:
						case_184();
						break;
					case 185:
						case_185();
						break;
					case 186:
						case_186();
						break;
					case 187:
						case_187();
						break;
					case 188:
						case_188();
						break;
					case 189:
						case_189();
						break;
					case 190:
						lexer.parsing_block++;
						break;
					case 191:
						case_191();
						break;
					case 192:
						yyVal = Parameter.Modifier.NONE;
						break;
					case 194:
						yyVal = yyVals[0 + yyTop];
						break;
					case 195:
						case_195();
						break;
					case 196:
						case_196();
						break;
					case 197:
						case_197();
						break;
					case 198:
						case_198();
						break;
					case 199:
						case_199();
						break;
					case 200:
						case_200();
						break;
					case 201:
						case_201();
						break;
					case 202:
						case_202();
						break;
					case 203:
						case_203();
						break;
					case 204:
						Error_DuplicateParameterModifier(GetLocation(yyVals[-1 + yyTop]), Parameter.Modifier.PARAMS);
						break;
					case 205:
						case_205();
						break;
					case 206:
						case_206();
						break;
					case 207:
						case_207();
						break;
					case 208:
						case_208();
						break;
					case 209:
						case_209();
						break;
					case 210:
						current_property = null;
						break;
					case 211:
						case_211();
						break;
					case 212:
						case_212();
						break;
					case 214:
						case_214();
						break;
					case 215:
						case_215();
						break;
					case 218:
						valid_param_mod = ParameterModifierType.Params | ParameterModifierType.DefaultValue;
						break;
					case 219:
						case_219();
						break;
					case 220:
						case_220();
						break;
					case 222:
						case_222();
						break;
					case 227:
						case_227();
						break;
					case 228:
						case_228();
						break;
					case 229:
						case_229();
						break;
					case 230:
						case_230();
						break;
					case 231:
						case_231();
						break;
					case 233:
						case_233();
						break;
					case 234:
						case_234();
						break;
					case 236:
						case_236();
						break;
					case 237:
						case_237();
						break;
					case 238:
						case_238();
						break;
					case 239:
						case_239();
						break;
					case 240:
						Error_SyntaxError(yyToken);
						break;
					case 243:
						case_243();
						break;
					case 244:
						case_244();
						break;
					case 245:
						report.Error(525, GetLocation(yyVals[0 + yyTop]), "Interfaces cannot contain fields or constants");
						break;
					case 246:
						report.Error(525, GetLocation(yyVals[0 + yyTop]), "Interfaces cannot contain fields or constants");
						break;
					case 251:
						report.Error(567, GetLocation(yyVals[0 + yyTop]), "Interfaces cannot contain operators");
						break;
					case 252:
						report.Error(526, GetLocation(yyVals[0 + yyTop]), "Interfaces cannot contain contructors");
						break;
					case 253:
						report.Error(524, GetLocation(yyVals[0 + yyTop]), "Interfaces cannot declare classes, structs, interfaces, delegates, or enumerations");
						break;
					case 255:
						case_255();
						break;
					case 257:
						case_257();
						break;
					case 258:
						case_258();
						break;
					case 259:
						case_259();
						break;
					case 261:
						yyVal = Operator.OpType.LogicalNot;
						break;
					case 262:
						yyVal = Operator.OpType.OnesComplement;
						break;
					case 263:
						yyVal = Operator.OpType.Increment;
						break;
					case 264:
						yyVal = Operator.OpType.Decrement;
						break;
					case 265:
						yyVal = Operator.OpType.True;
						break;
					case 266:
						yyVal = Operator.OpType.False;
						break;
					case 267:
						yyVal = Operator.OpType.Addition;
						break;
					case 268:
						yyVal = Operator.OpType.Subtraction;
						break;
					case 269:
						yyVal = Operator.OpType.Multiply;
						break;
					case 270:
						yyVal = Operator.OpType.Division;
						break;
					case 271:
						yyVal = Operator.OpType.Modulus;
						break;
					case 272:
						yyVal = Operator.OpType.BitwiseAnd;
						break;
					case 273:
						yyVal = Operator.OpType.BitwiseOr;
						break;
					case 274:
						yyVal = Operator.OpType.ExclusiveOr;
						break;
					case 275:
						yyVal = Operator.OpType.LeftShift;
						break;
					case 276:
						yyVal = Operator.OpType.RightShift;
						break;
					case 277:
						yyVal = Operator.OpType.Equality;
						break;
					case 278:
						yyVal = Operator.OpType.Inequality;
						break;
					case 279:
						yyVal = Operator.OpType.GreaterThan;
						break;
					case 280:
						yyVal = Operator.OpType.LessThan;
						break;
					case 281:
						yyVal = Operator.OpType.GreaterThanOrEqual;
						break;
					case 282:
						yyVal = Operator.OpType.LessThanOrEqual;
						break;
					case 283:
						case_283();
						break;
					case 284:
						valid_param_mod = ParameterModifierType.DefaultValue;
						break;
					case 285:
						case_285();
						break;
					case 286:
						valid_param_mod = ParameterModifierType.DefaultValue;
						break;
					case 287:
						case_287();
						break;
					case 288:
						case_288();
						break;
					case 289:
						case_289();
						break;
					case 290:
						case_290();
						break;
					case 291:
						case_291();
						break;
					case 292:
						case_292();
						break;
					case 293:
						case_293();
						break;
					case 295:
						current_block = null;
						yyVal = null;
						break;
					case 298:
						lexer.parsing_block++;
						break;
					case 299:
						case_299();
						break;
					case 300:
						lexer.parsing_block++;
						break;
					case 301:
						case_301();
						break;
					case 302:
						case_302();
						break;
					case 303:
						case_303();
						break;
					case 304:
						case_304();
						break;
					case 305:
						case_305();
						break;
					case 306:
						case_306();
						break;
					case 307:
						case_307();
						break;
					case 308:
						case_308();
						break;
					case 309:
						case_309();
						break;
					case 310:
						case_310();
						break;
					case 311:
						case_311();
						break;
					case 313:
						lexer.parsing_block++;
						break;
					case 314:
						case_314();
						break;
					case 317:
						current_event_field.AddDeclarator((FieldDeclarator)yyVals[0 + yyTop]);
						break;
					case 318:
						current_event_field.AddDeclarator((FieldDeclarator)yyVals[0 + yyTop]);
						break;
					case 319:
						case_319();
						break;
					case 320:
						lexer.parsing_block++;
						break;
					case 321:
						case_321();
						break;
					case 322:
						case_322();
						break;
					case 323:
						yyVal = yyVals[0 + yyTop];
						break;
					case 326:
						case_326();
						break;
					case 327:
						case_327();
						break;
					case 328:
						case_328();
						break;
					case 329:
						case_329();
						break;
					case 330:
						case_330();
						break;
					case 331:
						case_331();
						break;
					case 332:
						case_332();
						break;
					case 333:
						case_333();
						break;
					case 335:
						case_335();
						break;
					case 336:
						case_336();
						break;
					case 337:
						case_337();
						break;
					case 338:
						case_338();
						break;
					case 339:
						case_339();
						break;
					case 340:
						case_340();
						break;
					case 342:
						yyVal = yyVals[0 + yyTop];
						break;
					case 343:
						case_343();
						break;
					case 348:
						case_348();
						break;
					case 349:
						case_349();
						break;
					case 350:
						case_350();
						break;
					case 351:
						case_351();
						break;
					case 352:
						case_352();
						break;
					case 354:
						valid_param_mod = ParameterModifierType.PrimaryConstructor;
						break;
					case 355:
						case_355();
						break;
					case 356:
						lexer.ConstraintsParsing = false;
						break;
					case 357:
						case_357();
						break;
					case 359:
						case_359();
						break;
					case 361:
						case_361();
						break;
					case 362:
						case_362();
						break;
					case 364:
						case_364();
						break;
					case 365:
						case_365();
						break;
					case 366:
						case_366();
						break;
					case 367:
						case_367();
						break;
					case 369:
						case_369();
						break;
					case 370:
						case_370();
						break;
					case 371:
						case_371();
						break;
					case 372:
						case_372();
						break;
					case 373:
						lexer.parsing_generic_declaration = true;
						break;
					case 374:
						case_374();
						break;
					case 375:
						case_375();
						break;
					case 377:
						case_377();
						break;
					case 378:
						case_378();
						break;
					case 379:
						case_379();
						break;
					case 380:
						case_380();
						break;
					case 381:
						case_381();
						break;
					case 382:
						case_382();
						break;
					case 384:
						case_384();
						break;
					case 385:
						case_385();
						break;
					case 386:
						case_386();
						break;
					case 387:
						case_387();
						break;
					case 388:
						case_388();
						break;
					case 390:
						yyVal = new TypeExpression(compiler.BuiltinTypes.Void, GetLocation(yyVals[0 + yyTop]));
						break;
					case 391:
						lexer.parsing_generic_declaration = true;
						break;
					case 397:
						case_397();
						break;
					case 399:
						yyVal = new ComposedCast((FullNamedExpression)yyVals[-1 + yyTop], (ComposedTypeSpecifier)yyVals[0 + yyTop]);
						break;
					case 400:
						case_400();
						break;
					case 401:
						yyVal = new ComposedCast((ATypeNameExpression)yyVals[-1 + yyTop], (ComposedTypeSpecifier)yyVals[0 + yyTop]);
						break;
					case 403:
						case_403();
						break;
					case 404:
						case_404();
						break;
					case 405:
						yyVal = new ComposedCast((FullNamedExpression)yyVals[-1 + yyTop], (ComposedTypeSpecifier)yyVals[0 + yyTop]);
						break;
					case 406:
						yyVal = new ComposedCast(new TypeExpression(compiler.BuiltinTypes.Void, GetLocation(yyVals[-1 + yyTop])), (ComposedTypeSpecifier)yyVals[0 + yyTop]);
						break;
					case 407:
						case_407();
						break;
					case 408:
						case_408();
						break;
					case 409:
						case_409();
						break;
					case 410:
						yyVal = new TypeExpression(compiler.BuiltinTypes.Object, GetLocation(yyVals[0 + yyTop]));
						break;
					case 411:
						yyVal = new TypeExpression(compiler.BuiltinTypes.String, GetLocation(yyVals[0 + yyTop]));
						break;
					case 412:
						yyVal = new TypeExpression(compiler.BuiltinTypes.Bool, GetLocation(yyVals[0 + yyTop]));
						break;
					case 413:
						yyVal = new TypeExpression(compiler.BuiltinTypes.Decimal, GetLocation(yyVals[0 + yyTop]));
						break;
					case 414:
						yyVal = new TypeExpression(compiler.BuiltinTypes.Float, GetLocation(yyVals[0 + yyTop]));
						break;
					case 415:
						yyVal = new TypeExpression(compiler.BuiltinTypes.Double, GetLocation(yyVals[0 + yyTop]));
						break;
					case 417:
						yyVal = new TypeExpression(compiler.BuiltinTypes.SByte, GetLocation(yyVals[0 + yyTop]));
						break;
					case 418:
						yyVal = new TypeExpression(compiler.BuiltinTypes.Byte, GetLocation(yyVals[0 + yyTop]));
						break;
					case 419:
						yyVal = new TypeExpression(compiler.BuiltinTypes.Short, GetLocation(yyVals[0 + yyTop]));
						break;
					case 420:
						yyVal = new TypeExpression(compiler.BuiltinTypes.UShort, GetLocation(yyVals[0 + yyTop]));
						break;
					case 421:
						yyVal = new TypeExpression(compiler.BuiltinTypes.Int, GetLocation(yyVals[0 + yyTop]));
						break;
					case 422:
						yyVal = new TypeExpression(compiler.BuiltinTypes.UInt, GetLocation(yyVals[0 + yyTop]));
						break;
					case 423:
						yyVal = new TypeExpression(compiler.BuiltinTypes.Long, GetLocation(yyVals[0 + yyTop]));
						break;
					case 424:
						yyVal = new TypeExpression(compiler.BuiltinTypes.ULong, GetLocation(yyVals[0 + yyTop]));
						break;
					case 425:
						yyVal = new TypeExpression(compiler.BuiltinTypes.Char, GetLocation(yyVals[0 + yyTop]));
						break;
					case 448:
						case_448();
						break;
					case 452:
						yyVal = new NullLiteral(GetLocation(yyVals[0 + yyTop]));
						break;
					case 453:
						yyVal = new BoolLiteral(compiler.BuiltinTypes, true, GetLocation(yyVals[0 + yyTop]));
						break;
					case 454:
						yyVal = new BoolLiteral(compiler.BuiltinTypes, false, GetLocation(yyVals[0 + yyTop]));
						break;
					case 455:
						yyVal = new InterpolatedString((StringLiteral)yyVals[-2 + yyTop], (List<Expression>)yyVals[-1 + yyTop], (StringLiteral)yyVals[0 + yyTop]);
						break;
					case 456:
						yyVal = new InterpolatedString((StringLiteral)yyVals[0 + yyTop], null, null);
						break;
					case 457:
						case_457();
						break;
					case 458:
						case_458();
						break;
					case 459:
						yyVal = new InterpolatedStringInsert((Expression)yyVals[0 + yyTop]);
						break;
					case 460:
						case_460();
						break;
					case 461:
						lexer.parsing_interpolation_format = true;
						break;
					case 462:
						case_462();
						break;
					case 463:
						lexer.parsing_interpolation_format = true;
						break;
					case 464:
						case_464();
						break;
					case 469:
						case_469();
						break;
					case 470:
						yyVal = new ParenthesizedExpression((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
						break;
					case 471:
						case_471();
						break;
					case 472:
						case_472();
						break;
					case 473:
						case_473();
						break;
					case 474:
						case_474();
						break;
					case 475:
						case_475();
						break;
					case 476:
						case_476();
						break;
					case 477:
						case_477();
						break;
					case 478:
						case_478();
						break;
					case 479:
						yyVal = new CompletionMemberAccess((Expression)yyVals[-2 + yyTop], null, GetLocation(yyVals[0 + yyTop]));
						break;
					case 480:
						case_480();
						break;
					case 481:
						yyVal = new CompletionMemberAccess((Expression)yyVals[-2 + yyTop], null, lexer.Location);
						break;
					case 482:
						case_482();
						break;
					case 483:
						case_483();
						break;
					case 484:
						case_484();
						break;
					case 485:
						case_485();
						break;
					case 486:
						yyVal = null;
						break;
					case 488:
						case_488();
						break;
					case 489:
						case_489();
						break;
					case 490:
						yyVal = null;
						break;
					case 491:
						yyVal = yyVals[0 + yyTop];
						break;
					case 492:
						case_492();
						break;
					case 493:
						case_493();
						break;
					case 494:
						case_494();
						break;
					case 495:
						case_495();
						break;
					case 496:
						case_496();
						break;
					case 497:
						yyVal = new CompletionElementInitializer(null, GetLocation(yyVals[0 + yyTop]));
						break;
					case 498:
						case_498();
						break;
					case 499:
						case_499();
						break;
					case 500:
						case_500();
						break;
					case 501:
						case_501();
						break;
					case 504:
						yyVal = null;
						break;
					case 506:
						case_506();
						break;
					case 507:
						case_507();
						break;
					case 508:
						case_508();
						break;
					case 509:
						case_509();
						break;
					case 510:
						case_510();
						break;
					case 511:
						yyVal = new Argument((Expression)yyVals[0 + yyTop]);
						break;
					case 515:
						case_515();
						break;
					case 516:
						yyVal = new Argument((Expression)yyVals[0 + yyTop], Argument.AType.Ref);
						break;
					case 517:
						case_517();
						break;
					case 518:
						yyVal = new Argument((Expression)yyVals[0 + yyTop], Argument.AType.Out);
						break;
					case 519:
						case_519();
						break;
					case 520:
						case_520();
						break;
					case 521:
						case_521();
						break;
					case 522:
						case_522();
						break;
					case 523:
						case_523();
						break;
					case 525:
						case_525();
						break;
					case 526:
						case_526();
						break;
					case 527:
						case_527();
						break;
					case 528:
						case_528();
						break;
					case 529:
						case_529();
						break;
					case 530:
						case_530();
						break;
					case 531:
						case_531();
						break;
					case 532:
						case_532();
						break;
					case 533:
						yyVal = new Argument((Expression)yyVals[0 + yyTop]);
						break;
					case 535:
						yyVal = new This(GetLocation(yyVals[0 + yyTop]));
						break;
					case 536:
						case_536();
						break;
					case 537:
						case_537();
						break;
					case 538:
						yyVal = new UnaryMutator(UnaryMutator.Mode.IsPost, (Expression)yyVals[-1 + yyTop], GetLocation(yyVals[0 + yyTop]));
						break;
					case 539:
						yyVal = new UnaryMutator(UnaryMutator.Mode.PostDecrement, (Expression)yyVals[-1 + yyTop], GetLocation(yyVals[0 + yyTop]));
						break;
					case 540:
						case_540();
						break;
					case 541:
						case_541();
						break;
					case 542:
						case_542();
						break;
					case 543:
						case_543();
						break;
					case 544:
						case_544();
						break;
					case 545:
						case_545();
						break;
					case 546:
						case_546();
						break;
					case 547:
						lexer.parsing_type++;
						break;
					case 548:
						case_548();
						break;
					case 549:
						case_549();
						break;
					case 550:
						yyVal = new EmptyCompletion();
						break;
					case 553:
						yyVal = null;
						break;
					case 555:
						case_555();
						break;
					case 556:
						case_556();
						break;
					case 557:
						yyVal = new EmptyCompletion();
						break;
					case 558:
						yyVal = yyVals[-1 + yyTop];
						break;
					case 559:
						case_559();
						break;
					case 560:
						case_560();
						break;
					case 561:
						case_561();
						break;
					case 562:
						case_562();
						break;
					case 566:
						case_566();
						break;
					case 567:
						case_567();
						break;
					case 568:
						case_568();
						break;
					case 569:
						yyVal = 2;
						break;
					case 570:
						yyVal = (int)yyVals[-1 + yyTop] + 1;
						break;
					case 571:
						yyVal = null;
						break;
					case 572:
						yyVal = yyVals[0 + yyTop];
						break;
					case 573:
						case_573();
						break;
					case 574:
						case_574();
						break;
					case 575:
						case_575();
						break;
					case 576:
						case_576();
						break;
					case 577:
						case_577();
						break;
					case 579:
						case_579();
						break;
					case 580:
						case_580();
						break;
					case 581:
						case_581();
						break;
					case 582:
						case_582();
						break;
					case 583:
						case_583();
						break;
					case 584:
						case_584();
						break;
					case 585:
						case_585();
						break;
					case 586:
						case_586();
						break;
					case 587:
						case_587();
						break;
					case 588:
						case_588();
						break;
					case 589:
						start_anonymous(false, (ParametersCompiled)yyVals[0 + yyTop], false, GetLocation(yyVals[-1 + yyTop]));
						break;
					case 590:
						yyVal = end_anonymous((ParametersBlock)yyVals[0 + yyTop]);
						break;
					case 591:
						start_anonymous(false, (ParametersCompiled)yyVals[0 + yyTop], true, GetLocation(yyVals[-2 + yyTop]));
						break;
					case 592:
						yyVal = end_anonymous((ParametersBlock)yyVals[0 + yyTop]);
						break;
					case 593:
						yyVal = ParametersCompiled.Undefined;
						break;
					case 595:
						valid_param_mod = ParameterModifierType.Ref | ParameterModifierType.Out;
						break;
					case 596:
						case_596();
						break;
					case 597:
						case_597();
						break;
					case 599:
						yyVal = new Unary(Unary.Operator.LogicalNot, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 600:
						yyVal = new Unary(Unary.Operator.OnesComplement, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 601:
						case_601();
						break;
					case 602:
						case_602();
						break;
					case 603:
						case_603();
						break;
					case 604:
						case_604();
						break;
					case 605:
						case_605();
						break;
					case 606:
						case_606();
						break;
					case 608:
						yyVal = new Unary(Unary.Operator.UnaryPlus, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 609:
						yyVal = new Unary(Unary.Operator.UnaryNegation, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 610:
						yyVal = new UnaryMutator(UnaryMutator.Mode.IsIncrement, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 611:
						yyVal = new UnaryMutator(UnaryMutator.Mode.IsDecrement, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 612:
						yyVal = new Indirection((Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 613:
						yyVal = new Unary(Unary.Operator.AddressOf, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 614:
						case_614();
						break;
					case 615:
						case_615();
						break;
					case 616:
						case_616();
						break;
					case 617:
						case_617();
						break;
					case 618:
						case_618();
						break;
					case 619:
						case_619();
						break;
					case 621:
						case_621();
						break;
					case 622:
						case_622();
						break;
					case 623:
						case_623();
						break;
					case 624:
						case_624();
						break;
					case 625:
						case_625();
						break;
					case 626:
						case_626();
						break;
					case 628:
						case_628();
						break;
					case 629:
						case_629();
						break;
					case 630:
						case_630();
						break;
					case 631:
						case_631();
						break;
					case 632:
						yyVal = new As((Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 633:
						case_633();
						break;
					case 634:
						case_634();
						break;
					case 635:
						case_635();
						break;
					case 636:
						case_636();
						break;
					case 637:
						case_637();
						break;
					case 638:
						case_638();
						break;
					case 641:
						yyVal = new Unary(Unary.Operator.UnaryPlus, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 642:
						yyVal = new Unary(Unary.Operator.UnaryNegation, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 645:
						case_645();
						break;
					case 646:
						yyVal = new WildcardPattern(GetLocation(yyVals[0 + yyTop]));
						break;
					case 649:
						yyVal = new RecursivePattern((ATypeNameExpression)yyVals[-3 + yyTop], (Arguments)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
						break;
					case 650:
						yyVal = new PropertyPattern((ATypeNameExpression)yyVals[-3 + yyTop], (List<PropertyPatternMember>)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
						break;
					case 651:
						case_651();
						break;
					case 652:
						case_652();
						break;
					case 653:
						case_653();
						break;
					case 655:
						case_655();
						break;
					case 656:
						yyVal = new Arguments(0);
						break;
					case 658:
						case_658();
						break;
					case 659:
						case_659();
						break;
					case 660:
						yyVal = new Argument((Expression)yyVals[0 + yyTop]);
						break;
					case 661:
						case_661();
						break;
					case 663:
						case_663();
						break;
					case 664:
						case_664();
						break;
					case 665:
						case_665();
						break;
					case 666:
						case_666();
						break;
					case 668:
						case_668();
						break;
					case 669:
						case_669();
						break;
					case 670:
						case_670();
						break;
					case 671:
						case_671();
						break;
					case 672:
						case_672();
						break;
					case 673:
						case_673();
						break;
					case 674:
						case_674();
						break;
					case 675:
						case_675();
						break;
					case 677:
						case_677();
						break;
					case 678:
						case_678();
						break;
					case 679:
						case_679();
						break;
					case 680:
						case_680();
						break;
					case 682:
						case_682();
						break;
					case 683:
						case_683();
						break;
					case 685:
						case_685();
						break;
					case 686:
						case_686();
						break;
					case 688:
						case_688();
						break;
					case 689:
						case_689();
						break;
					case 691:
						case_691();
						break;
					case 692:
						case_692();
						break;
					case 694:
						case_694();
						break;
					case 695:
						case_695();
						break;
					case 697:
						case_697();
						break;
					case 699:
						case_699();
						break;
					case 700:
						case_700();
						break;
					case 701:
						case_701();
						break;
					case 702:
						case_702();
						break;
					case 703:
						case_703();
						break;
					case 704:
						case_704();
						break;
					case 705:
						case_705();
						break;
					case 706:
						case_706();
						break;
					case 707:
						case_707();
						break;
					case 708:
						case_708();
						break;
					case 709:
						case_709();
						break;
					case 710:
						case_710();
						break;
					case 711:
						case_711();
						break;
					case 712:
						case_712();
						break;
					case 713:
						case_713();
						break;
					case 714:
						case_714();
						break;
					case 715:
						case_715();
						break;
					case 716:
						case_716();
						break;
					case 717:
						case_717();
						break;
					case 718:
						case_718();
						break;
					case 719:
						case_719();
						break;
					case 720:
						yyVal = ParametersCompiled.EmptyReadOnlyParameters;
						break;
					case 721:
						case_721();
						break;
					case 722:
						start_block(Location.Null);
						break;
					case 723:
						case_723();
						break;
					case 725:
						case_725();
						break;
					case 727:
						case_727();
						break;
					case 728:
						case_728();
						break;
					case 729:
						case_729();
						break;
					case 730:
						case_730();
						break;
					case 731:
						case_731();
						break;
					case 732:
						case_732();
						break;
					case 733:
						case_733();
						break;
					case 734:
						valid_param_mod = ParameterModifierType.Ref | ParameterModifierType.Out;
						break;
					case 735:
						case_735();
						break;
					case 736:
						case_736();
						break;
					case 737:
						valid_param_mod = ParameterModifierType.Ref | ParameterModifierType.Out;
						break;
					case 738:
						case_738();
						break;
					case 739:
						case_739();
						break;
					case 745:
						yyVal = new ArglistAccess(GetLocation(yyVals[0 + yyTop]));
						break;
					case 746:
						case_746();
						break;
					case 747:
						case_747();
						break;
					case 748:
						case_748();
						break;
					case 750:
						yyVal = new BooleanExpression((Expression)yyVals[0 + yyTop]);
						break;
					case 751:
						yyVal = null;
						break;
					case 753:
						case_753();
						break;
					case 754:
						yyVal = null;
						break;
					case 755:
						yyVal = null;
						break;
					case 756:
						yyVal = yyVals[0 + yyTop];
						break;
					case 757:
						yyVal = yyVals[-1 + yyTop];
						break;
					case 758:
						case_758();
						break;
					case 759:
						case_759();
						break;
					case 761:
						case_761();
						break;
					case 762:
						case_762();
						break;
					case 763:
						case_763();
						break;
					case 764:
						case_764();
						break;
					case 765:
						yyVal = null;
						break;
					case 766:
						yyVal = yyVals[0 + yyTop];
						break;
					case 767:
						case_767();
						break;
					case 768:
						lexer.parsing_modifiers = false;
						break;
					case 770:
						case_770();
						break;
					case 771:
						case_771();
						break;
					case 772:
						case_772();
						break;
					case 773:
						case_773();
						break;
					case 774:
						case_774();
						break;
					case 775:
						case_775();
						break;
					case 776:
						case_776();
						break;
					case 777:
						case_777();
						break;
					case 778:
						case_778();
						break;
					case 779:
						case_779();
						break;
					case 780:
						case_780();
						break;
					case 781:
						case_781();
						break;
					case 782:
						case_782();
						break;
					case 783:
						case_783();
						break;
					case 784:
						case_784();
						break;
					case 785:
						case_785();
						break;
					case 788:
						current_type.SetBaseTypes((List<FullNamedExpression>)yyVals[0 + yyTop]);
						break;
					case 789:
						case_789();
						break;
					case 791:
						yyVal = yyVals[0 + yyTop];
						break;
					case 792:
						case_792();
						break;
					case 793:
						case_793();
						break;
					case 794:
						case_794();
						break;
					case 795:
						case_795();
						break;
					case 796:
						case_796();
						break;
					case 797:
						case_797();
						break;
					case 798:
						case_798();
						break;
					case 799:
						case_799();
						break;
					case 800:
						yyVal = new SpecialContraintExpr(SpecialConstraint.Class, GetLocation(yyVals[0 + yyTop]));
						break;
					case 801:
						yyVal = new SpecialContraintExpr(SpecialConstraint.Struct, GetLocation(yyVals[0 + yyTop]));
						break;
					case 802:
						yyVal = null;
						break;
					case 803:
						case_803();
						break;
					case 804:
						yyVal = new VarianceDecl(Variance.Covariant, GetLocation(yyVals[0 + yyTop]));
						break;
					case 805:
						yyVal = new VarianceDecl(Variance.Contravariant, GetLocation(yyVals[0 + yyTop]));
						break;
					case 806:
						case_806();
						break;
					case 807:
						yyVal = yyVals[0 + yyTop];
						break;
					case 808:
						case_808();
						break;
					case 809:
						case_809();
						break;
					case 810:
						case_810();
						break;
					case 811:
						case_811();
						break;
					case 816:
						current_block.AddStatement((Statement)yyVals[0 + yyTop]);
						break;
					case 817:
						current_block.AddStatement((Statement)yyVals[0 + yyTop]);
						break;
					case 819:
						case_819();
						break;
					case 822:
						current_block.AddStatement((Statement)yyVals[0 + yyTop]);
						break;
					case 823:
						current_block.AddStatement((Statement)yyVals[0 + yyTop]);
						break;
					case 852:
						case_852();
						break;
					case 853:
						case_853();
						break;
					case 854:
						case_854();
						break;
					case 855:
						case_855();
						break;
					case 856:
						case_856();
						break;
					case 859:
						case_859();
						break;
					case 860:
						case_860();
						break;
					case 861:
						case_861();
						break;
					case 865:
						case_865();
						break;
					case 866:
						yyVal = ComposedTypeSpecifier.CreatePointer(GetLocation(yyVals[0 + yyTop]));
						break;
					case 868:
						yyVal = Error_AwaitAsIdentifier(yyVals[0 + yyTop]);
						break;
					case 869:
						case_869();
						break;
					case 870:
						case_870();
						break;
					case 871:
						case_871();
						break;
					case 872:
						case_872();
						break;
					case 874:
						case_874();
						break;
					case 875:
						case_875();
						break;
					case 879:
						case_879();
						break;
					case 882:
						case_882();
						break;
					case 883:
						case_883();
						break;
					case 884:
						report.Error(145, lexer.Location, "A const field requires a value to be provided");
						break;
					case 885:
						current_variable.Initializer = (Expression)yyVals[0 + yyTop];
						break;
					case 890:
						case_890();
						break;
					case 892:
						case_892();
						break;
					case 893:
						case_893();
						break;
					case 894:
						case_894();
						break;
					case 895:
						yyVal = yyVals[-1 + yyTop];
						break;
					case 896:
						case_896();
						break;
					case 897:
						yyVal = yyVals[-1 + yyTop];
						break;
					case 898:
						yyVal = yyVals[-1 + yyTop];
						break;
					case 899:
						case_899();
						break;
					case 900:
						case_900();
						break;
					case 901:
						case_901();
						break;
					case 904:
						case_904();
						break;
					case 905:
						case_905();
						break;
					case 906:
						case_906();
						break;
					case 907:
						start_block(GetLocation(yyVals[0 + yyTop]));
						break;
					case 908:
						case_908();
						break;
					case 909:
						case_909();
						break;
					case 910:
						report.Warning(1522, 1, current_block.StartLocation, "Empty switch block");
						break;
					case 914:
						Error_SyntaxError(yyToken);
						break;
					case 916:
						case_916();
						break;
					case 917:
						current_block.AddStatement((Statement)yyVals[0 + yyTop]);
						break;
					case 918:
						case_918();
						break;
					case 919:
						case_919();
						break;
					case 920:
						yyVal = new SwitchLabel(null, GetLocation(yyVals[0 + yyTop]));
						break;
					case 925:
						case_925();
						break;
					case 926:
						case_926();
						break;
					case 927:
						case_927();
						break;
					case 928:
						case_928();
						break;
					case 929:
						case_929();
						break;
					case 930:
						case_930();
						break;
					case 931:
						yyVal = yyVals[0 + yyTop];
						break;
					case 932:
						case_932();
						break;
					case 933:
						case_933();
						break;
					case 934:
						case_934();
						break;
					case 935:
						case_935();
						break;
					case 936:
						yyVal = new Tuple<Location, Location>(GetLocation(yyVals[-2 + yyTop]), (Location)yyVals[0 + yyTop]);
						break;
					case 937:
						case_937();
						break;
					case 938:
						case_938();
						break;
					case 939:
						case_939();
						break;
					case 941:
						lexer.putback(125);
						break;
					case 942:
						yyVal = new EmptyStatement(lexer.Location);
						break;
					case 944:
						case_944();
						break;
					case 945:
						case_945();
						break;
					case 947:
						yyVal = null;
						break;
					case 949:
						yyVal = new EmptyStatement(lexer.Location);
						break;
					case 953:
						case_953();
						break;
					case 954:
						case_954();
						break;
					case 955:
						case_955();
						break;
					case 956:
						case_956();
						break;
					case 957:
						case_957();
						break;
					case 964:
						case_964();
						break;
					case 965:
						case_965();
						break;
					case 966:
						case_966();
						break;
					case 967:
						case_967();
						break;
					case 968:
						case_968();
						break;
					case 969:
						case_969();
						break;
					case 970:
						case_970();
						break;
					case 971:
						case_971();
						break;
					case 972:
						case_972();
						break;
					case 973:
						case_973();
						break;
					case 974:
						case_974();
						break;
					case 975:
						case_975();
						break;
					case 976:
						case_976();
						break;
					case 977:
						case_977();
						break;
					case 978:
						case_978();
						break;
					case 981:
						yyVal = new TryCatch((Block)yyVals[-1 + yyTop], (List<Catch>)yyVals[0 + yyTop], GetLocation(yyVals[-2 + yyTop]), false);
						break;
					case 982:
						case_982();
						break;
					case 983:
						case_983();
						break;
					case 984:
						case_984();
						break;
					case 985:
						case_985();
						break;
					case 986:
						case_986();
						break;
					case 989:
						case_989();
						break;
					case 990:
						case_990();
						break;
					case 991:
						case_991();
						break;
					case 992:
						case_992();
						break;
					case 993:
						yyVal = yyVals[-1 + yyTop];
						break;
					case 994:
						case_994();
						break;
					case 995:
						lexer.parsing_catch_when = false;
						break;
					case 996:
						lexer.parsing_catch_when = false;
						break;
					case 997:
						case_997();
						break;
					case 998:
						yyVal = new Checked((Block)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 999:
						yyVal = new Unchecked((Block)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
						break;
					case 1000:
						case_1000();
						break;
					case 1001:
						yyVal = new Unsafe((Block)yyVals[0 + yyTop], GetLocation(yyVals[-2 + yyTop]));
						break;
					case 1002:
						case_1002();
						break;
					case 1003:
						case_1003();
						break;
					case 1004:
						case_1004();
						break;
					case 1005:
						case_1005();
						break;
					case 1006:
						case_1006();
						break;
					case 1007:
						case_1007();
						break;
					case 1008:
						case_1008();
						break;
					case 1009:
						case_1009();
						break;
					case 1010:
						case_1010();
						break;
					case 1011:
						case_1011();
						break;
					case 1013:
						case_1013();
						break;
					case 1014:
						Error_MissingInitializer(lexer.Location);
						break;
					case 1015:
						case_1015();
						break;
					case 1016:
						case_1016();
						break;
					case 1017:
						case_1017();
						break;
					case 1018:
						case_1018();
						break;
					case 1019:
						case_1019();
						break;
					case 1020:
						case_1020();
						break;
					case 1021:
						case_1021();
						break;
					case 1022:
						case_1022();
						break;
					case 1023:
						case_1023();
						break;
					case 1024:
						current_block = new QueryBlock(current_block, lexer.Location);
						break;
					case 1025:
						case_1025();
						break;
					case 1026:
						current_block = new QueryBlock(current_block, lexer.Location);
						break;
					case 1027:
						case_1027();
						break;
					case 1028:
						case_1028();
						break;
					case 1029:
						case_1029();
						break;
					case 1031:
						case_1031();
						break;
					case 1032:
						case_1032();
						break;
					case 1033:
						current_block = new QueryBlock(current_block, lexer.Location);
						break;
					case 1034:
						case_1034();
						break;
					case 1035:
						case_1035();
						break;
					case 1036:
						case_1036();
						break;
					case 1037:
						case_1037();
						break;
					case 1038:
						yyVal = new object[2]
						{
							yyVals[0 + yyTop],
							GetLocation(yyVals[-1 + yyTop])
						};
						break;
					case 1039:
						case_1039();
						break;
					case 1041:
						case_1041();
						break;
					case 1047:
						current_block = new QueryBlock(current_block, lexer.Location);
						break;
					case 1048:
						case_1048();
						break;
					case 1049:
						current_block = new QueryBlock(current_block, lexer.Location);
						break;
					case 1050:
						case_1050();
						break;
					case 1051:
						case_1051();
						break;
					case 1052:
						case_1052();
						break;
					case 1053:
						case_1053();
						break;
					case 1054:
						case_1054();
						break;
					case 1055:
						case_1055();
						break;
					case 1056:
						case_1056();
						break;
					case 1057:
						case_1057();
						break;
					case 1058:
						case_1058();
						break;
					case 1060:
						yyVal = yyVals[0 + yyTop];
						break;
					case 1061:
						current_block = new QueryBlock(current_block, lexer.Location);
						break;
					case 1062:
						case_1062();
						break;
					case 1064:
						case_1064();
						break;
					case 1065:
						case_1065();
						break;
					case 1067:
						case_1067();
						break;
					case 1068:
						case_1068();
						break;
					case 1069:
						yyVal = new OrderByAscending((QueryBlock)current_block, (Expression)yyVals[0 + yyTop]);
						break;
					case 1070:
						case_1070();
						break;
					case 1071:
						case_1071();
						break;
					case 1072:
						yyVal = new ThenByAscending((QueryBlock)current_block, (Expression)yyVals[0 + yyTop]);
						break;
					case 1073:
						case_1073();
						break;
					case 1074:
						case_1074();
						break;
					case 1076:
						case_1076();
						break;
					case 1077:
						case_1077();
						break;
					case 1080:
						case_1080();
						break;
					case 1081:
						case_1081();
						break;
					case 1089:
						module.DocumentationBuilder.ParsedName = (MemberName)yyVals[0 + yyTop];
						break;
					case 1090:
						module.DocumentationBuilder.ParsedParameters = (List<DocumentationParameter>)yyVals[0 + yyTop];
						break;
					case 1091:
						case_1091();
						break;
					case 1092:
						case_1092();
						break;
					case 1093:
						case_1093();
						break;
					case 1094:
						yyVal = new MemberName((MemberName)yyVals[-2 + yyTop], MemberCache.IndexerNameAlias, Location.Null);
						break;
					case 1095:
						valid_param_mod = ParameterModifierType.Ref | ParameterModifierType.Out;
						break;
					case 1096:
						case_1096();
						break;
					case 1097:
						case_1097();
						break;
					case 1098:
						case_1098();
						break;
					case 1099:
						case_1099();
						break;
					case 1101:
						yyVal = new MemberName((MemberName)yyVals[-2 + yyTop], (MemberName)yyVals[0 + yyTop]);
						break;
					case 1103:
						valid_param_mod = ParameterModifierType.Ref | ParameterModifierType.Out;
						break;
					case 1104:
						yyVal = yyVals[-1 + yyTop];
						break;
					case 1105:
						yyVal = new List<DocumentationParameter>(0);
						break;
					case 1107:
						case_1107();
						break;
					case 1108:
						case_1108();
						break;
					case 1109:
						case_1109();
						break;
					}
					yyTop -= yyLen[num3];
					num = array[yyTop];
					int num5 = yyLhs[num3];
					if (num == 0 && num5 == 0)
					{
						if (debug != null)
						{
							debug.shift(0, 7);
						}
						num = 7;
						if (yyToken < 0)
						{
							yyToken = (yyLex.advance() ? yyLex.token() : 0);
							if (debug != null)
							{
								debug.lex(num, yyToken, yyname(yyToken), yyLex.value());
							}
						}
						if (yyToken != 0)
						{
							break;
						}
						if (debug != null)
						{
							debug.accept(yyVal);
						}
						return yyVal;
					}
					num = (((num3 = yyGindex[num5]) == 0 || (num3 += num) < 0 || num3 >= yyTable.Length || yyCheck[num3] != num) ? yyDgoto[num5] : yyTable[num3]);
					if (debug != null)
					{
						debug.shift(array[yyTop], num);
					}
					break;
				}
				yyTop++;
			}
		}

		private void case_6()
		{
			if (yyVals[0 + yyTop] != null)
			{
				Attributes attributes = (Attributes)yyVals[0 + yyTop];
				report.Error(1730, attributes.Attrs[0].Location, "Assembly and module attributes must precede all other elements except using clauses and extern alias declarations");
				current_namespace.UnattachedAttributes = attributes;
			}
		}

		private void case_8()
		{
			if (yyToken == 358)
			{
				report.Error(439, lexer.Location, "An extern alias declaration must precede all other elements");
			}
			else
			{
				Error_SyntaxError(yyToken);
			}
		}

		private void case_13()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			if (locatedToken.Value != "alias")
			{
				syntax_error(locatedToken.Location, "`alias' expected");
				return;
			}
			if (lang_version == LanguageVersion.ISO_1)
			{
				FeatureIsNotAvailable(locatedToken.Location, "external alias");
			}
			locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			if (locatedToken.Value == QualifiedAliasMember.GlobalAlias)
			{
				RootNamespace.Error_GlobalNamespaceRedefined(report, locatedToken.Location);
			}
			UsingExternAlias un = new UsingExternAlias(new SimpleMemberName(locatedToken.Value, locatedToken.Location), GetLocation(yyVals[-3 + yyTop]));
			current_namespace.AddUsing(un);
		}

		private void case_17()
		{
			if (doc_support)
			{
				Lexer.doc_state = XmlCommentState.Allowed;
			}
		}

		private void case_18()
		{
			UsingClause un;
			if (yyVals[-2 + yyTop] != null)
			{
				if (lang_version <= LanguageVersion.V_5)
				{
					FeatureIsNotAvailable(GetLocation(yyVals[-2 + yyTop]), "using static");
				}
				un = new UsingType((ATypeNameExpression)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop]));
			}
			else
			{
				un = new UsingNamespace((ATypeNameExpression)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop]));
			}
			current_namespace.AddUsing(un);
		}

		private void case_19()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			if (lang_version != LanguageVersion.ISO_1 && locatedToken.Value == "global")
			{
				report.Warning(440, 2, locatedToken.Location, "An alias named `global' will not be used when resolving `global::'. The global namespace will be used instead");
			}
			if (yyVals[-4 + yyTop] != null)
			{
				report.Error(8085, GetLocation(yyVals[-4 + yyTop]), "A `using static' directive cannot be used to declare an alias");
			}
			UsingAliasNamespace un = new UsingAliasNamespace(new SimpleMemberName(locatedToken.Value, locatedToken.Location), (ATypeNameExpression)yyVals[-1 + yyTop], GetLocation(yyVals[-5 + yyTop]));
			current_namespace.AddUsing(un);
		}

		private void case_20()
		{
			Error_SyntaxError(yyToken);
			yyVal = null;
		}

		private void case_23()
		{
			Attributes attributes = (Attributes)yyVals[-2 + yyTop];
			MemberName memberName = (MemberName)yyVals[0 + yyTop];
			if (attributes != null)
			{
				bool flag = true;
				if (current_namespace.DeclarationFound || current_namespace != file)
				{
					flag = false;
				}
				else
				{
					foreach (Attribute attr in attributes.Attrs)
					{
						if (!(attr.ExplicitTarget == "assembly") && !(attr.ExplicitTarget == "module"))
						{
							flag = false;
							break;
						}
					}
				}
				if (!flag)
				{
					report.Error(1671, memberName.Location, "A namespace declaration cannot have modifiers or attributes");
				}
			}
			module.AddAttributes(attributes, current_namespace);
			NamespaceContainer tc = new NamespaceContainer(memberName, current_namespace);
			current_namespace.AddTypeContainer(tc);
			current_container = (current_namespace = tc);
		}

		private void case_24()
		{
			if (doc_support)
			{
				Lexer.doc_state = XmlCommentState.Allowed;
			}
		}

		private void case_25()
		{
			object obj = yyVals[0 + yyTop];
			current_container = (current_namespace = current_namespace.Parent);
		}

		private void case_26()
		{
			report.Error(1514, lexer.Location, "Unexpected symbol `{0}', expecting `.' or `{{'", GetSymbolName(yyToken));
			NamespaceContainer tc = new NamespaceContainer((MemberName)yyVals[0 + yyTop], current_namespace);
			current_namespace.AddTypeContainer(tc);
		}

		private void case_29()
		{
			Error_SyntaxError(yyToken);
			yyVal = null;
		}

		private void case_30()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			yyVal = new MemberName(locatedToken.Value, locatedToken.Location);
		}

		private void case_31()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			yyVal = new MemberName((MemberName)yyVals[-2 + yyTop], locatedToken.Value, locatedToken.Location);
		}

		private void case_32()
		{
			Error_SyntaxError(yyToken);
			yyVal = new MemberName("<invalid>", lexer.Location);
		}

		private void case_45()
		{
			if (yyVals[0 + yyTop] != null)
			{
				TypeContainer typeContainer = (TypeContainer)yyVals[0 + yyTop];
				if ((typeContainer.ModFlags & (Modifiers.PROTECTED | Modifiers.PRIVATE)) != 0)
				{
					report.Error(1527, typeContainer.Location, "Namespace elements cannot be explicitly declared as private, protected or protected internal");
				}
				if (typeContainer.OptAttributes != null)
				{
					typeContainer.OptAttributes.ConvertGlobalAttributes(typeContainer, current_namespace, !current_namespace.DeclarationFound && current_namespace == file);
				}
			}
			current_namespace.DeclarationFound = true;
		}

		private void case_47()
		{
			current_namespace.UnattachedAttributes = (Attributes)yyVals[-1 + yyTop];
			report.Error(1518, lexer.Location, "Attributes must be attached to class, delegate, enum, interface or struct");
			lexer.putback(125);
		}

		private void case_55()
		{
			List<Attribute> attrs = (List<Attribute>)yyVals[0 + yyTop];
			yyVal = new Attributes(attrs);
		}

		private void case_56()
		{
			Attributes attributes = yyVals[-1 + yyTop] as Attributes;
			List<Attribute> list = (List<Attribute>)yyVals[0 + yyTop];
			if (attributes == null)
			{
				attributes = new Attributes(list);
			}
			else if (list != null)
			{
				attributes.AddAttributes(list);
			}
			yyVal = attributes;
		}

		private void case_57()
		{
			lexer.parsing_attribute_section = true;
		}

		private void case_58()
		{
			lexer.parsing_attribute_section = false;
			yyVal = yyVals[0 + yyTop];
		}

		private void case_59()
		{
			current_attr_target = (string)yyVals[-1 + yyTop];
			if (current_attr_target == "assembly" || current_attr_target == "module")
			{
				Lexer.check_incorrect_doc_comment();
			}
		}

		private void case_60()
		{
			if (current_attr_target == string.Empty)
			{
				yyVal = new List<Attribute>(0);
			}
			else
			{
				yyVal = yyVals[-2 + yyTop];
			}
			object obj = yyVals[-1 + yyTop];
			current_attr_target = null;
			lexer.parsing_attribute_section = false;
		}

		private void case_61()
		{
			yyVal = yyVals[-2 + yyTop];
			object obj = yyVals[-1 + yyTop];
		}

		private void case_62()
		{
			Error_SyntaxError(yyToken);
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			SimpleName expr = new SimpleName(locatedToken.Value, null, locatedToken.Location);
			yyVal = new List<Attribute>
			{
				new Attribute(null, expr, null, GetLocation(yyVals[-1 + yyTop]), false)
			};
		}

		private void case_63()
		{
			if (CheckAttributeTarget(yyToken, GetTokenName(yyToken), GetLocation(yyVals[0 + yyTop])).Length > 0)
			{
				Error_SyntaxError(yyToken);
			}
			yyVal = null;
		}

		private void case_64()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			yyVal = CheckAttributeTarget(yyToken, locatedToken.Value, locatedToken.Location);
		}

		private void case_68()
		{
			List<Attribute> list = (List<Attribute>)yyVals[-2 + yyTop];
			list?.Add((Attribute)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_70()
		{
			lexer.parsing_block--;
			ATypeNameExpression aTypeNameExpression = (ATypeNameExpression)yyVals[-2 + yyTop];
			if (aTypeNameExpression.HasTypeArguments)
			{
				report.Error(404, aTypeNameExpression.Location, "Attributes cannot be generic");
			}
			yyVal = new Attribute(current_attr_target, aTypeNameExpression, (Arguments[])yyVals[0 + yyTop], GetLocation(yyVals[-2 + yyTop]), lexer.IsEscapedIdentifier(aTypeNameExpression));
		}

		private void case_75()
		{
			Arguments arguments = new Arguments(4);
			arguments.Add((Argument)yyVals[0 + yyTop]);
			yyVal = new Arguments[2] { arguments, null };
		}

		private void case_76()
		{
			Arguments arguments = new Arguments(4);
			arguments.Add((Argument)yyVals[0 + yyTop]);
			yyVal = new Arguments[2] { null, arguments };
		}

		private void case_77()
		{
			Arguments[] array = (Arguments[])yyVals[-2 + yyTop];
			if (array[1] != null)
			{
				report.Error(1016, ((Argument)yyVals[0 + yyTop]).Expr.Location, "Named attribute arguments must appear after the positional arguments");
				array[0] = new Arguments(4);
			}
			Arguments arguments = array[0];
			if (arguments.Count > 0 && !(yyVals[0 + yyTop] is NamedArgument))
			{
				if (arguments[arguments.Count - 1] is NamedArgument)
				{
					Error_NamedArgumentExpected((NamedArgument)arguments[arguments.Count - 1]);
				}
			}
			arguments.Add((Argument)yyVals[0 + yyTop]);
		}

		private void case_78()
		{
			Arguments[] array = (Arguments[])yyVals[-2 + yyTop];
			if (array[1] == null)
			{
				array[1] = new Arguments(4);
			}
			array[1].Add((Argument)yyVals[0 + yyTop]);
		}

		private void case_81()
		{
			Error_SyntaxError(yyToken);
			yyVal = null;
		}

		private void case_83()
		{
			lexer.parsing_block--;
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			yyVal = new NamedArgument(locatedToken.Value, locatedToken.Location, (Expression)yyVals[0 + yyTop]);
		}

		private void case_84()
		{
			if (lang_version <= LanguageVersion.V_3)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-3 + yyTop]), "named argument");
			}
			Argument.AType modifier = ((yyVals[-1 + yyTop] != null) ? ((Argument.AType)yyVals[-1 + yyTop]) : Argument.AType.None);
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			yyVal = new NamedArgument(locatedToken.Value, locatedToken.Location, (Expression)yyVals[0 + yyTop], modifier);
		}

		private void case_91()
		{
			lexer.parsing_modifiers = true;
			lexer.parsing_block = 0;
		}

		private void case_92()
		{
			lexer.parsing_modifiers = true;
			lexer.parsing_block = 0;
		}

		private void case_106()
		{
			report.Error(1519, lexer.Location, "Unexpected symbol `{0}' in class, struct, or interface member declaration", GetSymbolName(yyToken));
			yyVal = null;
			lexer.parsing_generic_declaration = false;
		}

		private void case_107()
		{
			current_local_parameters = current_type.PrimaryConstructorParameters;
			if (current_local_parameters == null)
			{
				report.Error(9010, GetLocation(yyVals[0 + yyTop]), "Primary constructor body is not allowed");
				current_local_parameters = ParametersCompiled.EmptyReadOnlyParameters;
			}
			lexer.parsing_block++;
			start_block(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_108()
		{
			current_local_parameters = null;
			ClassOrStruct classOrStruct = current_type as ClassOrStruct;
			if (classOrStruct != null)
			{
				ToplevelBlock toplevelBlock = (ToplevelBlock)yyVals[0 + yyTop];
				if (classOrStruct.PrimaryConstructorBlock != null)
				{
					report.Error(8041, toplevelBlock.StartLocation, "Primary constructor already has a body");
				}
				else
				{
					classOrStruct.PrimaryConstructorBlock = toplevelBlock;
				}
			}
		}

		private void case_110()
		{
			lexer.ConstraintsParsing = true;
			valid_param_mod = ParameterModifierType.PrimaryConstructor;
			push_current_container(new Struct(current_container, (MemberName)yyVals[0 + yyTop], (Modifiers)yyVals[-4 + yyTop], (Attributes)yyVals[-5 + yyTop]), yyVals[-3 + yyTop]);
		}

		private void case_111()
		{
			valid_param_mod = (ParameterModifierType)0;
			lexer.ConstraintsParsing = false;
			if (yyVals[-2 + yyTop] != null)
			{
				current_type.PrimaryConstructorParameters = (ParametersCompiled)yyVals[-2 + yyTop];
			}
			if (yyVals[0 + yyTop] != null)
			{
				current_container.SetConstraints((List<Constraints>)yyVals[0 + yyTop]);
			}
			if (doc_support)
			{
				current_container.PartialContainer.DocComment = Lexer.consume_doc_comment();
			}
			lexer.parsing_modifiers = true;
		}

		private void case_112()
		{
			if (doc_support)
			{
				Lexer.doc_state = XmlCommentState.Allowed;
			}
		}

		private void case_113()
		{
			lexer.parsing_declaration--;
			if (doc_support)
			{
				Lexer.doc_state = XmlCommentState.Allowed;
			}
		}

		private void case_114()
		{
			object obj = yyVals[-1 + yyTop];
			yyVal = pop_current_class();
		}

		private void case_116()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			Modifiers modifiers = (Modifiers)yyVals[-3 + yyTop];
			current_field = new Const(current_type, (FullNamedExpression)yyVals[-1 + yyTop], modifiers, new MemberName(locatedToken.Value, locatedToken.Location), (Attributes)yyVals[-4 + yyTop]);
			current_type.AddMember(current_field);
			if ((modifiers & Modifiers.STATIC) != 0)
			{
				report.Error(504, current_field.Location, "The constant `{0}' cannot be marked static", current_field.GetSignatureForError());
			}
			yyVal = current_field;
		}

		private void case_117()
		{
			if (doc_support)
			{
				current_field.DocComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			current_field.Initializer = (ConstInitializer)yyVals[-2 + yyTop];
			current_field = null;
		}

		private void case_118()
		{
			Error_SyntaxError(yyToken);
			current_type.AddMember(new Const(current_type, (FullNamedExpression)yyVals[-1 + yyTop], (Modifiers)yyVals[-3 + yyTop], MemberName.Null, (Attributes)yyVals[-4 + yyTop]));
		}

		private void case_123()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new FieldDeclarator(new SimpleMemberName(locatedToken.Value, locatedToken.Location), (ConstInitializer)yyVals[0 + yyTop]);
		}

		private void case_125()
		{
			lexer.parsing_block--;
			yyVal = new ConstInitializer(current_field, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_126()
		{
			report.Error(145, lexer.Location, "A const field requires a value to be provided");
			yyVal = null;
		}

		private void case_129()
		{
			lexer.parsing_generic_declaration = false;
			FullNamedExpression fullNamedExpression = (FullNamedExpression)yyVals[-1 + yyTop];
			if (fullNamedExpression.Type != null && fullNamedExpression.Type.Kind == MemberKind.Void)
			{
				report.Error(670, GetLocation(yyVals[-1 + yyTop]), "Fields cannot have void type");
			}
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			current_field = new Field(current_type, fullNamedExpression, (Modifiers)yyVals[-2 + yyTop], new MemberName(locatedToken.Value, locatedToken.Location), (Attributes)yyVals[-3 + yyTop]);
			current_type.AddField(current_field);
			yyVal = current_field;
		}

		private void case_130()
		{
			if (doc_support)
			{
				current_field.DocComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			yyVal = current_field;
			current_field = null;
		}

		private void case_131()
		{
			if (lang_version < LanguageVersion.ISO_2)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-2 + yyTop]), "fixed size buffers");
			}
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			current_field = new FixedField(current_type, (FullNamedExpression)yyVals[-1 + yyTop], (Modifiers)yyVals[-3 + yyTop], new MemberName(locatedToken.Value, locatedToken.Location), (Attributes)yyVals[-4 + yyTop]);
			current_type.AddField(current_field);
		}

		private void case_132()
		{
			if (doc_support)
			{
				current_field.DocComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			current_field.Initializer = (ConstInitializer)yyVals[-2 + yyTop];
			yyVal = current_field;
			current_field = null;
		}

		private void case_135()
		{
			lexer.parsing_block++;
			current_local_parameters = ParametersCompiled.EmptyReadOnlyParameters;
			start_block(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_136()
		{
			lexer.parsing_block--;
			current_field.Initializer = (Expression)yyVals[0 + yyTop];
			end_block(lexer.Location);
			current_local_parameters = null;
		}

		private void case_141()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			yyVal = new FieldDeclarator(new SimpleMemberName(locatedToken.Value, locatedToken.Location), null);
		}

		private void case_143()
		{
			lexer.parsing_block--;
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			yyVal = new FieldDeclarator(new SimpleMemberName(locatedToken.Value, locatedToken.Location), (Expression)yyVals[0 + yyTop]);
		}

		private void case_148()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new FieldDeclarator(new SimpleMemberName(locatedToken.Value, locatedToken.Location), (ConstInitializer)yyVals[0 + yyTop]);
		}

		private void case_150()
		{
			lexer.parsing_block--;
			yyVal = new ConstInitializer(current_field, (Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_151()
		{
			report.Error(443, lexer.Location, "Value or constant expected");
			yyVal = null;
		}

		private void case_154()
		{
			Error_SyntaxError(yyToken);
			yyVal = null;
		}

		private void case_155()
		{
			if (doc_support)
			{
				Lexer.doc_state = XmlCommentState.NotAllowed;
			}
		}

		private void case_156()
		{
			Method method = (Method)yyVals[-2 + yyTop];
			method.Block = (ToplevelBlock)yyVals[0 + yyTop];
			async_block = false;
			if (method.Block == null)
			{
				method.ParameterInfo.CheckParameters(method);
				if ((method.ModFlags & Modifiers.ASYNC) != 0)
				{
					report.Error(1994, method.Location, "`{0}': The async modifier can only be used with methods that have a body", method.GetSignatureForError());
				}
			}
			else if (current_container.Kind == MemberKind.Interface)
			{
				report.Error(531, method.Location, "`{0}': interface members cannot have a definition", method.GetSignatureForError());
			}
			current_local_parameters = null;
			if (doc_support)
			{
				Lexer.doc_state = XmlCommentState.Allowed;
			}
		}

		private void case_158()
		{
			valid_param_mod = (ParameterModifierType)0;
			MemberName name = (MemberName)yyVals[-4 + yyTop];
			current_local_parameters = (ParametersCompiled)yyVals[-1 + yyTop];
			Method method = Method.Create(current_type, (FullNamedExpression)yyVals[-5 + yyTop], (Modifiers)yyVals[-6 + yyTop], name, current_local_parameters, (Attributes)yyVals[-7 + yyTop]);
			current_type.AddMember(method);
			async_block = (method.ModFlags & Modifiers.ASYNC) != 0;
			if (doc_support)
			{
				method.DocComment = Lexer.consume_doc_comment();
			}
			yyVal = method;
			lexer.ConstraintsParsing = true;
		}

		private void case_159()
		{
			lexer.ConstraintsParsing = false;
			if (yyVals[0 + yyTop] != null)
			{
				((Method)yyVals[-1 + yyTop]).SetConstraints((List<Constraints>)yyVals[0 + yyTop]);
			}
			yyVal = yyVals[-1 + yyTop];
		}

		private void case_161()
		{
			lexer.parsing_generic_declaration = false;
			valid_param_mod = ParameterModifierType.All;
		}

		private void case_163()
		{
			lexer.ConstraintsParsing = false;
			valid_param_mod = (ParameterModifierType)0;
			MemberName name = (MemberName)yyVals[-6 + yyTop];
			current_local_parameters = (ParametersCompiled)yyVals[-3 + yyTop];
			Modifiers modifiers = (Modifiers)yyVals[-10 + yyTop];
			modifiers |= Modifiers.PARTIAL;
			Method method = Method.Create(current_type, new TypeExpression(compiler.BuiltinTypes.Void, GetLocation(yyVals[-8 + yyTop])), modifiers, name, current_local_parameters, (Attributes)yyVals[-11 + yyTop]);
			current_type.AddMember(method);
			async_block = (method.ModFlags & Modifiers.ASYNC) != 0;
			if (yyVals[0 + yyTop] != null)
			{
				method.SetConstraints((List<Constraints>)yyVals[0 + yyTop]);
			}
			if (doc_support)
			{
				method.DocComment = Lexer.consume_doc_comment();
			}
			yyVal = method;
		}

		private void case_164()
		{
			MemberName memberName = (MemberName)yyVals[-3 + yyTop];
			report.Error(1585, memberName.Location, "Member modifier `{0}' must precede the member type and name", ModifiersExtensions.Name((Modifiers)yyVals[-4 + yyTop]));
			Method method = Method.Create(current_type, (FullNamedExpression)yyVals[-5 + yyTop], (Modifiers)0, memberName, (ParametersCompiled)yyVals[-1 + yyTop], (Attributes)yyVals[-7 + yyTop]);
			current_type.AddMember(method);
			current_local_parameters = (ParametersCompiled)yyVals[-1 + yyTop];
			if (doc_support)
			{
				method.DocComment = Lexer.consume_doc_comment();
			}
			yyVal = method;
		}

		private void case_165()
		{
			Error_SyntaxError(yyToken);
			current_local_parameters = ParametersCompiled.Undefined;
			MemberName name = (MemberName)yyVals[-1 + yyTop];
			Method method = Method.Create(current_type, (FullNamedExpression)yyVals[-2 + yyTop], (Modifiers)yyVals[-3 + yyTop], name, current_local_parameters, (Attributes)yyVals[-4 + yyTop]);
			current_type.AddMember(method);
			if (doc_support)
			{
				method.DocComment = Lexer.consume_doc_comment();
			}
			yyVal = method;
		}

		private void case_170()
		{
			if (lang_version < LanguageVersion.V_6)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[0 + yyTop]), "expression bodied members");
			}
			lexer.parsing_block++;
			start_block(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_171()
		{
			lexer.parsing_block = 0;
			current_block.AddStatement(new ContextualReturn((Expression)yyVals[-1 + yyTop]));
			Block block = end_block(GetLocation(yyVals[0 + yyTop]));
			block.IsCompilerGenerated = true;
			yyVal = block;
		}

		private void case_174()
		{
			List<Parameter> list = (List<Parameter>)yyVals[0 + yyTop];
			yyVal = new ParametersCompiled(list.ToArray());
		}

		private void case_175()
		{
			List<Parameter> list = (List<Parameter>)yyVals[-2 + yyTop];
			list.Add((Parameter)yyVals[0 + yyTop]);
			yyVal = new ParametersCompiled(list.ToArray());
		}

		private void case_176()
		{
			List<Parameter> list = (List<Parameter>)yyVals[-2 + yyTop];
			list.Add(new ArglistParameter(GetLocation(yyVals[0 + yyTop])));
			yyVal = new ParametersCompiled(list.ToArray(), true);
		}

		private void case_177()
		{
			if (yyVals[-2 + yyTop] != null)
			{
				report.Error(231, ((Parameter)yyVals[-2 + yyTop]).Location, "A params parameter must be the last parameter in a formal parameter list");
			}
			yyVal = new ParametersCompiled((Parameter)yyVals[-2 + yyTop]);
		}

		private void case_178()
		{
			if (yyVals[-2 + yyTop] != null)
			{
				report.Error(231, ((Parameter)yyVals[-2 + yyTop]).Location, "A params parameter must be the last parameter in a formal parameter list");
			}
			List<Parameter> list = (List<Parameter>)yyVals[-4 + yyTop];
			list.Add(new ArglistParameter(GetLocation(yyVals[-2 + yyTop])));
			yyVal = new ParametersCompiled(list.ToArray(), true);
		}

		private void case_179()
		{
			report.Error(257, GetLocation(yyVals[-2 + yyTop]), "An __arglist parameter must be the last parameter in a formal parameter list");
			yyVal = new ParametersCompiled(new Parameter[1]
			{
				new ArglistParameter(GetLocation(yyVals[-2 + yyTop]))
			}, true);
		}

		private void case_180()
		{
			report.Error(257, GetLocation(yyVals[-2 + yyTop]), "An __arglist parameter must be the last parameter in a formal parameter list");
			List<Parameter> list = (List<Parameter>)yyVals[-4 + yyTop];
			list.Add(new ArglistParameter(GetLocation(yyVals[-2 + yyTop])));
			yyVal = new ParametersCompiled(list.ToArray(), true);
		}

		private void case_183()
		{
			Error_SyntaxError(yyToken);
			yyVal = ParametersCompiled.EmptyReadOnlyParameters;
		}

		private void case_184()
		{
			parameters_bucket.Clear();
			Parameter parameter = (Parameter)yyVals[0 + yyTop];
			parameters_bucket.Add(parameter);
			default_parameter_used = parameter.HasDefaultValue;
			yyVal = parameters_bucket;
		}

		private void case_185()
		{
			List<Parameter> list = (List<Parameter>)yyVals[-2 + yyTop];
			Parameter parameter = (Parameter)yyVals[0 + yyTop];
			if (parameter != null)
			{
				if (parameter.HasExtensionMethodModifier)
				{
					report.Error(1100, parameter.Location, "The parameter modifier `this' can only be used on the first parameter");
				}
				else if (!parameter.HasDefaultValue && default_parameter_used)
				{
					report.Error(1737, parameter.Location, "Optional parameter cannot precede required parameters");
				}
				default_parameter_used |= parameter.HasDefaultValue;
				list.Add(parameter);
			}
			yyVal = yyVals[-2 + yyTop];
		}

		private void case_186()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			yyVal = new Parameter((FullNamedExpression)yyVals[-1 + yyTop], locatedToken.Value, (Parameter.Modifier)yyVals[-2 + yyTop], (Attributes)yyVals[-3 + yyTop], locatedToken.Location);
		}

		private void case_187()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			report.Error(1552, locatedToken.Location, "Array type specifier, [], must appear before parameter name");
			yyVal = new Parameter((FullNamedExpression)yyVals[-3 + yyTop], locatedToken.Value, (Parameter.Modifier)yyVals[-4 + yyTop], (Attributes)yyVals[-5 + yyTop], locatedToken.Location);
		}

		private void case_188()
		{
			Error_SyntaxError(yyToken);
			Location location = GetLocation(yyVals[0 + yyTop]);
			yyVal = new Parameter(null, null, Parameter.Modifier.NONE, (Attributes)yyVals[-1 + yyTop], location);
		}

		private void case_189()
		{
			Error_SyntaxError(yyToken);
			Location location = GetLocation(yyVals[0 + yyTop]);
			yyVal = new Parameter((FullNamedExpression)yyVals[-1 + yyTop], null, (Parameter.Modifier)yyVals[-2 + yyTop], (Attributes)yyVals[-3 + yyTop], location);
		}

		private void case_191()
		{
			lexer.parsing_block--;
			if (lang_version <= LanguageVersion.V_3)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-2 + yyTop]), "optional parameter");
			}
			Parameter.Modifier modifier = (Parameter.Modifier)yyVals[-5 + yyTop];
			if (modifier != Parameter.Modifier.NONE)
			{
				switch (modifier)
				{
				case Parameter.Modifier.REF:
				case Parameter.Modifier.OUT:
					report.Error(1741, GetLocation(yyVals[-5 + yyTop]), "Cannot specify a default value for the `{0}' parameter", Parameter.GetModifierSignature(modifier));
					break;
				case Parameter.Modifier.This:
					report.Error(1743, GetLocation(yyVals[-5 + yyTop]), "Cannot specify a default value for the `{0}' parameter", Parameter.GetModifierSignature(modifier));
					break;
				default:
					throw new NotImplementedException(modifier.ToString());
				}
				modifier = Parameter.Modifier.NONE;
			}
			if ((valid_param_mod & ParameterModifierType.DefaultValue) == 0)
			{
				report.Error(1065, GetLocation(yyVals[-2 + yyTop]), "Optional parameter is not valid in this context");
			}
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			yyVal = new Parameter((FullNamedExpression)yyVals[-4 + yyTop], locatedToken.Value, modifier, (Attributes)yyVals[-6 + yyTop], locatedToken.Location);
			if (yyVals[0 + yyTop] != null)
			{
				((Parameter)yyVal).DefaultValue = new DefaultParameterValueExpression((Expression)yyVals[0 + yyTop]);
			}
		}

		private void case_195()
		{
			Parameter.Modifier modifier = (Parameter.Modifier)yyVals[0 + yyTop];
			Parameter.Modifier modifier2 = (Parameter.Modifier)yyVals[-1 + yyTop] | modifier;
			if (((Parameter.Modifier)yyVals[-1 + yyTop] & modifier) == modifier)
			{
				Error_DuplicateParameterModifier(lexer.Location, modifier);
			}
			else
			{
				switch (modifier2 & ~Parameter.Modifier.This)
				{
				case Parameter.Modifier.REF:
					report.Error(1101, lexer.Location, "The parameter modifiers `this' and `ref' cannot be used altogether");
					break;
				case Parameter.Modifier.OUT:
					report.Error(1102, lexer.Location, "The parameter modifiers `this' and `out' cannot be used altogether");
					break;
				default:
					report.Error(1108, lexer.Location, "A parameter cannot have specified more than one modifier");
					break;
				}
			}
			yyVal = modifier2;
		}

		private void case_196()
		{
			if ((valid_param_mod & ParameterModifierType.Ref) == 0)
			{
				Error_ParameterModifierNotValid("ref", GetLocation(yyVals[0 + yyTop]));
			}
			yyVal = Parameter.Modifier.REF;
		}

		private void case_197()
		{
			if ((valid_param_mod & ParameterModifierType.Out) == 0)
			{
				Error_ParameterModifierNotValid("out", GetLocation(yyVals[0 + yyTop]));
			}
			yyVal = Parameter.Modifier.OUT;
		}

		private void case_198()
		{
			if ((valid_param_mod & ParameterModifierType.This) == 0)
			{
				Error_ParameterModifierNotValid("this", GetLocation(yyVals[0 + yyTop]));
			}
			if (lang_version <= LanguageVersion.ISO_2)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[0 + yyTop]), "extension methods");
			}
			yyVal = Parameter.Modifier.This;
		}

		private void case_199()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			yyVal = new ParamsParameter((FullNamedExpression)yyVals[-1 + yyTop], locatedToken.Value, (Attributes)yyVals[-3 + yyTop], locatedToken.Location);
		}

		private void case_200()
		{
			report.Error(1751, GetLocation(yyVals[-4 + yyTop]), "Cannot specify a default value for a parameter array");
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			yyVal = new ParamsParameter((FullNamedExpression)yyVals[-3 + yyTop], locatedToken.Value, (Attributes)yyVals[-5 + yyTop], locatedToken.Location);
		}

		private void case_201()
		{
			Error_SyntaxError(yyToken);
			yyVal = new ParamsParameter((FullNamedExpression)yyVals[-1 + yyTop], null, (Attributes)yyVals[-3 + yyTop], Location.Null);
		}

		private void case_202()
		{
			if ((valid_param_mod & ParameterModifierType.Params) == 0)
			{
				report.Error(1670, GetLocation(yyVals[0 + yyTop]), "The `params' modifier is not allowed in current context");
			}
		}

		private void case_203()
		{
			if (((Parameter.Modifier)yyVals[0 + yyTop] & Parameter.Modifier.This) != Parameter.Modifier.NONE)
			{
				report.Error(1104, GetLocation(yyVals[-1 + yyTop]), "The parameter modifiers `this' and `params' cannot be used altogether");
			}
			else
			{
				report.Error(1611, GetLocation(yyVals[-1 + yyTop]), "The params parameter cannot be declared as ref or out");
			}
		}

		private void case_205()
		{
			if ((valid_param_mod & ParameterModifierType.Arglist) == 0)
			{
				report.Error(1669, GetLocation(yyVals[0 + yyTop]), "__arglist is not valid in this context");
			}
		}

		private void case_206()
		{
			lexer.parsing_generic_declaration = false;
			if (doc_support)
			{
				tmpComment = Lexer.consume_doc_comment();
			}
		}

		private void case_207()
		{
			FullNamedExpression fullNamedExpression = (FullNamedExpression)yyVals[-3 + yyTop];
			current_property = new Property(current_type, fullNamedExpression, (Modifiers)yyVals[-4 + yyTop], (MemberName)yyVals[-2 + yyTop], (Attributes)yyVals[-5 + yyTop]);
			if (fullNamedExpression.Type != null && fullNamedExpression.Type.Kind == MemberKind.Void)
			{
				report.Error(547, GetLocation(yyVals[-3 + yyTop]), "`{0}': property or indexer cannot have void type", current_property.GetSignatureForError());
			}
			current_type.AddMember(current_property);
			lexer.PropertyParsing = true;
		}

		private void case_208()
		{
			lexer.PropertyParsing = false;
			if (doc_support)
			{
				current_property.DocComment = ConsumeStoredComment();
			}
		}

		private void case_209()
		{
			lexer.parsing_modifiers = true;
		}

		private void case_211()
		{
			lexer.parsing_generic_declaration = false;
			if (doc_support)
			{
				tmpComment = Lexer.consume_doc_comment();
			}
			current_local_parameters = ParametersCompiled.EmptyReadOnlyParameters;
		}

		private void case_212()
		{
			FullNamedExpression fullNamedExpression = (FullNamedExpression)yyVals[-3 + yyTop];
			Property property = new Property(current_type, fullNamedExpression, (Modifiers)yyVals[-4 + yyTop], (MemberName)yyVals[-2 + yyTop], (Attributes)yyVals[-5 + yyTop]);
			property.Get = new PropertyBase.GetMethod(property, Modifiers.COMPILER_GENERATED, null, property.Location);
			property.Get.Block = (ToplevelBlock)yyVals[0 + yyTop];
			if (current_container.Kind == MemberKind.Interface)
			{
				report.Error(531, property.Get.Block.StartLocation, "`{0}': interface members cannot have a definition", property.GetSignatureForError());
			}
			if (fullNamedExpression.Type != null && fullNamedExpression.Type.Kind == MemberKind.Void)
			{
				report.Error(547, GetLocation(yyVals[-3 + yyTop]), "`{0}': property or indexer cannot have void type", property.GetSignatureForError());
			}
			current_type.AddMember(property);
			current_local_parameters = null;
		}

		private void case_214()
		{
			lexer.parsing_block++;
			current_local_parameters = ParametersCompiled.EmptyReadOnlyParameters;
			start_block(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_215()
		{
			lexer.parsing_block--;
			((Property)current_property).Initializer = (Expression)yyVals[-1 + yyTop];
			end_block(GetLocation(yyVals[0 + yyTop]));
			current_local_parameters = null;
		}

		private void case_219()
		{
			valid_param_mod = (ParameterModifierType)0;
			FullNamedExpression fullNamedExpression = (FullNamedExpression)yyVals[-5 + yyTop];
			Indexer indexer = (Indexer)(current_property = new Indexer(current_type, fullNamedExpression, (MemberName)yyVals[-4 + yyTop], (Modifiers)yyVals[-6 + yyTop], (ParametersCompiled)yyVals[-1 + yyTop], (Attributes)yyVals[-7 + yyTop]));
			current_type.AddIndexer(indexer);
			if (fullNamedExpression.Type != null && fullNamedExpression.Type.Kind == MemberKind.Void)
			{
				report.Error(620, GetLocation(yyVals[-5 + yyTop]), "`{0}': indexer return type cannot be `void'", indexer.GetSignatureForError());
			}
			if (indexer.ParameterInfo.IsEmpty)
			{
				report.Error(1551, GetLocation(yyVals[-3 + yyTop]), "Indexers must have at least one parameter");
			}
			if (doc_support)
			{
				tmpComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			lexer.PropertyParsing = true;
			current_local_parameters = (ParametersCompiled)yyVals[-1 + yyTop];
		}

		private void case_220()
		{
			lexer.PropertyParsing = false;
			current_local_parameters = null;
			if (current_property.AccessorFirst != null && current_property.AccessorFirst.Block == null)
			{
				((Indexer)current_property).ParameterInfo.CheckParameters(current_property);
			}
			if (doc_support)
			{
				current_property.DocComment = ConsumeStoredComment();
			}
			current_property = null;
		}

		private void case_222()
		{
			current_property.Get = new Indexer.GetIndexerMethod(current_property, Modifiers.COMPILER_GENERATED, current_local_parameters, null, current_property.Location);
			current_property.Get.Block = (ToplevelBlock)yyVals[0 + yyTop];
		}

		private void case_227()
		{
			if (yyToken == 372)
			{
				report.Error(548, lexer.Location, "`{0}': property or indexer must have at least one accessor", current_property.GetSignatureForError());
			}
			else if (yyToken == 380)
			{
				report.Error(1597, lexer.Location, "Semicolon after method or accessor block is not valid");
			}
			else
			{
				report.Error(1014, GetLocation(yyVals[0 + yyTop]), "A get or set accessor expected");
			}
		}

		private void case_228()
		{
			if (yyVals[-1 + yyTop] != ModifierNone && lang_version == LanguageVersion.ISO_1)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-1 + yyTop]), "access modifiers on properties");
			}
			if (current_property.Get != null)
			{
				report.Error(1007, GetLocation(yyVals[0 + yyTop]), "Property accessor already defined");
			}
			if (current_property is Indexer)
			{
				current_property.Get = new Indexer.GetIndexerMethod(current_property, (Modifiers)yyVals[-1 + yyTop], ((Indexer)current_property).ParameterInfo.Clone(), (Attributes)yyVals[-2 + yyTop], GetLocation(yyVals[0 + yyTop]));
			}
			else
			{
				current_property.Get = new PropertyBase.GetMethod(current_property, (Modifiers)yyVals[-1 + yyTop], (Attributes)yyVals[-2 + yyTop], GetLocation(yyVals[0 + yyTop]));
			}
			current_local_parameters = current_property.Get.ParameterInfo;
			lexer.PropertyParsing = false;
		}

		private void case_229()
		{
			if (yyVals[0 + yyTop] != null)
			{
				current_property.Get.Block = (ToplevelBlock)yyVals[0 + yyTop];
				if (current_container.Kind == MemberKind.Interface)
				{
					report.Error(531, current_property.Get.Block.StartLocation, "`{0}': interface members cannot have a definition", current_property.Get.GetSignatureForError());
				}
			}
			current_local_parameters = null;
			lexer.PropertyParsing = true;
			if (doc_support && Lexer.doc_state == XmlCommentState.Error)
			{
				Lexer.doc_state = XmlCommentState.NotAllowed;
			}
		}

		private void case_230()
		{
			if (yyVals[-1 + yyTop] != ModifierNone && lang_version == LanguageVersion.ISO_1)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-1 + yyTop]), "access modifiers on properties");
			}
			if (current_property.Set != null)
			{
				report.Error(1007, GetLocation(yyVals[0 + yyTop]), "Property accessor already defined");
			}
			if (current_property is Indexer)
			{
				current_property.Set = new Indexer.SetIndexerMethod(current_property, (Modifiers)yyVals[-1 + yyTop], ParametersCompiled.MergeGenerated(compiler, ((Indexer)current_property).ParameterInfo, true, new Parameter(current_property.TypeExpression, "value", Parameter.Modifier.NONE, null, GetLocation(yyVals[0 + yyTop])), null), (Attributes)yyVals[-2 + yyTop], GetLocation(yyVals[0 + yyTop]));
			}
			else
			{
				current_property.Set = new PropertyBase.SetMethod(current_property, (Modifiers)yyVals[-1 + yyTop], ParametersCompiled.CreateImplicitParameter(current_property.TypeExpression, GetLocation(yyVals[0 + yyTop])), (Attributes)yyVals[-2 + yyTop], GetLocation(yyVals[0 + yyTop]));
			}
			current_local_parameters = current_property.Set.ParameterInfo;
			lexer.PropertyParsing = false;
		}

		private void case_231()
		{
			if (yyVals[0 + yyTop] != null)
			{
				current_property.Set.Block = (ToplevelBlock)yyVals[0 + yyTop];
				if (current_container.Kind == MemberKind.Interface)
				{
					report.Error(531, current_property.Set.Block.StartLocation, "`{0}': interface members cannot have a definition", current_property.Set.GetSignatureForError());
				}
			}
			current_local_parameters = null;
			lexer.PropertyParsing = true;
			if (doc_support && Lexer.doc_state == XmlCommentState.Error)
			{
				Lexer.doc_state = XmlCommentState.NotAllowed;
			}
		}

		private void case_233()
		{
			yyVal = null;
		}

		private void case_234()
		{
			Error_SyntaxError(1043, yyToken, "Invalid accessor body");
			yyVal = null;
		}

		private void case_236()
		{
			lexer.ConstraintsParsing = true;
			push_current_container(new Interface(current_container, (MemberName)yyVals[0 + yyTop], (Modifiers)yyVals[-4 + yyTop], (Attributes)yyVals[-5 + yyTop]), yyVals[-3 + yyTop]);
		}

		private void case_237()
		{
			lexer.ConstraintsParsing = false;
			if (yyVals[0 + yyTop] != null)
			{
				current_container.SetConstraints((List<Constraints>)yyVals[0 + yyTop]);
			}
			if (doc_support)
			{
				current_container.PartialContainer.DocComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			lexer.parsing_modifiers = true;
		}

		private void case_238()
		{
			lexer.parsing_declaration--;
			if (doc_support)
			{
				Lexer.doc_state = XmlCommentState.Allowed;
			}
		}

		private void case_239()
		{
			object obj = yyVals[0 + yyTop];
			yyVal = pop_current_class();
		}

		private void case_243()
		{
			lexer.parsing_modifiers = true;
			lexer.parsing_block = 0;
		}

		private void case_244()
		{
			lexer.parsing_modifiers = true;
			lexer.parsing_block = 0;
		}

		private void case_255()
		{
			OperatorDeclaration operatorDeclaration = (OperatorDeclaration)yyVals[-2 + yyTop];
			if (operatorDeclaration != null)
			{
				Operator obj = new Operator(current_type, operatorDeclaration.optype, operatorDeclaration.ret_type, (Modifiers)yyVals[-3 + yyTop], current_local_parameters, (ToplevelBlock)yyVals[0 + yyTop], (Attributes)yyVals[-4 + yyTop], operatorDeclaration.location);
				if (obj.Block == null)
				{
					obj.ParameterInfo.CheckParameters(obj);
				}
				if (doc_support)
				{
					obj.DocComment = tmpComment;
					Lexer.doc_state = XmlCommentState.Allowed;
				}
				current_type.AddOperator(obj);
			}
			current_local_parameters = null;
		}

		private void case_257()
		{
			report.Error(590, GetLocation(yyVals[0 + yyTop]), "User-defined operators cannot return void");
			yyVal = new TypeExpression(compiler.BuiltinTypes.Void, GetLocation(yyVals[0 + yyTop]));
		}

		private void case_258()
		{
			valid_param_mod = ParameterModifierType.DefaultValue;
			if ((Operator.OpType)yyVals[-1 + yyTop] == Operator.OpType.Is)
			{
				valid_param_mod |= ParameterModifierType.Out;
			}
		}

		private void case_259()
		{
			valid_param_mod = (ParameterModifierType)0;
			Location location = GetLocation(yyVals[-5 + yyTop]);
			Operator.OpType opType = (Operator.OpType)yyVals[-4 + yyTop];
			current_local_parameters = (ParametersCompiled)yyVals[-1 + yyTop];
			int count = current_local_parameters.Count;
			if (count == 1)
			{
				switch (opType)
				{
				case Operator.OpType.Addition:
					opType = Operator.OpType.UnaryPlus;
					break;
				case Operator.OpType.Subtraction:
					opType = Operator.OpType.UnaryNegation;
					break;
				}
			}
			if (IsUnaryOperator(opType))
			{
				switch (count)
				{
				case 2:
					report.Error(1020, location, "Overloadable binary operator expected");
					break;
				default:
					report.Error(1535, location, "Overloaded unary operator `{0}' takes one parameter", Operator.GetName(opType));
					break;
				case 1:
					break;
				}
			}
			else if (opType != Operator.OpType.Is)
			{
				switch (count)
				{
				case 1:
					report.Error(1019, location, "Overloadable unary operator expected");
					break;
				default:
					report.Error(1534, location, "Overloaded binary operator `{0}' takes two parameters", Operator.GetName(opType));
					break;
				case 2:
					break;
				}
			}
			if (doc_support)
			{
				tmpComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.NotAllowed;
			}
			yyVal = new OperatorDeclaration(opType, (FullNamedExpression)yyVals[-6 + yyTop], location);
		}

		private void case_283()
		{
			if (lang_version != LanguageVersion.Experimental)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[0 + yyTop]), "is user operator");
			}
			yyVal = Operator.OpType.Is;
		}

		private void case_285()
		{
			valid_param_mod = (ParameterModifierType)0;
			Location location = GetLocation(yyVals[-5 + yyTop]);
			current_local_parameters = (ParametersCompiled)yyVals[-1 + yyTop];
			if (current_local_parameters.Count != 1)
			{
				report.Error(1535, location, "Overloaded unary operator `implicit' takes one parameter");
			}
			if (doc_support)
			{
				tmpComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.NotAllowed;
			}
			yyVal = new OperatorDeclaration(Operator.OpType.Implicit, (FullNamedExpression)yyVals[-4 + yyTop], location);
		}

		private void case_287()
		{
			valid_param_mod = (ParameterModifierType)0;
			Location location = GetLocation(yyVals[-5 + yyTop]);
			current_local_parameters = (ParametersCompiled)yyVals[-1 + yyTop];
			if (current_local_parameters.Count != 1)
			{
				report.Error(1535, location, "Overloaded unary operator `explicit' takes one parameter");
			}
			if (doc_support)
			{
				tmpComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.NotAllowed;
			}
			yyVal = new OperatorDeclaration(Operator.OpType.Explicit, (FullNamedExpression)yyVals[-4 + yyTop], location);
		}

		private void case_288()
		{
			Error_SyntaxError(yyToken);
			current_local_parameters = ParametersCompiled.EmptyReadOnlyParameters;
			yyVal = new OperatorDeclaration(Operator.OpType.Implicit, null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_289()
		{
			Error_SyntaxError(yyToken);
			current_local_parameters = ParametersCompiled.EmptyReadOnlyParameters;
			yyVal = new OperatorDeclaration(Operator.OpType.Explicit, null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_290()
		{
			Constructor constructor = (Constructor)yyVals[-1 + yyTop];
			constructor.Block = (ToplevelBlock)yyVals[0 + yyTop];
			if (doc_support)
			{
				constructor.DocComment = ConsumeStoredComment();
			}
			current_local_parameters = null;
			if (doc_support)
			{
				Lexer.doc_state = XmlCommentState.Allowed;
			}
		}

		private void case_291()
		{
			if (doc_support)
			{
				tmpComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			valid_param_mod = ParameterModifierType.All;
		}

		private void case_292()
		{
			valid_param_mod = (ParameterModifierType)0;
			current_local_parameters = (ParametersCompiled)yyVals[-1 + yyTop];
			LocatedToken locatedToken = (LocatedToken)yyVals[-4 + yyTop];
			Modifiers modifiers = (Modifiers)yyVals[-5 + yyTop];
			Constructor constructor = new Constructor(current_type, locatedToken.Value, modifiers, (Attributes)yyVals[-6 + yyTop], current_local_parameters, locatedToken.Location);
			if (locatedToken.Value != current_container.MemberName.Name)
			{
				report.Error(1520, constructor.Location, "Class, struct, or interface method must have a return type");
			}
			else if ((modifiers & Modifiers.STATIC) != 0)
			{
				if (!current_local_parameters.IsEmpty)
				{
					report.Error(132, constructor.Location, "`{0}': The static constructor must be parameterless", constructor.GetSignatureForError());
				}
				if ((modifiers & Modifiers.AccessibilityMask) != 0)
				{
					report.Error(515, constructor.Location, "`{0}': static constructor cannot have an access modifier", constructor.GetSignatureForError());
				}
			}
			else if (current_type.Kind == MemberKind.Struct && current_local_parameters.IsEmpty)
			{
				if (lang_version < LanguageVersion.V_6)
				{
					FeatureIsNotAvailable(GetLocation(yyVals[-4 + yyTop]), "struct parameterless instance constructor");
				}
				if ((modifiers & Modifiers.PUBLIC) == 0)
				{
					report.Error(8075, constructor.Location, "`{0}': Structs parameterless instance constructor must be public", constructor.GetSignatureForError());
				}
			}
			current_type.AddConstructor(constructor);
			yyVal = constructor;
			start_block(lexer.Location);
		}

		private void case_293()
		{
			if (yyVals[0 + yyTop] != null)
			{
				Constructor constructor = (Constructor)yyVals[-1 + yyTop];
				constructor.Initializer = (ConstructorInitializer)yyVals[0 + yyTop];
				if (constructor.IsStatic)
				{
					report.Error(514, constructor.Location, "`{0}': static constructor cannot have an explicit `this' or `base' constructor call", constructor.GetSignatureForError());
				}
			}
			yyVal = yyVals[-1 + yyTop];
		}

		private void case_299()
		{
			lexer.parsing_block--;
			yyVal = new ConstructorBaseInitializer((Arguments)yyVals[-1 + yyTop], GetLocation(yyVals[-4 + yyTop]));
		}

		private void case_301()
		{
			lexer.parsing_block--;
			yyVal = new ConstructorThisInitializer((Arguments)yyVals[-1 + yyTop], GetLocation(yyVals[-4 + yyTop]));
		}

		private void case_302()
		{
			Error_SyntaxError(yyToken);
			yyVal = new ConstructorThisInitializer(null, GetLocation(yyVals[0 + yyTop]));
		}

		private void case_303()
		{
			Error_SyntaxError(yyToken);
			yyVal = null;
		}

		private void case_304()
		{
			if (doc_support)
			{
				tmpComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.NotAllowed;
			}
			current_local_parameters = ParametersCompiled.EmptyReadOnlyParameters;
		}

		private void case_305()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			if (locatedToken.Value != current_container.MemberName.Name)
			{
				report.Error(574, locatedToken.Location, "Name of destructor must match name of class");
			}
			else if (current_container.Kind != MemberKind.Class)
			{
				report.Error(575, locatedToken.Location, "Only class types can contain destructor");
			}
			Destructor destructor = new Destructor(current_type, (Modifiers)yyVals[-6 + yyTop], ParametersCompiled.EmptyReadOnlyParameters, (Attributes)yyVals[-7 + yyTop], locatedToken.Location);
			if (doc_support)
			{
				destructor.DocComment = ConsumeStoredComment();
			}
			destructor.Block = (ToplevelBlock)yyVals[0 + yyTop];
			current_type.AddMember(destructor);
			current_local_parameters = null;
		}

		private void case_306()
		{
			current_event_field = new EventField(current_type, (FullNamedExpression)yyVals[-1 + yyTop], (Modifiers)yyVals[-3 + yyTop], (MemberName)yyVals[0 + yyTop], (Attributes)yyVals[-4 + yyTop]);
			current_type.AddMember(current_event_field);
			if (current_event_field.MemberName.ExplicitInterface != null)
			{
				report.Error(71, current_event_field.Location, "`{0}': An explicit interface implementation of an event must use property syntax", current_event_field.GetSignatureForError());
			}
			yyVal = current_event_field;
		}

		private void case_307()
		{
			if (doc_support)
			{
				current_event_field.DocComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			current_event_field = null;
		}

		private void case_308()
		{
			current_event = new EventProperty(current_type, (FullNamedExpression)yyVals[-2 + yyTop], (Modifiers)yyVals[-4 + yyTop], (MemberName)yyVals[-1 + yyTop], (Attributes)yyVals[-5 + yyTop]);
			current_type.AddMember(current_event);
			lexer.EventParsing = true;
		}

		private void case_309()
		{
			if (current_container.Kind == MemberKind.Interface)
			{
				report.Error(69, GetLocation(yyVals[-2 + yyTop]), "Event in interface cannot have add or remove accessors");
			}
			lexer.EventParsing = false;
		}

		private void case_310()
		{
			if (doc_support)
			{
				current_event.DocComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			current_event = null;
			current_local_parameters = null;
		}

		private void case_311()
		{
			Error_SyntaxError(yyToken);
			current_type.AddMember(new EventField(current_type, (FullNamedExpression)yyVals[-1 + yyTop], (Modifiers)yyVals[-3 + yyTop], MemberName.Null, (Attributes)yyVals[-4 + yyTop]));
		}

		private void case_314()
		{
			lexer.parsing_block--;
			current_event_field.Initializer = (Expression)yyVals[0 + yyTop];
		}

		private void case_319()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			yyVal = new FieldDeclarator(new SimpleMemberName(locatedToken.Value, locatedToken.Location), null);
		}

		private void case_321()
		{
			lexer.parsing_block--;
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			yyVal = new FieldDeclarator(new SimpleMemberName(locatedToken.Value, locatedToken.Location), (Expression)yyVals[0 + yyTop]);
		}

		private void case_322()
		{
			if (current_container.Kind == MemberKind.Interface)
			{
				report.Error(68, lexer.Location, "`{0}': event in interface cannot have an initializer", current_event_field.GetSignatureForError());
			}
			if ((current_event_field.ModFlags & Modifiers.ABSTRACT) != 0)
			{
				report.Error(74, lexer.Location, "`{0}': abstract event cannot have an initializer", current_event_field.GetSignatureForError());
			}
		}

		private void case_326()
		{
			report.Error(65, lexer.Location, "`{0}': event property must have both add and remove accessors", current_event.GetSignatureForError());
		}

		private void case_327()
		{
			report.Error(65, lexer.Location, "`{0}': event property must have both add and remove accessors", current_event.GetSignatureForError());
		}

		private void case_328()
		{
			report.Error(1055, GetLocation(yyVals[0 + yyTop]), "An add or remove accessor expected");
			yyVal = null;
		}

		private void case_329()
		{
			if (yyVals[-1 + yyTop] != ModifierNone)
			{
				report.Error(1609, GetLocation(yyVals[-1 + yyTop]), "Modifiers cannot be placed on event accessor declarations");
			}
			current_event.Add = new EventProperty.AddDelegateMethod(current_event, (Attributes)yyVals[-2 + yyTop], GetLocation(yyVals[0 + yyTop]));
			current_local_parameters = current_event.Add.ParameterInfo;
			lexer.EventParsing = false;
		}

		private void case_330()
		{
			lexer.EventParsing = true;
			current_event.Add.Block = (ToplevelBlock)yyVals[0 + yyTop];
			if (current_container.Kind == MemberKind.Interface)
			{
				report.Error(531, current_event.Add.Block.StartLocation, "`{0}': interface members cannot have a definition", current_event.Add.GetSignatureForError());
			}
			current_local_parameters = null;
		}

		private void case_331()
		{
			if (yyVals[-1 + yyTop] != ModifierNone)
			{
				report.Error(1609, GetLocation(yyVals[-1 + yyTop]), "Modifiers cannot be placed on event accessor declarations");
			}
			current_event.Remove = new EventProperty.RemoveDelegateMethod(current_event, (Attributes)yyVals[-2 + yyTop], GetLocation(yyVals[0 + yyTop]));
			current_local_parameters = current_event.Remove.ParameterInfo;
			lexer.EventParsing = false;
		}

		private void case_332()
		{
			lexer.EventParsing = true;
			current_event.Remove.Block = (ToplevelBlock)yyVals[0 + yyTop];
			if (current_container.Kind == MemberKind.Interface)
			{
				report.Error(531, current_event.Remove.Block.StartLocation, "`{0}': interface members cannot have a definition", current_event.Remove.GetSignatureForError());
			}
			current_local_parameters = null;
		}

		private void case_333()
		{
			report.Error(73, lexer.Location, "An add or remove accessor must have a body");
			yyVal = null;
		}

		private void case_335()
		{
			current_type.UnattachedAttributes = (Attributes)yyVals[-1 + yyTop];
			report.Error(1519, GetLocation(yyVals[-1 + yyTop]), "An attribute is missing member declaration");
			lexer.putback(125);
		}

		private void case_336()
		{
			report.Error(1519, lexer.Location, "Unexpected symbol `}' in class, struct, or interface member declaration");
			lexer.putback(125);
			lexer.parsing_generic_declaration = false;
			FullNamedExpression type = (FullNamedExpression)yyVals[-1 + yyTop];
			current_field = new Field(current_type, type, (Modifiers)yyVals[-2 + yyTop], MemberName.Null, (Attributes)yyVals[-3 + yyTop]);
			current_type.AddField(current_field);
			yyVal = current_field;
		}

		private void case_337()
		{
			if (doc_support)
			{
				enumTypeComment = Lexer.consume_doc_comment();
			}
		}

		private void case_338()
		{
			if (doc_support)
			{
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			MemberName memberName = (MemberName)yyVals[-3 + yyTop];
			if (memberName.IsGeneric)
			{
				report.Error(1675, memberName.Location, "Enums cannot have type parameters");
			}
			push_current_container(new Enum(current_container, (FullNamedExpression)yyVals[-2 + yyTop], (Modifiers)yyVals[-5 + yyTop], memberName, (Attributes)yyVals[-6 + yyTop]), null);
		}

		private void case_339()
		{
			lexer.parsing_modifiers = true;
			if (doc_support)
			{
				Lexer.doc_state = XmlCommentState.Allowed;
			}
		}

		private void case_340()
		{
			if (doc_support)
			{
				current_container.DocComment = enumTypeComment;
			}
			lexer.parsing_declaration--;
			yyVal = pop_current_class();
		}

		private void case_343()
		{
			Error_TypeExpected(GetLocation(yyVals[-1 + yyTop]));
			yyVal = null;
		}

		private void case_348()
		{
			yyVal = yyVals[0 + yyTop];
		}

		private void case_349()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			EnumMember enumMember = new EnumMember((Enum)current_type, new MemberName(locatedToken.Value, locatedToken.Location), (Attributes)yyVals[-1 + yyTop]);
			((Enum)current_type).AddEnumMember(enumMember);
			if (doc_support)
			{
				enumMember.DocComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			yyVal = enumMember;
		}

		private void case_350()
		{
			lexer.parsing_block++;
			if (doc_support)
			{
				tmpComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.NotAllowed;
			}
		}

		private void case_351()
		{
			lexer.parsing_block--;
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			EnumMember enumMember = new EnumMember((Enum)current_type, new MemberName(locatedToken.Value, locatedToken.Location), (Attributes)yyVals[-4 + yyTop]);
			enumMember.Initializer = new ConstInitializer(enumMember, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
			((Enum)current_type).AddEnumMember(enumMember);
			if (doc_support)
			{
				enumMember.DocComment = ConsumeStoredComment();
			}
			yyVal = enumMember;
		}

		private void case_352()
		{
			Error_SyntaxError(yyToken);
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			EnumMember enumMember = new EnumMember((Enum)current_type, new MemberName(locatedToken.Value, locatedToken.Location), (Attributes)yyVals[-2 + yyTop]);
			((Enum)current_type).AddEnumMember(enumMember);
			if (doc_support)
			{
				enumMember.DocComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			yyVal = enumMember;
		}

		private void case_355()
		{
			valid_param_mod = (ParameterModifierType)0;
			ParametersCompiled parametersCompiled = (ParametersCompiled)yyVals[-1 + yyTop];
			Delegate obj = new Delegate(current_container, (FullNamedExpression)yyVals[-5 + yyTop], (Modifiers)yyVals[-7 + yyTop], (MemberName)yyVals[-4 + yyTop], parametersCompiled, (Attributes)yyVals[-8 + yyTop]);
			parametersCompiled.CheckParameters(obj);
			current_container.AddTypeContainer(obj);
			current_delegate = obj;
			lexer.ConstraintsParsing = true;
		}

		private void case_357()
		{
			if (doc_support)
			{
				current_delegate.DocComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			if (yyVals[-2 + yyTop] != null)
			{
				current_delegate.SetConstraints((List<Constraints>)yyVals[-2 + yyTop]);
			}
			yyVal = current_delegate;
			current_delegate = null;
		}

		private void case_359()
		{
			if (lang_version < LanguageVersion.ISO_2)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[0 + yyTop]), "nullable types");
			}
			yyVal = ComposedTypeSpecifier.CreateNullable(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_361()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			LocatedToken locatedToken2 = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new QualifiedAliasMember(locatedToken.Value, locatedToken2.Value, (TypeArguments)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_362()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			LocatedToken locatedToken2 = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new QualifiedAliasMember(locatedToken.Value, locatedToken2.Value, (int)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_364()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new MemberAccess((Expression)yyVals[-3 + yyTop], locatedToken.Value, (TypeArguments)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_365()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new MemberAccess((Expression)yyVals[-3 + yyTop], locatedToken.Value, (int)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_366()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new SimpleName(locatedToken.Value, (TypeArguments)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_367()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new SimpleName(locatedToken.Value, (int)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_369()
		{
			if (lang_version < LanguageVersion.ISO_2)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-2 + yyTop]), "generics");
			}
			yyVal = yyVals[-1 + yyTop];
		}

		private void case_370()
		{
			Error_TypeExpected(lexer.Location);
			yyVal = new TypeArguments();
		}

		private void case_371()
		{
			TypeArguments typeArguments = new TypeArguments();
			typeArguments.Add((FullNamedExpression)yyVals[0 + yyTop]);
			yyVal = typeArguments;
		}

		private void case_372()
		{
			TypeArguments typeArguments = (TypeArguments)yyVals[-2 + yyTop];
			typeArguments.Add((FullNamedExpression)yyVals[0 + yyTop]);
			yyVal = typeArguments;
		}

		private void case_374()
		{
			lexer.parsing_generic_declaration = false;
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			yyVal = new MemberName(locatedToken.Value, (TypeParameters)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_375()
		{
			MemberName memberName = (MemberName)yyVals[0 + yyTop];
			if (memberName.TypeParameters != null)
			{
				syntax_error(memberName.Location, $"Member `{memberName.GetSignatureForError()}' cannot declare type arguments");
			}
		}

		private void case_377()
		{
			lexer.parsing_generic_declaration = false;
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new MemberName(locatedToken.Value, (TypeParameters)yyVals[0 + yyTop], (ATypeNameExpression)yyVals[-2 + yyTop], locatedToken.Location);
		}

		private void case_378()
		{
			lexer.parsing_generic_declaration = false;
			yyVal = new MemberName("Item", GetLocation(yyVals[0 + yyTop]));
		}

		private void case_379()
		{
			lexer.parsing_generic_declaration = false;
			yyVal = new MemberName("Item", null, (ATypeNameExpression)yyVals[-1 + yyTop], GetLocation(yyVals[0 + yyTop]));
		}

		private void case_380()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			yyVal = new SimpleName(locatedToken.Value, (TypeArguments)yyVals[-1 + yyTop], locatedToken.Location);
		}

		private void case_381()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			LocatedToken locatedToken2 = (LocatedToken)yyVals[-2 + yyTop];
			yyVal = new QualifiedAliasMember(locatedToken.Value, locatedToken2.Value, (TypeArguments)yyVals[-1 + yyTop], locatedToken.Location);
		}

		private void case_382()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			yyVal = new MemberAccess((ATypeNameExpression)yyVals[-3 + yyTop], locatedToken.Value, (TypeArguments)yyVals[-1 + yyTop], locatedToken.Location);
		}

		private void case_384()
		{
			if (lang_version < LanguageVersion.ISO_2)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-2 + yyTop]), "generics");
			}
			yyVal = yyVals[-1 + yyTop];
		}

		private void case_385()
		{
			TypeParameters typeParameters = new TypeParameters();
			typeParameters.Add((TypeParameter)yyVals[0 + yyTop]);
			yyVal = typeParameters;
		}

		private void case_386()
		{
			TypeParameters typeParameters = (TypeParameters)yyVals[-2 + yyTop];
			typeParameters.Add((TypeParameter)yyVals[0 + yyTop]);
			yyVal = typeParameters;
		}

		private void case_387()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			yyVal = new TypeParameter(new MemberName(locatedToken.Value, locatedToken.Location), (Attributes)yyVals[-2 + yyTop], (VarianceDecl)yyVals[-1 + yyTop]);
		}

		private void case_388()
		{
			if (GetTokenName(yyToken) == "type")
			{
				report.Error(81, GetLocation(yyVals[0 + yyTop]), "Type parameter declaration must be an identifier not a type");
			}
			else
			{
				Error_SyntaxError(yyToken);
			}
			yyVal = new TypeParameter(MemberName.Null, null, null);
		}

		private void case_397()
		{
			report.Error(1536, GetLocation(yyVals[0 + yyTop]), "Invalid parameter type `void'");
			yyVal = new TypeExpression(compiler.BuiltinTypes.Void, GetLocation(yyVals[0 + yyTop]));
		}

		private void case_400()
		{
			if (yyVals[0 + yyTop] != null)
			{
				yyVal = new ComposedCast((ATypeNameExpression)yyVals[-1 + yyTop], (ComposedTypeSpecifier)yyVals[0 + yyTop]);
				return;
			}
			SimpleName simpleName = yyVals[-1 + yyTop] as SimpleName;
			if (simpleName != null && simpleName.Name == "var")
			{
				yyVal = new VarExpr(simpleName.Location);
			}
			else
			{
				yyVal = yyVals[-1 + yyTop];
			}
		}

		private void case_403()
		{
			Expression.Error_VoidInvalidInTheContext(GetLocation(yyVals[0 + yyTop]), report);
			yyVal = new TypeExpression(compiler.BuiltinTypes.Void, GetLocation(yyVals[0 + yyTop]));
		}

		private void case_404()
		{
			if (yyVals[0 + yyTop] != null)
			{
				yyVal = new ComposedCast((FullNamedExpression)yyVals[-1 + yyTop], (ComposedTypeSpecifier)yyVals[0 + yyTop]);
			}
		}

		private void case_407()
		{
			List<FullNamedExpression> list = new List<FullNamedExpression>(2);
			list.Add((FullNamedExpression)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_408()
		{
			List<FullNamedExpression> list = (List<FullNamedExpression>)yyVals[-2 + yyTop];
			list.Add((FullNamedExpression)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_409()
		{
			if (yyVals[0 + yyTop] is ComposedCast)
			{
				report.Error(1521, GetLocation(yyVals[0 + yyTop]), "Invalid base type `{0}'", ((ComposedCast)yyVals[0 + yyTop]).GetSignatureForError());
			}
			yyVal = yyVals[0 + yyTop];
		}

		private void case_448()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new CompletionSimpleName(MemberName.MakeName(locatedToken.Value, null), locatedToken.Location);
		}

		private void case_457()
		{
			List<Expression> list = new List<Expression>();
			list.Add((InterpolatedStringInsert)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_458()
		{
			List<Expression> list = (List<Expression>)yyVals[-2 + yyTop];
			list.Add((StringLiteral)yyVals[-1 + yyTop]);
			list.Add((InterpolatedStringInsert)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_460()
		{
			yyVal = new InterpolatedStringInsert((Expression)yyVals[-2 + yyTop])
			{
				Alignment = (Expression)yyVals[0 + yyTop]
			};
		}

		private void case_462()
		{
			lexer.parsing_interpolation_format = false;
			yyVal = new InterpolatedStringInsert((Expression)yyVals[-3 + yyTop])
			{
				Format = (string)yyVals[0 + yyTop]
			};
		}

		private void case_464()
		{
			lexer.parsing_interpolation_format = false;
			yyVal = new InterpolatedStringInsert((Expression)yyVals[-5 + yyTop])
			{
				Alignment = (Expression)yyVals[-3 + yyTop],
				Format = (string)yyVals[0 + yyTop]
			};
		}

		private void case_469()
		{
			yyVal = new ParenthesizedExpression((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_471()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new MemberAccess((Expression)yyVals[-3 + yyTop], locatedToken.Value, (TypeArguments)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_472()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new MemberAccess((Expression)yyVals[-3 + yyTop], locatedToken.Value, (int)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_473()
		{
			if (lang_version < LanguageVersion.V_6)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-3 + yyTop]), "null propagating operator");
			}
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new ConditionalMemberAccess((Expression)yyVals[-4 + yyTop], locatedToken.Value, (TypeArguments)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_474()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new MemberAccess((Expression)yyVals[-3 + yyTop], locatedToken.Value, (TypeArguments)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_475()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new MemberAccess(new BaseThis(GetLocation(yyVals[-3 + yyTop])), locatedToken.Value, (TypeArguments)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_476()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new MemberAccess(new SimpleName("await", ((LocatedToken)yyVals[-3 + yyTop]).Location), locatedToken.Value, (TypeArguments)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_477()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			LocatedToken locatedToken2 = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new QualifiedAliasMember(locatedToken.Value, locatedToken2.Value, (TypeArguments)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_478()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			LocatedToken locatedToken2 = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new QualifiedAliasMember(locatedToken.Value, locatedToken2.Value, (int)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_480()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new CompletionMemberAccess((Expression)yyVals[-3 + yyTop], locatedToken.Value, locatedToken.Location);
		}

		private void case_482()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new CompletionMemberAccess((Expression)yyVals[-3 + yyTop], locatedToken.Value, locatedToken.Location);
		}

		private void case_483()
		{
			yyVal = new Invocation((Expression)yyVals[-3 + yyTop], (Arguments)yyVals[-1 + yyTop]);
		}

		private void case_484()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Invocation((Expression)yyVals[-3 + yyTop], (Arguments)yyVals[-1 + yyTop]);
		}

		private void case_485()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Invocation((Expression)yyVals[-2 + yyTop], null);
		}

		private void case_488()
		{
			if (yyVals[-1 + yyTop] == null)
			{
				yyVal = new CollectionOrObjectInitializers(GetLocation(yyVals[-2 + yyTop]));
			}
			else
			{
				yyVal = new CollectionOrObjectInitializers((List<Expression>)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
			}
		}

		private void case_489()
		{
			yyVal = new CollectionOrObjectInitializers((List<Expression>)yyVals[-2 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_492()
		{
			List<Expression> list = new List<Expression>();
			list.Add((Expression)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_493()
		{
			List<Expression> list = (List<Expression>)yyVals[-2 + yyTop];
			list.Add((Expression)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_494()
		{
			Error_SyntaxError(yyToken);
			yyVal = yyVals[-1 + yyTop];
		}

		private void case_495()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			yyVal = new ElementInitializer(locatedToken.Value, (Expression)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_496()
		{
			LocatedToken locatedToken = (LocatedToken)Error_AwaitAsIdentifier(yyVals[-2 + yyTop]);
			yyVal = new ElementInitializer(locatedToken.Value, (Expression)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_498()
		{
			CompletionSimpleName completionSimpleName = yyVals[-1 + yyTop] as CompletionSimpleName;
			if (completionSimpleName == null)
			{
				yyVal = new CollectionElementInitializer((Expression)yyVals[-1 + yyTop]);
			}
			else
			{
				yyVal = new CompletionElementInitializer(completionSimpleName.Prefix, completionSimpleName.Location);
			}
		}

		private void case_499()
		{
			if (yyVals[-1 + yyTop] == null)
			{
				yyVal = new CollectionElementInitializer(GetLocation(yyVals[-2 + yyTop]));
			}
			else
			{
				yyVal = new CollectionElementInitializer((List<Expression>)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
			}
		}

		private void case_500()
		{
			if (lang_version < LanguageVersion.V_6)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-4 + yyTop]), "dictionary initializer");
			}
			yyVal = new DictionaryElementInitializer((Arguments)yyVals[-3 + yyTop], (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-4 + yyTop]));
		}

		private void case_501()
		{
			report.Error(1920, GetLocation(yyVals[-1 + yyTop]), "An element initializer cannot be empty");
			yyVal = new CollectionElementInitializer(GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_506()
		{
			Arguments arguments = new Arguments(4);
			arguments.Add((Argument)yyVals[0 + yyTop]);
			yyVal = arguments;
		}

		private void case_507()
		{
			Arguments arguments = (Arguments)yyVals[-2 + yyTop];
			if (arguments[arguments.Count - 1] is NamedArgument)
			{
				Error_NamedArgumentExpected((NamedArgument)arguments[arguments.Count - 1]);
			}
			arguments.Add((Argument)yyVals[0 + yyTop]);
			yyVal = arguments;
		}

		private void case_508()
		{
			Arguments arguments = (Arguments)yyVals[-2 + yyTop];
			NamedArgument namedArgument = (NamedArgument)yyVals[0 + yyTop];
			for (int i = 0; i < arguments.Count; i++)
			{
				NamedArgument namedArgument2 = arguments[i] as NamedArgument;
				if (namedArgument2 != null && namedArgument2.Name == namedArgument.Name)
				{
					report.Error(1740, namedArgument2.Location, "Named argument `{0}' specified multiple times", namedArgument2.Name);
				}
			}
			arguments.Add(namedArgument);
			yyVal = arguments;
		}

		private void case_509()
		{
			if (lexer.putback_char == -1)
			{
				lexer.putback(41);
			}
			Error_SyntaxError(yyToken);
			yyVal = yyVals[-2 + yyTop];
		}

		private void case_510()
		{
			report.Error(839, GetLocation(yyVals[-1 + yyTop]), "An argument is missing");
			yyVal = null;
		}

		private void case_515()
		{
			yyVal = new Argument((Expression)yyVals[0 + yyTop], Argument.AType.Ref);
		}

		private void case_517()
		{
			yyVal = new Argument((Expression)yyVals[0 + yyTop], Argument.AType.Out);
		}

		private void case_519()
		{
			yyVal = new Argument(new Arglist((Arguments)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop])));
		}

		private void case_520()
		{
			yyVal = new Argument(new Arglist(GetLocation(yyVals[-2 + yyTop])));
		}

		private void case_521()
		{
			yyVal = new ParenthesizedExpression((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_522()
		{
			if (lang_version != LanguageVersion.Experimental)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-1 + yyTop]), "declaration expression");
			}
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			LocalVariable localVariable = new LocalVariable(current_block, locatedToken.Value, locatedToken.Location);
			current_block.AddLocalName(localVariable);
			yyVal = new DeclarationExpression((FullNamedExpression)yyVals[-1 + yyTop], localVariable);
		}

		private void case_523()
		{
			if (lang_version != LanguageVersion.Experimental)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-3 + yyTop]), "declaration expression");
			}
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			LocalVariable localVariable = new LocalVariable(current_block, locatedToken.Value, locatedToken.Location);
			current_block.AddLocalName(localVariable);
			yyVal = new DeclarationExpression((FullNamedExpression)yyVals[-3 + yyTop], localVariable)
			{
				Initializer = (Expression)yyVals[0 + yyTop]
			};
		}

		private void case_525()
		{
			yyVal = new ElementAccess((Expression)yyVals[-3 + yyTop], (Arguments)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_526()
		{
			if (lang_version < LanguageVersion.V_6)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-3 + yyTop]), "null propagating operator");
			}
			yyVal = new ElementAccess((Expression)yyVals[-4 + yyTop], (Arguments)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]))
			{
				ConditionalAccess = true
			};
		}

		private void case_527()
		{
			Error_SyntaxError(yyToken);
			yyVal = new ElementAccess((Expression)yyVals[-3 + yyTop], (Arguments)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_528()
		{
			Error_SyntaxError(yyToken);
			yyVal = new ElementAccess((Expression)yyVals[-2 + yyTop], null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_529()
		{
			List<Expression> list = new List<Expression>(4);
			list.Add((Expression)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_530()
		{
			List<Expression> list = (List<Expression>)yyVals[-2 + yyTop];
			list.Add((Expression)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_531()
		{
			Arguments arguments = new Arguments(4);
			arguments.Add((Argument)yyVals[0 + yyTop]);
			yyVal = arguments;
		}

		private void case_532()
		{
			Arguments arguments = (Arguments)yyVals[-2 + yyTop];
			if (arguments[arguments.Count - 1] is NamedArgument && !(yyVals[0 + yyTop] is NamedArgument))
			{
				Error_NamedArgumentExpected((NamedArgument)arguments[arguments.Count - 1]);
			}
			arguments.Add((Argument)yyVals[0 + yyTop]);
			yyVal = arguments;
		}

		private void case_536()
		{
			yyVal = new ElementAccess(new BaseThis(GetLocation(yyVals[-3 + yyTop])), (Arguments)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_537()
		{
			Error_SyntaxError(yyToken);
			yyVal = new ElementAccess(null, null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_540()
		{
			if (yyVals[0 + yyTop] != null)
			{
				if (lang_version <= LanguageVersion.ISO_2)
				{
					FeatureIsNotAvailable(GetLocation(yyVals[-5 + yyTop]), "object initializers");
				}
				yyVal = new NewInitialize((FullNamedExpression)yyVals[-4 + yyTop], (Arguments)yyVals[-2 + yyTop], (CollectionOrObjectInitializers)yyVals[0 + yyTop], GetLocation(yyVals[-5 + yyTop]));
			}
			else
			{
				yyVal = new New((FullNamedExpression)yyVals[-4 + yyTop], (Arguments)yyVals[-2 + yyTop], GetLocation(yyVals[-5 + yyTop]));
			}
		}

		private void case_541()
		{
			if (lang_version <= LanguageVersion.ISO_2)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-2 + yyTop]), "collection initializers");
			}
			yyVal = new NewInitialize((FullNamedExpression)yyVals[-1 + yyTop], null, (CollectionOrObjectInitializers)yyVals[0 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_542()
		{
			yyVal = new ArrayCreation((FullNamedExpression)yyVals[-5 + yyTop], (List<Expression>)yyVals[-3 + yyTop], new ComposedTypeSpecifier(((List<Expression>)yyVals[-3 + yyTop]).Count, GetLocation(yyVals[-4 + yyTop]))
			{
				Next = (ComposedTypeSpecifier)yyVals[-1 + yyTop]
			}, (ArrayInitializer)yyVals[0 + yyTop], GetLocation(yyVals[-6 + yyTop]));
		}

		private void case_543()
		{
			if (yyVals[0 + yyTop] == null)
			{
				report.Error(1586, GetLocation(yyVals[-3 + yyTop]), "Array creation must have array size or array initializer");
			}
			yyVal = new ArrayCreation((FullNamedExpression)yyVals[-2 + yyTop], (ComposedTypeSpecifier)yyVals[-1 + yyTop], (ArrayInitializer)yyVals[0 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_544()
		{
			if (lang_version <= LanguageVersion.ISO_2)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-2 + yyTop]), "implicitly typed arrays");
			}
			yyVal = new ImplicitlyTypedArrayCreation((ComposedTypeSpecifier)yyVals[-1 + yyTop], (ArrayInitializer)yyVals[0 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_545()
		{
			report.Error(178, GetLocation(yyVals[-1 + yyTop]), "Invalid rank specifier, expecting `,' or `]'");
			yyVal = new ArrayCreation((FullNamedExpression)yyVals[-5 + yyTop], null, GetLocation(yyVals[-6 + yyTop]));
		}

		private void case_546()
		{
			Error_SyntaxError(yyToken);
			yyVal = new New((FullNamedExpression)yyVals[-1 + yyTop], null, GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_548()
		{
			lexer.parsing_type--;
			yyVal = yyVals[0 + yyTop];
		}

		private void case_549()
		{
			if (lang_version <= LanguageVersion.ISO_2)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-3 + yyTop]), "anonymous types");
			}
			yyVal = new NewAnonymousType((List<AnonymousTypeParameter>)yyVals[-1 + yyTop], current_container, GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_555()
		{
			List<AnonymousTypeParameter> list = new List<AnonymousTypeParameter>(4);
			list.Add((AnonymousTypeParameter)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_556()
		{
			List<AnonymousTypeParameter> list = (List<AnonymousTypeParameter>)yyVals[-2 + yyTop];
			list.Add((AnonymousTypeParameter)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_559()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			yyVal = new AnonymousTypeParameter((Expression)yyVals[0 + yyTop], locatedToken.Value, locatedToken.Location);
		}

		private void case_560()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			yyVal = new AnonymousTypeParameter(new SimpleName(locatedToken.Value, locatedToken.Location), locatedToken.Value, locatedToken.Location);
		}

		private void case_561()
		{
			MemberAccess memberAccess = (MemberAccess)yyVals[0 + yyTop];
			yyVal = new AnonymousTypeParameter(memberAccess, memberAccess.Name, memberAccess.Location);
		}

		private void case_562()
		{
			report.Error(746, lexer.Location, "Invalid anonymous type member declarator. Anonymous type members must be a member assignment, simple name or member access expression");
			yyVal = null;
		}

		private void case_566()
		{
			((ComposedTypeSpecifier)yyVals[-1 + yyTop]).Next = (ComposedTypeSpecifier)yyVals[0 + yyTop];
			yyVal = yyVals[-1 + yyTop];
		}

		private void case_567()
		{
			yyVal = ComposedTypeSpecifier.CreateArrayDimension(1, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_568()
		{
			yyVal = ComposedTypeSpecifier.CreateArrayDimension((int)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_573()
		{
			ArrayInitializer arrayInitializer = new ArrayInitializer(0, GetLocation(yyVals[-1 + yyTop]));
			arrayInitializer.VariableDeclaration = current_variable;
			yyVal = arrayInitializer;
		}

		private void case_574()
		{
			ArrayInitializer arrayInitializer = new ArrayInitializer((List<Expression>)yyVals[-2 + yyTop], GetLocation(yyVals[-3 + yyTop]));
			arrayInitializer.VariableDeclaration = current_variable;
			object obj = yyVals[-1 + yyTop];
			yyVal = arrayInitializer;
		}

		private void case_575()
		{
			List<Expression> list = new List<Expression>(4);
			list.Add((Expression)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_576()
		{
			List<Expression> list = (List<Expression>)yyVals[-2 + yyTop];
			list.Add((Expression)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_577()
		{
			yyVal = new TypeOf((FullNamedExpression)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_579()
		{
			Error_TypeExpected(lexer.Location);
			yyVal = null;
		}

		private void case_580()
		{
			if (lang_version < LanguageVersion.ISO_2)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[0 + yyTop]), "generics");
			}
			yyVal = yyVals[0 + yyTop];
		}

		private void case_581()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			if (lang_version == LanguageVersion.ISO_1)
			{
				FeatureIsNotAvailable(locatedToken.Location, "namespace alias qualifier");
			}
			yyVal = locatedToken;
		}

		private void case_582()
		{
			yyVal = new SizeOf((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_583()
		{
			Error_SyntaxError(yyToken);
			yyVal = new SizeOf((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_584()
		{
			yyVal = new CheckedExpr((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_585()
		{
			Error_SyntaxError(yyToken);
			yyVal = new CheckedExpr(null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_586()
		{
			yyVal = new UnCheckedExpr((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_587()
		{
			Error_SyntaxError(yyToken);
			yyVal = new UnCheckedExpr(null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_588()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new MemberAccess(new Indirection((Expression)yyVals[-3 + yyTop], GetLocation(yyVals[-2 + yyTop])), locatedToken.Value, (TypeArguments)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_596()
		{
			valid_param_mod = (ParameterModifierType)0;
			yyVal = yyVals[-1 + yyTop];
		}

		private void case_597()
		{
			if (lang_version < LanguageVersion.ISO_2)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-3 + yyTop]), "default value expression");
			}
			yyVal = new DefaultValueExpression((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_601()
		{
			yyVal = new Cast((FullNamedExpression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_602()
		{
			if (!async_block)
			{
				if (current_anonymous_method is LambdaExpression)
				{
					report.Error(4034, GetLocation(yyVals[-1 + yyTop]), "The `await' operator can only be used when its containing lambda expression is marked with the `async' modifier");
				}
				else if (current_anonymous_method != null)
				{
					report.Error(4035, GetLocation(yyVals[-1 + yyTop]), "The `await' operator can only be used when its containing anonymous method is marked with the `async' modifier");
				}
				else if (interactive_async.HasValue)
				{
					current_block.Explicit.RegisterAsyncAwait();
					interactive_async = true;
				}
				else
				{
					report.Error(4033, GetLocation(yyVals[-1 + yyTop]), "The `await' operator can only be used when its containing method is marked with the `async' modifier");
				}
			}
			else
			{
				current_block.Explicit.RegisterAsyncAwait();
			}
			yyVal = new Await((Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_603()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Unary(Unary.Operator.LogicalNot, null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_604()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Unary(Unary.Operator.OnesComplement, null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_605()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Cast((FullNamedExpression)yyVals[-2 + yyTop], null, GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_606()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Await(null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_614()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Unary(Unary.Operator.UnaryPlus, null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_615()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Unary(Unary.Operator.UnaryNegation, null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_616()
		{
			Error_SyntaxError(yyToken);
			yyVal = new UnaryMutator(UnaryMutator.Mode.IsIncrement, null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_617()
		{
			Error_SyntaxError(yyToken);
			yyVal = new UnaryMutator(UnaryMutator.Mode.IsDecrement, null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_618()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Indirection(null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_619()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Unary(Unary.Operator.AddressOf, null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_621()
		{
			yyVal = new Binary(Binary.Operator.Multiply, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_622()
		{
			yyVal = new Binary(Binary.Operator.Division, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_623()
		{
			yyVal = new Binary(Binary.Operator.Modulus, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_624()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.Multiply, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_625()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.Division, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_626()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.Modulus, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_628()
		{
			yyVal = new Binary(Binary.Operator.Addition, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_629()
		{
			yyVal = new Binary(Binary.Operator.Subtraction, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_630()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.Addition, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_631()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.Subtraction, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_633()
		{
			Is obj = new Is((Expression)yyVals[-3 + yyTop], (Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
			if (yyVals[0 + yyTop] != null)
			{
				if (lang_version != LanguageVersion.Experimental)
				{
					FeatureIsNotAvailable(GetLocation(yyVals[0 + yyTop]), "type pattern matching");
				}
				LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
				obj.Variable = new LocalVariable(current_block, locatedToken.Value, locatedToken.Location);
				current_block.AddLocalName(obj.Variable);
			}
			yyVal = obj;
		}

		private void case_634()
		{
			Is obj = new Is((Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
			if (lang_version != LanguageVersion.Experimental)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-1 + yyTop]), "pattern matching");
			}
			yyVal = obj;
		}

		private void case_635()
		{
			Error_SyntaxError(yyToken);
			yyVal = new As((Expression)yyVals[-2 + yyTop], null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_636()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Is((Expression)yyVals[-2 + yyTop], null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_637()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			yyVal = new Is(new SimpleName(locatedToken.Value, locatedToken.Location), (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_638()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			yyVal = new As(new SimpleName(locatedToken.Value, locatedToken.Location), (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_645()
		{
			yyVal = new Cast((FullNamedExpression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_651()
		{
			List<PropertyPatternMember> list = new List<PropertyPatternMember>();
			list.Add((PropertyPatternMember)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_652()
		{
			List<PropertyPatternMember> list = (List<PropertyPatternMember>)yyVals[-2 + yyTop];
			list.Add((PropertyPatternMember)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_653()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			yyVal = new PropertyPatternMember(locatedToken.Value, (Expression)yyVals[0 + yyTop], locatedToken.Location);
		}

		private void case_655()
		{
			if (yyVals[0 + yyTop] != null)
			{
				LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
				LocalVariable li = new LocalVariable(current_block, locatedToken.Value, locatedToken.Location);
				current_block.AddLocalName(li);
			}
		}

		private void case_658()
		{
			Arguments arguments = new Arguments(4);
			arguments.Add((Argument)yyVals[0 + yyTop]);
			yyVal = arguments;
		}

		private void case_659()
		{
			Arguments arguments = (Arguments)yyVals[-2 + yyTop];
			if (arguments[arguments.Count - 1] is NamedArgument && !(yyVals[0 + yyTop] is NamedArgument))
			{
				Error_NamedArgumentExpected((NamedArgument)arguments[arguments.Count - 1]);
			}
			arguments.Add((Argument)yyVals[0 + yyTop]);
			yyVal = arguments;
		}

		private void case_661()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			yyVal = new NamedArgument(locatedToken.Value, locatedToken.Location, (Expression)yyVals[0 + yyTop]);
		}

		private void case_663()
		{
			yyVal = new Binary(Binary.Operator.LeftShift, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_664()
		{
			yyVal = new Binary(Binary.Operator.RightShift, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_665()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.LeftShift, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_666()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.RightShift, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_668()
		{
			yyVal = new Binary(Binary.Operator.LessThan, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_669()
		{
			yyVal = new Binary(Binary.Operator.GreaterThan, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_670()
		{
			yyVal = new Binary(Binary.Operator.LessThanOrEqual, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_671()
		{
			yyVal = new Binary(Binary.Operator.GreaterThanOrEqual, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_672()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.LessThan, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_673()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.GreaterThan, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_674()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.LessThanOrEqual, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_675()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.GreaterThanOrEqual, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_677()
		{
			yyVal = new Binary(Binary.Operator.Equality, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_678()
		{
			yyVal = new Binary(Binary.Operator.Inequality, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_679()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.Equality, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_680()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.Inequality, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_682()
		{
			yyVal = new Binary(Binary.Operator.BitwiseAnd, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_683()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.BitwiseAnd, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_685()
		{
			yyVal = new Binary(Binary.Operator.ExclusiveOr, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_686()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.ExclusiveOr, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_688()
		{
			yyVal = new Binary(Binary.Operator.BitwiseOr, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_689()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.BitwiseOr, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_691()
		{
			yyVal = new Binary(Binary.Operator.LogicalAnd, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_692()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.LogicalAnd, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_694()
		{
			yyVal = new Binary(Binary.Operator.LogicalOr, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_695()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Binary(Binary.Operator.LogicalOr, (Expression)yyVals[-2 + yyTop], null);
		}

		private void case_697()
		{
			if (lang_version < LanguageVersion.ISO_2)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-1 + yyTop]), "null coalescing operator");
			}
			yyVal = new NullCoalescingOperator((Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_699()
		{
			yyVal = new Conditional(new BooleanExpression((Expression)yyVals[-4 + yyTop]), (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_700()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Conditional(new BooleanExpression((Expression)yyVals[-3 + yyTop]), (Expression)yyVals[-1 + yyTop], null, GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_701()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Conditional(new BooleanExpression((Expression)yyVals[-4 + yyTop]), (Expression)yyVals[-2 + yyTop], null, GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_702()
		{
			Error_SyntaxError(372);
			yyVal = new Conditional(new BooleanExpression((Expression)yyVals[-4 + yyTop]), (Expression)yyVals[-2 + yyTop], null, GetLocation(yyVals[-3 + yyTop]));
			lexer.putback(125);
		}

		private void case_703()
		{
			yyVal = new SimpleAssign((Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_704()
		{
			yyVal = new CompoundAssign(Binary.Operator.Multiply, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_705()
		{
			yyVal = new CompoundAssign(Binary.Operator.Division, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_706()
		{
			yyVal = new CompoundAssign(Binary.Operator.Modulus, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_707()
		{
			yyVal = new CompoundAssign(Binary.Operator.Addition, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_708()
		{
			yyVal = new CompoundAssign(Binary.Operator.Subtraction, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_709()
		{
			yyVal = new CompoundAssign(Binary.Operator.LeftShift, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_710()
		{
			yyVal = new CompoundAssign(Binary.Operator.RightShift, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_711()
		{
			yyVal = new CompoundAssign(Binary.Operator.BitwiseAnd, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_712()
		{
			yyVal = new CompoundAssign(Binary.Operator.BitwiseOr, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_713()
		{
			yyVal = new CompoundAssign(Binary.Operator.ExclusiveOr, (Expression)yyVals[-2 + yyTop], (Expression)yyVals[0 + yyTop]);
		}

		private void case_714()
		{
			List<Parameter> list = new List<Parameter>(4);
			list.Add((Parameter)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_715()
		{
			List<Parameter> list = (List<Parameter>)yyVals[-2 + yyTop];
			Parameter parameter = (Parameter)yyVals[0 + yyTop];
			if (list[0].GetType() != parameter.GetType())
			{
				report.Error(748, parameter.Location, "All lambda parameters must be typed either explicitly or implicitly");
			}
			list.Add(parameter);
			yyVal = list;
		}

		private void case_716()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			yyVal = new Parameter((FullNamedExpression)yyVals[-1 + yyTop], locatedToken.Value, (Parameter.Modifier)yyVals[-2 + yyTop], null, locatedToken.Location);
		}

		private void case_717()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			yyVal = new Parameter((FullNamedExpression)yyVals[-1 + yyTop], locatedToken.Value, Parameter.Modifier.NONE, null, locatedToken.Location);
		}

		private void case_718()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			yyVal = new ImplicitLambdaParameter(locatedToken.Value, locatedToken.Location);
		}

		private void case_719()
		{
			LocatedToken locatedToken = (LocatedToken)Error_AwaitAsIdentifier(yyVals[0 + yyTop]);
			yyVal = new ImplicitLambdaParameter(locatedToken.Value, locatedToken.Location);
		}

		private void case_721()
		{
			List<Parameter> list = (List<Parameter>)yyVals[0 + yyTop];
			yyVal = new ParametersCompiled(list.ToArray());
		}

		private void case_723()
		{
			Block block = end_block(Location.Null);
			block.IsCompilerGenerated = true;
			block.AddStatement(new ContextualReturn((Expression)yyVals[0 + yyTop]));
			yyVal = block;
		}

		private void case_725()
		{
			end_block(Location.Null).IsCompilerGenerated = true;
			Error_SyntaxError(yyToken);
			yyVal = null;
		}

		private void case_727()
		{
			Error_SyntaxError(yyToken);
			yyVal = null;
		}

		private void case_728()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			Parameter parameter = new ImplicitLambdaParameter(locatedToken.Value, locatedToken.Location);
			start_anonymous(true, new ParametersCompiled(parameter), false, locatedToken.Location);
		}

		private void case_729()
		{
			yyVal = end_anonymous((ParametersBlock)yyVals[0 + yyTop]);
		}

		private void case_730()
		{
			LocatedToken locatedToken = (LocatedToken)Error_AwaitAsIdentifier(yyVals[-1 + yyTop]);
			Parameter parameter = new ImplicitLambdaParameter(locatedToken.Value, locatedToken.Location);
			start_anonymous(true, new ParametersCompiled(parameter), false, locatedToken.Location);
		}

		private void case_731()
		{
			yyVal = end_anonymous((ParametersBlock)yyVals[0 + yyTop]);
		}

		private void case_732()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			Parameter parameter = new ImplicitLambdaParameter(locatedToken.Value, locatedToken.Location);
			start_anonymous(true, new ParametersCompiled(parameter), true, locatedToken.Location);
		}

		private void case_733()
		{
			yyVal = end_anonymous((ParametersBlock)yyVals[0 + yyTop]);
		}

		private void case_735()
		{
			valid_param_mod = (ParameterModifierType)0;
			start_anonymous(true, (ParametersCompiled)yyVals[-2 + yyTop], false, GetLocation(yyVals[-4 + yyTop]));
		}

		private void case_736()
		{
			yyVal = end_anonymous((ParametersBlock)yyVals[0 + yyTop]);
		}

		private void case_738()
		{
			valid_param_mod = (ParameterModifierType)0;
			start_anonymous(true, (ParametersCompiled)yyVals[-2 + yyTop], true, GetLocation(yyVals[-5 + yyTop]));
		}

		private void case_739()
		{
			yyVal = end_anonymous((ParametersBlock)yyVals[0 + yyTop]);
		}

		private void case_746()
		{
			yyVal = new RefValueExpr((Expression)yyVals[-3 + yyTop], (FullNamedExpression)yyVals[-1 + yyTop], GetLocation(yyVals[-5 + yyTop]));
		}

		private void case_747()
		{
			yyVal = new RefTypeExpr((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_748()
		{
			yyVal = new MakeRefExpr((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_753()
		{
			yyVal = yyVals[-1 + yyTop];
			if (lang_version != LanguageVersion.Experimental)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-2 + yyTop]), "primary constructor");
			}
		}

		private void case_758()
		{
			lexer.parsing_block++;
			current_type.PrimaryConstructorBaseArgumentsStart = GetLocation(yyVals[0 + yyTop]);
		}

		private void case_759()
		{
			current_type.PrimaryConstructorBaseArguments = (Arguments)yyVals[-1 + yyTop];
			lexer.parsing_block--;
			yyVal = yyVals[-5 + yyTop];
		}

		private void case_761()
		{
			lexer.ConstraintsParsing = true;
			Class obj = new Class(current_container, (MemberName)yyVals[0 + yyTop], (Modifiers)yyVals[-4 + yyTop], (Attributes)yyVals[-5 + yyTop]);
			if ((obj.ModFlags & Modifiers.STATIC) != 0 && lang_version == LanguageVersion.ISO_1)
			{
				FeatureIsNotAvailable(obj.Location, "static classes");
			}
			push_current_container(obj, yyVals[-3 + yyTop]);
			valid_param_mod = ParameterModifierType.PrimaryConstructor;
		}

		private void case_762()
		{
			valid_param_mod = (ParameterModifierType)0;
			lexer.ConstraintsParsing = false;
			if (yyVals[-1 + yyTop] != null)
			{
				current_type.PrimaryConstructorParameters = (ParametersCompiled)yyVals[-1 + yyTop];
			}
			if (yyVals[0 + yyTop] != null)
			{
				current_container.SetConstraints((List<Constraints>)yyVals[0 + yyTop]);
			}
			if (doc_support)
			{
				current_container.PartialContainer.DocComment = Lexer.consume_doc_comment();
				Lexer.doc_state = XmlCommentState.Allowed;
			}
			lexer.parsing_modifiers = true;
		}

		private void case_763()
		{
			lexer.parsing_declaration--;
			if (doc_support)
			{
				Lexer.doc_state = XmlCommentState.Allowed;
			}
		}

		private void case_764()
		{
			object obj = yyVals[0 + yyTop];
			yyVal = pop_current_class();
		}

		private void case_767()
		{
			mod_locations = null;
			yyVal = ModifierNone;
			lexer.parsing_modifiers = false;
		}

		private void case_770()
		{
			Modifiers modifiers = (Modifiers)yyVals[-1 + yyTop];
			Modifiers modifiers2 = (Modifiers)yyVals[0 + yyTop];
			if ((modifiers & modifiers2) != 0)
			{
				report.Error(1004, lexer.Location - ModifiersExtensions.Name(modifiers2).Length, "Duplicate `{0}' modifier", ModifiersExtensions.Name(modifiers2));
			}
			else if ((modifiers2 & Modifiers.AccessibilityMask) != 0 && (modifiers & Modifiers.AccessibilityMask) != 0 && (modifiers2 | (modifiers & Modifiers.AccessibilityMask)) != (Modifiers.PROTECTED | Modifiers.INTERNAL))
			{
				report.Error(107, lexer.Location - ModifiersExtensions.Name(modifiers2).Length, "More than one protection modifier specified");
			}
			yyVal = modifiers | modifiers2;
		}

		private void case_771()
		{
			yyVal = Modifiers.NEW;
			if (current_container.Kind == MemberKind.Namespace)
			{
				report.Error(1530, GetLocation(yyVals[0 + yyTop]), "Keyword `new' is not allowed on namespace elements");
			}
		}

		private void case_772()
		{
			yyVal = Modifiers.PUBLIC;
		}

		private void case_773()
		{
			yyVal = Modifiers.PROTECTED;
		}

		private void case_774()
		{
			yyVal = Modifiers.INTERNAL;
		}

		private void case_775()
		{
			yyVal = Modifiers.PRIVATE;
		}

		private void case_776()
		{
			yyVal = Modifiers.ABSTRACT;
		}

		private void case_777()
		{
			yyVal = Modifiers.SEALED;
		}

		private void case_778()
		{
			yyVal = Modifiers.STATIC;
		}

		private void case_779()
		{
			yyVal = Modifiers.READONLY;
		}

		private void case_780()
		{
			yyVal = Modifiers.VIRTUAL;
		}

		private void case_781()
		{
			yyVal = Modifiers.OVERRIDE;
		}

		private void case_782()
		{
			yyVal = Modifiers.EXTERN;
		}

		private void case_783()
		{
			yyVal = Modifiers.VOLATILE;
		}

		private void case_784()
		{
			yyVal = Modifiers.UNSAFE;
			if (!settings.Unsafe)
			{
				Error_UnsafeCodeNotAllowed(GetLocation(yyVals[0 + yyTop]));
			}
		}

		private void case_785()
		{
			yyVal = Modifiers.ASYNC;
		}

		private void case_789()
		{
			Error_SyntaxError(yyToken);
			current_type.SetBaseTypes((List<FullNamedExpression>)yyVals[-1 + yyTop]);
		}

		private void case_792()
		{
			List<Constraints> list = new List<Constraints>(1);
			list.Add((Constraints)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_793()
		{
			List<Constraints> list = (List<Constraints>)yyVals[-1 + yyTop];
			Constraints constraints = (Constraints)yyVals[0 + yyTop];
			foreach (Constraints item in list)
			{
				if (constraints.TypeParameter.Value == item.TypeParameter.Value)
				{
					report.Error(409, constraints.Location, "A constraint clause has already been specified for type parameter `{0}'", constraints.TypeParameter.Value);
				}
			}
			list.Add(constraints);
			yyVal = list;
		}

		private void case_794()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			yyVal = new Constraints(new SimpleMemberName(locatedToken.Value, locatedToken.Location), (List<FullNamedExpression>)yyVals[0 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_795()
		{
			Error_SyntaxError(yyToken);
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new Constraints(new SimpleMemberName(locatedToken.Value, locatedToken.Location), null, GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_796()
		{
			List<FullNamedExpression> list = new List<FullNamedExpression>(1);
			list.Add((FullNamedExpression)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_797()
		{
			List<FullNamedExpression> list = (List<FullNamedExpression>)yyVals[-2 + yyTop];
			SpecialContraintExpr specialContraintExpr = list[list.Count - 1] as SpecialContraintExpr;
			if (specialContraintExpr != null && (specialContraintExpr.Constraint & SpecialConstraint.Constructor) != SpecialConstraint.None)
			{
				report.Error(401, GetLocation(yyVals[-1 + yyTop]), "The `new()' constraint must be the last constraint specified");
			}
			specialContraintExpr = yyVals[0 + yyTop] as SpecialContraintExpr;
			if (specialContraintExpr != null)
			{
				if ((specialContraintExpr.Constraint & (SpecialConstraint.Class | SpecialConstraint.Struct)) != SpecialConstraint.None)
				{
					report.Error(449, specialContraintExpr.Location, "The `class' or `struct' constraint must be the first constraint specified");
				}
				else
				{
					specialContraintExpr = list[0] as SpecialContraintExpr;
					if (specialContraintExpr != null && (specialContraintExpr.Constraint & SpecialConstraint.Struct) != SpecialConstraint.None)
					{
						report.Error(451, GetLocation(yyVals[0 + yyTop]), "The `new()' constraint cannot be used with the `struct' constraint");
					}
				}
			}
			list.Add((FullNamedExpression)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_798()
		{
			if (yyVals[0 + yyTop] is ComposedCast)
			{
				report.Error(706, GetLocation(yyVals[0 + yyTop]), "Invalid constraint type `{0}'", ((ComposedCast)yyVals[0 + yyTop]).GetSignatureForError());
			}
			yyVal = yyVals[0 + yyTop];
		}

		private void case_799()
		{
			yyVal = new SpecialContraintExpr(SpecialConstraint.Constructor, GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_803()
		{
			if (lang_version <= LanguageVersion.V_3)
			{
				FeatureIsNotAvailable(lexer.Location, "generic type variance");
			}
			yyVal = yyVals[0 + yyTop];
		}

		private void case_806()
		{
			lexer.parsing_block++;
			start_block(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_808()
		{
			lexer.parsing_block--;
			yyVal = end_block(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_809()
		{
			lexer.parsing_block--;
			yyVal = end_block(lexer.Location);
		}

		private void case_810()
		{
			lexer.parsing_block++;
			current_block.StartLocation = GetLocation(yyVals[0 + yyTop]);
		}

		private void case_811()
		{
			lexer.parsing_block--;
			yyVal = end_block(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_819()
		{
			Error_SyntaxError(yyToken);
			yyVal = null;
		}

		private void case_852()
		{
			report.Error(1023, GetLocation(yyVals[0 + yyTop]), "An embedded statement may not be a declaration or labeled statement");
			yyVal = null;
		}

		private void case_853()
		{
			report.Error(1023, GetLocation(yyVals[0 + yyTop]), "An embedded statement may not be a declaration or labeled statement");
			yyVal = null;
		}

		private void case_854()
		{
			Error_SyntaxError(yyToken);
			yyVal = new EmptyStatement(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_855()
		{
			yyVal = new EmptyStatement(lexer.Location);
		}

		private void case_856()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			LabeledStatement labeledStatement = new LabeledStatement(locatedToken.Value, current_block, locatedToken.Location);
			current_block.AddLabel(labeledStatement);
			current_block.AddStatement(labeledStatement);
		}

		private void case_859()
		{
			if (yyVals[-1 + yyTop] is VarExpr)
			{
				yyVals[-1 + yyTop] = new SimpleName("var", ((VarExpr)yyVals[-1 + yyTop]).Location);
			}
			yyVal = new ComposedCast((FullNamedExpression)yyVals[-1 + yyTop], (ComposedTypeSpecifier)yyVals[0 + yyTop]);
		}

		private void case_860()
		{
			ATypeNameExpression aTypeNameExpression = (ATypeNameExpression)yyVals[-1 + yyTop];
			if (yyVals[0 + yyTop] == null)
			{
				if (aTypeNameExpression.Name == "var" && aTypeNameExpression is SimpleName)
				{
					yyVal = new VarExpr(aTypeNameExpression.Location);
				}
				else
				{
					yyVal = yyVals[-1 + yyTop];
				}
			}
			else
			{
				yyVal = new ComposedCast(aTypeNameExpression, (ComposedTypeSpecifier)yyVals[0 + yyTop]);
			}
		}

		private void case_861()
		{
			ATypeNameExpression left = (ATypeNameExpression)yyVals[-1 + yyTop];
			yyVal = new ComposedCast(left, (ComposedTypeSpecifier)yyVals[0 + yyTop]);
		}

		private void case_865()
		{
			((ComposedTypeSpecifier)yyVals[-1 + yyTop]).Next = (ComposedTypeSpecifier)yyVals[0 + yyTop];
			yyVal = yyVals[-1 + yyTop];
		}

		private void case_869()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			LocalVariable li = new LocalVariable(current_block, locatedToken.Value, locatedToken.Location);
			current_block.AddLocalName(li);
			current_variable = new BlockVariable((FullNamedExpression)yyVals[-1 + yyTop], li);
		}

		private void case_870()
		{
			yyVal = current_variable;
			current_variable = null;
			object obj = yyVals[-2 + yyTop];
		}

		private void case_871()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			LocalVariable li = new LocalVariable(current_block, locatedToken.Value, LocalVariable.Flags.Constant, locatedToken.Location);
			current_block.AddLocalName(li);
			current_variable = new BlockConstant((FullNamedExpression)yyVals[-1 + yyTop], li);
		}

		private void case_872()
		{
			yyVal = current_variable;
			current_variable = null;
		}

		private void case_874()
		{
			current_variable.Initializer = (Expression)yyVals[0 + yyTop];
			yyVal = current_variable;
		}

		private void case_875()
		{
			if (yyToken == 427)
			{
				report.Error(650, lexer.Location, "Syntax error, bad array declarator. To declare a managed array the rank specifier precedes the variable's identifier. To declare a fixed size buffer field, use the fixed keyword before the field type");
			}
			else
			{
				Error_SyntaxError(yyToken);
			}
		}

		private void case_879()
		{
			foreach (BlockVariableDeclarator declarator in current_variable.Declarators)
			{
				if (declarator.Initializer == null)
				{
					Error_MissingInitializer(declarator.Variable.Location);
				}
			}
		}

		private void case_882()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			LocalVariable li = new LocalVariable(current_variable.Variable, locatedToken.Value, locatedToken.Location);
			BlockVariableDeclarator decl = new BlockVariableDeclarator(li, null);
			current_variable.AddDeclarator(decl);
			current_block.AddLocalName(li);
		}

		private void case_883()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			LocalVariable li = new LocalVariable(current_variable.Variable, locatedToken.Value, locatedToken.Location);
			BlockVariableDeclarator decl = new BlockVariableDeclarator(li, (Expression)yyVals[0 + yyTop]);
			current_variable.AddDeclarator(decl);
			current_block.AddLocalName(li);
		}

		private void case_890()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			LocalVariable li = new LocalVariable(current_block, locatedToken.Value, LocalVariable.Flags.Constant, locatedToken.Location);
			BlockVariableDeclarator decl = new BlockVariableDeclarator(li, (Expression)yyVals[0 + yyTop]);
			current_variable.AddDeclarator(decl);
			current_block.AddLocalName(li);
		}

		private void case_892()
		{
			yyVal = new StackAlloc((Expression)yyVals[-3 + yyTop], (Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-4 + yyTop]));
		}

		private void case_893()
		{
			report.Error(1575, GetLocation(yyVals[-1 + yyTop]), "A stackalloc expression requires [] after type");
			yyVal = new StackAlloc((Expression)yyVals[0 + yyTop], null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_894()
		{
			yyVal = yyVals[-1 + yyTop];
		}

		private void case_896()
		{
			yyVal = yyVals[-1 + yyTop];
			report.Error(1002, GetLocation(yyVals[0 + yyTop]), "; expected");
			lexer.putback(125);
		}

		private void case_899()
		{
			ExpressionStatement expressionStatement = yyVals[0 + yyTop] as ExpressionStatement;
			if (expressionStatement == null)
			{
				Expression expr = yyVals[0 + yyTop] as Expression;
				yyVal = new StatementErrorExpression(expr);
			}
			else
			{
				yyVal = new StatementExpression(expressionStatement);
			}
		}

		private void case_900()
		{
			Expression s = (Expression)yyVals[0 + yyTop];
			yyVal = new StatementExpression(new OptionalAssign(s, lexer.Location));
		}

		private void case_901()
		{
			Error_SyntaxError(yyToken);
			yyVal = new EmptyStatement(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_904()
		{
			if (yyVals[0 + yyTop] is EmptyStatement)
			{
				Warning_EmptyStatement(GetLocation(yyVals[0 + yyTop]));
			}
			yyVal = new If((BooleanExpression)yyVals[-2 + yyTop], (Statement)yyVals[0 + yyTop], GetLocation(yyVals[-4 + yyTop]));
		}

		private void case_905()
		{
			yyVal = new If((BooleanExpression)yyVals[-4 + yyTop], (Statement)yyVals[-2 + yyTop], (Statement)yyVals[0 + yyTop], GetLocation(yyVals[-6 + yyTop]));
			if (yyVals[-2 + yyTop] is EmptyStatement)
			{
				Warning_EmptyStatement(GetLocation(yyVals[-2 + yyTop]));
			}
			if (yyVals[0 + yyTop] is EmptyStatement)
			{
				Warning_EmptyStatement(GetLocation(yyVals[0 + yyTop]));
			}
		}

		private void case_906()
		{
			Error_SyntaxError(yyToken);
			yyVal = new If((BooleanExpression)yyVals[-1 + yyTop], null, GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_908()
		{
			yyVal = new Switch((Expression)yyVals[-5 + yyTop], current_block.Explicit, GetLocation(yyVals[-7 + yyTop]));
			end_block(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_909()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Switch((Expression)yyVals[-1 + yyTop], null, GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_916()
		{
			SwitchLabel switchLabel = (SwitchLabel)yyVals[0 + yyTop];
			switchLabel.SectionStart = true;
			current_block.AddStatement(switchLabel);
		}

		private void case_918()
		{
			yyVal = new SwitchLabel((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_919()
		{
			Error_SyntaxError(yyToken);
			yyVal = new SwitchLabel((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_925()
		{
			if (yyVals[0 + yyTop] is EmptyStatement && lexer.peek_token() == 371)
			{
				Warning_EmptyStatement(GetLocation(yyVals[0 + yyTop]));
			}
			yyVal = new While((BooleanExpression)yyVals[-2 + yyTop], (Statement)yyVals[0 + yyTop], GetLocation(yyVals[-4 + yyTop]));
		}

		private void case_926()
		{
			Error_SyntaxError(yyToken);
			yyVal = new While((BooleanExpression)yyVals[-1 + yyTop], null, GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_927()
		{
			yyVal = new Do((Statement)yyVals[-5 + yyTop], (BooleanExpression)yyVals[-2 + yyTop], GetLocation(yyVals[-6 + yyTop]), GetLocation(yyVals[-4 + yyTop]));
		}

		private void case_928()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Do((Statement)yyVals[-1 + yyTop], null, GetLocation(yyVals[-2 + yyTop]), Location.Null);
		}

		private void case_929()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Do((Statement)yyVals[-4 + yyTop], (BooleanExpression)yyVals[-1 + yyTop], GetLocation(yyVals[-5 + yyTop]), GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_930()
		{
			start_block(GetLocation(yyVals[0 + yyTop]));
			current_block.IsCompilerGenerated = true;
			For s = new For(GetLocation(yyVals[-1 + yyTop]));
			current_block.AddStatement(s);
			yyVal = s;
		}

		private void case_932()
		{
			((For)yyVals[-2 + yyTop]).Initializer = (Statement)yyVals[-1 + yyTop];
			oob_stack.Push(yyVals[-2 + yyTop]);
		}

		private void case_933()
		{
			Tuple<Location, Location> tuple = (Tuple<Location, Location>)yyVals[-1 + yyTop];
			oob_stack.Pop();
			if (yyVals[0 + yyTop] is EmptyStatement && lexer.peek_token() == 371)
			{
				Warning_EmptyStatement(GetLocation(yyVals[0 + yyTop]));
			}
			((For)yyVals[-5 + yyTop]).Statement = (Statement)yyVals[0 + yyTop];
			yyVal = end_block(GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_934()
		{
			Error_SyntaxError(yyToken);
			yyVal = end_block(current_block.StartLocation);
		}

		private void case_935()
		{
			((For)oob_stack.Peek()).Condition = (BooleanExpression)yyVals[-1 + yyTop];
		}

		private void case_937()
		{
			report.Error(1525, GetLocation(yyVals[0 + yyTop]), "Unexpected symbol `}'");
			((For)oob_stack.Peek()).Condition = (BooleanExpression)yyVals[-1 + yyTop];
			yyVal = new Tuple<Location, Location>(GetLocation(yyVals[0 + yyTop]), GetLocation(yyVals[0 + yyTop]));
		}

		private void case_938()
		{
			((For)oob_stack.Peek()).Iterator = (Statement)yyVals[-1 + yyTop];
			yyVal = GetLocation(yyVals[0 + yyTop]);
		}

		private void case_939()
		{
			report.Error(1525, GetLocation(yyVals[0 + yyTop]), "Unexpected symbol expected ')'");
			((For)oob_stack.Peek()).Iterator = (Statement)yyVals[-1 + yyTop];
			yyVal = GetLocation(yyVals[0 + yyTop]);
		}

		private void case_944()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			LocalVariable li = new LocalVariable(current_block, locatedToken.Value, locatedToken.Location);
			current_block.AddLocalName(li);
			current_variable = new BlockVariable((FullNamedExpression)yyVals[-1 + yyTop], li);
		}

		private void case_945()
		{
			yyVal = current_variable;
			object obj = yyVals[-1 + yyTop];
			current_variable = null;
		}

		private void case_953()
		{
			StatementList statementList = yyVals[-2 + yyTop] as StatementList;
			if (statementList == null)
			{
				statementList = new StatementList((Statement)yyVals[-2 + yyTop], (Statement)yyVals[0 + yyTop]);
			}
			else
			{
				statementList.Add((Statement)yyVals[0 + yyTop]);
			}
			yyVal = statementList;
		}

		private void case_954()
		{
			report.Error(230, GetLocation(yyVals[-3 + yyTop]), "Type and identifier are both required in a foreach statement");
			start_block(GetLocation(yyVals[-2 + yyTop]));
			current_block.IsCompilerGenerated = true;
			Foreach s = new Foreach((Expression)yyVals[-1 + yyTop], null, null, null, null, GetLocation(yyVals[-3 + yyTop]));
			current_block.AddStatement(s);
			yyVal = end_block(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_955()
		{
			Error_SyntaxError(yyToken);
			start_block(GetLocation(yyVals[-3 + yyTop]));
			current_block.IsCompilerGenerated = true;
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			LocalVariable localVariable = new LocalVariable(current_block, locatedToken.Value, LocalVariable.Flags.Used | LocalVariable.Flags.ForeachVariable, locatedToken.Location);
			current_block.AddLocalName(localVariable);
			Foreach s = new Foreach((Expression)yyVals[-2 + yyTop], localVariable, null, null, null, GetLocation(yyVals[-4 + yyTop]));
			current_block.AddStatement(s);
			yyVal = end_block(GetLocation(yyVals[0 + yyTop]));
		}

		private void case_956()
		{
			start_block(GetLocation(yyVals[-5 + yyTop]));
			current_block.IsCompilerGenerated = true;
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			LocalVariable li = new LocalVariable(current_block, locatedToken.Value, LocalVariable.Flags.Used | LocalVariable.Flags.ForeachVariable, locatedToken.Location);
			current_block.AddLocalName(li);
			yyVal = li;
		}

		private void case_957()
		{
			if (yyVals[0 + yyTop] is EmptyStatement && lexer.peek_token() == 371)
			{
				Warning_EmptyStatement(GetLocation(yyVals[0 + yyTop]));
			}
			Foreach obj = new Foreach((Expression)yyVals[-6 + yyTop], (LocalVariable)yyVals[-1 + yyTop], (Expression)yyVals[-3 + yyTop], (Statement)yyVals[0 + yyTop], current_block, GetLocation(yyVals[-8 + yyTop]));
			end_block(GetLocation(yyVals[-2 + yyTop]));
			yyVal = obj;
		}

		private void case_964()
		{
			yyVal = new Break(GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_965()
		{
			yyVal = new Continue(GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_966()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Continue(GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_967()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new Goto(locatedToken.Value, GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_968()
		{
			yyVal = new GotoCase((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_969()
		{
			yyVal = new GotoDefault(GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_970()
		{
			yyVal = new Return((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_971()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Return((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_972()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Return(null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_973()
		{
			yyVal = new Throw((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_974()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Throw((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_975()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Throw(null, GetLocation(yyVals[-1 + yyTop]));
		}

		private void case_976()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			if (locatedToken.Value != "yield")
			{
				report.Error(1003, locatedToken.Location, "; expected");
			}
			else if (yyVals[-1 + yyTop] == null)
			{
				report.Error(1627, GetLocation(yyVals[0 + yyTop]), "Expression expected after yield return");
			}
			else if (lang_version == LanguageVersion.ISO_1)
			{
				FeatureIsNotAvailable(locatedToken.Location, "iterators");
			}
			current_block.Explicit.RegisterIteratorYield();
			yyVal = new Yield((Expression)yyVals[-1 + yyTop], locatedToken.Location);
		}

		private void case_977()
		{
			Error_SyntaxError(yyToken);
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			if (locatedToken.Value != "yield")
			{
				report.Error(1003, locatedToken.Location, "; expected");
			}
			else if (yyVals[-1 + yyTop] == null)
			{
				report.Error(1627, GetLocation(yyVals[0 + yyTop]), "Expression expected after yield return");
			}
			else if (lang_version == LanguageVersion.ISO_1)
			{
				FeatureIsNotAvailable(locatedToken.Location, "iterators");
			}
			current_block.Explicit.RegisterIteratorYield();
			yyVal = new Yield((Expression)yyVals[-1 + yyTop], locatedToken.Location);
		}

		private void case_978()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			if (locatedToken.Value != "yield")
			{
				report.Error(1003, locatedToken.Location, "; expected");
			}
			else if (lang_version == LanguageVersion.ISO_1)
			{
				FeatureIsNotAvailable(locatedToken.Location, "iterators");
			}
			current_block.ParametersBlock.TopBlock.IsIterator = true;
			yyVal = new YieldBreak(locatedToken.Location);
		}

		private void case_982()
		{
			yyVal = new TryFinally((Statement)yyVals[-2 + yyTop], (ExplicitBlock)yyVals[0 + yyTop], GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_983()
		{
			yyVal = new TryFinally(new TryCatch((Block)yyVals[-3 + yyTop], (List<Catch>)yyVals[-2 + yyTop], Location.Null, true), (ExplicitBlock)yyVals[0 + yyTop], GetLocation(yyVals[-4 + yyTop]));
		}

		private void case_984()
		{
			Error_SyntaxError(1524, yyToken);
			yyVal = new TryCatch((Block)yyVals[-1 + yyTop], null, GetLocation(yyVals[-2 + yyTop]), false);
		}

		private void case_985()
		{
			List<Catch> list = new List<Catch>(2);
			list.Add((Catch)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_986()
		{
			List<Catch> list = (List<Catch>)yyVals[-1 + yyTop];
			Catch obj = (Catch)yyVals[0 + yyTop];
			Catch obj2 = list[list.Count - 1];
			if (obj2.IsGeneral && obj2.Filter == null)
			{
				report.Error(1017, obj.loc, "Try statement already has an empty catch block");
			}
			list.Add(obj);
			yyVal = list;
		}

		private void case_989()
		{
			Catch obj = new Catch((ExplicitBlock)yyVals[0 + yyTop], GetLocation(yyVals[-2 + yyTop]));
			obj.Filter = (CatchFilterExpression)yyVals[-1 + yyTop];
			yyVal = obj;
		}

		private void case_990()
		{
			start_block(GetLocation(yyVals[-3 + yyTop]));
			Catch obj = new Catch((ExplicitBlock)current_block, GetLocation(yyVals[-4 + yyTop]));
			obj.TypeExpression = (FullNamedExpression)yyVals[-2 + yyTop];
			if (yyVals[-1 + yyTop] != null)
			{
				LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
				obj.Variable = new LocalVariable(current_block, locatedToken.Value, locatedToken.Location);
				current_block.AddLocalName(obj.Variable);
			}
			yyVal = obj;
			lexer.parsing_catch_when = true;
		}

		private void case_991()
		{
			((Catch)yyVals[-1 + yyTop]).Filter = (CatchFilterExpression)yyVals[0 + yyTop];
			yyVal = yyVals[-1 + yyTop];
		}

		private void case_992()
		{
			if (yyToken == 376)
			{
				report.Error(1015, lexer.Location, "A type that derives from `System.Exception', `object', or `string' expected");
			}
			else
			{
				Error_SyntaxError(yyToken);
			}
			yyVal = new Catch(null, GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_994()
		{
			end_block(Location.Null);
			Error_SyntaxError(yyToken);
			yyVal = null;
		}

		private void case_997()
		{
			if (lang_version <= LanguageVersion.V_5)
			{
				FeatureIsNotAvailable(GetLocation(yyVals[-4 + yyTop]), "exception filter");
			}
			yyVal = new CatchFilterExpression((Expression)yyVals[-1 + yyTop], GetLocation(yyVals[-4 + yyTop]));
		}

		private void case_1000()
		{
			if (!settings.Unsafe)
			{
				Error_UnsafeCodeNotAllowed(GetLocation(yyVals[0 + yyTop]));
			}
		}

		private void case_1002()
		{
			if (yyVals[0 + yyTop] is EmptyStatement && lexer.peek_token() == 371)
			{
				Warning_EmptyStatement(GetLocation(yyVals[0 + yyTop]));
			}
			yyVal = new Lock((Expression)yyVals[-2 + yyTop], (Statement)yyVals[0 + yyTop], GetLocation(yyVals[-4 + yyTop]));
		}

		private void case_1003()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Lock((Expression)yyVals[-1 + yyTop], null, GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_1004()
		{
			start_block(GetLocation(yyVals[-2 + yyTop]));
			current_block.IsCompilerGenerated = true;
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			LocalVariable li = new LocalVariable(current_block, locatedToken.Value, LocalVariable.Flags.Used | LocalVariable.Flags.FixedVariable, locatedToken.Location);
			current_block.AddLocalName(li);
			current_variable = new Fixed.VariableDeclaration((FullNamedExpression)yyVals[-1 + yyTop], li);
		}

		private void case_1005()
		{
			yyVal = current_variable;
			current_variable = null;
		}

		private void case_1006()
		{
			if (yyVals[0 + yyTop] is EmptyStatement && lexer.peek_token() == 371)
			{
				Warning_EmptyStatement(GetLocation(yyVals[0 + yyTop]));
			}
			Fixed s = new Fixed((Fixed.VariableDeclaration)yyVals[-1 + yyTop], (Statement)yyVals[0 + yyTop], GetLocation(yyVals[-9 + yyTop]));
			current_block.AddStatement(s);
			yyVal = end_block(GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_1007()
		{
			start_block(GetLocation(yyVals[-2 + yyTop]));
			current_block.IsCompilerGenerated = true;
			LocatedToken locatedToken = (LocatedToken)yyVals[0 + yyTop];
			LocalVariable li = new LocalVariable(current_block, locatedToken.Value, LocalVariable.Flags.Used | LocalVariable.Flags.UsingVariable, locatedToken.Location);
			current_block.AddLocalName(li);
			current_variable = new Using.VariableDeclaration((FullNamedExpression)yyVals[-1 + yyTop], li);
		}

		private void case_1008()
		{
			yyVal = current_variable;
			current_variable = null;
		}

		private void case_1009()
		{
			if (yyVals[0 + yyTop] is EmptyStatement && lexer.peek_token() == 371)
			{
				Warning_EmptyStatement(GetLocation(yyVals[0 + yyTop]));
			}
			Using s = new Using((Using.VariableDeclaration)yyVals[-1 + yyTop], (Statement)yyVals[0 + yyTop], GetLocation(yyVals[-8 + yyTop]));
			current_block.AddStatement(s);
			yyVal = end_block(GetLocation(yyVals[-2 + yyTop]));
		}

		private void case_1010()
		{
			if (yyVals[0 + yyTop] is EmptyStatement && lexer.peek_token() == 371)
			{
				Warning_EmptyStatement(GetLocation(yyVals[0 + yyTop]));
			}
			yyVal = new Using((Expression)yyVals[-2 + yyTop], (Statement)yyVals[0 + yyTop], GetLocation(yyVals[-4 + yyTop]));
		}

		private void case_1011()
		{
			Error_SyntaxError(yyToken);
			yyVal = new Using((Expression)yyVals[-1 + yyTop], null, GetLocation(yyVals[-3 + yyTop]));
		}

		private void case_1013()
		{
			Error_SyntaxError(yyToken);
		}

		private void case_1015()
		{
			current_variable.Initializer = (Expression)yyVals[0 + yyTop];
			yyVal = current_variable;
		}

		private void case_1016()
		{
			lexer.query_parsing = false;
			AQueryClause aQueryClause = yyVals[-1 + yyTop] as AQueryClause;
			aQueryClause.Tail.Next = (AQueryClause)yyVals[0 + yyTop];
			yyVal = aQueryClause;
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
		}

		private void case_1017()
		{
			AQueryClause aQueryClause = yyVals[-1 + yyTop] as AQueryClause;
			aQueryClause.Tail.Next = (AQueryClause)yyVals[0 + yyTop];
			yyVal = aQueryClause;
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
		}

		private void case_1018()
		{
			lexer.query_parsing = false;
			yyVal = yyVals[-1 + yyTop];
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
		}

		private void case_1019()
		{
			yyVal = yyVals[-1 + yyTop];
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
		}

		private void case_1020()
		{
			current_block = new QueryBlock(current_block, lexer.Location);
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			RangeVariable identifier = new RangeVariable(locatedToken.Value, locatedToken.Location);
			QueryStartClause start = new QueryStartClause((QueryBlock)current_block, (Expression)yyVals[0 + yyTop], identifier, GetLocation(yyVals[-3 + yyTop]));
			yyVal = new QueryExpression(start);
		}

		private void case_1021()
		{
			current_block = new QueryBlock(current_block, lexer.Location);
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			RangeVariable identifier = new RangeVariable(locatedToken.Value, locatedToken.Location);
			QueryStartClause start = new QueryStartClause((QueryBlock)current_block, (Expression)yyVals[0 + yyTop], identifier, GetLocation(yyVals[-4 + yyTop]))
			{
				IdentifierType = (FullNamedExpression)yyVals[-3 + yyTop]
			};
			yyVal = new QueryExpression(start);
		}

		private void case_1022()
		{
			current_block = new QueryBlock(current_block, lexer.Location);
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			RangeVariable identifier = new RangeVariable(locatedToken.Value, locatedToken.Location);
			QueryStartClause start = new QueryStartClause((QueryBlock)current_block, (Expression)yyVals[0 + yyTop], identifier, GetLocation(yyVals[-3 + yyTop]));
			yyVal = new QueryExpression(start);
		}

		private void case_1023()
		{
			current_block = new QueryBlock(current_block, lexer.Location);
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			RangeVariable identifier = new RangeVariable(locatedToken.Value, locatedToken.Location);
			QueryStartClause start = new QueryStartClause((QueryBlock)current_block, (Expression)yyVals[0 + yyTop], identifier, GetLocation(yyVals[-4 + yyTop]))
			{
				IdentifierType = (FullNamedExpression)yyVals[-3 + yyTop]
			};
			yyVal = new QueryExpression(start);
		}

		private void case_1025()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			RangeVariable rangeVariable = new RangeVariable(locatedToken.Value, locatedToken.Location);
			yyVal = new SelectMany((QueryBlock)current_block, rangeVariable, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-4 + yyTop]));
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
			((QueryBlock)current_block).AddRangeVariable(rangeVariable);
		}

		private void case_1027()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			RangeVariable rangeVariable = new RangeVariable(locatedToken.Value, locatedToken.Location);
			yyVal = new SelectMany((QueryBlock)current_block, rangeVariable, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-5 + yyTop]))
			{
				IdentifierType = (FullNamedExpression)yyVals[-4 + yyTop]
			};
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
			((QueryBlock)current_block).AddRangeVariable(rangeVariable);
		}

		private void case_1028()
		{
			AQueryClause aQueryClause = (AQueryClause)yyVals[-1 + yyTop];
			if (yyVals[0 + yyTop] != null)
			{
				aQueryClause.Next = (AQueryClause)yyVals[0 + yyTop];
			}
			if (yyVals[-2 + yyTop] != null)
			{
				AQueryClause obj = (AQueryClause)yyVals[-2 + yyTop];
				obj.Tail.Next = aQueryClause;
				aQueryClause = obj;
			}
			yyVal = aQueryClause;
		}

		private void case_1029()
		{
			AQueryClause next = (AQueryClause)yyVals[0 + yyTop];
			if (yyVals[-1 + yyTop] != null)
			{
				AQueryClause obj = (AQueryClause)yyVals[-1 + yyTop];
				obj.Tail.Next = next;
				next = obj;
			}
			yyVal = next;
		}

		private void case_1031()
		{
			report.Error(742, GetLocation(yyVals[0 + yyTop]), "Unexpected symbol `{0}'. A query body must end with select or group clause", GetSymbolName(yyToken));
			yyVal = yyVals[-1 + yyTop];
		}

		private void case_1032()
		{
			Error_SyntaxError(yyToken);
			yyVal = null;
		}

		private void case_1034()
		{
			yyVal = new Select((QueryBlock)current_block, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-2 + yyTop]));
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
		}

		private void case_1035()
		{
			if (linq_clause_blocks == null)
			{
				linq_clause_blocks = new Stack<QueryBlock>();
			}
			current_block = new QueryBlock(current_block, lexer.Location);
			linq_clause_blocks.Push((QueryBlock)current_block);
		}

		private void case_1036()
		{
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
			current_block = new QueryBlock(current_block, lexer.Location);
		}

		private void case_1037()
		{
			object[] array = (object[])yyVals[0 + yyTop];
			yyVal = new GroupBy((QueryBlock)current_block, (Expression)yyVals[-2 + yyTop], linq_clause_blocks.Pop(), (Expression)array[0], GetLocation(yyVals[-4 + yyTop]));
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
		}

		private void case_1039()
		{
			Error_SyntaxError(yyToken);
			yyVal = new object[2]
			{
				null,
				Location.Null
			};
		}

		private void case_1041()
		{
			((AQueryClause)yyVals[-1 + yyTop]).Tail.Next = (AQueryClause)yyVals[0 + yyTop];
			yyVal = yyVals[-1 + yyTop];
		}

		private void case_1048()
		{
			LocatedToken locatedToken = (LocatedToken)yyVals[-3 + yyTop];
			RangeVariable rangeVariable = new RangeVariable(locatedToken.Value, locatedToken.Location);
			yyVal = new Let((QueryBlock)current_block, rangeVariable, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-4 + yyTop]));
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
			((QueryBlock)current_block).AddRangeVariable(rangeVariable);
		}

		private void case_1050()
		{
			yyVal = new Where((QueryBlock)current_block, (Expression)yyVals[0 + yyTop], GetLocation(yyVals[-2 + yyTop]));
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
		}

		private void case_1051()
		{
			if (linq_clause_blocks == null)
			{
				linq_clause_blocks = new Stack<QueryBlock>();
			}
			current_block = new QueryBlock(current_block, lexer.Location);
			linq_clause_blocks.Push((QueryBlock)current_block);
		}

		private void case_1052()
		{
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
			current_block = new QueryBlock(current_block, lexer.Location);
			linq_clause_blocks.Push((QueryBlock)current_block);
		}

		private void case_1053()
		{
			current_block.AddStatement(new ContextualReturn((Expression)yyVals[-1 + yyTop]));
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
			current_block = new QueryBlock(current_block, lexer.Location);
		}

		private void case_1054()
		{
			current_block.AddStatement(new ContextualReturn((Expression)yyVals[-1 + yyTop]));
			current_block.SetEndLocation(lexer.Location);
			QueryBlock outerSelector = linq_clause_blocks.Pop();
			QueryBlock queryBlock = linq_clause_blocks.Pop();
			LocatedToken locatedToken = (LocatedToken)yyVals[-10 + yyTop];
			RangeVariable rangeVariable = new RangeVariable(locatedToken.Value, locatedToken.Location);
			RangeVariable rangeVariable2;
			if (yyVals[0 + yyTop] == null)
			{
				rangeVariable2 = rangeVariable;
				yyVal = new Join(queryBlock, rangeVariable, (Expression)yyVals[-7 + yyTop], outerSelector, (QueryBlock)current_block, GetLocation(yyVals[-11 + yyTop]));
			}
			else
			{
				Block parent = queryBlock.Parent;
				while (parent is QueryBlock)
				{
					parent = parent.Parent;
				}
				current_block.Parent = parent;
				((QueryBlock)current_block).AddRangeVariable(rangeVariable);
				locatedToken = (LocatedToken)yyVals[0 + yyTop];
				rangeVariable2 = new RangeVariable(locatedToken.Value, locatedToken.Location);
				yyVal = new GroupJoin(queryBlock, rangeVariable, (Expression)yyVals[-7 + yyTop], outerSelector, (QueryBlock)current_block, rangeVariable2, GetLocation(yyVals[-11 + yyTop]));
			}
			current_block = queryBlock.Parent;
			((QueryBlock)current_block).AddRangeVariable(rangeVariable2);
		}

		private void case_1055()
		{
			if (linq_clause_blocks == null)
			{
				linq_clause_blocks = new Stack<QueryBlock>();
			}
			current_block = new QueryBlock(current_block, lexer.Location);
			linq_clause_blocks.Push((QueryBlock)current_block);
		}

		private void case_1056()
		{
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
			current_block = new QueryBlock(current_block, lexer.Location);
			linq_clause_blocks.Push((QueryBlock)current_block);
		}

		private void case_1057()
		{
			current_block.AddStatement(new ContextualReturn((Expression)yyVals[-1 + yyTop]));
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
			current_block = new QueryBlock(current_block, lexer.Location);
		}

		private void case_1058()
		{
			current_block.AddStatement(new ContextualReturn((Expression)yyVals[-1 + yyTop]));
			current_block.SetEndLocation(lexer.Location);
			QueryBlock outerSelector = linq_clause_blocks.Pop();
			QueryBlock queryBlock = linq_clause_blocks.Pop();
			LocatedToken locatedToken = (LocatedToken)yyVals[-10 + yyTop];
			RangeVariable rangeVariable = new RangeVariable(locatedToken.Value, locatedToken.Location);
			RangeVariable rangeVariable2;
			if (yyVals[0 + yyTop] == null)
			{
				rangeVariable2 = rangeVariable;
				yyVal = new Join(queryBlock, rangeVariable, (Expression)yyVals[-7 + yyTop], outerSelector, (QueryBlock)current_block, GetLocation(yyVals[-12 + yyTop]))
				{
					IdentifierType = (FullNamedExpression)yyVals[-11 + yyTop]
				};
			}
			else
			{
				Block parent = queryBlock.Parent;
				while (parent is QueryBlock)
				{
					parent = parent.Parent;
				}
				current_block.Parent = parent;
				((QueryBlock)current_block).AddRangeVariable(rangeVariable);
				locatedToken = (LocatedToken)yyVals[0 + yyTop];
				rangeVariable2 = new RangeVariable(locatedToken.Value, locatedToken.Location);
				yyVal = new GroupJoin(queryBlock, rangeVariable, (Expression)yyVals[-7 + yyTop], outerSelector, (QueryBlock)current_block, rangeVariable2, GetLocation(yyVals[-12 + yyTop]))
				{
					IdentifierType = (FullNamedExpression)yyVals[-11 + yyTop]
				};
			}
			current_block = queryBlock.Parent;
			((QueryBlock)current_block).AddRangeVariable(rangeVariable2);
		}

		private void case_1062()
		{
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
			yyVal = yyVals[0 + yyTop];
		}

		private void case_1064()
		{
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
			current_block = new QueryBlock(current_block, lexer.Location);
		}

		private void case_1065()
		{
			((AQueryClause)yyVals[-3 + yyTop]).Next = (AQueryClause)yyVals[0 + yyTop];
			yyVal = yyVals[-3 + yyTop];
		}

		private void case_1067()
		{
			current_block.SetEndLocation(lexer.Location);
			current_block = current_block.Parent;
			current_block = new QueryBlock((QueryBlock)current_block, lexer.Location);
		}

		private void case_1068()
		{
			((AQueryClause)yyVals[-3 + yyTop]).Tail.Next = (AQueryClause)yyVals[0 + yyTop];
			yyVal = yyVals[-3 + yyTop];
		}

		private void case_1070()
		{
			yyVal = new OrderByAscending((QueryBlock)current_block, (Expression)yyVals[-1 + yyTop]);
		}

		private void case_1071()
		{
			yyVal = new OrderByDescending((QueryBlock)current_block, (Expression)yyVals[-1 + yyTop]);
		}

		private void case_1073()
		{
			yyVal = new ThenByAscending((QueryBlock)current_block, (Expression)yyVals[-1 + yyTop]);
		}

		private void case_1074()
		{
			yyVal = new ThenByDescending((QueryBlock)current_block, (Expression)yyVals[-1 + yyTop]);
		}

		private void case_1076()
		{
			current_block.SetEndLocation(GetLocation(yyVals[-1 + yyTop]));
			current_block = current_block.Parent;
			current_block = new QueryBlock(current_block, lexer.Location);
			if (linq_clause_blocks == null)
			{
				linq_clause_blocks = new Stack<QueryBlock>();
			}
			linq_clause_blocks.Push((QueryBlock)current_block);
		}

		private void case_1077()
		{
			QueryBlock block = linq_clause_blocks.Pop();
			LocatedToken locatedToken = (LocatedToken)yyVals[-2 + yyTop];
			RangeVariable identifier = new RangeVariable(locatedToken.Value, locatedToken.Location);
			yyVal = new QueryStartClause(block, null, identifier, GetLocation(yyVals[-3 + yyTop]))
			{
				next = (AQueryClause)yyVals[0 + yyTop]
			};
		}

		private void case_1080()
		{
			current_container = (current_type = new Class(current_container, new MemberName("<InteractiveExpressionClass>"), Modifiers.PUBLIC, null));
			ParametersCompiled parameters = new ParametersCompiled(new Parameter(new TypeExpression(compiler.BuiltinTypes.Object, Location.Null), "$retval", Parameter.Modifier.REF, null, Location.Null));
			Modifiers modifiers = Modifiers.PUBLIC | Modifiers.STATIC;
			if (settings.Unsafe)
			{
				modifiers |= Modifiers.UNSAFE;
			}
			current_local_parameters = parameters;
			InteractiveMethod interactiveMethod = new InteractiveMethod(current_type, new TypeExpression(compiler.BuiltinTypes.Void, Location.Null), modifiers, parameters);
			current_type.AddMember(interactiveMethod);
			oob_stack.Push(interactiveMethod);
			interactive_async = false;
			lexer.parsing_block++;
			start_block(lexer.Location);
		}

		private void case_1081()
		{
			lexer.parsing_block--;
			InteractiveMethod interactiveMethod = (InteractiveMethod)oob_stack.Pop();
			interactiveMethod.Block = (ToplevelBlock)end_block(lexer.Location);
			if (interactive_async == true)
			{
				interactiveMethod.ChangeToAsync();
			}
			InteractiveResult = (Class)pop_current_class();
			current_local_parameters = null;
		}

		private void case_1091()
		{
			module.DocumentationBuilder.ParsedBuiltinType = (TypeExpression)yyVals[-1 + yyTop];
			module.DocumentationBuilder.ParsedParameters = (List<DocumentationParameter>)yyVals[0 + yyTop];
			yyVal = null;
		}

		private void case_1092()
		{
			module.DocumentationBuilder.ParsedBuiltinType = new TypeExpression(compiler.BuiltinTypes.Void, GetLocation(yyVals[-1 + yyTop]));
			module.DocumentationBuilder.ParsedParameters = (List<DocumentationParameter>)yyVals[0 + yyTop];
			yyVal = null;
		}

		private void case_1093()
		{
			module.DocumentationBuilder.ParsedBuiltinType = (TypeExpression)yyVals[-3 + yyTop];
			module.DocumentationBuilder.ParsedParameters = (List<DocumentationParameter>)yyVals[0 + yyTop];
			LocatedToken locatedToken = (LocatedToken)yyVals[-1 + yyTop];
			yyVal = new MemberName(locatedToken.Value);
		}

		private void case_1096()
		{
			module.DocumentationBuilder.ParsedParameters = (List<DocumentationParameter>)yyVals[-1 + yyTop];
			yyVal = new MemberName((MemberName)yyVals[-6 + yyTop], MemberCache.IndexerNameAlias, Location.Null);
		}

		private void case_1097()
		{
			List<DocumentationParameter> list = ((List<DocumentationParameter>)yyVals[0 + yyTop]) ?? new List<DocumentationParameter>(1);
			list.Add(new DocumentationParameter((FullNamedExpression)yyVals[-1 + yyTop]));
			module.DocumentationBuilder.ParsedParameters = list;
			module.DocumentationBuilder.ParsedOperator = Operator.OpType.Explicit;
			yyVal = null;
		}

		private void case_1098()
		{
			List<DocumentationParameter> list = ((List<DocumentationParameter>)yyVals[0 + yyTop]) ?? new List<DocumentationParameter>(1);
			list.Add(new DocumentationParameter((FullNamedExpression)yyVals[-1 + yyTop]));
			module.DocumentationBuilder.ParsedParameters = list;
			module.DocumentationBuilder.ParsedOperator = Operator.OpType.Implicit;
			yyVal = null;
		}

		private void case_1099()
		{
			List<DocumentationParameter> parsedParameters = (List<DocumentationParameter>)yyVals[0 + yyTop];
			module.DocumentationBuilder.ParsedParameters = parsedParameters;
			module.DocumentationBuilder.ParsedOperator = (Operator.OpType)yyVals[-1 + yyTop];
			yyVal = null;
		}

		private void case_1107()
		{
			List<DocumentationParameter> list = new List<DocumentationParameter>();
			list.Add((DocumentationParameter)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_1108()
		{
			List<DocumentationParameter> list = yyVals[-2 + yyTop] as List<DocumentationParameter>;
			list.Add((DocumentationParameter)yyVals[0 + yyTop]);
			yyVal = list;
		}

		private void case_1109()
		{
			if (yyVals[-1 + yyTop] != null)
			{
				yyVal = new DocumentationParameter((Parameter.Modifier)yyVals[-1 + yyTop], (FullNamedExpression)yyVals[0 + yyTop]);
			}
			else
			{
				yyVal = new DocumentationParameter((FullNamedExpression)yyVals[0 + yyTop]);
			}
		}

		private void Error_ExpectingTypeName(Expression expr)
		{
			if (expr is Invocation)
			{
				report.Error(1002, expr.Location, "Expecting `;'");
			}
			else
			{
				expr.Error_InvalidExpressionStatement(report);
			}
		}

		private void Error_ParameterModifierNotValid(string modifier, Location loc)
		{
			report.Error(631, loc, "The parameter modifier `{0}' is not valid in this context", modifier);
		}

		private void Error_DuplicateParameterModifier(Location loc, Parameter.Modifier mod)
		{
			report.Error(1107, loc, "Duplicate parameter modifier `{0}'", Parameter.GetModifierSignature(mod));
		}

		private void Error_TypeExpected(Location loc)
		{
			report.Error(1031, loc, "Type expected");
		}

		private void Error_UnsafeCodeNotAllowed(Location loc)
		{
			report.Error(227, loc, "Unsafe code requires the `unsafe' command line option to be specified");
		}

		private void Warning_EmptyStatement(Location loc)
		{
			report.Warning(642, 3, loc, "Possible mistaken empty statement");
		}

		private void Error_NamedArgumentExpected(NamedArgument a)
		{
			report.Error(1738, a.Location, "Named arguments must appear after the positional arguments");
		}

		private void Error_MissingInitializer(Location loc)
		{
			report.Error(210, loc, "You must provide an initializer in a fixed or using statement declaration");
		}

		private object Error_AwaitAsIdentifier(object token)
		{
			if (async_block)
			{
				report.Error(4003, GetLocation(token), "`await' cannot be used as an identifier within an async method or lambda expression");
				return new LocatedToken("await", GetLocation(token));
			}
			return token;
		}

		private void push_current_container(TypeDefinition tc, object partial_token)
		{
			if (module.Evaluator != null)
			{
				TypeSpec definition = tc.Definition;
				Modifiers modifiers = (tc.ModFlags = (tc.ModFlags & ~Modifiers.AccessibilityMask) | Modifiers.PUBLIC);
				definition.Modifiers = modifiers;
				if (undo == null)
				{
					undo = new Undo();
				}
				undo.AddTypeContainer(current_container, tc);
			}
			if (partial_token != null)
			{
				current_container.AddPartial(tc);
			}
			else
			{
				current_container.AddTypeContainer(tc);
			}
			lexer.parsing_declaration++;
			current_container = tc;
			current_type = tc;
		}

		private TypeContainer pop_current_class()
		{
			TypeContainer result = current_container;
			current_container = current_container.Parent;
			current_type = current_type.Parent as TypeDefinition;
			return result;
		}

		[Conditional("FULL_AST")]
		private void StoreModifierLocation(object token, Location loc)
		{
			if (lbag != null)
			{
				if (mod_locations == null)
				{
					mod_locations = new List<Tuple<Modifiers, Location>>();
				}
				mod_locations.Add(Tuple.Create((Modifiers)token, loc));
			}
		}

		[Conditional("FULL_AST")]
		private void PushLocation(Location loc)
		{
			if (location_stack == null)
			{
				location_stack = new Stack<Location>();
			}
			location_stack.Push(loc);
		}

		private Location PopLocation()
		{
			if (location_stack == null)
			{
				return Location.Null;
			}
			return location_stack.Pop();
		}

		private string CheckAttributeTarget(int token, string a, Location l)
		{
			switch (a)
			{
			case "assembly":
			case "module":
			case "field":
			case "method":
			case "param":
			case "property":
			case "type":
				return a;
			default:
				if (!Tokenizer.IsValidIdentifier(a))
				{
					Error_SyntaxError(token);
				}
				else
				{
					report.Warning(658, 1, l, "`{0}' is invalid attribute target. All attributes in this attribute section will be ignored", a);
				}
				return string.Empty;
			}
		}

		private static bool IsUnaryOperator(Operator.OpType op)
		{
			switch (op)
			{
			case Operator.OpType.LogicalNot:
			case Operator.OpType.OnesComplement:
			case Operator.OpType.Increment:
			case Operator.OpType.Decrement:
			case Operator.OpType.True:
			case Operator.OpType.False:
			case Operator.OpType.UnaryPlus:
			case Operator.OpType.UnaryNegation:
				return true;
			default:
				return false;
			}
		}

		private void syntax_error(Location l, string msg)
		{
			report.Error(1003, l, "Syntax error, " + msg);
		}

		public CSharpParser(SeekableStreamReader reader, CompilationSourceFile file, ParserSession session)
			: this(reader, file, file.Compiler.Report, session)
		{
		}

		public CSharpParser(SeekableStreamReader reader, CompilationSourceFile file, Report report, ParserSession session)
		{
			this.file = file;
			current_container = (current_namespace = file);
			module = file.Module;
			compiler = file.Compiler;
			settings = compiler.Settings;
			this.report = report;
			lang_version = settings.Version;
			yacc_verbose_flag = settings.VerboseParserFlag;
			doc_support = settings.DocumentationFile != null;
			lexer = new Tokenizer(reader, file, session, report);
			oob_stack = new Stack<object>();
			lbag = session.LocationsBag;
			use_global_stacks = session.UseJayGlobalArrays;
			parameters_bucket = session.ParametersStack;
		}

		public void parse()
		{
			eof_token = 257;
			try
			{
				if (yacc_verbose_flag > 1)
				{
					yyparse(lexer, new yyDebugSimple());
				}
				else
				{
					yyparse(lexer);
				}
				lexer.cleanup();
			}
			catch (Exception ex)
			{
				if (ex is yyUnexpectedEof)
				{
					Error_SyntaxError(yyToken);
					UnexpectedEOF = true;
					return;
				}
				if (ex is yyException)
				{
					if (report.Errors == 0)
					{
						report.Error(-25, lexer.Location, "Parsing error");
					}
					return;
				}
				if (yacc_verbose_flag > 0 || ex is FatalException)
				{
					throw;
				}
				report.Error(589, lexer.Location, "Internal compiler error during parsing" + ex);
			}
		}

		private void CheckToken(int error, int yyToken, string msg, Location loc)
		{
			if (yyToken >= 260 && yyToken <= 370)
			{
				report.Error(error, loc, "{0}: `{1}' is a keyword", msg, GetTokenName(yyToken));
			}
			else
			{
				report.Error(error, loc, msg);
			}
		}

		private string ConsumeStoredComment()
		{
			string result = tmpComment;
			tmpComment = null;
			Lexer.doc_state = XmlCommentState.Allowed;
			return result;
		}

		private void FeatureIsNotAvailable(Location loc, string feature)
		{
			report.FeatureIsNotAvailable(compiler, loc, feature);
		}

		private Location GetLocation(object obj)
		{
			return (obj as LocatedToken)?.Location ?? (obj as MemberName)?.Location ?? (obj as Expression)?.Location ?? lexer.Location;
		}

		private void start_block(Location loc)
		{
			if (current_block == null)
			{
				current_block = new ToplevelBlock(compiler, current_local_parameters, loc);
				parsing_anonymous_method = false;
			}
			else if (parsing_anonymous_method)
			{
				current_block = new ParametersBlock(current_block, current_local_parameters, loc);
				parsing_anonymous_method = false;
			}
			else
			{
				current_block = new ExplicitBlock(current_block, loc, Location.Null);
			}
		}

		private Block end_block(Location loc)
		{
			Block block = current_block.Explicit;
			block.SetEndLocation(loc);
			current_block = block.Parent;
			return block;
		}

		private void start_anonymous(bool isLambda, ParametersCompiled parameters, bool isAsync, Location loc)
		{
			oob_stack.Push(current_anonymous_method);
			oob_stack.Push(current_local_parameters);
			oob_stack.Push(current_variable);
			oob_stack.Push(async_block);
			current_local_parameters = parameters;
			if (isLambda)
			{
				if (lang_version <= LanguageVersion.ISO_2)
				{
					FeatureIsNotAvailable(loc, "lambda expressions");
				}
				current_anonymous_method = new LambdaExpression(loc);
			}
			else
			{
				if (lang_version == LanguageVersion.ISO_1)
				{
					FeatureIsNotAvailable(loc, "anonymous methods");
				}
				current_anonymous_method = new AnonymousMethodExpression(loc);
			}
			async_block = isAsync;
			parsing_anonymous_method = true;
		}

		private AnonymousMethodExpression end_anonymous(ParametersBlock anon_block)
		{
			if (async_block)
			{
				anon_block.IsAsync = true;
			}
			current_anonymous_method.Block = anon_block;
			AnonymousMethodExpression result = current_anonymous_method;
			async_block = (bool)oob_stack.Pop();
			current_variable = (BlockVariable)oob_stack.Pop();
			current_local_parameters = (ParametersCompiled)oob_stack.Pop();
			current_anonymous_method = (AnonymousMethodExpression)oob_stack.Pop();
			return result;
		}

		private void Error_SyntaxError(int token)
		{
			Error_SyntaxError(0, token);
		}

		private void Error_SyntaxError(int error_code, int token)
		{
			Error_SyntaxError(error_code, token, "Unexpected symbol");
		}

		private void Error_SyntaxError(int error_code, int token, string msg)
		{
			Lexer.CompleteOnEOF = false;
			switch (token)
			{
			case 259:
				return;
			case 421:
				if (lexer.Location.Column == 0)
				{
					return;
				}
				break;
			}
			string symbolName = GetSymbolName(token);
			string text = GetExpecting();
			Location loc = lexer.Location - symbolName.Length;
			if (error_code == 0)
			{
				if (!(text == "`identifier'"))
				{
					error_code = ((!(text == "`)'")) ? 1525 : 1026);
				}
				else
				{
					if (token > 260 && token < 370)
					{
						report.Error(1041, loc, "Identifier expected, `{0}' is a keyword", symbolName);
						return;
					}
					error_code = 1001;
					text = "identifier";
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				report.Error(error_code, loc, "{1} `{0}'", symbolName, msg);
				return;
			}
			report.Error(error_code, loc, "{2} `{0}', expecting {1}", symbolName, text, msg);
		}

		private string GetExpecting()
		{
			int[] array = yyExpectingTokens(yyExpectingState);
			List<string> list = new List<string>(array.Length);
			bool flag = false;
			bool flag2 = false;
			foreach (int num in array)
			{
				flag2 |= num == 422;
				string tokenName = GetTokenName(num);
				if (!(tokenName == "<internal>"))
				{
					flag |= tokenName == "type";
					if (!list.Contains(tokenName))
					{
						list.Add(tokenName);
					}
				}
			}
			if (list.Count > 8)
			{
				return null;
			}
			if (flag & flag2)
			{
				list.Remove("identifier");
			}
			if (list.Count == 1)
			{
				return "`" + GetTokenName(array[0]) + "'";
			}
			StringBuilder stringBuilder = new StringBuilder();
			list.Sort();
			int count = list.Count;
			for (int j = 0; j < count; j++)
			{
				bool flag3 = j + 1 == count;
				if (flag3)
				{
					stringBuilder.Append("or ");
				}
				stringBuilder.Append('`');
				stringBuilder.Append(list[j]);
				stringBuilder.Append(flag3 ? "'" : ((count < 3) ? "' " : "', "));
			}
			return stringBuilder.ToString();
		}

		private string GetSymbolName(int token)
		{
			switch (token)
			{
			case 421:
				return ((Constant)lexer.Value).GetValue().ToString();
			case 422:
				return ((LocatedToken)lexer.Value).Value;
			case 265:
				return "bool";
			case 267:
				return "byte";
			case 270:
				return "char";
			case 337:
				return "void";
			case 275:
				return "decimal";
			case 279:
				return "double";
			case 288:
				return "float";
			case 295:
				return "int";
			case 300:
				return "long";
			case 316:
				return "sbyte";
			case 318:
				return "short";
			case 322:
				return "string";
			case 330:
				return "uint";
			case 331:
				return "ulong";
			case 334:
				return "ushort";
			case 304:
				return "object";
			case 382:
				return "+";
			case 383:
			case 434:
				return "-";
			case 384:
				return "!";
			case 388:
				return "&";
			case 389:
				return "|";
			case 390:
				return "*";
			case 391:
				return "%";
			case 392:
				return "/";
			case 393:
				return "^";
			case 396:
				return "++";
			case 397:
				return "--";
			case 398:
				return "<<";
			case 399:
				return ">>";
			case 386:
				return "<";
			case 387:
				return ">";
			case 400:
				return "<=";
			case 401:
				return ">=";
			case 402:
				return "==";
			case 403:
				return "!=";
			case 404:
				return "&&";
			case 405:
				return "||";
			case 416:
				return "->";
			case 417:
				return "??";
			case 406:
				return "*=";
			case 407:
				return "/=";
			case 408:
				return "%=";
			case 409:
				return "+=";
			case 410:
				return "-=";
			case 411:
				return "<<=";
			case 412:
				return ">>=";
			case 413:
				return "&=";
			case 414:
				return "^=";
			case 415:
				return "|=";
			default:
				return GetTokenName(token);
			}
		}

		private static string GetTokenName(int token)
		{
			switch (token)
			{
			case 261:
				return "abstract";
			case 262:
				return "as";
			case 263:
				return "add";
			case 362:
				return "async";
			case 264:
				return "base";
			case 266:
				return "break";
			case 268:
				return "case";
			case 269:
				return "catch";
			case 271:
				return "checked";
			case 272:
				return "class";
			case 273:
				return "const";
			case 274:
				return "continue";
			case 276:
				return "default";
			case 277:
				return "delegate";
			case 278:
				return "do";
			case 280:
				return "else";
			case 281:
				return "enum";
			case 282:
				return "event";
			case 283:
				return "explicit";
			case 284:
			case 358:
				return "extern";
			case 285:
				return "false";
			case 286:
				return "finally";
			case 287:
				return "fixed";
			case 289:
				return "for";
			case 290:
				return "foreach";
			case 291:
				return "goto";
			case 292:
				return "if";
			case 293:
				return "implicit";
			case 294:
				return "in";
			case 296:
				return "interface";
			case 297:
				return "internal";
			case 298:
				return "is";
			case 299:
				return "lock";
			case 301:
				return "namespace";
			case 302:
				return "new";
			case 303:
				return "null";
			case 305:
				return "operator";
			case 306:
				return "out";
			case 307:
				return "override";
			case 308:
				return "params";
			case 309:
				return "private";
			case 310:
				return "protected";
			case 311:
				return "public";
			case 312:
				return "readonly";
			case 313:
				return "ref";
			case 314:
				return "return";
			case 315:
				return "remove";
			case 317:
				return "sealed";
			case 319:
				return "sizeof";
			case 320:
				return "stackalloc";
			case 321:
				return "static";
			case 323:
				return "struct";
			case 324:
				return "switch";
			case 325:
				return "this";
			case 326:
				return "throw";
			case 327:
				return "true";
			case 328:
				return "try";
			case 329:
				return "typeof";
			case 332:
				return "unchecked";
			case 333:
				return "unsafe";
			case 335:
				return "using";
			case 336:
				return "virtual";
			case 338:
				return "volatile";
			case 339:
				return "where";
			case 340:
				return "while";
			case 341:
				return "__arglist";
			case 359:
				return "__refvalue";
			case 360:
				return "__reftype";
			case 361:
				return "__makeref";
			case 342:
				return "partial";
			case 343:
				return "=>";
			case 344:
			case 345:
				return "from";
			case 346:
				return "join";
			case 347:
				return "on";
			case 348:
				return "equals";
			case 349:
				return "select";
			case 350:
				return "group";
			case 351:
				return "by";
			case 352:
				return "let";
			case 353:
				return "orderby";
			case 354:
				return "ascending";
			case 355:
				return "descending";
			case 356:
				return "into";
			case 368:
				return "get";
			case 369:
				return "set";
			case 371:
				return "{";
			case 372:
				return "}";
			case 373:
			case 427:
				return "[";
			case 374:
				return "]";
			case 375:
			case 423:
			case 424:
				return "(";
			case 376:
				return ")";
			case 377:
				return ".";
			case 378:
				return ",";
			case 426:
				return "default:";
			case 379:
				return ":";
			case 380:
				return ";";
			case 381:
				return "~";
			case 365:
				return "when";
			case 367:
				return "}";
			case 366:
				return "${";
			case 364:
			case 382:
			case 383:
			case 384:
			case 386:
			case 387:
			case 388:
			case 389:
			case 390:
			case 391:
			case 392:
			case 393:
			case 396:
			case 397:
			case 398:
			case 399:
			case 400:
			case 401:
			case 402:
			case 403:
			case 404:
			case 405:
			case 406:
			case 407:
			case 408:
			case 409:
			case 410:
			case 411:
			case 412:
			case 413:
			case 414:
			case 415:
			case 416:
			case 417:
			case 434:
				return "<operator>";
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
				return "type";
			case 385:
				return "=";
			case 418:
			case 425:
				return "<";
			case 420:
				return ">";
			case 357:
			case 394:
				return "?";
			case 395:
				return "::";
			case 421:
				return "value";
			case 363:
			case 422:
				return "identifier";
			case 257:
				return "end-of-file";
			case 258:
			case 259:
			case 260:
			case 370:
			case 428:
			case 429:
			case 430:
			case 432:
			case 433:
				return "<internal>";
			default:
				return yyNames[token];
			}
		}
	}
}
