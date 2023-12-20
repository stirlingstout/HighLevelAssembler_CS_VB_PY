Imports System.ComponentModel.Design
Imports System.Data
Imports System.Net.Sockets

Namespace HLA_VB

    Public Module Instructions

        Public Class Memory

            Private ReadOnly Word(HIGHEST_MEMORY_ADDRESS) As MemoryLocation

            Public Iterator Function Words() As IEnumerable(Of (Address As Integer, w As MemoryLocation))
                For i = 0 To HIGHEST_MEMORY_ADDRESS
                    Yield (i, Word(i))
                Next
            End Function

            Shared Function ValidAddress(a As Integer) As Boolean
                Return a >= 0 And a <= HIGHEST_MEMORY_ADDRESS
            End Function

            Sub New()
                For i = 0 To HIGHEST_MEMORY_ADDRESS
                    Word(i) = New MemoryLocation()
                Next
            End Sub

            Sub Clear()
                For i = 0 To HIGHEST_MEMORY_ADDRESS
                    Word(i) = New MemoryLocation()
                Next
            End Sub

            Default Property At(address As Integer) As MemoryLocation
                Get
                    If ValidAddress(address) Then
                        Return Word(address)
                    Else
                        Throw New IndexOutOfRangeException($"Invalid memory address: {address}")
                    End If
                End Get
                Set(value As MemoryLocation)
                    If ValidAddress(address) Then
                        Word(address) = value
                    Else
                        Throw New IndexOutOfRangeException($"Invalid memory address: {address}")
                    End If
                End Set
            End Property
        End Class

        Class Registers
            Public Enum Flags
                EQ
                GT
                HALT
            End Enum

            Public Const FP_REGISTER = 15
            Public Const LR_REGISTER = 14
            Public Const SP_REGISTER = 13

            Shared Function ValidRegister(r As Integer) As Boolean
                Return r >= 0 And r <= HIGHEST_REGISTER_NUMBER
            End Function

            Private ReadOnly GP_Registers(HIGHEST_REGISTER_NUMBER) As Integer
            Private _PC As Integer ' If PC is a general purpose register then RETURN/END PROCEDURE becomes a MOV
            Public Property PC As Integer
                Get
                    Return _PC
                End Get
                Set(value As Integer)
                    If Memory.ValidAddress(value) Then
                        _PC = value
                    Else
                        Throw New IndexOutOfRangeException($"Attempted to set PC to invalid value {value}")
                    End If
                End Set
            End Property

            Public Property FP As Integer
                Get
                    Return GP_Registers(FP_REGISTER)
                End Get
                Set(value As Integer)
                    If Memory.ValidAddress(value) Then
                        GP_Registers(FP_REGISTER) = value
                    Else
                        Throw New IndexOutOfRangeException($"Attempted to set FP to invalid value {value}")
                    End If
                End Set
            End Property

            Public Property LR As Integer
                Get
                    Return GP_Registers(LR_REGISTER)
                End Get
                Set(value As Integer)
                    If Memory.ValidAddress(value) Then
                        GP_Registers(LR_REGISTER) = value
                    Else
                        Throw New IndexOutOfRangeException($"Attempted to set LR to invalid value {value}")
                    End If
                End Set
            End Property

            ''' <summary>
            ''' SP is decremented before storing when doing a push, so points to the last thing pushed
            ''' </summary>
            ''' <returns></returns>
            Public Property SP As Integer
                Get
                    Return GP_Registers(SP_REGISTER)
                End Get
                Set(value As Integer)
                    If Memory.ValidAddress(value) OrElse value = HIGHEST_MEMORY_ADDRESS + 1 Then ' Exception is for initialisation
                        GP_Registers(SP_REGISTER) = value
                    Else
                        Throw New IndexOutOfRangeException($"Attempted to set SP to invalid value {value}")
                    End If
                End Set
            End Property

            Private ReadOnly status_Register(Flags.HALT) As Boolean

            Sub New()
                PC = 0
                For r = 0 To HIGHEST_REGISTER_NUMBER
                    GP_Registers(r) = 0
                Next
                SP = HIGHEST_MEMORY_ADDRESS + 1 ' Actually r(13)
                status_Register(Flags.EQ) = False
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
                    If ValidRegister(r) Then
                        Return GP_Registers(r)
                    Else
                        Throw New IndexOutOfRangeException($"Invalid register number: {r}")
                    End If
                End Get
                Set(value As Integer)
                    If ValidRegister(r) Then

                        GP_Registers(r) = value
                    Else
                        Throw New IndexOutOfRangeException($"Invalid register number {r}")
                    End If

                End Set
            End Property

            Shared Function RegisterName(r As Integer) As String
                Select Case r
                    Case LR_REGISTER
                        Return "LR"
                    Case SP_REGISTER
                        Return "SP"
                    Case FP_REGISTER
                        Return "FP"
                    Case Else
                        Return $"R{r}"
                End Select
            End Function

            Public Iterator Function Contents() As IEnumerable(Of (Name As String, contents As String))
                For i = 0 To HIGHEST_REGISTER_NUMBER
                    Yield ($"{RegisterName(i)}", $"{Me(i)}") ' TODO: in hex?
                Next
                Yield (("PC", Me.PC))
                Yield (("Status", $"EQ: {Me.EQ}, GT: {Me.GT}"))
            End Function
        End Class

        Public Class MemoryLocation ' TODO: think of a better name
            Public Const MEMORY_LABEL_WIDTH = 10

            Public ReadOnly labels As List(Of String) ' But contents can be altered

            Public source As String

            Sub New()
                labels = New List(Of String)
            End Sub

            Sub AddLabel(label As String)
                labels.Add(label)
            End Sub

            Sub AddLabels(labels As List(Of String))
                Me.labels.AddRange(labels)
            End Sub

            Sub Clear()
                labels.Clear()
            End Sub

            Function HasLabel(label As String) As Boolean
                Return labels.Contains(label)
            End Function

            Public Overrides Function ToString() As String
                Return LabelDisplay()
            End Function

            Function LabelDisplay() As String
                If labels.Count = 0 Then
                    Return $"{String.Empty,-10} " ' Note space at end
                Else
                    Dim soFar = $"{(labels(0) + ":"),-10} "
                    For l = 1 To labels.Count - 1
                        soFar += Environment.NewLine + New String(" ", 5) + (labels(l) + ":").PadRight(MEMORY_LABEL_WIDTH) + " "
                    Next
                    Return soFar
                    ' TODO: the 5 spaces in the If function are the width of the address plus a space (see DisplayAssembly). Inelegant!
                End If
            End Function

            Overridable ReadOnly Property IsExecutable As Boolean
                Get
                    Return False
                End Get
            End Property

            Overridable ReadOnly Property IsPseudoOperation As Boolean
                Get
                    Return False
                End Get
            End Property

            Overridable Function GetValue() As Integer
                Throw New Exception($"Attempt to get value of an instruction")
            End Function

            Overridable Sub SetValue(value As Integer)
                Throw New Exception($"Attempt to overwrite an instruction")
            End Sub

            Overridable Sub Execute(r As Registers, m As Memory)
                Throw New Exception($"Attempt to execute memory location as load/store instruction")
            End Sub

            Overridable Sub Execute(r As Registers)
                Throw New Exception($"Attempt to execute memory location as arithmetic/logic instruction")
            End Sub

            Overridable ReadOnly Property UsesMemory As Boolean
                Get
                    Return True
                End Get
            End Property

            Public Overridable ReadOnly Property HasContents As Boolean
                Get
                    Return False
                End Get
            End Property
        End Class

        Class Data
            Inherits Instruction ' Strictly it's more of a MemoryLocation

            Private value As Integer

            Sub New()
                value = 0
            End Sub

            Sub New(value As Integer)
                Me.value = value
            End Sub

            Overloads Sub Clear()
                MyBase.Clear()
                value = 0
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

            Overrides Sub Execute(r As Registers, m As Memory)
                Throw New DataException($"Attempt to execute data as load/store instruction")
            End Sub

            Overrides Sub Execute(r As Registers)
                Throw New DataException($"Attempt to execute data as arithmetic/logic instruction")
            End Sub

            Public Overrides ReadOnly Property HasContents As Boolean
                Get
                    Return True
                End Get
            End Property
        End Class

