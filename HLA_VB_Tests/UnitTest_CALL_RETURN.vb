Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports System.Data
Imports System.Reflection.Metadata
Imports NuGet.Frameworks
Imports System.IO

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTestCALL_RETURN

        <TestMethod>
        Sub TestCALLRETURN1()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {$"CALL p", "HALT", "PROCEDURE P", "R1 = 456", "END PROCEDURE"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)

            Assert.AreEqual(456, r(1))
        End Sub

        <TestMethod>
        Sub TestCALLRETURN2()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"CALL p", "HALT",
                                                      "PROCEDURE P",
                                                      "R1 = 456",
                                                      "Call Q",
                                                      "END PROCEDURE",
                                                      "procedure Q",
                                                      "R2 = R1 + 1",
                                                      "end procedure"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)

            Assert.AreEqual(456, r(1))
            Assert.AreEqual(457, r(2))
        End Sub
    End Class

End Namespace
