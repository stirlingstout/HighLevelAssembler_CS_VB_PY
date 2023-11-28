Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_LSL
        <TestMethod>
        Sub TestLSL0()
            Dim l = New LSLRegisterInstruction(10, 11, 12)
            Dim r = New Registers()
            r(10) = 99
            r(11) = 45
            r(12) = 2
            l.Execute(r)
            Assert.AreEqual(180, r(10))
            Assert.AreEqual(45, r(11))
            Assert.AreEqual(2, r(12))

            r(10) = 99
            r(11) = 45
            r(12) = 32
            l.Execute(r)
            Assert.AreEqual(45, r(10))
            Assert.AreEqual(45, r(11))
            Assert.AreEqual(32, r(12))

            Const invalidRegister = 55
            Try
                Dim a = New LSLRegisterInstruction(invalidRegister, 0, 1)
                Assert.Fail($"Managed to create a LSLRegisterInstruction with an invalid Rd = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New LSLRegisterInstruction(0, invalidRegister, 1)
                Assert.Fail($"Managed to create a LSLRegisterInstruction with an invalid Rn = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New LSLRegisterInstruction(0, 1, invalidRegister)
                Assert.Fail($"Managed to create a LSLRegisterInstruction with an invalid Rm = {invalidRegister}")
            Catch ex As Exception

            End Try
        End Sub

        <TestMethod>
        Sub TestLSL1()
            Dim l = New LSLImmediateInstruction(0, 1, 9)
            Dim r = New Registers()
            r(0) = 99
            r(1) = 15
            l.Execute(r)
            Assert.AreEqual(15 << 9, r(0))
            Assert.AreEqual(15, r(1))

            l = New LSLImmediateInstruction(0, 1, 35)
            r(0) = 0
            r(1) = 2
            l.Execute(r)
            Assert.AreEqual(16, r(0))
            Assert.AreEqual(2, r(1))

            Const invalidRegister = 55
            Try
                Dim a = New LSLImmediateInstruction(invalidRegister, 0, 1)
                Assert.Fail($"Managed to create an LSLRegisterInstruction with an invalid Rd = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New LSLImmediateInstruction(0, invalidRegister, 1)
                Assert.Fail($"Managed to create an LSLImmediateInstruction with an invalid Rn = {invalidRegister}")
            Catch ex As Exception

            End Try

        End Sub
    End Class
End Namespace
