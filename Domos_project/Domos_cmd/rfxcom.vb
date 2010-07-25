Imports STRGS = Microsoft.VisualBasic.Strings
Imports VB = Microsoft.VisualBasic
Imports System.IO.Ports
Imports System.Math
Imports System.Net.Sockets

Public Class rfxcom
    ' Fournie une liste de fonction pour récupérer les valeurs lue par un RFXCOM USB
    ' Necessite l'installation du driver USB RFXCOM
    ' Auteur : David
    ' Date : 23/02/2009

#Region "Déclaration"
    Public WithEvents port As New System.IO.Ports.SerialPort
    Private port_ouvert As Boolean = False
    Private port_name As String = ""

    'Liste des commandes
    Public Const SWVERS As Byte = &H20
    Public Const MODEHS As Byte = &H21
    Public Const MODEKOP As Byte = &H23
    Public Const MODEARC As Byte = &H24
    Public Const MODEBD As Byte = &H25
    Public Const MODEB32 As Byte = &H29
    Public Const MODEVISONIC As Byte = &H40
    Public Const MODENOXLAT As Byte = &H41
    Public Const MODEVISAUX As Byte = &H42
    Public Const MODEVAR As Byte = &H2C
    Public Const ENALL As Byte = &H2A
    Public Const DISARC As Byte = &H2D
    Public Const DISKOP As Byte = &H2E
    Public Const DISX10 As Byte = &H2F
    Public Const DISHE As Byte = &H28
    Public Const DISOREGON As Byte = &H43
    Public Const DISATI As Byte = &H44
    Public Const DISVIS As Byte = &H45
    Public Const DISSOMFY As Byte = &H46

    'liste des variables de base
    Private slave As Boolean
    Private mess As Boolean = False
    Private currentdevice As Byte
    Private currentunit As Byte
    Private pulse As Char
    Private firstbyte As Boolean = True
    Private protocol As Byte = MODEVAR
    Private recbuf(30), recbytes, recbits As Byte
    Private bytecnt As Integer = 0
    Private message As String
    Private waitforack As Boolean = False
    Private vresponse As Boolean = False
    Private supply_voltage As Integer
    Private temperature As Single
    Private maxticks As Byte = 0
    Private simulate As Boolean
    Private TCPData(1024) As Byte
    Private client As TcpClient
    Private tcp As Boolean
    Private stream As NetworkStream
    Private WithEvents tmrRead As New Windows.Forms.Timer
    Dim messagetemp, messagelast, adresselast, valeurlast, recbuf_last As String
    Dim nblast As Integer = 0
#End Region

    Public Function ouvrir(ByVal numero As String) As String
        Try
            If Not port_ouvert Then
                port_name = numero 'pour se rapeller du nom du port
                If VB.Left(numero, 3) <> "COM" Then
                    'RFXCOM est un modele ethernet
                    tcp = True
                    client = New TcpClient(numero, 10001)
                    port_ouvert = True
                    Return ("Port IP " & port_name & " ouvert")
                Else
                    'RFXCOM est un modele usb
                    tcp = False
                    port.PortName = port_name 'nom du port : COM1
                    port.BaudRate = 38400 'vitesse du port 300, 600, 1200, 2400, 9600, 14400, 19200, 38400, 57600, 115200
                    port.Parity = IO.Ports.Parity.None 'pas de parité
                    port.StopBits = IO.Ports.StopBits.One 'un bit d'arrêt par octet
                    port.DataBits = 8 'nombre de bit par octet
                    port.Encoding = System.Text.Encoding.GetEncoding(1252)  'Extended ASCII (8-bits)
                    port.StopBits = StopBits.One
                    port.Handshake = Handshake.None
                    port.ReadBufferSize = CInt(4096)
                    port.ReceivedBytesThreshold = 1
                    port.ReadTimeout = 100
                    port.WriteTimeout = 500
                    port.Open()
                    port_ouvert = True
                    If port.IsOpen Then
                        port.DtrEnable = True
                        port.RtsEnable = True
                        port.DiscardInBuffer()
                    End If
                    Return ("Port " & port_name & " ouvert")
                End If
            Else
                Return ("Port " & port_name & " dejà ouvert")
            End If
        Catch ex As Exception
            Return ("ERR: " & ex.Message)
        End Try
    End Function

    Public Function lancer() As String
        If tcp Then
            Try
                stream = client.GetStream()
                stream.BeginRead(TCPData, 0, 1024, AddressOf TCPDataReceived, Nothing)
            Catch ex As Exception
                WriteLog("ERR: LANCER GETSTREAM")
            End Try

            Return "Handler IP OK"
        Else
            tmrRead.enabled = True
            AddHandler port.DataReceived, New SerialDataReceivedEventHandler(AddressOf DataReceived)
            Return "Handler COM OK"
        End If

    End Function

    Public Function fermer() As String
        Try
            If port_ouvert Then
                'suppression de l'attente de données à lire
                RemoveHandler port.DataReceived, AddressOf DataReceived
                'fermeture des ports
                If tcp Then
                    port_ouvert = False
                    client.Close()
                    stream.Close()
                    Return ("Port IP fermé")
                Else
                    If (Not (port Is Nothing)) Then ' The COM port exists.
                        If port.IsOpen Then
                            Dim limite As Integer = 0
                            Do While (port.BytesToWrite > 0 And limite < 100) ' Wait for the transmit buffer to empty.
                                limite = limite + 1
                            Loop
                            limite = 0
                            Do While (port.BytesToRead > 0 And limite < 100) ' Wait for the receipt buffer to empty.
                                limite = limite + 1
                            Loop
                            port.Close()
                            port.Dispose()
                            port_ouvert = False
                            Return ("Port " & port_name & " fermé")
                        End If
                        Return ("Port " & port_name & "  est déjà fermé")
                    End If
                    Return ("Port " & port_name & " n'existe pas")
                End If
            End If
        Catch ex As UnauthorizedAccessException
            Return ("ERR: Port " & port_name & " IGNORE") ' The port may have been removed. Ignore.
        End Try
        Return True
    End Function

    Public Function ecrire(ByVal commande As Byte, ByVal commande2 As Byte) As String
        Dim cmd() As Byte = {commande, commande2}
        Dim message As String = ""
        Try
            If commande2 = MODEB32 Then
                protocol = MODEB32
                message = "Init cmd to receiver => F0" & VB.Right("0" & Hex(MODEB32), 2) & " Mode: 32 bit"
            End If
            If commande2 = MODEVAR Then
                protocol = MODEVAR
                message = "Init cmd to receiver => F0" & VB.Right("0" & Hex(MODEVAR), 2) & " Mode: Variable length mode"
            End If
            If commande2 = MODEKOP Then
                protocol = MODEVAR
                message = "Init cmd to receiver => F0" & VB.Right("0" & Hex(MODEKOP), 2) & " Mode: 24 bit KOPPLA"
            End If
            If commande2 = MODEARC Then
                protocol = MODEARC
                message = "Init cmd to receiver => F0" & VB.Right("0" & Hex(MODEARC), 2) & "Mode: Arc"
            End If
            If commande2 = MODEHS Then
                protocol = MODEHS
                message = "Init cmd to receiver => F0" & VB.Right("0" & Hex(MODEHS), 2) & "Mode: RFXCOM-HS plugin mode"
            End If
            If commande2 = MODEVISONIC Then
                protocol = MODEVISONIC
                message = "Init cmd to receiver => F0" & VB.Right("0" & Hex(MODEVISONIC), 2) & "Mode: Visonic only"
            End If
            If commande2 = MODENOXLAT Then
                protocol = MODENOXLAT
                message = "Init cmd to receiver => F0" & VB.Right("0" & Hex(MODENOXLAT), 2) & "Mode: Visonic & Variable mode"
            End If
            If commande2 = MODEBD Then message = "Init cmd to receiver => F0" & VB.Right("0" & Hex(MODEBD), 2) & "Mode: Bd"
            If commande2 = MODEVISAUX Then message = "Init cmd to receiver => F0" & VB.Right("0" & Hex(MODEVISAUX), 2) & "Mode: Visionic AUX"

            If commande2 = DISKOP Then message = "Disable Koppla RF => F0" & VB.Right("0" & Hex(DISKOP), 2)
            If commande2 = DISX10 Then message = "Disable X10 RF => F0" & VB.Right("0" & Hex(DISX10), 2)
            If commande2 = DISARC Then message = "Disable ARC RF => F0" & VB.Right("0" & Hex(DISARC), 2)
            If commande2 = DISOREGON Then message = "Disable Oregon RF => F0" & VB.Right("0" & Hex(DISOREGON), 2)
            If commande2 = DISATI Then message = "Disable ATI Wonder RF => F0" & VB.Right("0" & Hex(DISATI), 2)
            If commande2 = DISHE Then message = "Disable HomeEasy RF => F0" & VB.Right("0" & Hex(DISHE), 2)
            If commande2 = DISVIS Then message = "Disable Visonic RF => F0" & VB.Right("0" & Hex(DISVIS), 2)

            If commande2 = ENALL Then message = "Enable ALL RF => F0" & VB.Right("0" & Hex(ENALL), 2)

            If commande2 = SWVERS Then
                maxticks = 0
                firstbyte = True
                waitforack = True
                vresponse = True
                message = "Version request to receiver => F0" & VB.Right("0" & Hex(SWVERS), 2)
            End If
            If tcp Then
                stream.Write(cmd, 0, cmd.Length)
            Else
                port.Write(cmd, 0, cmd.Length) 'port.Write(Chr(commande)) port.Write(Chr(commande2))
            End If
            waitforack = True
            Return message
        Catch ex As Exception
            Return ("ERR: " & ex.Message)
        End Try
    End Function

    Private Sub DataReceived(ByVal sender As Object, ByVal e As SerialDataReceivedEventArgs)
        Try
            While port_ouvert And port.BytesToRead > 0
                ProcessReceivedChar(port.ReadByte())
            End While
        Catch Ex As Exception
            WriteLog("ERR: RFXCOM Datareceived : " & Ex.Message)
        End Try
    End Sub

    Sub ReadErrorEvent(ByVal sender As Object, ByVal ev As SerialErrorReceivedEventArgs) Handles port.ErrorReceived
        Try
            While port.BytesToRead > 0
                Dim x = port.ReadByte()
                ProcessReceivedChar(x)
            End While
        Catch Ex As Exception
            WriteLog("ERR: RFXCOM Datareceived : " & Ex.Message)
        End Try
    End Sub

    Private Sub TCPDataReceived(ByVal ar As IAsyncResult)
        Dim intCount As Integer
        Try
            If port_ouvert Then intCount = stream.EndRead(ar)
            'intCount = client.GetStream.EndRead(ar)
            If port_ouvert Then ProcessNewTCPData(TCPData, 0, intCount)
            'Start a new read.
            'stream.BeginRead(TCPData, 0, 1024, AddressOf TCPDataReceived, Nothing)
            If port_ouvert Then stream.BeginRead(TCPData, 0, 1024, AddressOf TCPDataReceived, Nothing)
            'If port_ouvert Then TCPDataReceived(stream.BeginRead(TCPData, 0, 1024, Nothing, Nothing))
            'client.GetStream.BeginRead(TCPData, 0, 1024, AddressOf TCPDataReceived, Nothing)
        Catch Ex As Exception
            WriteLog("ERR: RFXCOM TCPDatareceived : " & Ex.Message)
        End Try
    End Sub

    Private Sub ProcessNewTCPData(ByVal Bytes() As Byte, ByVal offset As Integer, ByVal count As Integer)
        Dim intIndex As Integer
        Try
            For intIndex = offset To offset + count - 1
                ProcessReceivedChar(Bytes(intIndex))
            Next
        Catch ex As Exception
            WriteLog("ERR: RFXCOM ProcessNewTCPData : " & ex.Message)
        End Try
    End Sub

    Private Sub tmrRead_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrRead.Tick
        If Not firstbyte Then
            maxticks += 1
        End If
        If maxticks > 3 Then 'flush buffer due to 400ms timeout
            maxticks = 0
            firstbyte = True
            If vresponse = True Then  'display version?
                If (bytecnt = 2) Then
                    display_mess()
                End If
            Else
                WriteLog(" Buffer flushed due to timeout")
            End If
        Else

        End If
    End Sub

    Public Sub ProcessReceivedChar(ByVal sComChar As Byte)
        'si Domos est actif
        If domos_cmd.Serv_DOMOS Then
            'rassemble un message complet pour le traiter ensuite avec displaymess
            Dim temp As Byte
            maxticks = 0
            If firstbyte = True Then
                firstbyte = False
                bytecnt = 0
            End If
            temp = sComChar
            'WriteLog(VB.Right("0" & Hex(temp), 2))
            If waitforack = False Then
                If vresponse = True Then  'display version?
                    recbuf(bytecnt) = temp
                    If (bytecnt = 3) Then mess = True
                Else
                    Select Case protocol
                        Case MODEVAR, MODEVISONIC, MODENOXLAT, MODEARC
                            If bytecnt = 15 Then
                                recbuf(bytecnt - 1) = temp
                                mess = True
                            ElseIf bytecnt = 0 Then
                                recbits = temp And &H7F
                                If (temp And &H80) = 0 Then slave = False Else slave = True
                                If (recbits And &H7) = 0 Then recbytes = ((recbits And &H7F) >> 3) Else recbytes = ((recbits And &H7F) >> 3) + 1
                            ElseIf bytecnt = recbytes Then
                                recbuf(bytecnt - 1) = temp
                                bytecnt -= 1
                                mess = True
                            Else
                                recbuf(bytecnt - 1) = temp
                            End If
                        Case MODEB32
                            recbuf(bytecnt) = temp
                            If (bytecnt = 3) Then mess = True
                        Case MODEKOP
                            recbuf(bytecnt) = temp
                            If bytecnt = 2 Then mess = True
                    End Select
                End If
                bytecnt += 1
            Else
                mess = True
            End If
            If mess Then
                'gestion pour ne pas gérer deux fois le même paquet car la norme veut que les paquets soient envoyé deux fois de suite
                Dim xxx As String = ""
                For i As Integer = 0 To bytecnt
                    xxx = xxx & (VB.Right("0" & Hex(recbuf(i)), 2))
                Next
                If recbuf_last <> xxx Or nblast = 2 Then 'nouveau paquet ou c'est la troisieme fois qu'on le recoit 
                    display_mess()
                    recbuf_last = xxx
                    nblast = 1
                Else 'c'est la deuxieme paquet indentique qu'on recoit, on l'ignore
                    nblast = 2
                    firstbyte = True
                    mess = False
                End If
            End If
        End If
    End Sub

    Public Sub display_mess()
        'interprete le message recu
        Dim parity As Integer
        Dim rfxsensor, rfxpower As Boolean
        Dim logtemp As String = ""
        mess = False
        firstbyte = True

        'affichage de la chaine reçu
        'Dim valtemp As String = ""
        'Dim xxx As String = ""
        'For i As Integer = 0 To bytecnt
        '    'valtemp = valtemp & recbuf(i)
        '    xxx = xxx & (VB.Right("0" & Hex(recbuf(i)), 2))
        'Next
        'WriteLog(xxx)
        If waitforack = False Then
            If bytecnt = 4 Then
                parity = Not (((recbuf(0) And &HF0) >> 4) + (recbuf(0) And &HF) _
                + (recbuf(1) >> 4) + (recbuf(1) And &HF) _
                + (recbuf(2) >> 4) + (recbuf(2) And &HF) + (recbuf(3) >> 4)) And &HF
                If (parity = (recbuf(3) And &HF)) And (recbuf(0) + (recbuf(1) Xor &HF) = &HFF) Then rfxsensor = True Else rfxsensor = False
            ElseIf bytecnt = 6 Then
                parity = Not ((recbuf(0) >> 4) + (recbuf(0) And &HF) _
                + (recbuf(1) >> 4) + (recbuf(1) And &HF) _
                + (recbuf(2) >> 4) + (recbuf(2) And &HF) _
                + (recbuf(3) >> 4) + (recbuf(3) And &HF) _
                + (recbuf(4) >> 4) + (recbuf(4) And &HF) _
                + (recbuf(5) >> 4)) And &HF
                If (parity = (recbuf(5) And &HF)) And (recbuf(0) + (recbuf(1) Xor &HF) = &HFF) Then rfxpower = True Else rfxpower = False
            End If
            If vresponse = True Then  'display version
                vresponse = False
                recbits = 0
                If recbuf(0) = Asc("M") Then
                    logtemp = " Version Master=" & VB.Right("0" & Hex(recbuf(1)), 2)
                    If bytecnt > 3 Then
                        If recbuf(2) = Asc("S") Then
                            logtemp = logtemp & " Slave=" & VB.Right("0" & Hex(recbuf(3)), 2)
                        Else
                            logtemp = logtemp & VB.Right("0" & Hex(recbuf(2)), 2)
                        End If
                    End If
                ElseIf recbuf(0) = Asc("S") Then
                    logtemp = " Version Slave=" & VB.Right("0" & Hex(recbuf(1)), 2)
                    If bytecnt > 3 Then
                        If recbuf(2) = Asc("M") Then
                            logtemp = logtemp & " Master=" & VB.Right("0" & Hex(recbuf(3)), 2)
                        Else
                            logtemp = logtemp & VB.Right("0" & Hex(recbuf(2)), 2)
                        End If
                    End If
                Else
                    logtemp = "Version " & VB.Right("0" & Hex(recbuf(0)), 2)
                    If bytecnt > 3 Then
                        If recbuf(1) = Asc("S") Then
                            logtemp = logtemp & " Slave=" & VB.Right("0" & Hex(recbuf(2)), 2)
                        ElseIf recbuf(1) = Asc("M") Then
                            logtemp = logtemp & " Master=" & VB.Right("0" & Hex(recbuf(2)), 2)
                        Else
                            logtemp = logtemp & " " & VB.Right("0" & Hex(recbuf(1)), 2)
                        End If
                    ElseIf bytecnt = 3 Then
                        logtemp = logtemp & " " & VB.Right("0" & Hex(recbuf(1)), 2)
                    End If
                End If
                WriteLog(logtemp)
            ElseIf protocol = MODEARC Then
                processARC()
            ElseIf protocol = MODEKOP Then
                processkoppla()
            ElseIf rfxsensor Then
                processrfxsensor()
            ElseIf rfxpower Then
                processrfxmeter()
            ElseIf protocol = MODEVISONIC Then
                processvisonic(recbits)
            ElseIf protocol = MODENOXLAT Then
                If recbits = 36 Or recbits = 66 Or recbits = 72 Then
                    processvisonic(recbits)
                ElseIf recbits > 59 Then
                    If processoregon(recbits) = False Then
                        processvisonic(recbits)
                    End If
                Else
                    processx(recbits)
                End If
            ElseIf protocol = MODEVAR And recbits = 20 Then
                processati()
            ElseIf protocol = MODEVAR And recbits = 21 Then
                processatiplus()
            ElseIf protocol = MODEVAR And recbits = 34 Or recbits = 38 Then
                processhe()
            ElseIf protocol = MODEVAR And recbits = 56 Then
                processsomfy()
            ElseIf protocol = MODEVAR And (recbits = 56 Or recbits > 59) Then
                processoregon(recbits)
            Else
                processx(recbits)
            End If
            'If (protocol = MODEVAR Or protocol = MODENOXLAT) And recbits <> 0 Then
            '    If slave Then
            '        WriteMessage(" bits=" & Convert.ToString(recbits) & " from SLAVE", False)
            '    Else
            '        WriteMessage(" bits=" & Convert.ToString(recbits), False)
            '    End If
            'End If
        Else
            WriteLog("ACK")
        End If
        waitforack = False
    End Sub

