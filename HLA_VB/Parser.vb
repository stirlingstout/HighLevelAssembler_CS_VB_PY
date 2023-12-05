Imports HLA_VB.HLA_VB
Imports HLA_VB.HLA_VB.Instructions
Imports HLA_VB.HLA_VB.Scanner

Public Class Parser

    ' TODO: we could reduce the number of patterns here by allowing capturing of operators and register/immediate
    Public Shared ReadOnly patterns As New List(Of (pattern As IEnumerable(Of Token), generator As Func(Of List(Of Token), List(Of MemoryLocation)))) From
{
("R0 = MEM[100]".ToTokens(), AddressOf LDRDirect),
("R0 = MEM[First]".ToTokens(), AddressOf LDRDirectLabel),
("MEM[100] = R0".ToTokens(), AddressOf STRDirect),
("MEM[First] = R0".ToTokens(), AddressOf STRDirectLabel),
("R0 = R1 + R2".ToTokens(), AddressOf ADDRegister),
("R0 = R1 + 25".ToTokens(), AddressOf ADDImmediate),
("R0 = R1 - R2".ToTokens(), AddressOf SUBRegister),
("R0 = R1 - 25".ToTokens(), AddressOf SUBImmediate),
("R0 = R1 AND R2".ToTokens(), AddressOf ANDRegister),
("R0 = R1 AND 25".ToTokens(), AddressOf ANDImmediate),
("R0 = R1 OR R2".ToTokens(), AddressOf ORRRegister),
("R0 = R1 OR 25".ToTokens(), AddressOf ORRImmediate),
("R0 = R1 EOR R2".ToTokens(), AddressOf EORRegister),
("R0 = R1 EOR 25".ToTokens(), AddressOf EORImmediate),
("R0 = R1 << R2".ToTokens(), AddressOf LSLRegister),
("R0 = R1 << 25".ToTokens(), AddressOf LSLImmediate),
("R0 = R1 >> R2".ToTokens(), AddressOf LSRRegister),
("R0 = R1 >> 25".ToTokens(), AddressOf LSRImmediate),
("R0 = R1".ToTokens(), AddressOf MOVRegister),
("R0 = 25".ToTokens(), AddressOf MOVImmediate),
("IF R0 < R1 GOTO Start".ToTokens(), AddressOf IFStatementRLTRegister),
("IF R0 < 25 GOTO Start".ToTokens(), AddressOf IFStatementRLTImmediate),
("IF R0 > R1 GOTO Start".ToTokens(), AddressOf IFStatementRGTRegister),
("IF R0 > 25 GOTO Start".ToTokens(), AddressOf IFStatementRGTImmediate),
("GOTO Label".ToTokens(), AddressOf BAlwaysLabel),
("HALT".ToTokens(), AddressOf HALT),
("FOR R1 = 1 TO 10".ToTokens(), AddressOf FORIntegerToInteger),
("FOR R1 = 100 DOWNTO 1".ToTokens(), AddressOf FORIntegerDownToInteger),
("END FOR".ToTokens(), AddressOf ENDFOR)
    }

    ' TODO: consider if we want to allow branch instructions to address without labels

End Class
