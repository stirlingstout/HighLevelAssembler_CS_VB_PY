Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_B
        <TestMethod>
        Sub TestB0()
            Dim destination = 100
            Dim b = New BInstruction(destination)
            Dim r = New Registers()
            Assert.AreEqual(0, r.PC)
            b.Execute(r)
            Assert.AreEqual(destination, r.PC)

            b = New BInstruction("START") ' So address is -1
            Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() b.Execute(r))

            destination = 55
            b.PatchAddress("START", destination)
            b.Execute(r)
            Assert.AreEqual(destination, r.PC)
        End Sub
    End Class
End Namespace
