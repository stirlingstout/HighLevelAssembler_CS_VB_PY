Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_EOR
        <TestMethod>
        Sub TestEOR0()
            Dim l = New EORRegisterInstruction(0, 1, 2)
            Dim r = New Registers()
            r(0) = 99
            r(1) = 0
            r(2) = 255
            l.Execute(r)
            Assert.AreEqual(255, r(0))
            Assert.AreEqual(0, r(1))
            Assert.AreEqual(255, r(2))

            r(0) = 99
            r(1) = 15
            r(2) = 15 << 4
            l.Execute(r)
            Assert.AreEqual(255, r(0))
            Assert.AreEqual(15, r(1))
            Assert.AreEqual(15 << 4, r(2))

            Const invalidRegister = 55
            Try
                Dim a = New EORRegisterInstruction(invalidRegister, 0, 1)
                Assert.Fail($"Managed to create a EORRegisterInstruction with an invalid Rd = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New EORRegisterInstruction(0, invalidRegister, 1)
                Assert.Fail($"Managed to create a EORRegisterInstruction with an invalid Rn = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New EORRegisterInstruction(0, 1, invalidRegister)
                Assert.Fail($"Managed to create a EORRegisterInstruction with an invalid Rm = {invalidRegister}")
            Catch ex As Exception

            End Try
        End Sub

        <TestMethod>
        Sub TestEOR1()
            Dim l = New EORImmediateInstruction(0, 1, 9)
            Dim r = New Registers()
            r(0) = 99
            r(1) = (15 << 4)
            l.Execute(r)
            Assert.AreEqual((15 << 4) Xor 9, r(0))
            Assert.AreEqual((15 << 4), r(1))

            l = New EORImmediateInstruction(0, 1, 15 << 4)
            r(0) = 0
            r(1) = 9
            l.Execute(r)
            Assert.AreEqual((15 << 4) + 9, r(0))
            Assert.AreEqual(9, r(1))

            Const invalidRegister = 55
            Try
                Dim a = New EORImmediateInstruction(invalidRegister, 0, 1)
                Assert.Fail($"Managed to create an EORRegisterInstruction with an invalid Rd = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New EORImmediateInstruction(0, invalidRegister, 1)
                Assert.Fail($"Managed to create an EORImmediateInstruction with an invalid Rn = {invalidRegister}")
            Catch ex As Exception

            End Try

        End Sub
    End Class
End Namespace
