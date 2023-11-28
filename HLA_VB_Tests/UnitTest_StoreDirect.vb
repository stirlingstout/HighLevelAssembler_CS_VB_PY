Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports System.Data
Imports System.Reflection.Metadata

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTestStoreDirect

        <TestMethod>
        Sub TestStoreDirect0()
            Dim l = New StoreInstructionDirect(0, 100)
            Dim r = New Registers()
            Dim m = New Memory()
            Dim d = New Data(98)
            m(100) = d
            Assert.AreEqual(98, m(100).GetValue())
            r(0) = 99
            l.Execute(r, m)
            Assert.AreEqual(99, r(0))
            Assert.AreEqual(99, m(100).GetValue())

            Try
                l = New StoreInstructionDirect(16, 100)
                Assert.Fail($"Managed to create a store instruction for an invalid register")
            Catch ex As Exception

            End Try

            Try
                l = New StoreInstructionDirect(5, HIGHEST_MEMORY_ADDRESS + 1)
                Assert.Fail($"Managed to create a store instruction for an invalid memory location")
            Catch ex As Exception

            End Try
        End Sub

    End Class

End Namespace