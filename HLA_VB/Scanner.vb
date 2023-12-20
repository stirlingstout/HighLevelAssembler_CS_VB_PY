Imports System.Runtime.CompilerServices

Namespace HLA_VB

    Public Module Scanner
        Public Enum TokenType
            Keyword
            Register
            StringLiteral
            IntegerLiteral
            CharacterLiteral ' TODO: string, character literals not in specification
            Identifier
            Symbol
            Wildcard
            EndOfText
        End Enum

        ''' <summary>
        ''' Location, Start, and PROCEDURE need to be keywords otherwise if they are just identifiers there are two possible parses for each
        ''' </summary>
        Public Enum KeywordType
            [IF]
            [MEMORY]
            [GOTO]
            [DATA]
            [END]
            [FOR]
            [TO]
            [DOWNTO]
            [REPEAT]
            [UNTIL]
            [WHILE]
            [THEN]
            [ELSE]
            [LOCATION]
            [EXECUTE]
            [ALIAS]
            [CALL]
            [PROCEDURE]
            [RETURN]
        End Enum

        Class Token
            Public type As TokenType
            Public k As KeywordType
            Public s As String
            Public i As Integer
            Public r As Integer
            Public c As Char
            Public id As String
            Public w As String
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
                Debug.Assert(type = TokenType.Symbol OrElse type = TokenType.Identifier OrElse type = TokenType.Wildcard)
                Me.type = type
                Select Case type
                    Case TokenType.Symbol
                        Me.sym = s
                    Case TokenType.Identifier
                        Me.id = s
                    Case TokenType.Wildcard
                        Me.w = s
                    Case Else
                        Debug.Assert($"Invalid token type for {s}")
                End Select
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
                                            "END",
                                            "FOR",
                                            "TO",
                                            "DOWNTO",
                                            "REPEAT",
                                            "UNTIL",
                                            "WHILE",
                                            "THEN",
                                            "ELSE",
                                            "LOCATION",
                                            "EXECUTE",
                                            "ALIAS",
                                            "CALL",
                                            "PROCEDURE",
                                            "RETURN"
                                            }

            Private ReadOnly keywords = New List(Of KeywordType) From
                                            {KeywordType.IF,
                                            KeywordType.MEMORY,
                                            KeywordType.GOTO, KeywordType.GOTO, KeywordType.GOTO,
                                            KeywordType.DATA,
                                            KeywordType.END, KeywordType.END, KeywordType.END,
                                            KeywordType.FOR, KeywordType.TO, KeywordType.DOWNTO,
                                            KeywordType.REPEAT, KeywordType.UNTIL,
                                            KeywordType.WHILE,
                                            KeywordType.THEN, KeywordType.ELSE,
                                            KeywordType.LOCATION, KeywordType.EXECUTE, KeywordType.ALIAS,
                                            KeywordType.CALL, KeywordType.PROCEDURE, KeywordType.RETURN}

            Public Shared ReadOnly Aliases As New Dictionary(Of String, Token)

            Public Shared Sub ClearAliases()
                Aliases.Clear()
            End Sub

            Public Shared Sub AddAlias(name As String, value As Token)
                If Aliases.TryAdd(name, value) Then
                Else
                    Throw New Exception($"Attempt to add a duplicate of an existing alias {name}")
                End If
            End Sub

            Public Shared Sub RemoveAlias(name As String)
                If Aliases.Remove(name) Then
                Else
                    Throw New Exception($"Attempt to remove a missing alias {name}")
                End If
            End Sub

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
                If ch = "'" Then    ' Used for comments in the VB.NET version
                    ch = EOT
                End If
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
                    ElseIf Aliases.TryGetValue(identifier.ToUpper(), t) Then
                        Debug.Assert(True)
                    Else
                        t = New Token(identifier.ToUpper(), TokenType.Identifier)
                    End If
                ElseIf Char.IsPunctuation(ch) OrElse Char.IsSymbol(ch) Then
                    Dim sym As String, type As TokenType = TokenType.Symbol
                    Select Case ch
                        Case "?"
                            NextCharacter()
                            sym = Char.ToUpper(ch)           ' ?2 matches a register or an integer, ?o matches an operator, ?i matches an identifier
                            type = TokenType.Wildcard           ' ?a matches an integer or an identifier, ?. matches any single token, ?* matches any number of tokens
                            NextCharacter()
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
                    t = New Token(sym, type)
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
        ''' a token for = will only match a token for =.
        ''' A wildcard token can be ?2 (to match either a register or an integer literal),
        ''' ?o to match an operator (symbol really), ?i to match an identifier
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
            ElseIf second.type = TokenType.Wildcard Then
                Select Case second.w
                    Case "2" ' <operand2>: either an integer literal or a register
                        result = (first.type = TokenType.IntegerLiteral OrElse first.type = TokenType.Register)
                    Case "I"
                        result = (first.type = TokenType.Identifier)
                    Case "O"
                        result = (first.type = TokenType.Symbol)
                    Case "A" ' either a label or an address
                        result = (first.type = TokenType.Identifier OrElse first.type = TokenType.IntegerLiteral)
                    Case "." ' any token
                        result = True
                    Case "*" ' any number of tokens
                        result = True ' TODO: this won't completely work since Matches only works on a single token.
                    Case Else
                        Debug.Fail($"Unrecognised wildcard {second.w}")
                End Select
            End If
            Return result
        End Function

        <Extension>
        Public Function Matches(tokenList As IEnumerable(Of Token), pattern As IEnumerable(Of Token)) As Boolean
            Return tokenList.Count = pattern.Count AndAlso
                tokenList.Zip(pattern).All(Function(pair) pair.First.Matches(pair.Second))

        End Function

    End Module
End Namespace