'
' La dll ZibaseDll.dll est développée par la société APITRONIC revendeur de la Zibase et de ses accessoires : http://www.planete-domotique.com
'

Imports STRGS = Microsoft.VisualBasic.Strings
Imports VB = Microsoft.VisualBasic
Imports System.Math
Imports System.Threading
Imports System.Globalization
Imports System.ComponentModel
Imports ZibaseDll

Public Class zibasemodule : Implements ISynchronizeInvoke
    Private messagelast, adresselast, valeurlast As String
    Private nblast As Integer = 0
    Public WithEvents zba As New ZibaseDll.ZiBase

    Public Function BeginInvoke(ByVal method As System.Delegate, ByVal args() As Object) As System.IAsyncResult Implements System.ComponentModel.ISynchronizeInvoke.BeginInvoke
        BeginInvoke = Nothing
    End Function
    Public Function EndInvoke(ByVal result As System.IAsyncResult) As Object Implements System.ComponentModel.ISynchronizeInvoke.EndInvoke
        EndInvoke = Nothing
    End Function
    Public Function Invoke(ByVal method As System.Delegate, ByVal args() As Object) As Object Implements System.ComponentModel.ISynchronizeInvoke.Invoke
        Invoke = Nothing
    End Function
    Public ReadOnly Property InvokeRequired() As Boolean Implements System.ComponentModel.ISynchronizeInvoke.InvokeRequired
        Get
            Return False
        End Get
    End Property

#Region "Structure"
    Public Structure SensorInfo
        Dim sName As String
        Dim sType As String
        Dim sID As String
        Dim dwValue As Long
        Dim sValue As String
    End Structure

    Public Enum State
        STATE_OFF = 0
        STATE_ON = 1
        STATE_DIM = 3
    End Enum

    Public Enum Protocol
        PROTOCOL_BROADCAST = 0
        PROTOCOL_VISONIC433 = 1
        PROTOCOL_CHACON = 3
        PROTOCOL_DOMIA = 4
        PROTOCOL_X10 = 5
    End Enum
