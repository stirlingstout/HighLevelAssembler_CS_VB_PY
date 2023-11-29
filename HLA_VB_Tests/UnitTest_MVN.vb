Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_MVN
        <TestMethod>
        Sub TestMVN0()
            Dim m = New MVNRegisterInstruction(0, 1)
            Dim r = New Registers()
            r(0) = 0
            r(1) = 1
            m.Execute(r)
            Assert.AreEqual(-2, r(0))
            Assert.AreEqual(1, r(1))

            Dim failed = False
            Try
                Dim e = New MVNRegisterInstruction(55, 0)
            Catch ex As Exception
                failed = True
            End Try
            Assert.IsTrue(failed, $"Managed to create an MVN instruction with invalid Rd={55}")

            failed = False
            Try
                Dim e = New MVNRegisterInstruction(0, 55)
            Catch ex As Exception
                failed = True
            End Try
            Assert.IsTrue(failed, $"Managed to create an MVN instruction with invalid Rm={55}")


        End Sub

        <TestMethod>
        Sub TestMVN1()
            Dim m = New MVNImmediateInstruction(0, -2)
            Dim r = New Registers()
            r(0) = 0
            r(1) = 1
            m.Execute(r)
            Assert.AreEqual(1, r(0))
            Assert.AreEqual(1, r(1))

            Dim failed = False
            Try
                Dim e = New MVNImmediateInstruction(55, 0)
            Catch ex As Exception
                failed = True
            End Try
            Assert.IsTrue(failed, $"Managed to create an MVN instruction with invalid Rd={55}")
        End Sub
    End Class
End Namespace