#Region "Pseudo-operations (don't generate any code/data)"

        Class PseudoOperation
            Inherits Instruction

            Public Overrides ReadOnly Property IsPseudoOperation As Boolean
                Get
                    Return True
                End Get
            End Property

            Public Overrides ReadOnly Property UsesMemory As Boolean
                Get
                    Return False
                End Get
            End Property
        End Class

        ''' <summary>
        ''' Supplies the start address from which the program executes. The name of this
        ''' class, as well as that of Label and Location, is used in CompileHLA so may need checking
        ''' if you rename it!
        ''' </summary>
        Class StartExecution
            Inherits PseudoOperation

            Public Property Location As Integer
            Public Property LocationLabel As String

            Sub New(location As Integer)
                Me.Location = location
            End Sub

            Sub New(locationLabel As String)
                Me.Location = -1
                Me.LocationLabel = locationLabel
            End Sub
        End Class

        ''' <summary>
        ''' Instances of this class are just used to carry a generated label back from code generation, e.g., FOR0
        ''' </summary>
        Class Label
            Inherits PseudoOperation

            Public Property LabelText As String

            Sub New()
                LabelText = ""
            End Sub

            Sub New(label As String)
                Me.LabelText = label
            End Sub

            Overloads Sub Clear()
                MyBase.Clear()
                LabelText = ""
            End Sub

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{LabelText}:"
            End Function

            Overrides Sub Execute(r As Registers, m As Memory)
                Throw New DataException($"Attempt to execute label holder as load/store instruction")
            End Sub

            Overrides Sub Execute(r As Registers)
                Throw New DataException($"Attempt to execute label holder as arithmetic/logic instruction")
            End Sub
        End Class

        ''' <summary>
        ''' Supplies the address (numerical only) at which the next instruction/data is to be deposited
        ''' </summary>
        Class Location
            Inherits PseudoOperation

            Public Property Location As Integer

            Sub New(location As Integer)
                Me.Location = location
            End Sub
        End Class

        ''' <summary>
        ''' Allows the programmer to define a name for a constant, register, or symbol
        ''' </summary>
        Class [Alias]
            Inherits PseudoOperation

            Public Property Name As String
            Public Property Value As Token

            Sub New(name As String, value As Token)
                Me.Name = name
                Me.Value = value
            End Sub
        End Class