#Region "Process"

    'OK
    Private Sub processx(ByVal recbits As Byte)
        Dim adresse As String = ""
        Dim valeur As String = ""

        'Dim hsaddr As Integer
        If ((recbuf(0) Xor recbuf(1)) = &HFF) And (recbuf(0) = &HEE) Then
            processrfremote()
        ElseIf recbuf(3) = 0 And (recbuf(2) And &HF) = 0 Then
            processatiplus()
        ElseIf ((recbuf(0) Xor recbuf(1)) = &HFF) And ((recbuf(2) Xor recbuf(3)) = &HFF) Then
            processx10()
        ElseIf recbits = 32 And ((recbuf(0) Xor recbuf(1)) = &HFE) And ((recbuf(2) Xor recbuf(3)) = &HFF) Then
            processdm10()
        ElseIf (recbuf(0) = ((recbuf(1) And &HF0) + (&HF - (recbuf(1) And &HF)))) And ((recbuf(2) Xor recbuf(3)) = &HFF) Then
            processx10security()
        ElseIf ((recbuf(2) Xor recbuf(3)) = &HFF) Then
            Select Case recbuf(2)
                Case &H44
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S DWS Visonic door sensor Alert + Tamper"
                Case &HC4
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S DWS Visonic door sensor Normal + Tamper)"
                Case &H4
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S DWS Visonic door sensor Alert "
                Case &H5
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S DWS Visonic door sensor Alert (battery low)"
                Case &H84
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S DWS Visonic door sensor Normal"
                Case &H85
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S DWS Visonic door sensor Normal (battery low)"
                Case &H4C
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S DWS Visonic motion sensor Alert + Tamper"
                Case &HCC
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S DWS Visonic motion sensor Normal + Tamper)"
                Case &HC
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S DWS Visonic motion sensor Alert "
                Case &HD
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S DWS Visonic motion sensor Alert (battery low)"
                Case &H8C
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S DWS Visonic motion sensor Normal"
                Case &H8D
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S DWS Visonic motion sensor Normal (battery low)"
                Case &HE0
                    If recbuf(0) = &HFF Then
                        WriteLog("X10Security : Master receiver jamming detected")
                    ElseIf recbuf(0) = &H0 Then
                        WriteLog("X10Security : Slave receiver jamming detected")
                    Else
                        WriteLog("X10Security : ERR: Unknown data packet received")
                    End If
                Case &HF8
                    If recbuf(0) = &HFF Then
                        WriteLog("X10Security : Master receiver end jamming detected")
                    ElseIf recbuf(0) = &H0 Then
                        WriteLog("X10Security : Slave receiver end jamming detected")
                    Else
                        WriteLog("ERR: unknown cmd")
                    End If
                Case &H2
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S REMOTE Visonic keyfob ARM Away (max)"
                Case &HE
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S REMOTE Visonic keyfob ARM Home (min)"
                Case &H22
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S REMOTE Visonic keyfob Panic   "
                Case &H42
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S REMOTE Visonic keyfob Lights On"
                Case &H82
                    adresse = (recbuf(0) * 256 + recbuf(1)).ToString
                    valeur = "S REMOTE Visonic keyfob Disarm  "
                Case Else : WriteLog("ERR: Unknown data packet received")
            End Select
            'hsaddr = createhsaddr()
            'If protocol = MODEB32 Then
            '    WriteLog("X10Security : addr:" & VB.Right("0" & Hex(recbuf(0)), 2) & " ID:" & VB.Right("    " & Str(hsaddr), 5))
            'Else
            '    WriteLog("X10Security : addr:" & VB.Right("0" & Hex(recbuf(0)), 2) & VB.Right("0" & Hex(recbuf(1)), 2) & " " & VB.Right("0" & Hex(recbuf(4)), 2) & " ID:" & VB.Right("    " & Str(hsaddr), 5))
            'End If
            If adresse <> "" Then WriteRetour(adresse, valeur)

        ElseIf protocol = MODEVAR Or protocol = MODENOXLAT Then
            If recbits = 32 And recbuf(0) = &H52 And recbuf(1) = &H46 Then
                Select Case recbuf(2)
                    Case &H58 : valeur = "RFXSensor Type-1"
                    Case &H32 : valeur = "RFXSensor Type-2"
                    Case &H33 : valeur = "RFXSensor Type-3"
                    Case &H34 : valeur = "RFXVamp"
                    Case Else : valeur = "Unknown RFXSensor"
                End Select
                If recbuf(2) <> &H34 Then
                    If (recbuf(3) And &H80) = 0 Then valeur = valeur & " Fast sampling mode" Else valeur = valeur & " Slow sampling mode"
                End If
                valeur = valeur & " version:" & CStr(recbuf(3) And &H7F)
                WriteLog(valeur)
            ElseIf recbits = 40 And recbuf(0) = &H53 And recbuf(1) = &H45 And recbuf(2) = &H4E Then
                valeur = "Sensor " & Hex(recbuf(3)) & " type="
                Select Case recbuf(4)
                    Case &H26 : valeur = valeur & "DS2438"
                    Case &H28 : valeur = valeur & "DS18B20"
                    Case Else : valeur = "ERR: Processx : unknown 1-Wire sensor"
                End Select
                WriteLog(valeur)
            ElseIf recbits = 48 Then
                WriteLog("ERR: Processx : noise or a 1-Wire sensor internal address")
            ElseIf (((recbuf(2) And &HF0) = &H10) Or ((recbuf(2) And &HF0) = &H20)) And recbits = 44 Then
                processdigimax()
            Else
                WriteLog("ERR: Processx : noise or an unknown data packet received")
            End If
        Else
            WriteLog("ERR: Processx : Unknown data packet received")
        End If
    End Sub

    'pas géré
    Private Sub processhe()
        'WriteLog("ERR: Process HE pas encore géré")
        'WriteMessage(" HE", False)
        'WriteMessage(" Device=" & Hex(recbuf(0) >> 6), False)
        'WriteMessage(VB.Right("0" & Hex((recbuf(0) << 2 Or recbuf(1) >> 6) And &HFF), 2), False)
        'WriteMessage(VB.Right("0" & Hex((recbuf(1) << 2 Or recbuf(2) >> 6) And &HFF), 2), False)
        'WriteMessage(VB.Right("0" & Hex((recbuf(2) << 2 Or recbuf(3) >> 6) And &HFF), 2), False)
        'WriteMessage(" Unit=" & CStr((recbuf(3) And &HF) + 1), False)
        'If recbits = 34 Then
        '    Select Case recbuf(3) And &H30
        '        Case &H0 : WriteMessage(" OFF", False)
        '        Case &H10 : WriteMessage(" ON", False)
        '        Case &H20 : WriteMessage(" GROUP OFF", False)
        '        Case &H30 : WriteMessage(" GROUP ON", False)
        '    End Select
        'Else
        '    Select Case recbuf(4) And &HC
        '        Case &H0
        '            WriteMessage(" Error (preset record length without preset bits set)", False)
        '        Case &H4
        '            If (recbuf(3) And &H20) = &H0 Then
        '                WriteMessage(" Preset command,", False)
        '            Else
        '                WriteMessage(" Preset group command,", False)
        '            End If
        '        Case &H8 : WriteMessage(" Reserved (unexpected)", False)
        '        Case &HC : WriteMessage(" Reserved (unexpected)", False)
        '    End Select
        '    WriteMessage(" Level=" & CStr((recbuf(4) >> 4) + 1), False)
        'End If
    End Sub

    'OK
    Private Sub processati()
        Dim remote As Integer
        Dim adresse, valeur As String
        If recbuf(0) > recbuf(1) Then remote = recbuf(0) - recbuf(1) Else remote = (recbuf(0) + &H100) - recbuf(1)
        adresse = (recbuf(2) >> 4).ToString
        'WriteMessage(" ATI[" & (recbuf(2) >> 4).ToString & "]C Remote type= ", False)
        'Select Case remote
        '    Case &HC5
        '        WriteMessage("ATI Remote Wonder", False)
        '    Case &HD5
        '        WriteMessage("Medion", False)
        '    Case Else
        '        WriteMessage("Unknown:" & Hex(remote), False)
        'End Select
        'WriteMessage(" Channel=" & Str(recbuf(2) >> 4), False)
        Select Case recbuf(1)
            Case &H0 : If remote = &HC5 Then valeur = "A" Else valeur = "Mute"
            Case &H1 : valeur = "B"
            Case &H2 : valeur = "power"
            Case &H3 : valeur = "TV"
            Case &H4 : valeur = "DVD"
            Case &H5 : If remote = &HC5 Then valeur = "?" Else valeur = "Photo"
            Case &H6 : If remote = &HC5 Then valeur = "Guide" Else valeur = "Music"
            Case &H7 : valeur = "Drag"
            Case &H8 : If remote = &HC5 Then valeur = "VOL+" Else valeur = "VOL-"
            Case &H9 : If remote = &HC5 Then valeur = "VOL-" Else valeur = "VOL+"
            Case &HA : valeur = "MUTE"
            Case &HB : valeur = "CHAN+"
            Case &HC : valeur = "CHAN-"
            Case &HD : valeur = "1"
            Case &HE : valeur = "2"
            Case &HF : valeur = "3"
            Case &H10 : valeur = "4"
            Case &H11 : valeur = "5"
            Case &H12 : valeur = "6"
            Case &H13 : valeur = "7"
            Case &H14 : valeur = "8"
            Case &H15 : valeur = "9"
            Case &H16 : valeur = "txt"
            Case &H17 : valeur = "0"
            Case &H18 : valeur = "snapshot ESC"
            Case &H19 : If remote = &HC5 Then valeur = "C" Else valeur = "DVD MENU"
            Case &H1A : valeur = "^"
            Case &H1B : If remote = &HC5 Then valeur = "D" Else valeur = "Setup"
            Case &H1C : valeur = "TV/RADIO"
            Case &H1D : valeur = "<"
            Case &H1E : valeur = "OK"
            Case &H1F : valeur = ">"
            Case &H20 : valeur = "<-"
            Case &H21 : valeur = "E"
            Case &H22 : valeur = "v"
            Case &H23 : valeur = "F"
            Case &H24 : valeur = "Rewind"
            Case &H25 : valeur = "Play"
            Case &H26 : valeur = "Fast forward"
            Case &H27 : valeur = "Record"
            Case &H28 : valeur = "Stop"
            Case &H29 : valeur = "Pause"
            Case &H2C : valeur = "TV"
            Case &H2D : valeur = "VCR"
            Case &H2E : valeur = "RADIO"
            Case &H2F : valeur = "TV Preview"
            Case &H30 : valeur = "Channel list"
            Case &H31 : valeur = "Video Desktop"
            Case &H32 : valeur = "red"
            Case &H33 : valeur = "green"
            Case &H34 : valeur = "yellow"
            Case &H35 : valeur = "blue"
            Case &H36 : valeur = "rename TAB"
            Case &H37 : valeur = "Acquire image"
            Case &H38 : valeur = "edit image"
            Case &H39 : valeur = "Full screen"
            Case &H3A : valeur = "DVD Audio"
            Case &H70 : valeur = "Cursor-left"
            Case &H71 : valeur = "Cursor-right"
            Case &H72 : valeur = "Cursor-up"
            Case &H73 : valeur = "Cursor-down"
            Case &H74 : valeur = "Cursor-up-left"
            Case &H75 : valeur = "Cursor-up-right"
            Case &H76 : valeur = "Cursor-down-right"
            Case &H77 : valeur = "Cursor-down-left"
            Case &H78 : valeur = "V"
            Case &H79 : valeur = "V-End"
            Case &H7C : valeur = "X"
            Case &H7D : valeur = "X-End"
            Case Else : valeur = "unknown"
        End Select
        WriteRetour(adresse, valeur)
    End Sub

    'OK
    Private Sub processatiplus()
        Dim remote As Integer
        Dim adresse, valeur As String
        If recbuf(0) > recbuf(1) Then remote = recbuf(0) - recbuf(1) Else remote = (recbuf(0) + &H100) - recbuf(1)
        adresse = (recbuf(2) >> 4).ToString
        'WriteMessage(" ATIPLUS[" & (recbuf(2) >> 4).ToString & "]C Remote type= ", False)
        'Select Case remote
        '    Case &HC5 : WriteMessage("ATI Remote Wonder Plus", False)
        '    Case Else : WriteMessage("Unknown remote:" & Hex(remote), False)
        'End Select
        'WriteMessage(" Channel=" & Str(recbuf(2) >> 4), False)
        'If (recbuf(1) And &H80) = 0 Then WriteMessage(" Even ", False) Else WriteMessage(" Odd  ", False)
        Select Case (recbuf(1) And &H7F)
            Case &H0 : valeur = "A"
            Case &H1 : valeur = "B"
            Case &H2 : valeur = "power"
            Case &H3 : valeur = "TV"
            Case &H4 : valeur = "DVD"
            Case &H5 : valeur = "?"
            Case &H6 : valeur = "Guide"
            Case &H7 : valeur = "Drag"
            Case &H8 : valeur = "VOL+"
            Case &H9 : valeur = "VOL-"
            Case &HA : valeur = "MUTE"
            Case &HB : valeur = "CHAN+"
            Case &HC : valeur = "CHAN-"
            Case &HD : valeur = "1"
            Case &HE : valeur = "2"
            Case &HF : valeur = "3"
            Case &H10 : valeur = "4"
            Case &H11 : valeur = "5"
            Case &H12 : valeur = "6"
            Case &H13 : valeur = "7"
            Case &H14 : valeur = "8"
            Case &H15 : valeur = "9"
            Case &H16 : valeur = "txt"
            Case &H17 : valeur = "0"
            Case &H18 : valeur = "Open Setup Menu"
            Case &H19 : valeur = "C"
            Case &H1A : valeur = "^"
            Case &H1B : valeur = "D"
            Case &H1C : valeur = "FM"
            Case &H1D : valeur = "<"
            Case &H1E : valeur = "OK"
            Case &H1F : valeur = ">"
            Case &H20 : valeur = "Max/Restore window"
            Case &H21 : valeur = "E"
            Case &H22 : valeur = "v"
            Case &H23 : valeur = "F"
            Case &H24 : valeur = "Rewind"
            Case &H25 : valeur = "Play"
            Case &H26 : valeur = "Fast forward"
            Case &H27 : valeur = "Record"
            Case &H28 : valeur = "Stop"
            Case &H29 : valeur = "Pause"
            Case &H2A : valeur = "TV2"
            Case &H2B : valeur = "Clock"
            Case &H2C : valeur = "i"
            Case &H2D : valeur = "ATI"
            Case &H2E : valeur = "RADIO"
            Case &H2F : valeur = "TV Preview"
            Case &H30 : valeur = "Channel list"
            Case &H31 : valeur = "Video Desktop"
            Case &H32 : valeur = "red"
            Case &H33 : valeur = "green"
            Case &H34 : valeur = "yellow"
            Case &H35 : valeur = "blue"
            Case &H36 : valeur = "rename TAB"
            Case &H37 : valeur = "Acquire image"
            Case &H38 : valeur = "edit image"
            Case &H39 : valeur = "Full screen"
            Case &H3A : valeur = "DVD Audio"
            Case &H70 : valeur = "Cursor-left"
            Case &H71 : valeur = "Cursor-right"
            Case &H72 : valeur = "Cursor-up"
            Case &H73 : valeur = "Cursor-down"
            Case &H74 : valeur = "Cursor-up-left"
            Case &H75 : valeur = "Cursor-up-right"
            Case &H76 : valeur = "Cursor-down-right"
            Case &H77 : valeur = "Cursor-down-left"
            Case &H78 : valeur = "Left Mouse Button"
            Case &H79 : valeur = "V-End"
            Case &H7C : valeur = "Right Mouse Button"
            Case &H7D : valeur = "X-End"
            Case Else : valeur = "unknown"
        End Select
        WriteRetour(adresse, valeur)
    End Sub

    'pas géré
    Private Sub processsomfy()
        'WriteLog("Process Somfy pas encore géré")
        'WriteMessage(" Somfy ", False)
    End Sub

    'pas géré
    Private Sub processARC()
        'WriteLog("ERR: Process ARC pas encore géré")
        'Dim group As Byte
        'Dim unit As Byte
        'Dim housecode As Char
        'Dim i As Integer
        'If bytecnt = 3 Then
        '    group = (((recbuf(1) And &H40) >> 5) Or ((recbuf(1) And &H10) >> 4)) + 1
        '    unit = (((recbuf(1) And &H4) >> 1) Or (recbuf(1) And &H1)) + 1
        '    housecode = Chr((((recbuf(2) And &H40) >> 3) Or ((recbuf(2) And &H10) >> 2) Or ((recbuf(2) And &H4) >> 1) Or (recbuf(2) And &H1)) + &H41)
        '    If recbuf(1) = &HFF Then
        '        Select Case recbuf(0)
        '            Case &H54
        '                message = " GROUP Housecode=" & housecode & " Command: ON"
        '            Case &H14
        '                message = " GROUP Housecode=" & housecode & " Command: OFF"
        '            Case &H55
        '                message = " GROUP Housecode=" & housecode & " Command: Button released"
        '            Case Else
        '        End Select
        '    Else
        '        Select Case recbuf(0)
        '            Case &H54
        '                message = " Housecode=" & housecode & " Group=" & Str(group) & " Unit=" & Str(unit) & " Command: ON"
        '            Case &H14
        '                message = " Housecode=" & housecode & " Group=" & Str(group) & " Unit=" & Str(unit) & " Command: OFF"
        '            Case &H55
        '                message = " Housecode=" & housecode & " Group=" & Str(group) & " Unit=" & Str(unit) & " Command: Button released"
        '            Case Else
        '        End Select
        '    End If
        'ElseIf bytecnt = 8 Or bytecnt = 9 Then
        '    message = " HomeEasy code="
        '    For i = 0 To (bytecnt - 1)
        '        message = message & VB.Right("0" & Hex(recbuf(i)), 2)
        '    Next
        'Else
        '    message = " Unknown code="
        '    For i = 0 To (bytecnt - 1)
        '        message = message & VB.Right("0" & Hex(recbuf(i)), 2)
        '    Next
        'End If
        'WriteMessage(message & " bits=" & recbits, False)
    End Sub

    'pas géré : renvoi le message recu dans le log
    Private Sub processkoppla()
        Dim temp As Byte
        Dim parity, i As Integer
        Dim morech As Boolean
        message = " System=" & VB.Right("0" & Trim(Str((recbuf(2) And &HF) + 1)), 2)
        message = message & "  CH:"
        If (recbuf(2) And &H20) = &H20 Then
            message = message & "1"
            morech = True
        End If
        If (recbuf(2) And &H40) <> 0 Then
            If morech Then
                message = message & "-"
            End If
            morech = True
            message = message & "2"
        End If
        If (recbuf(2) And &H80) <> 0 Then
            If morech Then
                message = message & "-"
            End If
            morech = True
            message = message & "3"
        End If
        If (recbuf(1) And &H1) <> 0 Then
            If morech Then
                message = message & "-"
            End If
            morech = True
            message = message & "4"
        End If
        If (recbuf(1) And &H2) <> 0 Then
            If morech Then
                message = message & "-"
            End If
            morech = True
            message = message & "5"
        End If
        If (recbuf(1) And &H4) <> 0 Then
            If morech Then
                message = message & "-"
            End If
            morech = True
            message = message & "6"
        End If
        If (recbuf(1) And &H8) <> 0 Then
            If morech Then
                message = message & "-"
            End If
            morech = True
            message = message & "7"
        End If
        If (recbuf(1) And &H10) <> 0 Then
            If morech Then
                message = message & "-"
            End If
            morech = True
            message = message & "8"
        End If
        If (recbuf(1) And &H20) <> 0 Then
            If morech Then
                message = message & "-"
            End If
            morech = True
            message = message & "9"
        End If
        If (recbuf(2) And &H10) <> 0 Then
            If morech Then
                message = message & "-"
            End If
            morech = True
            message = message & "10"
        End If
        message = message & "  Command:"
        If (recbuf(0) And &H3F) = &H10 Then
            message = message & " ON"
        ElseIf (recbuf(0) And &H3F) = &H11 Then
            message = message & " LEVEL 1"
        ElseIf (recbuf(0) And &H3F) = &H12 Then
            message = message & " LEVEL 2"
        ElseIf (recbuf(0) And &H3F) = &H13 Then
            message = message & " LEVEL 3"
        ElseIf (recbuf(0) And &H3F) = &H14 Then
            message = message & " LEVEL 4"
        ElseIf (recbuf(0) And &H3F) = &H15 Then
            message = message & " LEVEL 5"
        ElseIf (recbuf(0) And &H3F) = &H16 Then
            message = message & " LEVEL 6"
        ElseIf (recbuf(0) And &H3F) = &H17 Then
            message = message & " LEVEL 7"
        ElseIf (recbuf(0) And &H3F) = &H18 Then
            message = message & " LEVEL 8"
        ElseIf (recbuf(0) And &H3F) = &H19 Then
            message = message & " LEVEL 9"
        ElseIf (recbuf(0) And &H3F) = &H1A Then
            message = message & " OFF"
        ElseIf (recbuf(0) And &H38) = &H0 Then
            message = message & " UP"
            message = message & " cnt=" & VB.Right("0" & (Hex(recbuf(0) And &H7)), 2)
        ElseIf (recbuf(0) And &H38) = &H8 Then
            message = message & " DOWN"
            message = message & " cnt=" & VB.Right("0" & (Hex(recbuf(0) And &H7)), 2)
        ElseIf (recbuf(0) And &H3F) = &H1C Then
            message = message & " PROG"
        Else
            message = message & " unknown cmd:" & VB.Right("0" & (Hex(recbuf(0) And &H3F)), 2)
        End If

        parity = &H0
        temp = recbuf(0)
        For i = 1 To 4
            parity = parity + (temp And &H1)
            temp = temp >> 2
        Next
        If (parity And &H1) <> 1 Then
            message = message & " Odd parity error in byte 0"
        End If

        parity = &H0
        For i = 1 To 8
            parity = parity + (recbuf(0) And &H1)
            recbuf(0) = recbuf(0) >> 1
        Next
        If (parity And &H1) <> 0 Then
            message = message & " Even parity error in byte 0"
        End If

        parity = &H0
        temp = recbuf(2)
        For i = 1 To 4
            parity = parity + (temp And &H1)
            temp = temp >> 2
        Next
        temp = recbuf(1)
        For i = 1 To 4
            parity = parity + (temp And &H1)
            temp = temp >> 2
        Next
        If (parity And &H1) <> 1 Then
            message = message & " Odd parity error in byte 1-2"
        End If

        parity = &H0
        For i = 1 To 8
            parity = parity + (recbuf(2) And &H1)
            recbuf(2) = recbuf(2) >> 1
        Next
        For i = 1 To 8
            parity = parity + (recbuf(1) And &H1)
            recbuf(1) = recbuf(1) >> 1
        Next
        If (parity And &H1) <> 0 Then
            message = message & " Even parity error in byte 1-2"
        End If
        WriteLog("ERR: Process KOPPLA pas encore géré : " & message)
    End Sub

    'pas géré
    Private Sub processvisonic(ByVal recbits As Byte)
        'WriteLog("ERR: Process VISONIC pas encore géré")
        'Dim parity As Integer
        'If recbits = 96 Then
        '    WriteMessage(" MKP150 cmd from Console", False)
        'ElseIf recbits = 80 Then
        '    WriteMessage("       MKP150 cmd", False)
        'ElseIf recbits = 72 Then
        '    WriteMessage("       MKP150 cmd", False)
        'ElseIf recbits = 66 Then
        '    WriteMessage("       CodeSecure", False)
        '    WriteMessage(" encr:" & VB.Right("0" & Hex(recbuf(0)), 2) & VB.Right("0" & Hex(recbuf(1)), 2) & VB.Right("0" & Hex(recbuf(2)), 2) & VB.Right("0" & Hex(recbuf(3)), 2), False)
        '    WriteMessage(" serial:" & VB.Right("0" & Hex(recbuf(4)), 2) & VB.Right("0" & Hex(recbuf(5)), 2) & VB.Right("0" & Hex(recbuf(6)), 2) & Hex((recbuf(7) >> 4) And &HF), False)
        '    WriteMessage(" Button:", False)
        '    If (recbuf(7) And &H1) <> 0 Then WriteMessage(" Light", False)
        '    If (recbuf(7) And &H2) <> 0 Then WriteMessage(" Arm", False)
        '    If (recbuf(7) And &H4) <> 0 Then WriteMessage(" Disarm", False)
        '    If (recbuf(7) And &H8) <> 0 Then WriteMessage(" Arm-Home", False)
        '    If (recbuf(8) And &H4) <> 0 Then WriteMessage(" Repeat bit", False)
        '    If (recbuf(8) And &H8) <> 0 Then WriteMessage(" Bat-Low", False)

        'ElseIf recbits = 36 Then
        '    parity = &H0
        '    If (recbuf(0) And &H10) <> 0 Then parity += 1
        '    If (recbuf(0) And &H1) <> 0 Then parity += 1
        '    If (recbuf(1) And &H10) <> 0 Then parity += 1
        '    If (recbuf(1) And &H1) <> 0 Then parity += 1
        '    If (recbuf(2) And &H10) <> 0 Then parity += 1
        '    If (recbuf(2) And &H1) <> 0 Then parity += 1
        '    If (recbuf(3) And &H10) <> 0 Then parity += 1
        '    If (recbuf(3) And &H1) <> 0 Then parity += 1
        '    If (recbuf(4) And &H10) <> 0 Then parity += 1
        '    If (parity And &H1) <> 0 Then WriteMessage(" B0 Parity error B4+B8+B12+B16+B20+B24+B28+B32", True)
        '    parity = &H0
        '    If (recbuf(0) And &H20) <> 0 Then parity += 1
        '    If (recbuf(0) And &H2) <> 0 Then parity += 1
        '    If (recbuf(1) And &H20) <> 0 Then parity += 1
        '    If (recbuf(1) And &H2) <> 0 Then parity += 1
        '    If (recbuf(2) And &H20) <> 0 Then parity += 1
        '    If (recbuf(2) And &H2) <> 0 Then parity += 1
        '    If (recbuf(3) And &H20) <> 0 Then parity += 1
        '    If (recbuf(3) And &H2) <> 0 Then parity += 1
        '    If (recbuf(4) And &H20) <> 0 Then parity += 1
        '    If (parity And &H1) <> 0 Then WriteMessage(" B1 Parity error B5+B9+B13+B17+B21+B25+B29+B33", True)
        '    parity = &H0
        '    If (recbuf(0) And &H40) <> 0 Then parity += 1
        '    If (recbuf(0) And &H4) <> 0 Then parity += 1
        '    If (recbuf(1) And &H40) <> 0 Then parity += 1
        '    If (recbuf(1) And &H4) <> 0 Then parity += 1
        '    If (recbuf(2) And &H40) <> 0 Then parity += 1
        '    If (recbuf(2) And &H4) <> 0 Then parity += 1
        '    If (recbuf(3) And &H40) <> 0 Then parity += 1
        '    If (recbuf(3) And &H4) <> 0 Then parity += 1
        '    If (recbuf(4) And &H40) <> 0 Then parity += 1
        '    If (parity And &H1) <> 0 Then WriteMessage(" B2 Parity error B6+B10+B14+B18+B22+B26+B30+B34", True)
        '    parity = &H0
        '    If (recbuf(0) And &H80) <> 0 Then parity += 1
        '    If (recbuf(0) And &H8) <> 0 Then parity += 1
        '    If (recbuf(1) And &H80) <> 0 Then parity += 1
        '    If (recbuf(1) And &H8) <> 0 Then parity += 1
        '    If (recbuf(2) And &H80) <> 0 Then parity += 1
        '    If (recbuf(2) And &H8) <> 0 Then parity += 1
        '    If (recbuf(3) And &H80) <> 0 Then parity += 1
        '    If (recbuf(3) And &H8) <> 0 Then parity += 1
        '    If (recbuf(4) And &H80) <> 0 Then parity += 1
        '    If (parity And &H1) <> 0 Then WriteMessage(" B3 Parity error B7+B11+B15+B19+B23+B27+B31+B35", True)
        '    WriteMessage("               PowerCode", False)
        '    WriteMessage(" Addr:" & VB.Right("0" & Hex(recbuf(0)), 2) + VB.Right("0" & Hex(recbuf(1)), 2) + VB.Right("0" & Hex(recbuf(2)), 2), False)
        '    If (recbuf(3) And &H80) <> 0 Then
        '        WriteMessage(" Tamper   ,", False)
        '    Else
        '        WriteMessage(" No Tamper,", False)
        '    End If
        '    If (recbuf(3) And &H40) <> 0 Then
        '        WriteMessage("Alert,", False)
        '    Else
        '        WriteMessage("Close,", False)
        '    End If
        '    If (recbuf(3) And &H20) <> 0 Then
        '        WriteMessage("Battery-Low,", False)
        '    Else
        '        WriteMessage("Battery-OK ,", False)
        '    End If
        '    If (recbuf(3) And &H10) <> 0 Then
        '        WriteMessage("Alive,", False)
        '    Else
        '        WriteMessage("Event,", False)
        '    End If
        '    If (recbuf(3) And &H8) <> 0 Then
        '        WriteMessage("Restore reported    ,", False)
        '    Else
        '        WriteMessage("Restore not reported,", False)
        '    End If
        '    If (recbuf(3) And &H4) <> 0 Then
        '        WriteMessage("Primary contact", False)
        '    Else
        '        WriteMessage("Second. contact", False)
        '    End If
        '    If (recbuf(3) And &H2) <> 0 Then WriteMessage(",Bit 5??", False)
        '    If (recbuf(3) And &H1) <> 0 Then WriteMessage(",Bit 4??", False)
        'End If
    End Sub

    'pas géré
    Private Sub processrfxsensor()
        'WriteLog("ERR: Process RFXSENSOR pas encore géré ")
        'Dim barometer As Integer
        'Dim measured_value, humidity As Single

        'WriteMessage("               RFXsensor[" & (recbuf(0) * 256 + recbuf(1)).ToString, False)
        'Select Case (recbuf(0) And &H3)
        '    Case 0 : WriteMessage("]T", False)
        '    Case 1 : WriteMessage("]Z", False)
        '    Case 2 : WriteMessage("]V", False)
        '    Case 3 : WriteMessage("]?", False)
        'End Select
        'WriteMessage(" RFXSensor ", False)
        'WriteMessage(" addr:" & VB.Right("0" & Hex(recbuf(0)), 2), False)
        'WriteMessage(VB.Right("0" & Hex(recbuf(1)), 2), False)
        'WriteMessage(" ID:" & Convert.ToString(recbuf(1) + (recbuf(0) * 256)) & " ", False)
        'If (recbuf(3) And &H10) <> 0 Then
        '    Select Case recbuf(2)
        '        Case &H81 : WriteMessage(" Error: No 1-Wire device connected", False)
        '        Case &H82 : WriteMessage(" Error: 1-Wire ROM CRC error", False)
        '        Case &H83 : WriteMessage(" Error: 1-Wire device connected is not a DS1820", False)
        '        Case &H84 : WriteMessage(" Error: No end of read signal received from 1-Wire device", False)
        '        Case &H85 : WriteMessage(" Error: 1-Wire device Scratchpad CRC error", False)
        '        Case &H1 : WriteMessage(" Info: address incremented", False)
        '        Case &H2 : WriteMessage(" Info: battery low", False)
        '        Case Else : WriteMessage(" Unknown Info/Error code!", False)
        '    End Select
        'Else
        '    If (recbuf(0) And &H3) = 0 Then
        '        measured_value = recbuf(2) + ((recbuf(3) >> 5) * 0.125)
        '        If measured_value > 200 Then
        '            '   WriteMessage("Temp:-" & Convert.ToString(256.0 - measured_value) & "ºC | " & Convert.ToString((256.0 - measured_value) * 1.8 + 32) & "ºF", False)
        '            measured_value = 0 - (256 - measured_value)
        '        End If
        '        WriteMessage("Temp:" & Convert.ToString(measured_value) & "ºC | " & Convert.ToString((measured_value) * 1.8 + 32) & "ºF", False)
        '        temperature = measured_value

        '    ElseIf (recbuf(0) And &H3) = 1 Then
        '        measured_value = (recbuf(2) * 256 + recbuf(3)) >> 5
        '        ' It is assumed that only one RFXSensor is active!
        '        ' For correct processing you need to save the temp and voltage for each sensor
        '        ' and use this in the checks and calculations.
        '        If supply_voltage <> 0 Then
        '            humidity = (((measured_value / supply_voltage) - 0.16) / 0.0062)
        '            barometer = ((measured_value / supply_voltage) + 0.095) / 0.0009
        '        Else
        '            WriteMessage(" ", True)
        '            WriteMessage("                                   Not yet able to calculate the right RH and barometric pressure values.", True)
        '            WriteMessage("                                   Supply Voltage not yet available! Now 4.7V assumed. Reset the RFXSensor or wait. (max. 80min)", True)
        '            humidity = (((measured_value / 470) - 0.16) / 0.0062)
        '            barometer = ((measured_value / 470) + 0.095) / 0.0009
        '        End If
        '        If temperature <> 0 Then
        '            humidity = Math.Round(humidity / (1.0546 - 0.00216 * temperature), 2)
        '        Else
        '            If supply_voltage <> 0 Then
        '                WriteMessage(" ", True)
        '                WriteMessage("                                   Not yet able to calculate the right RH and barometric pressure values.", True)
        '            End If
        '            WriteMessage("                                   Temperature not yet available! Now 25ºC assumed. Reset the RFXSensor or wait. (max. 80min)", True)
        '            humidity = Math.Round(humidity / (1.0546 - 0.00216 * 25), 2)
        '        End If
        '        If supply_voltage = 0 Or temperature = 0 Then
        '            WriteMessage("                                   ", False)
        '        End If
        '        WriteMessage("RH:" & Convert.ToString(humidity) & "%", False)
        '        WriteMessage(" Barometer:" & Convert.ToString(barometer) & "hPa", False)
        '        WriteMessage(" A/D voltage:" & Convert.ToString(measured_value / 100), False)
        '    ElseIf (recbuf(0) And &H3) = 2 Then
        '        supply_voltage = (recbuf(2) * 256 + recbuf(3)) >> 5
        '        WriteMessage("Supply Voltage:" & Convert.ToString(supply_voltage / 100), False)
        '    ElseIf (recbuf(0) And &H3) = 3 Then
        '        If (recbuf(3) And &H20) = 0 Then
        '            WriteMessage("ZAP25:" & Convert.ToString(Math.Round((5 / 1024) * (recbuf(2) * 2 + (recbuf(3) >> 7)) / 0.033, 2) & "A"), False)
        '            WriteMessage(" ZAP50:" & Convert.ToString(Math.Round((5 / 1024) * (recbuf(2) * 2 + (recbuf(3) >> 7)) / 0.023, 2) & "A"), False)
        '            WriteMessage(" ZAP100:" & Convert.ToString(Math.Round((5 / 1024) * (recbuf(2) * 2 + (recbuf(3) >> 7)) / 0.019, 2) & "A"), False)
        '        Else
        '            WriteMessage("Voltage=" & Convert.ToString(recbuf(2) * 2), False)
        '        End If
        '    End If
        'End If
    End Sub

    'pas géré
    Private Sub processrfxmeter()
        'WriteLog("ERR: Process RFXMETER pas encore géré")
        'Dim measured_value As Single
        'WriteMessage("           RFXMeter[" & (recbuf(0) * 256 + recbuf(1)).ToString & "]M RFXMeter", False)
        'WriteMessage(" addr:" & VB.Right("0" & Hex(recbuf(0)), 2), False)
        'WriteMessage(VB.Right("0" & Hex(recbuf(1)), 2), False)
        'WriteMessage(" ID:" & Convert.ToString(recbuf(1) + (recbuf(0) * 256)) & " ", False)
        'Select Case recbuf(5) And &HF0
        '    Case &H0
        '        measured_value = ((recbuf(4) * 65536) + (recbuf(2) * 256) + recbuf(3))
        '        WriteMessage("RFXMeter: " & Convert.ToString(measured_value), False)
        '        WriteMessage(";  RFXPower: " & Convert.ToString(measured_value / 100) & " kWh", False)
        '        WriteMessage(";  RFXPower-Module: " & Convert.ToString(measured_value / 1000) & " kWh", False)
        '    Case &H10
        '        WriteMessage("Interval: ", False)
        '        Select Case recbuf(2)
        '            Case &H1 : WriteMessage("30 sec.", False)
        '            Case &H2 : WriteMessage("1 min.", False)
        '            Case &H4 : WriteMessage("6 (old=5) min.", False)
        '            Case &H8 : WriteMessage("12 (old=10) min.", False)
        '            Case &H10 : WriteMessage("15 min.", False)
        '            Case &H20 : WriteMessage("30 min.", False)
        '            Case &H40 : WriteMessage("45 min.", False)
        '            Case &H80 : WriteMessage("60 min.", False)
        '            Case Else : WriteMessage("illegal value", False)
        '        End Select
        '    Case &H20
        '        Select Case (recbuf(4) And &HC0)
        '            Case &H0 : WriteMessage("Input-0 ", False)
        '            Case &H40 : WriteMessage("Input-1 ", False)
        '            Case &H80 : WriteMessage("Input-2 ", False)
        '            Case Else : WriteMessage("Error, unknown input ", False)
        '        End Select
        '        measured_value = (((recbuf(4) And &H3F) * 65536) + (recbuf(2) * 256) + recbuf(3)) / 1000
        '        WriteMessage("Calibration: " & Convert.ToString(measured_value) & "msec ", False)
        '        If measured_value <> 0 Then
        '            WriteMessage("RFXPower= " & Convert.ToString(Round(1 / ((16 * measured_value) / (3600000 / 100)), 3)) & "kW", False)
        '            WriteMessage(" RFXPwr= " & Convert.ToString(Round(1 / ((16 * measured_value) / (3600000 / 62.5)), 3)) & "|" & Convert.ToString(Round((1 / ((16 * measured_value) / (3600000 / 62.5))) * 1.917, 3)) & "kW", False)
        '        End If
        '    Case &H30
        '        WriteMessage("New address set", False)
        '    Case &H40
        '        Select Case (recbuf(4) And &HC0)
        '            Case &H0 : WriteMessage("Counter for Input-0 will be set to zero within 5 seconds OR push MODE button for next command.", False)
        '            Case &H40 : WriteMessage("Counter for Input-1 will be set to zero within 5 seconds OR push MODE button for next command.", False)
        '            Case &H80 : WriteMessage("Counter for Input-2 will be set to zero within 5 seconds OR push MODE button for next command.", False)
        '            Case Else : WriteMessage("Error, unknown input ", False)
        '        End Select
        '    Case &H50
        '        WriteMessage("Push MODE push button within 5 seconds to increment the 1st digit.", False)
        '        measured_value = (recbuf(2) >> 4) * 100000 + (recbuf(2) And &HF) * 10000 + (recbuf(3) >> 4) _
        '        * 1000 + (recbuf(3) And &HF) * 100 + (recbuf(4) >> 4) * 10 + (recbuf(4) And &HF)
        '        WriteMessage("Counter value = " & VB.Right("00000" & Convert.ToString(measured_value), 6), False)
        '    Case &H60
        '        WriteMessage("Push MODE push button within 5 seconds to increment the 2nd digit.", False)
        '        measured_value = (recbuf(2) >> 4) * 100000 + (recbuf(2) And &HF) * 10000 + (recbuf(3) >> 4) _
        '        * 1000 + (recbuf(3) And &HF) * 100 + (recbuf(4) >> 4) * 10 + (recbuf(4) And &HF)
        '        WriteMessage("Counter value = " & VB.Right("00000" & Convert.ToString(measured_value), 6), False)
        '    Case &H70
        '        WriteMessage("Push MODE push button within 5 seconds to increment the 3rd digit.", False)
        '        measured_value = (recbuf(2) >> 4) * 100000 + (recbuf(2) And &HF) * 10000 + (recbuf(3) >> 4) _
        '        * 1000 + (recbuf(3) And &HF) * 100 + (recbuf(4) >> 4) * 10 + (recbuf(4) And &HF)
        '        WriteMessage("Counter value = " & VB.Right("00000" & Convert.ToString(measured_value), 6), False)
        '    Case &H80
        '        WriteMessage("Push MODE push button within 5 seconds to increment the 4th digit.", False)
        '        measured_value = (recbuf(2) >> 4) * 100000 + (recbuf(2) And &HF) * 10000 + (recbuf(3) >> 4) _
        '        * 1000 + (recbuf(3) And &HF) * 100 + (recbuf(4) >> 4) * 10 + (recbuf(4) And &HF)
        '        WriteMessage("Counter value = " & VB.Right("00000" & Convert.ToString(measured_value), 6), False)
        '    Case &H90
        '        WriteMessage("Push MODE push button within 5 seconds to increment the 5th digit.", False)
        '        measured_value = (recbuf(2) >> 4) * 100000 + (recbuf(2) And &HF) * 10000 + (recbuf(3) >> 4) _
        '        * 1000 + (recbuf(3) And &HF) * 100 + (recbuf(4) >> 4) * 10 + (recbuf(4) And &HF)
        '        WriteMessage("Counter value = " & VB.Right("00000" & Convert.ToString(measured_value), 6), False)
        '    Case &HA0
        '        WriteMessage("Push MODE push button within 5 seconds to increment the 6th digit.", False)
        '        measured_value = (recbuf(2) >> 4) * 100000 + (recbuf(2) And &HF) * 10000 + (recbuf(3) >> 4) _
        '        * 1000 + (recbuf(3) And &HF) * 100 + (recbuf(4) >> 4) * 10 + (recbuf(4) And &HF)
        '        WriteMessage("Counter value = " & VB.Right("00000" & Convert.ToString(measured_value), 6), False)
        '    Case &HB0
        '        Select Case recbuf(4)
        '            Case &H0 : WriteMessage("Counter for Input-0 reset to zero.", False)
        '            Case &H40 : WriteMessage("Counter for Input-1 reset to zero.", False)
        '            Case &H80 : WriteMessage("Counter for Input-2 reset to zero.", False)
        '            Case Else : WriteMessage("protocol error.", False)
        '        End Select
        '    Case &HC0
        '        WriteMessage("Enter SET INTERVAL RATE mode within 5 seconds OR push MODE button for next command.", False)
        '    Case &HD0
        '        Select Case (recbuf(4) And &HC0)
        '            Case &H0 : WriteMessage("Enter CALIBRATION mode for Input-0 within 5 seconds OR push MODE button for next command.", False)
        '            Case &H40 : WriteMessage("Enter CALIBRATION mode for Input-1 within 5 seconds OR push MODE button for next command.", False)
        '            Case &H80 : WriteMessage("Enter CALIBRATION mode for Input-2 within 5 seconds OR push MODE button for next command.", False)
        '            Case Else : WriteMessage("Error, unknown input ", False)
        '        End Select
        '    Case &HE0
        '        WriteMessage("Enter SET ADDRESS mode within 5 seconds OR push MODE button for next command.", False)
        '    Case &HF0
        '        If recbuf(2) < &H40 Then
        '            WriteMessage("RFXPower Identification,", False)
        '        ElseIf recbuf(2) < &H80 Then
        '            WriteMessage("RFXWater Identification,", False)
        '        ElseIf recbuf(2) < &HC0 Then
        '            WriteMessage("RFXGas Identification,", False)
        '        Else
        '            WriteMessage("RFXMeter Identification,", False)
        '        End If
        '        WriteMessage(" Firmware Version: " & VB.Right("0" & Hex(recbuf(2)), 2), False)
        '        WriteMessage(", Interval rate: ", False)
        '        Select Case recbuf(3)
        '            Case &H1 : WriteMessage("30 seconds", False)
        '            Case &H2 : WriteMessage("1 minute", False)
        '            Case &H4 : WriteMessage("6 minutes", False)
        '            Case &H8 : WriteMessage("12 minutes", False)
        '            Case &H10 : WriteMessage("15 minutes", False)
        '            Case &H20 : WriteMessage("30 minutes", False)
        '            Case &H40 : WriteMessage("45 minutes", False)
        '            Case &H80 : WriteMessage("60 minutes", False)
        '            Case Else : WriteMessage("illegal value", False)
        '        End Select
        '    Case Else
        '        WriteMessage("illegal packet type", False)
        'End Select
    End Sub

    'pas géré : renvoi le message recu dans le log
    Private Sub processrfremote()
        message = " REMOTE[0]C PC Remote: "
        Select Case recbuf(2)
            Case &H2 : message = message & "0"
            Case &H82 : message = message & "1"
            Case &HD1 : message = message & "MP3"
            Case &H42 : message = message & "2"
            Case &HD2 : message = message & "DVD"
            Case &HC2 : message = message & "3"
            Case &HD3 : message = message & "CD"
            Case &H22 : message = message & "4"
            Case &HD4 : message = message & "PC or SHIFT-4"
            Case &HA2 : message = message & "5"
            Case &HD5 : message = message & "SHIFT-5"
            Case &H62 : message = message & "6"
            Case &HE2 : message = message & "7"
            Case &H12 : message = message & "8"
            Case &H92 : message = message & "9"
            Case &HC0 : message = message & "CH-"
            Case &H40 : message = message & "CH+"
            Case &HE0 : message = message & "VOL-"
            Case &H60 : message = message & "VOL+"
            Case &HA0 : message = message & "MUTE"
            Case &H3A : message = message & "INFO"
            Case &H38 : message = message & "REW"
            Case &HB8 : message = message & "FF"
            Case &HB0 : message = message & "PLAY"
            Case &H72 : message = message & "PAUSE"
            Case &H70 : message = message & "STOP"
            Case &HB6 : message = message & "MENU"
            Case &HFF : message = message & "REC"
            Case &HC9 : message = message & "EXIT"
            Case &HD8 : message = message & "TEXT"
            Case &HD9 : message = message & "SHIFT-TEXT"
            Case &HF2 : message = message & "TELETEXT"
            Case &HD7 : message = message & "SHIFT-TELETEXT"
            Case &HBA : message = message & "A+B"
            Case &H52 : message = message & "ENT"
            Case &HD6 : message = message & "SHIFT-ENT"
            Case Else : message = message & "Unknown cmd"
        End Select
        WriteLog("ERR: Process RFXMETER pas encore géré : " & message)
    End Sub

    'OK
    Private Sub processx10()
        Dim recbytes As Byte
        Dim adresse, valeur As String
        Select Case (recbuf(0) And &HF0)
            Case &H60 : adresse = "A"
            Case &H70 : adresse = "B"
            Case &H40 : adresse = "C"
            Case &H50 : adresse = "D"
            Case &H80 : adresse = "E"
            Case &H90 : adresse = "F"
            Case &HA0 : adresse = "G"
            Case &HB0 : adresse = "H"
            Case &HE0 : adresse = "I"
            Case &HF0 : adresse = "J"
            Case &HC0 : adresse = "K"
            Case &HD0 : adresse = "L"
            Case &H0 : adresse = "M"
            Case &H10 : adresse = "N"
            Case &H20 : adresse = "O"
            Case &H30 : adresse = "P"
            Case Else : adresse = "Unknown unit-"
        End Select
        Select Case recbuf(2)
            Case &H80 : valeur = "All lights off"
            Case &H90 : valeur = "All lights on"
            Case &H88 : valeur = "Bright"
            Case &H98 : valeur = "Dim"
            Case Else
                If (recbuf(2) And &H10) = 0 Then recbytes = 0 Else recbytes = &H1
                If (recbuf(2) And &H8) <> 0 Then recbytes = recbytes + &H2
                If (recbuf(2) And &H40) <> 0 Then recbytes = recbytes + &H4
                If (recbuf(0) And &H4) <> 0 Then recbytes = recbytes + &H8
                recbytes = recbytes + 1
                adresse = adresse + Trim(Str(recbytes))
                If (recbuf(2) And &H1) = 1 Then
                    valeur = "-Prog Koppla (non X10)"
                ElseIf (recbuf(2) And &H20) = 0 Then
                    valeur = "ON"
                Else
                    valeur = "OFF"
                End If
        End Select
        WriteRetour(adresse, valeur)
    End Sub

    'OK
    Private Sub processdm10()
        Dim adresse, valeur As String
        adresse = (recbuf(0) * 256 + recbuf(1)).ToString
        'WriteMessage(" addr:" & VB.Right("0" & Hex(recbuf(0)), 2), False)
        'WriteMessage(" ID:" & VB.Right("    " & Str(createhsaddr()), 5), False)
        Select Case recbuf(2)
            Case &HE0 : valeur = "Motion detected"
            Case &HF0 : valeur = "Dark detected"
            Case &HF8 : valeur = "Light detected"
            Case Else : valeur = "Unknown command:" & VB.Right("0" & Hex(recbuf(2)), 2)
        End Select
        WriteRetour(adresse, valeur)
    End Sub

    'OK
    Private Sub processx10security()
        Dim adresse As String = ""
        Dim valeur As String = ""
        'Dim hsaddr As Integer
        adresse = (recbuf(0) * 256 + recbuf(1)).ToString
        Select Case recbuf(2)
            Case &H0 : valeur = "Alert (Max delay)"
            Case &H1 : valeur = "Alert (Battery+Max delay)"
            Case &H4 : valeur = "Alert"
            Case &H5 : valeur = "Alert (Battery)"
            Case &H80 : valeur = "Normal (Max delay)"
            Case &H81 : valeur = "Normal (battery+Max delay)"
            Case &H84 : valeur = "Normal"
            Case &H85 : valeur = "Normal (battery)"
            Case &H40 : valeur = "Alert + Tamper (Max delay)"
            Case &H44 : valeur = "Alert + Tamper"
            Case &HC0 : valeur = "Normal + Tamper (Max delay)"
            Case &HC4 : valeur = "Normal + Tamper"
            Case &HC : valeur = "Alert"
            Case &H8C : valeur = "Normal"
            Case &H20 : valeur = "Dark sensor"
            Case &H4C : valeur = "Alert + Tamper"
            Case &HCC : valeur = "Normal + Tamper"
            Case &H2 : valeur = "ARM Away (max)"
            Case &H82 : valeur = "Disarm"
            Case &H42 : valeur = "Lights On"
            Case &HC2 : valeur = "Lights Off"
            Case &H22 : valeur = "Panic"
            Case &HA : valeur = "ARM Home (max)"
            Case &H6 : valeur = "ARM Away (min)"
            Case &H86 : valeur = "Disarm"
            Case &H46 : valeur = "Lights On"
            Case &HC6 : valeur = "Lights Off"
            Case &H26 : valeur = "Panic"
            Case &HE : valeur = "ARM Home (min)"
            Case &H3 : valeur = "Panic"
            Case &H1C : valeur = "Temp -< Set"
            Case &H2B : valeur = "Temp > Set"
            Case &HE0 : valeur = "Motion"
            Case &HF0 : valeur = "Darkness detected"
            Case &HF8 : valeur = "Light detected"
            Case Else : valeur = "DEBUG : X10Security : Secur ??????"
                'Case &H0
                '    'valeur = "S DWS DS10/90  Alert (max delay)"
                '    valeur = "Alert"
                'Case &H1
                '    ' valeur = "S DWS DS10/90  Alert (max delay, batt low)"
                '    valeur = "Alert + Battery"
                'Case &H40
                '    'valeur = "S DWS DS90  Alert + Tamper(max delay)"
                '    valeur = "Alert + Tamper"
                'Case &H44
                '    'valeur = "S DWS Visonic or DS90  Alert + Tamper"
                '    valeur = "Alert : Open + Tamper"
                'Case &HC0
                '    'valeur = "S DWS DS90  Normal + Tamper (max delay)"
                '    valeur = "Alert : Open + Tamper"
                'Case &HC4
                '    'valeur = "S DWS Visonic or DS90  Normal + Tamper"
                '    valeur = "Alert : Closed + Tamper"
                'Case &H4
                '    'valeur = "S DWS Visonic or DS10/90  Alert"
                '    valeur = "Alert : Open"
                'Case &H5
                '    'valeur = "S DWS Visonic or DS10/90  Alert (battery low)"
                '    valeur = "Alert : Open + battery low"
                'Case &H80
                '    'valeur = "S DWS DS10/90  Normal (max delay)"
                '    valeur = "Normal"
                'Case &H81
                '    'valeur = "S DWS DS10/90  Normal (max delay, batt low)"
                '    valeur = "Alert : Closed + battery low"
                'Case &H84
                '    'valeur = "S DWS Visonic or DS10/90  Normal"
                '    valeur = "Closed"
                'Case &H85
                '    'valeur = "S DWS Visonic or DS10/90  Normal (battery low)"
                '    valeur = "Alert : Closed + battery low"
                'Case &HC : valeur = "DEBUG : Alert : MOTION S Visonic or MS10/20/90  Alert"
                'Case &H8C : valeur = "DEBUG : MOTION S Visonic or MS10/20/90  Normal"
                'Case &H20 : valeur = "DEBUG : X10Security : MS20  Dark sensor"
                'Case &H4C : valeur = "DEBUG : Alert : MOTION S Visonic or MS90  Alert + Tamper"
                'Case &HCC : valeur = "DEBUG : Alert : MOTION S Visonic or MS90  Normal + Tamper"
                'Case &HD
                '    'valeur = "DEBUG : MOTION S Visonic or MS90  Alert + batt.low"
                '    valeur = "Alert : Battery low"
                'Case &H8D
                '    'valeur = "DEBUG : MOTION S Visonic or MS90  Normal + batt.low"
                '    valeur = "Alert : Battery low"
                'Case &HE0
                '    If recbuf(0) = &HFF Then
                '        valeur = "DEBUG : X10Security : Master receiver jamming detected"
                '    ElseIf recbuf(0) = &H0 Then
                '        valeur = "DEBUG : X10Security : Slave receiver jamming detected"
                '    Else
                '        valeur = "DEBUG : GF DM10  Motion"
                '    End If
                'Case &HF0 : valeur = "DEBUG : GF DM10  Darkness detected"
                'Case &HF8
                '    If recbuf(0) = &HFF Then
                '        valeur = "DEBUG : X10Security : Master end receiver jamming detected"
                '    ElseIf recbuf(0) = &H0 Then
                '        valeur = "DEBUG : X10Security : Slave receiver end jamming detected"
                '    Else
                '        valeur = "DEBUG : GF DM10  Light detected"
                '    End If
                'Case &H6 : valeur = "DEBUG : REMOTE S KR10/SH624 ARM Away (min)"
                'Case &H26
                '    'valeur = "DEBUG : REMOTE S Visonic or KR10/Smoke Panic"
                '    valeur = "Smoke Panic"
                'Case &H46 : valeur = "DEBUG : REMOTE S KR10  Lights On"
                'Case &H86 : valeur = "DEBUG : REMOTE S KR10  Disarm"
                'Case &HC6 : valeur = "DEBUG : REMOTE S KR10  Lights Off"
                'Case &H2 : valeur = "DEBUG : REMOTE S Visonic or SH624 ARM Away (max)"
                'Case &H3 : valeur = "DEBUG : REMOTE S SH624 Panic"
                'Case &HA : valeur = "DEBUG : REMOTE S SH624 ARM Home (max)"
                'Case &HE : valeur = "DEBUG : REMOTE S Visonic or SH624 ARM Home (min)"
                'Case &H22 : valeur = "DEBUG : REMOTE S SH624 Panic"
                'Case &H42 : valeur = "DEBUG : REMOTE S Visonic or SH624 Lights On"
                'Case &H82 : valeur = "DEBUG : REMOTE S Visonic or SH624 Disarm"
                'Case &HC2 : valeur = "DEBUG : REMOTE S SH624 Lights Off"
                'Case Else : valeur = "DEBUG : X10Security : Secur ??????"
        End Select
        If protocol = MODEB32 Then
            'hsaddr = createhsaddr()
            'WriteLog("DEBUG : X10Security : addr:" & VB.Right("0" & Hex(recbuf(0)), 2) & " ID:" & VB.Right("    " & Str(hsaddr), 5))
        Else
            'hsaddr = createhsaddr()
            'WriteLog("DEBUG : X10Security : addr:" & VB.Right("0" & Hex(recbuf(0)), 2) & VB.Right("0" & Hex(recbuf(1)), 2) & " " & VB.Right("0" & Hex(recbuf(4)), 2) & " ID:" & VB.Right("    " & Str(hsaddr), 5))
        End If
        If valeur <> "" Then
            valeur = valeur + " " + VB.Right("0" & Hex(recbuf(0)), 2) + "-" + VB.Right("0" & Hex(recbuf(1)), 2) + "-" + VB.Right("0" & Hex(recbuf(2)), 2) + "-" + VB.Right("0" & Hex(recbuf(3)), 2) + "-" + VB.Right("0" & Hex(recbuf(4)), 2) + "-" + VB.Right("0" & Hex(recbuf(5)), 2)
            WriteRetour(adresse, valeur)
        End If
    End Sub

    'OK
    Private Sub processdigimax()
        Dim parity, hsaddr As Integer
        Dim adresse, valeur As String
        Dim err As Boolean = False
        hsaddr = recbuf(1) * 256 + recbuf(0)
        adresse = CStr(recbuf(1) * 256 + recbuf(0))
        'WriteMessage(" DIGIMAX[" & CStr(recbuf(1) * 256 + recbuf(0)) & "]TY", False)
        'WriteMessage(" Digimax  addr:" & VB.Right("0" & Hex(recbuf(0)), 2), False)
        'WriteMessage(VB.Right("0" & Hex(recbuf(1)), 2) & " ID:" & VB.Right("    " & Str(hsaddr), 5), False)
        If (recbuf(4) And &H40) = 0 Then
            Select Case recbuf(2) And &H30
                Case &H0 : valeur = "No Set temp available"
                Case &H10 : valeur = "Demand for heat"
                Case &H20 : valeur = "No demand for heat"
                Case &H30 : valeur = "Initializing"
            End Select
        Else
            Select Case recbuf(2) And &H30
                Case &H0 : valeur = "No Set temp available"
                Case &H10 : valeur = "No demand for cooling"
                Case &H20 : valeur = "Demand for cooling"
                Case &H30 : valeur = "Initializing"
            End Select
        End If
        valeur = Convert.ToString(recbuf(3)) 'valeur de la temperature mesurée
        If protocol = MODEB32 Then
            parity = Not (((recbuf(0) And &HF0) >> 4) + (recbuf(0) And &HF) + ((recbuf(1) And &HF0) >> 4) + (recbuf(1) And &HF) + ((recbuf(2) And &HF0) >> 4)) And &HF
            If parity <> (recbuf(2) And &HF) Then
                WriteLog("ERR: DIGIMAX " & adresse & " : Parity error on address/status SB:" & Hex(parity))
                err = True
            End If
        Else
            valeur = valeur & "->" & Convert.ToString(recbuf(4) And &H3F) 'ajout de la temperature désiré
            parity = Not (((recbuf(0) And &HF0) >> 4) + (recbuf(0) And &HF) + ((recbuf(1) And &HF0) >> 4) + (recbuf(1) And &HF) + ((recbuf(2) And &HF0) >> 4)) And &HF
            If parity <> (recbuf(2) And &HF) Then
                WriteLog("ERR: DIGIMAX " & adresse & " : Parity error on address/status SB:" & Hex(parity))
                err = True
            End If
            parity = Not (((recbuf(3) And &HF0) >> 4) + (recbuf(3) And &HF) + ((recbuf(4) And &HF0) >> 4) + (recbuf(4) And &HF)) And &HF
            If parity <> ((recbuf(5) And &HF0) >> 4) Then
                WriteLog("ERR: DIGIMAX " & adresse & " : Parity error on temp/set SB:" & Hex(parity))
                err = True
            End If
        End If
        If err = False Then WriteRetour(adresse, valeur)
    End Sub

    'OK
    Private Function processoregon(ByVal recbits As Byte) As Boolean
        Dim direction As Integer
        Dim uv, dd, mm, yy As Single
        Dim hr, mn, sc, dag As String
        Dim oregon As Boolean = False
        'Dim ch As Byte
        Dim adresse, valeur As String

        'WriteMessage(" addr:" & VB.Right("0" & Hex(recbuf(3)), 2), False)

        If recbuf(0) = &HA And recbuf(1) = &H4D And recbits >= 72 Then
            '------------- THR128,THx138 ---------------
            oregon = True
            adresse = CStr((recbuf(3)) * 256 + ((recbuf(2) >> 4) And &H7))
            'ch = wrchannel()
            If ((recbuf(5) And &HF0) < &HA0) And ((recbuf(5) And &HF) < &HA) And ((recbuf(4) And &HF0) < &HA0) Then
                If (recbuf(6) And &H8) = 0 Then
                    valeur = CStr(CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10)
                Else
                    valeur = CStr(0 - (CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10))
                End If
            Else
                valeur = "ERR: wrong value in temperature field=" & Hex(recbuf(5)) & "." & Hex(recbuf(4) >> 4)
            End If
            WriteRetour(adresse, valeur)
            If (recbuf(4) And &H4) = &H4 Then WriteRetour(adresse, "ERR: battery empty")
            'checksum8()

        ElseIf recbuf(0) = &HEA And recbuf(1) = &H4C And recbits >= 60 Then
            '------------- THN132N,THWR288 ---------------
            oregon = True
            adresse = CStr((recbuf(3)) * 256 + ((recbuf(2) >> 4) And &H7))
            'ch = wrchannel()
            If (recbuf(6) And &H8) = 0 Then
                valeur = CStr(CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10)
            Else
                valeur = CStr(0 - (CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10))
            End If
            WriteRetour(adresse, valeur)
            If (recbuf(4) And &H4) = &H4 Then WriteRetour(adresse, "ERR: battery empty")
            'checksumw()

        ElseIf recbuf(0) = &H1A And recbuf(1) = &H2D And recbits >= 72 Then
            '------------- THGN122N-NX,THGR228N,THGR268 ---------------
            oregon = True
            adresse = CStr((recbuf(3)) * 256 + ((recbuf(2) >> 4) And &H7))
            'ch = wrchannel()
            If (recbuf(6) And &H8) = 0 Then
                valeur = CStr(CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10)
            Else
                valeur = CStr(0 - (CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10))
            End If
            WriteRetour(adresse & "_THE", valeur)
            valeur = CStr(VB.Right(Hex(((recbuf(7) << 4) And &HF0) + ((recbuf(6) >> 4) And &HF)), 2))
            WriteRetour(adresse & "_HYG", valeur)
            valeur = wrhum(recbuf(7) And &HC0)
            WriteRetour(adresse & "_HUM", valeur)
            If (recbuf(4) And &H4) = &H4 Then WriteRetour(adresse & "_THE", "ERR: battery empty")
            'checksum8()

        ElseIf recbuf(0) = &HFA And recbuf(1) = &H28 And recbits >= 72 Then
            '------------- THR128,THx138 ---------------
            oregon = True
            adresse = CStr((recbuf(3)) * 256 + ((recbuf(2) >> 4) And &H7))
            'ch = wrchannel3()
            If (recbuf(6) And &H8) = 0 Then
                valeur = CStr(CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10)
            Else
                valeur = CStr(0 - (CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10))
            End If
            WriteRetour(adresse & "_THE", valeur)
            valeur = CStr(VB.Right(Hex(((recbuf(7) << 4) And &HF0) + ((recbuf(6) >> 4) And &HF)), 2))
            WriteRetour(adresse & "_HYG", valeur)
            valeur = wrhum(recbuf(7) And &HC0)
            WriteRetour(adresse & "_HUM", valeur)
            If (recbuf(4) And &H4) = &H4 Then WriteRetour(adresse & "_THE", "ERR: battery empty")
            'checksum8()

        ElseIf (recbuf(0) And &HF) = &HA And recbuf(1) = &HCC And recbits >= 72 Then
            '------------- RTGR328N ---------------
            oregon = True
            adresse = CStr((recbuf(3)) * 256 + ((recbuf(2) >> 4) And &H7))
            'ch = wrchannel3()
            'WriteMessage(" counter:" & CStr(recbuf(0) >> 4), False)
            If (recbuf(6) And &H8) = 0 Then
                valeur = CStr(CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10)
            Else
                valeur = CStr(0 - (CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10))
            End If
            WriteRetour(adresse & "_THE", valeur)
            valeur = CStr(VB.Right(Hex(((recbuf(7) << 4) And &HF0) + ((recbuf(6) >> 4) And &HF)), 2))
            WriteRetour(adresse & "_HYG", valeur)
            valeur = wrhum(recbuf(7) And &HC0)
            WriteRetour(adresse & "_HUM", valeur)
            If (recbuf(4) And &H4) = &H4 Then WriteRetour(adresse & "_THE", "ERR: battery empty")
            'checksum8()

        ElseIf recbuf(0) = &HCA And recbuf(1) = &H2C And recbits >= 72 Then
            '------------- THGR328 ---------------
            oregon = True
            adresse = CStr((recbuf(3)) * 256 + ((recbuf(2) >> 4) And &H7))
            'ch = wrchannel()
            'WriteMessage(" counter:" & CStr(recbuf(0) >> 4), False)
            If (recbuf(6) And &H8) = 0 Then
                valeur = CStr(CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10)
            Else
                valeur = CStr(0 - (CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10))
            End If
            WriteRetour(adresse & "_THE", valeur)
            valeur = CStr(VB.Right(Hex(((recbuf(7) << 4) And &HF0) + ((recbuf(6) >> 4) And &HF)), 2))
            WriteRetour(adresse & "_HYG", valeur)
            valeur = wrhum(recbuf(7) And &HC0)
            WriteRetour(adresse & "_HUM", valeur)
            If (recbuf(4) And &H4) = &H4 Then WriteRetour(adresse & "_THE", "ERR: battery empty")
            'checksum8()

        ElseIf recbuf(0) = &HFA And recbuf(1) = &HB8 And recbits >= 72 Then
            '------------- WTGR800 ---------------
            oregon = True
            adresse = CStr(recbuf(3) * 256)
            If (recbuf(6) And &H8) = 0 Then
                valeur = CStr(CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10)
            Else
                valeur = CStr(0 - (CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10))
            End If
            WriteRetour(adresse & "_THE", valeur)
            valeur = CStr(VB.Right(Hex(((recbuf(7) << 4) And &HF0) + ((recbuf(6) >> 4) And &HF)), 2))
            WriteRetour(adresse & "_HYG", valeur)
            valeur = wrhum(recbuf(7) And &HC0)
            WriteRetour(adresse & "_HUM", valeur)
            valeur = wrbattery()
            WriteRetour(adresse & "_BAT", valeur)
            'checksum8()

        ElseIf recbuf(0) = &H1A And recbuf(1) = &H3D And recbits >= 72 Then
            '------------- THGR918 ---------------
            oregon = True
            adresse = CStr((recbuf(3)) * 256 + ((recbuf(2) >> 4) And &H7))
            'ch = wrchannel()
            If (recbuf(6) And &H8) = 0 Then
                valeur = CStr(CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10)
            Else
                valeur = CStr(0 - (CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10))
            End If
            WriteRetour(adresse & "_THE", valeur)
            valeur = CStr(VB.Right(Hex(((recbuf(7) << 4) And &HF0) + ((recbuf(6) >> 4) And &HF)), 2))
            WriteRetour(adresse & "_HYG", valeur)
            valeur = wrhum(recbuf(7) And &HC0)
            WriteRetour(adresse & "_HUM", valeur)
            valeur = wrbattery()
            WriteRetour(adresse & "_BAT", valeur)
            'checksum8()

        ElseIf recbuf(0) = &H5A And recbuf(1) = &H5D And recbits >= 88 Then
            '------------- BTHR918 ---------------
            oregon = True
            adresse = CStr((recbuf(3)) * 256 + ((recbuf(2) >> 4) And &H7))
            'ch = wrchannel()
            If (recbuf(6) And &H8) = 0 Then
                valeur = CStr(CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10)
            Else
                valeur = CStr(0 - (CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10))
            End If
            WriteRetour(adresse & "_THE", valeur)
            valeur = CStr(VB.Right(Hex(((recbuf(7) << 4) And &HF0) + ((recbuf(6) >> 4) And &HF)), 2))
            WriteRetour(adresse & "_HYG", valeur)
            valeur = wrhum(recbuf(7) And &HC0)
            WriteRetour(adresse & "_HUM", valeur)
            valeur = CStr(recbuf(8) + 795) 'en hPa
            WriteRetour(adresse & "_BAR", valeur)
            valeur = wrforecast(recbuf(9) And &HF)
            WriteRetour(adresse & "_FOR", valeur)
            If (recbuf(4) And &H4) = &H4 Then WriteRetour(adresse & "_THE", "ERR: battery empty")
            'checksum10()

        ElseIf recbuf(0) = &H5A And recbuf(1) = &H6D And recbits >= 88 Then
            '------------- BTHR918N,BTHR968 ---------------
            oregon = True
            adresse = CStr((recbuf(3)) * 256 + ((recbuf(2) >> 4) And &H7))
            'ch = wrchannel()
            If (recbuf(6) And &H8) = 0 Then
                valeur = CStr(CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10)
            Else
                valeur = CStr(0 - (CSng(Hex(recbuf(5))) + CSng(Hex(recbuf(4) >> 4)) / 10))
            End If
            WriteRetour(adresse & "_THE", valeur)
            valeur = CStr(VB.Right(Hex(((recbuf(7) << 4) And &HF0) + ((recbuf(6) >> 4) And &HF)), 2))
            WriteRetour(adresse & "_HYG", valeur)
            valeur = wrhum(recbuf(7) And &HC0)
            WriteRetour(adresse & "_HUM", valeur)
            valeur = CStr(recbuf(8) + 795) 'en hPa
            WriteRetour(adresse & "_BAR", valeur)
            valeur = wrforecast(recbuf(9) >> 4)
            WriteRetour(adresse & "_FOR", valeur)
            valeur = wrbattery()
            WriteRetour(adresse & "_BAT", valeur)
            'checksum10()

        ElseIf recbuf(0) = &H2A And recbuf(1) = &H1D And recbits >= 80 Then
            '------------- RGR126,RGR682,RGR918 ---------------
            oregon = True
            adresse = CStr(recbuf(3) * 256)
            valeur = CStr(CSng(Hex(recbuf(5))) * 10 + CSng(Hex((recbuf(4) >> 4) And &HF)))
            WriteRetour(adresse & "_RAF", valeur) 'mm/hr
            valeur = CStr(CSng(Hex(recbuf(8) And &HF)) * 1000 + CSng(Hex(recbuf(7))) * 10 + CSng(Hex(recbuf(6) >> 4)))
            WriteRetour(adresse & "_RAT", valeur) 'mm
            valeur = Hex(recbuf(6) And &HF)
            WriteRetour(adresse & "_RAP", valeur) 'flip cnt
            If (recbuf(4) And &H4) <> 0 Then WriteRetour(adresse & "_RAF", "ERR: battery empty")
            'checksum2()

        ElseIf recbuf(0) = &H2A And recbuf(1) = &H19 And recbits >= 84 Then
            '------------- PCR800 ---------------
            oregon = True
            adresse = CStr(recbuf(3) * 256)
            valeur = CStr(Round((((CSng(Hex(recbuf(5))) + CSng(Hex((recbuf(4) >> 4) And &HF))) / 10 + CSng(Hex((recbuf(4) And &HF))) / 100) * 25.4), 2)) ' mm/hr"
            WriteRetour(adresse & "_RAF", valeur) 'mm/hr
            valeur = (CSng(Hex(recbuf(7))) / 100 + CSng(Hex(recbuf(6) >> 4)) / 1000)
            valeur = CStr(Round(((valeur + (CSng(Hex(recbuf(9) And &HF)) * 100 + CSng(Hex(recbuf(8))))) * 25.4), 2))
            WriteRetour(adresse & "_RAT", valeur) 'mm
            If (recbuf(4) And &H4) <> 0 Then WriteRetour(adresse & "_RAF", "ERR: battery empty")
            'checksumr()

        ElseIf recbuf(0) = &H6 And recbuf(1) = &HE4 And recbits >= 84 Then
            '------------- RAIN XX ---------------
            oregon = True
            adresse = CStr(recbuf(3) * 256)
            valeur = CStr(Round((((CSng(Hex(recbuf(5))) + CSng(Hex((recbuf(4) >> 4) And &HF))) / 10 + CSng(Hex((recbuf(4) And &HF))) / 100) * 25.4), 2)) ' mm/hr"
            WriteRetour(adresse & "_RAF", valeur) 'mm/hr
            valeur = (CSng(Hex(recbuf(7))) / 100 + CSng(Hex(recbuf(6) >> 4)) / 1000)
            valeur = CStr(Round(((valeur + (CSng(Hex(recbuf(9) And &HF)) * 100 + CSng(Hex(recbuf(8))))) * 25.4), 2))
            WriteRetour(adresse & "_RAT", valeur) 'mm
            If (recbuf(4) And &H4) <> 0 Then WriteRetour(adresse & "_RAF", "ERR: battery empty")
            'checksumr()

        ElseIf recbuf(0) = &H1A And recbuf(1) = &H99 And recbits >= 80 Then
            '------------- WTGR800 ---------------
            oregon = True
            adresse = CStr(recbuf(3) * 256)
            valeur = CStr(CSng(recbuf(4) >> 4) * 22.5)
            WriteRetour(adresse & "_WID", valeur) '°
            valeur = wrdirection(direction)
            WriteRetour(adresse & "_WIL", valeur) 'direction en lettres
            valeur = (CSng(Hex(recbuf(7) And &HF)) * 10) + (CSng(Hex(recbuf(6))) / 10)
            WriteRetour(adresse & "_WIS", valeur) 'vitesse en m/s
            valeur = wrspeed(valeur)
            WriteRetour(adresse & "_WIF", valeur) 'vitesse en Force
            ' autre mesure mais je sais pas a quoi ca correpond
            'speed = CSng(Hex(recbuf(8))) + (CSng(Hex((recbuf(7) >> 4) And &HF)) / 10)
            'WriteMessage(" av.", False)
            'wrspeed(speed)
            valeur = wrbattery()
            WriteRetour(adresse & "_BAT", valeur)
            'checksum9()

        ElseIf recbuf(0) = &H1A And recbuf(1) = &H89 And recbits >= 80 Then
            '------------- WGR800 ---------------
            oregon = True
            adresse = CStr(recbuf(3) * 256)
            valeur = CStr(CSng(recbuf(4) >> 4) * 22.5)
            WriteRetour(adresse & "_WID", valeur) '°
            valeur = wrdirection(direction)
            WriteRetour(adresse & "_WIL", valeur) 'direction en lettres
            valeur = (CSng(Hex(recbuf(7) And &HF)) * 10) + (CSng(Hex(recbuf(6))) / 10)
            WriteRetour(adresse & "_WIS", valeur) 'vitesse en m/s
            valeur = wrspeed(valeur)
            WriteRetour(adresse & "_WIF", valeur) 'vitesse en Force
            ' autre mesure mais je sais pas a quoi ca correpond
            'speed = CSng(Hex(recbuf(8))) + (CSng(Hex((recbuf(7) >> 4) And &HF)) / 10)
            'WriteMessage(" av.", False)
            'wrspeed(speed)
            valeur = wrbattery()
            WriteRetour(adresse & "_BAT", valeur)
            'checksum9()

        ElseIf recbuf(0) = &H3A And recbuf(1) = &HD And recbits >= 80 Then
            '------------- STR918,WGR918 ---------------
            oregon = True
            adresse = CStr(recbuf(3) * 256)
            valeur = CStr(CSng(recbuf(4) >> 4) * 22.5)
            WriteRetour(adresse & "_WID", valeur) '°
            valeur = wrdirection(direction)
            WriteRetour(adresse & "_WIL", valeur) 'direction en lettres
            valeur = (CSng(Hex(recbuf(7) And &HF)) * 10) + (CSng(Hex(recbuf(6))) / 10)
            WriteRetour(adresse & "_WIS", valeur) 'vitesse en m/s
            valeur = wrspeed(valeur)
            WriteRetour(adresse & "_WIF", valeur) 'vitesse en Force
            ' autre mesure mais je sais pas a quoi ca correpond
            'speed = CSng(Hex(recbuf(8))) + (CSng(Hex((recbuf(7) >> 4) And &HF)) / 10)
            'WriteMessage(" av.", False)
            'wrspeed(speed)
            valeur = wrbattery()
            WriteRetour(adresse & "_BAT", valeur)
            'checksum9()

        ElseIf recbuf(0) = &HEA And recbuf(1) = &H7C And recbits >= 60 Then
            '------------- UVR138 ---------------
            oregon = True
            adresse = CStr(recbuf(3) * 256)
            uv = CSng(Hex(recbuf(5) And &HF)) * 10 + CSng(Hex(recbuf(4) >> 4))
            WriteRetour(adresse & "_UVV", CStr(uv)) 'en chiffre
            If uv < 3 Then
                WriteRetour(adresse & "_UVL", "Low") 'en level
            ElseIf uv < 6 Then
                WriteRetour(adresse & "_UVL", "Medium") 'en level
            ElseIf uv < 8 Then
                WriteRetour(adresse & "_UVL", "High") 'en level
            ElseIf uv < 11 Then
                WriteRetour(adresse & "_UVL", "Very High") 'en level
            Else
                WriteRetour(adresse & "_UVL", "Dangerous") 'en level
            End If
            If (recbuf(4) And &H4) = &H4 Then WriteRetour(adresse & "_UVV", "ERR: battery empty")
            'checksumw()

        ElseIf recbuf(0) = &HDA And recbuf(1) = &H78 And recbits >= 64 Then
            '------------- UVN800 ---------------
            oregon = True
            adresse = CStr((recbuf(3)) * 256)
            uv = CSng(Hex(recbuf(5) And &HF)) * 10 + CSng(Hex(recbuf(4) >> 4))
            WriteRetour(adresse & "_UVV", CStr(uv)) 'en chiffre
            If uv < 3 Then
                WriteRetour(adresse & "_UVL", "Low") 'en level
            ElseIf uv < 6 Then
                WriteRetour(adresse & "_UVL", "Medium") 'en level
            ElseIf uv < 8 Then
                WriteRetour(adresse & "_UVL", "High") 'en level
            ElseIf uv < 11 Then
                WriteRetour(adresse & "_UVL", "Very High") 'en level
            Else
                WriteRetour(adresse & "_UVL", "Dangerous") 'en level
            End If
            If (recbuf(4) And &H4) = &H4 Then WriteRetour(adresse & "_UVV", "ERR: battery empty")
            'checksum7()

        ElseIf recbuf(0) = &H8A And recbuf(1) = &HEC And recbits >= 96 Then
            '------------- RTGR328N ---------------
            oregon = True
            adresse = CStr(recbuf(3) * 256 + (recbuf(2) >> 4))
            'ch = wrchannel3()
            hr = VB.Right("0" & CStr(CSng(recbuf(7) And &HF) * 10 + CSng(recbuf(6) >> 4)), 2)
            mn = VB.Right("0" & CStr(CSng(recbuf(6) And &HF) * 10 + CSng(recbuf(5) >> 4)), 2)
            sc = VB.Right("0" & CStr(CSng(recbuf(5) And &HF) * 10 + CSng(recbuf(4) >> 4)), 2)
            valeur = " time=" & CStr(hr) & ":" & CStr(mn) & ":" & CStr(sc)
            Select Case recbuf(9) And &H7
                Case 0 : dag = " Sunday"
                Case 1 : dag = " Monday"
                Case 2 : dag = " Tuesday"
                Case 3 : dag = " Wednesday"
                Case 4 : dag = " Thursday"
                Case 5 : dag = " Friday"
                Case 6 : dag = " Saterday"
                Case Else : dag = " day error"
            End Select
            valeur = valeur & dag
            dd = CSng(recbuf(8) And &HF) * 10 + CSng(recbuf(7) >> 4)
            mm = CSng(recbuf(8) >> 4)
            yy = CSng(recbuf(10) And &HF) * 10 + CSng(recbuf(9) >> 4) + 2000
            valeur = valeur & " " & dd & "-" & mm & "-" & yy
            WriteRetour(adresse, valeur)
            'checksum11()

        ElseIf recbuf(0) = &HEA And (recbuf(1) And &HC0) = &H0 And recbits >= 64 Then
            '------------- cent-a-meter ---------------
            oregon = True
            adresse = CStr(recbuf(2) * 256)
            ' WriteMessage(" counter:" & CSng(recbuf(1) And &HF), False)
            valeur = CStr((CSng(recbuf(3)) + CSng((recbuf(4) And &H3) * 256)) / 10)
            WriteRetour(adresse & "_CT1", valeur) 'en Ampere
            valeur = CStr((CSng((recbuf(4) >> 2) And &H3F) + CSng((recbuf(5) And &HF) * 64)) / 10)
            WriteRetour(adresse & "_CT2", valeur) 'en Ampere
            valeur = CStr((CSng((recbuf(5) >> 4) And &HF) + CSng((recbuf(6) And &H3F) * 16)) / 10)
            WriteRetour(adresse & "_CT3", valeur) 'en Ampere
            'checksume()

        ElseIf recbits = 56 Then
            '------------- BWR101,BWR102 ---------------
            oregon = True
            adresse = CStr(recbuf(1) >> 4)
            If IsNumeric(Hex(recbuf(4))) And IsNumeric(Hex(recbuf(3) >> 4)) Then
                'WriteMessage(" addr:" & Hex(recbuf(1) >> 4), False)
                valeur = CStr(CSng(Hex(recbuf(5) And &H1)) * 100 + CSng(Hex(recbuf(4))) + CSng(Hex(recbuf(3) >> 4)) / 10)
                WriteRetour(adresse, valeur) 'en kg
                'WriteMessage(" Unknown byte=" & CStr(Hex(recbuf(3) And &HF)) & CStr(Hex(recbuf(2) >> 4)), False)
                If Not (((recbuf(0) And &HF0) = (recbuf(5) And &HF0)) And ((recbuf(1) And &HF) = (recbuf(6) And &HF))) Then
                    WriteRetour(adresse, "ERR: Checksum error")
                End If
            Else
                WriteRetour(adresse, "ERR: weight value is not a decimal value.")
            End If

        ElseIf (recbuf(0) And &HF) = &H3 And recbits = 64 Then
            '------------- GR101 ---------------
            'Dim i As Integer
            oregon = True
            adresse = CStr(recbuf(1) >> 4)
            'For i = 7 To 0 Step -1
            '    WriteMessage(VB.Right("0" & Hex(recbuf(i)), 2), False)
            'Next
            valeur = CStr(Round((((recbuf(4) And &HF) * 4096) + (recbuf(3) * 16) + (recbuf(2) >> 4) / 400.8), 1))
            WriteRetour(adresse, valeur) 'en kg
        End If
        Return oregon
    End Function

