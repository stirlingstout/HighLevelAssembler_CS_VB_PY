Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports NuGet.Frameworks

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_BEQ
        <TestMethod>
        Sub TestBEQ0()
            Dim destination = 100

            Dim c = New CMPRegisterInstruction(0, 1)
            Dim r = New Registers()
            r(0) = 1
            r(1) = r(0)
            c.Execute(r)
            Assert.IsTrue(r.EQ)

            Dim b = New BEQInstruction(destination)
            b.Execute(r)
            Assert.AreEqual(destination, r.PC)
        End Sub

        <TestMethod>
        Sub TestBEQ1()
            Dim destination = 100

            Dim c = New CMPRegisterInstruction(0, 1)
            Dim r = New Registers()
            Assert.AreEqual(0, r.PC)
            r(0) = 1
            r(1) = r(0) + 1
            c.Execute(r)
            Assert.IsTrue(r.NE)

            Dim b = New BEQInstruction(destination)
            b.Execute(r)
            Assert.AreEqual(0, r.PC)
        End Sub

    End Class
End Namespace
