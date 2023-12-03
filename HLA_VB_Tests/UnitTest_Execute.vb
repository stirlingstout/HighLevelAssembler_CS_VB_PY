Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports NuGet.Frameworks
Imports System.Data

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_Execute
        <TestMethod>
        Sub TestExecute0()
            Dim m As New Memory()
            Dim r As New Registers()

            Const dataValue = 25
            m(0) = New LoadInstructionDirect(0, 100)
            m(100) = New Data(dataValue)
            m(1) = New HALTInstruction()

            ExecuteProgram(m, r)
            Assert.AreEqual(dataValue, r(0))
        End Sub

        <TestMethod>
        Sub TestExecute1()
            Dim m As New Memory()
            Dim r As New Registers()

            Const dataValue = 25
            Const increment = 45
            m(0) = New LoadInstructionDirect(0, 100)
            m(1) = New ADDImmediateInstruction(1, 0, increment)
            m(100) = New Data(dataValue)
            m(2) = New HALTInstruction()

            ExecuteProgram(m, r)
            Assert.AreEqual(dataValue + increment, r(1))
        End Sub

        <TestMethod>
        Sub TestExecute2()
            Dim m As New Memory()
            Dim r As New Registers()

            Const dataValue = 25
            Const increment = 45
            m(0) = New LoadInstructionDirect(0, 100)
            m(1) = New ADDImmediateInstruction(1, 0, increment)
            m(100) = New Data(dataValue)


            Assert.ThrowsException(Of DataException)(Sub() ExecuteProgram(m, r))
            ' This is how you can get Assert.ThrowsException to work! Thanks to
            ' https://groups.google.com/g/nunit-discuss/c/STiMNTVxoPE
        End Sub
    End Class
End Namespace