#End Region

    Public Function lancer() As String
        Try
            zba.StartZB()
            Return "Connexion ouverte"
        Catch ex As Exception
            Return "ERR: lancement de l'initialisation : " & ex.Message
        End Try
    End Function

    Public Function lancer_research() As String
        Try
            zba.RestartZibaseSearch()
            Return "Research OK"
        Catch ex As Exception
            Return "ERR: research : " & ex.Message
        End Try
    End Function

    Public Function fermer() As String
        Try
            zba.StopZB()
            Return "Connexion fermée"
        Catch ex As Exception
            Return "ERR: Fermeture des zibases : " & ex.Message
        End Try
    End Function

    'reception d'une valeur -> analyse
    Private Sub zba_UpdateSensorInfo(ByVal seInfo As ZibaseDll.ZiBase.SensorInfo) Handles zba.UpdateSensorInfo
        'WriteLog("DBG: " & seInfo.sID & "_" & seInfo.sType & " ----> " & seInfo.sValue)
        traitement(seInfo.sID, seInfo.sType, seInfo.dwValue)
    End Sub
    Private Sub zba_NewSensorDetected(ByVal seInfo As ZibaseDll.ZiBase.SensorInfo) Handles zba.NewSensorDetected
        'si on detecte une nouveau device
        'WriteLog("DBG: " & seInfo.sID & "_" & seInfo.sType & " ----> " & seInfo.sValue)
        traitement(seInfo.sID, seInfo.sType, seInfo.dwValue)
    End Sub

    'nouvelle zibase detecté -> Log
    Private Sub zba_newzibasedetected(ByVal ZiInfo As ZibaseDll.ZiBase.ZibaseInfo) Handles zba.NewZibaseDetected
        WriteLog("Nouvelle Zibase détecté : " & ZiInfo.sLabelBase & "-" & ZiInfo.lIpAddress)
    End Sub

    'la zibase a qqch à logger
    Private Sub ZibaseLog(ByVal sMsg As String, ByVal level As Integer) Handles zba.WriteMessage
        WriteLog("DBG: " & sMsg & " - " & level)
    End Sub

    'interroger la zibase sur l'etat d'un device
    Public Function zba_getsensorinfo(ByVal composants_id As Integer) As String
        Dim sei As ZiBase.SensorInfo
        Dim adressetype As String() = Nothing
        Dim tabletmp() As DataRow
        Try
            tabletmp = domos_svc.table_composants.Select("composants_id = '" & composants_id.ToString & "'")
            If tabletmp.GetUpperBound(0) >= 0 Then
                adressetype = Split(tabletmp(0)("composants_addresse"), "_")
                sei = zba.GetSensorInfo(adressetype(0), adressetype(1))
                If STRGS.UCase(sei.sType) = "TEM" Then
                    Return sei.dwValue / 100 'si c'est une temperature on / par 100
                Else
                    Return sei.dwValue
                End If
            Else
                'erreur d'adresse composant
                'tabletmp = domos_svc.table_composants_bannis.Select("composants_bannis_id = '" & composants_id.ToString & "'")
                'If tabletmp.GetUpperBound(0) >= 0 Then
                ''on ne logue pas car c'est une adresse bannie
                '    Return ("")
                'Else
                'Return ("ERR: GetSensorInfo : Composant non trouvé : " & composants_id.ToString)
                'End If
                Return ("ERR: GetSensorInfo : Composant non trouvé : " & composants_id.ToString)

            End If
        Catch ex As Exception
            Return "ERR: GetSensorInfo : " & ex.Message
        End Try
    End Function

    'ecrire device
    Public Function Ecrirecommand(ByVal composants_id As Integer, ByVal ordre As String, ByVal iDim As Integer)
        'composants_id : id du composant
        'ordre : ordre à envoyer
        'iDim: nombre de 0 à 100 pour l'ordre DIM sur chacon
        Dim protocole As ZiBase.Protocol
        Dim adresse(), modele() As String
        Dim tabletmp() As DataRow


        'WriteLog("Ecrirecommand 1")



        Try
            tabletmp = domos_svc.table_composants.Select("composants_id = '" & composants_id.ToString & "'")
            If tabletmp.GetUpperBound(0) >= 0 Then



                'WriteLog("Ecrirecommand 2 first" & composants_id)



                modele = Split(tabletmp(0)("composants_modele_nom"), "_")
                adresse = Split(tabletmp(0)("composants_adresse"), "_")
                Select Case UCase(modele(0))
                    Case "BROADC" : protocole = ZiBase.Protocol.PROTOCOL_BROADCAST
                    Case "CHACON" : protocole = ZiBase.Protocol.PROTOCOL_CHACON
                    Case "DOMIA" : protocole = ZiBase.Protocol.PROTOCOL_DOMIA
                    Case "VIS433" : protocole = ZiBase.Protocol.PROTOCOL_VISONIC433
                    Case "VIS868" : protocole = ZiBase.Protocol.PROTOCOL_VISONIC868
                    Case "X10" : protocole = ZiBase.Protocol.PROTOCOL_X10
                    Case Else : Return ("ERR: protocole incorrect : " & modele(0))
                End Select



                ' WriteLog("Ecrirecommand 2" & protocole.ToString & "-" & adresse(0) & "-" & modele(0))



                'ecriture sur la zibase
                Select Case UCase(ordre)
                    Case "ON"
                        zba.SendCommand(adresse(0), ZiBase.State.STATE_ON, 0, protocole, 1)
                    Case "OFF"
                        zba.SendCommand(adresse(0), ZiBase.State.STATE_OFF, 0, protocole, 1)
                    Case "DIM"
                        If UCase(adresse(1)) <> "CHACON" Then
                            zba.SendCommand(adresse(0), ZiBase.State.STATE_DIM, 0, protocole, 1)
                        Else
                            zba.SendCommand(adresse(0), ZiBase.State.STATE_DIM, iDim, protocole, 1)
                        End If
                    Case Else
                        Return ("ERR: ordre incorrect : " & ordre)
                End Select


                'WriteLog("Ecrirecommand 3")


                'modification de l'etat en memoire
                tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                tabletmp(0)("composants_etat") = ordre & "" & iDim
                tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                Return (" -> ecrit " & adresse(0) & " -> " & ordre)
            Else
                'erreur d'adresse composant
                'tabletmp = domos_svc.table_composants_bannis.Select("composants_bannis_id = '" & composants_id.ToString & "'")
                'If tabletmp.GetUpperBound(0) >= 0 Then
                '    'on ne logue pas car c'est une adresse bannie
                'Else
                '    Return ("ERR: Composant non trouvé : " & composants_id.ToString & " : " & ordre)
                'End If
                Return ("ERR: Composant non trouvé : " & composants_id.ToString & " : " & ordre)
            End If

            'Return " -> zibase write Composants_id: " & composants_id & " -> " & ordre
        Catch ex As Exception
            Return "ERR: Ecrirecommand" & ex.Message
        End Try

    End Function

    'executer un script stocké sur la zibase
    Public Function ExecScript(ByVal sScript As String)
        'sScript : nom du script sur la zibase
        Try
            zba.ExecScript(sScript)
            Return " -> Executé : " & sScript
        Catch ex As Exception
            Return "Err: ExecScript: " & ex.Message
        End Try
    End Function

    'executer un scénario stocké sur la zibase
    Public Function RunScenario(ByVal sCmd As String)
        'sCmd : nom du scenario sur la zibase
        Try
            zba.RunScenario(sCmd)
            Return " -> Executé : " & sCmd
        Catch ex As Exception
            Return "Err: RunScenario: " & ex.Message
        End Try
    End Function