#End Region

        MustInherit Class Instruction
            Inherits MemoryLocation

            Overrides Sub Execute(r As Registers, m As Memory)
                Throw New Exception($"Attempting to execute an abstract instruction")
            End Sub

            Public Overrides Sub Execute(r As Registers)
                Throw New Exception($"Attempting to execute an abstract instruction")
            End Sub

            Public Overrides ReadOnly Property IsExecutable As Boolean
                Get
                    Return True
                End Get
            End Property

            Overloads Sub Clear()
                MyBase.Clear()
            End Sub

            Public Overrides ReadOnly Property HasContents As Boolean
                Get
                    Return True
                End Get
            End Property

            Overridable ReadOnly Property Opcode As String
                Get
                    Return ""
                End Get
            End Property

        End Class

#Region "LDR and STR"
        MustInherit Class MemoryReferenceInstruction
            Inherits Instruction

            Public Rd As Integer

            Public location As Integer
            Public locationLabel As String

            Sub New(toFromRegister As Integer, location As Integer)
                If Registers.ValidRegister(toFromRegister) Then
                    If Memory.ValidAddress(location) Then
                        Rd = toFromRegister
                        Me.location = location
                    Else
                        Throw New IndexOutOfRangeException($"Invalid address {location}")
                    End If
                Else
                    Throw New IndexOutOfRangeException($"Invalid register number {toFromRegister}")
                End If
            End Sub

            Sub New(toFromRegister As Integer, location As String)
                If Registers.ValidRegister(toFromRegister) Then
                    Rd = toFromRegister
                    Me.locationLabel = location
                    Me.location = BranchInstruction.InvalidAddress
                Else
                    Throw New IndexOutOfRangeException($"Invalid register number {toFromRegister}")
                End If
            End Sub

            Public Overrides Function Equals(obj As Object) As Boolean
                Return Rd = CType(obj, MemoryReferenceInstruction).Rd AndAlso
                location = CType(obj, MemoryReferenceInstruction).location AndAlso
                locationLabel = CType(obj, MemoryReferenceInstruction).locationLabel ' TODO: any way these can be different?
            End Function

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
                If TypeOf m(location) Is Data Then
                    r(Rd) = m(location).GetValue()
                Else
                    Throw New DataException($"Attempt to load from a non-data location {location} ({locationLabel})")
                End If
            End Sub

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}LDR  {Registers.RegisterName(Rd)}, {location} {locationLabel}"
            End Function

            Public Overrides Function Equals(obj As Object) As Boolean
                Return MyBase.Equals(obj) AndAlso TypeName(obj) = TypeName(Me)
            End Function
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
                If TypeOf m(location) Is Data Then
                    m(location).SetValue(r(Rd))
                Else
                    Throw New DataException($"Attempt to store into a non-data location {location} ({locationLabel})")
                End If
            End Sub

            Public Overrides Function Equals(obj As Object) As Boolean
                Return MyBase.Equals(obj) AndAlso TypeName(obj) = TypeName(Me)
            End Function
            ' TODO see if this can be moved up the hierarchy into instruction. Is MyBase dynamic? MyClass/MyBase?

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}STR  {Registers.RegisterName(Rd)}, {location} {locationLabel}"
            End Function
        End Class

        MustInherit Class MemoryReferenceInstructionIndirect
            Inherits Instruction

            Public Rd As Integer
            Public Rn As Integer
            Public offset As Integer

            Sub New(toFromRegister As Integer, indexRegister As Integer, offset As Integer)
                If Registers.ValidRegister(toFromRegister) Then
                    If Memory.ValidAddress(indexRegister) Then
                        Rd = toFromRegister
                        Rn = indexRegister
                        Me.offset = offset
                    Else
                        Throw New IndexOutOfRangeException($"Invalid index register {indexRegister}")
                    End If
                Else
                    Throw New IndexOutOfRangeException($"Invalid register number {toFromRegister}")
                End If
            End Sub

            Public Overrides Function Equals(obj As Object) As Boolean
                Return Rd = CType(obj, MemoryReferenceInstructionIndirect).Rd AndAlso
                Rn = CType(obj, MemoryReferenceInstructionIndirect).Rn AndAlso
                offset = CType(obj, MemoryReferenceInstructionIndirect).offset
            End Function

            ''' <summary>
            ''' Returns the printable version of the index expression, e.g., R1, R1 - 6, R2 + 4. The [] are not included
            ''' </summary>
            ''' <returns></returns>
            Protected Function IndexExpression() As String
                Dim offsetString As String
                If offset > 0 Then
                    offsetString = $" + {Math.Abs(offset)}"
                ElseIf offset < 0 Then
                    offsetString = $" - {Math.Abs(offset)}"
                Else ' Must be 0
                    offsetString = ""
                End If
                Return $"{Registers.RegisterName(Rn)}{offsetString}"
            End Function

        End Class

        Class LoadInstructionIndirect
            Inherits MemoryReferenceInstructionIndirect

            Sub New(toRegister As Integer, indexRegister As Integer)
                MyBase.New(toRegister, indexRegister, 0)
            End Sub

            Sub New(toRegister As Integer, indexRegister As Integer, offset As Integer)
                MyBase.New(toRegister, indexRegister, offset)
            End Sub

            Public Overrides Sub Execute(r As Registers, m As Memory)
                If Memory.ValidAddress(r(Rn) + offset) AndAlso TypeOf m(r(Rn) + offset) Is Data Then
                    r(Rd) = m(r(Rn) + offset).GetValue()
                Else
                    Throw New DataException($"Attempt to load indirect from an invalid or non-data location R{Rn} = {r(Rn) + offset}")
                End If
            End Sub

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}LDR  {Registers.RegisterName(Rd)}, [{Me.IndexExpression()}]"
            End Function

            Public Overrides Function Equals(obj As Object) As Boolean
                Return MyBase.Equals(obj) AndAlso TypeName(obj) = TypeName(Me)
            End Function
        End Class

        Class StoreInstructionIndirect
            Inherits MemoryReferenceInstructionIndirect

            Sub New(fromRegister As Integer, indexRegister As Integer)
                MyBase.New(fromRegister, indexRegister, 0)
            End Sub

            Sub New(fromRegister As Integer, indexRegister As Integer, offset As Integer)
                MyBase.New(fromRegister, indexRegister, offset)
            End Sub

            Public Overrides Sub Execute(r As Registers, m As Memory)
                If Memory.ValidAddress(r(Rn) + offset) Then
                    m(r(Rn) + offset) = New Data(r(Rd))
                Else
                    Throw New DataException($"Attempt to store indirect into an invalid location R{Rn} = {r(Rn) + offset}")
                End If
            End Sub

            Public Overrides Function Equals(obj As Object) As Boolean
                Return MyBase.Equals(obj) AndAlso TypeName(obj) = TypeName(Me)
            End Function
            ' TODO see if this can be moved up the hierarchy into instruction. Is MyBase dynamic? MyClass/MyBase?

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}STR  {Registers.RegisterName(Rd)}, [{Me.IndexExpression()}]"
            End Function
        End Class
