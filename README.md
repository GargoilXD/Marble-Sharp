# Marble-Sharp

A verbose and simple programming language hobby project, implemented in C# with an in-Godot IDE.

> Status: early prototype. The language, standard library, and IDE are all subject to change. Sister implementation: [Marble-Godot](https://github.com/GargoilXD/Marble-Godot) (GDScript backend, capitalized syntax), the original implementation of Marble.

## What is Marble?

Marble is a small, explicitly typed, imperative language designed to read like prose. This repo hosts the C# evolution of the language: tokenizer, token sequencer, parser, tree interpreter, and a Godot `.NET` editor UI that exposes each pipeline stage.

Compared to Marble-Godot, syntax here is lowercase (`function`, `variable`, `integer`, `for`) with a richer modifier and definition set (`classified`, `unlimited`, `reference`, `structure`, `enumeration`, `constructor`).

## Features

- Hand-written tokenizer with keyword classifications (`Main/Scripts/Tokenizer.cs`)
- Token sequencer + recursive parser producing `Node` trees (`TokenSequencer.cs`, `Parser.cs`; `OldParser.cs` kept for reference)
- Tree-walking interpreter with scoped storage and async `Input()` dialog (`Main/Scripts/Interpreter.cs`, `Modules/MarbleClasses/Storage/ContextualStorage.cs`)
- IDE scene with code editor, per-stage output tabs, and syntax highlighting (`Main/MarbleIDE.tscn`, `Main/MarbleIDE.cs`)
- Staged execution: stop after tokenizer, parser, or interpreter
- Persistent editor state via `Main/Save/save.tres` (`Ctrl+S` to save, `Ctrl+T` to toggle tabs)
- Sample programs in `Programs/*.mb`, including classes, matrices, and a toy CPU

## Requirements

- Godot 4.2 with .NET support (`config/features` includes `"4.2"`, `"C#"`, `"GL Compatibility"`)
- .NET 6 SDK (`TargetFramework net6.0` in `Marble Sharp.csproj`, `Godot.NET.Sdk/4.2.2`)
- `GL Compatibility` renderer (set in `project.godot`)

## Quickstart

1. Clone the repo.
2. Open `project.godot` in Godot 4.2 .NET.
3. Build the solution (`Marble Sharp.sln`) when prompted.
4. Press `F5` to run the main scene (`res://Main/MarbleIDE.tscn`).
5. Paste a sample from `Programs/` into the editor and press `Run`.
6. Use the stage option button to set stop-at (`TOKENIZER`, `PARSER`, `INTERPRETER`) and inspect each tab.

Keyboard:

- `Ctrl+S`: save current editor text and stop-at selection to `Main/Save/save.tres`
- `Ctrl+T`: show/hide the stage tabs
- `Run`: tokenize, parse, then interpret
- `Clear`: clear all three output panes

## Example

From `Programs/GuessingGame.mb`:

```marble
function list Make_random_sample(integer Size, integer lower, integer upper) {
	list sample = []
	for (x in Size) {
		sample.append(Random(lower, upper))
	}
	return sample
}

list Computer_numbers = Make_random_sample(5, 0, 10)
integer Size = 5
list Responses = []

for (x in Size) {
	Responses.append(Input('Guess a number:'))
}

integer number_matched = 0
for (x in Size) {
	for (y in Size) {
		if (Responses[x] == (Computer_numbers[y] + '')) {
			number_matched += 1
		}
	}
}

Print('The computer chose:', Computer_numbers)
Print('You chose:', Responses)
Print('There were', number_matched, 'Matches')
```

More samples:

- `Programs/ClassTest.mb`: classes, constructors, inheritance sketch, lambdas, `=>` single-expression bodies
- `Programs/Matrix.mb`: `Matrix` class with determinant and multiply
- `Programs/CPU.mb`: toy CPU stepping through an instruction register
- `Programs/Ideas.mb`, `Programs/Tokenizer.mb`: syntax sketches
- `Main/Language_structure.txt`: operator-precedence and declaration-shape notes

## Project structure

```
.
├── project.godot                  # Godot 4.2 .NET, main scene: Main/MarbleIDE.tscn
├── Marble Sharp.csproj / .sln     # Godot.NET.Sdk/4.2.2, net6.0, RootNamespace MarbleSharp
├── Main/
│   ├── MarbleIDE.tscn / .cs       # IDE controller: Run, staged display, highlighting, save
│   ├── MarbleIDETheme.tres
│   ├── InputGetter.cs             # Input() dialog used by the interpreter
│   ├── MarbleCLI.cs               # Empty stub (not wired up)
│   ├── Language_structure.txt     # Precedence / declaration notes
│   ├── Save/
│   │   ├── EditorSave.cs
│   │   └── save.tres              # Persisted editor state
│   └── Scripts/
│       ├── Tokenizer.cs           # Lexer
│       ├── TokenSequencer.cs      # Token stream shaping before parse
│       ├── Parser.cs              # Token -> Node tree (current)
│       ├── OldParser.cs           # Previous parser, reference only
│       └── Interpreter.cs         # Node tree walker (~114 KB)
├── Modules/
│   ├── Token/                     # Token, DataToken, KeywordToken, OperatorToken, SymbolToken, positions
│   ├── Node/                      # Node, DataNode, OperatorNode, DefinitionNode, DecisionNode, LoopNode, KeywordNode
│   ├── MarbleClasses/
│   │   ├── Storage/               # ContextualStorage, StorageClass/Function/Variable/Entity, MarbleData types
│   │   ├── InterpreterOutput.cs
│   │   └── FlowController.cs
│   └── Error/                     # Error, TokenizerError, ParserError, InterpreterError
├── Programs/                      # Sample Marble programs (.mb)
├── Test/
│   └── QuickTest.cs               # Empty stub
└── Assets/                        # Fonts (SpaceMono, CascadiaMono), Icons (Marble.png/.svg)
```

Note: `.vs/` is local Visual Studio state and should stay out of version control.

## How it works

1. **Tokenize.** `Tokenizer.Tokenize()` reads `MarbleIDE.EditorCode` into `Token`s. Keywords are matched case-sensitively (see table below). Failures throw `TokenizerError` with a `TokenPosition`, displayed in the Tokenizer tab.
2. **Sequence + parse.** `TokenSequencer` normalizes the stream, then `Parser.Parse(tokens)` builds `Node`s (`DataNode`, `BinaryOperatorNode`, `FunctionDefinitionNode`, `IFNode`, `ForNode`, etc.). Failures surface as `ParserError` in the Parser tab. Custom datatypes from `class` / `structure` / `enumeration` are fed back into the editor highlighter via `OnNewDatatype`.
3. **Interpret.** `Interpreter.Interprete(nodes, storage)` walks the tree with `ContextualStorage` scopes and appends to output, shown in the Interpreter tab. `Input()` awaits the `InputGetter` dialog. Control-flow breaks (`break` / `continue` / `return`) propagate via `FlowController`.

The stop-at selector short-circuits after the chosen stage so you can debug lexing and parsing independently.

## Language cheat-sheet

Comments use `#`:

```
# this is a comment #
```

Strings use `"double"` or `'single'` quotes. `$"..."` is a format string. Newlines are significant (`END_OF_LINE` tokens). `=>` introduces a single-expression body.

| Category | Keywords |
|---|---|
| Modifiers | `constant`, `static`, `public`, `private`, `classified`, `unlimited`, `reference` |
| Datatypes | `void`, `variant`, `boolean`, `integer`, `float`, `string`, `list`, `dictionary`, `callable`, `object` |
| Definitions | `class`, `structure`, `enumeration`, `function`, `constructor`, `variable` |
| Loops | `for`, `while` |
| Decisions | `if`, `else`, `elseif`, `match`, `case`, `default` |
| Flow control | `break`, `continue`, `return`, `breakpoint` |
| Exception handling | `try`, `catch` |
| Operators (word) | `not`, `and`, `or`, `in`, `is`, `extends` |
| Builtins | `Assert`, `Print`, `PrintLine`, `Range`, `Random`, `Input` |

Symbolic operators include `+ - * / ^ % = < > . & | $` and `// || && == != <= >= += -= *= /= ^= %= =>`. `^` is exponent, `//` is integer divide, `$` starts a format string.

Class sketch from `Programs/ClassTest.mb`:

```marble
class Human {
	public variable integer Age
	public variable string Gender
	public constructor(integer age, string gender) {
		Age = age
		Gender = gender
	}
}
```

## Testing

- `Test/QuickTest.cs`: empty stub, not wired into the IDE.
- `Programs/*.mb`: the de facto test corpus. Run each file through the IDE and check the Interpreter tab.
- `Main/Language_structure.txt`: precedence expectations worth turning into parser tests.

There is no automated test runner yet. Per project baseline, the next step would be unit tests for tokenizer/sequencer/parser plus full-stack runs of `Programs/` against expected outputs.

## Roadmap / limitations

- `MarbleCLI.cs` is an empty placeholder; the IDE is the only runner
- No package manager, module imports, or file I/O beyond the editor
- Error reporting stops at the first error per stage
- `OldParser.cs` is dead code kept for reference and should eventually be removed
- `.vs/` and `.godot/` churn should be tightened in `.gitignore`

## Contributing

Small hobby project. Keep changes minimal and match the surrounding C# style. If you add syntax, add a sample under `Programs/` showing it running, and update the cheat-sheet above.

## License

MIT — see `LICENSE` (to be added; intended license is MIT). No credentials are stored in this repo; local state lives in `Main/Save/save.tres`.
