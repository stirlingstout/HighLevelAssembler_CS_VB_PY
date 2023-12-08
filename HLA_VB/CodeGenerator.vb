Imports System.Data
Imports System.Runtime.CompilerServices
Imports System.Security.Cryptography
Imports HLA_VB.HLA_VB.Instructions
Imports HLA_VB.HLA_VB.Scanner

Module CodeGenerator

    ' These functions take a list of tokens representing the current line and return a list of
    ' memory locations/instructions. List because IF ... GOTO generates two instructions and some of the
    ' control structures generate even more
    Function LDRDirect(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = MEMORY[100]
        ' 0  1  2    3 4 5
        Return New List(Of MemoryLocation)() From {New LoadInstructionDirect(t(0).r, t(4).i)}
    End Function

    Function LDRDirectLabel(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = MEMORY[Start]
        ' 0  1  2 3  4  5
        Return New List(Of MemoryLocation)() From {New LoadInstructionDirect(t(0).r, t(4).id)}
    End Function

    Function STRDirect(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' MEMORY[100] = Rd
        '  0 1 2 3 4 5
        Return New List(Of MemoryLocation)() From {New StoreInstructionDirect(t(5).r, t(2).i)}
    End Function

    Function STRDirectLabel(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' MEMORY[Start] = Rd
        '  0 1  2  3 4 5
        Return New List(Of MemoryLocation)() From {New StoreInstructionDirect(t(5).r, t(2).id)}
    End Function

    Function ArithmeticOperation(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn ?o ?2
        ' 0  1 2  3  4
        Dim i As Instruction
        Select Case t(3).sym
            Case "+"
                If t(4).type = TokenType.IntegerLiteral Then
                    i = New ADDImmediateInstruction(t(0).r, t(2).r, t(4).i)
                Else
                    i = New ADDRegisterInstruction(t(0).r, t(2).r, t(4).r)
                End If
            Case "-"
                If t(4).type = TokenType.IntegerLiteral Then
                    i = New SUBImmediateInstruction(t(0).r, t(2).r, t(4).i)
                Else
                    i = New SUBRegisterInstruction(t(0).r, t(2).r, t(4).r)
                End If
            Case "<<"
                If t(4).type = TokenType.IntegerLiteral Then
                    i = New LSLImmediateInstruction(t(0).r, t(2).r, t(4).i)
                Else
                    i = New LSLRegisterInstruction(t(0).r, t(2).r, t(4).r)
                End If
            Case ">>"
                If t(4).type = TokenType.IntegerLiteral Then
                    i = New LSRImmediateInstruction(t(0).r, t(2).r, t(4).i)
                Else
                    i = New LSRRegisterInstruction(t(0).r, t(2).r, t(4).r)
                End If
            Case Else
                Debug.Fail($"Invalid operator match for {t(3).sym}")
                i = Nothing
        End Select
        Return New List(Of MemoryLocation)() From {i}
    End Function

    Function LogicOperation(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = Rn ?i ?2
        ' 0  1 2  3  4
        Dim i As Instruction
        Select Case t(3).id
            Case "AND"
                If t(4).type = TokenType.IntegerLiteral Then
                    i = New ANDImmediateInstruction(t(0).r, t(2).r, t(4).i)
                Else
                    i = New ANDRegisterInstruction(t(0).r, t(2).r, t(4).r)
                End If
            Case "OR"
                If t(4).type = TokenType.IntegerLiteral Then
                    i = New ORRImmediateInstruction(t(0).r, t(2).r, t(4).i)
                Else
                    i = New ORRRegisterInstruction(t(0).r, t(2).r, t(4).r)
                End If
            Case "EOR"
                If t(4).type = TokenType.IntegerLiteral Then
                    i = New EORImmediateInstruction(t(0).r, t(2).r, t(4).i)
                Else
                    i = New EORRegisterInstruction(t(0).r, t(2).r, t(4).r)
                End If
            Case Else
                Debug.Fail($"Invalid operator match for {t(3).sym}")
                i = Nothing
        End Select
        Return New List(Of MemoryLocation)() From {i}
    End Function

    Function MOVOperation(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = ?2
        ' 0  1 2
        If t(2).type = TokenType.Register Then
            Return New List(Of MemoryLocation)() From {New MOVRegisterInstruction(t(0).r, t(2).r)}
        Else
            Return New List(Of MemoryLocation)() From {New MOVImmediateInstruction(t(0).r, t(2).i)}
        End If
    End Function

    Function SimpleIFStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' IF Rn ?o  ?2 GOTO Start (may need another wildcard if we allow labels and integers, i.e., branches to direct addresses)
        ' 0  1  2   3  4    5
        Dim c As Instruction, b As Instruction
        If t(3).type = TokenType.Register Then
            c = New CMPRegisterInstruction(t(1).r, t(3).r)
        Else
            c = New CMPImmediateInstruction(t(1).r, t(3).i)
        End If
        Select Case t(2).sym
            Case "<"
                b = New BLTInstruction(t(5).id)
            Case ">"
                b = New BGTInstruction(t(5).id)
            Case "="
                b = New BEQInstruction(t(5).id)
            Case "<>"
                b = New BNEInstruction(t(5).id)
            Case Else
                Throw New Exception($"Invalid operator {t(2).sym} in IF statement")
        End Select
        Return New List(Of MemoryLocation)() From {c, b}
    End Function

    Function BAlwaysLabel(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' GOTO Start
        ' 0      1
        Return New List(Of MemoryLocation)() From {New BInstruction(t(1).id)}
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
    Function REPEATStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
#Enable Warning IDE0060 ' Remove unused parameter
        ' REPEAT
        ' 0
        REPEATCount += 1
        Return New List(Of MemoryLocation)() From {New Label($"REPEAT{REPEATCount}")}
    End Function

    Function UNTILStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' UNTIL R1 ?o ?2
        ' 0     1  2  3
        REPEATCount -= 1

        Dim c, b, beq As Instruction
        beq = Nothing
        ' beq used to handle UNTIL < using a BGT and a BEQ.
        ' TODO Optimise the immediate case by increasing/decreasing the operand

        If t(3).type = TokenType.Register Then
            c = New CMPRegisterInstruction(t(1).r, t(3).r)
        Else
            c = New CMPImmediateInstruction(t(1).r, t(3).i)
        End If
        Dim destination As String = $"REPEAT{REPEATCount + 1}"
        Select Case t(2).sym
            Case "<"
                b = New BGTInstruction(destination)
                beq = New BEQInstruction(destination)
            Case "<="
                b = New BGTInstruction(destination)
            Case ">"
                b = New BLTInstruction(destination)
                beq = New BEQInstruction(destination)
            Case ">="
                b = New BLTInstruction(destination)
            Case "="
                b = New BNEInstruction(destination)
            Case "<>"
                b = New BEQInstruction(destination)
            Case Else
                Debug.Fail($"Invalid operator {t(2).sym} in UNTIL statement")
                b = Nothing
        End Select
        Dim result = New List(Of MemoryLocation)() From {c, b}
        If beq IsNot Nothing Then
            result.Add(New BEQInstruction(destination))
        End If
        Return result
    End Function

    Function FORTOStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' FOR R1 = ?2 TO ?2
        '  0  1  2 3  4  5
        FORCount += 1
        FORLoops.Push((t(1).r, True))
        Dim m, c, b As Instruction
        If t(3).type = TokenType.Register Then
            m = New MOVRegisterInstruction(t(1).r, t(3).r)
        Else
            m = New MOVImmediateInstruction(t(1).r, t(3).i)
        End If
        If t(5).type = TokenType.Register Then
            c = New CMPRegisterInstruction(t(1).r, t(5).r)
        Else
            c = New CMPImmediateInstruction(t(1).r, t(5).i)
        End If
        Return New List(Of MemoryLocation)() From {m,
                                                   New Label($"FOR{FORCount}"),
                                                   c,
                                                   New BGTInstruction($"ENDFOR{FORCount}")}
    End Function

    Function FORDOWNTOStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' FOR R1 = ?2 DOWNTO ?2
        '  0  1  2 3  4      5
        FORCount += 1
        FORLoops.Push((t(1).r, False))
        Dim m, c, b As Instruction
        If t(3).type = TokenType.Register Then
            m = New MOVRegisterInstruction(t(1).r, t(3).r)
        Else
            m = New MOVImmediateInstruction(t(1).r, t(3).i)
        End If
        If t(5).type = TokenType.Register Then
            c = New CMPRegisterInstruction(t(1).r, t(5).r)
        Else
            c = New CMPImmediateInstruction(t(1).r, t(5).i)
        End If
        Return New List(Of MemoryLocation)() From {m,
                                                   New Label($"FOR{FORCount}"),
                                                   c,
                                                   New BLTInstruction($"ENDFOR{FORCount}")}
    End Function

    Function ENDFOR(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' ENDFOR or END FOR
        '  0
        FORCount -= 1
        Dim ChangeIndex As ArithmeticLogicInstruction
        Dim f As (rd As Integer, Incrementing As Boolean)
        If FORLoops.TryPop(f) Then
            With f
                If .Incrementing Then
                    ChangeIndex = New ADDImmediateInstruction(.rd, .rd, 1)
                Else
                    ChangeIndex = New SUBImmediateInstruction(.rd, .rd, 1)
                End If
            End With
            Return New List(Of MemoryLocation)() From {ChangeIndex,
                                                   New BInstruction($"FOR{FORCount + 1}"),
                                                   New Label($"ENDFOR{FORCount + 1}")}
        Else
            Throw New Exception($"END FOR without a corresponding FOR")
        End If
    End Function


    Function WHILEStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' WHILE R1 ?o ?2
        ' 0     1  2  3
        WHILECount += 1

        Dim c, b, beq As Instruction
        ' beq used to handle UNTIL < using a BGT and a BEQ.
        beq = Nothing
        ' TODO Optimise the immediate case by increasing/decreasing the operand

        If t(3).type = TokenType.Register Then
            c = New CMPRegisterInstruction(t(1).r, t(3).r)
        Else
            c = New CMPImmediateInstruction(t(1).r, t(3).i)
        End If
        Dim destination As String = $"ENDWHILE{WHILECount}"
        Select Case t(2).sym
            Case "<"
                b = New BGTInstruction(destination)
                beq = New BEQInstruction(destination)
            Case "<="
                b = New BGTInstruction(destination)
            Case ">"
                b = New BLTInstruction(destination)
                beq = New BEQInstruction(destination)
            Case ">="
                b = New BLTInstruction(destination)
            Case "="
                b = New BNEInstruction(destination)
            Case "<>"
                b = New BEQInstruction(destination)
            Case Else
                Debug.Fail($"Invalid operator {t(2).sym} in WHILE statement")
                b = Nothing
        End Select
        Dim result = New List(Of MemoryLocation)() From {New Label($"WHILE{WHILECount}"), c, b}
        If beq IsNot Nothing Then
            result.Add(New BEQInstruction(destination))
        End If
        Return result
    End Function

#Disable Warning IDE0060 ' Remove unused parameter
    Function ENDWHILE(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
#Enable Warning IDE0060 ' Remove unused parameter
        ' END WHILE
        '   0   1
        WHILECount -= 1
        Return New List(Of MemoryLocation)() From {New BInstruction($"WHILE{WHILECount + 1}"),
                                                   New Label($"ENDWHILE{WHILECount + 1}")}
    End Function

    Function DATAStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' DATA 100
        '   0   1
        Return New List(Of MemoryLocation)() From {New Data(t(1).i)}
    End Function
#End Region
End Module
