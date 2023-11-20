Imports System
Imports System.Data
Imports System.Diagnostics.Tracing
Imports System.Reflection.Emit
Imports System.Runtime.CompilerServices
Imports HLA_VB.HLA_VB.Scanner

Namespace HLA_VB

    Public Module Program
        Const HIGHEST_MEMORY_ADDRESS = 1024

        Public Class MemoryLocation
            Public Const MEMORY_LABEL_WIDTH = 10

            Private labels As List(Of String)

            Sub New()
                labels = New List(Of String)
            End Sub

            Sub AddLabel(label As String)
                labels.Add(label)
            End Sub

            Function HasLabel(label As String) As Boolean
                Return labels.Contains(label)
            End Function

            Public Overrides Function ToString() As String
                If labels.Count = 0 Then
                    Return $"{String.Empty,-10} " ' Note space at end
                Else
                    Return String.Join(Environment.NewLine, labels.Select(Function(label) (label + ":").PadRight(10))) + " "
                End If
            End Function
        End Class

        Class Data
            Inherits MemoryLocation

            Private value As Integer

            Sub New()
                value = 0
            End Sub

            Sub New(value As Integer)
                Me.value = value
            End Sub

            Function GetValue() As Integer
                Return value
            End Function

            Sub SetValue(value As Integer)
                Me.value = value
            End Sub

            Public Overrides Function ToString() As String
                Return $"{MyBase.ToString()}{value}"
            End Function
        End Class

        Class Instruction
            Inherits MemoryLocation
        End Class

        Class EmptyInstruction
            ' TODO: not tested. Does it need to be?
            Inherits Instruction
            ' Used to label empty lines. May not be needed since we can have multiple labels to a memory location

            Public Overrides Function ToString() As String
                Return MyBase.ToString()
            End Function
        End Class

        Class Memory
            Private words(HIGHEST_MEMORY_ADDRESS) As MemoryLocation
        End Class

        Public Enum TokenType
            Keyword
            Register
            StringLiteral
            IntegerLiteral
            CharacterLiteral ' TODO: character literals not in specification
            Identifier
            Symbol
            EndOfText
        End Enum
        Public Enum KeywordType
            [IF]
            [MEMORY]
            [GOTO]
            [DATA]
            [END]
        End Enum

        Class Token
            Public type As TokenType
            Public k As KeywordType
            Public s As String
            Public i As Integer
            Public r As Integer
            Public c As Char
            Public id As String
            Public sym As String
        End Class

        Class Scanner


            Private text As String
            Private position As Integer = -1
            Private ch As Char
            Private Const EOT = Chr(0)
            Private ReadOnly keywordStrings As List(Of String) = New List(Of String)() From
                                                            {"IF",
                                                            "MEMORY",
                                                            "GOTO", "GO", "BRANCH",
                                                            "DATA",
                                                            "END", "STOP", "HALT"}

            Private ReadOnly keywords As List(Of KeywordType) = New List(Of KeywordType) From
                                                            {KeywordType.IF,
                                                            KeywordType.MEMORY,
                                                            KeywordType.GOTO, KeywordType.GOTO, KeywordType.GOTO,
                                                            KeywordType.DATA,
                                                            KeywordType.END, KeywordType.END, KeywordType.END}

            Public t As Token

            Sub New(onText As String)
                text = onText
                position = 0
                NextCharacter()
            End Sub

            Private Sub NextCharacter()
                If position < text.Length Then
                    ch = text(position)
                    position += 1
                Else
                    ch = EOT
                End If
            End Sub

            Sub NextToken()
                t = New Token()
                Do Until ch = EOT OrElse Not Char.IsWhiteSpace(ch)
                    NextCharacter()
                Loop
                If ch = EOT Then
                    t.type = TokenType.EndOfText
                ElseIf Char.IsDigit(ch) Then
                    t.i = 0
                    Do
                        t.i = t.i * 10 + Integer.Parse(ch)
                        NextCharacter()
                    Loop Until Not Char.IsDigit(ch)
                    t.type = TokenType.IntegerLiteral
                ElseIf Char.IsLetter(ch) Then
                    Dim identifier As String = ""
                    Do
                        identifier += ch
                        NextCharacter()
                    Loop Until Not Char.IsLetterOrDigit(ch)
                    If keywordStrings.Contains(identifier.ToUpper()) Then
                        t.k = keywords(keywordStrings.IndexOf(identifier.ToUpper()))
                        t.type = TokenType.Keyword
                    ElseIf identifier.ToUpper()(0) = "R" AndAlso identifier.Length > 1 AndAlso Integer.TryParse(identifier.Substring(1), t.r) Then
                        t.r = Integer.Parse(identifier.Substring(1)) ' strictly superfluous (see TryParse)
                        t.type = TokenType.Register
                    Else
                        t.id = identifier.ToUpper()
                        t.type = TokenType.Identifier
                    End If
                ElseIf Char.IsPunctuation(ch) OrElse Char.IsSymbol(ch) Then
                    Select Case ch
                        Case ",", "#", "[", "]", "(", ")"
                            t.sym = ch
                            t.type = TokenType.Symbol
                            NextCharacter()
                        Case "+", "-"
                            t.sym = ch
                            NextCharacter()
                            If ch = "=" Then ' allow +=, -=
                                t.sym += ch
                                NextCharacter()
                            End If
                            t.type = TokenType.Symbol
                        Case "!"
                            NextCharacter()
                            If ch = "=" Then
                                t.sym = "<>"
                                t.type = TokenType.Symbol
                                NextCharacter()
                            Else
                                Throw New Exception($"Unexpected NextToken symbol case for:{ch}")
                            End If
                        Case "<", ">"
                            t.sym = ch
                            NextCharacter()
                            If ch = t.sym Or ch = "=" Then ' So <<, <=, >>, >=
                                t.sym += ch
                                NextCharacter()
                            ElseIf ch = ">" Then
                                t.sym = "<>"
                                NextCharacter()
                            End If
                            t.type = TokenType.Symbol
                        Case Else
                            Throw New Exception($"Unexpected NextToken symbol case for:{ch}")
                    End Select
                Else
                    Throw New Exception($"Unexpected NextToken case for:{ch}")
                End If
            End Sub

        End Class

        <Extension()>
        Function ToTokens(source As String) As List(Of Token)
            Dim result As New List(Of Token)
            Dim s As New Scanner(If(source, ""))
            Do
                s.NextToken()
                result.Add(s.t)
            Loop Until s.t.type = TokenType.EndOfText
            Return result
        End Function

        Sub Main(args As String())
            Console.WriteLine("Hello World!")
        End Sub
    End Module

End Namespace