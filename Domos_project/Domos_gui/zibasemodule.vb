Imports STRGS = Microsoft.VisualBasic.Strings
Imports VB = Microsoft.VisualBasic
Imports ZibaseDll

Public Class zibasemodule
    Dim WithEvents zba As ZiBase = New ZiBase

    Public Sub lancer()
        zba.StartZB()
        MsgBox("Zibase lancée")
    End Sub

    Public Sub lancer_research()
        zba.RestartZibaseSearch()
        MsgBox("Research OK")
    End Sub

    Public Sub fermer()
        zba.StopZB()
        MsgBox("Zibase fermée")
    End Sub

    Private Sub zba_NewSensorDetected(ByVal seInfo As ZibaseDll.ZiBase.SensorInfo) Handles zba.NewSensorDetected
        MsgBox("NEW:" & seInfo.sID & " / " & seInfo.sType & " : " & seInfo.sValue)
    End Sub

    Private Sub zba_NewZibaseDetected(ByVal zbInfo As ZibaseDll.ZiBase.ZibaseInfo) Handles zba.NewZibaseDetected
        MsgBox("NZB:" & zbInfo.sLabelBase & " / " & zbInfo.lIpAddress)
    End Sub

    Private Sub zba_UpdateSensorInfo(ByVal seInfo As ZibaseDll.ZiBase.SensorInfo) Handles zba.UpdateSensorInfo
        MsgBox("UPD:" & seInfo.sID & " / " & seInfo.sType & " : " & seInfo.sValue)
    End Sub

    Private Sub zba_WriteMessage(ByVal sMsg As String, ByVal level As Integer) Handles zba.WriteMessage
        MsgBox("write : " & sMsg)
    End Sub

End Class