#End Region

#Region "Arithmetic logic instructions"
        ' TODO: Investigate whether by creating an immediate and register superclass of instruction types there don't need to be as many ToString implementations
        MustInherit Class ArithmeticLogicInstruction
            Inherits Instruction

            Protected Rd As Integer
            Protected Rn As Integer

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer)
                If Registers.ValidRegister(destinationRegister) Then
                    If Registers.ValidRegister(firstOperandRegister) Then
                        Rd = destinationRegister
                        Rn = firstOperandRegister
                    Else
                        Throw New IndexOutOfRangeException($"Invalid first operand register {firstOperandRegister} for arithmetic logic instruction")
                    End If
                Else
                    Throw New IndexOutOfRangeException($"Invalid destination register {destinationRegister} for arithmetic logic instruction")
                End If

            End Sub

            Public Overrides Sub Execute(r As Registers)
                Throw New Exception($"Attempting to execute an abstract instruction")
            End Sub

            Public Overrides Sub Execute(r As Registers, m As Memory)
                Execute(r)  ' Arithmetic Logic instructions don't access memory
            End Sub

            Public Overrides Function Equals(obj As Object) As Boolean
                With CType(obj, ArithmeticLogicInstruction)
                    Return Rd = .Rd And Rn = .Rn
                End With
            End Function
        End Class

        MustInherit Class ArithmeticLogicInstructionRegister
            Inherits ArithmeticLogicInstruction

            Protected Rm As Integer

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister)
                If Registers.ValidRegister(secondOperandRegister) Then
                    Rm = secondOperandRegister
                Else
                    Throw New IndexOutOfRangeException($"Invalid second operand register {secondOperandRegister}")
                End If
            End Sub

            Public Overrides Function Equals(obj As Object) As Boolean
                Return MyBase.Equals(obj) AndAlso
                    Rm = CType(obj, ArithmeticLogicInstructionRegister).Rm
            End Function

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{Me.Opcode,-4} {Registers.RegisterName(Rd)}, {Registers.RegisterName(Rn)}, {Registers.RegisterName(Rm)}"
            End Function

        End Class

        MustInherit Class ArithmeticLogicInstructionImmediate
            Inherits ArithmeticLogicInstruction

            Protected value As Integer

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister)
                value = secondOperandImmediate
            End Sub

            Public Overrides Function Equals(obj As Object) As Boolean
                Return MyBase.Equals(obj) AndAlso
                    value = CType(obj, ArithmeticLogicInstructionImmediate).value
            End Function

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{Me.Opcode,-4} {Registers.RegisterName(Rd)}, {Registers.RegisterName(Rn)}, #{value}"
            End Function

        End Class

        Class ADDRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) + r(Rm)
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "ADD"
                End Get
            End Property
        End Class

        Class ADDImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) + value
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "ADD"
                End Get
            End Property
        End Class

        Class SUBRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) - r(Rm)
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "SUB"
                End Get
            End Property
        End Class

        Class SUBImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) - value
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "SUB"
                End Get
            End Property
        End Class

        Class ANDRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) And r(Rm)
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "AND"
                End Get
            End Property
        End Class

        Class ANDImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) And value
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "AND"
                End Get
            End Property
        End Class

        Class ORRRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) Or r(Rm)
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "ORR"
                End Get
            End Property
        End Class

        Class ORRImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) Or value
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "ORR"
                End Get
            End Property

        End Class

        Class EORRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) Xor r(Rm)
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "EOR"
                End Get
            End Property
        End Class

        Class EORImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) Xor value
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "EOR"
                End Get
            End Property
        End Class

        Class LSLRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) << (r(Rm) And (REGISTER_SIZE - 1))
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "LSL"
                End Get
            End Property
        End Class

        Class LSLImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) << (value And (REGISTER_SIZE - 1))
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "LSL"
                End Get
            End Property
        End Class

        Class LSRRegisterInstruction
            Inherits ArithmeticLogicInstructionRegister

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandRegister As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandRegister)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) >> r(Rm)
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "LSR"
                End Get
            End Property
        End Class

        Class LSRImmediateInstruction
            Inherits ArithmeticLogicInstructionImmediate

            Sub New(destinationRegister As Integer, firstOperandRegister As Integer, secondOperandImmediate As Integer)
                MyBase.New(destinationRegister, firstOperandRegister, secondOperandImmediate)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rn) >> value
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "LSR"
                End Get
            End Property
        End Class

