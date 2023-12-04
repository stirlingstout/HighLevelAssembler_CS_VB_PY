Imports HLA_VB.HLA_VB
Imports HLA_VB.HLA_VB.Instructions
Imports HLA_VB.HLA_VB.Scanner

Public Class Parser

    Shared ReadOnly patterns As New List(Of (pattern As IEnumerable(Of Token), generator As Func(Of List(Of Token), List(Of Instruction)))) From
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
("R0 = R1 >> 25".ToTokens(), AddressOf LSRImmediate)
}


End Class
