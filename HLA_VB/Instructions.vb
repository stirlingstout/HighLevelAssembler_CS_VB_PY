Namespace HLA_VB

    Public Module Instructions

        Function ValidRegister(r As Integer) As Boolean
            Return r >= 0 And r <= HIGHEST_REGISTER_NUMBER
        End Function

        Function ValidAddress(a As Integer) As Boolean
            Return a >= 0 And a <= HIGHEST_MEMORY_ADDRESS
        End Function

        Public Class Memory
            Private ReadOnly words(HIGHEST_MEMORY_ADDRESS) As MemoryLocation

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

            Private ReadOnly GP_Registers(HIGHEST_REGISTER_NUMBER) As Integer
            Private _PC As Integer
            Property PC As Integer
                Get
                    Return _PC
                End Get
                Set(value As Integer)
                    If ValidAddress(value) Then
                        _PC = value
                    Else
                        Throw New IndexOutOfRangeException($"Attempted to set PC to invalid value {value}")
                    End If
                End Set
            End Property

            Private ReadOnly status_Register(Flags.HALT) As Boolean

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

        Public Class MemoryLocation
            Public Const MEMORY_LABEL_WIDTH = 10

            Private ReadOnly labels As List(Of String)

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

#Region "LDR and STR"
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
#End Region

#Region "Arithmetic logic instructions"
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

#End Region

#Region "Odds and ends"
        Class HALTInstruction
            Inherits Instruction

            Public Overrides Sub Execute(r As Registers)
                r.Halted = True
            End Sub
        End Class

        Class MVNRegisterInstruction
            Inherits Instruction

            Private ReadOnly Rd As Integer
            Private ReadOnly Rm As Integer

            Sub New(Rd As Integer, Rm As Integer)
                Debug.Assert(ValidRegister(Rd), $"Invalid register in constructor for MVN instruction: {Rd}")
                Debug.Assert(ValidRegister(Rm), $"Invalid register in constructor for MVN instruction: {Rm}")
                Me.Rd = Rd
                Me.Rm = Rm
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = Not r(Rm)
            End Sub
        End Class

        Class MVNImmediateInstruction
            Inherits Instruction

            Private ReadOnly Rd As Integer
            Private ReadOnly value As Integer

            Sub New(Rd As Integer, value As Integer)
                Debug.Assert(ValidRegister(Rd), $"Invalid register in constructor for MVN instruction: {Rd}")
                Me.Rd = Rd
                Me.value = value
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r(Rd) = Not value
            End Sub
        End Class

        Class CMPRegisterInstruction
            Inherits Instruction

            Private ReadOnly Rd As Integer
            Private ReadOnly Rm As Integer

            Sub New(Rd As Integer, Rm As Integer)
                Debug.Assert(ValidRegister(Rd), $"Invalid register in constructor for CMP instruction: {Rd}")
                Debug.Assert(ValidRegister(Rm), $"Invalid register in constructor for CMP instruction: {Rm}")
                Me.Rd = Rd
                Me.Rm = Rm
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r.SetFlags(r(Rd) - r(Rm))
            End Sub
        End Class

        Class CMPImmediateInstruction
            Inherits Instruction

            Private ReadOnly Rd As Integer
            Private ReadOnly value As Integer

            Sub New(Rd As Integer, value As Integer)
                Debug.Assert(ValidRegister(Rd), $"Invalid register in constructor for CMP instruction: {Rd}")
                Me.Rd = Rd
                Me.value = value
            End Sub

            Public Overrides Sub Execute(r As Registers)
                r.SetFlags(r(Rd) - value)
            End Sub
        End Class
#End Region

#Region "Branches"
        Class BranchInstruction
            Inherits Instruction

            Protected destination As Integer
            Protected ReadOnly destinationLabel As String

            Sub New(destination As Integer)
                Me.destination = destination
            End Sub

            Sub New(label As String)
                Me.destination = -1
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
        End Class
#End Region
    End Module

End Namespace