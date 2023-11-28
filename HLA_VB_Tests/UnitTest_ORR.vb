Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_ORR
        <TestMethod>
        Sub TestORR0()
            Dim l = New ORRRegisterInstruction(0, 1, 2)
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
            Assert.AreEqual(240, r(2))

            Const invalidRegister = 55
            Try
                Dim a = New ORRRegisterInstruction(invalidRegister, 0, 1)
                Assert.Fail($"Managed to create a ORRRegisterInstruction with an invalid Rd = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New ORRRegisterInstruction(0, invalidRegister, 1)
                Assert.Fail($"Managed to create a ORRRegisterInstruction with an invalid Rn = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New ORRRegisterInstruction(0, 1, invalidRegister)
                Assert.Fail($"Managed to create a ORRRegisterInstruction with an invalid Rm = {invalidRegister}")
            Catch ex As Exception

            End Try
        End Sub

        <TestMethod>
        Sub TestORR1()
            Dim l = New ORRImmediateInstruction(0, 1, 9)
            Dim r = New Registers()
            r(0) = 99
            r(1) = (15 << 4)
            ' Who knew that << (indeed all bitwise operators) had a lower precedence than +?
            ' See https://learn.microsoft.com/en-us/dotnet/visual-basic/language-reference/operators/operator-precedence
            l.Execute(r)
            Assert.AreEqual((15 << 4) + 9, r(0))
            Assert.AreEqual((15 << 4), r(1))

            l = New ORRImmediateInstruction(0, 1, 15 << 4)
            r(0) = 0
            r(1) = 9
            l.Execute(r)
            Assert.AreEqual((15 << 4) + 9, r(0))
            Assert.AreEqual(9, r(1))

            Const invalidRegister = 55
            Try
                Dim a = New ORRImmediateInstruction(invalidRegister, 0, 1)
                Assert.Fail($"Managed to create an ORRRegisterInstruction with an invalid Rd = {invalidRegister}")
            Catch ex As Exception

            End Try

            Try
                Dim a = New ORRImmediateInstruction(0, invalidRegister, 1)
                Assert.Fail($"Managed to create an ORRImmediateInstruction with an invalid Rn = {invalidRegister}")
            Catch ex As Exception

            End Try

        End Sub
    End Class
End Namespace