#End Region

#Region "Fonctions"

    Function createhsaddr() As Integer
        Dim hsaddr As Integer = 0
        If (recbuf(0) And &H1) <> 0 Then hsaddr = hsaddr Or &H80
        If (recbuf(0) And &H2) <> 0 Then hsaddr = hsaddr Or &H40
        If (recbuf(0) And &H4) <> 0 Then hsaddr = hsaddr Or &H20
        If (recbuf(0) And &H8) <> 0 Then hsaddr = hsaddr Or &H10
        If (recbuf(0) And &H10) <> 0 Then hsaddr = hsaddr Or &H8
        If (recbuf(0) And &H20) <> 0 Then hsaddr = hsaddr Or &H4
        If (recbuf(0) And &H40) <> 0 Then hsaddr = hsaddr Or &H2
        If (recbuf(0) And &H80) <> 0 Then hsaddr = hsaddr Or &H1
        If (recbuf(1) And &H1) <> 0 Then hsaddr = hsaddr Or &H8000
        If (recbuf(1) And &H2) <> 0 Then hsaddr = hsaddr Or &H4000
        If (recbuf(1) And &H4) <> 0 Then hsaddr = hsaddr Or &H2000
        If (recbuf(1) And &H8) <> 0 Then hsaddr = hsaddr Or &H1000
        If (recbuf(1) And &H10) <> 0 Then hsaddr = hsaddr Or &H800
        If (recbuf(1) And &H20) <> 0 Then hsaddr = hsaddr Or &H400
        If (recbuf(1) And &H40) <> 0 Then hsaddr = hsaddr Or &H200
        If (recbuf(1) And &H80) <> 0 Then hsaddr = hsaddr Or &H100
        Return hsaddr
    End Function

    Public Function wrdirection(ByVal direction As Integer) As String
        If direction > 348.75 Or direction < 11.26 Then
            Return "N"
        ElseIf direction < 33.76 Then
            Return "NNE"
        ElseIf direction < 56.26 Then
            Return "NE"
        ElseIf direction < 78.76 Then
            Return "ENE"
        ElseIf direction < 101.26 Then
            Return "E"
        ElseIf direction < 123.76 Then
            Return "ESE"
        ElseIf direction < 146.26 Then
            Return "SE"
        ElseIf direction < 168.76 Then
            Return "SSE"
        ElseIf direction < 191.26 Then
            Return "S"
        ElseIf direction < 213.76 Then
            Return "SSW"
        ElseIf direction < 236.26 Then
            Return "SW"
        ElseIf direction < 258.76 Then
            Return "WSW"
        ElseIf direction < 281.26 Then
            Return "W"
        ElseIf direction < 303.76 Then
            Return "WNW"
        ElseIf direction < 326.26 Then
            Return "NW"
        Else
            Return "NNW"
        End If
    End Function

    Public Function wrspeed(ByVal speed As Single) As String
        'WriteMessage(" speed " & CStr(speed) & " m/sec", False)
        If speed < 0.2 Then
            Return "0"
        ElseIf speed < 1.6 Then
            Return "1"
        ElseIf speed < 3.4 Then
            Return "2"
        ElseIf speed < 5.5 Then
            Return "3"
        ElseIf speed < 8 Then
            Return "4"
        ElseIf speed < 10.8 Then
            Return "5"
        ElseIf speed < 13.9 Then
            Return "6"
        ElseIf speed < 17.2 Then
            Return "7"
        ElseIf speed < 20.8 Then
            Return "8"
        ElseIf speed < 25.4 Then
            Return "9"
        ElseIf speed < 28.5 Then
            Return "10"
        ElseIf speed < 32.7 Then
            Return "11"
        Else
            Return "12"
        End If
        'WriteMessage("Bft ", False)
    End Function

    Function wrbattery() As String
        Select Case (recbuf(4) And &HF)
            Case 0 : Return "battery 100%"
            Case 1 : Return "battery 90%"
            Case 2 : Return "battery 80%"
            Case 3 : Return "battery 70%"
            Case 4 : Return "battery 60%"
            Case 5 : Return "battery 50%"
            Case 6 : Return "battery 40%"
            Case 7 : Return "battery 30%"
            Case 8 : Return "battery 20%"
            Case 9 : Return "battery 10%"
            Case Else : Return ""
        End Select
    End Function

    Function wrchannel() As Byte
        Select Case (recbuf(2) And &H70)
            Case &H10
                'WriteMessage(" CH 1", False)
                wrchannel = 1
            Case &H20
                'WriteMessage(" CH 2", False)
                wrchannel = 2
            Case &H40
                'WriteMessage(" CH 3", False)
                wrchannel = 4
            Case Else
                ' WriteMessage(" CH ? = " & VB.Right("0" & Hex(recbuf(2)), 2), False)
                wrchannel = 0
        End Select
    End Function

    Function wrchannel3() As Byte
        'WriteMessage(" CH " & (recbuf(2) >> 4), False)
        wrchannel3 = (recbuf(2) >> 4)
    End Function

    Function wrforecast(ByVal forecast As Byte) As String
        Select Case forecast
            Case &HC : Return "Sunny "
            Case &H6 : Return "Partly"
            Case &H2 : Return "Cloudy"
            Case &H3 : Return "Rain"
            Case Else : Return "forecast ??"
        End Select
    End Function

    Function wrhum(ByVal hum As Byte) As String
        Select Case hum
            Case &H0 : Return "Normal"
            Case &H40 : Return "Comfort"
            Case &H80 : Return "Dry"
            Case &HC0 : Return "Wet"
            Case Else : Return ""
        End Select
    End Function

    Function cs8() As Byte
        Dim cs As Byte

        cs = (recbuf(0) >> 4 And &HF) + (recbuf(0) And &HF)
        cs += (recbuf(1) >> 4 And &HF) + (recbuf(1) And &HF)
        cs += (recbuf(2) >> 4 And &HF) + (recbuf(2) And &HF)
        cs += (recbuf(3) >> 4 And &HF) + (recbuf(3) And &HF)
        cs += (recbuf(4) >> 4 And &HF) + (recbuf(4) And &HF)
        cs += (recbuf(5) >> 4 And &HF) + (recbuf(5) And &HF)
        cs += (recbuf(6) >> 4 And &HF) + (recbuf(6) And &HF)
        cs += (recbuf(7) >> 4 And &HF) + (recbuf(7) And &HF)
        Return cs
    End Function

    Sub checksume()
        Dim cs As Short
        cs = (recbuf(0) >> 4 And &HF) + (recbuf(0) And &HF)
        cs += (recbuf(1) >> 4 And &HF) + (recbuf(1) And &HF)
        cs += (recbuf(2) >> 4 And &HF) + (recbuf(2) And &HF)
        cs += (recbuf(3) >> 4 And &HF) + (recbuf(3) And &HF)
        cs += (recbuf(4) >> 4 And &HF) + (recbuf(4) And &HF)
        cs += (recbuf(5) >> 4 And &HF) + (recbuf(5) And &HF)
        cs += (recbuf(6) >> 4 And &HF) + (recbuf(6) And &HF)
        cs = (cs - recbuf(7)) And &HFF
        If cs <> &H18 Then
            'WriteMessage(" Checksum Error", False)
        End If
    End Sub

    Sub checksum7()
        Dim cs As Short
        cs = (recbuf(0) >> 4 And &HF) + (recbuf(0) And &HF)
        cs += (recbuf(1) >> 4 And &HF) + (recbuf(1) And &HF)
        cs += (recbuf(2) >> 4 And &HF) + (recbuf(2) And &HF)
        cs += (recbuf(3) >> 4 And &HF) + (recbuf(3) And &HF)
        cs += (recbuf(4) >> 4 And &HF) + (recbuf(4) And &HF)
        cs += (recbuf(5) >> 4 And &HF) + (recbuf(5) And &HF)
        cs += (recbuf(6) >> 4 And &HF) + (recbuf(6) And &HF)
        cs = (cs - recbuf(7)) And &HFF
        If cs <> &HA Then
            'WriteMessage(" Checksum Error", False)
        End If
    End Sub

    Sub checksum8()
        Dim cs As Short
        cs = cs8()
        cs = (cs - recbuf(8)) And &HFF
        If cs <> &HA Then
            ' WriteMessage(" Checksum Error", False)
        End If
    End Sub

    Sub checksum2()
        Dim cs As Short
        cs = cs8()
        cs += recbuf(8) And &HF
        cs = (cs - ((recbuf(8) >> 4 And &HF) + ((recbuf(9) << 4) And &HF0))) And &HFF
        If cs <> &HA Then
            'WriteMessage(" Checksum Error", False)
        End If
    End Sub

    Sub checksum9()
        Dim cs As Short
        cs = cs8()
        cs += (recbuf(8) >> 4 And &HF) + (recbuf(8) And &HF)
        cs = (cs - recbuf(9)) And &HFF
        If cs <> &HA Then
            'WriteMessage(" Checksum Error", False)
        End If
    End Sub

    Sub checksum10()
        Dim cs As Short
        cs = cs8()
        cs += (recbuf(8) >> 4 And &HF) + (recbuf(8) And &HF)
        cs += (recbuf(9) >> 4 And &HF) + (recbuf(9) And &HF)
        cs = (cs - recbuf(10)) And &HFF
        If cs <> &HA Then
            'WriteMessage(" Checksum Error", False)
        End If
    End Sub

    Sub checksum11()
        Dim cs As Short
        cs = cs8()
        cs += (recbuf(8) >> 4 And &HF) + (recbuf(8) And &HF)
        cs += (recbuf(9) >> 4 And &HF) + (recbuf(9) And &HF)
        cs += (recbuf(10) >> 4 And &HF) + (recbuf(10) And &HF)
        cs = (cs - recbuf(11)) And &HFF
        If cs <> &HA Then
            'WriteMessage(" Checksum Error", False)
        End If
    End Sub

    Function checksumw() As Byte
        Dim cs As Short

        cs = (recbuf(0) >> 4 And &HF) + (recbuf(0) And &HF)
        cs += (recbuf(1) >> 4 And &HF) + (recbuf(1) And &HF)
        cs += (recbuf(2) >> 4 And &HF) + (recbuf(2) And &HF)
        cs += (recbuf(3) >> 4 And &HF) + (recbuf(3) And &HF)
        cs += (recbuf(4) >> 4 And &HF) + (recbuf(4) And &HF)
        cs += (recbuf(5) >> 4 And &HF) + (recbuf(5) And &HF)
        cs += (recbuf(6) And &HF)
        cs = (cs - ((recbuf(6) >> 4 And &HF) + (recbuf(7) << 4 And &HF0))) And &HFF
        If cs <> &HA Then
            'WriteMessage(" Checksum Error", False)
        End If
        Return cs
    End Function

    Function checksumr() As Byte
        Dim cs As Short

        cs = cs8()
        cs += (recbuf(8) >> 4 And &HF) + (recbuf(8) And &HF)
        cs += (recbuf(9) And &HF)
        cs = (cs - ((recbuf(9) >> 4 And &HF) + (recbuf(10) << 4 And &HF0))) And &HFF
        If cs <> &HA Then
            ' WriteMessage(" Checksum Error", False)
        End If
        Return cs
    End Function

