Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports NuGet.Frameworks

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_CMP
        <TestMethod>
        Sub TestCMP0()
            Dim c = New CMPRegisterInstruction(0, 1)
            Dim r = New Registers()
            Assert.IsFalse(r.EQ)
            Assert.IsTrue(r.NE)
            r(0) = 0
            r(1) = r(0)
            c.Execute(r)
            Assert.IsTrue(r.EQ)
            Assert.IsFalse(r.NE)
            Assert.IsFalse(r.LT)
            Assert.IsFalse(r.GT)

            Assert.AreEqual(0, r(1))
            Assert.AreEqual(0, r(0))

            r(0) = 100
            r(1) = 150
            c.Execute(r)
            Assert.IsFalse(r.EQ)
            Assert.IsTrue(r.NE)
            Assert.IsTrue(r.LT)
            Assert.IsFalse(r.GT)

            Assert.ThrowsException(Of IndexOutOfRangeException)(Function() New CMPRegisterInstruction(55, 0))
        End Sub

        <TestMethod>
        Sub TestCMP0_1()
            Assert.ThrowsException(Of IndexOutOfRangeException)(Function() New CMPRegisterInstruction(0, 55))
        End Sub

        <TestMethod>
        Sub TestCMP1()
            Dim c = New CMPImmediateInstruction(0, -1)
            Dim r = New Registers()
            r(0) = 0
            r(1) = 1
            c.Execute(r)
            Assert.IsTrue(r.NE)
            Assert.IsFalse(r.EQ)
            Assert.IsFalse(r.LT)
            Assert.IsTrue(r.GT)

            Assert.AreEqual(1, r(1))
            Assert.AreEqual(0, r(0))

            Assert.ThrowsException(Of IndexOutOfRangeException)(Function() New CMPImmediateInstruction(55, 0))
        End Sub
    End Class
End Namespace