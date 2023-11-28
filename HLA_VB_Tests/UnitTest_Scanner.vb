Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports System.Data
Imports System.Reflection.Metadata

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_Scanner

        <TestClass>
        Public Class TestScanner
            Inherits TestCommon

            <TestMethod>
            Sub TestScanner0()
                Dim s = New Scanner("")
                s.NextToken()
                Assert.AreEqual(TokenType.EndOfText, s.t.type)
            End Sub

            <TestMethod>
            Sub TestScanner1()
                Dim s = New Scanner("!")
                Assert.ThrowsException(Of System.Exception)(AddressOf s.NextToken, "Unexpected NextToken symbol case for:!")
            End Sub

            <TestMethod>
            Sub TestScanner2()
                Dim s = New Scanner("123")
                s.NextToken()
                Assert.AreEqual(TokenType.IntegerLiteral, s.t.type)
                Assert.AreEqual(123, s.t.i)
                s.NextToken()
                Assert.AreEqual(TokenType.EndOfText, s.t.type)
            End Sub

            <TestMethod>
            Sub TestScanner3()
                Dim s = New Scanner("0")
                s.NextToken()
                Assert.AreEqual(TokenType.IntegerLiteral, s.t.type)
                Assert.AreEqual(0, s.t.i)
                s.NextToken()
                Assert.AreEqual(TokenType.EndOfText, s.t.type)
            End Sub

            <TestMethod>
            Sub TestScanner4()
                Dim s = New Scanner("Start")
                s.NextToken()
                Assert.AreEqual(TokenType.Identifier, s.t.type)
                Assert.AreEqual("START", s.t.id)
                s.NextToken()
                Assert.AreEqual(TokenType.EndOfText, s.t.type)
            End Sub

            <TestMethod>
            Sub TestScanner4_1()
                Dim s = New Scanner("R1 R")
                s.NextToken()
                Assert.AreEqual(TokenType.Register, s.t.type)
                Assert.AreEqual(1, s.t.r)
                s.NextToken()
                Assert.AreEqual(TokenType.Identifier, s.t.type)
                Assert.AreEqual("R", s.t.id)
                s.NextToken()
                Assert.AreEqual(TokenType.EndOfText, s.t.type)
            End Sub

            <TestMethod>
            Sub TestScanner5()
                Dim s = New Scanner("123Start")
                s.NextToken()
                Assert.AreEqual(TokenType.IntegerLiteral, s.t.type)
                Assert.AreEqual(123, s.t.i)
                s.NextToken()
                Assert.AreEqual(TokenType.Identifier, s.t.type)
                Assert.AreEqual("START", s.t.id)
                s.NextToken()
                Assert.AreEqual(TokenType.EndOfText, s.t.type)
            End Sub

            <TestMethod>
            Sub TestScanner6()
                Dim s = New Scanner("   123Start   ")
                s.NextToken()
                Assert.AreEqual(TokenType.IntegerLiteral, s.t.type)
                Assert.AreEqual(123, s.t.i)
                s.NextToken()
                Assert.AreEqual(TokenType.Identifier, s.t.type)
                Assert.AreEqual("START", s.t.id)
                s.NextToken()
                Assert.AreEqual(TokenType.EndOfText, s.t.type)
            End Sub

            <TestMethod>
            Sub TestScanner7()
                Dim s = New Scanner("   123Start  END ")
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.IntegerLiteral)
                Assert.AreEqual(123, s.t.i)
                s.NextToken()
                Assert.AreEqual(TokenType.Identifier, s.t.type)
                Assert.AreEqual("START", s.t.id)
                s.NextToken()
                Assert.AreEqual(TokenType.Keyword, s.t.type)
                Assert.AreEqual(KeywordType.END, s.t.k)
                s.NextToken()
                Assert.AreEqual(TokenType.EndOfText, s.t.type)
            End Sub

            <TestMethod>
            Sub TestScanner8() ' single characters
                Dim s = New Scanner("   ,#[]()")
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual(",", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual("#", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual("[", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual("]", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual("(", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual(")", s.t.sym)
                s.NextToken()
                Assert.AreEqual(TokenType.EndOfText, s.t.type)
            End Sub

            <TestMethod>
            Sub TestScanner9() ' operators and compound operators
                Dim s = New Scanner("   + - += -= != < << > >> <> ")
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual("+", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual("-", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual("+=", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual("-=", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual("<>", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual("<", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual("<<", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual(">", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual(">>", s.t.sym)
                s.NextToken()
                Assert.AreEqual(s.t.type, TokenType.Symbol)
                Assert.AreEqual("<>", s.t.sym)
                s.NextToken()
                Assert.AreEqual(TokenType.EndOfText, s.t.type)
            End Sub
        End Class

        <TestClass>
        Public Class TestToTokens

            <TestMethod>
            Sub TestToTokens0()
                Dim l As New List(Of Token)
                Dim source As String = Nothing
                l = source.ToTokens()
                Assert.AreEqual(1, l.Count)
                Assert.AreEqual(TokenType.EndOfText, l(0).type)
                l = "".ToTokens()
                Assert.AreEqual(1, l.Count)
                Assert.AreEqual(TokenType.EndOfText, l(0).type)
            End Sub

            <TestMethod>
            Sub TestToTokens1()
                Dim l As New List(Of Token)
                l = "ADD R1,R2,R3".ToTokens()
                Assert.AreEqual(7, l.Count) ' 7 because of the end of text token
                Assert.AreEqual(TokenType.Identifier, l(0).type)
                Assert.AreEqual("ADD", l(0).id)
                Assert.AreEqual(TokenType.Register, l(1).type)
                Assert.AreEqual(1, l(1).r)
                Assert.AreEqual(TokenType.Symbol, l(2).type)
                Assert.AreEqual(",", l(2).sym)
                Assert.AreEqual(TokenType.Register, l(3).type)
                Assert.AreEqual(2, l(3).r)
                Assert.AreEqual(TokenType.Symbol, l(4).type)
                Assert.AreEqual(",", l(4).sym)
                Assert.AreEqual(TokenType.Register, l(5).type)
                Assert.AreEqual(3, l(5).r)
                Assert.AreEqual(TokenType.EndOfText, l(6).type)
            End Sub

            <TestMethod>
            Sub TestToTokens2()
                Dim l As New List(Of Token)
                l = "ADD R14,R13,#27".ToTokens()
                Assert.AreEqual(8, l.Count)
                Assert.AreEqual(TokenType.Identifier, l(0).type)
                Assert.AreEqual("ADD", l(0).id)
                Assert.AreEqual(TokenType.Register, l(1).type)
                Assert.AreEqual(14, l(1).r)
                Assert.AreEqual(TokenType.Symbol, l(2).type)
                Assert.AreEqual(",", l(2).sym)
                Assert.AreEqual(TokenType.Register, l(3).type)
                Assert.AreEqual(13, l(3).r)
                Assert.AreEqual(TokenType.Symbol, l(4).type)
                Assert.AreEqual(",", l(4).sym)
                Assert.AreEqual(TokenType.Symbol, l(5).type)
                Assert.AreEqual("#", l(5).sym)
                Assert.AreEqual(TokenType.IntegerLiteral, l(6).type)
                Assert.AreEqual(27, l(6).i)
                Assert.AreEqual(TokenType.EndOfText, l(7).type)
            End Sub

            <TestMethod>
            Sub TestToTokens3()
                Dim l As New List(Of Token)
                l = "goto Start".ToTokens()
                Assert.AreEqual(3, l.Count)
                Assert.AreEqual(TokenType.Keyword, l(0).type)
                Assert.AreEqual(KeywordType.GOTO, l(0).k)
                Assert.AreEqual(TokenType.Identifier, l(1).type)
                Assert.AreEqual("START", l(1).id)
                Assert.AreEqual(TokenType.EndOfText, l(2).type)
            End Sub

            <TestMethod>
            Sub TestToTokens4()
                Dim l As New List(Of Token)
                l = "IF R1=R2 GO ENDofProgram".ToTokens()
                Assert.AreEqual(7, l.Count)
                Assert.AreEqual(TokenType.Keyword, l(0).type)
                Assert.AreEqual(KeywordType.IF, l(0).k)
                Assert.AreEqual(TokenType.Register, l(1).type)
                Assert.AreEqual(1, l(1).r)
                Assert.AreEqual(TokenType.Symbol, l(2).type)
                Assert.AreEqual("=", l(2).sym)
                Assert.AreEqual(TokenType.Register, l(3).type)
                Assert.AreEqual(2, l(3).r)
                Assert.AreEqual(TokenType.Keyword, l(4).type)
                Assert.AreEqual(KeywordType.GOTO, l(4).k)
                Assert.AreEqual(TokenType.Identifier, l(5).type)
                Assert.AreEqual("ENDOFPROGRAM", l(5).id)
                Assert.AreEqual(TokenType.EndOfText, l(6).type)
            End Sub

            <TestMethod>
            Sub TestToTokens8()
                Dim l As New List(Of Token)
                l = "IF R1<>R2 GO Again".ToTokens()
                Assert.AreEqual(7, l.Count)
                Assert.AreEqual(TokenType.Keyword, l(0).type)
                Assert.AreEqual(KeywordType.IF, l(0).k)
                Assert.AreEqual(TokenType.Register, l(1).type)
                Assert.AreEqual(1, l(1).r)
                Assert.AreEqual(TokenType.Symbol, l(2).type)
                Assert.AreEqual("<>", l(2).sym)
                Assert.AreEqual(TokenType.Register, l(3).type)
                Assert.AreEqual(2, l(3).r)
                Assert.AreEqual(TokenType.Keyword, l(4).type)
                Assert.AreEqual(KeywordType.GOTO, l(4).k)
                Assert.AreEqual(TokenType.Identifier, l(5).type)
                Assert.AreEqual("AGAIN", l(5).id)
                Assert.AreEqual(TokenType.EndOfText, l(6).type)
            End Sub

            <TestMethod>
            Sub TestToTokens9()
                Dim l As New List(Of Token)
                l = "IF R1!=R2 GO Again".ToTokens() ' != returned as <>
                Assert.AreEqual(7, l.Count)
                Assert.AreEqual(TokenType.Keyword, l(0).type)
                Assert.AreEqual(KeywordType.IF, l(0).k)
                Assert.AreEqual(TokenType.Register, l(1).type)
                Assert.AreEqual(1, l(1).r)
                Assert.AreEqual(TokenType.Symbol, l(2).type)
                Assert.AreEqual("<>", l(2).sym)
                Assert.AreEqual(TokenType.Register, l(3).type)
                Assert.AreEqual(2, l(3).r)
                Assert.AreEqual(TokenType.Keyword, l(4).type)
                Assert.AreEqual(KeywordType.GOTO, l(4).k)
                Assert.AreEqual(TokenType.Identifier, l(5).type)
                Assert.AreEqual("AGAIN", l(5).id)
                Assert.AreEqual(TokenType.EndOfText, l(6).type)
            End Sub

            <TestMethod>
            Sub TestToTokens10()
                Dim l As New List(Of Token)
                l = "R0 = R2 << 4".ToTokens()
                Assert.AreEqual(6, l.Count)
                Assert.AreEqual(TokenType.Register, l(0).type)
                Assert.AreEqual(0, l(0).r)
                Assert.AreEqual(TokenType.Symbol, l(1).type)
                Assert.AreEqual("=", l(1).sym)
                Assert.AreEqual(TokenType.Register, l(2).type)
                Assert.AreEqual(2, l(2).r)
                Assert.AreEqual(TokenType.Symbol, l(3).type)
                Assert.AreEqual("<<", l(3).sym)
                Assert.AreEqual(TokenType.IntegerLiteral, l(4).type)
                Assert.AreEqual(4, l(4).i)
                Assert.AreEqual(TokenType.EndOfText, l(5).type)
            End Sub

            <TestMethod>
            Sub TestToTokens11()
                Dim l As New List(Of Token)
                l = "R0 = R2 >> 4".ToTokens()
                Assert.AreEqual(6, l.Count)
                Assert.AreEqual(TokenType.Register, l(0).type)
                Assert.AreEqual(0, l(0).r)
                Assert.AreEqual(TokenType.Symbol, l(1).type)
                Assert.AreEqual("=", l(1).sym)
                Assert.AreEqual(TokenType.Register, l(2).type)
                Assert.AreEqual(2, l(2).r)
                Assert.AreEqual(TokenType.Symbol, l(3).type)
                Assert.AreEqual(">>", l(3).sym)
                Assert.AreEqual(TokenType.IntegerLiteral, l(4).type)
                Assert.AreEqual(4, l(4).i)
                Assert.AreEqual(TokenType.EndOfText, l(5).type)
            End Sub

            <TestMethod>
            Sub TestToTokens12()
                Dim l As New List(Of Token)
                l = "IF R1>R2 GO Again".ToTokens()
                Assert.AreEqual(7, l.Count)
                Assert.AreEqual(TokenType.Keyword, l(0).type)
                Assert.AreEqual(KeywordType.IF, l(0).k)
                Assert.AreEqual(TokenType.Register, l(1).type)
                Assert.AreEqual(1, l(1).r)
                Assert.AreEqual(TokenType.Symbol, l(2).type)
                Assert.AreEqual(">", l(2).sym)
                Assert.AreEqual(TokenType.Register, l(3).type)
                Assert.AreEqual(2, l(3).r)
                Assert.AreEqual(TokenType.Keyword, l(4).type)
                Assert.AreEqual(KeywordType.GOTO, l(4).k)
                Assert.AreEqual(TokenType.Identifier, l(5).type)
                Assert.AreEqual("AGAIN", l(5).id)
                Assert.AreEqual(TokenType.EndOfText, l(6).type)
            End Sub

            <TestMethod>
            Sub TestToTokens13()
                Dim l As New List(Of Token)
                l = "IF R1<R2 GO Again".ToTokens()
                Assert.AreEqual(7, l.Count)
                Assert.AreEqual(TokenType.Keyword, l(0).type)
                Assert.AreEqual(KeywordType.IF, l(0).k)
                Assert.AreEqual(TokenType.Register, l(1).type)
                Assert.AreEqual(1, l(1).r)
                Assert.AreEqual(TokenType.Symbol, l(2).type)
                Assert.AreEqual("<", l(2).sym)
                Assert.AreEqual(TokenType.Register, l(3).type)
                Assert.AreEqual(2, l(3).r)
                Assert.AreEqual(TokenType.Keyword, l(4).type)
                Assert.AreEqual(KeywordType.GOTO, l(4).k)
                Assert.AreEqual(TokenType.Identifier, l(5).type)
                Assert.AreEqual("AGAIN", l(5).id)
                Assert.AreEqual(TokenType.EndOfText, l(6).type)
            End Sub

            <TestMethod>
            Sub TestToTokens14()
                Dim l As New List(Of Token)
                l = "R1 += 26".ToTokens()
                Assert.AreEqual(4, l.Count)
                Assert.AreEqual(TokenType.Register, l(0).type)
                Assert.AreEqual(1, l(0).r)
                Assert.AreEqual(TokenType.Symbol, l(1).type)
                Assert.AreEqual("+=", l(1).sym)
                Assert.AreEqual(TokenType.IntegerLiteral, l(2).type)
                Assert.AreEqual(26, l(2).i)
                Assert.AreEqual(TokenType.EndOfText, l(3).type)
            End Sub

            <TestMethod>
            Sub TestToTokens15()
                Dim l As New List(Of Token)
                l = "R1 -= 26".ToTokens()
                Assert.AreEqual(4, l.Count)
                Assert.AreEqual(TokenType.Register, l(0).type)
                Assert.AreEqual(1, l(0).r)
                Assert.AreEqual(TokenType.Symbol, l(1).type)
                Assert.AreEqual("-=", l(1).sym)
                Assert.AreEqual(TokenType.IntegerLiteral, l(2).type)
                Assert.AreEqual(26, l(2).i)
                Assert.AreEqual(TokenType.EndOfText, l(3).type)
            End Sub

            <TestMethod>
            Sub TestToTokens16()
                Dim l As New List(Of Token)
                l = "R10 = r15 - 26".ToTokens()
                Assert.AreEqual(6, l.Count)
                Assert.AreEqual(TokenType.Register, l(0).type)
                Assert.AreEqual(10, l(0).r)
                Assert.AreEqual(TokenType.Symbol, l(1).type)
                Assert.AreEqual("=", l(1).sym)
                Assert.AreEqual(TokenType.Register, l(2).type)
                Assert.AreEqual(15, l(2).r)
                Assert.AreEqual(TokenType.Symbol, l(3).type)
                Assert.AreEqual("-", l(3).sym)
                Assert.AreEqual(TokenType.IntegerLiteral, l(4).type)
                Assert.AreEqual(26, l(4).i)
                Assert.AreEqual(TokenType.EndOfText, l.Last().type)
            End Sub

            <TestMethod>
            Sub TestToTokens17()
                Dim l As New List(Of Token)
                l = "R10 = r15 + 26".ToTokens()
                Assert.AreEqual(6, l.Count)
                Assert.AreEqual(TokenType.Register, l(0).type)
                Assert.AreEqual(10, l(0).r)
                Assert.AreEqual(TokenType.Symbol, l(1).type)
                Assert.AreEqual("=", l(1).sym)
                Assert.AreEqual(TokenType.Register, l(2).type)
                Assert.AreEqual(15, l(2).r)
                Assert.AreEqual(TokenType.Symbol, l(3).type)
                Assert.AreEqual("+", l(3).sym)
                Assert.AreEqual(TokenType.IntegerLiteral, l(4).type)
                Assert.AreEqual(26, l(4).i)
                Assert.AreEqual(TokenType.EndOfText, l.Last().type)
            End Sub
        End Class

    End Class

End Namespace