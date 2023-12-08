Imports HLA_VB.HLA_VB
Imports HLA_VB.HLA_VB.Instructions
Imports HLA_VB.HLA_VB.Scanner

Public Class Parser

    Public Shared ReadOnly patterns As New List(Of (pattern As IEnumerable(Of Token), generator As Func(Of List(Of Token), List(Of MemoryLocation)))) From
{
("R0 = MEMORY[100]".ToTokens(), AddressOf LDRDirect),
("R0 = MEMORY[First]".ToTokens(), AddressOf LDRDirectLabel),
("MEMORY[100] = R0".ToTokens(), AddressOf STRDirect),
("MEMORY[First] = R0".ToTokens(), AddressOf STRDirectLabel),
("R0 = R1 ?o ?2".ToTokens(), AddressOf ArithmeticOperation),
("R0 = R1 ?i R2".ToTokens(), AddressOf LogicOperation),
("R0 = ?2".ToTokens(), AddressOf MOVOperation),
("R0 = NOT ?2".ToTokens(), AddressOf MVNOperation),
("IF R0 ?o ?2 GOTO Start".ToTokens(), AddressOf SimpleIFStatement),
("GOTO Label".ToTokens(), AddressOf BAlwaysLabel),
("HALT".ToTokens(), AddressOf HALT),
("FOR R1 = ?2 TO ?2".ToTokens(), AddressOf FORTOStatement),
("FOR R1 = ?2 DOWNTO ?2".ToTokens(), AddressOf FORDOWNTOStatement),
("END FOR".ToTokens(), AddressOf ENDFOR),
("REPEAT".ToTokens(), AddressOf REPEATStatement),
("UNTIL R1 ?o ?2".ToTokens(), AddressOf UNTILStatement),
("WHILE R1 ?o ?2".ToTokens(), AddressOf WHILEStatement),
("END WHILE".ToTokens(), AddressOf ENDWHILE),
("DATA 100".ToTokens(), AddressOf DATAStatement),
("DATA".ToTokens(), AddressOf DATAStatement)
}
    ' TODO: consider if we want to allow branch instructions to address without labels
    ' TODO: the LDR and STR could be condensed if we have a wildcard that can match an integer or an identifier
End Class
