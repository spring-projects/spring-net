# Replace ANTLR with Parlot in the expression language

This branch replaces the ANTLR 2 parser of `Spring.Expressions` with a parser generated at build time by
[Parlot](https://github.com/sebastienros/parlot)'s source generator, and removes the ANTLR 2.7 runtime that `Spring.Core`
embedded (101 files), the generated lexer and parser, the `Antlr` build target and the Windows-only tool it ran:
18,348 lines deleted, 915 added for the parser (184 of grammar, 731 of tokens, tree building and errors).

## What changes for users

- **The same expressions, the same trees.** Everything that was accepted is accepted, with the same node classes, the
  same token types and the same texts, and what was rejected is rejected. See [Equivalence](#equivalence). Evaluation
  only changes by a stack check, described below, whose cost is in [Benchmarks](#benchmarks).
- **Parsing is 8 to 48 times faster, with 6.7 to 39 times fewer allocations, on .NET 8**, and 3 to 7 times faster on
  .NET Framework. An `ObjectWrapper` access by property name is 13 times faster, as it allocated a lexer and a parser to
  read a name. See [Benchmarks](#benchmarks).
- **A denial of service is fixed.** The ANTLR grammar decides between `(a)` and `(a;b)` by parsing the expression with a
  syntactic predicate, then parsing it again. Nested, that doubles the time with each level:

  | Depth of `((((1))))` | 14 | 16 | 18 | 20 | 22 | 50 | 200 |
  |---|--:|--:|--:|--:|--:|--:|--:|
  | ANTLR | 207 ms | 771 ms | 3.1 s | 12.5 s | 49 s | about 400 years | not measurable |
  | Parlot, with the JIT of the first parse of the process | | | | | 7 ms | 7 ms | 7 ms |

  A 45-character expression takes one core for 49 seconds with ANTLR, and any application that evaluates an expression it
  did not write is exposed, even when the expression is then rejected. (The time for depth 50 is extrapolated from the others.)
- **Deep nesting is a syntax error instead of a crash.** Past the limits below, ANTLR overflows the stack, which .NET
  doesn't let code catch: the runtime terminates the process. The new parser checks the stack in its recursive rule, and
  limits nesting to 2,000 levels, as the evaluator recurses too. Measured in a 1 MB thread, on a first parse:

  | | ANTLR | Parlot |
  |---|---|---|
  | `((((1))))` | over 22 is impractical, see above | 200 levels work, 400 are a `SyntaxErrorException` |
  | `----1` | 3,000 work, 5,000 crash the process | 1,500 work, 2,500 are a `SyntaxErrorException`, whatever the length |

  **Evaluation checks the stack too, which is new.** A long chain such as `1+1+1+...` parses without recursion in both,
  and then overflowed the stack when *evaluated*, at 3,000 terms in a 1 MB thread, and 1,900 prefix operators can do the
  same on a smaller stack. `BaseNode` now calls `RuntimeHelpers.EnsureSufficientExecutionStack()` for each node it evaluates or sets, so
  this is an `InsufficientExecutionStackException`. It costs time on prepared expressions, see [Benchmarks](#benchmarks).
- **No Windows-only tool to change the grammar.** The grammar is C#, compiled by the normal build.
- **Dependencies.** No package is added at run time for Parlot: `Parlot.SourceGenerator` is a build-only analyzer
  (`PrivateAssets="all"`), which emits the parser into `Spring.Core`. The generated code needs `System.Memory` (now an
  explicit dependency, it was already there through `Microsoft.Extensions.Logging.Abstractions`) on .NET Framework and
  .NET Standard 2.0.
- **The assembly is 8% larger.** `Spring.Core.dll` goes from 908 KB to 983 KB (`netstandard2.0`, Release): the generated
  parser and its embedded support code are larger than the code they replace.

## Public API changes

The repository ships on NuGet and these are breaking for code that used them, which is also listed in
`BreakingChanges.txt`. They need a recompilation at least, as return types change:

| Before | After |
|---|---|
| All public types of `Spring.Expressions.Parser.antlr` and `.antlr.collections` (`BaseAST`, `AST`, `IToken`, `RecognitionException`, `TokenStreamRecognitionException`, `CharScanner`, ...) | Removed |
| `SpringAST : antlr.BaseAST, ISerializable` | `SpringAST : ISerializable` |
| `getFirstChild()`, `getNextSibling()` returning `AST` | Return `SpringAST` |
| `addChild(AST)`, `setFirstChild(AST)`, `setNextSibling(AST)`, `initialize(AST)`, `Equals(AST)` | Take `SpringAST` |
| `SpringAST(IToken)`, `initialize(IToken)` | Removed |
| Members of `BaseAST` nothing used: `EqualsList`, `EqualsListPartial`, `EqualsTree`, `EqualsTreePartial`, `findAll`, `findAllPartial`, `setVerboseStringConversion`, `ToTree`, `decode`, `encode`, `xmlSerialize*`, `Clone` | Removed |
| `internal SyntaxErrorException : RecognitionException`, so `catch (RecognitionException)` | `public SyntaxErrorException : Exception`, with `Line`, `Column` and `Expression` |
| `SyntaxErrorException.ToString()` was `line 1:6: <message>` | The usual text of an exception, with its stack trace |

Kept as they were: `Expression.Parse`, every node class and their constructors, `BaseNode` (it now checks the stack in `GetValue` and `SetValue`, without a change of signature), the other members of
`SpringAST` (`getText`, `setText`, `Type`, `Text`, `getNumberOfChildren`, `removeChildren`, `setType`, `ToStringList`,
`ToStringTree`, `Equals(object)`), and `Spring.Expressions.Parser.ExpressionParserTokenTypes`, the values of `Type`.

**Serialization is compatible in both directions.** `SpringAST` writes the same `down`, `right`, `ttype` and `text`
entries. An expression serialized by the previous version was deserialized and evaluated by this one with
`BinaryFormatter`, and the other way around. The `BinaryFormatter` and SOAP tests of `ExpressionEvaluatorTests` pass.

## Equivalence

The history of this branch has `ParserDifferentialTests` (commit `eecad8a5`, removed when ANTLR could no longer run). It
runs the two parsers side by side, through all four entry points (`Parse`, `ParsePrimary`, `ParseProperty` and
`ParseLambda`), and compares the printed trees (class, token type, text, children) and the errors. The inputs are 4,204
strings from the test suite and a hand-written list of edge cases, generated expressions covering each grammar rule, and random mutations
of both with a vocabulary of the tokens and their near misses.

- An early run of 3.9 million parses found 21 differences, all from one lexer rule (`.` followed by a digit always starts a number,
  even an invalid one), now fixed. Before that, smaller runs found the other lexer and follow-set quirks listed below.
- The final run: **1,571,952 parses, 615,238 accepted with identical trees, 956,714 rejected by both, no other result.**
- Run again on the final grammar, with the recursion limit left out, it gives the same counts.
- The rejected ones have the same column in 91% of the cases, and the same column and message in 81%.

`ExpressionGrammarTests` stays, with the trees and errors ANTLR gave for 216 expressions, chosen for the quirks of the
grammar, written to `ExpressionGrammarGolden.json` by ANTLR itself. They are compared exactly, for `Parse` and `ParsePrimary`: the 61
entries where an error has another position or text hold both, this parser's as the expected one and ANTLR's for reference.

The ANTLR grammar has unusual behaviour that this port keeps deliberately, because it decides what is accepted: a dot between
nodes is optional (`a b` is `a.b`, `1 .a` and `a.` parse), `T(` is one token only without a space, `T(...)` reads any token
up to `)`, a real number with a fraction has to have a valid exponent after `e` while an integer doesn't (`1.5ex` fails, `1ex`
is `1` then `ex`), `Ul` is not an integer suffix but `lU` is, keywords can't be property names, the relational and power
operators don't chain, the line of an error is always 1 and its column counts a tab as moving to the next multiple of 8.

Differences, all deliberate:

- **Positions and messages of syntax errors.** An error is reported at the furthest token reached, where ANTLR's LL(2)
  prediction often reports the token before it: for `#hope.floats#` it reported `floats`, and now `#`. Where an error
  named the expected token, such as `expecting "RPAREN", found 'x'`, it is usually now `unexpected token: ["x",<33>,line=1,col=5]`.
  Most lexical errors, and the messages of `ParseLambda` and `ParseProperty` are as they were.
- **`￿` in the input.** ANTLR took that char for the end of the input and ignored what followed. It is now an
  invalid char.
- **`QualifiedIdentifier` has no children.** ANTLR kept one child per token of the name, which only served to rebuild its
  text; the text is now whole. `getText()` returns the same value.
- **The nesting limits above.** The parser doesn't return why it failed, so at 32 or more open brackets or prefix operators an error reads
  `unexpected token or expression nested too deeply: "("`, which can also be a plain syntax error at that depth.

## Design

| File | Role |
|---|---|
| `Expressions/ExpressionGrammar.parlot.cs` | The grammar, a port of `Expression.g`. Build-only: read by the generator, not compiled. |
| `Expressions/ExpressionGrammar.cs` | The generated `TryParse` declaration, the tokens as `Terms.Scan` scanners, the nodes and the errors. |
| `Expressions/SpringAST.cs` | The child and sibling links, and the members of `antlr.BaseAST` that remain. |

**Detaching the nodes.** Nodes kept their method names, and only the types of the variables walking them changed from `AST`
to `SpringAST`, so that the 60 node classes are almost untouched. Apart from the stack check in `BaseNode`, the evaluation doesn't change.

**PEG vs LL(2).** A PEG choice can backtrack where ANTLR commits to an alternative after looking at two tokens. Three things keep the same language:

- Tokens are scanned as ANTLR's lexer does: `^` doesn't match the start of `^{`, `{` doesn't match `{|`, `.` doesn't match `.5`. Otherwise,
  `a^{b,c}`, an error, would be read again as `a ^ {b,c}`.
- A name followed by `(` can only be a call, so `a(b;c)` isn't read again as a property followed by an expression list. This is `Not(lparen)` in
  the grammar.
- `Parse` needs the end of the input, so a failed alternative can't be accepted. `ParsePrimary` doesn't, and checks what follows it with
  the follow sets of the ANTLR grammar (`CanFollowPrimary`).

**Named arguments.** ANTLR commits to a named argument when it sees a name and `=`. Without that, a failed one was read again as an assignment, which
took exponential time on nested `new a(b=` (1.6 s at 177 chars), so the other alternative starts with `Not(id.AndSkip(assign))`.

**Operator tables.** `Operators()` handles the unary and the left-associative levels. The relational and the power levels
don't chain, and the third operand of `?:` is a whole expression, which a table can't express, so these remain sequences. There
are three tables and three sequences rather than one table, on which one expression spends a little more than it could.

**One grammar, three rules.** `Parse`, `ParsePrimary` and `ParseLambda` share one generated parser through a configuration
parameter and `If`, as generating one each would triple the code. `ParseProperty` is a scan by hand.

**Why the generator rather than the `Parlot` runtime package:** `Spring.Core` embeds its parser and is strong-named,
and the repository already builds with the .NET 10 SDK, which is what the generator requires. A consumer of the package doesn't
need this SDK. `net472`, the framework floor of 4.0, is also the floor of the generator, so it needs no MSBuild workaround.

## Tests

Green reference and result of `Spring.Core.Tests`, whose `ExpressionEvaluatorTests` (133 tests) only compile on .NET Framework:

| | Before | After |
|---|---|---|
| `net462`, Windows | 2,741 passed, 0 failed, 23 skipped | 2,773 passed, 0 failed, 23 skipped |
| `net8.0`, macOS | 2,458 passed, 5 failed, 20 skipped | 2,490 passed, 5 failed, 20 skipped |

The 5 failures are the same before and after, and don't touch expressions: currency and percent formatters that depend on the culture of the
machine. The 32 new tests are `ExpressionGrammarTests`.

These figures predate the rebase onto the 4.0 groundwork. After it, `net10.0` on macOS has 2,494 passed, the same 5 failures and 92 skipped.

## Benchmarks

`test/Spring/Spring.Benchmark/ExpressionBenchmark.cs` only uses the public API, so that the same file runs against both versions.
The commit that adds it (`25c6a0de`) still has ANTLR:

```bash
git worktree add ../spring-antlr 25c6a0de
cp test/Spring/Spring.Benchmark/*.cs ../spring-antlr/test/Spring/Spring.Benchmark/
# In each checkout:
dotnet run -c Release -f net8.0 --project test/Spring/Spring.Benchmark/Spring.Benchmark.csproj -- --filter '*ExpressionBenchmark*' '*ObjectWrapperBenchmark*' --job short
```

The expressions: *Small* is `Name.Length + 1` (15 chars), *Medium* a conditional with two property paths (86 chars), *Large* seven terms with selections,
projections, list and map initializers, a type, a constructor, a lambda and an expression list (491 chars).

BenchmarkDotNet v0.15.2, `--job short`, one launch of each version, two machines:

- **Arm64:** macOS 15.8.2, Apple M4 Pro, .NET 8.0.30, Arm64 RyuJIT, with other work running (load average 5 to 18). Two rounds, the lower of the two for each cell,
  as one measurement of the Parlot Large parse was disturbed (7.5 µs and 16.8 µs, with a deviation of 7.5 µs).
- **x64:** Windows 11 (Hyper-V), AMD EPYC 7763, .NET 8.0.31 and .NET Framework (`net472` job), one round. This machine has shown
  differences of up to 27% between runs of identical code: read the ratios as orders of magnitude, not as precise values.

### Parsing only

`Expression.Parse`, mean time and allocations:

| Size | | ANTLR | Parlot | Faster | Fewer allocations |
|---|---|--:|--:|--:|--:|
| Small | Arm64, .NET 8 | 1,922 ns, 20,785 B | 216 ns, 536 B | 8.9x | 39x |
| | x64, .NET 8 | 4,357 ns, 20.3 KB | 550 ns, 536 B | 7.9x | 39x |
| | x64, .NET Framework | 5,541 ns, 20.4 KB | 1,695 ns, 546 B | 3.3x | 38x |
| Medium | Arm64, .NET 8 | 53.2 µs, 28,770 B | 1.13 µs, 2,040 B | 47x | 14x |
| | x64, .NET 8 | 49.1 µs, 28.1 KB | 2.81 µs, 2,040 B | 17x | 14x |
| | x64, .NET Framework | 59.9 µs, 28.7 KB | 8.42 µs, 2,086 B | 7.1x | 14x |
| Large | Arm64, .NET 8 | 363 µs, 81,181 B | 7.5 µs, 12,088 B | 48x | 6.7x |
| | x64, .NET 8 | 352 µs, 79.3 KB | 19.2 µs, 12,088 B | 18x | 6.7x |
| | x64, .NET Framework | 418 µs, 83.0 KB | 60.3 µs, 12,300 B | 6.9x | 6.9x |

Most of ANTLR's 20 KB for the smallest expression is its lexer and token buffers, created per parse. A parse on .NET Framework is
about three times slower than on .NET 8, as it reads through the `System.Memory` package.

### The layers above

`ExpressionEvaluator.GetValue(root, expression)` parses each time, with no cache. The first evaluation of a parse also does its reflection
lookups and creates dynamic methods, which dominate the Large expression, so its time depends less on the parser.

| `GetValue`, uncached | | ANTLR | Parlot | Faster |
|---|---|--:|--:|--:|
| Small | Arm64, .NET 8 | 2,139 ns, 21,193 B | 421 ns, 944 B | 5.1x |
| | x64, .NET 8 | 4,619 ns, 20.7 KB | 992 ns, 944 B | 4.7x |
| | x64, .NET Framework | 7,222 ns, 20.9 KB | 2,978 ns, 963 B | 2.4x |
| Medium | Arm64, .NET 8 | 70.7 µs, 35,523 B | 17.3 µs, 8,792 B | 4.1x |
| | x64, .NET 8 | 74.8 µs, 34.9 KB | 23.6 µs, 9,008 B | 3.2x |
| | x64, .NET Framework | 128 µs, 38.3 KB | 65.3 µs, 11,882 B | 2.0x |
| Large | Arm64, .NET 8 | 615 µs, 104 KB | 270 µs, 35,348 B | 2.3x |
| | x64, .NET 8 | 1,441 µs, 102 KB | 1,042 µs, 35,563 B | 1.4x |
| | x64, .NET Framework | 2,176 µs, 115 KB | 1,678 µs, 44,753 B | 1.3x |

`Expression.Parse` then 100 `GetValue`, as the documentation recommends. The cost of the parse is spread over the evaluations, and
what remains is the first evaluation, so the gain shrinks as expected:

| Parse + 100 `GetValue` | | ANTLR | Parlot | Faster |
|---|---|--:|--:|--:|
| Small | Arm64, .NET 8 | 6,984 ns | 5,220 ns | 1.3x |
| | x64, .NET 8 | 15.3 µs | 11.2 µs | 1.4x |
| | x64, .NET Framework | 24.6 µs | 19.5 µs | 1.3x |
| Medium | Arm64, .NET 8 | 154 µs | 90.5 µs | 1.7x |
| | x64, .NET 8 | 299 µs | 216 µs | 1.4x |
| | x64, .NET Framework | 520 µs | 453 µs | 1.1x |
| Large | Arm64, .NET 8 | 848 µs | 458 µs | 1.9x |
| | x64, .NET 8 | 1,976 µs | 1,527 µs | 1.3x |
| | x64, .NET Framework | 3,005 µs | 2,459 µs | 1.2x |

A prepared `GetValue` doesn't depend on the parser, but it pays for the stack check `BaseNode` now does. Arm64, .NET 8, ANTLR then Parlot without
the check then with it, one short run of each: Small 49 ns, 50 ns, 56 ns; Medium 816 ns, 749 ns, 806 ns; Large 2,042 ns, 1,972 ns, 2,382 ns, with the
same allocations. The last run gave 57 ns, 781 ns and 2,354 ns. So about 4% to 21%, within the noise of this machine for the smallest gaps. An application that evaluates prepared expressions in a
hot loop gives back that much, for not crashing on a deep tree.

The tables above were measured before the last fixes, which added the check, the guard on named arguments and a `Then` closing each primary expression,
with the same allocations. One new run on Arm64 is 5% to 12% slower at parsing than the tables (Small 243 ns, Medium 1,265 ns, Large 8,616 ns). That is within
what this machine varies under load, though it wasn't isolated, and the ratios to ANTLR stay at 8x to 42x there. x64 wasn't measured again.

`ObjectWrapper.GetPropertyValue` parses the name on each call. `Name` takes the path that only reads a name (`ParseProperty`), `Friends[1].Address.City` the one
that parses a primary expression (`ParsePrimary`):

| | | ANTLR | Parlot | Faster | Fewer allocations |
|---|---|--:|--:|--:|--:|
| `Name` | Arm64, .NET 8 | 1,142 ns, 19.3 KB | 84 ns, 240 B | 14x | 82x |
| | x64, .NET 8 | 2,241 ns, 19.3 KB | 173 ns, 240 B | 13x | 82x |
| | x64, .NET Framework | 3,342 ns | 477 ns, 249 B | 7.0x | |
| `Friends[1].Address.City` | Arm64, .NET 8 | 2,643 ns, 21.6 KB | 617 ns, 1,456 B | 4.3x | 15x |
| | x64, .NET 8 | 5,812 ns, 21.6 KB | 1,402 ns, 1,456 B | 4.1x | 15x |
| | x64, .NET Framework | 8,939 ns | 4,011 ns, 1,492 B | 2.2x | |

## Follow-ups, not in this change

- `IntLiteralNode` can't evaluate the suffixes the grammar accepts: `5L` parses and then throws a `FormatException`.
- The line of a syntax error is always 1, whatever the input, because the ANTLR lexer never counted lines. Kept for compatibility.

## Parlot gaps found

Nothing here blocked the port. Parlot 2.2.1:

1. **`net462` is refused** (`PARLOT026`, floor 4.7.2), and the package's `netstandard` reference doesn't resolve on its reference assemblies, while the generated code works there. This took four
   MSBuild targets until the 4.0 floor moved to `net472`. Same finding as Lucene.NET.
2. **`PARLOT021` is misleading when a call doesn't bind.** `If(() => rule == x, a, If(...))` with parsers whose types made inference fail reported "may only be read inside supported parse-time callbacks"
   rather than the inference error, as the check requires a bound argument. Giving the type, `If<BaseNode>(...)`, fixed it.
3. **`Unary` can't mix operator types**, although `docs/parsers.md` shows `.Unary((Terms.Char('-'), ...), (Terms.Text("not"), ...))`: `Unary<TInput>(params ...)` can't be inferred.
4. **A scanner is called with an empty span at the end of the input.** The docs of `Scan` don't say so, and a scanner reading `span[0]` throws `IndexOutOfRangeException`.
5. **No non-associative operator level** (`a == b == c` and `a ^ b ^ c` have to fail), and no last operand of `Ternary` that is a whole expression. These force a chain of tables and sequences here.
6. **No way to commit to an alternative without losing the failure position.** `ElseError` reports where it was reached, not the furthest failure, so a choice that must not backtrack takes a negative lookahead (`Not(lparen)`).
7. **An entry point can't return where it stopped.** A rule without `Eof` needs `When((context, x) => ...)` reading `context.Scanner.Cursor` to check what follows.
8. **The stack per level is high before tier-up**: in a 1 MB thread, between 200 and 400 levels of parentheses on a first parse in Release, and 96 in the Debug test host. It is far more than ANTLR could reach in practice, and it fails cleanly.
9. **The generated code is large**: the assembly grows by 75 KB even though 101 ANTLR files and 6,000 generated lines go away. Parsers used twice (`methodOrProperty`, `arguments`) didn't look shared across the choices that use them; this wasn't investigated further.
10. **The failure overload doesn't say why a parse failed.** Reaching `MaxRecursionDepth` or the end of the stack returns `false` and a position, like a syntax error. The diagnostics overload has the reason but generates the grammar twice. So the message here is a guess from the depth at the failure: from 32 open brackets or prefix operators it says `unexpected token or expression nested too deeply`, including for a plain typo at that depth, and a real stack failure below it reads as an unexpected token.
