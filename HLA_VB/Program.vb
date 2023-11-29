Imports System
Imports System.Data
Imports System.Diagnostics.Tracing
Imports System.IO
Imports System.Net.NetworkInformation
Imports System.Reflection.Emit
Imports System.Runtime.CompilerServices

Namespace HLA_VB
    ' TODO: think about enumerations for symbols and/or operators

    Public Module Program
        Const HLA_EXTENSION = ".hla"

        Public Const HIGHEST_MEMORY_ADDRESS = 1024
        Public Const HIGHEST_REGISTER_NUMBER = 15
        Public Const REGISTER_SIZE = 32

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

            Private ReadOnly text As String
            Private position As Integer = -1
            Private ch As Char
            Private Const EOT = Chr(0)
            Private ReadOnly keywordStrings = New List(Of String)() From
                                                    {"IF",
                                                    "MEMORY",
                                                    "GOTO", "GO", "BRANCH",
                                                    "DATA",
                                                    "END", "STOP", "HALT"}

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
                    ElseIf identifier.ToUpper()(0) = "R" AndAlso identifier.Length > 1 AndAlso Integer.TryParse(identifier.Substring(1).AsSpan(), t.r) Then
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
                        Case "="
                            t.sym = ch
                            t.type = TokenType.Symbol
                            NextCharacter()
                            If ch = "=" Then ' allows == to be 'translated' to =. Means that the assignments could get away with using  ==
                                NextCharacter()
                            End If
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
                            ElseIf ch = ">" Then ' must be <> because this case is for < or > and >> would be picked up on previous if
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

        Sub DisplayMenu()
            Console.WriteLine("L - (L)oad a HLA file")
            Console.WriteLine("D - (D)isplay the HLA program")
            Console.WriteLine("C - (C)ompile the current HLA program")
            Console.WriteLine("E - (E)xecute the compiled program")
            Console.WriteLine("N - create a (N)ew HLA program")
            Console.WriteLine("H - display the (H)elp file")
            Console.WriteLine("Q - (Q)uit the HLA program")
        End Sub

        Function GetMenuOption() As String
            Console.Write("Enter option: ")
            Return Console.ReadLine().ToUpper()
        End Function

        Function LoadHLAFile(existingProgram As List(Of String)) As List(Of String)
            Console.Write("HLA filename: ")
            Dim Filename = Console.ReadLine().ToUpper()
            If Not Filename.EndsWith(HLA_EXTENSION) Then
                Filename += HLA_EXTENSION
            End If
            Dim program = New List(Of String)
            If File.Exists(Filename) Then
                Using HLA = New StreamReader(Filename)
                    program.AddRange(HLA.ReadToEnd().Split(Environment.NewLine))
                End Using
            Else
                Console.WriteLine($"Couldn't find the file {Filename}")
                program = existingProgram
            End If
            Return program
        End Function

        Sub DisplayHLA(program As List(Of String))
            Dim number = 1
            For Each line In program
                Console.WriteLine($"{number,-4}  {line}")
                number += 1
            Next
        End Sub

        Function NewHLAProgram() As List(Of String)

            Console.WriteLine("Enter your program line by line. Just press Enter to finish")
            Dim lineNumber = 1
            Dim Program = New List(Of String)
            Do
                Console.Write($"Line {lineNumber}: ")
                Dim line = Console.ReadLine()
                If line <> "" Then
                    Debug.Assert(line IsNot Nothing)
                    Program = Program.Append(line).ToList()
                    lineNumber += 1
                Else
                    Exit Do
                End If
            Loop

            Return Program
        End Function

        Sub Main(args As String())
            Dim program As New List(Of String)
            Do
                DisplayMenu()
                Select Case GetMenuOption()
                    Case "L"
                        program = LoadHLAFile(program)
                    Case "D"
                        DisplayHLA(program)
                    Case "C"
                    Case "E"
                    Case "N"
                        program = NewHLAProgram()
                    Case "H"
                    Case "Q"
                        Exit Do
                    Case Else
                End Select
            Loop
        End Sub
    End Module

End Namespace