Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_LSR
        <TestMethod>
        Sub TestLSR0()
            Dim l = New LSRRegisterInstruction(10, 11, 12)
            Dim r = New Registers()
            r(10) = 99
            r(11) = 64
            r(12) = 2
            l.Execute(r)
            Assert.AreEqual(16, r(10))
            Assert.AreEqual(64, r(11))
            Assert.AreEqual(2, r(12))

            r(10) = 99
            r(11) = 45
            r(12) = 33
            l.Execute(r)
            Assert.AreEqual(22, r(10))
            Assert.AreEqual(45, r(11))
            Assert.AreEqual(33, r(12))

            Const invalidRegister = 55
            Try
                Dim a = New LSRRegisterInstruction(invalidRegister, 0, 1)
                Assert.Fail($"Managed to create a LSRRegisterInstruction with an invalid Rd = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New LSRRegisterInstruction(0, invalidRegister, 1)
                Assert.Fail($"Managed to create a LSRRegisterInstruction with an invalid Rn = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New LSRRegisterInstruction(0, 1, invalidRegister)
                Assert.Fail($"Managed to create a LSRRegisterInstruction with an invalid Rm = {invalidRegister}")
            Catch ex As Exception

            End Try
        End Sub

        <TestMethod>
        Sub TestLSR1()
            Dim l = New LSRImmediateInstruction(0, 1, 9)
            Dim r = New Registers()
            r(0) = 99
            r(1) = 15
            l.Execute(r)
            Assert.AreEqual(0, r(0))
            Assert.AreEqual(15, r(1))

            l = New LSRImmediateInstruction(0, 1, REGISTER_SIZE - 2)
            r(0) = 0
            r(1) = Integer.MaxValue
            l.Execute(r)
            Assert.AreEqual(1, r(0))
            Assert.AreEqual(Integer.MaxValue, r(1))

            Const invalidRegister = 55
            Try
                Dim a = New LSRImmediateInstruction(invalidRegister, 0, 1)
                Assert.Fail($"Managed to create an LSRRegisterInstruction with an invalid Rd = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New LSRImmediateInstruction(0, invalidRegister, 1)
                Assert.Fail($"Managed to create an LSRImmediateInstruction with an invalid Rn = {invalidRegister}")
            Catch ex As Exception

            End Try

        End Sub
    End Class
End Namespace
