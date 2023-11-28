Imports HLA_VB.HLA_VB
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace HLA_VB_Tests

    Public Class UnitTest_Data
        <TestClass>
        Public Class UnitTestData
            Inherits TestCommon

            <TestMethod>
            Sub TestData0()
                Dim d = New Data()
                Assert.AreEqual(0, d.GetValue())
            End Sub

            <TestMethod>
            Sub TestData1()
                Dim d = New Data()
                d.SetValue(27)
                Assert.AreEqual(27, d.GetValue())
            End Sub

            <TestMethod>
            Sub TestData2()
                Dim d = New Data()
                d.SetValue(98)
                Assert.AreEqual($"{" ".PadRight(TotalWidth)}98", d.ToString())
            End Sub

            <TestMethod>
            Sub TestData3()
                Dim d = New Data()
                d.SetValue(-15)
                d.AddLabel("Start")
                Assert.AreEqual($"{"Start:".PadRight(TotalWidth)}-15", d.ToString())
            End Sub
        End Class

    End Class
End Namespace