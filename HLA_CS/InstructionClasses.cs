using System.Diagnostics;

namespace CS_HLA
{
    // TODO: CPU?
    // TODO: put Instructions in memory

    internal class Label
    {
        public readonly string? text;

        public Label(string? label)
        {
            this.text = label;
        }

        public override string ToString()
        {
            return $"{(text == null || text == "" ? "" : text + ':'),-10}";
        }
    }

    internal class Instruction
    {
        public const string NO_ERROR_MESSAGE = "";

        public static Dictionary<string, string> arithmeticLogicOperators;

        public static Dictionary<string, string> relationalOperators;

        static Instruction()
        {   // invert / transpose the dictionaries that convert from HLA to assembler so the Source methods can
            // do the reverse
            arithmeticLogicOperators = HLA.arithmeticLogicOperators.ToDictionary(kv => kv.Value, kv => kv.Key);
            relationalOperators = HLA.relationalOperators.ToDictionary(kv => kv.Value, kv => kv.Key);
        }

        public const bool EXECUTION_OK = true;
        public const bool EXECUTION_FAILED = false;

        public const int EQ_FLAG = 0; // TODO: belong in CPU model
        public const int GT_FLAG = 1;

        public string Opcode { get; }

        public readonly Label label;

        public Instruction(string? label, string opcode)
        {
            this.label = new Label(label);
            this.Opcode = opcode;
        }

        // TODO: have a structure/class for the CPU with registers, PC, Memory etc.
        /// <summary>
        /// Executes the instruction.
        /// </summary>
        /// <param name="PC">the updated PC if changed otherwise null</param>
        /// <param name="status">the status flags</param>
        /// <param name="registers">the registers</param>
        /// <param name="memory"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public virtual (bool, string, int?, bool[], int?[], int?[]) Execute(int? PC, bool[] status, int?[] registers, int?[] memory)
        {
            return Opcode switch
            {
                "HALT" => (EXECUTION_OK, NO_ERROR_MESSAGE, null, status, registers, memory),
                "DATA" => (EXECUTION_FAILED, $"Attempting to execute data at {PC}", null, status, registers, memory),
                _ => (EXECUTION_FAILED, $"Invalid opcode {Opcode}", null, status, registers, memory)
            };
        }

        public override string ToString()
        {
            return $"{label} {Opcode,-4}";
        }

        public string Source { get => $"{label} {(Opcode == "DATA" ? Opcode : HLA.END)}"; }
    }

    internal class EmptyInstruction : Instruction
    {   // Just used to label empty lines
        public EmptyInstruction(string label) : base(label, "")
        {
        }

        public override string ToString()
        {
            return $"{label}";
        }

        public override (bool, string, int?, bool[], int?[], int?[]) Execute(int? PC, bool[] status, int?[] registers, int?[] memory)
        {
            // executing an empty instruction doesn't do anything
            return (EXECUTION_OK, NO_ERROR_MESSAGE, PC, status, registers, memory);
        }

        public new string Source { get => $"{label}"; }
    }

    internal class DataInstruction : Instruction
    {
        int? value;

        public DataInstruction(string label, int? value) : base(label, "DATA")
        {
            this.value = value;
        }

        public override string ToString()
        {
            return $"{label} {(value.HasValue ? value.Value : "DATA")}";
        }

        public new string Source { get => $"{label} {(value.HasValue ? value.Value : "DATA")}"; }
    }

    internal class BranchInstruction : Instruction
    {
        public int? Destination { get; }
        public string? DestinationLabel { get; }

        public BranchInstruction(string label, string opcode, int destination) : base(label, opcode)
        {
            Debug.Assert(opcode.StartsWith("B"));
            this.Destination = destination;
            this.DestinationLabel = "";
        }

        public BranchInstruction(string label, string opcode, string destination) : base(label, opcode)
        {
            Debug.Assert(opcode.StartsWith("B"));
            this.Destination = null;
            this.DestinationLabel = destination;
        }

        public override (bool, string, int?, bool[], int?[], int?[]) Execute(int? PC, bool[] status, int?[] registers, int?[] memory)
        {
            return (EXECUTION_OK, NO_ERROR_MESSAGE, Destination, status, registers, memory);
        }