#End Region

#Region "Write"

    Public Sub WriteLog(ByVal message As String)
        'utilise la fonction de base pour loguer un event
        If STRGS.InStr(message, "ERR:") > 0 Then
            domos_cmd.log("RFX : " & message, 2)
        Else
            domos_cmd.log("RFX : " & message, 9)
        End If
    End Sub

    Public Sub WriteRetour(ByVal adresse As String, ByVal valeur As String)
        Dim tabletmp() As DataRow
        Dim dateheure, Err As String
        Try
            tabletmp = domos_cmd.table_composants.Select("composants_adresse = '" & adresse.ToString & "' AND composants_modele_norme = 'RFX'")
            If tabletmp.GetUpperBound(0) >= 0 Then
                '--- On attend au moins x seconde entre deux receptions de valeur pour le meme composant (x sec = rfx_tpsentrereponse/100)
                If (DateTime.Now - Date.Parse(tabletmp(0)("composants_etatdate"))).TotalMilliseconds > domos_cmd.rfx_tpsentrereponse Then
                    If VB.Left(valeur, 4) <> "ERR:" Then 'si y a pas erreur d'acquisition
                        '--- Remplacement de , par .
                        valeur = STRGS.Replace(valeur, ",", ".")
                        '--- Correction si besoin ---
                        If (tabletmp(0)("composants_correction") <> "" And tabletmp(0)("composants_correction") <> "0") Then
                            valeur = valeur + CDbl(tabletmp(0)("composants_correction"))
                        End If
                        '--- comparaison du relevé avec le dernier etat ---
                        '--- si la valeur a changé ou (autre chose qu'un nombre et different de humidité) --- 
                        If valeur.ToString <> tabletmp(0)("composants_etat").ToString() Or (Not IsNumeric(valeur) And (VB.Right(tabletmp(0)("composants_adresse"), 4) <> "_HUM")) Then
                            'si nombre alors 
                            If (IsNumeric(valeur) And IsNumeric(tabletmp(0)("lastetat")) And IsNumeric(tabletmp(0)("composants_etat"))) Then
                                'on vérifie que la valeur a changé par rapport a l'avant dernier etat (lastetat) si domos.lastetat (table config)
                                If domos_cmd.lastetat And valeur.ToString = tabletmp(0)("lastetat").ToString() Then
                                    domos_cmd.log("RFX : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString & " (inchangé lastetat " & tabletmp(0)("lastetat").ToString() & ")", 8)
                                    '--- Modification de la date dans la base SQL ---
                                    dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                    Err = domos_cmd.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                    If Err <> "" Then Log("SQL: table_comp_changed " & Err, 2)
                                Else
                                    'on vérifie que la valeur a changé de plus de composants_precision sinon inchangé
                                    'If (valeur + CDbl(tabletmp(0)("composants_precision"))).ToString >= tabletmp(0)("composants_etat").ToString() And (valeur - CDbl(tabletmp(0)("composants_precision"))).ToString <= tabletmp(0)("composants_etat").ToString() Then
                                    If (CDbl(valeur) + CDbl(tabletmp(0)("composants_precision"))) >= CDbl(tabletmp(0)("composants_etat")) And (CDbl(valeur) - CDbl(tabletmp(0)("composants_precision"))) <= CDbl(tabletmp(0)("composants_etat")) Then
                                        'log de "inchangé précision"
                                        domos_cmd.log("RFX : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString & " (inchangé precision " & tabletmp(0)("composants_etat").ToString & "+-" & tabletmp(0)("composants_precision").ToString & ")", 8)
                                        '--- Modification de la date dans la base SQL ---
                                        dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                        Err = domos_cmd.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                        If Err <> "" Then Log("SQL: table_comp_changed " & Err, 2)
                                    Else
                                        domos_cmd.log("RFX : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString, 6)
                                        '  --- modification de l'etat du composant dans la table en memoire ---
                                        tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                                        tabletmp(0)("composants_etat") = valeur.ToString
                                        tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                    End If
                                End If
                            Else
                                domos_cmd.log("RFX : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString, 6)
                                '  --- modification de l'etat du composant dans la table en memoire ---
                                tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                                tabletmp(0)("composants_etat") = valeur.ToString
                                tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                            End If
                            'Domos.log("RFX : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString)
                            ''  --- modification de l'etat du composant dans la table en memoire ---
                            'tabletmp(0)("composants_etat") = valeur.ToString
                            'tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                        Else
                            'la valeur n'a pas changé, on log en 7 et on maj la date dans la base sql
                            domos_cmd.log("RFX : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString & " (inchangé " & tabletmp(0)("composants_etat").ToString() & ")", 7)
                            '--- Modification de la date dans la base SQL ---
                            dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                            Err = domos_cmd.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                            If Err <> "" Then Log("SQL: table_comp_changed " & Err, 2)
                        End If
                    Else
                        'erreur d'acquisition
                        domos_cmd.log("RFX : " & tabletmp(0)("composants_nom").ToString() & " : " & valeur.ToString, 2)
                    End If
                Else
                    'Domos.log("RFX : IGNORE : Etat recu il y a moins de 2 sec : " & adresse.ToString & " : " & valeur.ToString)
                End If
            Else
                'erreur d'adresse composant
                tabletmp = domos_cmd.table_composants_bannis.Select("composants_bannis_adresse = '" & adresse.ToString & "' AND composants_bannis_norme = 'RFX'")
                If tabletmp.GetUpperBound(0) >= 0 Then
                    'on ne logue pas car c'est une adresse bannie
                Else
                    domos_cmd.log("RFX : Adresse composant : " & adresse.ToString & " : " & valeur.ToString, 2)
                End If
            End If
        Catch ex As Exception
            domos_cmd.log("RFX : Writeretour Exception : " & ex.Message, 2)
        End Try
        adresselast = adresse
        valeurlast = valeur
        nblast = 1
        'Else
        'nblast = 2
        'End If
    End Sub

    'Public Sub WriteMessage(ByVal message As String, ByVal linefeed As Boolean)
    '    messagetemp = messagetemp & message
    '    If linefeed Then
    '        If messagetemp <> messagelast And messagetemp <> "" Then
    '            Domos.log("RFX : " & messagetemp)
    '            messagelast = messagetemp
    '        End If
    '        messagetemp = ""
    '    End If
    'End Sub

#End Region

End Class
