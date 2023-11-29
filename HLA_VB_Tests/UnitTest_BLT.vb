Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports NuGet.Frameworks

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_BLT

        <TestMethod>
        Sub TestBLT0()
            Dim destination = 100

            Dim c = New CMPRegisterInstruction(0, 1)
            Dim r = New Registers()
            r(0) = 1
            r(1) = r(0) + 1
            c.Execute(r)
            Assert.IsTrue(r.LT)

            Dim b = New BLTInstruction(destination)
            b.Execute(r)
            Assert.AreEqual(destination, r.PC)
        End Sub

        <TestMethod>
        Sub TestBLT1()
            Dim destination = 100

            Dim c = New CMPRegisterInstruction(0, 1)
            Dim r = New Registers()
            Assert.AreEqual(0, r.PC)
            r(0) = 1
            r(1) = r(0)
            c.Execute(r)
            Assert.IsFalse(r.LT)

            Dim b = New BLTInstruction(destination)
            b.Execute(r)
            Assert.AreEqual(0, r.PC)
        End Sub

    End Class
End Namespace
