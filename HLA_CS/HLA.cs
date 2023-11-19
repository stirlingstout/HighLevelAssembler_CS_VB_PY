namespace CS_HLA
{
    using System;
    using System.Diagnostics;
    using System.IO;

    // type aliases: only available in C#12 and .NET 8.0
    using AssembledCode = List<(string label, int PC, Instruction instruction)>;
    using ErrorList = List<string>;
    using SourceCode = List<string>;
    using Token = (string text, int? intOrRegisterNumber, HLA.TokenType type);

    // TODO: change all Source to ToSource as in ToString

    internal class HLA
    {

        const string COMMENT = "--";
        const string HLA_EXTENSION = ".HLA";

        public static readonly int? NO_ADDRESS = null;
        public static readonly int? NO_REGISTER = null;

        public const int NUMBER_OF_REGISTERS = 16;
        public const int NUMBER_OF_MEMORY_LOCATIONS = 1024;

        public const string MEMORY = "MEM";                    // Could change to Memory, M, RAM etc
        public const string MEMORY_ACCESS_START = "[";         // Change to MEM( for VB.NET indexing style and
        public const string MEMORY_ACCESS_END = "]";           //  change to )
        public const string ASSIGNMENT = "=";                  // Could be <-, :=, <=
        public const string AND_OPERATOR = "&";                // For VB.NET change to AND and
        public const string OR_OPERATOR = "|";                 //  change to OR and
        public const string XOR_OPERATOR = "^";                //  change to XOR
        public const string NOT_OPERATOR = "!";                //  change to NOT
        public const string EQUAL_OPERATOR = "==";             // For VB.NET change to = and
        public const string NOT_EQUAL_OPERATOR = "!=";         //  change to <>
        public const string LESS_THAN_OPERATOR = "<";          // Difficult to see what you'd change this to for VB.NET
        public const string GREATER_THAN_OPERATOR = ">";       //  or this
        public const string END = "END";                      // Although the tokeniser and parser accept others this
                                                              //  is what the Source methods of the Instruction classes use
        public const string GOTO = "GOTO";
        enum Operand2Type
        {
            Immediate,
            Register,
            Label,
            Error
        }

        enum MemoryReferenceType
        {
            Literal,
            Label,
            Error
        }

        public enum TokenType
        {
            Literal, // e.g., number
            Register, // e.g., R1
            Symbol, // e.g., >>
            Keyword, // e.g., IF
            Identifier // e.g., AGAIN
        }

        public static Dictionary<string, string> arithmeticLogicOperators = new() {
                                            { "+", "ADD" },{"-", "SUB" }, {AND_OPERATOR, "AND" }, {OR_OPERATOR, "ORR"},
                                            {XOR_OPERATOR, "EOR" }, {NOT_OPERATOR, "MVN" }, {"<<", "LSL"}, {">>", "LSR"} };
        public static Dictionary<string, string> relationalOperators = new() {
                                            {EQUAL_OPERATOR,"BEQ" }, {NOT_EQUAL_OPERATOR, "BNE" }, {LESS_THAN_OPERATOR, "BLT" },
                                            {GREATER_THAN_OPERATOR, "BGT" } };
        static void Main()
        {
            static void SetupConsole()
            {   // add any setting of sizes or colours here
                Console.Clear();
            }

            SetupConsole();

            var Program = new SourceCode();
            var Assembly = new AssembledCode();
            var Errors = new ErrorList();
            var finished = false;
            do
            {
                var Choice = GetMenuOption();
                switch (Choice)
                {
                    case "L":
                    case "O": // allow Load or Open
                        Program = LoadHLAFile(Program);
                        Assembly.Clear();
                        Errors.Clear();
                        break;
                    case "C":
                    case "T": // allow Compile or Translate
                        if (Program.Count > 0)
                        {
                            (Assembly, Errors) = CompileHLA(Program);
                            if (Errors.Count > 0)
                            {
                                DisplayErrors(Program, Errors);
                                Assembly.Clear();
                                Errors.Clear();
                            }
                            else
                            {
                                DisplayAssembly(Assembly);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No HLA program to compile");
                        }
                        break;
                    case "D":
                    case "S": // allow Display or Show
                        if (Program.Count > 0)
                        {
                            DisplayHLA(Program);
                            if (Assembly.Count > 0)
                            {
                                DisplayAssembly(Assembly);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No HLA program to display");
                        }
                        break;
                    case "E":
                    case "I": // allow Execute or Interpret
                        ExecuteAssembly(Assembly);
                        break;
                    case "N":
                        Program = NewHLAProgram();
                        Assembly.Clear();
                        Errors.Clear();
                        break;
                    case "H":
                    case "?": // allow Help or ?
                        DisplayHelp();
                        break;
                    case "Q":
                        finished = true;
                        break;
                }
            } while (!finished);

            Console.WriteLine("HLA - High Level Assembly finished");
            Console.ReadLine();
        }

        static string GetMenuOption()
        {
            DisplayMenu();
            Console.Write("Enter option: ");
            return Console.ReadLine()!.ToUpper();
        }

        static void DisplayMenu()
        {
            Console.WriteLine("L - (L)oad a HLA file");
            Console.WriteLine("D - (D)isplay the HLA program");
            Console.WriteLine("C - (C)ompile the current HLA program");
            Console.WriteLine("E - (E)xecute the compiled program");
            Console.WriteLine("N - create a (N)ew HLA program");
            Console.WriteLine("H - display the (H)elp file");
            Console.WriteLine("Q - (Q)uit the HLA program");
        }

        static List<string> LoadHLAFile(SourceCode existingProgram)
        {
            Console.Write("HLA filename: ");
            string Filename = Console.ReadLine()!.ToUpper();
            if (!Filename.EndsWith(HLA_EXTENSION))
            {
                Filename += HLA_EXTENSION;
            }
            List<string> program = new();
            if (File.Exists(Filename))
            {
                using StreamReader HLA = new(Filename);
                program.AddRange(HLA.ReadToEnd().Split(Environment.NewLine));
            }
            else
            {
                Console.WriteLine($"Couldn't find the file {Filename}");
                program = existingProgram;
            }
            return program;
        }

        static void DisplayHLA(SourceCode Program)
        {
            var number = 1;
            foreach (var line in Program)
            {
                Console.WriteLine($"{number,-4}  {line}");
                number += 1;
            }
        }

        static void DisplayAssembly(AssembledCode Code)
        {
            foreach (var (label, PC, instruction) in Code)
            {
                Debug.Assert(label == instruction.label.text); // stops irritating message about not using label
                Console.WriteLine($"{PC,4}  {instruction}");
            }
        }

        static (AssembledCode, ErrorList) CompileHLA(SourceCode Program)
        {
            //  HLA code                                is compiled to this assembly language

            //  Rd = MEM[<memory ref>]                  LDR  Rd, <memory ref>
            //  MEM[<memory ref>] = Rd                  STR  Rd, <memory ref>
            //  Rd = Rn + <operand2>                    ADD  Rd, Rn, <operand2>
            //  Rd = Rn - <operand2>                    SUB  Rd, Rn, <operand2>
            //  Rd = <operand2>                         MOV  Rd, <operand2>
            //  
            //  IF Rn <relop> <operand2> GOTO <label>   CMP  Rn, <operand2>
            //                                          B<relop> <label>

            //  GOTO / GO / BRANCH <label>              B    <label>
            //  Rd = Rn && <operand2>                   AND  Rd, Rn, <operand2>
            //  Rd = Rn || <operand2>                   ORR  Rd, Rn, <operand2>
            //  Rd = Rn ^ <operand2>                    EOR  Rd, Rn, <operand2>
            //  Rd = !<operand2>                        MVN  Rd, <operand2>
            //  Rd = Rn << <operand2>                   LSL  Rd, Rn, <operand2>
            //  Rd = Rn >> <operand2>                   LSR  Rd, Rn, <operand2>
            //  END | HALT | STOP                       HALT
            //  DATA
            //      label a location in memory for data.    
            //      Executing it gives a run-time error

            AssembledCode Assembly = new();
            int PC = 0;
            ErrorList Errors = new();

            foreach (string line in Program)
            {
                // The normal approach for a compiler would be to build a tokeniser or scanner which would
                // break each line into tokens by scanning character by character (see, for example,
                // https://people.inf.ethz.ch/wirth/ProjectOberon/Sources/ORS.Mod.txt: PROCEDURE Get scans
                // the text and returns the next token. Since the HLA language is so simple the simpler
                // approach of splitting the line into tokens based on white space separators is taken.
                // The downside of this simpler approach is that if you miss out the spaces in a statement,
                // for example writing R1=45 rather than R1 = 45, then you get an error.
                // Using a tokeniser/scanner on this would return the tokens R1, =, and 45 correctly.

                // UPDATE: now the scanner / tokeniser approach is taken

                // Project Oberon (not the UK satellite project) (http://projectoberon.net/) documents the design of a desktop computer system
                // based on a Field Programmable Gate Array (FPGA). You can see how the processor is designed
                // in the Verilog language and then used to configure the FPGA (just a collection of thousands
                // of individual logic gates and circuits). You also get the compiler source and the source
                // for the operating system. There is a Windows software implementation available.

                // Project Oberon is not to be confused with the UK satellite project proposed in 2018.


                List<string> keywords = new() { "IF", MEMORY, GOTO, "GO", "BRANCH", "DATA", END, "STOP", "HALT" };

                List<Token> Tokenise(IEnumerable<char> line, List<Token> soFar)
                {
                    Func<char, bool> isDigit = char.IsDigit;
                    Func<char, bool> isLetter = char.IsLetter;
                    Func<char, bool> isLetterOrDigit = char.IsLetterOrDigit;
                    Func<char, bool> isSymbol = char.IsSymbol;
                    Func<char, bool> isPunctuation = char.IsPunctuation;
                    string token(Func<char, bool> t) => string.Join("", line.TakeWhile(t));

                    TokenType classify(string s)
                    {
                        if (s.All(isDigit))
                        {
                            return TokenType.Literal;
                        }
                        else if (IsRegisterRef(s)) { return TokenType.Register; }
                        else if (keywords.Contains(s)) { return TokenType.Keyword; }
                        else if (isLetter(s.First())) { return TokenType.Identifier; }
                        else { return TokenType.Symbol; }
                    }

                    if (line.Count() == 0)
                    {
                        return soFar.TakeWhile(s => s.text != COMMENT).ToList();
                    }
                    else if (char.IsWhiteSpace(line.First()) || line.First() == ',')
                    {
                        return Tokenise(line.Skip(1), soFar);
                    }
                    else if (isDigit(line.First()))
                    {
                        soFar.Add((token(isDigit), Convert.ToInt32(token(isDigit)), TokenType.Literal));
                        return Tokenise(line.SkipWhile(isDigit), soFar);
                    }
                    else if (isLetter(line.First()))
                    {
                        var t = token(isLetterOrDigit);
                        if (classify(t) == TokenType.Register)
                        {
                            soFar.Add((t, RegisterNumber(t), TokenType.Register));
                        }
                        else
                        {
                            soFar.Add((t, null, classify(t)));
                        }
                        return Tokenise(line.SkipWhile(isLetterOrDigit), soFar);
                    }
                    else if (isSymbol(line.First()))
                    {   // TODO: there must be a better way to do this
                        soFar.Add((token(c => isSymbol(c) || isPunctuation(c)), null, TokenType.Symbol));
                        return Tokenise(line.SkipWhile(c => isSymbol(c) || isPunctuation(c)), soFar);
                    }
                    else if (isPunctuation(line.First())) // this solves != having punctuation followed by a symbol
                    {
                        soFar.Add((token(c => isPunctuation(c) || isSymbol(c)), null, TokenType.Symbol));
                        return Tokenise(line.SkipWhile(c => isPunctuation(c) || isSymbol(c)), soFar);
                    }
                    else
                    {
                        Debug.Fail("Tokenise does not cover all possible classes");
                        return soFar.ToList();
                    }
                }

                void Error(string message)
                {
                    Errors.Add($"{message}: {line.Trim()}");
                }

                (string label, int? immediate, int? register, ErrorList Errors, Operand2Type type) Operand2(Token operand, ErrorList Errors, string line)
                {
                    switch (operand.type)
                    {
                        case TokenType.Literal:
                            return ("", operand.intOrRegisterNumber, null, Errors, Operand2Type.Immediate);
                        case TokenType.Register:
                            return ("", null, operand.intOrRegisterNumber, Errors, Operand2Type.Register);
                        case TokenType.Identifier:
                            return (operand.text, null, null, Errors, Operand2Type.Label);
                        default:
                            Error($"Invalid operand2 {operand.text}");
                            return ("", null, NO_REGISTER, Errors, Operand2Type.Error);
                    }
                }

                (string, int?, List<string>, MemoryReferenceType type) MemoryRef(Token reference, List<String> Errors, string line)
                {
                    switch (reference.type)
                    {
                        case TokenType.Literal:
                            return ("", reference.intOrRegisterNumber, Errors, MemoryReferenceType.Literal);
                        case TokenType.Identifier:
                            return (reference.text, null, Errors, MemoryReferenceType.Label);
                        case TokenType.Register:
                            Error($"Indexed addressing is not currently supported");
                            return ("", null, Errors, MemoryReferenceType.Error);
                        default:
                            Errors.Add($"Invalid memory reference");
                            return ("", null, Errors, MemoryReferenceType.Error);
                    }
                }

                Instruction assembledInstruction = new EmptyInstruction("");

                string? label = "";
                string? opcode;
                int? Rd;
                int? Rn;
                int? Rm;
                string? operand;
                int? immediate;
                Operand2Type operandType;
                MemoryReferenceType memoryType;
                string? memory;
                int? address;

                IEnumerable<Token> tokens = Tokenise(line.ToUpper(), new List<(string, int?, TokenType)>());

                Token token(int index) => tokens.ElementAt(index);

                if (tokens.Count() > 1 && token(0).type == TokenType.Identifier && token(1).text == ":")
                {
                    label = tokens.First().text;
                    assembledInstruction = new EmptyInstruction(label);
                    tokens = tokens.Skip(2);
                }
                else
                {
                    label = "";
                }

                if (tokens.Any())
                {
                    switch (tokens.First().type)
                    {
                        case TokenType.Literal:
                            assembledInstruction = new DataInstruction(label, tokens.First().intOrRegisterNumber);
                            break;
                        case TokenType.Symbol:
                            Error($"HLA statements can't begin with a symbol");
                            break;
                        case TokenType.Register:
                            if (tokens.Count() > 1 && token(1).text == ASSIGNMENT)
                            {
                                Rd = token(0).intOrRegisterNumber;
                                Debug.Assert(Rd.HasValue);
                                switch (tokens.Count())
                                {
                                    case 6:
                                        // Rd = MEM [ <memory ref> ] => LDR  Rd, <memory ref>
                                        // 0  1 2   3 4            5
                                        if (token(2).text == MEMORY && token(3).text == MEMORY_ACCESS_START && token(5).text == MEMORY_ACCESS_END)
                                        {
                                            (memory, address, Errors, memoryType) = MemoryRef(token(4), Errors, line);

                                            switch (memoryType)
                                            {
                                                case MemoryReferenceType.Literal:
                                                    assembledInstruction = new MemoryReferenceInstruction(label, "LDR", Rd.Value, address!.Value);
                                                    break;
                                                case MemoryReferenceType.Label:
                                                    assembledInstruction = new MemoryReferenceInstruction(label, "LDR", Rd.Value, memory);
                                                    break;
                                                default:
                                                    Error($"Can only access a memory location with a number or a label");
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            Error($"Invalid syntax for a memory read");
                                        }
                                        break;
                                    case 5:
                                        // Rd = Rn <operator> <operand2> => <operator translation>  Rd, Rn, <operand2>
                                        // 0  1 2  3          4
                                        // <operator> is +, -, AND_OPERATOR, OR_OPERATOR, XOR_OPERATOR, <<, >>
                                        // see the case 4: branch for NOT handling
                                        if (token(2).type == TokenType.Register)
                                        {
                                            if (arithmeticLogicOperators.ContainsKey(token(3).text))
                                            {
                                                Rn = token(2).intOrRegisterNumber;
                                                Debug.Assert(Rn.HasValue);
                                                opcode = arithmeticLogicOperators[token(3).text];
                                                (operand, immediate, Rm, Errors, operandType) = Operand2(token(4), Errors, line);
                                                switch (operandType)
                                                {
                                                    case Operand2Type.Register:
                                                        Debug.Assert(Rn.HasValue && Rm.HasValue); // means we don't need a ! (!)
                                                        assembledInstruction = new ArithmeticLogicRegisterInstruction(label, opcode, Rd.Value, Rn.Value, Rm.Value);
                                                        break;
                                                    case Operand2Type.Immediate:
                                                        Debug.Assert(immediate.HasValue);
                                                        assembledInstruction = new ArithmeticLogicImmediateInstruction(label, opcode, Rd.Value, Rn.Value, immediate.Value);
                                                        break;
                                                    case Operand2Type.Label: // TODO: allow Rd = Rn + <label or identifier>
                                                    default:
                                                        Error($"Arithmetic / logic operators can only have a right side of register or number");
                                                        break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (token(2).type == TokenType.Literal || token(2).type == TokenType.Identifier)
                                                Error($"Invalid arithmetic / logic statement (number or symbol must come at end)");
                                        }
                                        break;
                                    case 4:
                                        // Rd = ! <operand2> => MVN  Rd, <operand2>
                                        // 0  1 2 3
                                        if (token(2).text == NOT_OPERATOR)
                                        {
                                            opcode = arithmeticLogicOperators[NOT_OPERATOR];
                                            (operand, immediate, Rm, Errors, operandType) = Operand2(token(3), Errors, line);

                                            switch (operandType)
                                            {
                                                case Operand2Type.Register:
                                                    Debug.Assert(Rm.HasValue);
                                                    assembledInstruction = new SingleRegisterRegisterInstruction(label, opcode, Rd.Value, Rm.Value);
                                                    break;
                                                case Operand2Type.Immediate:
                                                    Debug.Assert(immediate.HasValue);
                                                    assembledInstruction = new SingleRegisterImmediateInstruction(label, opcode, Rd.Value, immediate.Value, "");
                                                    break;
                                                default:
                                                    Error($"Currently only registers or numbers can be used in an {NOT_OPERATOR} statement");
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            Error($"Invalid monadic operator {token(2).text} (currently only {NOT_OPERATOR} is supported)");
                                        }
                                        break;
                                    case 3:
                                        // Rd = <operand2> => MOV  Rd, <operand2>
                                        // 0  1 2
                                        (operand, immediate, Rm, Errors, operandType) = Operand2(token(2), Errors, line);
                                        switch (operandType)
                                        {
                                            case Operand2Type.Immediate:
                                                Debug.Assert(immediate.HasValue);
                                                assembledInstruction = new SingleRegisterImmediateInstruction(label, "MOV", Rd.Value, immediate.Value, "");
                                                break;
                                            case Operand2Type.Register:
                                                Debug.Assert(Rm.HasValue);
                                                assembledInstruction = new SingleRegisterRegisterInstruction(label, "MOV", Rd.Value, Rm.Value);
                                                break;
                                            case Operand2Type.Label:
                                            default:
                                                Error($"Currently you can only MOV a register or a number");
                                                break;
                                        }
                                        break;
                                    case 2: // can't think of something you'd do with just two tokens (Rd CLEAR?)
                                    default:
                                        Error($"Unexpected end of statement");
                                        break;
                                }
                            }
                            else
                            {
                                Error($"Missing {ASSIGNMENT}");
                            }
                            break;
                        case TokenType.Keyword:
                            switch (tokens.First().text)
                            {
                                case "IF":
                                    //  IF Rn <relop> <operand2> GOTO <label> => CMP  Rn, <operand2>
                                    //  0  1  2       3          4    5          B<relop> <label>
                                    if (tokens.Count() == 6)
                                    {
                                        if (token(4).type == TokenType.Keyword && (new string[] { "GOTO", "GO", "BRANCH" }).Contains(token(4).text))
                                        {
                                            if (token(2).type == TokenType.Symbol && relationalOperators.ContainsKey(token(2).text))
                                            {
                                                opcode = relationalOperators[token(2).text];
                                                (operand, immediate, Rm, Errors, operandType) = Operand2(token(3), Errors, line);
                                                switch (operandType)
                                                {
                                                    case Operand2Type.Register:
                                                        Debug.Assert(token(3).intOrRegisterNumber.HasValue);
                                                        Assembly.Add((label,
                                                            PC,
                                                            new SingleRegisterRegisterInstruction(label, "CMP", RegisterNumber(token(1).text), token(3).intOrRegisterNumber!.Value)));
                                                        break;
                                                    case Operand2Type.Immediate:
                                                        Assembly.Add((label,
                                                            PC,
                                                            new SingleRegisterImmediateInstruction(label, "CMP", RegisterNumber(token(1).text), immediate!.Value, line.Trim())));
                                                        break;
                                                    default:
                                                        Error($"Currently IF statements can only test a register against a number or another register");
                                                        break;
                                                }
                                                PC += 1;

                                                label = ""; // to avoid duplicating any label for the IF on the branch
                                                address = MemoryAddress(token(5).text, Assembly);
                                                if (address.HasValue)
                                                {
                                                    assembledInstruction = new ConditionalBranchInstruction(label, opcode, address.Value, line.Trim());
                                                }
                                                else
                                                {
                                                    // we don't know where the destination is yet but will patch it up at the end (or issue an error)
                                                    assembledInstruction = new ConditionalBranchInstruction(label, opcode, token(5).text, line.Trim());
                                                }
                                            }
                                            else
                                            {
                                                Error($"You can only use one of {String.Join("/", relationalOperators.Keys)} for tests");
                                            }
                                        }
                                        else
                                        {
                                            Error($"If the IF test succeeds you can only GOTO, GO, or BRANCH");
                                        }
                                    }
                                    else
                                    {
                                        Error($"Unexpected end of line (or too many tokens) for IF");
                                    }
                                    break;
                                case "MEM":
                                    // (MEM [ <memory ref> ] = Rd) => STR  Rd, <memory ref>
                                    //  0   1 2            3 4 5
                                    if (tokens.Count() == 6 &&
                                        token(1).text == MEMORY_ACCESS_START &&
                                        token(3).text == MEMORY_ACCESS_END &&
                                        token(4).text == ASSIGNMENT)
                                    {
                                        if (tokens.Last().type == TokenType.Register)
                                        {
                                            switch (token(2).type)
                                            {
                                                case TokenType.Literal:
                                                    assembledInstruction = new MemoryReferenceInstruction(label,
                                                        "STR",
                                                        tokens.Last().intOrRegisterNumber,
                                                        token(2).intOrRegisterNumber);
                                                    break;
                                                case TokenType.Identifier:
                                                    assembledInstruction = new MemoryReferenceInstruction(label,
                                                         "STR", tokens.Last().intOrRegisterNumber,
                                                         token(2).text);
                                                    break;
                                                case TokenType.Register:
                                                    Error($"Currently the CPU model does not support indexed addressing");
                                                    break;
                                                default:
                                                    Error($"Invalid memory location (use a label or a number)");
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            Error($"You can only store a register into memory");
                                        }
                                    }
                                    else
                                    {
                                        Error($"Invalid syntax for a memory write");
                                    }
                                    break;
                                case "GOTO":
                                case "GO":
                                case "BRANCH":
                                    // GOTO <label> => B    <label>
                                    // 0    1
                                    if (tokens.Count() == 2)
                                    {
                                        switch (token(1).type)
                                        {
                                            case TokenType.Literal:
                                                Debug.Assert(token(1).intOrRegisterNumber.HasValue);
                                                assembledInstruction = new BranchInstruction(label, "B", token(1).intOrRegisterNumber!.Value);
                                                break;
                                            case TokenType.Identifier:
                                                assembledInstruction = new BranchInstruction(label, "B", token(1).text);
                                                break;
                                            case TokenType.Register: // TODO: indirect via register?
                                            default:
                                                Error($"The destination of a {token(0).text} can only be an address or a label");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        Error($"A {token(0).text} statement must have a destination");
                                    }
                                    break;
                                case "END":
                                case "STOP":
                                case "HALT":
                                    assembledInstruction = new Instruction(label, "HALT");
                                    break;
                                case "DATA":
                                    assembledInstruction = new DataInstruction(label, null);
                                    break;
                                default:
                                    Error($"HLA statements can't start with the keyword {tokens.First().text}");
                                    break;
                            }
                            break;
                        case TokenType.Identifier:
                        default:
                            break;
                    }

                    Assembly.Add((label, PC, assembledInstruction));
                    PC += 1;
                }
                else
                {
                    if (label != "")
                    {
                        //  allows a line that only has a label
                        Assembly.Add((label, PC, new EmptyInstruction(label)));
                    }
                }
            }
            return (Patchup(Assembly, Errors), Errors);
        }

        static AssembledCode Patchup(AssembledCode Assembly, ErrorList Errors)
        {
            // TODO: see if there's a simpler way to do this. Make the address / destination read / write?
            AssembledCode result = new();
            foreach ((string label, int PC, Instruction code) in Assembly)
            {
                if (code is MemoryReferenceInstruction mri)
                {
                    if (mri.AddressLabel != null && mri.AddressLabel != "")
                    {
                        int? address = MemoryAddress(mri.AddressLabel, Assembly);
                        if (address.HasValue)
                        {
                            result.Add((label, PC, new MemoryReferenceInstruction(mri.label.text ?? "", mri.Opcode, mri.Register, address.Value)));
                        }
                        else
                        {
                            Errors.Add($"Address {mri.AddressLabel} not defined in {mri.Source.Trim()}");
                        }
                    }
                    else
                    {
                        result.Add((label, PC, mri));
                    }
                }
                else if (code is BranchInstruction bi)
                {
                    if (bi.DestinationLabel != null && bi.DestinationLabel != "")
                    {
                        int? address = MemoryAddress(bi.DestinationLabel, Assembly);
                        if (address.HasValue)
                        {
                            result.Add((label, PC, new BranchInstruction(bi.label.text ?? "", bi.Opcode, address.Value)));
                        }
                        else
                        {
                            Errors.Add($"Address {bi.DestinationLabel} not defined in {bi.Source.Trim()}");
                        }
                    }
                    else
                    {
                        result.Add((label, PC, bi));
                    }
                }
                else if (code is ConditionalBranchInstruction cbi)
                {
                    if (cbi.DestinationLabel != null && cbi.DestinationLabel != "")
                    {
                        int? address = MemoryAddress(cbi.DestinationLabel, Assembly);
                        if (address.HasValue)
                        {
                            result.Add((label, PC, new ConditionalBranchInstruction(cbi.label.text ?? "", cbi.Opcode, address.Value, cbi.SourceStatement)));
                        }
                        else
                        {
                            Errors.Add($"Address {cbi.DestinationLabel} not defined in {cbi.Source.Trim()}");
                        }
                    }
                    else
                    {
                        result.Add((label, PC, cbi));
                    }
                }
                else
                {
                    result.Add((label, PC, code));
                }
            }
            return result;
        }
        static bool IsRegisterRef(string RegisterRef)
        {
            // TODO: think about a constant for "R" so could be changed to REGISTER
            // TODO: Range checking on register numbers?
            if (RegisterRef.StartsWith("R") && RegisterRef[1..].All(c => Char.IsDigit(c)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static void DisplayErrors(List<string> Program, List<string> Errors)
        {
            static int Find(string s, List<string> inList)
            {
                var position = 0;
                while (position < inList.Count)
                {
                    if (s.IndexOf(inList[position], StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        return position;
                    }
                    position += 1;
                }
                Debug.Fail($"Couldn't find program part of {s}");
                return position;
            }

            foreach (var errorDetail in Errors)
                Console.WriteLine($"Error at line {Find(errorDetail, Program) + 1}:  {errorDetail.Trim()}");
        }

        static void ExecuteAssembly(AssembledCode Assembly)
        {
            //  LDR  Rd, <memory ref>
            //  STR  Rd, <memory ref>
            //  ADD  Rd, Rn, <operand2>
            //  SUB  Rd, Rn, <operand2>
            //  MOV  Rd, <operand2>
            //  CMP  Rn, <operand2>
            //  B            <label>
            //  B<condition> <label>
            //  AND Rd, Rn, <operand2>
            //  ORR Rd, Rn, <operand2>
            //  EOR Rd, Rn, <operand2>
            //  MVN Rd, <operand2>
            //  LSL Rd, Rn, <operand2>
            //  LSR Rd, Rn, <operand2>
            //  HALT

            Nullable<int>[] Memory = new int?[NUMBER_OF_MEMORY_LOCATIONS];
            Nullable<int>[] Registers = new int?[NUMBER_OF_REGISTERS];
            // Memory[<address>} = null or Registers[<RegisterNumber>] = null => something being read has
            //  not been initialised by the program being executed / interpreted.
            // Every element of Memory / Registers is automatically set to null initially.

            int? PC = 0;
            var Running = true;
            var ErrorMessage = "";
            bool[] Status = { false, false }; //  EQ, GT flags
            while (Running && ErrorMessage == "" && PC.HasValue && PC < Assembly.Count)
            {
                // Fetch
                int? MAR = PC;
                Instruction MDR = Assembly[MAR.Value].instruction;
                Instruction CIR = MDR;
                PC += 1;

                // Execute
                (Running, ErrorMessage, PC, Status, Registers, Memory) = CIR.Execute(PC, Status, Registers, Memory);
            }
            if (ErrorMessage != "")
            {
                // Exception handling
                Console.WriteLine($"Program terminated with error: {ErrorMessage}");
            }
            DisplayRegisters(Registers);
            Console.WriteLine();
            DisplayMemory(Memory);
        }

        static int RegisterNumber(string Register)
        {
            return Convert.ToInt32(Register[1..]);
        }

        static int? LabelAddress(AssembledCode Assembly, string LabelToFind)
        {
            int? Destination = null;
            foreach ((string label, int address, Instruction instruction) in Assembly)
            {
                if (instruction.label.text == LabelToFind)
                {
                    Destination = address;
                }
            }
            return Destination;
        }

        static int? MemoryAddress(string MemoryRef, AssembledCode Assembly)
        {
            if (MemoryRef.All(c => Char.IsDigit(c)))
            {
                return Convert.ToInt32(MemoryRef);
            }
            else
            { // assume a label
                return LabelAddress(Assembly, MemoryRef);
            }
        }

        static string Binary(int? n, int width)
        {
            Debug.Assert(n.HasValue);
            var result = "";
            do
            {
                if (n % 2 == 0)
                {
                    result = "0" + result;
                }
                else
                {
                    result = "1" + result;
                }
                n /= 2;
            } while (n != 0);
            while (result.Length < width)
            {
                result = $"0{result}";
            }
            return result;
        }

        static void DisplayRegisters(int?[] Registers)
        {
            foreach (var i in Enumerable.Range(0, Registers.Length))
            {
                Console.WriteLine($"R{i,-2} = {Registers[i],10} {Registers[i]:x6} {Binary(Registers[i], 16)}");
            }
        }

        static void DisplayMemory(int?[] Memory)
        {
            //  TODO: display labels for memory locations
            foreach (var i in Enumerable.Range(0, Memory.Length))
            {
                if (Memory[i].HasValue)  // so we only display memory that has something in it
                {
                    Console.WriteLine($"{MEMORY}{MEMORY_ACCESS_START}{i,4}{MEMORY_ACCESS_START} = {Memory[i],10} {Memory[i]:x6} {Binary(Memory[i], 16)} \"{(Memory[i] >= 32 && Memory[i] <= 127 ? (char)Memory[i]!.Value : "")}\"");
                }
            }
            Console.WriteLine(new String('-', 50));
        }

        static SourceCode NewHLAProgram()
        {
            Console.WriteLine("Enter your program line by line. Just press Enter to finish");
            var lineNumber = 1;
            SourceCode Program = new();
            do
            {
                string? line;
                Console.Write($"Line {lineNumber}: ");
                line = Console.ReadLine(); // anyone know how Console.ReadLine could return null?
                if (line != "")
                {
                    Debug.Assert(line != null);
                    Program = Program.Append(line).ToList();
                    lineNumber += 1;
                }
                else
                {
                    break;
                }
            } while (true);

            return Program;
        }

        static void DisplayHelp()
        {  // TODO: see about an RTF version
            const string HELP_FILENAME = "HLA.hlp";
            if (File.Exists(HELP_FILENAME))
            {
                try
                {
                    Process.Start("NOTEPAD.EXE", HELP_FILENAME);
                }
                catch
                {
                    Console.WriteLine($"Could not start NOTEPAD or find the help file ({HELP_FILENAME})");
                }
            }
        }

    }
}