        public override string ToString()
        {
            return $"{label,-10} {"B",-4} {(Destination.HasValue ? Destination : DestinationLabel)}";
        }

        public new string Source { get => $"{label} {HLA.GOTO,-4} {(Destination.HasValue ? Destination : DestinationLabel)}"; }
    }

    internal class ConditionalBranchInstruction : BranchInstruction
    {
        public string SourceStatement; // Holds the IF statement that this is part of

        public ConditionalBranchInstruction(string label, string opcode, int destination, string IFStatement) : base(label, opcode, destination)
        {
            this.SourceStatement = IFStatement;
        }

        public ConditionalBranchInstruction(string label, string opcode, string destination, string SourceStatement) : base(label, opcode, destination)
        {
            this.SourceStatement = SourceStatement;
        }

        public override (bool, string, int?, bool[], int?[], int?[]) Execute(int? PC, bool[] status, int?[] registers, int?[] memory)
        {
            return Opcode switch
            {   // Note that the FE cycle in ExecuteAsembly has already incremented PC, so we don't do anything here
                "BEQ" => (EXECUTION_OK, NO_ERROR_MESSAGE, status[EQ_FLAG] ? Destination : PC, status, registers, memory),
                "BNE" => (EXECUTION_OK, NO_ERROR_MESSAGE, status[EQ_FLAG] ? PC : Destination, status, registers, memory),
                "BGT" => (EXECUTION_OK, NO_ERROR_MESSAGE, status[GT_FLAG] ? Destination : PC, status, registers, memory),
                "BLT" => (EXECUTION_OK, NO_ERROR_MESSAGE, !status[GT_FLAG] && !status[EQ_FLAG] ? Destination : PC, status, registers, memory),
                _ => (EXECUTION_FAILED, $"Invalid opcode {Opcode}", null, status, registers, memory)
            };
        }

        public override string ToString()
        {
            return $"{label} {Opcode,-4} {(Destination.HasValue ? Destination : DestinationLabel)}";
        }

        // it is possible that there are unresolved labels in an IF statement both in the CMP and the conditional branch. This means
        // that HLA.Patch may output the same IF statement twice but so what?
        public new string Source { get => SourceStatement; }
    }

    internal class MemoryReferenceInstruction : Instruction
    {
        public int? Address { get; }
        public string? AddressLabel { get; }
        public int? Register { get; }

        public MemoryReferenceInstruction(string label, string opcode, int? register, int? address) : base(label, opcode)
        {
            Debug.Assert(register >= 0 && register < HLA.NUMBER_OF_REGISTERS);
            Debug.Assert(address >= 0 && address < HLA.NUMBER_OF_MEMORY_LOCATIONS);
            this.Register = register;
            this.Address = address;
            this.AddressLabel = "";
        }

        public MemoryReferenceInstruction(string label, string opcode, int? register, string? address) : base(label, opcode)
        {
            // For a forward reference we create this and then patch it up
            Debug.Assert(register >= 0 && register < HLA.NUMBER_OF_REGISTERS);
            this.Register = register;
            this.AddressLabel = address;
        }

        public override (bool, string, int?, bool[], int?[], int?[]) Execute(int? PC, bool[] status, int?[] registers, int?[] memory)
        {
            bool ValidAddress(int location) => memory[location].HasValue && location >= 0 && location < HLA.NUMBER_OF_MEMORY_LOCATIONS;
            Debug.Assert(Address.HasValue);
            switch (Opcode)
            {
                case "LDR":

                    // once we've compiled we patch up labels to addresses
                    if (ValidAddress(Address.Value))
                    {
                        Debug.Assert(Register.HasValue);
                        registers[Register.Value] = memory[Address.Value]!.Value;
                        return (EXECUTION_OK, NO_ERROR_MESSAGE, PC, status, registers, memory);
                    }
                    else
                    {
                        return (EXECUTION_FAILED, $"Invalid memory access of location {Address} at {PC}", null, status, registers, memory);
                    }
                case "STR":
                    Debug.Assert(Register.HasValue);
                    memory[Address.Value] = registers[Register.Value];
                    return (EXECUTION_OK, NO_ERROR_MESSAGE, PC, status, registers, memory);
                default:
                    return (EXECUTION_FAILED, $"Invalid opcode {Opcode}", null, status, registers, memory);
            }
        }

