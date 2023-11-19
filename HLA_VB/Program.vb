Imports System
Imports System.Data
Imports System.Diagnostics.Tracing
Imports System.Reflection.Emit
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

        Class Scanner
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
            Public Enum KeywordType
                [IF]
                [MEMORY]
                [GOTO]
                [DATA]
                [END]
            End Enum
            Private ReadOnly keywords As List(Of KeywordType) = New List(Of KeywordType) From
                                                            {KeywordType.IF,
                                                            KeywordType.MEMORY,
                                                            KeywordType.GOTO, KeywordType.GOTO, KeywordType.GOTO,
                                                            KeywordType.DATA,
                                                            KeywordType.END, KeywordType.END, KeywordType.END}

            Public type As TokenType
            Public k As KeywordType
            Public s As String
            Public i As Integer
            Public r As Integer
            Public c As Char
            Public id As String
            Public sym As String

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
                Do Until ch = EOT OrElse Not Char.IsWhiteSpace(ch)
                    NextCharacter()
                Loop
                If ch = EOT Then
                    type = TokenType.EndOfText
                ElseIf Char.IsDigit(ch) Then
                    i = 0
                    Do
                        i = i * 10 + Integer.Parse(ch)
                        NextCharacter()
                    Loop Until Not Char.IsDigit(ch)
                    type = TokenType.IntegerLiteral
                ElseIf Char.IsLetter(ch) Then
                    Dim identifier As String = ""
                    Do
                        identifier += ch
                        NextCharacter()
                    Loop Until Not Char.IsLetterOrDigit(ch)
                    If keywordStrings.Contains(identifier.ToUpper()) Then
                        k = keywords(keywordStrings.IndexOf(identifier.ToUpper()))
                        type = TokenType.Keyword
                    ElseIf identifier.ToUpper()(0) = "R" AndAlso identifier.Length > 1 AndAlso Integer.TryParse(identifier.Substring(1), r) Then
                        r = Integer.Parse(identifier.Substring(1)) ' strictly superfluous (see TryParse)
                        type = TokenType.Register
                    Else
                        id = identifier.ToUpper()
                        type = TokenType.Identifier
                    End If
                ElseIf Char.IsPunctuation(ch) OrElse Char.IsSymbol(ch) Then
                    Select Case ch
                        Case ",", "#", "[", "]", "(", ")"
                            sym = ch
                            type = TokenType.Symbol
                            NextCharacter()
                        Case "+", "-"
                            sym = ch
                            NextCharacter()
                            If ch = "=" Then ' allow +=, -=
                                sym += ch
                                NextCharacter()
                            End If
                            type = TokenType.Symbol
                        Case "!"
                            NextCharacter()
                            If ch = "=" Then
                                sym = "<>"
                                type = TokenType.Symbol
                                NextCharacter()
                            Else
                                Throw New Exception($"Unexpected NextToken symbol case for:{ch}")
                            End If
                        Case "<", ">"
                            sym = ch
                            NextCharacter()
                            If ch = sym Or ch = "=" Then ' So <<, <=, >>, >=
                                sym += ch
                                NextCharacter()
                            ElseIf ch = ">" Then
                                sym = "<>"
                                type = TokenType.Symbol
                                NextCharacter()
                            End If
                            type = TokenType.Symbol
                        Case Else
                            Throw New Exception($"Unexpected NextToken symbol case for:{ch}")
                    End Select
                Else
                    Throw New Exception($"Unexpected NextToken case for:{ch}")
                End If
            End Sub

        End Class

        Sub Main(args As String())
            Console.WriteLine("Hello World!")
        End Sub
    End Module

End Namespace