Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports System

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_ADD
        <TestMethod>
        Sub TestAdd0()
            Dim l = New ADDRegisterInstruction(0, 1, 2)
            Dim r = New Registers()
            r(0) = 99
            r(1) = 55
            r(2) = 105
            l.Execute(r)
            Assert.AreEqual(160, r(0))
            Assert.AreEqual(55, r(1))
            Assert.AreEqual(105, r(2))

            Const invalidRegister = 55
            Try
                Dim a = New ADDRegisterInstruction(invalidRegister, 0, 1)
                Assert.Fail($"Managed to create an AddRegisterInstruction with an invalid Rd = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New ADDRegisterInstruction(0, invalidRegister, 1)
                Assert.Fail($"Managed to create an AddRegisterInstruction with an invalid Rn = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New ADDRegisterInstruction(0, 1, invalidRegister)
                Assert.Fail($"Managed to create an AddRegisterInstruction with an invalid Rm = {invalidRegister}")
            Catch ex As Exception

            End Try
        End Sub

        <TestMethod>
        Sub TestAdd1()
            Dim l = New ADDImmediateInstruction(0, 1, 2)
            Dim r = New Registers()
            r(0) = 99
            r(1) = 55
            l.Execute(r)
            Assert.AreEqual(57, r(0))
            Assert.AreEqual(55, r(1))

            Const invalidRegister = 55
            Try
                Dim a = New ADDImmediateInstruction(invalidRegister, 0, 1)
                Assert.Fail($"Managed to create an AddRegisterInstruction with an invalid Rd = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New ADDImmediateInstruction(0, invalidRegister, 1)
                Assert.Fail($"Managed to create an AddImmediateInstruction with an invalid Rn = {invalidRegister}")
            Catch ex As Exception

            End Try

        End Sub
    End Class
End Namespace
