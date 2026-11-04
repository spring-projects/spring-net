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

## Nesting limits

- **Parser:** about 2,000 levels of nested parentheses, calls or prefix operators (`-`, `+`, `!`) are accepted.
  A deeper input fails with a `SyntaxErrorException` ("nested too deeply"). A small thread stack can make the parser stop
  sooner, with the same exception.
- **Evaluation:** binary operators in a chain (`1+1+1+...`) parse without that limit, but the tree they make is as deep as the
  chain is long. `BaseNode.GetValue` and `SetValue` call `RuntimeHelpers.EnsureSufficientExecutionStack()`, so evaluating a tree
  too deep for the stack of the current thread throws `InsufficientExecutionStackException` instead of ending the process.
- **Property paths** (`a.a.a...`) are not recursive when evaluated: a path of 160,000 nodes parses and evaluates in linear time.

Largest nesting that parses and evaluates on a thread with the given stack size, and the exception one level deeper throws
(Release build, measured on 2026-11-04). The numbers depend on the runtime and the JIT, so read them as orders of magnitude:

| Runtime | Stack | `1+1+...` (chain) | `((...1...))` (parentheses) | `a.a.a...` (path) |
|---|---|---|---|---|
| .NET 10 | 256 KB | 134 (`InsufficientExecutionStackException`) | 31 (`SyntaxErrorException`) | 160,000, no limit |
| .NET 10 | 1 MB | 953 (`InsufficientExecutionStackException`) | 223 (`SyntaxErrorException`) | 160,000, no limit |
| .NET Framework 4.8 | 256 KB | 665 (`InsufficientExecutionStackException`) | 47 (`SyntaxErrorException`) | 160,000, no limit |
| .NET Framework 4.8 | 1 MB | 2,712 (`InsufficientExecutionStackException`) | 175 (`SyntaxErrorException`) | 160,000, no limit |

With a 64 MB stack, parentheses and prefix operators reach the limit of 1,999 levels of the parser on both runtimes, and a
chain of binary operators reaches about 70,000 on .NET 10.

The check costs something on .NET Framework 4.8. Measured on 2026-11-04 with BenchmarkDotNet, for the evaluation alone
(`ExpressionBenchmark.GetValuePrepared`), it adds about 10% to a small expression (120 vs 109 ns), 3% to a medium one
(2.54 vs 2.46 µs) and 14% to a large one (5.62 vs 4.94 µs), with the same allocations. It stays because without it an
expression that is too deep ends the process.

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
