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
                Throw New Exception($"Invalid operator match for {t(3).sym}")
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
                Throw New Exception($"Invalid operator match for {t(3).sym}")
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

    Function MVNOperation(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' Rd = NOT ?2
        ' 0  1 2   3
        If t(3).type = TokenType.Register Then
            Return New List(Of MemoryLocation)() From {New MVNRegisterInstruction(t(0).r, t(3).r)}
        Else
            Return New List(Of MemoryLocation)() From {New MVNImmediateInstruction(t(0).r, t(3).i)}
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
    Private IFCount = -1
    Private FORCount = -1
    Private WHILECount = -1
    Private ReadOnly FORStatements As New Stack(Of (Rd As Integer, Incrementing As Boolean))
    ' FOR loops are unusual in that we need to know about the register used and the 'direction' of the
    ' loop when we compile the END FOR
    Private ReadOnly IFStatements As New Stack(Of Integer)
    ' IF statements unusual in that we need to what label we need to generate after the next ELSE IF, ELSE, or END IF

    Sub ResetStructureTracking()
        REPEATCount = -1
        IFCount = -1
        FORCount = -1
        WHILECount = -1
        FORStatements.Clear()
        IFStatements.Clear()
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
        FORStatements.Push((t(1).r, True))
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
        c.AddLabel($"FOR{FORCount}")
        Return New List(Of MemoryLocation)() From {m,
                                                   c,
                                                   New BGTInstruction($"ENDFOR{FORCount}")}
    End Function

    Function FORDOWNTOStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' FOR R1 = ?2 DOWNTO ?2
        '  0  1  2 3  4      5
        FORCount += 1
        FORStatements.Push((t(1).r, False))
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
        c.AddLabel($"FOR{FORCount}")
        Return New List(Of MemoryLocation)() From {m,
                                                   c,
                                                   New BLTInstruction($"ENDFOR{FORCount}")}
    End Function

    Function ENDFOR(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' ENDFOR or END FOR
        '  0
        FORCount -= 1
        Dim ChangeIndex As ArithmeticLogicInstruction
        Dim f As (rd As Integer, Incrementing As Boolean)
        If FORStatements.TryPop(f) Then
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
        Dim start = $"WHILE{WHILECount}", finish = $"ENDWHILE{WHILECount}"

        Dim c, b, beq As Instruction
        ' beq used to handle WHILE < using a BGT and a BEQ.
        beq = Nothing
        ' TODO Optimise the immediate case by increasing/decreasing the operand

        If t(3).type = TokenType.Register Then
            c = New CMPRegisterInstruction(t(1).r, t(3).r)
        Else
            c = New CMPImmediateInstruction(t(1).r, t(3).i)
        End If
        c.AddLabel(start)
        Select Case t(2).sym
            Case "<"
                b = New BGTInstruction(finish)
                beq = New BEQInstruction(finish)
            Case "<="
                b = New BGTInstruction(finish)
            Case ">"
                b = New BLTInstruction(finish)
                beq = New BEQInstruction(finish)
            Case ">="
                b = New BLTInstruction(finish)
            Case "="
                b = New BNEInstruction(finish)
            Case "<>"
                b = New BEQInstruction(finish)
            Case Else
                Debug.Fail($"Invalid operator {t(2).sym} in WHILE statement")
                ' TODO: remove most of the DebUgs
                b = Nothing
        End Select
        Dim result = New List(Of MemoryLocation)() From {c, b}
        If beq IsNot Nothing Then
            result.Add(New BEQInstruction(finish))
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

    Function IFStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' IF R1 ?o ?2 THEN
        ' 0  1  2  3  4
        IFCount += 1
        IFStatements.Push(0) ' Top of stack holds the integer to be used in the next label after ELSE IF, ELSE, or END IF
        Dim nextLabel = $"EIF{IFCount}_{IFStatements.Peek()}"

        ' TODO: refactor this so we can generate the common pattern of CMP .../ B.. ... (and its opposite)
        Dim c, b, beq As Instruction
        ' beq used to handle UNTIL < using a BGT and a BEQ.
        beq = Nothing

        If t(3).type = TokenType.Register Then
            c = New CMPRegisterInstruction(t(1).r, t(3).r)
        Else
            c = New CMPImmediateInstruction(t(1).r, t(3).i)
        End If
        Select Case t(2).sym
            Case "<"
                b = New BGTInstruction(nextLabel)
                beq = New BEQInstruction(nextLabel)
            Case "<="
                b = New BGTInstruction(nextLabel)
            Case ">"
                b = New BLTInstruction(nextLabel)
                beq = New BEQInstruction(nextLabel)
            Case ">="
                b = New BLTInstruction(nextLabel)
            Case "="
                b = New BNEInstruction(nextLabel)
            Case "<>"
                b = New BEQInstruction(nextLabel)
            Case Else
                Throw New Exception($"Invalid operator {t(2).sym} in IF statement")
                b = Nothing
        End Select
        Dim result = New List(Of MemoryLocation)() From {c, b}
        If beq IsNot Nothing Then
            result.Add(New BEQInstruction(nextLabel))
        End If
        Return result
    End Function

    Function ComparisonInstructions(r As Integer, op As String, operand2 As Token, compLabel As String, nextLabel As String) As List(Of MemoryLocation)
        ' TODO: refactor this so we can generate the common pattern of CMP .../ B.. ... (and its opposite)
        Dim c, b, beq As Instruction
        ' beq used to handle UNTIL < using a BGT and a BEQ.
        beq = Nothing

        If operand2.type = TokenType.Register Then
            c = New CMPRegisterInstruction(r, operand2.r)
        Else
            c = New CMPImmediateInstruction(r, operand2.i)
        End If
        c.AddLabel(compLabel)
        Select Case op
            Case "<"
                b = New BGTInstruction(nextLabel)
                beq = New BEQInstruction(nextLabel)
            Case "<="
                b = New BGTInstruction(nextLabel)
            Case ">"
                b = New BLTInstruction(nextLabel)
                beq = New BEQInstruction(nextLabel)
            Case ">="
                b = New BLTInstruction(nextLabel)
            Case "="
                b = New BNEInstruction(nextLabel)
            Case "<>"
                b = New BEQInstruction(nextLabel)
            Case Else
                Throw New Exception($"Invalid operator {op} in IF statement")
                b = Nothing
        End Select
        Dim result = New List(Of MemoryLocation)() From {c, b}
        If beq IsNot Nothing Then
            result.Add(New BEQInstruction(nextLabel))
        End If
        Return result
    End Function

    Function ELSEIFStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' ELSE IF R1 ?o ?2 THEN
        ' 0    1  2  3  4  5
        If IFStatements.Any() Then
            Dim IFBlockNumber = IFStatements.Pop()
            IFStatements.Push(IFBlockNumber + 1)
            Dim b = New BInstruction($"EIF{IFCount}") ' branch after the THEN block has executed
            Dim l = $"EIF{IFCount}_{IFBlockNumber}" ' label for ELSE IF comparisons, added to comparison instruction
            Return ComparisonInstructions(t(2).r, t(3).sym, t(4), l, $"EIF{IFCount}_{IFStatements.Peek()}")
        Else
            Throw New Exception($"ELSE without a corrsponding IF")
        End If
    End Function

#Disable Warning IDE0060 ' Remove unused parameter
    Function ELSEStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
#Enable Warning IDE0060 ' Remove unused parameter
        ' ELSE
        ' 0
        If IFStatements.Any() Then
            Dim IFBlockNumber = IFStatements.Pop()
            IFStatements.Push(IFBlockNumber + 1)
            Return New List(Of MemoryLocation) From {New BInstruction($"EIF{IFCount}"), New Label($"EIF{IFCount}_{IFBlockNumber}")}
        Else
            Throw New Exception($"ELSE without a corresponding IF")
        End If
    End Function

#Disable Warning IDE0060 ' Remove unused parameter
    Function ENDIFStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
#Enable Warning IDE0060 ' Remove unused parameter
        ' END IF
        '  0  1
        If IFStatements.Any() Then
            Dim IFBlockNumber = IFStatements.Pop()
            Return New List(Of MemoryLocation)() From {New Label($"EIF{IFCount}_{IFBlockNumber}"), New Label($"EIF{IFCount}")}
        Else
            Throw New Exception("END IF without a corresponding IF")
        End If
    End Function
#End Region

#Region "DATA and other pseudo-instructions"
    Function DATAStatement(t As IEnumerable(Of Token)) As List(Of MemoryLocation)
        ' DATA 100
        '   0   1
        If t.Count > 2 Then ' Remember t has an EndOfText token at the end
            Return New List(Of MemoryLocation)() From {New Data(t(1).i)}
        Else
            Return New List(Of MemoryLocation)() From {New Data(0)}
        End If
    End Function
#End Region
End Module
