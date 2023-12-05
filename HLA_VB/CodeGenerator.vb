Imports System.Runtime.CompilerServices
Imports System.Security.Cryptography
Imports HLA_VB.HLA_VB.Instructions
Imports HLA_VB.HLA_VB.Scanner

Module CodeGenerator

    ' These functions take a list of tokens representing the current line and return a list of
    ' memory locations/instructions. List because IF ... GOTO generates two instructions
    Function LDRDirect(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = MEM[100]
        ' 0  1  2 3 4 5
        Return New List(Of MemoryLocation)() From {New LoadInstructionDirect(t(0).r, t(4).i)}
    End Function

    Function LDRDirectLabel(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = MEM[Start]
        ' 0  1  2 3  4  5
        Return New List(Of MemoryLocation)() From {New LoadInstructionDirect(t(0).r, t(4).id)}
    End Function

    Function STRDirect(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' MEM[100] = Rd
        '  0 1 2 3 4 5
        Return New List(Of MemoryLocation)() From {New StoreInstructionDirect(t(5).r, t(2).i)}
    End Function

    Function STRDirectLabel(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' MEM[Start] = Rd
        '  0 1  2  3 4 5
        Return New List(Of MemoryLocation)() From {New StoreInstructionDirect(t(5).r, t(2).id)}
    End Function

    Function ADDRegister(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New ADDRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function


    Function ADDImmediate(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New ADDImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function SUBRegister(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New SUBRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function

    Function SUBImmediate(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New SUBImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function ANDRegister(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New ANDRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function

    Function ANDImmediate(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New ANDImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function ORRRegister(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New ORRRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function

    Function ORRImmediate(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New ORRImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function EORRegister(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New EORRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function

    Function EORImmediate(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New EORImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function LSLRegister(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New LSLRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function

    Function LSLImmediate(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New LSLImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function LSRRegister(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New LSRRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function

    Function LSRImmediate(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of MemoryLocation)() From {New LSRImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function MOVRegister(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rm
        ' 0  1 2
        Return New List(Of MemoryLocation)() From {New MOVRegisterInstruction(t(0).r, t(2).r)}
    End Function

    Function MOVImmediate(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = 45
        ' 0  1 2
        Return New List(Of MemoryLocation)() From {New MOVImmediateInstruction(t(0).r, t(2).i)}
    End Function

    Function IFStatementRLTRegister(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' IF Rn < Rm GOTO Start (these two functions may become 4 if we allow branches to addresses)
        ' 0  1  2 3  4    5
        Return New List(Of MemoryLocation)() From {New CMPRegisterInstruction(t(1).r, t(3).r), New BLTInstruction(t(5).id)}
    End Function

    Function IFStatementRLTImmediate(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' IF Rn < 56 GOTO Start
        ' 0  1  2 3  4    5
        Return New List(Of MemoryLocation)() From {New CMPImmediateInstruction(t(1).r, t(3).i), New BLTInstruction(t(5).id)}
    End Function

    Function BAlwaysLabel(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' GOTO Start
        ' 0      1
        Return New List(Of MemoryLocation)() From {New BInstruction(t(1).id)}
    End Function

    Function IFStatementRGTRegister(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' IF Rn < Rm GOTO Start (these two functions may become 4 if we allow branches to addresses)
        ' 0  1  2 3  4    5
        Return New List(Of MemoryLocation)() From {New CMPRegisterInstruction(t(1).r, t(3).r), New BGTInstruction(t(5).id)}
    End Function

    Function IFStatementRGTImmediate(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' IF Rn < 56 GOTO Start
        ' 0  1  2 3  4    5
        Return New List(Of MemoryLocation)() From {New CMPImmediateInstruction(t(1).r, t(3).i), New BGTInstruction(t(5).id)}
    End Function

#Disable Warning IDE0060 ' Remove unused parameter
    Function HALT(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
#Enable Warning IDE0060 ' Remove unused parameter
        ' HALT
        ' 0
        Return New List(Of MemoryLocation)() From {New HALTInstruction()}
    End Function

#Region "Control structures"

    Private REPEATCount = -1
    Private IFELSECount = -1
    Private FORCount = -1
    Private WHILECount = -1
    Private ReadOnly FORLoops As New Stack(Of (Rd As Integer, Incrementing As Boolean))
    ' FOR loops are unusual in that we need to know about the register used and the 'direction' of the
    ' loop when we compile the END FOR

    Sub ResetStructureCounts()
        REPEATCount = -1
        IFELSECount = -1
        FORCount = -1
        WHILECount = -1
        FORLoops.Clear()
    End Sub

#Disable Warning IDE0060 ' Remove unused parameter
    Function REPEAT(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
#Enable Warning IDE0060 ' Remove unused parameter
        ' REPEAT
        ' 0
        REPEATCount += 1
        Return New List(Of MemoryLocation)() From {New LabelHolder($"REPEAT{REPEATCount}")}
    End Function

    Function UNTIL_REQR(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' UNTIL R1 = R2
        ' 0     1  2 3
        REPEATCount -= 1
        Return New List(Of MemoryLocation)() From {New CMPRegisterInstruction(t(1).r, t(3).r),
                                                   New BNEInstruction($"REPEAT{REPEATCount + 1}")}
    End Function


    Function UNTIL_RLTR(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' UNTIL R1 < R2
        ' 0     1  2 3
        REPEATCount -= 1
        Return New List(Of MemoryLocation)() From {New CMPRegisterInstruction(t(1).r, t(3).r),
                                                   New BGTInstruction($"REPEAT{REPEATCount + 1}"),
                                                   New BEQInstruction($"REPEAT{REPEATCount + 1}")}
    End Function

    Function UNTIL_RGTR(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' UNTIL R1 > R2
        ' 0     1  2 3
        REPEATCount -= 1
        Return New List(Of MemoryLocation)() From {New CMPRegisterInstruction(t(1).r, t(3).r),
                                                   New BLTInstruction($"REPEAT{REPEATCount + 1}"),
                                                   New BEQInstruction($"REPEAT{REPEATCount + 1}")}
    End Function

    Function UNTIL_RNER(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' UNTIL R1 <> R2
        ' 0     1  2 3
        REPEATCount -= 1
        Return New List(Of MemoryLocation)() From {New CMPRegisterInstruction(t(1).r, t(3).r),
                                                   New BEQInstruction($"REPEAT{REPEATCount + 1}")}
    End Function

    Function FORIntegerToInteger(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' FOR R1 = 1 TO 10
        '  0  1  2 3 4  5
        FORCount += 1
        FORLoops.Push((t(1).r, True))
        Return New List(Of MemoryLocation)() From {New MOVImmediateInstruction(t(1).r, t(3).i),
                                                   New LabelHolder($"FOR{FORCount}"),
                                                   New CMPImmediateInstruction(t(1).r, t(5).i),
                                                   New BGTInstruction($"ENDFOR{FORCount}")}
    End Function

    Function FORIntegerDownToInteger(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' FOR R1 = 100 DOWNTO 1
        '  0  1  2 3     4   5
        FORCount += 1
        FORLoops.Push((t(1).r, False))
        Return New List(Of MemoryLocation)() From {New MOVImmediateInstruction(t(1).r, t(3).i),
                                                   New LabelHolder($"FOR{FORCount}"),
                                                   New CMPImmediateInstruction(t(1).r, t(5).i),
                                                   New BLTInstruction($"ENDFOR{FORCount}")}
    End Function

    Function ENDFOR(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' ENDFOR or END FOR
        '  0
        FORCount -= 1
        Dim ChangeIndex As ArithmeticLogicInstruction
        With FORLoops.Pop()
            If .Incrementing Then
                ChangeIndex = New ADDImmediateInstruction(.Rd, .Rd, 1)
            Else
                ChangeIndex = New SUBImmediateInstruction(.Rd, .Rd, 1)
            End If
        End With
        Return New List(Of MemoryLocation)() From {ChangeIndex,
                                                   New BInstruction($"FOR{FORCount + 1}"),
                                                   New LabelHolder($"ENDFOR{FORCount + 1}")}
    End Function

#End Region
End Module