#End Region

#Region "Odds and ends"

        Class OddInstruction
            Inherits Instruction

            Public Overrides Sub Execute(r As Registers, m As Memory)
                Execute(r)
            End Sub

        End Class

        Class HALTInstruction
            Inherits OddInstruction

            Public Overrides Sub Execute(r As Registers)
                r.Halted = True
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "HALT"
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{Me.Opcode,-4}"
            End Function
        End Class

        Class MOVRegisterInstruction
            Inherits OddInstruction

            Private ReadOnly Rd As Integer
            Private ReadOnly Rm As Integer

            Sub New(Rd As Integer, Rm As Integer)
                If Registers.ValidRegister(Rd) Then
                    If Registers.ValidRegister(Rm) Then
                        Me.Rd = Rd
                        Me.Rm = Rm
                    Else
                        Throw New IndexOutOfRangeException($"Invalid register in constructor for MOV instruction {Rm}")
                    End If
                Else
                    Throw New IndexOutOfRangeException($"Invalid register in constructor for MOV instruction {Rd}")
                End If
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = r(Rm)
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "MOV"
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{Me.Opcode,-4} {Registers.RegisterName(Rd)}, {Registers.RegisterName(Rm)}"
            End Function
        End Class

        Class MOVImmediateInstruction
            Inherits OddInstruction

            Private ReadOnly Rd As Integer
            Private ReadOnly value As Integer

            Sub New(Rd As Integer, value As Integer)
                If Registers.ValidRegister(Rd) Then
                    Me.Rd = Rd
                    Me.value = value
                Else
                    Throw New IndexOutOfRangeException($"Invalid register in constructor for MOV instruction {Rd}")
                End If
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = value
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "MOV"
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{Me.Opcode,-4} {Registers.RegisterName(Rd)}, #{value}"
            End Function
        End Class

        Class MVNRegisterInstruction
            Inherits OddInstruction

            Private ReadOnly Rd As Integer
            Private ReadOnly Rm As Integer

            Sub New(Rd As Integer, Rm As Integer)
                If Registers.ValidRegister(Rd) Then
                    If Registers.ValidRegister(Rm) Then
                        Me.Rd = Rd
                        Me.Rm = Rm
                    Else
                        Throw New IndexOutOfRangeException($"Invalid register in constructor for MVN instruction {Rm}")
                    End If
                Else
                    Throw New IndexOutOfRangeException($"Invalid register in constructor for MVN instruction {Rd}")
                End If
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = Not r(Rm)
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "MVN"
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{Me.Opcode,-4} {Registers.RegisterName(Rd)}, {Registers.RegisterName(Rm)}"
            End Function

        End Class

        Class MVNImmediateInstruction
            Inherits OddInstruction

            Private ReadOnly Rd As Integer
            Private ReadOnly value As Integer

            Sub New(Rd As Integer, value As Integer)
                If Registers.ValidRegister(Rd) Then
                    Me.Rd = Rd
                    Me.value = value
                Else
                    Throw New IndexOutOfRangeException($"Invalid register in constructor for MVN instruction {Rd}")
                End If
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = Not value
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "MVN"
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{Me.Opcode,-4} {Registers.RegisterName(Rd)}, #{value}"
            End Function
        End Class

        Class CMPRegisterInstruction
            Inherits OddInstruction

            Private ReadOnly Rd As Integer
            Private ReadOnly Rm As Integer

            Sub New(Rd As Integer, Rm As Integer)
                If Registers.ValidRegister(Rd) Then
                    If Registers.ValidRegister(Rm) Then
                        Me.Rd = Rd
                        Me.Rm = Rm
                    Else
                        Throw New IndexOutOfRangeException($"Invalid register in constructor for CMP instruction {Rm}")
                    End If
                Else
                    Throw New IndexOutOfRangeException($"Invalid register in constructor for CMP instruction {Rd}")
                End If
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r.SetFlags(r(Rd) - r(Rm))
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "CMP"
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{Me.Opcode,-4} {Registers.RegisterName(Rd)}, {Registers.RegisterName(Rm)}"
            End Function
        End Class

        Class CMPImmediateInstruction
            Inherits OddInstruction

            Private ReadOnly Rd As Integer
            Private ReadOnly value As Integer

            Sub New(Rd As Integer, value As Integer)
                If Registers.ValidRegister(Rd) Then
                    Me.Rd = Rd
                    Me.value = value
                Else
                    Throw New IndexOutOfRangeException($"Invalid register in constructor for CMP instruction {Rd}")
                End If
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r.SetFlags(r(Rd) - value)
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "CMP"
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{Me.Opcode,-4} {Registers.RegisterName(Rd)}, #{value}"
            End Function
        End Class
