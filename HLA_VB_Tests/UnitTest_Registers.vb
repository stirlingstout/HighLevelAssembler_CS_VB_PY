Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports System.Data
Imports System.Reflection.Metadata
Imports NuGet.Frameworks
Imports System.Diagnostics

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTestRegisters

        <TestMethod>
        Sub TestRegisterPC0()
            Dim r = New Registers()
            Assert.AreEqual(0, r.PC)
            r.PC = 10
            Assert.AreEqual(10, r.PC)
            Try
                r.PC = -1
                Assert.Fail("Managed to set PC to an invalid value")
            Catch ex As Exception

            End Try
            Try
                r.PC = HIGHEST_MEMORY_ADDRESS + 1
                Assert.Fail("Managed to set PC to an invalid value")
            Catch ex As Exception

            End Try
        End Sub

        <TestMethod>
        Sub TestStatusRegister0()
            Dim r = New Registers()
            Assert.AreEqual(0, r.PC)
            Assert.IsFalse(r.EQ)
            Assert.IsTrue(r.NE)
            Assert.IsFalse(r.GT)
            Assert.IsTrue(r.LT) ' This isn't strictly correct. The initial state of flags is 'odd'
            r.SetFlags(0)
            Assert.IsTrue(r.EQ)
            Assert.IsFalse(r.NE)
            Assert.IsFalse(r.GT)
            Assert.IsFalse(r.LT)
            r.SetFlags(25 - 24)
            Assert.IsFalse(r.EQ)
            Assert.IsTrue(r.NE)
            Assert.IsTrue(r.GT)
            Assert.IsFalse(r.LT)
        End Sub

        <TestMethod>
        Sub TestGeneralPurposeRegister0()
            Dim r = New Registers()
            r(0) = 100
            Assert.AreEqual(100, r(0))
            r(15) = -1
            Assert.AreEqual(-1, r(15))
            Try
                r(16) = 326
                Assert.Fail($"Managed to set an invalid register, r(16)")
            Catch ex As Exception

            End Try
            Try
                Dim f = r(-1)
                Assert.Fail($"Managed to read an invalid register, r(-1)")
            Catch ex As Exception

            End Try
        End Sub
    End Class


End Namespace