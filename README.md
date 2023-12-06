# HighLevelAssembler

## NOTES ON THE THREE IMPLEMENTATIONS
## C#
First implementation and the first large C# program I've written. Style is not brilliant, but as an experiment with C# it's been very useful.
No control structures implemented. Requires C# V12 and .NET 8 to compile.
No unit tests.

## VB.NET
Second implementation (make sure you use the `control_structures` branch.) Much nicer style (I think: write one, throw it away and write it correctly the second time) and the parsing system is much easier to extend.
Over 100 unit tests.

## Python 3
Third implementation. Not started yet.

## Summary
Low-level language programming is hard. Assembly language programming is hard. High-level languages were developed to make programming easier. If students are to learn the concepts of assembly language programming it may help to start by programming in a language that is halfway between the high-level language they are (one hopes) used to and the AQA assembly language instruction set. HighLevelAssembler (HLA) is a language that tries to be that 'bridge' language.

## The HLA language definition in EBNF

`"` and `"` indicates a terminal or literal.

`|` indicates a choice, e.g., a digit is 0 or 1 or 2 ... or 9.

`{` `}` indicates repetition of zero or more times, e.g., a number is a digit followed by any number of digits.

`[` `]` indicates an optional element, e.g., a statement has an optional label followed by a colon followed by a simple_statement.

`(*` and `*)` indicate the start and end of a comment.

```
digit                         = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" .
letter                        = "A" | "B" | "C" | ... | "X" | "Y" | "Z" .
number                        = digit { digit } .
label                         = letter { letter } .
variable                      = "R0" | "R1" | ... "R14" | "R15" .
memory_reference              = "Memory" "[" ( number | label ) "]" .
operand                       = variable | number .
arithmetic_operator           = "+" | "-" .
logical_operator              = "AND" | "&" | "OR" | "|" | "XOR" | "^" .   (* "&" | "|" | "^" for C# or Java based HLAs *)     
shift_operator                = "<<" | ">>" .
conditional_operator          = "=" | "==" | "<>" | "!=" | "<" | ">" .     (* "==", "!=" for C#, Java, and Python based HLAs *)

declaration                   = [ label ":" ] "DATA" number .
statement                     = [ label ":" ] simple_statement .
simple_statement              = variable_assignment | memory_assignment | arithmetic_statement | not_statement | shift_statement | conditional_statement | goto_statement | halt_statement . 
variable_assignment           = variable "=" ( memory_reference | number ) .
memory_assignment             = memory_reference = variable .
arithmetic_statement          = variable "=" variable arithmetic_operator operand .
not_statement                 = variable "=" ( "NOT" | "!" ) operand .     (* "!" for C# or Java based HLAs *)
shift_statement               = variable shift_operator operand .
conditional_statement         = "IF" variable conditional_operator operand "GOTO" label .
goto_statement                = "GOTO" label .
halt_statement                = "HALT" | END" | "STOP" .

```

# Translation of HLA statements to AQA assembly instructions

Most of these should be pretty obvious and will shown by example.

Notes: All input is converted to UPPERCASE, so r1 and R1 are treated identically, as are Memory, memory, and MEMORY.
       Spaces and commas are ignored.

Variable assignment
```
R1 = Memory[100]              => LDR    R1, 100
R2 = Memory[SOURCE]           => LDR    R2, SOURCE
```
Memory assignment
```
Memory[101] = R2              => STR    R2, 101
Memory[LEFTDIGIT] = R3        => STR    R3, LEFTDIGIT
```
Arithmetic statement
```
R1 = R1 + R2                  => ADD    R1, R1, R2
R1 = R1 + 1                   => ADD    R1, R1, #1
```
Not statement
```
R1 = !R2                      => MVN    R1, R2
R1 = !0                       => MVN    R1, #0
```
Shift statement
```
R1 = R1 << R2                 => LSL    R1, R1, R2
R1 = R3 >> 4                  => LSR    R1, R3, #4
```
Conditional statement: the only HLA statement that compiles to more than one assembly instruction
```
IF R1 < R2 GOTO AGAIN         => CMP    R1, R2
                                 BLT    AGAIN
IF R3 == 0 GOTO DONE          => CMP    R3, #0
                                 BEQ    DONE
```
Goto statement
```
GOTO START                    => B      START
```
Halt statement
```
HALT                          => HALT
STOP                          => STOP
END                           => END
```
Declaration
```
DATA number                   => Reserves a memory location and initialises its contents to
                                 to number
                                 Executing a DATA location causes an error
```  

## Control structures

The grammar for these has not been written yet, but looking at the `Parser.vb` file and the `UnitTest_CodeGeneration.vb` files should give you the main ideas. The old AQA specification had a section on translating HLL control structures into assembler for which this would have been very useful.