#Region "Write"

    Public Sub traitement(ByVal adresse As String, ByVal type As String, ByVal valeur As String)

        'modification des informations suivant le type
        Select Case type
            Case "tem"
                'valeur = STRGS.Left(valeur, (valeur.Length - 2))
                valeur = valeur / 100
                type = "THE" 'tem Température (°C)
                'Case "hum"
                'valeur = STRGS.Left(valeur, (valeur.Length - 1))
            Case "temc"
                'valeur = STRGS.Left(valeur, (valeur.Length - 2))
                valeur = valeur / 100
                type = "THC" 'Température de consigne (Thermostat : °C)
        End Select

        'Action suivant le type
        Select Case type
            Case "bat" : If STRGS.UCase(valeur) = "LOW" Then WriteRetour(adresse & "_THE", "ERR: battery empty") 'Niveau de batterie (Ok / Low)
            Case "lev" 'on envoie rien : Niveau de réception RF (1 à 5)
            Case "lnk" : WriteLog("DBG: Etat de la connexion avec la Zibase " & adresse & " : " & valeur) 'Etat de la connexion Zibase
            Case "" : WriteRetour(adresse, valeur) ' si pas de type particulier
            Case Else
                'tem Température (°C)
                'temc Température de consigne (Thermostat : °C)
                'hum Humidité (%)
                'Lnk Etat de la connexion Zibase
                'kwh Mesure d’énergie totale (CM119)
                'Kw Mesure d’énergie instantanée (CM119)
                'tra Niveau de pluie total (Total Rain)
                'cra Niveau de pluie courant (Currant Rain)
                'awi Mesure de la vitesse du vent
                'drt Direction du vent
                'uvl Niveau d’UV
                'sta Status pour un switch (ON/OFF)
                WriteRetour(adresse & "_" & STRGS.UCase(type), valeur)
        End Select

    End Sub

    Public Sub WriteLog(ByVal message As String)
        'utilise la fonction de base pour loguer un event
        If STRGS.InStr(message, "DBG:") > 0 Then
            domos_svc.log("ZIB : " & message, 10)
        ElseIf STRGS.InStr(message, "ERR:") > 0 Then
            domos_svc.log("ZIB : " & message, 2)
        Else
            domos_svc.log("ZIB : " & message, 9)
        End If
    End Sub

    Public Sub WriteRetour(ByVal adresse As String, ByVal valeur As String)
        'Forcer le . 
        Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
        My.Application.ChangeCulture("en-US")

        Dim tabletmp() As DataRow
        Dim dateheure, Err As String
        Try
            tabletmp = domos_svc.table_composants.Select("composants_adresse = '" & adresse.ToString & "' AND composants_modele_norme = 'ZIB'")
            If tabletmp.GetUpperBound(0) >= 0 Then
                '--- On attend au moins x seconde entre deux receptions de valeur pour le meme composant (x sec = rfx_tpsentrereponse/100)
                If (DateTime.Now - Date.Parse(tabletmp(0)("composants_etatdate"))).TotalMilliseconds > domos_svc.rfx_tpsentrereponse Then
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
                                If domos_svc.lastetat And valeur.ToString = tabletmp(0)("lastetat").ToString() Then
                                    domos_svc.log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString & " (inchangé lastetat " & tabletmp(0)("lastetat").ToString() & ")", 8)
                                    '--- Modification de la date dans la base SQL ---
                                    dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                    Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                    If Err <> "" Then Log("SQL: table_comp_changed " & Err, 2)
                                Else
                                    'on vérifie que la valeur a changé de plus de composants_precision sinon inchangé
                                    If (CDbl(valeur) + CDbl(tabletmp(0)("composants_precision"))) >= CDbl(tabletmp(0)("composants_etat")) And (CDbl(valeur) - CDbl(tabletmp(0)("composants_precision"))) <= CDbl(tabletmp(0)("composants_etat")) Then
                                        'log de "inchangé précision"
                                        domos_svc.log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString & " (inchangé precision " & tabletmp(0)("composants_etat").ToString & "+-" & tabletmp(0)("composants_precision").ToString & ")", 8)
                                        '--- Modification de la date dans la base SQL ---
                                        dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                        Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                        If Err <> "" Then Log("SQL: table_comp_changed " & Err, 2)
                                    Else
                                        domos_svc.log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString, 6)
                                        '  --- modification de l'etat du composant dans la table en memoire ---
                                        tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                                        tabletmp(0)("composants_etat") = valeur.ToString
                                        tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                    End If
                                End If
                            Else
                                domos_svc.log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString, 6)
                                '  --- modification de l'etat du composant dans la table en memoire ---
                                If VB.Left(valeur, 4) <> "CFG:" Then
                                    tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                                    tabletmp(0)("composants_etat") = valeur.ToString
                                    tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                End If
                            End If
                            'Domos.log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString)
                            ''  --- modification de l'etat du composant dans la table en memoire ---
                            'tabletmp(0)("composants_etat") = valeur.ToString
                            'tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                        Else
                            'la valeur n'a pas changé, on log en 7 et on maj la date dans la base sql
                            domos_svc.log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString & " (inchangé " & tabletmp(0)("composants_etat").ToString() & ")", 7)
                            '--- Modification de la date dans la base SQL ---
                            dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                            Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                            If Err <> "" Then WriteLog("ERR: SQL: table_comp_changed " & Err)
                        End If
                    Else
                        'erreur d'acquisition
                        WriteLog("ERR: " & tabletmp(0)("composants_nom").ToString() & " : " & valeur.ToString)
                    End If
                Else
                    WriteLog("DBG: IGNORE : Etat recu il y a moins de 2 sec : " & adresse.ToString & " : " & valeur.ToString)
                End If
            Else
                'erreur d'adresse composant
                tabletmp = domos_svc.table_composants_bannis.Select("composants_bannis_adresse = '" & adresse.ToString & "' AND composants_bannis_norme = 'ZIB'")
                If tabletmp.GetUpperBound(0) >= 0 Then
                    'on ne logue pas car c'est une adresse bannie
                    WriteLog("DBG: IGNORE : Adresse Bannie : " & adresse.ToString & " : " & valeur.ToString)
                Else
                    WriteLog("ERR: Adresse composant : " & adresse.ToString & " : " & valeur.ToString)
                End If
            End If

        Catch ex As Exception
            WriteLog("ERR: Writeretour Exception : " & ex.Message)
        End Try
        adresselast = adresse
        valeurlast = valeur
        nblast = 1

    End Sub

#End Region

End Class
