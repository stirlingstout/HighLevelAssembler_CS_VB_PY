Imports System
Imports System.Data
Imports System.Diagnostics.Tracing
Imports System.IO
Imports System.Net.NetworkInformation
Imports System.Reflection.Emit
Imports System.Runtime.CompilerServices

Namespace HLA_VB
    ' TODO: think about enumerations for symbols and/or operators

    Public Module Program
        Const HLA_EXTENSION = ".hla"

        Public Const HIGHEST_MEMORY_ADDRESS = 1024
        Public Const HIGHEST_REGISTER_NUMBER = 15
        Public Const REGISTER_SIZE = 32

        Function ValidRegister(r As Integer) As Boolean
            Return r >= 0 And r <= HIGHEST_REGISTER_NUMBER
        End Function

        Function ValidAddress(a As Integer) As Boolean
            Return a >= 0 And a <= HIGHEST_MEMORY_ADDRESS
        End Function

        Public Class MemoryLocation
            Public Const MEMORY_LABEL_WIDTH = 10

            Private labels As List(Of String)

            Sub New()
                labels = New List(Of String)
            End Sub

            Sub AddLabel(label As String)
                labels.Add(label)
            End Sub

            Function HasLabel(label As String) As Boolean
                Return labels.Contains(label)
            End Function

            Public Overrides Function ToString() As String
                If labels.Count = 0 Then
                    Return $"{String.Empty,-10} " ' Note space at end
                Else
                    Return String.Join(Environment.NewLine, labels.Select(Function(label) (label + ":").PadRight(10))) + " "
                End If
            End Function

            Overridable Function GetValue() As Integer
                Throw New Exception($"Attempt to get value of an instruction")
            End Function

            Overridable Sub SetValue(value As Integer)
                Throw New Exception($"Attempt to overwrite an instruction")
            End Sub

            Overridable Sub Execute(r As Registers, m As Memory)
                Throw New Exception($"Attempt to execute data as load/store instruction")
            End Sub

            Overridable Sub Execute(r As Registers)
                Throw New Exception($"Attempt to execute data as arithmetic/logic instruction")
            End Sub
        End Class

        Class Data
            Inherits MemoryLocation

            Private value As Integer

            Sub New()
                value = 0
            End Sub

            Sub New(value As Integer)
                Me.value = value
            End Sub

            Overrides Function GetValue() As Integer
                Return value
            End Function

            Public Overrides Sub SetValue(value As Integer)
                Me.value = value
            End Sub

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{value}"
            End Function
        End Class

        MustInherit Class Instruction
            Inherits MemoryLocation

            Overrides Sub Execute(r As Registers, m As Memory)
                Throw New Exception($"Attempting to execute an abstract instruction")
            End Sub

            Public Overrides Sub Execute(r As Registers)
                Throw New Exception($"Attempting to execute an abstract instruction")
            End Sub

        End Class

        MustInherit Class MemoryReferenceInstruction
            Inherits Instruction

            Protected Rd As Integer

            Protected location As Integer
            Protected locationLabel As String

            Sub New(toFromRegister As Integer, location As Integer)
                Debug.Assert(ValidRegister(toFromRegister), $"Invalid register number {toFromRegister}")
                Debug.Assert(ValidAddress(location))
                Rd = toFromRegister
                Me.location = location
            End Sub

            Sub New(toFromRegister As Integer, location As String)
                Debug.Assert(ValidRegister(toFromRegister), $"Invalid register number {toFromRegister}")
                Rd = toFromRegister
                Me.locationLabel = location
            End Sub
        End Class

        Class LoadInstructionDirect
            Inherits MemoryReferenceInstruction

            Sub New(toRegister As Integer, fromLocation As Integer)
                MyBase.New(toRegister, fromLocation)
            End Sub

            Sub New(toRegister As Integer, fromLocation As String)
                MyBase.New(toRegister, fromLocation)
            End Sub

            Sub UpdateLocation(locationLabel As String, location As Integer)
                If Me.locationLabel = locationLabel Then
                    Me.location = location
                Else
                    ' TODO: decide what to do: use an assertion, throw an exception or do nothing? 
                End If
            End Sub

            Public Overrides Sub Execute(r As Registers, m As Memory)
                Debug.Assert(TypeOf m(location) Is Data, "Attempt to load from a non-data location")
                r(Rd) = m(location).GetValue()
            End Sub
        End Class

        Class StoreInstructionDirect
            Inherits MemoryReferenceInstruction

            Sub New(fromRegister As Integer, toLocation As Integer)
                MyBase.New(fromRegister, toLocation)
            End Sub

            Sub New(fromRegister As Integer, toLocation As String)
                MyBase.New(fromRegister, toLocation)
            End Sub

            Sub UpdateLocation(locationLabel As String, location As Integer)
                If Me.locationLabel = locationLabel Then
                    Me.location = location
                Else
                    ' TODO: decide what to do: use an assertion, throw an exception or do nothing? 
                End If
            End Sub

            Public Overrides Sub Execute(r As Registers, m As Memory)
                Debug.Assert(TypeOf m(location) Is Data, "Attempt to load from a non-data location")
                m(location).SetValue(r(Rd))
            End Sub
        End Class

        MustInherit Class ArithmeticLogicInstruction
            Inherits Instruction

            Protected Rd As Integer
            Protected Rn As Integer

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer)
                Debug.Assert(ValidRegister(destinationRegister), $"Invalid destination register {destinationRegister} for arithmetic logic instruction")
                Debug.Assert(ValidRegister(firstOperandRegister), $"Invalid first operand register {firstOperandRegister} for arithmetic logic instruction")
                Rd = destinationRegister
                Rn = firstOperandRegister
            End Sub

            Public Overrides Sub Execute(r As Registers)
                Throw New Exception($"Attempting to execute an abstract instruction")
            End Sub

        End Class

        MustInherit Class ArithmeticLogicInstructionRegister
            Inherits ArithmeticLogicInstruction

            Protected Rm As Integer

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister)
                Debug.Assert(ValidRegister(secondOperandRegister), $"Invalid second operand register {secondOperandRegister}")
                Rm = secondOperandRegister
            End Sub
        End Class

        MustInherit Class ArithmeticLogicInstructionImmediate
            Inherits ArithmeticLogicInstruction

            Protected value As Integer

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister)
                value = secondOperandImmediate
            End Sub
        End Class

        Class ADDRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) + r(Rm)
            End Sub
        End Class

        Class ADDImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) + value
            End Sub

        End Class

        Class SUBRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) - r(Rm)
            End Sub
        End Class

        Class SUBImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) - value
            End Sub

        End Class

        Class ANDRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) And r(Rm)
            End Sub
        End Class

        Class ANDImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) And value
            End Sub

        End Class

        Class ORRRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) Or r(Rm)
            End Sub
        End Class

        Class ORRImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) Or value
            End Sub

        End Class

        Class EORRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) Xor r(Rm)
            End Sub
        End Class

        Class EORImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) Xor value
            End Sub

        End Class

        Class LSLRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) << (r(Rm) And (REGISTER_SIZE - 1))
            End Sub
        End Class

        Class LSLImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) << (value And (REGISTER_SIZE - 1))
            End Sub

        End Class

        Class LSRRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) >> r(Rm)
            End Sub
        End Class

        Class LSRImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) >> value
            End Sub

        End Class

        Class Memory
            Private words(HIGHEST_MEMORY_ADDRESS) As MemoryLocation

            Sub New()
                For i = 0 To HIGHEST_MEMORY_ADDRESS
                    words(i) = New MemoryLocation()
                Next
            End Sub

            ' TODO: not sure about this. Proably need 
            Default Property At(address As Integer) As MemoryLocation
                Get
                    Debug.Assert(address >= 0 And address <= HIGHEST_MEMORY_ADDRESS, $"Invalid memory address: {address}")
                    Return words(address)
                End Get
                Set(value As MemoryLocation)
                    Debug.Assert(address >= 0 And address <= HIGHEST_MEMORY_ADDRESS, $"Invalid register number: {address}")
                    words(address) = value
                End Set
            End Property
        End Class

        Class Registers
            Public Enum Flags
                EQ
                GT
                HALT
            End Enum

            Private GP_Registers(HIGHEST_REGISTER_NUMBER) As Integer
            Private _PC As Integer
            Property PC As Integer
                Get
                    Return _PC
                End Get
                Set(value As Integer)
                    Debug.Assert(ValidAddress(value), $"Attempted to set PC to invalid value {value}")
                    _PC = value
                End Set
            End Property

            Private status_Register(Flags.HALT) As Boolean

            Sub New()
                PC = 0
                status_Register(Flags.EQ) = False
                status_Register(Flags.GT) = False
                status_Register(Flags.HALT) = False
            End Sub

            Sub SetFlags(value As Integer)
                ' Set the flags by passing in the result of Rd - <operand2>
                status_Register(Flags.EQ) = value = 0
                status_Register(Flags.GT) = value > 0
            End Sub

            ReadOnly Property EQ As Boolean
                Get
                    Return status_Register(Flags.EQ)
                End Get
            End Property

            ReadOnly Property GT As Boolean
                Get
                    Return status_Register(Flags.GT)
                End Get
            End Property

            ReadOnly Property LT As Boolean
                Get
                    Return Not status_Register(Flags.GT) And Not status_Register(Flags.EQ)
                End Get
            End Property

            ReadOnly Property NE As Boolean
                Get
                    Return Not status_Register(Flags.EQ)
                End Get
            End Property

            Property Halted As Boolean
                Get
                    Return status_Register(Flags.HALT)
                End Get
                Set(value As Boolean)
                    status_Register(Flags.HALT) = value
                End Set
            End Property

            Default Property At(r As Integer) As Integer
                Get
                    Debug.Assert(r >= 0 And r <= HIGHEST_REGISTER_NUMBER, $"Invalid register number: {r}")
                    Return GP_Registers(r)
                End Get
                Set(value As Integer)
                    Debug.Assert(r >= 0 And r <= HIGHEST_REGISTER_NUMBER, $"Invalid register number: {r}")
                    GP_Registers(r) = value
                End Set
            End Property
        End Class

        Public Enum TokenType
            Keyword
            Register
            StringLiteral
            IntegerLiteral
            CharacterLiteral ' TODO: character literals not in specification
            Identifier
            Symbol
            EndOfText
        End Enum

        Public Enum KeywordType
            [IF]
            [MEMORY]
            [GOTO]
            [DATA]
            [END]
        End Enum

        Class Token
            Public type As TokenType
            Public k As KeywordType
            Public s As String
            Public i As Integer
            Public r As Integer
            Public c As Char
            Public id As String
            Public sym As String
        End Class

        Class Scanner

            Private text As String
            Private position As Integer = -1
            Private ch As Char
            Private Const EOT = Chr(0)
            Private ReadOnly keywordStrings As List(Of String) = New List(Of String)() From
                                                            {"IF",
                                                            "MEMORY",
                                                            "GOTO", "GO", "BRANCH",
                                                            "DATA",
                                                            "END", "STOP", "HALT"}

            Private ReadOnly keywords As List(Of KeywordType) = New List(Of KeywordType) From
                                                            {KeywordType.IF,
                                                            KeywordType.MEMORY,
                                                            KeywordType.GOTO, KeywordType.GOTO, KeywordType.GOTO,
                                                            KeywordType.DATA,
                                                            KeywordType.END, KeywordType.END, KeywordType.END}

            Public t As Token

            Sub New(onText As String)
                text = onText
                position = 0
                NextCharacter()
            End Sub

            Private Sub NextCharacter()
                If position < text.Length Then
                    ch = text(position)
                    position += 1
                Else
                    ch = EOT
                End If
            End Sub

            Sub NextToken()
                t = New Token()
                Do Until ch = EOT OrElse Not Char.IsWhiteSpace(ch)
                    NextCharacter()
                Loop
                If ch = EOT Then
                    t.type = TokenType.EndOfText
                ElseIf Char.IsDigit(ch) Then
                    t.i = 0
                    Do
                        t.i = t.i * 10 + Integer.Parse(ch)
                        NextCharacter()
                    Loop Until Not Char.IsDigit(ch)
                    t.type = TokenType.IntegerLiteral
                ElseIf Char.IsLetter(ch) Then
                    Dim identifier As String = ""
                    Do
                        identifier += ch
                        NextCharacter()
                    Loop Until Not Char.IsLetterOrDigit(ch)
                    If keywordStrings.Contains(identifier.ToUpper()) Then
                        t.k = keywords(keywordStrings.IndexOf(identifier.ToUpper()))
                        t.type = TokenType.Keyword
                    ElseIf identifier.ToUpper()(0) = "R" AndAlso identifier.Length > 1 AndAlso Integer.TryParse(identifier.Substring(1), t.r) Then
                        t.r = Integer.Parse(identifier.Substring(1)) ' strictly superfluous (see TryParse)
                        t.type = TokenType.Register
                    Else
                        t.id = identifier.ToUpper()
                        t.type = TokenType.Identifier
                    End If
                ElseIf Char.IsPunctuation(ch) OrElse Char.IsSymbol(ch) Then
                    Select Case ch
                        Case ",", "#", "[", "]", "(", ")"
                            t.sym = ch
                            t.type = TokenType.Symbol
                            NextCharacter()
                        Case "="
                            t.sym = ch
                            t.type = TokenType.Symbol
                            NextCharacter()
                            If ch = "=" Then ' allows == to be 'translated' to =. Means that the assignments could get away with using  ==
                                NextCharacter()
                            End If
                        Case "+", "-"
                            t.sym = ch
                            NextCharacter()
                            If ch = "=" Then ' allow +=, -=
                                t.sym += ch
                                NextCharacter()
                            End If
                            t.type = TokenType.Symbol
                        Case "!"
                            NextCharacter()
                            If ch = "=" Then
                                t.sym = "<>"
                                t.type = TokenType.Symbol
                                NextCharacter()
                            Else
                                Throw New Exception($"Unexpected NextToken symbol case for:{ch}")
                            End If
                        Case "<", ">"
                            t.sym = ch
                            NextCharacter()
                            If ch = t.sym Or ch = "=" Then ' So <<, <=, >>, >=
                                t.sym += ch
                                NextCharacter()
                            ElseIf ch = ">" Then ' must be <> because this case is for < or > and >> would be picked up on previous if
                                t.sym = "<>"
                                NextCharacter()
                            End If
                            t.type = TokenType.Symbol
                        Case Else
                            Throw New Exception($"Unexpected NextToken symbol case for:{ch}")
                    End Select
                Else
                    Throw New Exception($"Unexpected NextToken case for:{ch}")
                End If
            End Sub

        End Class

        <Extension()>
        Function ToTokens(source As String) As List(Of Token)
            Dim result As New List(Of Token)
            Dim s As New Scanner(If(source, ""))
            Do
                s.NextToken()
                result.Add(s.t)
            Loop Until s.t.type = TokenType.EndOfText
            Return result
        End Function

        Sub DisplayMenu()
            Console.WriteLine("L - (L)oad a HLA file")
            Console.WriteLine("D - (D)isplay the HLA program")
            Console.WriteLine("C - (C)ompile the current HLA program")
            Console.WriteLine("E - (E)xecute the compiled program")
            Console.WriteLine("N - create a (N)ew HLA program")
            Console.WriteLine("H - display the (H)elp file")
            Console.WriteLine("Q - (Q)uit the HLA program")
        End Sub

        Function GetMenuOption() As String
            Console.Write("Enter option: ")
            Return Console.ReadLine().ToUpper()
        End Function

        Function LoadHLAFile(existingProgram As List(Of String)) As List(Of String)
            Console.Write("HLA filename: ")
            Dim Filename = Console.ReadLine().ToUpper()
            If Not Filename.EndsWith(HLA_EXTENSION) Then
                Filename += HLA_EXTENSION
            End If
            Dim program = New List(Of String)
            If File.Exists(Filename) Then
                Using HLA = New StreamReader(Filename)
                    program.AddRange(HLA.ReadToEnd().Split(Environment.NewLine))
                End Using
            Else
                Console.WriteLine($"Couldn't find the file {Filename}")
                program = existingProgram
            End If
            Return program
        End Function

        Sub DisplayHLA(program As List(Of String))
            Dim number = 1
            For Each line In program
                Console.WriteLine($"{number,-4}  {line}")
                number += 1
            Next
        End Sub

        Function NewHLAProgram() As List(Of String)

            Console.WriteLine("Enter your program line by line. Just press Enter to finish")
            Dim lineNumber = 1
            Dim Program = New List(Of String)
            Do
                Console.Write($"Line {lineNumber}: ")
                Dim line = Console.ReadLine()
                If line <> "" Then
                    Debug.Assert(line IsNot Nothing)
                    Program = Program.Append(line).ToList()
                    lineNumber += 1
                Else
                    Exit Do
                End If
            Loop

            Return Program
        End Function

        Sub Main(args As String())
            Dim program As New List(Of String)
            Do
                DisplayMenu()
                Select Case GetMenuOption()
                    Case "L"
                        program = LoadHLAFile(program)
                    Case "D"
                        DisplayHLA(program)
                    Case "C"
                    Case "E"
                    Case "N"
                        program = NewHLAProgram()
                    Case "H"
                    Case "Q"
                        Exit Do
                    Case Else
                End Select
            Loop
        End Sub
    End Module

End Namespace