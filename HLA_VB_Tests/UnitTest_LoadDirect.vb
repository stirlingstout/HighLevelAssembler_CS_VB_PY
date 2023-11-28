Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports System.Data
Imports System.Reflection.Metadata
Imports NuGet.Frameworks

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTestLoadDirect
        <TestMethod>
        Sub TestLoadDirect0()
            Dim l = New LoadInstructionDirect(0, 100)
            Dim r = New Registers()
            Dim m = New Memory()
            Dim d = New Data(27)
            m(100) = d
            Assert.IsTrue(TypeOf m(100) Is Data)
            Assert.AreEqual(27, m(100).GetValue())
            Dim params() As Object = {r, m}
            Try
                d.Execute(r, m)
                Assert.Fail("Managed to execute data")
            Catch ex As Exception

            End Try
            ' Used Try Catch because couldn't get Assert.ThrowsException(Of AssertFailedException)(AddressOf d.Execute, "Managed to execute data", r, m) to compile
        End Sub

        <TestMethod>
        Sub TestLoadDirect1()
            Dim l = New LoadInstructionDirect(0, 100)
            Dim r = New Registers()
            Dim m = New Memory()
            Dim d = New Data(98)
            m(100) = d
            l.Execute(r, m)
            Assert.AreEqual(98, r(0))

            Try
                l = New LoadInstructionDirect(16, 100)
                Assert.Fail($"Managed to create a load instruction for an invalid register")
            Catch ex As Exception

            End Try

            Try
                l = New LoadInstructionDirect(5, HIGHEST_MEMORY_ADDRESS + 1)
                Assert.Fail($"Managed to create a load instruction for an invalid memory location")
            Catch ex As Exception

            End Try
        End Sub


    End Class


End Namespace