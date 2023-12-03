Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Interfaces

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_ScannerMatching

        <TestClass>
        Public Class TestScannerMatching

            <TestMethod>
            Sub TestTokenMatch0()
                Dim t1 = New Token(0)
                Dim t2 = New Token(5)
                Assert.IsTrue(t1.Matches(t2))
                Assert.IsTrue(t2.Matches(t1))
            End Sub

            <TestMethod>
            Sub TestTokenMatch1()
                Dim t1 = New Token(5)
                Dim t2 = New Token(5)
                Assert.IsTrue(t1.Matches(t2))
                Assert.IsTrue(t2.Matches(t1))
            End Sub

            <TestMethod>
            Sub TestTokenMatch2()
                Dim t1 = New Token(KeywordType.GOTO)
                Dim t2 = New Token(KeywordType.GOTO)
                Assert.IsTrue(t1.Matches(t2))
                Assert.IsTrue(t2.Matches(t1))
            End Sub

            <TestMethod>
            Sub TestTokenMatch3()
                Dim t1 = New Token(KeywordType.GOTO)
                Dim t2 = New Token(KeywordType.IF)
                Assert.IsFalse(t1.Matches(t2))
                Assert.IsFalse(t2.Matches(t1))
            End Sub

            <TestMethod>
            Sub TestTokenMatch4()
                Dim t1 = New Token(CType(5, Byte))
                Dim t2 = New Token(CType(0, Byte))
                Assert.IsTrue(t1.Matches(t2))
                Assert.IsTrue(t2.Matches(t1))
            End Sub

            <TestMethod>
            Sub TestTokenMatch5()
                Dim t1 = New Token(CType(5, Byte))
                Dim t2 = New Token(CType(5, Byte))
                Assert.IsTrue(t1.Matches(t2))
                Assert.IsTrue(t2.Matches(t1))
            End Sub

            <TestMethod>
            Sub TestTokenMatch6()
                Dim t1 = New Token("=", TokenType.Symbol)
                Dim t2 = New Token("=", TokenType.Symbol)
                Assert.IsTrue(t1.Matches(t2))
                Assert.IsTrue(t2.Matches(t1))
            End Sub

            <TestMethod>
            Sub TestTokenMatch7()
                Dim t1 = New Token("=", TokenType.Symbol)
                Dim t2 = New Token("<>", TokenType.Symbol)
                Assert.IsFalse(t1.Matches(t2))
                Assert.IsFalse(t2.Matches(t1))
            End Sub

        End Class

        <TestClass>
        Public Class TestTokenListMatching
            <TestMethod>
            Sub TestTokenListMatch0()
                Dim l1 = "R1 = R2 + R3".ToTokens()
                Assert.IsTrue(l1.Matches(l1))
            End Sub

            <TestMethod>
            Sub TestTokenListMatch1()
                Dim l1 = "R1 = R2 + R3".ToTokens()
                Dim l2 = "R1 = R2 + R3".ToTokens()
                Assert.IsTrue(l1.Matches(l2))
                Assert.IsTrue(l2.Matches(l1))
            End Sub

            <TestMethod>
            Sub TestTokenListMatch2()
                Dim l1 = "R1 = R2 + R3".ToTokens()
                Dim l2 = "R0 = R2 + R3".ToTokens()
                Assert.IsTrue(l1.Matches(l2))
                Assert.IsTrue(l2.Matches(l1))
                l2 = "R1 = R0 + R3".ToTokens()
                Assert.IsTrue(l1.Matches(l2))
                Assert.IsTrue(l2.Matches(l1))
                l2 = "R1 = r2 + R9".ToTokens()
                Assert.IsTrue(l1.Matches(l2))
                Assert.IsTrue(l2.Matches(l1))
            End Sub

            <TestMethod>
            Sub TestTokenListMatch3()
                Dim l1 = "R1 = R2 + 45".ToTokens()
                Dim l2 = "R0 = R2 + 45".ToTokens()
                Assert.IsTrue(l1.Matches(l2))
                Assert.IsTrue(l2.Matches(l1))
                l2 = "R1 = R0 + 19".ToTokens()
                Assert.IsTrue(l1.Matches(l2))
                Assert.IsTrue(l2.Matches(l1))
            End Sub

            <TestMethod>
            Sub TestTokenListMatch4()
                Dim l1 = "IF R1 = R3 GOTO Start".ToTokens()
                Dim l2 = "IF R1 = R3 GOTO Start".ToTokens()
                Assert.IsTrue(l1.Matches(l2))
                Assert.IsTrue(l2.Matches(l1))
                l2 = "IF R1 = 25 GOTO Last".ToTokens()
                Assert.IsFalse(l1.Matches(l2))
                Assert.IsFalse(l2.Matches(l1))
                l2 = "IF R9 = r0 GOTO last".ToTokens()
                Assert.IsTrue(l1.Matches(l2))
                Assert.IsTrue(l2.Matches(l1))
                l2 = "IF R9 = r0 GOTO end".ToTokens()
                Assert.IsFalse(l1.Matches(l2)) ' since end is a keyword!
                Assert.IsFalse(l2.Matches(l1))
            End Sub

            <TestMethod>
            Sub TestTokenListMatch5()
                Dim l1 = "IF R1 = R3 GOTO Start".ToTokens()
                Dim l2 = "GOTO Start".ToTokens()
                Assert.IsFalse(l1.Matches(l2))
                Assert.IsFalse(l2.Matches(l1))
            End Sub

        End Class

    End Class

End Namespace