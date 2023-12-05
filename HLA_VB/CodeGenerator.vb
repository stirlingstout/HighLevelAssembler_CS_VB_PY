Imports HLA_VB.HLA_VB.Instructions
Imports HLA_VB.HLA_VB.Scanner

Module CodeGenerator

    ' These functions take a list of tokens representing the current line and return a list of
    ' memory locations/instructions. List because IF ... GOTO generates two instructions
    Function LDRDirect(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = MEM[100]
        ' 0  1  2 3 4 5
        Return New List(Of Instruction)() From {New LoadInstructionDirect(t(0).r, t(4).i)}
    End Function

    Function LDRDirectLabel(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = MEM[Start]
        ' 0  1  2 3  4  5
        Return New List(Of Instruction)() From {New LoadInstructionDirect(t(0).r, t(4).id)}
    End Function

    Function STRDirect(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' MEM[100] = Rd
        '  0 1 2 3 4 5
        Return New List(Of Instruction)() From {New StoreInstructionDirect(t(5).r, t(2).i)}
    End Function

    Function STRDirectLabel(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' MEM[Start] = Rd
        '  0 1  2  3 4 5
        Return New List(Of Instruction)() From {New StoreInstructionDirect(t(5).r, t(2).id)}
    End Function

    Function ADDRegister(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New ADDRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function


    Function ADDImmediate(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New ADDImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function SUBRegister(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New SUBRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function


    Function SUBImmediate(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New SUBImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function ANDRegister(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New ANDRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function

    Function ANDImmediate(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New ANDImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function ORRRegister(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New ORRRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function

    Function ORRImmediate(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New ORRImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function EORRegister(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New EORRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function

    Function EORImmediate(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New EORImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function LSLRegister(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New LSLRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function

    Function LSLImmediate(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New LSLImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function LSRRegister(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + Rm
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New LSRRegisterInstruction(t(0).r, t(2).r, t(4).r)}
    End Function

    Function LSRImmediate(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rn + 45
        ' 0  1 2  3 4
        Return New List(Of Instruction)() From {New LSRImmediateInstruction(t(0).r, t(2).r, t(4).i)}
    End Function

    Function MOVRegister(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = Rm
        ' 0  1 2
        Return New List(Of Instruction)() From {New MOVRegisterInstruction(t(0).r, t(2).r)}
    End Function

    Function MOVImmediate(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' Rd = 45
        ' 0  1 2
        Return New List(Of Instruction)() From {New MOVImmediateInstruction(t(0).r, t(2).i)}
    End Function

    Function IFStatementRLTRegister(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' IF Rn < Rm GOTO Start (these two functions may become 4 if we allow branches to addresses)
        ' 0  1  2 3  4    5
        Return New List(Of Instruction)() From {New CMPRegisterInstruction(t(1).r, t(3).r), New BLTInstruction(t(5).id)}
    End Function

    Function IFStatementRLTImmediate(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' IF Rn < 56 GOTO Start
        ' 0  1  2 3  4    5
        Return New List(Of Instruction)() From {New CMPImmediateInstruction(t(1).r, t(3).i), New BLTInstruction(t(5).id)}
    End Function

    Function BAlwaysLabel(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' GOTO Start
        ' 0      1
        Return New List(Of Instruction)() From {New BInstruction(t(1).id)}
    End Function
    Function IFStatementRGTRegister(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' IF Rn < Rm GOTO Start (these two functions may become 4 if we allow branches to addresses)
        ' 0  1  2 3  4    5
        Return New List(Of Instruction)() From {New CMPRegisterInstruction(t(1).r, t(3).r), New BGTInstruction(t(5).id)}
    End Function

    Function IFStatementRGTImmediate(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' IF Rn < 56 GOTO Start
        ' 0  1  2 3  4    5
        Return New List(Of Instruction)() From {New CMPImmediateInstruction(t(1).r, t(3).i), New BGTInstruction(t(5).id)}
    End Function

    Function HALT(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' HALT
        ' 0
        Return New List(Of Instruction)() From {New HALTInstruction()}
    End Function

    Function REPEAT(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' REPEAT doesn't really work, because it doesn't generate an instruction
        ' 0
        Return New List(Of Instruction)() From {New HALTInstruction()}
    End Function

    Function FORIntegerToInteger(t As IEnumerable(Of Token)) As List(Of Instruction)
        ' FOR R1 = 1 TO 10
        '  0  1  2 3 4  5
        Return New List(Of Instruction)() From {New HALTInstruction()}
    End Function

End Module
