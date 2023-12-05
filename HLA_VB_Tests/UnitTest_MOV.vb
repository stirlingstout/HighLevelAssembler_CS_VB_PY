Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_MOV
        <TestMethod>
        Sub TestMOV0()
            Dim m = New MOVRegisterInstruction(0, 1)
            Dim r = New Registers()
            r(0) = 0
            r(1) = 1
            m.Execute(r)
            Assert.AreEqual(1, r(0))
            Assert.AreEqual(1, r(1))

            ' TODO: fix these using the Sub deletegate mechanism
            Dim failed = False
            Try
                Dim e = New MOVRegisterInstruction(55, 0)
            Catch ex As Exception
                failed = True
            End Try
            Assert.IsTrue(failed, $"Managed to create an MOV instruction with invalid Rd={55}")

            failed = False
            Try
                Dim e = New MOVRegisterInstruction(0, 55)
            Catch ex As Exception
                failed = True
            End Try
            Assert.IsTrue(failed, $"Managed to create an MOV instruction with invalid Rm={55}")
        End Sub

        <TestMethod>
        Sub TestMOV1()
            Dim m = New MOVImmediateInstruction(0, -2)
            Dim r = New Registers()
            r(0) = 0
            r(1) = 1
            m.Execute(r)
            Assert.AreEqual(-2, r(0))
            Assert.AreEqual(1, r(1))

            Dim failed = False
            Try
                Dim e = New MOVImmediateInstruction(55, 0)
            Catch ex As Exception
                failed = True
            End Try
            Assert.IsTrue(failed, $"Managed to create an MOV instruction with invalid Rd={55}")
        End Sub
    End Class
End Namespace