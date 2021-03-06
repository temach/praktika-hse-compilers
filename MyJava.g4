grammar MyJava;

options {
    language = CSharp;
}

// starting point for parsing a java file
compilationUnit
    :   typeDeclaration* EOF
    ;

typeDeclaration
    :   classOrInterfaceModifier* classDeclaration
    |   ';'
    ;

classOrInterfaceModifier
    :   (   'public'     // class or interface
        |   'private'    // class or interface
        |   'static'     // class or interface
        )+
    ;

classDeclaration
    :   'class' Identifier classBody
    ;

classBody
    :   '{' classBodyDeclaration* '}'
    ;

classBodyDeclaration
    :   ';'
    |   classOrInterfaceModifier* memberDeclaration
    ;

memberDeclaration
    :   methodDeclaration                           # methodMemberDecl
    |   fieldDeclaration                            # fieldMemberDecl
    // this is for nested classes
    |   classDeclaration                            # classMemberDecl
    ;

// METHODS
methodDeclaration
    :   (type|'void') Identifier formalParameters ('[' ']')*
        ('throws' qualifiedNameList)?
        (   methodBody
        |   ';'
        )
    ;

// FIELDS
fieldDeclaration
    :   type variableDeclarator ';'
    ;

variableDeclarator
    :   variableDeclaratorId ('=' variableInitializer)?
    ;

variableInitializer
    :   arrayInitializer
    |   expression
    ;

arrayInitializer
    :   '{' (variableInitializer (',' variableInitializer)* (',')? )? '}'
    ;

variableDeclaratorId
    :   Identifier ('[' ']')*
    ;

// TYPES
type
    :   classOrInterfaceType ('[' ']')*
    |   primitiveType ('[' ']')*
    ;

classOrInterfaceType
    :   Identifier ('.' Identifier )*
    ;

primitiveType
    :   'boolean'
    |   'char'
    ;

qualifiedNameList
    :   qualifiedName (',' qualifiedName)*
    ;

formalParameters
    :   '(' formalParameterList? ')'
    ;

formalParameterList
    :   formalParameter (',' formalParameter)* (',' lastFormalParameter)?
    |   lastFormalParameter
    ;

formalParameter
    :   type variableDeclaratorId
    ;

lastFormalParameter
    :   type '...' variableDeclaratorId
    ;

methodBody
    :   block
    ;

qualifiedName
    :   Identifier ('.' Identifier)*
    ;

literal
    :   CharacterLiteral
    |   BooleanLiteral
    |   'null'
    ;

// STATEMENTS / BLOCKS

block
    :   '{' blockStatement* '}'
    ;

blockStatement
    :   localVariableDeclaration
    |   statement
    |   typeDeclaration
    ;

localVariableDeclaration
    :   type variableDeclarator ';'
    ;

// FOR-EACH loop in java
statement
    :   block
    |   'for' '(' enhancedForControl ')' statement
    |   ';'
    |   expression ';'
    ;

enhancedForControl
    :   type variableDeclaratorId ':' expression
    ;

// EXPRESSIONS

expressionList
    :   expression (',' expression)*
    ;

expression
    :   primary
    |   expression '.' Identifier
    |   expression '.' 'this'
    |   expression '[' expression ']'
    |   expression '(' expressionList? ')'
    |   'new' creator
    |   expression ('++' | '--')
    |   ('+'|'-'|'++'|'--') expression
    |   ('~'|'!') expression
    |   expression ('*'|'/'|'%') expression
    |   expression ('+'|'-') expression
    |   expression ('<=' | '>=' | '>' | '<') expression
    |   expression ('==' | '!=') expression
    |   expression '&' expression
    |   expression '^' expression
    |   expression '|' expression
    |   expression '&&' expression
    |   expression '||' expression
    |   expression '?' expression ':' expression
    |   <assoc=right> expression
        (   '='
        |   '+='
        |   '-='
        |   '*='
        |   '/='
        |   '&='
        |   '|='
        |   '^='
        |   '>>='
        |   '>>>='
        |   '<<='
        |   '%='
        )
        expression
    ;

primary
    :   '(' expression ')'
    |   'this'
    |   literal
    |   Identifier
    ;

// class creation with new keyword
creator
    :   createdName (classCreatorRest)
    ;

createdName
    :   Identifier ('.' Identifier )*
    |   primitiveType
    ;

classCreatorRest
    :   arguments classBody?
    ;

arguments
    :   '(' expressionList? ')'
    ;

// LEXER

// §3.9 Keywords

BOOLEAN       : 'boolean';
CHAR          : 'char';
CLASS         : 'class';
FOR           : 'for';
NEW           : 'new';
PRIVATE       : 'private';
PUBLIC        : 'public';
STATIC        : 'static';
VOID          : 'void';

// §3.10.1 Integer Literals
// empty

// §3.10.2 Floating-Point Literals
// empty

// §3.10.3 Boolean Literals

BooleanLiteral
    :   'true'
    |   'false'
    ;

// §3.10.4 Character Literals

CharacterLiteral
    :   '\'' SingleCharacter '\''
    |   '\'' EscapeSequence '\''
    ;

fragment
SingleCharacter
    :   ~['\\]
    ;

// §3.10.5 String Literals
//empty

// §3.10.6 Escape Sequences for Character and String Literals

fragment
EscapeSequence
    :   '\\' [btnfr"'\\]
    ;

// §3.10.7 The Null Literal

NullLiteral
    :   'null'
    ;

// §3.11 Separators

LPAREN          : '(';
RPAREN          : ')';
LBRACE          : '{';
RBRACE          : '}';
LBRACK          : '[';
RBRACK          : ']';
SEMI            : ';';
COMMA           : ',';
DOT             : '.';

// §3.12 Operators

ASSIGN          : '=';
GT              : '>';
LT              : '<';
BANG            : '!';
TILDE           : '~';
QUESTION        : '?';
COLON           : ':';
EQUAL           : '==';
LE              : '<=';
GE              : '>=';
NOTEQUAL        : '!=';
AND             : '&&';
OR              : '||';
INC             : '++';
DEC             : '--';
ADD             : '+';
SUB             : '-';
MUL             : '*';
DIV             : '/';
BITAND          : '&';
BITOR           : '|';
CARET           : '^';
MOD             : '%';


// §3.8 Identifiers (must appear after all keywords in the grammar)

Identifier
    :   JavaLetter JavaLetterOrDigit*
    ;

fragment
JavaLetter
    :   [a-zA-Z$_] // these are the "java letters" below 0x7F
    ;

fragment
JavaLetterOrDigit
    :   [a-zA-Z0-9_] // these are the "java letters or digits" below 0x7F
    ;

//
// Whitespace and comments
//

WS  :  [ \t\r\n\u000C]+ -> skip
    ;

COMMENT
    :   '/*' .*? '*/' -> skip
    ;

LINE_COMMENT
    :   '//' ~[\r\n]* -> skip
    ;

