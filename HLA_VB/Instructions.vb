Imports System.ComponentModel.Design
Imports System.Data

Namespace HLA_VB

    Public Module Instructions

        Public Class Memory
            Implements IEnumerable, IEnumerator

            Private ReadOnly words(HIGHEST_MEMORY_ADDRESS) As MemoryLocation

            Private currentLocation As Integer
            Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
                Return CType(Me, IEnumerator)
            End Function

            Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
                currentLocation += 1
                Return currentLocation <= HIGHEST_MEMORY_ADDRESS
            End Function

            Public Sub Reset() Implements IEnumerator.Reset
                currentLocation = -1
            End Sub

            Public ReadOnly Property Current As Object Implements IEnumerator.Current
                Get ' TODO: look at https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/make-class-foreach-statement
                    Return words(currentLocation)
                End Get
            End Property

            Shared Function ValidAddress(a As Integer) As Boolean
                Return a >= 0 And a <= HIGHEST_MEMORY_ADDRESS
            End Function

            Sub New()
                For i = 0 To HIGHEST_MEMORY_ADDRESS
                    words(i) = New MemoryLocation()
                Next
                currentLocation = -1
            End Sub

            Sub Clear()
                For i = 0 To HIGHEST_MEMORY_ADDRESS
                    words(i).Clear()
                Next
            End Sub

            Default Property At(address As Integer) As MemoryLocation
                Get
                    If ValidAddress(address) Then
                        Return words(address)
                    Else
                        Throw New IndexOutOfRangeException($"Invalid memory address: {address}")
                    End If
                End Get
                Set(value As MemoryLocation)
                    If ValidAddress(address) Then
                        words(address) = value
                    Else
                        Throw New IndexOutOfRangeException($"Invalid register number: {address}")
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

            Shared Function ValidRegister(r As Integer) As Boolean
                Return r >= 0 And r <= HIGHEST_REGISTER_NUMBER
            End Function

            Private ReadOnly GP_Registers(HIGHEST_REGISTER_NUMBER) As Integer
            Private _PC As Integer
            Property PC As Integer
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
        End Class

        Public Class MemoryLocation
            Public Const MEMORY_LABEL_WIDTH = 10

            Private ReadOnly labels As List(Of String) ' But contents can be altered

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
                If labels.Count = 0 Then
                    Return $"{String.Empty,-10} " ' Note space at end
                Else
                    Return String.Join(Environment.NewLine, labels.Select(Function(label) (label + ":").PadRight(10))) + " "
                End If
            End Function

            Overridable ReadOnly Property IsExecutable As Boolean
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
        End Class

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

        End Class

#Region "LDR and STR"
        MustInherit Class MemoryReferenceInstruction
            Inherits Instruction

            Protected Rd As Integer

            Protected location As Integer
            Protected locationLabel As String

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
                    Throw New DataException("Attempt to load from a non-data location")
                End If
            End Sub

            Public Overrides Function ToString() As String
                Return $"LDR R{Rd}, {location} {locationLabel}"
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
                    Throw New DataException("Attempt to load from a non-data location")
                End If
            End Sub

            Public Overrides Function Equals(obj As Object) As Boolean
                Return MyBase.Equals(obj) AndAlso TypeName(obj) = TypeName(Me)
            End Function
            ' TODO see if this can be moved up the hierarchy into instruction. Is MyBase dynamic? MyClass/MyBase?
        End Class
#End Region

#Region "Arithmetic logic instructions"
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
