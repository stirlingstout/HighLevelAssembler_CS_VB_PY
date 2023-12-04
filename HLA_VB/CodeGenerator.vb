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

End Module
