Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_AND
        <TestMethod>
        Sub TestAND0()
            Dim l = New ANDRegisterInstruction(0, 1, 2)
            Dim r = New Registers()
            r(0) = 99
            r(1) = -1
            r(2) = 255
            l.Execute(r)
            Assert.AreEqual(255, r(0))
            Assert.AreEqual(-1, r(1))
            Assert.AreEqual(255, r(2))

            r(0) = 99
            r(1) = 245
            r(2) = 15 << 4
            l.Execute(r)
            Assert.AreEqual(240, r(0))
            Assert.AreEqual(245, r(1))
            Assert.AreEqual(240, r(2))

            Const invalidRegister = 55
            Assert.ThrowsException(Of IndexOutOfRangeException)(Function() New ANDRegisterInstruction(invalidRegister, 0, 1))
            Assert.ThrowsException(Of IndexOutOfRangeException)(Function() New ANDRegisterInstruction(0, invalidRegister, 1))
            Assert.ThrowsException(Of IndexOutOfRangeException)(Function() New ANDRegisterInstruction(0, 1, invalidRegister))

        End Sub

        <TestMethod>
        Sub TestAND1()
            Dim l = New ANDImmediateInstruction(0, 1, 15)
            Dim r = New Registers()
            r(0) = 99
            r(1) = (15 << 4) + 9
            ' Who knew that << (indeed all bitwise operators) had a lower precedence than +?
            ' See https://learn.microsoft.com/en-us/dotnet/visual-basic/language-reference/operators/operator-precedence
            l.Execute(r)
            Assert.AreEqual(9, r(0))
            Assert.AreEqual(249, r(1))

            l = New ANDImmediateInstruction(0, 1, 15 << 4)
            r(0) = 99
            r(1) = (15 << 4) + 9
            l.Execute(r)
            Assert.AreEqual(15 << 4, r(0))
            Assert.AreEqual(249, r(1))

            Const invalidRegister = 55
            Assert.ThrowsException(Of IndexOutOfRangeException)(Function() New ANDImmediateInstruction(invalidRegister, 0, 1))
        End Sub

        <TestMethod>
        Sub TestAnd2()
            Const invalidRegister = 55
            Assert.ThrowsException(Of IndexOutOfRangeException)(Function() New ANDImmediateInstruction(0, invalidRegister, 1))
        End Sub
    End Class
End Namespace
