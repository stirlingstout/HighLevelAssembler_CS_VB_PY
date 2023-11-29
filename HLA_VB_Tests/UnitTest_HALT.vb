Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_HALT
        <TestMethod>
        Sub TestHALT0()
            Dim h = New HALTInstruction()
            Dim r = New Registers()
            Assert.IsFalse(r.Halted)
            h.Execute(r)
            Assert.IsTrue(r.Halted)
        End Sub
    End Class
End Namespace