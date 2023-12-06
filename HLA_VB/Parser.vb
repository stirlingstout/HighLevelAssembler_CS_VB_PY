Imports HLA_VB.HLA_VB
Imports HLA_VB.HLA_VB.Instructions
Imports HLA_VB.HLA_VB.Scanner

Public Class Parser

    ' TODO: we could reduce the number of patterns here by allowing capturing of operators and register/immediate
    Public Shared ReadOnly patterns As New List(Of (pattern As IEnumerable(Of Token), generator As Func(Of List(Of Token), List(Of MemoryLocation)))) From
{
("R0 = MEMORY[100]".ToTokens(), AddressOf LDRDirect),
("R0 = MEMORY[First]".ToTokens(), AddressOf LDRDirectLabel),
("MEMORY[100] = R0".ToTokens(), AddressOf STRDirect),
("MEMORY[First] = R0".ToTokens(), AddressOf STRDirectLabel),
("R0 = R1 ?o ?2".ToTokens(), AddressOf ArithmeticOperation),
("R0 = R1 ?i R2".ToTokens(), AddressOf LogicOperation),
("R0 = ?2".ToTokens(), AddressOf MOVOperation),
("IF R0 ?o ?2 GOTO Start".ToTokens(), AddressOf IFStatement),
("GOTO Label".ToTokens(), AddressOf BAlwaysLabel),
("HALT".ToTokens(), AddressOf HALT),
("FOR R1 = ?2 TO ?2".ToTokens(), AddressOf FORTo),
("FOR R1 = ?2 DOWNTO ?2".ToTokens(), AddressOf FORDownto),
("END FOR".ToTokens(), AddressOf ENDFOR),
("REPEAT".ToTokens(), AddressOf REPEAT_Statement),
("UNTIL R1 ?o ?2".ToTokens(), AddressOf UNTIL_Statement),
("WHILE R1 <> R2".ToTokens(), AddressOf WHILE_RNER),
("WHILE R1 <> 100".ToTokens(), AddressOf WHILE_RNEI),
("END WHILE".ToTokens(), AddressOf ENDWHILE)
    }

    ' TODO: consider if we want to allow branch instructions to address without labels

End Class