        public override string ToString()
        {
            return $"{label} {Opcode,-4} R{Register}, {(Address.HasValue ? Address.Value : AddressLabel)}";
        }

        public new string Source
        {
            get
            {
                string memory = $"{HLA.MEMORY}{HLA.MEMORY_ACCESS_START}{(Address.HasValue ? Address : AddressLabel)}{HLA.MEMORY_ACCESS_END}";
                switch (Opcode)
                {
                    case "LDR":
                        return $"R{Register} {HLA.ASSIGNMENT} {memory}";
                    case "STR":
                        return $"{memory} {HLA.ASSIGNMENT} R{Register}";
                    default:
                        return "Memory reference instruction not using LDR or STR";
                }
            }
        }
    }

    internal class ArithmeticLogicInstruction : Instruction
    {
        public int Rd { get; }
        public int Rn { get; }

        public ArithmeticLogicInstruction(string label, string opcode, int Rd, int Rn) : base(label, opcode)
        {
            Debug.Assert(Rd >= 0 && Rd < HLA.NUMBER_OF_REGISTERS);
            Debug.Assert(Rn >= 0 && Rn < HLA.NUMBER_OF_REGISTERS);
            this.Rd = Rd;
            this.Rn = Rn;
        }

        public int? Perform(int left, int right)
        {
            return Opcode switch
            {
                "ADD" => left + right,
                "SUB" => left - right,
                "AND" => left & right,
                "ORR" => left | right,
                "EOR" => left ^ right,
                "LSL" => left << right,
                "LSR" => left >> right,
                _ => null
            };
        }
    }

    internal class ArithmeticLogicRegisterInstruction : ArithmeticLogicInstruction
    {
        public int Rm { get; }

        public ArithmeticLogicRegisterInstruction(string label, string opcode, int Rd, int Rn, int Rm) : base(label, opcode, Rd, Rn)
        {
            Debug.Assert(Rm >= 0 && Rm < HLA.NUMBER_OF_REGISTERS);
            this.Rm = Rm;
        }

        public override (bool, string, int?, bool[], int?[], int?[]) Execute(int? PC, bool[] status, int?[] registers, int?[] memory)
        {
            Debug.Assert(registers[Rn].HasValue && registers[Rm].HasValue);
            int? result = Perform(registers[Rn]!.Value, registers[Rm]!.Value);
            if (result.HasValue)
            {
                registers[Rd] = result.Value;
                return (EXECUTION_OK, NO_ERROR_MESSAGE, PC, status, registers, memory);
            }
            else
            {
                return (EXECUTION_FAILED, $"Invalid opcode {Opcode}", null, status, registers, memory);
            }
        }

        public override string ToString()
        {
            return $"{label} {Opcode,-4} R{Rd}, R{Rn}, R{Rm}";
        }

        public new string Source { get => $"{label} R{Rd} {HLA.ASSIGNMENT} R{Rn} {Instruction.arithmeticLogicOperators[Opcode]} R{Rm}"; }

    }

    internal class ArithmeticLogicImmediateInstruction : ArithmeticLogicInstruction
    {
        public int Immediate { get; }

        public ArithmeticLogicImmediateInstruction(string label, string opcode, int Rd, int Rn, int immediate) : base(label, opcode, Rd, Rn)
        {
            this.Immediate = immediate;
        }

        public override (bool, string, int?, bool[], int?[], int?[]) Execute(int? PC, bool[] status, int?[] registers, int?[] memory)
        {
            int? result = Perform(registers[Rn]!.Value, Immediate);
            if (result.HasValue)
            {
                registers[Rd] = result.Value;
                return (EXECUTION_OK, NO_ERROR_MESSAGE, PC, status, registers, memory);
            }
            else
            {
                return (EXECUTION_FAILED, $"Invalid opcode {Opcode}", null, status, registers, memory);
            }
        }

        public override string ToString()
        {
            return $"{label} {Opcode,-4} R{Rd}, R{Rn}, #{Immediate}";
        }

