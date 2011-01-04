Imports STRGS = Microsoft.VisualBasic.Strings
Imports VB = Microsoft.VisualBasic
Imports System.IO.Ports
Imports System.Math
Imports System.Net.Sockets
Imports System.Threading
Imports System.Globalization

Public Class montellstick
    Dim table_comp As New DataTable
    Dim ouvert As Boolean = False

    Public Sub New()
        '---------- Creation table des erreurs ----------
        Dim x As New DataColumn
        table_comp.Dispose()
        x = New DataColumn
        x.ColumnName = "nom"
        table_comp.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "id"
        table_comp.Columns.Add(x)
    End Sub

    Public Function ouvrir()
        Dim nbdevice As Integer = 0
        Dim deviceid As Integer = 0
        Try
            'TellStick.GetDevices()
            nbdevice = TellStick.GetNumberOfDevices()
            For i = 0 To nbdevice - 1
                deviceid = TellStick.GetDeviceId(i)
                'liste(i) = STRGS.UCase(TellStick.GetName(deviceid))
                Dim newRow As DataRow
                newRow = table_comp.NewRow()
                newRow.Item("nom") = STRGS.UCase(TellStick.GetName(deviceid))
                newRow.Item("id") = deviceid
                table_comp.Rows.Add(newRow)
            Next
            ouvert = True
            Return "Ouvert"
        Catch ex As Exception
            Return "ERR: " & ex.Message
        End Try
    End Function

    Public Function fermer()
        Try
            table_comp.Dispose()
            ouvert = False
            Return "Fermé"
        Catch ex As Exception
            ouvert = False
            Return "ERR: " & ex.Message
        End Try
    End Function

    Public Function ecrire(ByVal ordre As String, ByVal device As String)
        Try
            Dim tabletmp() As DataRow
            tabletmp = table_comp.Select("nom = '" & device & "'")
            If tabletmp.GetUpperBound(0) >= 0 Then
                Select Case STRGS.UCase(ordre)
                    Case "ON" : Return TellStick.TurnOn(tabletmp(0)("id"))
                    Case "OFF" : Return TellStick.TurnOff(tabletmp(0)("id"))
                    Case Else : Return "ERR: Ordre incorrect : " & ordre
                End Select
            Else
                Return "ERR: Device non trouvé : " & device
            End If
        Catch ex As Exception
            Return "ERR: " & ex.Message
        End Try
    End Function


End Class
