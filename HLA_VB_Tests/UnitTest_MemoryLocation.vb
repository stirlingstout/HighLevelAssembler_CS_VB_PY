Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports System.Data

Namespace HLA_VB_Tests

    Public Class TestCommon
        Protected Const TotalWidth As Integer = 11
    End Class

    <TestClass>
    Public Class UnitTestMemoryLocation
        Inherits TestCommon

        <TestMethod>
        Sub TestMemoryLocation0()
            Dim l = New MemoryLocation()
            Assert.AreEqual(" ".PadRight(UnitTestMemoryLocation.TotalWidth), l.ToString())
        End Sub

        <TestMethod>
        Sub TestMemoryLocation1()
            Dim l = New MemoryLocation()
            l.AddLabel("Hello")
            Assert.AreEqual("Hello:".PadRight(UnitTestMemoryLocation.TotalWidth), l.ToString())
        End Sub

        <TestMethod>
        Sub TestMemoryLocation2()
            Dim l = New MemoryLocation()
            l.AddLabel("Hello")
            l.AddLabel("Goodbye")
            Assert.AreEqual(("Hello:".PadRight(TotalWidth - 1) + Environment.NewLine + "Goodbye:".PadRight(TotalWidth)), l.ToString())
        End Sub

        <TestMethod>
        Sub TestMemoryLocation3()
            Dim l = New MemoryLocation()
            l.AddLabel("Hello")
            l.AddLabel("Goodbye")
            Assert.IsTrue(l.HasLabel("Hello"))
            Assert.IsTrue(l.HasLabel("Goodbye"))
            Assert.IsFalse(l.HasLabel(""))
            Assert.IsFalse(l.HasLabel("HELLO"))
            Assert.IsFalse(l.HasLabel("GJHFDS"))
        End Sub
    End Class

    <TestClass>
    Public Class UnitTestData
        Inherits TestCommon

        <TestMethod>
        Sub TestData0()
            Dim d = New Data()
            Assert.AreEqual(0, d.GetValue())
        End Sub

        <TestMethod>
        Sub TestData1()
            Dim d = New Data()
            d.SetValue(27)
            Assert.AreEqual(27, d.GetValue())
        End Sub

        <TestMethod>
        Sub TestData2()
            Dim d = New Data()
            d.SetValue(98)
            Assert.AreEqual($"{" ".PadRight(TotalWidth)}98", d.ToString())
        End Sub

        <TestMethod>
        Sub TestData3()
            Dim d = New Data()
            d.SetValue(-15)
            d.AddLabel("Start")
            Assert.AreEqual($"{"Start:".PadRight(TotalWidth)}-15", d.ToString())
        End Sub
    End Class

    <TestClass>
    Public Class TestScanner
        Inherits TestCommon

        <TestMethod>
        Sub TestScanner0()
            Dim s = New Scanner("")
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.EndOfText, s.type)
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
            Assert.AreEqual(Scanner.TokenType.IntegerLiteral, s.type)
            Assert.AreEqual(123, s.i)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.EndOfText, s.type)
        End Sub

        <TestMethod>
        Sub TestScanner3()
            Dim s = New Scanner("0")
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.IntegerLiteral, s.type)
            Assert.AreEqual(0, s.i)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.EndOfText, s.type)
        End Sub

        <TestMethod>
        Sub TestScanner4()
            Dim s = New Scanner("Start")
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.Identifier, s.type)
            Assert.AreEqual("START", s.id)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.EndOfText, s.type)
        End Sub

        <TestMethod>
        Sub TestScanner4_1()
            Dim s = New Scanner("R1 R")
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.Register, s.type)
            Assert.AreEqual(1, s.r)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.Identifier, s.type)
            Assert.AreEqual("R", s.id)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.EndOfText, s.type)
        End Sub

        <TestMethod>
        Sub TestScanner5()
            Dim s = New Scanner("123Start")
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.IntegerLiteral)
            Assert.AreEqual(123, s.i)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.Identifier, s.type)
            Assert.AreEqual("START", s.id)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.EndOfText, s.type)
        End Sub

        <TestMethod>
        Sub TestScanner6()
            Dim s = New Scanner("   123Start   ")
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.IntegerLiteral)
            Assert.AreEqual(123, s.i)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.Identifier, s.type)
            Assert.AreEqual("START", s.id)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.EndOfText, s.type)
        End Sub

        <TestMethod>
        Sub TestScanner7()
            Dim s = New Scanner("   123Start  END ")
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.IntegerLiteral)
            Assert.AreEqual(123, s.i)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.Identifier, s.type)
            Assert.AreEqual("START", s.id)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.Keyword, s.type)
            Assert.AreEqual(Scanner.KeywordType.END, s.k)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.EndOfText, s.type)
        End Sub

        <TestMethod>
        Sub TestScanner8() ' single characters
            Dim s = New Scanner("   ,#[]()")
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual(",", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual("#", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual("[", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual("]", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual("(", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual(")", s.sym)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.EndOfText, s.type)
        End Sub

        <TestMethod>
        Sub TestScanner9() ' operators and compound operators
            Dim s = New Scanner("   + - += -= != < << > >> <> ")
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual("+", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual("-", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual("+=", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual("-=", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual("<>", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual("<", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual("<<", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual(">", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual(">>", s.sym)
            s.NextToken()
            Assert.AreEqual(s.type, Scanner.TokenType.Symbol)
            Assert.AreEqual("<>", s.sym)
            s.NextToken()
            Assert.AreEqual(Scanner.TokenType.EndOfText, s.type)
        End Sub

    End Class


End Namespace