#End Region

#Region "Branches"
        Class BranchInstruction
            Inherits Instruction

            Friend Const InvalidAddress = -1

            Public destination As Integer
            Public ReadOnly destinationLabel As String

            Sub New(destination As Integer)
                Me.destination = destination
            End Sub

            Sub New(label As String)
                Me.destination = InvalidAddress
                Me.destinationLabel = label
            End Sub

            Function DestinationIs(label As String) As Boolean
                Return destinationLabel.ToUpper() = label.ToUpper()
                ' Note that all labels should be uppercase anyway
            End Function

            Sub PatchAddress(label As String, destination As Integer)
                If DestinationIs(label) Then
                    Me.destination = destination
                Else
                    ' TODO: decide how to handle this
                End If
            End Sub

            Public Overrides Sub Execute(r As Registers, m As Memory)
                Execute(r)
            End Sub

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{Me.Opcode,-4} {destination} {destinationLabel}"
            End Function
        End Class

        Class BInstruction
            Inherits BranchInstruction

            Sub New(destination As Integer)
                MyBase.New(destination)
            End Sub

            Sub New(label As String)
                MyBase.New(label)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r.PC = destination ' Valid address throws an exception if this is invalid, i.e., -1
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "B"
                End Get
            End Property
        End Class

        Class BLInstruction
            Inherits BranchInstruction

            Sub New(destination As Integer)
                MyBase.New(destination)
            End Sub

            Sub New(label As String)
                MyBase.New(label)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r.LR = r.PC ' PC has already been incremented when an instruction is executed
                r.PC = destination ' Valid address throws an exception if this is invalid, i.e., -1
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "BL"
                End Get
            End Property
        End Class

        Class RETURNInstruction
            Inherits OddInstruction

            Sub New()
                MyBase.New()
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r.PC = r.LR ' Valid address throws an exception if this is invalid, i.e., -1
            End Sub

            Public Overrides ReadOnly Property opcode As String
                Get
                    Return "RET"
                End Get
            End Property
        End Class

        Class BEQInstruction
            Inherits BranchInstruction

            Sub New(destination As Integer)
                MyBase.New(destination)
            End Sub

            Sub New(label As String)
                MyBase.New(label)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                If r.EQ Then
                    r.PC = destination ' Valid address throws an exception if this is invalid, i.e., -1
                End If
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "BEQ"
                End Get
            End Property
        End Class

        Class BNEInstruction
            Inherits BranchInstruction

            Sub New(destination As Integer)
                MyBase.New(destination)
            End Sub

            Sub New(label As String)
                MyBase.New(label)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                If r.NE Then
                    r.PC = destination ' Valid address throws an exception if this is invalid, i.e., -1
                End If
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "BNE"
                End Get
            End Property
        End Class

        Class BGTInstruction
            Inherits BranchInstruction

            Sub New(destination As Integer)
                MyBase.New(destination)
            End Sub

            Sub New(label As String)
                MyBase.New(label)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                If r.GT Then
                    r.PC = destination ' Valid address throws an exception if this is invalid, i.e., -1
                End If
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "BGT"
                End Get
            End Property
        End Class

        Class BLTInstruction
            Inherits BranchInstruction

            Sub New(destination As Integer)
                MyBase.New(destination)
            End Sub

            Sub New(label As String)
                MyBase.New(label)
            End Sub

            Public Overrides Sub Execute(r As Registers)
                If r.LT Then
                    r.PC = destination ' Valid address throws an exception if this is invalid, i.e., -1
                End If
            End Sub

            Public Overrides ReadOnly Property Opcode As String
                Get
                    Return "BLT"
                End Get
            End Property
        End Class
#End Region
    End Module

End Namespace
