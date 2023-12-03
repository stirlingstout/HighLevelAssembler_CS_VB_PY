Imports System.Runtime.CompilerServices

Namespace HLA_VB

    Public Module Scanner
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

            Sub New(k As KeywordType)
                Me.type = TokenType.Keyword
                Me.k = k
            End Sub

            Sub New()
                Me.type = TokenType.EndOfText
            End Sub

            Sub New(i As Integer)
                Me.type = TokenType.IntegerLiteral
                Me.i = i
            End Sub

            Sub New(s As String)
                Me.type = TokenType.StringLiteral
                Me.s = s
            End Sub

            Sub New(r As Byte)
                Me.type = TokenType.Register
                Me.r = r
            End Sub

            Sub New(c As Char)
                Me.type = TokenType.CharacterLiteral
                Me.c = c
            End Sub

            Sub New(s As String, type As TokenType)
                Debug.Assert(type = TokenType.Symbol OrElse type = TokenType.Identifier)
                Me.type = type
                If type = TokenType.Symbol Then
                    Me.sym = s
                Else
                    Me.id = s
                End If
            End Sub
        End Class

        Public Class Scanner

            Private ReadOnly text As String
            Private position As Integer = -1
            Private ch As Char
            Private Const EOT = Chr(0)
            Private ReadOnly keywordStrings = New List(Of String)() From
                                            {"IF",
                                            "MEMORY",
                                            "GOTO",
                                            "DATA",
                                            "END"}

            Private ReadOnly keywords = New List(Of KeywordType) From
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
                Do Until ch = EOT OrElse Not Char.IsWhiteSpace(ch)
                    NextCharacter()
                Loop
                If ch = EOT Then
                    t = New Token()
                ElseIf Char.IsDigit(ch) Then
                    Dim i = 0
                    Do
                        i = i * 10 + Integer.Parse(ch)
                        NextCharacter()
                    Loop Until Not Char.IsDigit(ch)
                    t = New Token(i)
                ElseIf Char.IsLetter(ch) Then
                    Dim identifier As String = ""
                    Do
                        identifier += ch
                        NextCharacter()
                    Loop Until Not Char.IsLetterOrDigit(ch)
                    If keywordStrings.Contains(identifier.ToUpper()) Then
                        t = New Token(CType(keywordStrings.IndexOf(identifier.ToUpper()), KeywordType))
                    ElseIf identifier.ToUpper()(0) = "R" AndAlso identifier.Length > 1 AndAlso identifier.Length <= 3 Then
                        Dim r As Byte
                        t = New Token(identifier.ToUpper(), TokenType.Identifier)
                        If Byte.TryParse(identifier.AsSpan(1), r) Then
                            t = New Token(r)
                        End If
                    Else
                        t = New Token(identifier.ToUpper(), TokenType.Identifier)
                    End If
                ElseIf Char.IsPunctuation(ch) OrElse Char.IsSymbol(ch) Then
                    Dim sym As String
                    Select Case ch
                        Case ",", "#", "[", "]", "(", ")", ":"
                            sym = ch
                            NextCharacter()
                        Case "="
                            sym = ch
                            NextCharacter()
                            If ch = "=" Then ' allows == to be 'translated' to =. Means that the assignments could get away with using  ==
                                NextCharacter()
                            End If
                        Case "+", "-"
                            sym = ch
                            NextCharacter()
                            If ch = "=" Then ' allow +=, -=
                                sym += ch
                                NextCharacter()
                            End If
                        Case "!"
                            NextCharacter()
                            If ch = "=" Then
                                sym = "<>"
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
                            ElseIf ch = ">" Then ' must be <> because this case is for < or > and >> would be picked up on previous if
                                sym = "<>"
                                NextCharacter()
                            End If
                        Case Else
                            Throw New Exception($"Unexpected NextToken symbol case for:{ch}")
                    End Select
                    t = New Token(sym, TokenType.Symbol)
                Else
                    Throw New Exception($"Unexpected NextToken case for:{ch}")
                End If
            End Sub

        End Class

        <Extension()>
        Public Function ToTokens(source As String) As List(Of Token)
            Dim result As New List(Of Token)
            Dim s As New Scanner(If(source, ""))
            Do
                s.NextToken()
                result.Add(s.t)
            Loop Until s.t.type = TokenType.EndOfText
            Return result
        End Function

        ''' <summary>
        ''' For two tokens to 'match' they must be of the same type.
        ''' Further, if they are symbols their text must match
        ''' So a token for R1 will match a token for R0, but 
        ''' a token for = will only match a token for =
        ''' </summary>
        ''' <param name="first"></param>
        ''' <param name="second"></param>
        ''' <returns></returns>
        <Extension>
        Function Matches(first As Token, second As Token) As Boolean
            Dim result = False
            If first.type = second.type Then
                result = True
                If first.type = TokenType.Keyword Then
                    result = (first.k = second.k)
                ElseIf first.type = TokenType.Symbol Then
                    result = (first.sym = second.sym)
                End If
            End If
            Return result
        End Function

        <Extension>
        Public Function Matches(tokenList As List(Of Token), pattern As List(Of Token)) As Boolean
            Return tokenList.Count = pattern.Count AndAlso
                tokenList.Zip(pattern).All(Function(pair) pair.First.Matches(pair.Second))

        End Function

    End Module
End Namespace