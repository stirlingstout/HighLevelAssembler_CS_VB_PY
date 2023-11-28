Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_SUB
        <TestMethod>
        Sub TestSUB0()
            Dim l = New SUBRegisterInstruction(0, 1, 2)
            Dim r = New Registers()
            r(0) = 99
            r(1) = 55
            r(2) = 105
            l.Execute(r)
            Assert.AreEqual(-50, r(0))
            Assert.AreEqual(55, r(1))
            Assert.AreEqual(105, r(2))

            Const invalidRegister = 55
            Try
                Dim a = New SUBRegisterInstruction(invalidRegister, 0, 1)
                Assert.Fail($"Managed to create a SUBRegisterInstruction with an invalid Rd = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New SUBRegisterInstruction(0, invalidRegister, 1)
                Assert.Fail($"Managed to create a SUBRegisterInstruction with an invalid Rn = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New SUBRegisterInstruction(0, 1, invalidRegister)
                Assert.Fail($"Managed to create a SUBRegisterInstruction with an invalid Rm = {invalidRegister}")
            Catch ex As Exception

            End Try
        End Sub

        <TestMethod>
        Sub TestSUB1()
            Dim l = New SUBImmediateInstruction(0, 1, 2)
            Dim r = New Registers()
            r(0) = 99
            r(1) = 55
            l.Execute(r)
            Assert.AreEqual(53, r(0))
            Assert.AreEqual(55, r(1))

            Const invalidRegister = 55
            Try
                Dim a = New SUBImmediateInstruction(invalidRegister, 0, 1)
                Assert.Fail($"Managed to create an SUBRegisterInstruction with an invalid Rd = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New SUBImmediateInstruction(0, invalidRegister, 1)
                Assert.Fail($"Managed to create an SUBImmediateInstruction with an invalid Rn = {invalidRegister}")
            Catch ex As Exception

            End Try

        End Sub
    End Class
End Namespace
