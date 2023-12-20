﻿Imports System.ComponentModel.Design
Imports HLA_VB.HLA_VB
Imports HLA_VB.HLA_VB.Instructions
Imports HLA_VB.HLA_VB.Scanner

Public Class Parser

    Public Shared ReadOnly patterns As New List(Of (pattern As IEnumerable(Of Token), generator As Func(Of List(Of Token), List(Of Instruction)))) From
{
("R0 = MEMORY[?a]".ToTokens(), AddressOf LDRDirect),
("R0 = MEMORY[R1]".ToTokens(), AddressOf LDRIndirect),
("R0 = MEMORY[R1 ?o 100]".ToTokens(), AddressOf LDRIndirectOffset),
("MEMORY[?a] = R0".ToTokens(), AddressOf STRDirect),
("MEMORY[R1] = R0".ToTokens(), AddressOf STRIndirect),
("MEMORY[R1 ?o 100] = R0".ToTokens(), AddressOf STRIndirectOffset),
("R0 = R1 ?o ?2".ToTokens(), AddressOf ArithmeticOperation),
("R0 = R1 ?i R2".ToTokens(), AddressOf LogicOperation),
("R0 = R1 ?i 100".ToTokens(), AddressOf LogicOperation),
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
("DATA".ToTokens(), AddressOf DATAStatement),
("IF R1 ?o ?2 THEN".ToTokens(), AddressOf IFStatement),
("ELSE IF R1 ?o ?2 THEN".ToTokens(), AddressOf ELSEIFStatement),
("ELSE".ToTokens(), AddressOf ELSEStatement),
("END IF".ToTokens(), AddressOf ENDIFStatement),
("EXECUTE ?a".ToTokens(), AddressOf StartPseudoOperation),
("LOCATION 100".ToTokens(), AddressOf LocationPseudoOperation),
("ALIAS ?i = ?.".ToTokens(), AddressOf ALIASPseudoOperation),
("CALL ?a".ToTokens(), AddressOf CALLStatement),
("END PROCEDURE".ToTokens(), AddressOf ENDPROCEDUREStatement),
("PROCEDURE Start".ToTokens(), AddressOf PROCEDUREDeclaration)
}
    ' TODO: consider if we want to allow branch instructions to address without labels
    ' TODO: the LDR and STR could be condensed if we have a wildcard that can match an integer or an identifier
End Class
