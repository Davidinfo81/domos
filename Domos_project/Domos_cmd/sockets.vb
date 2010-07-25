Imports STRGS = Microsoft.VisualBasic.Strings
Imports System.Net.Sockets
Imports System.Net
Imports VB = Microsoft.VisualBasic

Public Class sockets
    Private myTcpListener As TcpListener
    Public socket_ouvert As Boolean = False
    'Private myTcpClient As New TcpClient
    Private myBuffer((New TcpClient).ReceiveBufferSize - 1) As Byte


    Public Function ouvrir(ByVal adresse As String, ByVal port As Integer)
        Try
            'créer le listener qui attend une connexion cliente
            myTcpListener = New TcpListener(IPAddress.Parse(adresse), port)
            myTcpListener.Start()
            socket_ouvert = True
            'des qu'une connexion arrive on la traite dans socket_accept
            myTcpListener.BeginAcceptTcpClient(AddressOf socket_accept, myTcpListener)
            Return "En attente d une connexion cliente"
        Catch ex As Exception
            Return "ERR: Ouverture socket : " & ex.ToString
        End Try
    End Function

    Public Function fermer()
        Try
            'mystream.Close()
            'myTcpClient.Close()
            socket_ouvert = False
            myTcpListener.Stop()
            Return "Fermé"
        Catch ex As Exception
            Return "ERR: Fermeture socket : " & ex.ToString
        End Try
    End Function

    Public Sub socket_accept(ByVal ar As IAsyncResult)
        Try
            If socket_ouvert Then
                'on recupere les parametres
                Dim myTcpListenerx As TcpListener = CType(ar.AsyncState, TcpListener)
                'on stoppe l'attente de connexion
                Dim tcpClient As TcpClient = myTcpListenerx.EndAcceptTcpClient(ar)

                'dés qu'on a une donnée dans le buffer, on la traite dans socket_received
                Dim mystream As NetworkStream
                mystream = tcpClient.GetStream()
                mystream.BeginRead(myBuffer, 0, (tcpClient.ReceiveBufferSize - 1), AddressOf socket_Received, mystream)

                'on relance l'attente de connexion
                myTcpListenerx.BeginAcceptTcpClient(AddressOf socket_accept, myTcpListenerx)
            End If
        Catch Ex As Exception
            WriteLog("ERR: socket_accept : " & Ex.Message)
        End Try

    End Sub

    Public Sub socket_Received(ByVal ar As IAsyncResult)
        Dim intCount As Integer
        Try
            If socket_ouvert Then
                'on recupere les parametres
                Dim mystream2 As NetworkStream = CType(ar.AsyncState, NetworkStream)
                'on arrete de lire
                intCount = mystream2.EndRead(ar)

                'on traite la donnée lu
                Dim clientdata As String = System.Text.Encoding.ASCII.GetString(myBuffer, 0, intCount)
                WriteLog("message reçu : " & clientdata)

                'on repond ACK
                Dim responseString As String = "ACK"
                Dim sendBytes As [Byte]() = System.Text.Encoding.ASCII.GetBytes(responseString)
                mystream2.Write(sendBytes, 0, sendBytes.Length)
                WriteLog("message envoyé : " & responseString)

                'on ferme le flux
                mystream2.Close()

                'on lance l'action adequat
                domos_cmd.action(clientdata)

            End If
        Catch ex As Exception
            WriteLog("ERR: socket_received : " & ex.Message)
        End Try
    End Sub

    Public Sub WriteLog(ByVal message As String)
        'utilise la fonction de base pour loguer un event
        If STRGS.InStr(message, "ERR:") > 0 Then
            domos_cmd.log("SOC : " & message, 2)
        Else
            domos_cmd.log("SOC : " & message, 3)
        End If
    End Sub

End Class
