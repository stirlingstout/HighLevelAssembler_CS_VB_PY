Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports System.Data
Imports System.Reflection.Metadata
Imports NuGet.Frameworks
Imports System.IO

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTestIndirectLoadAndStore

        <TestMethod>
        Sub TestLoadIndirect1()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)
            Const badLocation = 2000

            Dim program = New List(Of String)() From {$"R1 = memory[R2]", "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.ThrowsException(Of DataException)(Sub() ExecuteProgram(m, r))
        End Sub

        <TestMethod>
        Sub TestLoadIndirect2()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)
            Const badLocation = 2000

            Dim program = New List(Of String)() From {$"R2 = {badLocation}", "R1 = memory[R2]", "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.ThrowsException(Of DataException)(Sub() ExecuteProgram(m, r))
        End Sub

        <TestMethod>
        Sub TestLoadIndirect3()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)
            Const firstLocation = 1000

            Dim program = New List(Of String)() From {$"R2 = {firstLocation}",
                                                      $"FOR R3 = 1 to 4",
                                                      "R1 = memory[R2]",
                                                      "R0 = R0 + R1",
                                                      "r2 = R2 + 1",
                                                      "END FOR",
                                                      "HALT",
                                                      $"LOcation {firstLocation}",
                                                      "data 1",
                                                      "data 2",
                                                      "data 3",
                                                      "data 4"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(10, r(0))
            Assert.AreEqual(5, r(3))
            Assert.AreEqual(firstLocation + 4, r(2))
        End Sub

        <TestMethod>
        Sub TestLoadAndStoreIndirect4()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = (New StreamReader("BubbleSort.hla")).ReadToEnd().Split(Environment.NewLine).ToList()
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.AreEqual(0, r.PC)
            ExecuteProgram(m, r)

            Assert.IsTrue(r(1) = 0)

            For i = 0 To 8
                Assert.IsTrue(m(101 + i).GetValue() <= m(102 + i).GetValue())
            Next i
        End Sub
    End Class

End Namespace
