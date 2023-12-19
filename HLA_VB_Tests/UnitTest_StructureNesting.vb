Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports System.Data
Imports System.Reflection.Metadata

Namespace HLA_VB_Tests

    <TestClass>
    Public Class UnitTest_Nesting

        <TestMethod>
        Sub TestIncorrectREPEATNesting0()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0",
                                                      "UNTIL r1 > 0",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(1, errors.Count)
            Assert.IsTrue(errors(0).StartsWith("REPEAT UNTIL incorrectly nested (or UNTIL without REPEAT)"))
        End Sub

        <TestMethod>
        Sub TestIncorrectREPEATNesting1()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0",
                                                      "WHILE R1 > 0",
                                                      "UNTIL r1 > 0",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.IsTrue(errors.Any())
            Assert.IsTrue(errors(0).StartsWith("REPEAT UNTIL incorrectly nested (or UNTIL without REPEAT)"))
        End Sub

        <TestMethod>
        Sub TestIncorrectWHILENesting0()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0",
                                                      "END WHILE",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.IsTrue(errors.Any())
            Assert.IsTrue(errors(0).StartsWith("WHILE END WHILE incorrectly nested (or END WHILE without WHILE)"))
        End Sub

        <TestMethod>
        Sub TestIncorrectWHILENesting1()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0",
                                                      "REPEAT",
                                                      "END WHILE",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.IsTrue(errors.Any())
            Assert.IsTrue(errors(0).StartsWith("WHILE END WHILE incorrectly nested (or END WHILE without WHILE)"))
        End Sub

        <TestMethod>
        Sub TestIncorrectFORNesting0()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0",
                                                      "END FOR",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.IsTrue(errors.Any())
            Assert.IsTrue(errors(0).StartsWith("FOR END FOR incorrectly nested (or END FOR without FOR)"))
        End Sub

        <TestMethod>
        Sub TestIncorrectFORNesting1()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0",
                                                      "WHILE R1 > 0",
                                                      "END FOR",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.IsTrue(errors.Any())
            Assert.IsTrue(errors(0).StartsWith("FOR END FOR incorrectly nested (or END FOR without FOR)"))
        End Sub

        <TestMethod>
        Sub TestIncorrectIFNesting0()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0",
                                                      "END IF",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.IsTrue(errors.Any())
            Assert.IsTrue(errors(0).StartsWith("END IF without a corresponding IF"))
        End Sub

        <TestMethod>
        Sub TestIncorrectIFNesting1()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0",
                                                      "IF R1 = 0 THEN",
                                                      "ELSE IF R1 <> 0 THEN",
                                                      "REPEAT",
                                                      "END IF",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.IsTrue(errors.Any())
            Assert.IsTrue(errors(0).StartsWith("END IF without a corresponding IF"))
        End Sub

        <TestMethod>
        Sub TestIncorrectIFNesting2()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0",
                                                      "IF R1 = 0 THEN",
                                                      "ELSE",
                                                      "REPEAT",
                                                      "END IF",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.IsTrue(errors.Any())
            Assert.IsTrue(errors(0).StartsWith("END IF without a corresponding IF"))
        End Sub
    End Class


End Namespace