        public new string Source { get => $"{label} R{Rd} {HLA.ASSIGNMENT} R{Rn} {Instruction.arithmeticLogicOperators[Opcode]} {Immediate}"; }

    }

    internal class SingleRegisterInstruction : Instruction
    {   // Oddities such as MOV, CMP, and MVN with single register and <operand2>
        // TODO: SingleRegisterInstruction isn't quite the right name

        public string IFStatement;

        public int Rd { get; }

        public SingleRegisterInstruction(string label, string opcode, int Rd, string IFStatement) : base(label, opcode)
        {
            this.Rd = Rd;
            this.IFStatement = IFStatement;
        }

        public (int?[], bool[], bool, string) Perform(int?[] registers, int value, bool[] status)
        {
            switch (Opcode)
            {
                case "MOV":
                    registers[Rd] = value;
                    return (registers, status, EXECUTION_OK, NO_ERROR_MESSAGE);
                case "CMP":
                    return (registers, new bool[] { registers[Rd] == value, registers[Rd] > value }, EXECUTION_OK, NO_ERROR_MESSAGE);
                case "MVN":
                    registers[Rd] = ~value;
                    return (registers, status, EXECUTION_OK, NO_ERROR_MESSAGE);
                default:
                    return (registers, status, EXECUTION_FAILED, $"Invalid opcode {Opcode}");
            }
        }
    }

    internal class SingleRegisterRegisterInstruction : SingleRegisterInstruction
    {
        public int Rm { get; }

        public SingleRegisterRegisterInstruction(string label, string opcode, int Rd, int Rm) : base(label, opcode, Rd, NO_ERROR_MESSAGE)
        {
            this.Rm = Rm;
        }

        public override (bool, string, int?, bool[], int?[], int?[]) Execute(int? PC, bool[] status, int?[] registers, int?[] memory)
        {   // TODO: any need to return arrays?
            (int?[] registers, bool[] status, bool OK, string message) result = Perform(registers, registers[Rm]!.Value, status);
            if (result.OK)
            {
                return (EXECUTION_OK, NO_ERROR_MESSAGE, PC, status, registers, memory);
            }
            else
            {
                return (EXECUTION_FAILED, result.message, null, status, registers, memory);
            }
        }

        public override string ToString()
        {
            return $"{label} {Opcode,-4} R{Rd}, R{Rm}";
        }

        public new string Source
        {
            get
            {
                switch (Opcode)
                {
                    case "MVN":
                        return $"{label} R{Rd} {HLA.ASSIGNMENT} {HLA.NOT_OPERATOR} R{Rm}";
                    case "CMP":
                        return IFStatement;
                    case "MOV":
                        return $"{label} R{Rd} {HLA.ASSIGNMENT} R{Rm}";
                    default:
                        return "Instruction not using MVN, CMP, or MOV"; // slightly odd error message
                }
            }
        }
    }

    internal class SingleRegisterImmediateInstruction : SingleRegisterInstruction
    {
        public int Immediate { get; }

        public SingleRegisterImmediateInstruction(string label, string opcode, int Rd, int immediate, string IFStatement) : base(label, opcode, Rd, IFStatement)
        {
            this.Immediate = immediate;
        }

        public override (bool, string, int?, bool[], int?[], int?[]) Execute(int? PC, bool[] status, int?[] registers, int?[] memory)
        {
            (registers, status, bool OK, string message) = Perform(registers, Immediate, status);
            if (OK)
            {
                return (EXECUTION_OK, NO_ERROR_MESSAGE, PC, status, registers, memory);
            }
            else
            {
                return (EXECUTION_FAILED, message, null, status, registers, memory);
            }
        }

        public override string ToString()
        {
            return $"{label} {Opcode,-4} R{Rd}, #{Immediate}";
        }

        public new string Source
        {
            get
            {
                switch (Opcode)
                {
                    case "MVN":
                        return $"{label} R{Rd} {HLA.ASSIGNMENT} {HLA.NOT_OPERATOR} {Immediate}";
                    case "CMP":
                        return IFStatement;
                    case "MOV":
                        return $"{label} R{Rd} {HLA.ASSIGNMENT} {Immediate}";
                    default:
                        return "Instruction not using MVN, CMP, or MOV"; // slightly odd error message
                }
            }

        }

    }
}

