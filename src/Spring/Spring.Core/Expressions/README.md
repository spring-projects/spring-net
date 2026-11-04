# Expression parser

`ExpressionGrammar.parlot.cs` is the grammar, a build-only file that the `Parlot.SourceGenerator` analyzer turns into code of
`Spring.Core`. `ExpressionGrammar.cs` has the tokens, the construction of the nodes and the syntax errors. The language is the
one of the former ANTLR 2 grammar, and `ExpressionGrammarTests` holds the trees and errors that parser gave for its quirks in
`ExpressionGrammarGolden.json`.

## Quirks kept on purpose

These decide what is accepted, so they stay:

- A dot between nodes is optional: `a b` is `a.b`, and `1 .a` and `a.` parse.
- `T(` is one token only without a space, and `T(...)` reads any token up to `)`.
- A real number with a fraction has to have a valid exponent after `e`, while an integer doesn't: `1.5ex` fails, `1ex` is `1`
  then `ex`.
- `Ul` is not an integer suffix but `lU` is.
- Keywords can't be property names.
- The relational and power operators don't chain.
- The line of an error is always 1, and its column counts a tab as moving to the next multiple of 8.

## PEG vs LL(2)

A PEG choice can backtrack where ANTLR committed to an alternative after looking at two tokens. These keep the same language:

- Tokens are scanned as ANTLR's lexer did: `^` doesn't match the start of `^{`, `{` doesn't match `{|`, `.` doesn't match `.5`.
  Otherwise `a^{b,c}`, an error, would be read again as `a ^ {b,c}`.
- A name followed by `(` can only be a call, so `a(b;c)` isn't read again as a property followed by an expression list. This is
  `Not(lparen)` in the grammar.
- A name followed by `=` in arguments can only be a named argument: `Not(id.AndSkip(assign))` on the other alternative. Without
  it, nested `new a(b=` takes exponential time.
- `Parse` needs the end of the input, so a failed alternative can't be accepted. `ParsePrimary` doesn't, and checks what follows
  with the follow sets of the ANTLR grammar (`CanFollowPrimary`).

## Layout of the grammar

`Operators()` handles the unary and the left-associative levels. The relational and power levels don't chain, and the third
operand of `?:` is a whole expression, which a table can't express, so these are sequences. `Parse`, `ParsePrimary` and
`ParseLambda` share one generated parser through a configuration parameter and `If`, as generating one each would triple the
code. `ParseProperty` is a scan by hand.
