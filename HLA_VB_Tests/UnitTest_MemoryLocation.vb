Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports System.Data
Imports System.Reflection.Metadata

Namespace HLA_VB_Tests

    Public Class TestCommon
        Protected Const TotalWidth As Integer = 11
    End Class

    <TestClass>
    Public Class UnitTestMemoryLocation
        Inherits TestCommon

        <TestMethod>
        Sub TestMemoryLocation0()
            Dim l = New MemoryLocation()
            Assert.AreEqual(" ".PadRight(UnitTestMemoryLocation.TotalWidth), l.ToString())
        End Sub

        <TestMethod>
        Sub TestMemoryLocation1()
            Dim l = New MemoryLocation()
            l.AddLabel("Hello")
            Assert.AreEqual("Hello:".PadRight(UnitTestMemoryLocation.TotalWidth), l.ToString())
        End Sub

        <TestMethod>
        Sub TestMemoryLocation2()
            Dim l = New MemoryLocation()
            l.AddLabel("Hello")
            l.AddLabel("Goodbye")
            Assert.AreEqual(("Hello:".PadRight(TotalWidth - 1) + Environment.NewLine + "     Goodbye:   ".PadRight(TotalWidth)), l.ToString())
        End Sub

        <TestMethod>
        Sub TestMemoryLocation3()
            Dim l = New MemoryLocation()
            l.AddLabel("Hello")
            l.AddLabel("Goodbye")
            Assert.IsTrue(l.HasLabel("Hello"))
            Assert.IsTrue(l.HasLabel("Goodbye"))
            Assert.IsFalse(l.HasLabel(""))
            Assert.IsFalse(l.HasLabel("HELLO"))
            Assert.IsFalse(l.HasLabel("GJHFDS"))
        End Sub
    End Class


End Namespace

