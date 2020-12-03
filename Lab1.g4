grammar Lab1;


/*
 * Parser Rules
 */

compileUnit : expression EOF;

expression :
	'-' expression  #UnminExpr
	|LPAREN expression RPAREN #ParenthesizedExpr
	|expression EXPONENT expression #ExponentialExpr
    |expression operatorToken=(MULTIPLY | DIVIDE) expression #MultiplicativeExpr
	|expression operatorToken=(ADD | SUBTRACT) expression #AdditiveExpr
	|operatorToken= (MAX | MIN) LPAREN expression DESP expression RPAREN #MaxMinExpr
	|operatorToken= (MMAX | MMIN) LPAREN expression (DESP expression)* RPAREN #MmaxMminExpr
	|NUMBER #NumberExpr
	|IDENTIFIER #IdentifierExpr
	; 

/*
 * Lexer Rules
 */

NUMBER : INT (',' INT)?; 
IDENTIFIER : [A-Z]+([0-9]+);

INT : ('0'..'9')+;


EXPONENT : '^';
MULTIPLY : '*';
DIVIDE : '/';
SUBTRACT : '-';
ADD : '+';
LPAREN : '(';
RPAREN : ')';
MAX : 'max';
MIN : 'min';
MMAX : 'mmax';
MMIN : 'mmin';
DESP : ';';


WS : [ \t\r\n] -> channel(HIDDEN);
