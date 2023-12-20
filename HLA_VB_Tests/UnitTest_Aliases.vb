Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB

Namespace HLA_VB_Tests

    <TestClass>
    Public Class UnitTest_Aliases

        <TestMethod>
        Sub TestAliasDefinition0()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"ALIAS A = R5",
                                                  "alias N = 5000",
                                                  "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.AreEqual(2, Scanner.Scanner.Aliases.Count)
            Assert.AreEqual(TokenType.Register, Scanner.Scanner.Aliases("A").type)
            Assert.AreEqual(5, Scanner.Scanner.Aliases("A").r)
            Assert.AreEqual(TokenType.IntegerLiteral, Scanner.Scanner.Aliases("N").type)
            Assert.AreEqual(5000, Scanner.Scanner.Aliases("N").i)
        End Sub

        <TestMethod>
        Sub TestAliasEvaluation0()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"ALIAS A = R5",
                                                  "alias N = 5000",
                                                  "A = n",
                                                  "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.AreEqual(2, Scanner.Scanner.Aliases.Count)
            Assert.AreEqual(TokenType.Register, Scanner.Scanner.Aliases("A").type)
            Assert.AreEqual(5, Scanner.Scanner.Aliases("A").r)
            Assert.AreEqual(TokenType.IntegerLiteral, Scanner.Scanner.Aliases("N").type)
            Assert.AreEqual(5000, Scanner.Scanner.Aliases("N").i)

            ExecuteProgram(m, r)
            Assert.AreEqual(5000, r(5))
        End Sub

        <TestMethod>
        Sub TestAliasEvaluation1()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"ALIAS A = R5",
                                                  "alias N = 5000",
                                                  "A = n",
                                                  "alias shl = <<",
                                                  "a = a shl 2",
                                                  "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.AreEqual(3, Scanner.Scanner.Aliases.Count)
            Assert.AreEqual(TokenType.Register, Scanner.Scanner.Aliases("A").type)
            Assert.AreEqual(5, Scanner.Scanner.Aliases("A").r)
            Assert.AreEqual(TokenType.IntegerLiteral, Scanner.Scanner.Aliases("N").type)
            Assert.AreEqual(5000, Scanner.Scanner.Aliases("N").i)
            Assert.AreEqual(TokenType.Symbol, Scanner.Scanner.Aliases("SHL").type)
            Assert.AreEqual("<<", Scanner.Scanner.Aliases("SHL").sym)

            ExecuteProgram(m, r)
            Assert.AreEqual(20000, r(5))
        End Sub

        <TestMethod>
        Sub TestAliasEvaluation2()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"ALIAS A = B",
                                                  "alias B = A",
                                                  "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.AreEqual(2, Scanner.Scanner.Aliases.Count)
        End Sub
    End Class

End Namespace