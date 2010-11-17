Imports STRGS = Microsoft.VisualBasic.Strings
Imports System.Threading
Imports System.Timers
Imports System.IO
Imports System.Globalization
Imports Microsoft.Win32
Imports System.ServiceProcess
Imports System.Net.Mail

Public Class domos_svc
    'declaration de mes classes_
    Shared onewire As New onewire
    Shared onewire2 As New onewire
    Shared socket As New sockets
    Public Shared mysql As New mysql
    Shared rfxcom As New rfxcom
    Shared plcbus As New plcbus
    Shared x10 As New x10
    Shared zibase As New zibasemodule
    Private shared lock_logfile As New Object
    Private shared lock_tablethread As New Object

    'variable interne au script
    Public Shared table_config, table_composants, table_composants_bannis, table_macros, table_timer, table_thread, table_erreur As New DataTable
    Public Shared Serv_DOMOS, Serv_WIR, Serv_WI2, Serv_PLC, Serv_X10, Serv_RFX, Serv_ZIB, Serv_TSK, Serv_SOC As Boolean
    Public Shared Port_PLC, Port_X10, Port_RFX, Port_WIR, Port_WI2, socket_ip, WIR_adaptername As String
    Public Shared PLC_timeout, X10_timeout, Action_timeout, rfx_tpsentrereponse, socket_port, lastetat, WIR_res As Integer
    Public Shared WIR_timeout, ZIB_timeout, TSK_timeout, heure_coucher_correction, heure_lever_correction As Integer
    Public Shared logs_erreur_nb, logs_erreur_duree As Integer
    Public Shared gps_longitude, gps_latitude, mail_smtp, mail_from, mail_to As String
    Public Shared mail_action as integer
    Dim WithEvents timer_pool, timer_timer As New System.Timers.Timer
    Dim err As String = ""
    Public Shared log_niveau As String
    Public Shared parametrevb
    Public Shared log_dest As Integer
    Private mysql_ip, mysql_db, mysql_login, mysql_mdp As String
    Private Shared install_dir As String
    Private controller As New ServiceController("DOMOS", ".")
    Public Shared etape_startup As Integer

    'Variables specifiques
    Private soleil = New Soleil
    Public Shared var_soleil_lever As Date = DateAndTime.Now.ToString("yyyy-MM-dd") & " 07:00:00" 'heure du lever du soleil par defaut
    Public Shared var_soleil_coucher As Date = DateAndTime.Now.ToString("yyyy-MM-dd") & " 21:00:00" 'heure de coucher du soleil par defaut
    Public Shared var_soleil_lever2 As Date = DateAndTime.Now.ToString("yyyy-MM-dd") & " 07:00:00" 'heure du lever du soleil par defaut
    Public Shared var_soleil_coucher2 As Date = DateAndTime.Now.ToString("yyyy-MM-dd") & " 21:00:00" 'heure de coucher du soleil par defaut

    Protected Overrides Sub OnStart(ByVal args() As String)
        'Forcer le . 
        Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
        My.Application.ChangeCulture("en-US")

        '---------- Creation table des threads ----------
        table_thread.Dispose()
        Dim x As New DataColumn
        x.ColumnName = "composant_id"
        table_thread.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "source"
        table_thread.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "norme"
        table_thread.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "datetime"
        table_thread.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "thread"
        table_thread.Columns.Add(x)

        '---------- Creation table des erreurs ----------
        table_erreur.Dispose()
        x = New DataColumn
        x.ColumnName = "texte"
        table_erreur.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "nombre"
        table_erreur.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "datetime"
        table_erreur.Columns.Add(x)
        
        '---- initialisation des variables par défaut ----
        Serv_WIR = False
        Serv_WI2 = False
        Serv_PLC = False
        Serv_X10 = False
        Serv_RFX = False
        Serv_ZIB = False
        Serv_TSK = False
        Serv_DOMOS = False
        Serv_SOC = True
        log_niveau = "0-1-2-3-4-5-6-7-8-9-A-B-C-D-E-F" 'log tous les msgs
        log_dest = 2 '0=txt, 1=sql, 2=txt+sql
        X10_timeout = 500
        PLC_timeout = 500
        WIR_timeout = 500
        ZIB_timeout = 500
        TSK_timeout = 500
        Action_timeout = 500
        rfx_tpsentrereponse = 1500
        lastetat = 1
        WIR_res = 0.1
        WIR_adaptername = ""
        heure_coucher_correction = 0
        heure_lever_correction = 0
        gps_longitude = 0
        gps_latitude = 0
        etape_startup = 0
        logs_erreur_nb = 0
        logs_erreur_duree = 60
		mail_action = 0
		
        svc_start()
    End Sub

    Protected Overrides Sub OnStop()
        svc_stop()
    End Sub

    Protected Overrides Sub OnCustomCommand(ByVal command As Integer)
        If command = 200 Then
            'EventLog.WriteEntry("Custom Command 200 : SQL-optimise")
            log("Custom Command 200 : SQL-optimise", -1)
            action("([AN#SQL#optimise])")
        ElseIf command = 201 Then
            log("Custom Command 201 : SQL-purgelogs", -1)
            action("([AN#SQL#purgelogs])")
        ElseIf command = 202 Then
            log("Custom Command 202 : SQL-reconnect", -1)
            action("([AN#SQL#reconnect])")
        ElseIf command = 210 Then
            log("Custom Command 210 : AFFICHE-tables", -1)
            action("([AS#afftables])")
        ElseIf command = 211 Then
            log("Custom Command 211 : MAJ-tables", -1)
            action("([AS#maj_all])")
        ElseIf command = 212 Then
            log("Custom Command 212 : MAJ-table_composants", -1)
            action("([AS#maj_composants])")
        ElseIf command = 213 Then
            log("Custom Command 213 : MAJ-table_composants_bannis", -1)
            action("([AS#maj_composants_bannis])")
        ElseIf command = 214 Then
            log("Custom Command 214 : MAJ-table_macros", -1)
            action("([AS#maj_macro])")
        ElseIf command = 215 Then
            log("Custom Command 215 : MAJ-table_timer", -1)
            action("([AS#maj_timer])")
        End If
    End Sub

    Private Sub svc_start()
        Dim resumemail, texte As String
        Try
            'Forcer le . 
            Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
            My.Application.ChangeCulture("en-US")

            log("-------------------------------------------------------------------------------", 0)
            Dim x As New DataColumn

            '---------- Récupération configuration Registry ----------
            etape_startup = 1
            resumemail = "Récupération configuration Registry"
            Dim regKey, regKey2 As RegistryKey
            regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE")
            If regKey Is Nothing Then
                log("Service arreté car erreur lecture registre HKLM\SOFTWARE", 1)
                [Stop]()
                'controller.Stop()
                Exit Sub
            Else
                regKey2 = regKey.OpenSubKey("Domos")
                If regKey2 Is Nothing Then
                    log("Service arreté car erreur lecture registre HKLM\SOFTWARE\Domos", 1)
                    [Stop]()
                    Exit Sub
                End If
            End If
            mysql_ip = regKey2.GetValue("mysql_ip", "127.0.0.1")
            mysql_db = regKey2.GetValue("mysql_db", "domos")
            mysql_login = regKey2.GetValue("mysql_login", "domos")
            mysql_mdp = regKey2.GetValue("mysql_mdp", "domos")
            install_dir = regKey2.GetValue("install_dir", "C:\Domos\")
            regKey2.Close()
            regKey.Close()

            'mysql_ip = My.Settings.mysql_ip
            'mysql_db = My.Settings.mysql_db
            'mysql_login = My.Settings.mysql_login
            'mysql_mdp = My.Settings.mysql_mdp

            '---------- Connexion MySQL ----------
            etape_startup = 2
            resumemail = resumemail & Chr(10) & "Connexion au serveur Mysql : " & mysql_ip & "-" & mysql_db & " " & mysql_login & ":" & mysql_mdp
            err = mysql.mysql_connect(mysql_ip, mysql_db, mysql_login, mysql_mdp)
            If err <> "" Then
                log("", 1)
                log("--- Démarrage du Service ---", 1)
                log("SQL : Connexion au serveur " & mysql_ip & " :", 1)
                log("      -> " & err, 1)
                log("--- Service Arrété ---", 1)
                [Stop]()
                Exit Sub
            End If
            log("", 0)
            log("--- Démarrage du Service ---", 0)
            log("--- Démarrage du Service ---", -1)
            log("", 0)
            log("SQL : Connexion au serveur " & mysql_ip & " :", 0)
            log("      -> Connecté à " & mysql_ip & ":" & mysql_db & " avec " & mysql_login & "/" & mysql_mdp, 0)
            log("SQL : Connecté à " & mysql_ip & ":" & mysql_db & " avec " & mysql_login & "/" & mysql_mdp, -1)
            resumemail = resumemail & Chr(10) & " -> connecté"
            log("", 0)

            '----- recupération de la config -----
            etape_startup = 3
            log("SQL : Récupération de la configuration :", 0)
            log("SQL : Récupération de la configuration", -1)
            resumemail = resumemail & Chr(10) & "Récupération de la configuration"
            err = Table_maj_sql(table_config, "SELECT config_nom,config_valeur FROM config")
            If table_config.Rows.Count() > 0 Then
                log_niveau = table_config.Select("config_nom = 'log_niveau'")(0)("config_valeur")
                log_dest = table_config.Select("config_nom = 'log_dest'")(0)("config_valeur")
                Serv_WIR = table_config.Select("config_nom = 'Serv_WIR'")(0)("config_valeur")
                Serv_WI2 = table_config.Select("config_nom = 'Serv_WI2'")(0)("config_valeur")
                Serv_PLC = table_config.Select("config_nom = 'Serv_PLC'")(0)("config_valeur")
                Serv_X10 = table_config.Select("config_nom = 'Serv_X10'")(0)("config_valeur")
                Serv_RFX = table_config.Select("config_nom = 'Serv_RFX'")(0)("config_valeur")
                Serv_ZIB = table_config.Select("config_nom = 'Serv_ZIB'")(0)("config_valeur")
                Serv_TSK = table_config.Select("config_nom = 'Serv_TSK'")(0)("config_valeur")
                Port_RFX = table_config.Select("config_nom = 'Port_RFX'")(0)("config_valeur")
                Port_X10 = table_config.Select("config_nom = 'Port_X10'")(0)("config_valeur")
                Port_PLC = table_config.Select("config_nom = 'Port_PLC'")(0)("config_valeur")
                Port_WIR = table_config.Select("config_nom = 'Port_WIR'")(0)("config_valeur")
                Port_WI2 = table_config.Select("config_nom = 'Port_WI2'")(0)("config_valeur")
                X10_timeout = table_config.Select("config_nom = 'X10_timeout'")(0)("config_valeur")
                PLC_timeout = table_config.Select("config_nom = 'PLC_timeout'")(0)("config_valeur")
                WIR_timeout = table_config.Select("config_nom = 'WIR_timeout'")(0)("config_valeur")
                ZIB_timeout = table_config.Select("config_nom = 'ZIB_timeout'")(0)("config_valeur")
                TSK_timeout = table_config.Select("config_nom = 'TSK_timeout'")(0)("config_valeur")
                Action_timeout = table_config.Select("config_nom = 'Action_timeout'")(0)("config_valeur")
                rfx_tpsentrereponse = table_config.Select("config_nom = 'rfx_tpsentrereponse'")(0)("config_valeur")
                socket_ip = table_config.Select("config_nom = 'socket_ip'")(0)("config_valeur")
                socket_port = table_config.Select("config_nom = 'socket_port'")(0)("config_valeur")
                lastetat = table_config.Select("config_nom = 'lastetat'")(0)("config_valeur")
                WIR_res = table_config.Select("config_nom = 'WIR_res'")(0)("config_valeur")
                WIR_adaptername = table_config.Select("config_nom = 'WIR_adaptername'")(0)("config_valeur")
                heure_lever_correction = table_config.Select("config_nom = 'heure_lever_correction'")(0)("config_valeur")
                heure_coucher_correction = table_config.Select("config_nom = 'heure_coucher_correction'")(0)("config_valeur")
                gps_longitude = table_config.Select("config_nom = 'gps_longitude'")(0)("config_valeur")
                gps_latitude = table_config.Select("config_nom = 'gps_latitude'")(0)("config_valeur")
                mail_smtp = table_config.Select("config_nom = 'mail_smtp'")(0)("config_valeur")
                mail_from = table_config.Select("config_nom = 'mail_from'")(0)("config_valeur")
                mail_to = table_config.Select("config_nom = 'mail_to'")(0)("config_valeur")
                mail_action = table_config.Select("config_nom = 'mail_action'")(0)("config_valeur")
                logs_erreur_nb = table_config.Select("config_nom = 'logs_erreur_nb'")(0)("config_valeur")
                logs_erreur_duree = table_config.Select("config_nom = 'logs_erreur_duree'")(0)("config_valeur")
                log("      -> LOG_NIVEAU=" & log_niveau & " LOG_DESTINATION=" & log_dest, 0)
                log("      -> WIR=" & Serv_WIR & " WI2=" & Serv_WI2 & " PLC=" & Serv_PLC & ":" & Port_PLC & " X10=" & Serv_X10 & ":" & Port_X10, 0)
                log("      -> ZIB=" & Serv_ZIB & " TSK=" & Serv_TSK & " RFX=" & Serv_RFX & ":" & Port_RFX, 0)
                log("      -> Action_timeout=" & Action_timeout & " PLC_timeout=" & PLC_timeout & " X10_timeout=" & X10_timeout & " WIR_timeout=" & WIR_timeout & " ZIB_timeout=" & ZIB_timeout & " TSK_timeout=" & TSK_timeout, 0)
                log("      -> RFX_tpsentrereponse=" & rfx_tpsentrereponse & " Lastetat=" & lastetat, 0)
                log("      -> heure_lever_correction=" & heure_lever_correction & " heure_coucher_correction=" & heure_coucher_correction, 0)
                log("      -> longitude=" & gps_longitude & " latitude=" & gps_latitude, 0)
                log("      -> Mail smtp=" & mail_smtp & " From=" & mail_from & " To=" & mail_to & " Action=" & mail_action, 0)
                log("      -> WIR_res=" & WIR_res & " WIR_adaptername=" & WIR_adaptername, 0)
                log("      -> Socket Activé=" & Serv_SOC & " IP=" & socket_ip & " Port=" & socket_port, 0)
                log("      -> Logs erreur NB=" & logs_erreur_nb & " Logs erreur Duree=" & logs_erreur_duree, 0)
            Else
                log("      -> ERR: pas de données récupérées : fermeture du programme", 1)
                'svc_stop()
                [Stop]()
                Exit Sub
            End If
            log("", 0)

            '---------- Initialisation de la clé USB 1-wire -------
            etape_startup = 4
            If Serv_WIR Then
                resumemail = resumemail & Chr(10) & "Initialisation 1-wire"
                err = onewire.initialisation(WIR_adaptername, Port_WIR)
                If STRGS.Left(err, 4) = "ERR:" Then
                    Serv_WIR = False 'desactivation du onewire car la clé USB n'est pas dispo
                    log("WIR : " & err, 2)
                    log("      -> Désactivation du servive OneWire", 0)
                Else
                    log("WIR : " & err, 0)
                End If
                log("WIR : " & err, -1)
            End If
            '---------- Initialisation de la clé USB 1-wire 2 -------
            etape_startup = 5
            If Serv_WI2 Then
                resumemail = resumemail & Chr(10) & "Initialisation 1-wire 2"
                err = onewire2.initialisation(WIR_adaptername, Port_WI2)
                If STRGS.Left(err, 4) = "ERR:" Then
                    Serv_WI2 = False 'desactivation du onewire car la clé USB n'est pas dispo
                    log("WI2 : " & err, 2)
                    log("      -> Désactivation du servive OneWire", 0)
                Else
                    log("WI2 : " & err, 0)
                End If
                log("WI2 : " & err, -1)
            End If
            '---------- Initialisation du RFXCOM -------
            etape_startup = 6
            If Serv_RFX Then
                resumemail = resumemail & Chr(10) & "Initialisation RFXCOM"
                err = rfxcom.ouvrir(Port_RFX)
                If STRGS.Left(err, 4) = "ERR:" Then
                    Serv_RFX = False 'desactivation du RFXCOM car erreur d'ouverture
                    log("RFX : " & err, 2)
                    log("      -> Désactivation du servive RFXCOM", 0)
                Else
                    log("RFX : " & err, 0)
                End If
                log("RFX : " & err, -1)
            End If
            '---------- Initialisation du PLCBUS -------
            etape_startup = 7
            If Serv_PLC Then
                resumemail = resumemail & Chr(10) & "Initialisation PLC"
                err = plcbus.ouvrir(Port_PLC)
                If STRGS.Left(err, 4) = "ERR:" Then
                    Serv_PLC = False 'desactivation du PLCBUS car erreur d'ouverture
                    log("PLC : " & err, 2)
                    log("      -> Désactivation du servive PLCBUS", 0)
                Else
                    log("PLC : " & err, 0)
                End If
                log("PLC : " & err, -1)
            End If
            log("", 0)

            '---------- Initialisation du X10 -------
            etape_startup = 8
            If Serv_X10 Then
                resumemail = resumemail & Chr(10) & "Initialisation X10"
                err = x10.ouvrir(Port_X10)
                If STRGS.Left(err, 4) = "ERR:" Then
                    Serv_X10 = False 'desactivation du X10 car erreur d'ouverture
                    log("X10 : " & err, 2)
                    log("      -> Désactivation du servive X10", 0)
                Else
                    log("X10 : " & err, 0)
                End If
                log("X10 : " & err, -1)
            End If
            log("", 0)

            '----- recupération de la liste des composants actifs -----
            etape_startup = 20
            resumemail = resumemail & Chr(10) & "Récupération liste des composants"
            log("SQL : Récupération de la liste des composants actifs :", 0)
            log("SQL : Récupération de la liste des composants actifs", -1)
            Dim Condition_service As String = ""
            If Not Serv_PLC Then Condition_service &= " AND composants_modele_norme<>'PLC'"
            If Not Serv_WIR Then Condition_service &= " AND composants_modele_norme<>'WIR'"
            If Not Serv_WI2 Then Condition_service &= " AND composants_modele_norme<>'WI2'"
            If Not Serv_X10 Then Condition_service &= " AND composants_modele_norme<>'X10'"
            If Not Serv_RFX Then Condition_service &= " AND composants_modele_norme<>'RFX'"
            If Not Serv_ZIB Then Condition_service &= " AND composants_modele_norme<>'ZIB'"
            If Not Serv_TSK Then Condition_service &= " AND composants_modele_norme<>'TSK'"
            err = Table_maj_sql(table_composants, "SELECT composants.*,composants_modele.* FROM composants,composants_modele WHERE composants_modele=composants_modele_id AND composants_actif='1'" & Condition_service)
            If err <> "" Then
                log("SQL : " & err, 1)
                log("--- Fermeture du programme ---", 1)
                'svc_stop()
                [Stop]()
                Exit Sub
            Else
                If table_composants.Rows.Count() > 0 Then
                    'ajout dune colonne pour le timer et le dernier etat
                    x = New DataColumn
                    x.ColumnName = "timer"
                    table_composants.Columns.Add(x)
                    x = New DataColumn
                    x.ColumnName = "lastetat"
                    table_composants.Columns.Add(x)
                    For i = 0 To table_composants.Rows.Count() - 1
                        Dim date_pooling As Date
                        date_pooling = DateAndTime.Now.AddSeconds(5 + i) 'on initilise le timer à dans 5+i secondes
                        table_composants.Rows(i).Item("timer") = date_pooling.ToString("yyyy-MM-dd HH:mm:ss")
                        table_composants.Rows(i).Item("lastetat") = table_composants.Rows(i).Item("composants_etat")
                        '--- Remplacement de , par .
                        table_composants.Rows(i).Item("composants_etat") = STRGS.Replace(table_composants.Rows(i).Item("composants_etat"), ",", ".")
                    Next
                    'affichage
                    For i = 0 To table_composants.Rows.Count() - 1
                        texte = "     -> Id : " & table_composants.Rows(i).Item("composants_id") & " -- Nom : " & table_composants.Rows(i).Item("composants_nom") & " -- Adresse : " & table_composants.Rows(i).Item("composants_adresse") & " -- Valeur : " & table_composants.Rows(i).Item("composants_etat") & " -- Polling : " & table_composants.Rows(i).Item("composants_polling") & " -- Type : " & table_composants.Rows(i).Item("composants_modele_norme") & "-" & table_composants.Rows(i).Item("composants_modele_nom")
                        resumemail = resumemail & Chr(10) & texte
                        log(texte, 0)
                    Next
                Else
                    log("      -> Aucun composant trouvé : fermeture du programme", 1)
                    resumemail = resumemail & Chr(10) & "     -> Aucun composant trouvé"
                    'svc_stop()
                    [Stop]()
                    Exit Sub
                End If
            End If

            '---------- Initialisation des heures du soleil -------
            etape_startup = 21
            resumemail = resumemail & Chr(10) & Chr(10) & "Initialisation des heures du soleil"
            log("SOL : Initialisation des heures du soleil", 0)
            'soleil.City("Algrange")
            soleil.City_gps(gps_longitude, gps_latitude)
            soleil.CalculateSun()
            var_soleil_lever = soleil.Sunrise
            var_soleil_coucher = soleil.Sunset
            var_soleil_coucher2 = DateAdd(DateInterval.Minute, heure_coucher_correction, var_soleil_coucher)
            var_soleil_lever2 = DateAdd(DateInterval.Minute, heure_lever_correction, var_soleil_lever)
            resumemail = resumemail & Chr(10) & "     -> Heure du lever : " & var_soleil_lever & " (" & var_soleil_lever2 & ")"
            resumemail = resumemail & Chr(10) & "     -> Heure du coucher : " & var_soleil_coucher & " (" & var_soleil_coucher2 & ")"
            log("      -> Heure du lever : " & var_soleil_lever & " (" & var_soleil_lever2 & ")", 0)
            log("      -> Heure du coucher : " & var_soleil_coucher & " (" & var_soleil_coucher2 & ")", 0)
            log("Initialisation des heures du soleil : " & var_soleil_lever & " (" & var_soleil_lever2 & ") - " & var_soleil_coucher & " (" & var_soleil_coucher2 & ")", -1)
            '---------- Calcul pour savoir si on est de jour ou nuit -------
            Dim tabletemp = table_composants.Select("composants_adresse = 'jour'")
            Dim dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
            If tabletemp.GetLength(0) = 1 Then
                If DateAndTime.Now.ToString("HH:mm:ss") > var_soleil_lever.ToString("HH:mm:ss") And DateAndTime.Now.ToString("HH:mm:ss") < var_soleil_coucher.ToString("HH:mm:ss") Then
                    tabletemp(0)("composants_etat") = "1" 'maj du composant virtuel JOUR à jour
                    err = mysql.mysql_nonquery("UPDATE composants SET composants_etat='1',composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletemp(0)("composants_id") & "'")
                    If err <> "" Then log("SQL: Start Maj Jour " & err, 2)
                    log("      -> Maj composant virtuel JOUR : JOUR (1)", 0)
                Else
                    tabletemp(0)("composants_etat") = "0" 'maj du composant virtuel JOUR à nuit
                    err = mysql.mysql_nonquery("UPDATE composants SET composants_etat='0',composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletemp(0)("composants_id") & "'")
                    If err <> "" Then log("SQL: Start Maj Jour " & err, 2)
                    log("      -> Maj composant virtuel JOUR : NUIT (0)", 0)
                End If
            Else
                log("      -> ERR: Maj composant virtuel JOUR : Non trouvé", 1)
            End If
            '---------- Calcul pour savoir si on est de jour ou nuit avec heures corrigés-------
            tabletemp = table_composants.Select("composants_adresse = 'jour2'")
            If tabletemp.GetLength(0) = 1 Then
                If DateAndTime.Now.ToString("HH:mm:ss") > var_soleil_lever2.ToString("HH:mm:ss") And DateAndTime.Now.ToString("HH:mm:ss") < var_soleil_coucher2.ToString("HH:mm:ss") Then
                    tabletemp(0)("composants_etat") = "1" 'maj du composant virtuel JOUR2 à jour
                    err = mysql.mysql_nonquery("UPDATE composants SET composants_etat='1',composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletemp(0)("composants_id") & "'")
                    If err <> "" Then log("SQL: Start Maj Jour " & err, 2)
                    log("      -> Maj composant virtuel JOUR2 : JOUR (1)", 0)
                Else
                    tabletemp(0)("composants_etat") = "0" 'maj du composant virtuel JOUR2 à nuit
                    err = mysql.mysql_nonquery("UPDATE composants SET composants_etat='0',composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletemp(0)("composants_id") & "'")
                    If err <> "" Then log("SQL: Start Maj Jour " & err, 2)
                    log("      -> Maj composant virtuel JOUR2 : NUIT (0)", 0)
                End If
            Else
                log("      -> ERR: Maj composant virtuel JOUR : Non trouvé", 1)
            End If
            log("", 0)


            '---------- Ajout d'un handler sur la modification de l'etat d'un composant -------
            etape_startup = 22
            resumemail = resumemail & Chr(10) & "Lancement Handler sur la table composants"
            log("DOM : Lancement de l'handler sur l etat des composants", 0)
            AddHandler table_composants.ColumnChanged, New DataColumnChangeEventHandler(AddressOf table_composants_changed)
            log("", 0)

            '----- recupération de la liste des composants bannis -----
            log("SQL : Récupération de la liste des composants bannis :", 0)
            log("SQL : Récupération de la liste des composants bannis", -1)
            resumemail = resumemail & Chr(10) & "Récupération de la liste des composants bannis"
            err = Table_maj_sql(table_composants_bannis, "SELECT * FROM composants_bannis")
            If err <> "" Then
                log("SQL : " & err, 2)
            Else
                If table_composants_bannis.Rows.Count() > 0 Then
                    'affichage de la liste des composants bannis
                    For i = 0 To table_composants_bannis.Rows.Count() - 1
                        texte = "      -> Id : " & table_composants_bannis.Rows(i).Item("composants_bannis_id") & " -- Norme : " & table_composants_bannis.Rows(i).Item("composants_bannis_norme") & " -- Adresse : " & table_composants_bannis.Rows(i).Item("composants_bannis_adresse") & " -- Description : " & table_composants_bannis.Rows(i).Item("composants_bannis_description")
                        resumemail = resumemail & Chr(10) & texte
                        log(texte, 0)
                    Next
                Else
                    log("      -> Aucun composant banni trouvé", 0)
                    resumemail = resumemail & Chr(10) & "     -> Aucun composant banni trouvé"
                End If
            End If
            log("", 0)

            '----- recupération de la liste des macros -----
            etape_startup = 23
            log("SQL : Récupération de la liste des macros :", 0)
            log("SQL : Récupération de la liste des macros", -1)
            resumemail = resumemail & Chr(10) & Chr(10) & "Récupération de la liste des macros"
            err = Table_maj_sql(table_macros, "SELECT * FROM macro WHERE macro_actif='1' AND macro_conditions NOT LIKE '%CT#%'")
            If err <> "" Then
                log("SQL : " & err, 2)
            Else
                x = New DataColumn
                x.ColumnName = "verrou"
                table_macros.Columns.Add(x)
                If table_macros.Rows.Count() > 0 Then
                    'affichage de la liste des macros
                    For i = 0 To table_macros.Rows.Count() - 1
                        table_macros.Rows(i).Item("verrou") = False
                        texte = "      -> Id : " & table_macros.Rows(i).Item("macro_id") & " -- Nom : " & table_macros.Rows(i).Item("macro_nom") & " -- Condition : " & table_macros.Rows(i).Item("macro_conditions") & " -- Action : " & table_macros.Rows(i).Item("macro_actions")
                        resumemail = resumemail & Chr(10) & texte
                        log(texte, 0)
                    Next
                Else
                    log("      -> Aucune macro trouvée", 0)
                    resumemail = resumemail & Chr(10) & "     -> Aucune macro trouvée"
                End If
            End If
            log("", 0)

            '----- recupération de la liste des timers -----
            etape_startup = 24
            log("SQL : Récupération de la liste des timers :", 0)
            log("SQL : Récupération de la liste des timers", -1)
            resumemail = resumemail & Chr(10) & Chr(10) & "Récupération de la liste des timers"
            err = Table_maj_sql(table_timer, "SELECT * FROM macro WHERE macro_actif='1' AND macro_conditions LIKE '%CT#%'")
            If err <> "" Then
                log("SQL : " & err, 2)
            Else
                x = New DataColumn
                x.ColumnName = "timer"
                table_timer.Columns.Add(x)
                If table_timer.Rows.Count() > 0 Then
                    'affichage de la liste des timers
                    For i = 0 To table_timer.Rows.Count() - 1
                        table_timer.Rows(i).Item("timer") = timer_convertendate(table_timer.Rows(i).Item("macro_conditions"))
                        texte = "      -> Id : " & table_timer.Rows(i).Item("macro_id") & " -- Nom : " & table_timer.Rows(i).Item("macro_nom") & " -- Condition : " & table_timer.Rows(i).Item("macro_conditions") & " -- Action : " & table_timer.Rows(i).Item("macro_actions") & " -- Timer : " & table_timer.Rows(i).Item("timer")
                        resumemail = resumemail & Chr(10) & texte
                        log(texte, 0)
                    Next
                Else
                    log("      -> Aucun timer trouvé", 0)
                    resumemail = resumemail & Chr(10) & "     -> Aucun timer trouvé"
            End If
                log("", 0)
            End If

            '---------- RFXCOM : Lancement du Handler et paramétrage -------
            etape_startup = 25
            If Serv_RFX Then
                log("RFX : Lancement et paramétrage du RFXCOM", 0)
                log("RFX : Lancement et paramétrage du RFXCOM", -1)
                resumemail = resumemail & Chr(10) & Chr(10) & "Lancement et paramétrage du RFXCOM"
                log("     -> " & rfxcom.lancer(), 0)
                ' ENALL / DISARC / DISKOP / DISX10 / DISHE / DISOREGON / DISATI / DISVIS / DISSOMFY
                err = rfxcom.ecrire(&HF0, rfxcom.MODEVAR)
                If STRGS.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err, 0)
                err = rfxcom.ecrire(&HF0, rfxcom.ENALL)
                If STRGS.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err, 0)
                'err = rfxcom.ecrire(&HF0, rfxcom.DISOREGON)
                'If STR.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err, 0)
                err = rfxcom.ecrire(&HF0, rfxcom.DISATI)
                If STRGS.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err, 0)
                err = rfxcom.ecrire(&HF0, rfxcom.DISSOMFY)
                If STRGS.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err, 0)
                err = rfxcom.ecrire(&HF0, rfxcom.DISVIS)
                If STRGS.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err, 0)
                'err = rfxcom.ecrire(&HF0, rfxcom.DISHE)
                'If STRGS.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err, 0)
                err = rfxcom.ecrire(&HF0, rfxcom.DISKOP)
                If STRGS.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err, 0)
                err = rfxcom.ecrire(&HF0, rfxcom.DISARC)
                If STRGS.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err, 0)
                'err = rfxcom.ecrire(&HF0, rfxcom.SWVERS)
                'If STR.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err)
                log("", 0)
            End If

            '---------- Lancement de la zibase -------
            etape_startup = 26
            If Serv_ZIB Then
                resumemail = resumemail & Chr(10) & "Lancement de la Zibase"
                err = zibase.lancer()
                If STRGS.Left(err, 4) = "ERR:" Then
                    Serv_ZIB = False 'desactivation du ZIB car erreur de lancement
                    log("ZIB : " & err, 2)
                    log("     -> Désactivation du servive ZIB", 0)
                Else
                    log("ZIB : " & err, 0)
                End If
                log("ZIB : " & err, -1)
                log("", 0)
            End If

            '---------- Initialisation du Socket -------
            etape_startup = 30
            If Serv_SOC Then
                resumemail = resumemail & Chr(10) & "Initialisation du socket"
                err = socket.ouvrir(socket_ip, socket_port)
                If STRGS.Left(err, 4) = "ERR:" Then
                    Serv_SOC = False
                    log("SOC : " & err, 2)
                    log("     -> Désactivation du servive SOCKET", 0)
                Else
                    log("SOC : " & err, 0)
                End If
                log("SOC : " & err, -1)
                log("", 0)
            End If

            '---------- Lancement du pooling -------
            etape_startup = 31
            log("DOM : Lancement du Pool", 0)
            log("Lancement du Pool", -1)
            resumemail = resumemail & Chr(10) & "Lancement du pool"
            timer_pool = New System.Timers.Timer
            AddHandler timer_pool.Elapsed, AddressOf pool
            timer_pool.Interval = 1000
            timer_pool.Start()
            '---------- Lancement du timer -------
            etape_startup = 32
            log("DOM : Lancement du Timer", 0)
            log("Lancement du Timer", -1)
            resumemail = resumemail & Chr(10) & "Lancement du Timer"
            timer_timer = New System.Timers.Timer
            AddHandler timer_timer.Elapsed, AddressOf timer
            timer_timer.Interval = 1000
            timer_timer.Start()

            'le service est démarré
            etape_startup = 99
            Serv_DOMOS = True
            log("", 0)
            log("--- Service démarré ---", 0)
            log("--- Service démarré ---", -1)
            resumemail = resumemail & Chr(10) & "Service démarré !"
            SendMessage("Service started", "Domos Service a été démarré à " & Date.Now.ToString("yyyy-MM-dd HH:mm:ss") & Chr(10) & Chr(10) & Chr(10) & resumemail, 1)
            log("", 0)
        Catch ex As Exception
            log("ERR: Init exception : Fermeture du programme : " & ex.ToString, 1)
            'svc_stop()
            [Stop]()
            Exit Sub
        End Try
    End Sub

    Private Sub svc_stop()
        Try
            Dim resumemail As String
            log_niveau = 0
            log("", 0)
            log("--- Arret du Service ---", 0)
            log("--- Arret du Service ---", -1)
            resumemail = ""
            log("", 0)

            '---------- arret du timer -------
            If etape_startup > 32 Then
                log("DOM : Arret du Timer :", 0)
                log("Arret du Timer", -1)
                resumemail = resumemail & Chr(10) & "Arret du timer"
                timer_timer.Stop()
                timer_timer.Dispose()
                log("      -> Arrété", 0)
            End If
            '---------- arret du pool -------
            If etape_startup > 31 Then
                log("DOM : Arret du Pool :", 0)
                log("Arret du Pool", -1)
                resumemail = resumemail & Chr(10) & "Arret du pool"
                timer_pool.Stop()
                timer_pool.Dispose()
                log("      -> Arrété", 0)
            End If

            '---------- liberation du Socket -------
            If etape_startup > 30 Then
                If Serv_SOC Then
                    log("SOC : Fermeture de la connexion : ", 0)
                    resumemail = resumemail & Chr(10) & "SOC : Fermeture de la connexion"
                    err = socket.fermer()
                    If STRGS.Left(err, 4) = "ERR:" Then
                        log("      -> SOC Fermeture : " & err, 2)
                    Else
                        log("      -> SOC Fermeture : " & err, 0)
                    End If
                    log("SOC : Fermeture : " & err, -1)
                End If
            End If

            '---------- attente fin des threads : 5 secondes -------
            log("DOM : Attente fin des threads :", 0)
            log("DOM : Attente fin des threads", -1)
            resumemail = resumemail & Chr(10) & "Attente fin des threads"
            Dim i As Integer = 0
            While table_thread.Rows.Count() > 0 And i < 10
                Try
                    wait(50)
                    i = i + 1
                Catch ex As Exception
                End Try
            End While
            If i = 6 Then
                table_thread.Reset()
                log("      -> OK (Time out)", 0)
            Else
                log("      -> OK", 0)
            End If

            '---------- liberation du 1-wire -------
            If etape_startup > 4 Then
                If Serv_WIR Then
                    log("WIR : Fermeture de la clé USB : ", 0)
                    resumemail = resumemail & Chr(10) & "WIR : Fermeture de la clé USB"
                    err = onewire.close()
                    If STRGS.Left(err, 4) = "ERR:" Then
                        log("      -> WIR Fermeture : " & err, 2)
                    Else
                        log("      -> WIR Fermeture : " & err, 0)
                    End If
                    log("WIR : Fermeture : " & err, -1)
                End If
            End If

            '---------- liberation du 1-wire 2 -------
            If etape_startup > 5 Then
                If Serv_WI2 Then
                    log("WI2 : Fermeture de la clé USB : ", 0)
                    resumemail = resumemail & Chr(10) & "WI2 : Fermeture de la clé USB"
                    err = onewire2.close()
                    If STRGS.Left(err, 4) = "ERR:" Then
                        log("      -> WI2 Fermeture : " & err, 2)
                    Else
                        log("      -> WI2 Fermeture : " & err, 0)
                    End If
                    log("WI2 : Fermeture : " & err, -1)
                End If
            End If

            '---------- liberation du RFXCOM -------
            If etape_startup > 6 Then
                If Serv_RFX Then
                    log("RFX : Fermeture de la connexion : ", 0)
                    resumemail = resumemail & Chr(10) & "RFX : Fermeture de la connexion"
                    err = rfxcom.fermer()
                    If STRGS.Left(err, 4) = "ERR:" Then
                        log("      -> RFX Fermeture : " & err, 2)
                    Else
                        log("      -> RFX Fermeture : " & err, 0)
                    End If
                    log("RFX : Fermeture : " & err, -1)
                End If
            End If

            '---------- liberation du PLCBUS -------
            If etape_startup > 7 Then
                If Serv_PLC Then
                    log("PLC : Fermeture de la connexion : ", 0)
                    resumemail = resumemail & Chr(10) & "PLC : Fermeture de la connexion"
                    err = plcbus.fermer()
                    If STRGS.Left(err, 4) = "ERR:" Then
                        log("      -> PLC Fermeture : " & err, 2)
                    Else
                        log("      -> PLC Fermeture : " & err, 0)
                    End If
                    log("PLC : Fermeture : " & err, -1)
                End If
            End If

            '---------- liberation du X10 -------
            If etape_startup > 8 Then
                If Serv_X10 Then
                    log("X10 : Fermeture de la connexion : ", 0)
                    resumemail = resumemail & Chr(10) & "X10 : Fermeture de la connexion"
                    err = x10.fermer()
                    If STRGS.Left(err, 4) = "ERR:" Then
                        log("      -> X10 Fermeture : " & err, 2)
                    Else
                        log("      -> X10 Fermeture : " & err, 0)
                    End If
                    log("X10 : Fermeture : " & err, -1)
                End If
            End If

            '---------- liberation de la zibase -------
            If etape_startup > 26 Then
                If Serv_ZIB Then
                    log("ZIB : Fermeture de la connexion : ", 0)
                    resumemail = resumemail & Chr(10) & "ZIB : Fermeture de la connexion"
                    err = zibase.fermer()
                    If STRGS.Left(err, 4) = "ERR:" Then
                        log("      -> ZIB Fermeture : " & err, 2)
                    Else
                        log("      -> ZIB Fermeture : " & err, 0)
                    End If
                    log("ZIB : Fermeture : " & err, -1)
                End If
            End If

            '---------- deconnexion mysql -------
            If etape_startup > 2 Then
                log("SQL : Déconnexion du serveur", 0)
                resumemail = resumemail & Chr(10) & "SQL : Fermeture de la connexion"
                log("", 0)
                err = mysql.mysql_close()
                log("SQL : Déconnexion du serveur " & err, -1)
                If err <> "" Then
                    log("     -> ERR: " & err, 2)
                    Exit Sub
                End If
                log("     -> Déconnecté", 0)
            End If

            'le service est arreté
            Serv_DOMOS = False
            etape_startup = 0
            log("", 0)
            resumemail = resumemail & Chr(10) & Chr(10) & "Service arrété"
            log("--- Service arrété ---", 0)
            log("--- Service arrété ---", -1)
            SendMessage("Service stopped", "Domos Service a été arrêté à " & Date.Now.ToString("yyyy-MM-dd HH:mm:ss") & Chr(10) & Chr(10) & Chr(10) & resumemail, 1)
            log("", 0)
        Catch ex As Exception
            log("ERR: Close exception : " & ex.ToString, 1)
        End Try
    End Sub

    Private Sub svc_restart()
        svc_stop()
        'wait(500)
        svc_start()
    End Sub

    Public Shared Sub log(ByVal texte As String, ByVal niveau As String)
        'log les infos dans un fichier texte et/ou sql
        ' texte : string contenant le texte à logger
        ' niveau : int contenant le type de log
        '   -2 : uniquement mail
        '   -1 : uniquement pour les eventslogs
        '    0 : Programme : Lancement / Arrêt / redémarrage...
        '    1 : Erreurs critiques : erreurs faisant ou pouvant planter le programme ou bloquant le fonctionnement
        '    2 : Erreurs générales : erreurs de base : composant non trouvé...
        '    3 : Messages reçues
        '    4 : Lancement d'une macro/timer
        '    5 : Actions d'une macro/timer
        '    6 : Valeurs de composant ayant changé
        '    7 : Valeurs de composant n'ayant pas changé (inchangé)
        '    8 : Valeurs de composant ayant changé mais < à précision (inchangé precision)
        '    9 : Divers
        '   10 : DEBUG

        Dim dateheure As String = ""
        Dim fichierlog As String = ""
        Dim textemodifie As String = ""
        Dim erreur_log As Integer = 1
        Dim tabletemp() As DataRow

        Try
        	If not (texte Is Nothing) Then
	            texte = texte.Replace("'", "")
	            'si on doit loguer une erreur et pas en mode debug et config > 0
	            ' : gestion de la table des erreurs 
	            If niveau = "2" And STRGS.InStr("-10", niveau) <= 0 and logs_erreur_nb>0 and logs_erreur_duree>0 Then
	                textemodifie = Left(texte, texte.Length - 4)
	                'si c'est une erreur générale
	                tabletemp = table_erreur.Select("texte = '" & textemodifie & "'")
	                If tabletemp.GetLength(0) >= 1 Then
	                    'on a déjà une erreur de ce type en memoire
	                    If Date.Now.ToString("yyyy-MM-dd HH:mm:ss") > tabletemp(0)("datetime").ToString Then
	                        'ca fait au moins x minutes qu'on a eu cette erreur, on supprime
	                        tabletemp(0).Item("datetime") = DateAdd(DateInterval.Minute, logs_erreur_duree, DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")
	                        tabletemp(0).Item("nombre") = 1
	                        'tabletemp(0).Delete()
	                    ElseIf CInt(tabletemp(0).Item("nombre").ToString) >= logs_erreur_nb Then
	                        'ca fait moins de x minutes et + de logs_erreur_nb fois, on ne logue pas
	                        tabletemp(0).Item("nombre") = CInt(tabletemp(0).Item("nombre").ToString) + 1
	                        erreur_log = 0
	                    Else
	                        'ca fait moins de x minutes et - de logs_erreur_nb fois, on logue (repeat)
	                        tabletemp(0).Item("nombre") = CInt(tabletemp(0).Item("nombre").ToString) + 1
	                        If CInt(tabletemp(0).Item("nombre").ToString) < logs_erreur_nb Then
	                            texte = texte & " (" & tabletemp(0).Item("nombre") & "x)"
	                        Else
	                            texte = texte & " (" & tabletemp(0).Item("nombre") & "x -> erreur multiple, on ne logue plus jusqu'à " & tabletemp(0)("datetime").ToString & ")"
	                            'envoi d'un mail car erreur redondante
	                            SendMessage("Erreur redondante", texte, 3)
	                        End If
	                    End If
	                Else
	                    'cet erreur n'est pas encore présente, on l'ajoute
	                    Dim newRow As DataRow
	                    newRow = table_erreur.NewRow()
	                    newRow.Item("texte") = textemodifie
	                    newRow.Item("nombre") = 1
	                    newRow.Item("datetime") = DateAdd(DateInterval.Minute, logs_erreur_duree, DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")
	                    table_erreur.Rows.Add(newRow)
	                End If
	            End If
	        End If
        Catch ex As Exception
        	try
	            ecrireevtlog("LOG: Exception Table_erreur : " & ex.ToString & " --> " & texte, 1, 109)
                SendMessage("LOG: Exception Table_erreur", ex.ToString & " --> " & texte, 4)
	            'si erreur de corruption : Réinitialisation de la gestion de la table des erreurs
	            If STRGS.InStr("DataTable internal index is corrupted", ex.ToString) > 0 then
					'logs_erreur_nb=0
					table_erreur.dispose()
					Dim x As New DataColumn
					x = New DataColumn
					x.ColumnName = "texte"
					table_erreur.Columns.Add(x)
					x = New DataColumn
					x.ColumnName = "nombre"
					table_erreur.Columns.Add(x)
					x = New DataColumn
					x.ColumnName = "datetime"
					table_erreur.Columns.Add(x)
					log("LOG : Error DataTable internal index is corrupted --> Réinitialisation de la table des erreurs",2)
					SendMessage("LOG: ERR: DataTable internal index is corrupted", "LOG : Error DataTable internal index is corrupted --> Réinitialisation de la table des erreurs",2)
	            End If
            Catch ex2 As Exception
                ecrireevtlog("LOG: Exception Table_erreur Exception : " & ex2.ToString, 1, 109)
	        End Try
        End Try
        
        try
            'si on peut loguer ou si c'est un debug
            If erreur_log = 1 and not (texte Is Nothing) Then
                If niveau <> "-1" And niveau <> "-2" And STRGS.InStr("-" & log_niveau, niveau) > 0 Then
                    fichierlog = install_dir & "logs\log_" & DateAndTime.Now.ToString("yyyyMMdd") & ".txt"
                    dateheure = DateAndTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    If Not Directory.Exists(install_dir & "logs") Then
                        Directory.CreateDirectory(install_dir & "logs")
                    End If
                    If log_dest = 0 Or log_dest = 2 Then
                        domos_svc.EcrireFichier(fichierlog, dateheure & " " & niveau & " " & texte & Environment.NewLine())
                    End If
                    If (log_dest = 1 Or log_dest = 2) And mysql.connected Then
                        texte = STRGS.Replace(texte, "'", "&#39;")
                        texte = STRGS.Replace(texte, """", "&quot;")
                        mysql.mysql_nonquery("INSERT INTO logs(logs_source,logs_description,logs_date) VALUES('" & niveau & "', '" & texte & "', '" & dateheure & "')")
                    End If
                End If
                'Log dans les events logs / mail
                Select Case niveau
                    Case "-2" : SendMessage("Message", texte, 1)
                    Case "-1" : ecrireevtlog(texte, 3, 100)
                    Case "1"
                        ecrireevtlog(texte, 1, 101)
                        SendMessage("Erreur critique", texte, 2)
                    Case "2" : SendMessage("Erreur", texte, 4)
                    Case "3" : ecrireevtlog(texte, 3, 103)
                    Case "4" : ecrireevtlog(texte, 3, 104)
                End Select
            End If
        Catch ex As Exception
            ecrireevtlog("LOG: Exception : " & ex.ToString & " --> " & texte, 1, 109)
            SendMessage("LOG: Exception", ex.ToString & " --> " & texte, 2)
        End Try
    End Sub

    Private Shared Sub ecrireevtlog(ByVal message As String, ByVal type As Integer, ByVal evtid As Integer)
        'message = text to write to event log
        'type = 1:error, 2:warning, 3:information 
        Dim myEventLog = New EventLog()
        Try
            myEventLog.Source = "Domos"
            Select Case type
                Case 1 : myEventLog.WriteEntry(message, EventLogEntryType.Error, evtid)
                Case 2 : myEventLog.WriteEntry(message, EventLogEntryType.Warning, evtid)
                Case 3 : myEventLog.WriteEntry(message, EventLogEntryType.Information, evtid)
            End Select
            'Diagnostics.EventLog.WriteEntry("Domos", message)
        Catch ex As Exception
            log("LOG : EcrireEvtLog Execption : " & ex.ToString, 2)
        End Try
    End Sub

    Private Shared Sub EcrireFichier(ByVal CheminFichier As String, ByVal Texte As String)
        'ecrit texte dans le fichier CheminFichier
        Dim FreeF As Integer
        Try
            FreeF = FreeFile()
            Texte = Replace(Texte, vbLf, vbCrLf)
            SyncLock lock_logfile
            	FileOpen(FreeF, CheminFichier, OpenMode.Append)
            	Print(FreeF, Texte)
            	FileClose(FreeF)
            End SyncLock
        Catch ex As IOException
            wait(500)
            EcrireFichier(CheminFichier, Texte)
        Catch ex As Exception
            wait(500)
            log("LOG : EcrireFichier Exception : " & Texte & " ::: " & ex.ToString, 2)
        End Try
    End Sub

    Public Shared Sub SendMessage(ByVal subject As String, ByVal messageBody As String, ByVal niveau As Integer)
    	'niveau : 
    	'0=manuel
    	'1=auto par Domos
    	'2=erreur critique
    	'3=erreur redondantes
    	'4=erreurs
    
    	'mail_action :
    	'0=desactive 
    	'1=manuel 
    	'2=manuel-auto 
    	'3=manuel-auto-erreurcritique 
    	'4=manuel-auto-erreurcritique-erreursredondante 
    	'5=manuel-auto-erreurs
    	
        'envoi un email
        Dim message As New MailMessage()
        Dim client As New SmtpClient()
        dim envoiemailautorise as boolean=true
        Try
        	'on verifie si on peut envoyer un mail
        	If mail_from = "" or mail_smtp = "" or mail_to = "" Then envoiemailautorise = false
        	If mail_action = 0 then envoiemailautorise = false
        	If mail_action = 1 and niveau > 0 then envoiemailautorise = false
        	If mail_action = 2 and niveau > 1 then envoiemailautorise = false
        	If mail_action = 3 and niveau > 2 then envoiemailautorise = false
        	If mail_action = 4 and niveau > 3 then envoiemailautorise = false
        	If mail_action = 5 and niveau = 3 then envoiemailautorise = false 'car une erreur redondante est déjà envoyé voir fonction log
        	
            If envoiemailautorise Then
                Dim fromAddress As String = mail_from
                Dim toAddress As String = mail_to
                Dim ccAddress As String = ""
                Dim smtpserver As String = mail_smtp

                'Set the sender's address
                message.From = New MailAddress(fromAddress)

                'Allow multiple "To" addresses to be separated by a semi-colon
                If (toAddress.Trim.Length > 0) Then
                    For Each addr As String In toAddress.Split(";"c)
                        message.To.Add(New MailAddress(addr))
                    Next
                End If

                'Allow multiple "Cc" addresses to be separated by a semi-colon
                If (ccAddress.Trim.Length > 0) Then
                    For Each addr As String In ccAddress.Split(";"c)
                        message.CC.Add(New MailAddress(addr))
                    Next
                End If

                'Set the subject and message body text
                message.Subject = "[DOMOS] " & subject
                message.Body = messageBody

                'Set the SMTP server to be used to send the message
                client.Host = smtpserver

                'Send the e-mail message
                client.Send(message)
            End If
        Catch ex As Exception
            log("LOG : Send_message Execption : " & ex.ToString, 2)
        End Try
    End Sub

    Public Shared Sub wait(ByVal msec As Integer)
        '100msec = 1 secondes
        Try
            Dim ticks = Date.Now.Ticks + (msec * 100000) '10000000 = 1 secondes
            Dim limite = 0
            While limite = 0
                If ticks <= Date.Now.Ticks Then limite = 1
            End While
        Catch ex As Exception
            log("DOM : Wait Execption : " & ex.ToString, 2)
        End Try
    End Sub

    Private Function Table_maj_sql(ByRef table As DataTable, ByVal requete As String) As String
        'maj d une table avec les resultats de la requete
        Try
            table.Clear()
            Table_maj_sql = mysql.mysql_query(table, requete)
        Catch ex As Exception
            'Table_maj_sql = Nothing
            Table_maj_sql = ""
            log("DOM : Table_maj_sql Exception : " & ex.ToString, 2)
        End Try
    End Function

    Private Function timer_convertendate(ByVal condition As String) As String
        'convertit un timer ([CT#=#ss#mm#hh#jj#MMM#JJJ]) en dateTime
        try
	        If STRGS.Len(condition) >= 16 Then
	            condition = STRGS.Mid(condition, 8, STRGS.Len(condition) - 7 - 2) 'on supprimer les ([CT#=# ]) de chaque cote de la liste
	            Dim conditions = STRGS.Split(condition, "#")
	
	            'Dim s = CrontabSchedule.Parse("0 17-19 * * *")
	            Dim s = CrontabSchedule.Parse(conditions(1) & " " & conditions(2) & " " & conditions(3) & " " & conditions(4) & " " & conditions(5))
	            'recupere le prochain shedule
	            Dim nextcron = s.GetNextOccurrence(DateAndTime.Now)
	            If (conditions(0) <> "*" And conditions(0) <> "") Then nextcron = nextcron.AddSeconds(conditions(0))
	            Return nextcron.ToString("yyyy-MM-dd HH:mm:ss")
	
	            'recupere la liste des prochains shedule
	            'Dim nextcron = s.GetNextOccurrences(DateAndTime.Now, DateAndTime.Now.AddDays(1))
	            'For Each i In nextcron
	            '    MsgBox(i.ToString("yyyy/MM/dd HH:mm:ss"))
	            'Next
	        End If
	        Return ""
        Catch ex As Exception
            log("DOM : Table_maj_sql Exception : " & ex.ToString, 2)
            Return ""
        End Try
    End Function

    Private Sub tables_maj(ByVal table As String)
        'fonction permettant de rafraichir le contenu des tables en memoire depuis les tables mysql
        'table = ALL / COMPOSANTS / COMPOSANTS_BANNIS / MACRO / TIMER
        Dim table_temp As New DataTable
        Dim result
        Dim x As New DataColumn

        'Maj de la table composant
        If table = "ALL" Or table = "COMPOSANTS" Then
            Try
                log("MAJ : Maj de la table_composants", 0)
                Dim Condition_service As String = ""
                If Not Serv_PLC Then Condition_service &= " AND composants_modele_norme<>'PLC'"
                If Not Serv_WIR Then Condition_service &= " AND composants_modele_norme<>'WIR'"
                If Not Serv_WI2 Then Condition_service &= " AND composants_modele_norme<>'WI2'"
                If Not Serv_X10 Then Condition_service &= " AND composants_modele_norme<>'X10'"
                If Not Serv_RFX Then Condition_service &= " AND composants_modele_norme<>'RFX'"
                If Not Serv_ZIB Then Condition_service &= " AND composants_modele_norme<>'ZIB'"
                If Not Serv_TSK Then Condition_service &= " AND composants_modele_norme<>'TSK'"
                err = Table_maj_sql(table_temp, "SELECT composants.*,composants_modele.* FROM composants,composants_modele WHERE composants_modele=composants_modele_id AND composants_actif='1'" & Condition_service)
                If err <> "" Then
                    log("MAJ : SQL ERR: " & err, 0)
                Else
                    If table_temp.Rows.Count() > 0 Then 'on a récupéré la nouvelle liste des composants, on fait la maj
                        'Maj et suppression des composants
                        If table_composants.Rows.Count() > 0 Then
                            For i = table_composants.Rows.Count() - 1 To 0 Step -1
                                result = table_temp.Select("composants_id = " & table_composants.Rows(i).Item("composants_id"))
                                If result.GetLength(0) = 0 Then 'le composant n'existe plus --> DELETE
                                    table_composants.Rows.Remove(table_composants.Rows(i))
                                ElseIf result.GetLength(0) = 1 Then 'le composant existe --> MAJ
                                    For j = 0 To table_composants.Columns.Count - 1
                                        'si la colonne n'est pas l'etat, le dernier etat ou timer --> maj
                                        If table_composants.Columns(j).Caption <> "timer" And table_composants.Columns(j).Caption <> "lastetat" And table_composants.Columns(j).Caption <> "composants_etat" Then
                                            table_composants.Rows(i).Item(j) = result(0)(j)
                                        End If
                                    Next
                                End If
                            Next
                        End If
                        'ajout dune colonne pour le timer
                        x = New DataColumn
                        x.ColumnName = "timer"
                        table_temp.Columns.Add(x)
                        x = New DataColumn
                        x.ColumnName = "lastetat"
                        table_temp.Columns.Add(x)
                        'ajout des nouveaux composants
                        For i = 0 To table_temp.Rows.Count() - 1
                            result = table_composants.Select("composants_id = " & table_temp.Rows(i).Item("composants_id"))
                            If result.GetLength(0) = 0 Then 'le composant n'est pas dans la table_composants, on l'ajoute
                                'creation de la dateheure du timer
                                Dim date_pooling As Date
                                date_pooling = DateAndTime.Now.AddSeconds(5 + i) 'on initilise le timer à dans 5+i secondes
                                table_temp.Rows(i).Item("composants_etat") = STRGS.Replace(table_temp.Rows(i).Item("composants_etat"), ",", ".")
                                table_temp.Rows(i).Item("timer") = date_pooling.ToString("yyyy-MM-dd HH:mm:ss")
                                table_temp.Rows(i).Item("lastetat") = table_temp.Rows(i).Item("composants_etat")
                                'ajout
                                table_composants.ImportRow(table_temp.Rows(i))
                            End If
                        Next
                        'affichage de la nouvelle liste
                        For i = 0 To table_composants.Rows.Count() - 1
                            log("      -> Id : " & table_composants.Rows(i).Item("composants_id") & " -- Nom : " & table_composants.Rows(i).Item("composants_nom") & " -- Adresse : " & table_composants.Rows(i).Item("composants_adresse") & " -- Valeur : " & table_composants.Rows(i).Item("composants_etat") & " -- Polling : " & table_composants.Rows(i).Item("composants_polling") & " -- Type : " & table_composants.Rows(i).Item("composants_modele_norme") & "-" & table_composants.Rows(i).Item("composants_modele_nom"), 0)
                        Next
                    Else
                        log("      -> Pas de composant trouvé !", 0)
                    End If
                End If
            Catch ex As Exception
                log("MAJ : composants error : " & ex.Message, 0)
            End Try
        End If

        'Maj de la table composant bannis
        If table = "ALL" Or table = "COMPOSANTS_BANNIS" Then
            Try
                log("MAJ : Maj de la table_composants bannis", 0)
                err = Table_maj_sql(table_temp, "SELECT * FROM composants_bannis")
                If err <> "" Then
                    log("MAJ : SQL ERR: " & err, 0)
                Else
                    If table_temp.Rows.Count() > 0 Then 'on a récupéré la nouvelle liste des composants bannis, on fait la maj
                        'Maj et suppression des composants bannis
                        If table_composants_bannis.Rows.Count() > 0 Then
                            For i = table_composants_bannis.Rows.Count() - 1 To 0 Step -1
                                result = table_temp.Select("composants_bannis_id = " & table_composants_bannis.Rows(i).Item("composants_bannis_id"))
                                If result.GetLength(0) = 0 Then 'le composant n'existe plus --> DELETE
                                    table_composants_bannis.Rows.Remove(table_composants_bannis.Rows(i))
                                ElseIf result.GetLength(0) = 1 Then 'le composant banni existe --> MAJ
                                    'log("MAJ: mise à jour du composant banni : " & table_composants_bannis.Rows(i).Item("composants_bannis_id"))
                                    For j = 0 To table_composants_bannis.Columns.Count - 1
                                        table_composants_bannis.Rows(i).Item(j) = result(0)(j)
                                    Next
                                End If
                            Next
                        End If

                        'ajout des nouveaux composants bannis
                        For i = 0 To table_temp.Rows.Count() - 1
                            result = table_composants_bannis.Select("composants_bannis_id = " & table_temp.Rows(i).Item("composants_bannis_id"))
                            If result.GetLength(0) = 0 Then 'le composant bannis n'est pas dans la table_composants_bannis, on l'ajoute
                                table_composants_bannis.ImportRow(table_temp.Rows(i))
                            End If
                        Next

                        'affichage de la nouvelle liste
                        For i = 0 To table_composants_bannis.Rows.Count() - 1
                            log("      -> Id : " & table_composants_bannis.Rows(i).Item("composants_bannis_id") & " -- Norme : " & table_composants_bannis.Rows(i).Item("composants_bannis_norme") & " -- Adresse : " & table_composants_bannis.Rows(i).Item("composants_bannis_adresse") & " -- Description : " & table_composants_bannis.Rows(i).Item("composants_bannis_description"), 0)
                        Next
                    Else
                        log("      -> Pas de composant banni trouvé !", 0)
                    End If
                End If
            Catch ex As Exception
                log("MAJ : composants_bannis error : " & ex.Message, 0)
            End Try
        End If

        'Maj de la table Macro
        If table = "ALL" Or table = "MACRO" Then
            Try
                log("MAJ : Maj de la table_macros", 0)
                table_temp = New DataTable
                err = Table_maj_sql(table_temp, "SELECT * FROM macro WHERE macro_actif='1' AND macro_conditions NOT LIKE '%CT#%'")
                If err <> "" Then
                    log("MAJ : SQL ERR: " & err, 0)
                Else
                    If table_temp.Rows.Count() > 0 Then 'on a récupéré la nouvelle liste des macros, on fait la maj
                        'Maj et suppression des macros existantes
                        If table_macros.Rows.Count() > 0 Then
                            For i = table_macros.Rows.Count() - 1 To 0 Step -1
                                result = table_temp.Select("macro_id = " & table_macros.Rows(i).Item("macro_id"))
                                If result.GetLength(0) = 0 Then 'la macro n'existe plus --> DELETE
                                    table_macros.Rows.Remove(table_macros.Rows(i))
                                ElseIf result.GetLength(0) = 1 Then 'la macro existe --> MAJ
                                    For j = 0 To table_macros.Columns.Count - 1
                                        'si la colonne n'est pas le verrou --> maj
                                        If table_macros.Columns(j).Caption <> "verrou" Then
                                            table_macros.Rows(i).Item(j) = result(0)(j)
                                        End If
                                    Next
                                End If
                            Next
                        End If
                        'ajout dune colonne pour le verrou
                        x = New DataColumn
                        x.ColumnName = "verrou"
                        table_temp.Columns.Add(x)
                        'ajout des nouvelles macros
                        For i = 0 To table_temp.Rows.Count() - 1
                            result = table_macros.Select("macro_id = " & table_temp.Rows(i).Item("macro_id"))
                            If result.GetLength(0) = 0 Then 'la macro n'est pas dans la table_macros, on l'ajoute
                                table_temp.Rows(i).Item("verrou") = False
                                'ajout
                                table_macros.ImportRow(table_temp.Rows(i))
                            End If
                        Next
                        'affichage de la nouvelle liste
                        For i = 0 To table_macros.Rows.Count() - 1
                            log("      -> Id : " & table_macros.Rows(i).Item("macro_id") & " -- Nom : " & table_macros.Rows(i).Item("macro_nom") & " -- Condition : " & table_macros.Rows(i).Item("macro_conditions") & " -- Action : " & table_macros.Rows(i).Item("macro_actions"), 0)
                        Next
                    Else
                        log("      -> Pas de macro trouvée !", 0)
                    End If
                End If
            Catch ex As Exception
                log("MAJ : macro error : " & ex.Message, 0)
            End Try
        End If

        'Maj de la table Timer
        If table = "ALL" Or table = "TIMER" Then
            Try
                log("MAJ : Maj de la table_timer", 0)
                table_temp = New DataTable
                err = Table_maj_sql(table_temp, "SELECT * FROM macro WHERE macro_actif='1' AND macro_conditions LIKE '%CT#%'")
                If err <> "" Then
                    log("MAJ : SQL ERR: " & err, 0)
                Else
                    If table_temp.Rows.Count() > 0 Then 'on a récupéré la nouvelle liste des timers, on fait la maj
                        'Maj et suppression des timers existants
                        If table_timer.Rows.Count() > 0 Then
                            For i = table_timer.Rows.Count() - 1 To 0 Step -1
                                result = table_temp.Select("macro_id = " & table_timer.Rows(i).Item("macro_id"))
                                If result.GetLength(0) = 0 Then 'le timer n'existe plus --> DELETE
                                    table_timer.Rows.Remove(table_timer.Rows(i))
                                ElseIf result.GetLength(0) = 1 Then 'le timer existe --> MAJ
                                    For j = 0 To table_timer.Columns.Count - 1
                                        'si la colonne n'est pas le verrou --> maj
                                        If table_timer.Columns(j).Caption <> "timer" Then
                                            table_timer.Rows(i).Item(j) = result(0)(j)
                                        End If
                                    Next
                                End If
                            Next
                        End If
                        'ajout dune colonne pour le timer
                        x = New DataColumn
                        x.ColumnName = "timer"
                        table_temp.Columns.Add(x)
                        'ajout des nouveaux timers
                        For i = 0 To table_temp.Rows.Count() - 1
                            result = table_timer.Select("macro_id = " & table_temp.Rows(i).Item("macro_id"))
                            If result.GetLength(0) = 0 Then 'le timer n'est pas dans la table_timer, on l'ajoute
                                table_temp.Rows(i).Item("timer") = timer_convertendate(table_temp.Rows(i).Item("macro_conditions"))
                                'ajout
                                table_timer.ImportRow(table_temp.Rows(i))
                            End If
                        Next
                        'affichage de la nouvelle liste
                        For i = 0 To table_timer.Rows.Count() - 1
                            table_timer.Rows(i).Item("timer") = timer_convertendate(table_timer.Rows(i).Item("macro_conditions"))
                            log("      -> Id : " & table_timer.Rows(i).Item("macro_id") & " -- Nom : " & table_timer.Rows(i).Item("macro_nom") & " -- Condition : " & table_timer.Rows(i).Item("macro_conditions") & " -- Action : " & table_timer.Rows(i).Item("macro_actions") & " -- Timer : " & table_timer.Rows(i).Item("timer"), 0)
                        Next
                    Else
                        log("      -> Pas de Timer trouvé !", 0)
                    End If
                End If
            Catch ex As Exception
                log("MAJ : timer error : " & ex.Message, 0)
            End Try
        End If

    End Sub

    Private Sub tables_aff()
        'fonction affichant (log) le contenu de toutes les tables en memoire
        Dim temp As String
        
        Try
	        'Affiche la table composant
            log("AFF : table_composants", 0)
	        For i = 0 To table_composants.Rows.Count() - 1
	            temp = ""
	            For j = 0 To table_composants.Columns.Count() - 1
	                temp = temp & table_composants.Columns(j).Caption & ":" & table_composants.Rows(i).Item(j) & " "
	            Next
	            log("      -> " & temp, 0)
            Next
            If table_composants.Rows.Count() = 0 Then log("     -> pas de composant", 0)
	        'Affiche la table composant bannis
            log("AFF : table_composants_bannis", 0)
	        For i = 0 To table_composants_bannis.Rows.Count() - 1
	            temp = ""
	            For j = 0 To table_composants_bannis.Columns.Count() - 1
	                temp = temp & table_composants_bannis.Columns(j).Caption & ":" & table_composants_bannis.Rows(i).Item(j) & " "
	            Next
	            log("      -> " & temp, 0)
            Next
            If table_composants_bannis.Rows.Count() = 0 Then log("     -> pas de composant banni", 0)
	        'Affiche la table Macro
            log("AFF : table_macros", 0)
	        For i = 0 To table_macros.Rows.Count() - 1
	            temp = ""
	            For j = 0 To table_macros.Columns.Count() - 1
	                temp = temp & table_macros.Columns(j).Caption & ":" & table_macros.Rows(i).Item(j) & " "
	            Next
	            log("      -> " & temp, 0)
            Next
            If table_macros.Rows.Count() = 0 Then log("     -> pas de macro", 0)
	        'Affiche la table Timer
            log("AFF : table_timer", 0)
	        For i = 0 To table_timer.Rows.Count() - 1
	            temp = ""
	            For j = 0 To table_timer.Columns.Count() - 1
	                temp = temp & table_timer.Columns(j).Caption & ":" & table_timer.Rows(i).Item(j) & " "
	            Next
	            log("      -> " & temp, 0)
            Next
            If table_timer.Rows.Count() = 0 Then log("     -> pas de timer", 0)
            'Affiche la table Thread
            log("AFF : table_thread", 0)
            For i = 0 To table_thread.Rows.Count() - 1
                temp = ""
                For j = 0 To table_thread.Columns.Count() - 1
                    temp = temp & table_thread.Columns(j).Caption & ":" & table_thread.Rows(i).Item(j) & " "
                Next
                log("      -> " & temp, 0)
            Next
            If table_thread.Rows.Count() = 0 Then log("     -> pas de threads", 0)
            'Affiche la table Erreur
            log("AFF : table_erreur", 0)
            For i = 0 To table_erreur.Rows.Count() - 1
                temp = ""
                For j = 0 To table_erreur.Columns.Count() - 1
                    temp = temp & table_erreur.Columns(j).Caption & ":" & table_erreur.Rows(i).Item(j) & " "
                Next
                log("      -> " & temp, 0)
            Next
            If table_erreur.Rows.Count() = 0 Then log("     -> pas d erreurs", 0)
        Catch ex As Exception
            log("AFF : Exception : " & ex.Message, 0)
        End Try
    End Sub

    Private Sub thread_ajout(ByVal composant_id As String, ByVal norme As String, ByVal source As String, ByRef mythread As Thread)
        'ajoute une ligne dans la table pour le nouveau thread
        Try
            Dim newRow As DataRow
            newRow = table_thread.NewRow()
            newRow.Item("composant_id") = composant_id
            newRow.Item("source") = source
            newRow.Item("norme") = norme
            newRow.Item("datetime") = Date.Now.ToString("yyyy-MM-dd HH:mm:ss")
            newRow.Item("thread") = mythread
            SyncLock lock_tablethread
                table_thread.Rows.Add(newRow)
            End SyncLock
        Catch ex As Exception
            log("DOM : Thread_Ajout Exception : " & ex.ToString, 2)
        End Try
    End Sub

    Private Sub pool(ByVal sender As System.Object, ByVal e As System.EventArgs)
        'fonction s'executant toutes les secondes
        'on verifie si un composant doit etre checker : si colonne timer= dateheure
        Dim y As Thread
        Dim tabletemp() As DataRow
        Dim tabletemp2() As DataRow
        Dim dateettime As DateTime
        Try
            dateettime = DateAndTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            tabletemp = table_composants.Select("composants_polling <> '0'")
            For i = 0 To tabletemp.GetUpperBound(0)
                If tabletemp(i)("timer") <= dateettime Then
                    '--- maj du timer du composant ---
                    Dim date_pooling As Date
                    date_pooling = DateAndTime.Now.AddSeconds(tabletemp(i)("composants_polling")) 'on initialise
                    tabletemp(i)("timer") = date_pooling.ToString("yyyy-MM-dd HH:mm:ss")
                    '--- test pour savoir si un thread est deja lancé sur ce composant ---
                    SyncLock lock_tablethread
                        tabletemp2 = table_thread.Select("composant_id = '" & tabletemp(i)("composants_id") & "'")
                    End SyncLock
                    If tabletemp2.GetUpperBound(0) < 0 Then
                        Select Case tabletemp(i)("composants_modele_nom").ToString() 'choix de l'action en fonction du modele
                            Case "DS18B20", "DS2423_A", "DS2423_B", "DS2406_capteur", "DS2406_relais" 'WIR ou WI2 : compteur, temperature, switch, relais
                                Dim ecrire As ECRIRE = New ECRIRE(tabletemp(i)("composants_id"), "", "", "", "")
                                y = New Thread(AddressOf ecrire.action)
                                y.Name = "ecrire_" & tabletemp(i)("composants_id")
                                thread_ajout(tabletemp(i)("composants_id").ToString, tabletemp(i)("composants_modele_norme").ToString, "ECR", y)
                                y.Start()
                            Case "2267-2268", "2263-2264" 'PLC : MicroModule lampes ou MicroModule Appareils
                                Dim ecrire As ECRIRE = New ECRIRE(tabletemp(i)("composants_id"), "STATUS_REQUEST", "", "", "")
                                y = New Thread(AddressOf ecrire.action)
                                y.Name = "ecrire_" & tabletemp(i)("composants_id")
                                thread_ajout(tabletemp(i)("composants_id").ToString, tabletemp(i)("composants_modele_norme").ToString, "ECR", y)
                                y.Start()
                            Case "ZIB_STA" 'ZIB : switch
                                Dim ecrire As ECRIRE = New ECRIRE(tabletemp(i)("composants_id"), "STATUS_REQUEST", "", "", "")
                                y = New Thread(AddressOf ecrire.action)
                                y.Name = "ecrire_" & tabletemp(i)("composants_id")
                                thread_ajout(tabletemp(i)("composants_id").ToString, tabletemp(i)("composants_modele_norme").ToString, "ECR", y)
                                y.Start()
                            Case Else
                                log("POL : Pas de fonction associé à " & tabletemp(i)("composants_modele_nom").ToString() & ":" & tabletemp(i)("composants_nom").ToString(), 2)
                        End Select
                    Else
                        log("POL : Un thread est déjà associé à " & tabletemp(i)("composants_nom").ToString(), 2)
                    End If
                End If
            Next
        Catch ex As Exception
            log("POL : Exception : " & ex.ToString, 2)
        End Try
    End Sub

    Private Sub timer()
        'fonction s'executant toutes les secondes
        'on verifie si un timer doit s'executer
        Dim tabletemp() As DataRow
        Try
            Dim heure As String
            heure = DateAndTime.Now.ToString("HH:mm:ss")
            '--- HEURES DU SOLEIL --> Composant virtuel JOUR ---
            If STRGS.Right(var_soleil_lever, 8) = heure Then 'si lever du soleil
                tabletemp = table_composants.Select("composants_adresse = 'jour'")
                If tabletemp.GetLength(0) = 1 Then
                    tabletemp(0)("composants_etat") = "1" 'maj du composant virtuel JOUR
                    log("TIM : lever du soleil -> Maj composant virtuel JOUR=1", 4)
                Else
                    log("TIM : ERR: Maj composant virtuel JOUR=1 : Non trouvé", 2)
                    tables_aff() 'affichage des tables car pas normal qu'on est pas le composant JOUR
                End If
            ElseIf STRGS.Right(var_soleil_coucher, 8) = heure Then 'si coucher du soleil
                tabletemp = table_composants.Select("composants_adresse = 'jour'")
                If tabletemp.GetLength(0) = 1 Then
                    tabletemp(0)("composants_etat") = "0" 'maj du composant virtuel JOUR
                    log("TIM : coucher du soleil -> Maj composant virtuel JOUR=0", 4)
                Else
                    log("TIM : ERR: Maj composant virtuel JOUR=0 : Non trouvé", 2)
                    tables_aff() 'affichage des tables car pas normal qu'on est pas le composant JOUR
                End If
            End If
            '--- HEURES DU SOLEIL CORRIGEES --> Composant virtuel JOUR2 ---
            If STRGS.Right(var_soleil_lever2, 8) = heure Then 'si lever du soleil corrige
                tabletemp = table_composants.Select("composants_adresse = 'jour2'")
                If tabletemp.GetLength(0) = 1 Then
                    tabletemp(0)("composants_etat") = "1" 'maj du composant virtuel JOUR2
                    log("TIM : lever du soleil corrigé -> Maj composant virtuel JOUR2=1", 4)
                Else
                    log("TIM : ERR: Maj composant virtuel JOUR2=1 : Non trouvé", 2)
                    tables_aff() 'affichage des tables car pas normal qu'on est pas le composant JOUR2
                End If
            ElseIf STRGS.Right(var_soleil_coucher2, 8) = heure Then 'si coucher du soleil corrige
                tabletemp = table_composants.Select("composants_adresse = 'jour2'")
                If tabletemp.GetLength(0) = 1 Then
                    tabletemp(0)("composants_etat") = "0" 'maj du composant virtuel JOUR2
                    log("TIM : coucher du soleil corrigé -> Maj composant virtuel JOUR2=0", 4)
                Else
                    log("TIM : ERR: Maj composant virtuel JOUR2=0 : Non trouvé", 2)
                    tables_aff() 'affichage des tables car pas normal qu'on est pas le composant JOUR2
                End If
            End If

            '--- Actions à faire chaque jour à minuit ---
            If heure = "00:01:00" Then
                'Calcul de l'heure de lever et coucher du soleil
                Me.soleil.CalculateSun()
                var_soleil_lever = Me.soleil.Sunrise
                var_soleil_coucher = Me.soleil.Sunset
                var_soleil_coucher2 = DateAdd(DateInterval.Minute, heure_coucher_correction, var_soleil_coucher)
                var_soleil_lever2 = DateAdd(DateInterval.Minute, heure_lever_correction, var_soleil_lever)
                log("TIM : maj Heure du soleil : ", 0)
                log("      -> Heure du lever : " & var_soleil_lever & " (" & var_soleil_lever2 & ")", 0)
                log("      -> Heure du coucher : " & var_soleil_coucher & " (" & var_soleil_coucher2 & ")", 0)
            End If

            '--- Traitement des timers
            For i = 0 To table_timer.Rows.Count() - 1
                If table_timer(i)("timer") <= DateAndTime.Now.ToString("yyyy-MM-dd HH:mm:ss") Then
                    'reprogrammation du prochain shedule
                    table_timer(i)("timer") = timer_convertendate(table_timer.Rows(i).Item("macro_conditions"))
                    'lancement des actions
                    log("TIM : " & table_timer.Rows(i).Item("macro_nom") & " : OK", 4)
                    action(table_timer.Rows(i).Item("macro_actions"))
                End If
            Next

        Catch ex As Exception
            log("TIM : TIMER Exception : " & ex.ToString, 2)
        End Try
    End Sub

    Private Sub table_composants_changed(ByVal sender As Object, ByVal args As DataColumnChangeEventArgs)
        Dim err, dateheure, etat_temp As String
        '--- si on a un changement de l'etat d'un composant ---
        Try
            If args.Column.ColumnName = "composants_etat" Then
                '--- Enregistrement du releve dans la base SQL ---
                dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                etat_temp = args.Row.Item("composants_etat")
                etat_temp = STRGS.Replace(etat_temp, ",", ".") 'correction pour avoir un point au lieu d'une virgule
                err = mysql.mysql_nonquery("INSERT INTO releve(releve_Composants,releve_Valeur,releve_DateHeure) VALUES('" & args.Row.Item("composants_id") & "', '" & etat_temp & "', '" & dateheure & "')")
                If err <> "" Then log("DOM : table_comp_changed SQL " & err, 2)
                '--- modification de l'etat du composant dans la base ---
                err = mysql.mysql_nonquery("UPDATE composants SET composants_etat='" & etat_temp & "',composants_etatdate='" & dateheure & "' WHERE composants_id='" & args.Row.Item("composants_id") & "'")
                If err <> "" Then log("DOM : table_comp_changed SQL" & err, 2)
                '--- gestion des macros ---
                macro(args.Row.Item("composants_id"), args.Row.Item("composants_etat"))
            End If
        Catch ex As Exception
            log("DOM : table_comp_changed exception : " & ex.ToString, 2)
        End Try
    End Sub

    Private Sub macro(ByVal comp_id As String, ByVal comp_etat As String)
        Try
            Dim tabletemp() As DataRow
            tabletemp = table_composants.Select("composants_id = '" & comp_id & "'")
            If tabletemp.GetLength(0) = 1 Then 'composant trouvé
                For i = 0 To table_macros.Rows.Count() - 1
                    'test si le composant fait partie d'une macro
                    If STRGS.InStr(table_macros.Rows(i).Item("macro_conditions"), "CC#" & comp_id & "#") > 0 Then
                        If analyse_cond(table_macros.Rows(i).Item("macro_conditions"), comp_id) Then 'verification des conditions
                            If table_macros.Rows(i).Item("verrou") = False Then 'si la macro n'est pas déjà en cours d'execution
                                table_macros.Rows(i).Item("verrou") = True
                                log("MAC : " & table_macros.Rows(i).Item("macro_nom") & " : OK", 4)
                                action(table_macros.Rows(i).Item("macro_actions"))
                                table_macros.Rows(i).Item("verrou") = False
                            End If
                        End If
                    End If
                Next
            End If
        Catch ex As Exception
            log("MAC : Macro Exception : " & ex.ToString, 2)
        End Try
    End Sub

    Function analyse_cond(ByVal liste As String, ByVal composants_id As String) As Boolean
        'fonction recursive d'analyse des conditions d'une macro
        Dim resultat As Boolean = True
        Dim resultat_temp As Boolean = True
        Dim posfin As Integer = 1
        Dim posdebut As Integer = 2
        Dim operateur As String = "&&"
        Dim listesup As Boolean = False
        Try

            'log("liste de base " & liste & " --- avec comp ID:" & composants_id, 9)
            If STRGS.Left(liste, 1) = "!" Then
                liste = STRGS.Mid(liste, 3, STRGS.Len(liste) - 2) 'on supprimer les !(...) de chaque cote de la liste
                While (posfin < STRGS.Len(liste)) 'tant que toute la liste n'a pas ete traite
                    If liste(posfin - 1) = "(" Then 'c'est une liste
                        'log(" x1 -> " & posdebut & "-" & posfin, 9)
                        posfin = STRGS.InStr(posdebut, liste, ")")
                        'log(" x2 -> " & posdebut & "-" & posfin, 9)
                        For i = (posdebut - 1) To (posfin - 1)
                            If liste(i) = "(" Then posfin = STRGS.InStr(posfin + 1, liste, ")")
                        Next
                        'log(" x22 -> " & posdebut & "-" & posfin, 9)
                        'log("sous liste " & STRGS.Mid(liste, posdebut - 1, posfin - posdebut + 2), 9)
                        'si le test concerne le composant modifié alors je teste sinon j'ignore
                        If (STRGS.InStr(STRGS.Mid(liste, posdebut - 1, posfin - posdebut + 2), composants_id) > 0) Then
                            resultat_temp = analyse_cond(STRGS.Mid(liste, posdebut - 1, posfin - posdebut + 2), composants_id) 'on relance une analyse sur cette sous liste
                        Else
                            resultat_temp = False
                        End If
                    ElseIf liste(posfin - 1) = "[" Then 'c'est un atome
                        posfin = STRGS.InStr(posdebut, liste, "]")
                        'log("atome " & STRGS.Mid(liste, posdebut, posfin - posdebut), 9)
                        'si le test concerne le composant modifié alors je teste sinon j'ignore
                        If (STRGS.InStr(STRGS.Mid(liste, posdebut, posfin - posdebut), composants_id) > 0) Then
                            resultat_temp = test_cond(STRGS.Mid(liste, posdebut, posfin - posdebut)) 'on teste la condition
                        Else
                            resultat_temp = False
                        End If
                    End If
                    'Calcul du resultat
                    If operateur = "&&" Then
                        resultat = resultat And resultat_temp
                    Else
                        resultat = resultat Or resultat_temp
                    End If
                    'log(" x3 -> " & posdebut & "-" & posfin, 9)
                    If (posfin) < STRGS.Len(liste) Then 'on a pas fini, on avance à l'element suivant
                        operateur = STRGS.Mid(liste, posfin + 1, 2)
                        posdebut = posfin + 4
                        posfin = posfin + 3
                    End If
                    'log(" x4 -> " & posdebut & "-" & posfin, 9)
                End While
            Else
                liste = STRGS.Mid(liste, 2, STRGS.Len(liste) - 2) 'on supprimer les () de chaque cote de la liste
                While (posfin < STRGS.Len(liste)) 'tant que toute la liste n'a pas ete traite
                    If liste(posfin - 1) = "(" Then 'c'est une liste
                        'log(" x1 -> " & posdebut & "-" & posfin, 9)
                        posfin = STRGS.InStr(posdebut, liste, ")")
                        'log(" x2 -> " & posdebut & "-" & posfin, 9)
                        For i = (posdebut - 1) To (posfin - 1)
                            If liste(i) = "(" Then posfin = STRGS.InStr(posfin + 1, liste, ")")
                        Next
                        'log(" x22 -> " & posdebut & "-" & posfin, 9)
                        'log("sous liste " & STRGS.Mid(liste, posdebut - 1, posfin - posdebut + 2), 9)
                        resultat_temp = analyse_cond(STRGS.Mid(liste, posdebut - 1, posfin - posdebut + 2), composants_id) 'on relance une analyse sur cette sous liste
                    ElseIf liste(posfin - 1) = "[" Then 'c'est un atome
                        posfin = STRGS.InStr(posdebut, liste, "]")
                        'log("atome " & STRGS.Mid(liste, posdebut, posfin - posdebut), 9)
                        resultat_temp = test_cond(STRGS.Mid(liste, posdebut, posfin - posdebut)) 'on teste la condition
                    End If
                    'Calcul du resultat
                    If operateur = "&&" Then
                        resultat = resultat And resultat_temp
                    Else
                        resultat = resultat Or resultat_temp
                    End If
                    'log(" x3 -> " & posdebut & "-" & posfin, 9)
                    If (posfin) < STRGS.Len(liste) Then 'on a pas fini, on avance à l'element suivant
                        operateur = STRGS.Mid(liste, posfin + 1, 2)
                        posdebut = posfin + 4
                        posfin = posfin + 3
                    End If
                    'log(" x4 -> " & posdebut & "-" & posfin, 9)
                End While
            End If


        Catch ex As Exception
            resultat = False
            log("MAC : analyse_cond Exception : " & ex.ToString & " --> " & liste, 2)
        End Try
        Return resultat
    End Function

    Function test_cond(ByVal text As String) As Boolean
        'teste une condition d'une macro
        Dim resultat As Boolean = False
        Dim etat, etat2
        Try
            Dim texte = STRGS.Split(text, "#")
            If (texte(0) = "CC" Or texte(0) = "CI") Then 'c'est un composant : CC#id#=<>#valeur ou CI#id#=<>#valeur
                Dim tabletemp = table_composants.Select("composants_id = " & texte(1))
                If tabletemp.GetLength(0) > 0 Then
                    If IsNumeric(tabletemp(0)("composants_etat")) Then etat = CDbl(tabletemp(0)("composants_etat")) Else etat = tabletemp(0)("composants_etat")
                    If IsNumeric(texte(3)) Then etat2 = CDbl(texte(3)) Else etat2 = texte(3)
                    Select Case texte(2)
                        Case "<" : If etat < etat2 Then resultat = True
                        Case "<=" : If etat <= etat2 Then resultat = True
                        Case "=" : If etat = etat2 Then resultat = True
                        Case ">" : If etat > etat2 Then resultat = True
                        Case ">=" : If etat >= etat2 Then resultat = True
                        Case "!" : If etat <> etat2 Then resultat = True
                    End Select
                Else
                    'logform(" > composant non trouvé " & texte(1))
                End If
            ElseIf texte(0) = "CH" Then 'c'est une condition sur l'heure : CH#=<>#ss#mm#hh#jj#MMM#JJJ
                resultat = True
                If (texte(2) <> "*") Then
                    Select Case texte(1)
                        Case "<" : If texte(2) >= DateAndTime.Now.ToString("ss") Then resultat = False
                        Case "<=" : If texte(2) > DateAndTime.Now.ToString("ss") Then resultat = False
                        Case "=" : If texte(2) <> DateAndTime.Now.ToString("ss") Then resultat = False
                        Case ">" : If texte(2) <= DateAndTime.Now.ToString("ss") Then resultat = False
                        Case ">=" : If texte(2) < DateAndTime.Now.ToString("ss") Then resultat = False
                        Case "!" : If texte(2) = DateAndTime.Now.ToString("ss") Then resultat = False
                    End Select
                End If
                If (texte(3) <> "*") Then
                    Select Case texte(1)
                        Case "<" : If texte(3) >= DateAndTime.Now.ToString("mm") Then resultat = False
                        Case "<=" : If texte(3) > DateAndTime.Now.ToString("mm") Then resultat = False
                        Case "=" : If texte(3) <> DateAndTime.Now.ToString("mm") Then resultat = False
                        Case ">" : If texte(3) <= DateAndTime.Now.ToString("mm") Then resultat = False
                        Case ">=" : If texte(3) < DateAndTime.Now.ToString("mm") Then resultat = False
                        Case "!" : If texte(3) = DateAndTime.Now.ToString("mm") Then resultat = False
                    End Select
                End If
                If (texte(4) <> "*") Then
                    Select Case texte(1)
                        Case "<" : If texte(4) >= DateAndTime.Now.ToString("HH") Then resultat = False
                        Case "<=" : If texte(4) > DateAndTime.Now.ToString("HH") Then resultat = False
                        Case "=" : If texte(4) <> DateAndTime.Now.ToString("HH") Then resultat = False
                        Case ">" : If texte(4) <= DateAndTime.Now.ToString("HH") Then resultat = False
                        Case ">=" : If texte(4) < DateAndTime.Now.ToString("HH") Then resultat = False
                        Case "!" : If texte(4) = DateAndTime.Now.ToString("HH") Then resultat = False
                    End Select
                End If
                If (texte(5) <> "*") Then
                    Select Case texte(1)
                        Case "<" : If texte(5) >= DateAndTime.Now.Day.ToString Then resultat = False
                        Case "<=" : If texte(5) > DateAndTime.Now.Day.ToString Then resultat = False
                        Case "=" : If texte(5) <> DateAndTime.Now.Day.ToString Then resultat = False
                        Case ">" : If texte(5) <= DateAndTime.Now.Day.ToString Then resultat = False
                        Case ">=" : If texte(5) < DateAndTime.Now.Day.ToString Then resultat = False
                        Case "!" : If texte(5) = DateAndTime.Now.Day.ToString Then resultat = False
                    End Select
                End If
                If (texte(6) <> "*") Then 'secondes
                    Select Case texte(1)
                        Case "<" : If texte(6) >= DateAndTime.Now.Month.ToString Then resultat = False
                        Case "<=" : If texte(6) > DateAndTime.Now.Month.ToString Then resultat = False
                        Case "=" : If texte(6) <> DateAndTime.Now.Month.ToString Then resultat = False
                        Case ">" : If texte(6) <= DateAndTime.Now.Month.ToString Then resultat = False
                        Case ">=" : If texte(6) < DateAndTime.Now.Month.ToString Then resultat = False
                        Case "!" : If texte(6) = DateAndTime.Now.Month.ToString Then resultat = False
                    End Select
                End If
                If (texte(7) <> "*") Then 'secondes
                    Select Case texte(1)
                        Case "<" : If texte(7) >= DateAndTime.Now.DayOfWeek.ToString Then resultat = False
                        Case "<=" : If texte(7) > DateAndTime.Now.DayOfWeek.ToString Then resultat = False
                        Case "=" : If texte(7) <> DateAndTime.Now.DayOfWeek.ToString Then resultat = False
                        Case ">" : If texte(7) <= DateAndTime.Now.DayOfWeek.ToString Then resultat = False
                        Case ">=" : If texte(7) < DateAndTime.Now.DayOfWeek.ToString Then resultat = False
                        Case "!" : If texte(7) = DateAndTime.Now.DayOfWeek.ToString Then resultat = False
                    End Select
                End If
            ElseIf texte(0) = "CT" Then 'c'est un TIMER, on renvoie true car on ne le teste pas, ca sert à programmer
                resultat = True
            End If
        Catch ex As Exception
            log("MAC : Test_cond exception : " & ex.ToString & " --> " & text, 2)
        End Try
        Return resultat
    End Function

    Sub action(ByVal liste As String)
        'execute la liste d'actions passé en paramétre
        Dim posdebut As Integer = 2
        Dim posfin As Integer = 2
        Dim contenu
        Dim timer_valeur As String = ""
        Dim y As Thread
        Dim tabletemp() As DataRow
        Dim tabletempp() As DataRow
        Dim tblthread() As DataRow
        Try
            liste = STRGS.Mid(liste, 2, STRGS.Len(liste) - 2) 'on supprimer les () de chaque cote de la liste
            While (posfin < STRGS.Len(liste)) 'tant que toute la liste n'a pas ete traite
                posfin = STRGS.InStr(posdebut, liste, "]")
                contenu = STRGS.Split(STRGS.Mid(liste, posdebut, posfin - posdebut), "#")

                '--------------------------- AC = Action sur un composant -------------------------
                If contenu(0) = "AC" Then 'c'est un composant :  : [AC#compid#Valeur] ou [AC#compid#Valeur#Valeur2]
                    'recherche du composant
                    tabletemp = table_composants.Select("composants_id = '" & contenu(1) & "'")
                    If tabletemp.GetLength(0) = 1 Then
                        'verification si on a pas déjà un thread sur ce comp sinon on boucle pour attendre Action_timeout/10 = 5 sec par défaut
                        SyncLock lock_tablethread
                            tblthread = table_thread.Select("composant_id = '" & contenu(1) & "'")
                        End SyncLock
                        Dim limite As Integer = 0
                        While (tblthread.GetLength(0) > 0 And limite < (Action_timeout / 10))
                            wait(10)
                            SyncLock lock_tablethread
                                tblthread = table_thread.Select("composant_id = '" & contenu(1) & "'")
                            End SyncLock
                            limite = limite + 1
                        End While
                        'a la fin du timeout on verifie si Libre
                        SyncLock lock_tablethread
                            tblthread = table_thread.Select("composant_id = '" & contenu(1) & "'")
                        End SyncLock
                        If tblthread.GetUpperBound(0) < 0 Then
                            'gestion du timer dans une action de type [AC#xx#...#timer20] -> pause de 20 sec avant d'executer l'action
                            Dim lastcontenu = contenu(UBound(contenu))
                            If STRGS.Left(lastcontenu, 5) = "timer" Then
                                log("MAC :  -> Action composant : tempo avant action : " & contenu(0) & "-" & contenu(1) & " Timer=" & contenu(UBound(contenu)), 6)
                                timer_valeur = STRGS.Right(lastcontenu, STRGS.Len(lastcontenu) - 5) * 100
                            Else
                                timer_valeur = ""
                            End If

                            Dim x As ECRIRE
                            If UBound(contenu) = 5 Then
                                x = New ECRIRE(tabletemp(0)("composants_id"), contenu(2), contenu(3), contenu(4), timer_valeur)
                            ElseIf UBound(contenu) = 4 Then
                                x = New ECRIRE(tabletemp(0)("composants_id"), contenu(2), contenu(3), "", timer_valeur)
                            Else
                                x = New ECRIRE(tabletemp(0)("composants_id"), contenu(2), "", "", timer_valeur)
                            End If
                            y = New Thread(AddressOf x.action)
                            y.Name = "ecrire_" & tabletemp(0)("composants_id")
                            thread_ajout(tabletemp(0)("composants_id").ToString, tabletemp(0)("composants_modele_norme").ToString, "ECR", y)
                            y.Start()
                            'on modifie l'etat du composant en memoire
                            tabletempp = table_composants.Select("composants_id = '" & contenu(1) & "'")
                            If tabletempp.GetLength(0) = 1 Then
                                tabletempp(0)("composants_etat") = contenu(2)
                            End If
                            log("MAC :  -> Action composant : Composant : " & tabletemp(0)("composants_nom") & " Etat=" & contenu(2), 6)
                        Else
                            log("MAC :  -> Action composant : Un thread est déjà associé à " & tabletemp(0)("composants_nom").ToString(), 2)
                        End If
                    Else
                        log("MAC :  -> Action composant : composant ID=" & contenu(1) & " non trouvé", 2)
                    End If
                    wait(100)

                    '--------------------------- ME = Etat d'un composant -------------------------
                ElseIf contenu(0) = "ME" Then 'c'est l'etat d'un composant à modifier :  : [ME#compid#Etat]
                    'recherche du composant
                    tabletemp = table_composants.Select("composants_id = '" & contenu(1) & "'")
                    If tabletemp.GetLength(0) = 1 Then
                        tabletemp(0)("composants_etat") = contenu(2) 'maj du composant en memoire
                        log("MAC :  -> Modif Etat : Composant : " & tabletemp(0)("composants_nom") & " Etat=" & contenu(2), 6)
                    Else
                        log("MAC :  -> Modif Etat : Composant ID=" & contenu(1) & " non trouvé", 2)
                    End If

                    '--------------------------- MA = Adresse d'un composant -------------------------
                ElseIf contenu(0) = "MA" Then 'c'est l'adresse d'un composant à modifier :  : [MA#compid#Adresse]
                    tabletemp = table_composants.Select("composants_id = '" & contenu(1) & "'")
                    If tabletemp.GetLength(0) = 1 Then
                        tabletemp(0)("composants_adresse") = contenu(2) 'maj du composant en memoire
                        log("MAC :  -> Modif Adresse composant : " & tabletemp(0)("composants_nom") & " Adresse=" & contenu(2), 6)
                    Else
                        log("MAC :  -> Modif Adresse composant : ID=" & contenu(1) & " non trouvé", 2)
                    End If

                    '--------------------------- MM = condition/action macro -------------------------
                ElseIf contenu(0) = "MM" Then 'c'est la condition/action d'une macro à modifier :  : [MM#macroid#Condition#action]
                    tabletemp = table_macros.Select("macro_id = '" & contenu(1) & "'")
                    'decodage des infos
                    Dim macro_cond As String = contenu(2)
                    Dim macro_act As String = contenu(3)
                    macro_cond = macro_cond.Replace("_1", "(")
                    macro_cond = macro_cond.Replace("_2", "[")
                    macro_cond = macro_cond.Replace("_3", "#")
                    macro_cond = macro_cond.Replace("_4", "]")
                    macro_cond = macro_cond.Replace("_5", ")")
                    macro_act = macro_act.Replace("_1", "(")
                    macro_act = macro_act.Replace("_2", "[")
                    macro_act = macro_act.Replace("_3", "#")
                    macro_act = macro_act.Replace("_4", "]")
                    macro_act = macro_act.Replace("_5", ")")
                    If tabletemp.GetLength(0) = 1 Then
                        'maj des champs en memoire
                        tabletemp(0)("macro_conditions") = macro_cond 'maj de la condition
                        tabletemp(0)("macro_actions") = macro_act 'maj de l'action
                        log("MAC :  -> Modif Macro :" & tabletemp(0)("macro_nom") & " Cond=" & macro_cond & " Action=" & macro_act, 6)
                    Else
                        tabletemp = table_timer.Select("macro_id = '" & contenu(1) & "'")
                        If tabletemp.GetLength(0) = 1 Then
                            'maj des champs en memoire
                            tabletemp(0)("macro_conditions") = macro_cond 'maj de la condition
                            tabletemp(0)("macro_actions") = macro_act 'maj de l'action
                            log("MAC :  -> Modif Timer :" & tabletemp(0)("macro_nom") & " Cond=" & macro_cond & " Action=" & macro_act, 6)
                        Else
                            log("MAC :  -> Modif Macro/Timer : ID=" & contenu(1) & " non trouvé : liste=" & liste, 2)
                        End If
                    End If

                    '---------------------------------- AL = LOG ----------------------------------
                ElseIf contenu(0) = "AL" Then 'on doit juste loguer : [AL#texte]
                    log("MAC :  -> " & contenu(1), 5)

                    '---------------------------------- ML = mail ----------------------------------
                ElseIf contenu(0) = "ML" Then 'on doit juste mailer : [ML#sujet#texte]
                    SendMessage(contenu(1), contenu(2), 0)

                    '-------------------------- AS = action sur le SERVICE ------------------------
                ElseIf contenu(0) = "AS" Then 'on execute une action sur le service  : [AS#action(#divers)]
                    log("MAC :  -> " & contenu(1), 5)
                    If contenu(1) = "stop" Then 'arret du service
                        If Serv_DOMOS Then
                            Dim controller As New ServiceController("DOMOS", ".")
                            controller.Stop()
                            [Stop]() 'svc_stop()
                        End If
                    ElseIf contenu(1) = "restart" Then 'restart du service
                        If Serv_DOMOS Then
                            svc_restart()
                        Else
                            svc_start()
                        End If
                    ElseIf contenu(1) = "maj" Or contenu(1) = "maj_all" Then 'maj des tables
                        If Serv_DOMOS Then tables_maj("ALL")
                    ElseIf contenu(1) = "maj_composants" Then 'maj de la table COMPOSANTS
                        If Serv_DOMOS Then tables_maj("COMPOSANTS")
                    ElseIf contenu(1) = "maj_composants_bannis" Then 'maj de la table COMPOSANTS_BANNIS
                        If Serv_DOMOS Then tables_maj("COMPOSANTS_BANNIS")
                    ElseIf contenu(1) = "maj_macro" Then 'maj de la table MACRO
                        If Serv_DOMOS Then tables_maj("MACRO")
                    ElseIf contenu(1) = "maj_timer" Then 'maj de la table TIMER
                        If Serv_DOMOS Then tables_maj("TIMER")
                    ElseIf contenu(1) = "afftables" Then 'Affiche les tables en memoires
                        tables_aff()
                    End If

                    '------------------------------- AN = Modules  -------------------------------
                ElseIf contenu(0) = "AN" Then 'Gestion des modules : [AN#module#action]
                    If contenu(1) = "SQL" Then 'module SQL [AN#SQL#optimise-purgelogs-reconnect]
                        If contenu(2) = "optimise" Then 'optimisation de la base mysql
                            log("MAC :  -> Optimisation de la base MySQL", 5)
                            err = mysql.mysql_nonquery("OPTIMIZE TABLE logs, releve, composants, macro, plan, users")
                            If err <> "" Then log("MAC : AN#SQL#optimise : " & err, 2)
                        ElseIf contenu(2) = "purgelogs" Then 'purge de logs > 2 mois
                            log("MAC :  -> Purge des logs de plus 2 mois", 5)
                            err = mysql.mysql_nonquery("DELETE FROM logs WHERE logs_date<'" & DateAdd(DateInterval.Month, -2, DateAndTime.Now).ToString("yyyy/MM/dd") & "'")
                            If err <> "" Then log("MAC : AN#SQL#purgelogs : " & err, 2)
                        ElseIf contenu(2) = "reconnect" Then 'deco/reco a la base SQL
                            log("MAC :  -> Déco/Reco à la base SQL", 5)
                            log("      -> Déco", 5)
                            err = mysql.mysql_close()
                            If err <> "" Then log("MAC : AN#SQL#reconnect : " & err, 2)
                            err = mysql.mysql_connect(mysql_ip, mysql_db, mysql_login, mysql_mdp)
                            log("      -> Reco", 5)
                            If err <> "" Then log("MAC : " & err, 2)
                        End If
                    End If

                    '---------------------------- AM = Lancer une macro ------------------------
                ElseIf contenu(0) = "AM" Then 'execution d'une macro : [AM#macros_id]
                    tabletemp = table_macros.Select("macro_id = '" & contenu(1) & "'")
                    If tabletemp.GetLength(0) = 1 Then 'macro trouvé
                        log("MAC : AM# : Analyse Macro : " & tabletemp(0)("macro_nom"), 5)
                        If analyse_cond(tabletemp(0)("macro_conditions"), "") Then 'verification des conditions
                            If tabletemp(0)("verrou") = False Then 'si la macro n'est pas déjà en cours d'execution
                                tabletemp(0)("verrou") = True
                                log("MAC : AM# : " & tabletemp(0)("macro_nom") & " : OK", 4)
                                action(tabletemp(0)("macro_actions"))
                                tabletemp(0)("verrou") = False
                            Else
                                log("MAC : AM# : macro " & tabletemp(0)("macro_nom") & " vérouillé", 2)
                            End If
                        Else
                            log("MAC : AM# : macro " & tabletemp(0)("macro_nom") & " conditions:" & tabletemp(0)("macro_conditions") & " pas bonnes", 2)
                        End If
                    Else
                        log("MAC : AM# : macro " & tabletemp(0)("macro_nom") & " n existe pas", 2)
                    End If

                    '---------------------------- EM = executer une macro (sans conditions) ------------------------
                ElseIf contenu(0) = "EM" Then 'execution d'une macro sans verifier les conditions: [AM#macros_id]
                    tabletemp = table_macros.Select("macro_id = '" & contenu(1) & "'")
                    If tabletemp.GetLength(0) = 1 Then 'macro trouvé
                        log("MAC : EM# : Execution Macro : " & tabletemp(0)("macro_nom"), 5)
                        If tabletemp(0)("verrou") = False Then 'si la macro n'est pas déjà en cours d'execution
                            tabletemp(0)("verrou") = True
                            log("MAC : EM# : " & tabletemp(0)("macro_nom") & " : OK", 4)
                            action(tabletemp(0)("macro_actions"))
                            tabletemp(0)("verrou") = False
                        Else
                            log("MAC : EM# : macro " & tabletemp(0)("macro_nom") & " vérouillé", 2)
                        End If
                    Else
                        log("MAC : EM# : macro " & tabletemp(0)("macro_nom") & " n existe pas", 2)
                    End If
                End If

                'on a pas fini de traiter la liste d'actions, on avance à l'element suivant
                If (posfin) < STRGS.Len(liste) Then
                    posdebut = posfin + 2
                    posfin = posfin + 2
                End If

                'pause entre chaque action de 0.1 seconde
                wait(10)
            End While
        Catch ex As Exception
            log("MAC : Action exception : " & ex.ToString & " --> " & liste, 2)
        End Try
    End Sub

    Private Class ECRIRE
        Private compid As Integer
        Private valeur As String = ""
        Private valeur2 As String = ""
        Private valeur3 As String = ""
        Private valeur4 As String = ""
        Public Sub New(ByVal id As Integer, ByVal val As String, ByVal val2 As String, ByVal val3 As String, ByVal val4 As String)
            compid = id
            valeur = val
            valeur2 = val2
            valeur3 = val3
            valeur4 = val4 'timer
        End Sub
        Public Sub action()
            Dim tabletmp As DataRow()
            Dim tblthread As DataRow()
            Dim limite As Integer = 0
            Dim err As String = ""
            Try
                tabletmp = table_composants.Select("composants_id = '" & compid & "'")
                If tabletmp.GetLength(0) > 0 Then
                    'si il y a un timer avant action, faire une pause de valeur4 secondes
                    If valeur4 <> "" Then
                        wait(valeur4)
                    End If
                    'suivant la norme du composant
                    Select Case tabletmp(0)("composants_modele_norme").ToString
                        Case "PLC"
                        	try
	                            'verification si on a pas déjà un thread qui ecrit sur le plcbus sinon on boucle pour attendre PLC_timeout/10 = 5 sec par défaut
                                SyncLock lock_tablethread
                                    tblthread = table_thread.Select("norme='PLC' AND source='ECR_PLC' AND composant_id<>'" & compid & "'")
                                End SyncLock
                                While (tblthread.GetLength(0) > 0 And limite < (PLC_timeout / 5))
                                    wait(5)
                                    SyncLock lock_tablethread
                                        tblthread = table_thread.Select("norme='PLC' AND source='ECR_PLC' AND composant_id<>'" & compid & "'")
                                    End SyncLock
                                    limite = limite + 1
                                End While
	                            If (limite < (PLC_timeout / 5)) Then 'on a attendu moins que le timeout
	                                'maj du thread pour dire qu'on ecrit sur le bus
                                    SyncLock lock_tablethread
                                        tblthread = table_thread.Select("norme='PLC' AND source='ECR' AND composant_id='" & compid & "'")
                                    End SyncLock
                                    If tblthread.GetLength(0) = 1 Then
                                        SyncLock lock_tablethread
                                            tblthread(0)("source") = "ECR_PLC"
                                        End SyncLock
                                        If valeur2 <> "" Then
                                            err = plcbus.ecrire(tabletmp(0)("composants_adresse"), valeur, valeur2, 0)
                                        Else
                                            err = plcbus.ecrire(tabletmp(0)("composants_adresse"), valeur, 0, 0)
                                        End If
                                        If STRGS.Left(err, 4) = "ERR:" Then
                                            log("ECR : PLC : " & err, 2)
                                        Else
                                            log("ECR : " & err, 5)
                                        End If
                                        wait(50) 'pause de 0.5sec pour recevoir le ack et libérer le bus correctement
                                    Else
                                        log("ECR : PLC Thread non trouvé : composant ID=" & compid, 2)
                                    End If
	                            Else
	                                log("ECR : Le port PLCBUS nest pas disponible : " & tabletmp(0)("composants_adresse") & "-" & valeur, 2)
	                            End If
				            Catch ex1 As Exception
				                log("ECR : PLC Exception : " & ex1.ToString & " --> ID=" & compid & " / " & valeur, 2)
				            End Try
                        Case "WIR"
                        	try
	                            Dim modelewir As String = tabletmp(0)("composants_modele_nom").ToString
	                            'on verifie que le modele est géré
	                            If modelewir = "DS18B20" Or modelewir = "DS2423_A" Or modelewir = "DS2423_B" Or modelewir = "DS2406_capteur" Or modelewir = "DS2406_relais" Then
	                                'Forcer le . 
	                                Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
	                                My.Application.ChangeCulture("en-US")
	
	                                'verification si on a pas déjà un thread qui ecrit sur le bus sinon on boucle pour attendre WIR_timeout/10 = 5 sec par défaut
                                    SyncLock lock_tablethread

                                    End SyncLock
                                    tblthread = table_thread.Select("norme='WIR' AND source='ECR_WIR' AND composant_id<>'" & compid & "'")
	                                While (tblthread.GetLength(0) > 0 And limite < (WIR_timeout / 10))
	                                    wait(10)
                                        SyncLock lock_tablethread
                                            tblthread = table_thread.Select("norme='WIR' AND source='ECR_WIR' AND composant_id<>'" & compid & "'")
                                        End SyncLock
                                        limite = limite + 1
	                                End While
	                                If (limite < (WIR_timeout / 10)) Then 'on a attendu moins que le timeout
	                                    'maj du thread pour dire qu'on ecrit sur le bus
                                        SyncLock lock_tablethread
                                            tblthread = table_thread.Select("norme='WIR' AND source='ECR' AND composant_id='" & compid & "'")
                                        End SyncLock
                                        If tblthread.GetLength(0) = 1 Then
                                            SyncLock lock_tablethread
                                                tblthread(0)("source") = "ECR_WIR"
                                            End SyncLock
                                            
                                            Dim wirvaleur, wirvaleur_etat, wirvaleur_activite As String
                                            Dim wirvaleur2 As Double
                                            Dim wirdateheure As String
                                            wirdateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)

                                            'traitement du DS18B20
                                            If modelewir = "DS18B20" Then
                                                'recuperation valeur et traitement
                                                If tabletmp(0)("composants_modele_norme").ToString() = "WIR" Then
                                                    wirvaleur = onewire.temp_get(tabletmp(0)("composants_adresse").ToString(), WIR_res)
                                                Else
                                                    wirvaleur = onewire2.temp_get(tabletmp(0)("composants_adresse").ToString(), WIR_res)
                                                End If
                                                If STRGS.Left(wirvaleur, 4) <> "ERR:" And IsNumeric(wirvaleur) Then 'si y a pas erreur d'acquisition, action
                                                    If (tabletmp(0)("composants_correction") <> "") Then
                                                        wirvaleur2 = Math.Round(CDbl(wirvaleur) + CDbl(tabletmp(0)("composants_correction")), 1) 'correction de la temperature
                                                    Else
                                                        wirvaleur2 = Math.Round(CDbl(wirvaleur), 1)
                                                    End If
                                                    'correction de l'etat si pas encore initialisé à une valeur
                                                    If tabletmp(0)("composants_etat").ToString() = "" Then tabletmp(0)("composants_etat") = 0
                                                    'comparaison du relevé avec le dernier etat
                                                    If wirvaleur2.ToString <> tabletmp(0)("composants_etat").ToString() Then
                                                        'on verifie si valeur=lastetat
                                                        If lastetat And wirvaleur2.ToString = tabletmp(0)("lastetat").ToString() Then
                                                            log("ECR : WIR DS18B20 : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & wirvaleur2 & "°C (inchangé lastetat)", 8)
                                                            '--- Modification de la date dans la base SQL ---
                                                            err = mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & wirdateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                                            If err <> "" Then log("ECR : WIR DS18B20 : inchange lastetat : " & err, 2)
                                                        Else
                                                            'on vérifie que la valeur a changé de plus de composants_precision sinon inchangé
                                                            If (CDbl(wirvaleur2) + CDbl(tabletmp(0)("composants_precision"))) >= CDbl(tabletmp(0)("composants_etat")) And (CDbl(wirvaleur2) - CDbl(tabletmp(0)("composants_precision"))) <= CDbl(tabletmp(0)("composants_etat")) Then
                                                                log("ECR : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & wirvaleur2 & "°C (inchangé precision)", 8)
                                                                '--- Modification de la date dans la base SQL ---
                                                                err = mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & wirdateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                                                If err <> "" Then log("ECR : WIR DS18B20 : inchange precision : " & err, 2)
                                                            Else
                                                                'la valeur a changé, on modifie le composant
                                                                log("ECR : WIR DS18B20 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & wirvaleur2 & "°C ", 6)
                                                                '--- modification de l'etat du composant dans la table en memoire ---
                                                                tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                                                                tabletmp(0)("composants_etat") = wirvaleur2
                                                                tabletmp(0)("composants_etatdate") = wirdateheure
                                                            End If
                                                        End If
                                                    Else
                                                        'la valeur n a pas changé
                                                        log("ECR : WIR DS18B20 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & wirvaleur2 & "°C (inchangé)", 7)
                                                        '--- Modification de la date dans la base SQL ---
                                                        err = mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & wirdateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                                        If err <> "" Then log("ECR : WIR DS18B20 : inchange : " & err, 2)
                                                    End If
                                                Else
                                                    'erreur
                                                    log("ECR : WIR DS18B20 : erreur acquisition valeur : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & wirvaleur, 2)
                                                End If
                                            ElseIf modelewir = "DS2423_A" Or modelewir = "DS2423_B" Then
                                                'traitement des compteurs DS2423
                                                Dim AorB As Boolean
                                                'on verifie quel compteur on interroge
                                                If modelewir = "DS2423_A" Then AorB = True Else AorB = False
                                                'recuperation de la valeur
                                                wirvaleur = onewire.counter(tabletmp(0)("composants_adresse").ToString(), AorB)
                                                If STRGS.Left(wirvaleur, 4) <> "ERR:" And IsNumeric(wirvaleur) Then 'si y a pas erreur d'acquisition, action
                                                    '--- comparaison du relevé avec le dernier etat ---
                                                    If wirvaleur <> tabletmp(0)("composants_etat").ToString() Then
                                                        log("ECR : WIR DS2423 : " & tabletmp(0)("composants_adresse").ToString() & " : " & wirvaleur & " ", 6)
                                                        '--- modification de l'etat du composant dans la table en memoire ---
                                                        tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                                                        tabletmp(0)("composants_etat") = wirvaleur
                                                        tabletmp(0)("composants_etatdate") = wirdateheure
                                                    Else
                                                        'la valeur n a pas changé
                                                        log("ECR : WIR DS2423 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & wirvaleur & " (inchangé)", 7)
                                                        err = mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & wirdateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                                        If err <> "" Then log("ECR : WIR DS2423 : inchange : " & err, 2)
                                                    End If
                                                Else
                                                    'erreur
                                                    log("ECR : WIR DS2423 : erreur acquisition valeur : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & wirvaleur, 2)
                                                End If
                                            ElseIf modelewir = "DS2406_capteur" Then
                                                'traitement des switch DS2406_capteur
                                                wirvaleur = onewire.switch_get(tabletmp(0)("composants_adresse").ToString())
                                                If STRGS.Left(wirvaleur, 4) <> "ERR:" Then 'si y a pas erreur d'acquisition, action
                                                    '--- comparaison du relevé avec le dernier etat ---
                                                    wirvaleur_etat = STRGS.Left(wirvaleur, 1)
                                                    wirvaleur_activite = STRGS.Right(wirvaleur, 1)
                                                    If wirvaleur_activite <> tabletmp(0)("composants_etat").ToString() Then
                                                        log("WIR (ECR): DS2406_capteur : " & tabletmp(0)("composants_adresse").ToString() & " : " & wirvaleur_activite & " ", 6)
                                                        '--- modification de l'etat du composant dans la table en memoire ---
                                                        tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                                                        tabletmp(0)("composants_etat") = wirvaleur_activite
                                                        tabletmp(0)("composants_etatdate") = wirdateheure
                                                    Else
                                                        'la valeur n a pas changé
                                                        log("ECR : WIR DS2406_capteur : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & wirvaleur_activite & " (inchangé)", 7)
                                                        err = mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & wirdateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                                        If err <> "" Then log("ECR : WIR DS2406_capteur : inchange : " & err, 2)
                                                    End If
                                                Else
                                                    'erreur
                                                    log("ECR : WIR DS2406_capteur : erreur acquisition valeur : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & wirvaleur, 2)
                                                End If
                                            ElseIf modelewir = "DS2406_relais" Then
                                                'traitement des switch DS2406_relais
                                                log("ECR : WIR DS2406_relais : pas encore géré", 2)
                                            End If
                                            wait(50) 'pause de 0.5sec libérer le bus correctement

                                        Else
                                            log("ECR : WIR : Thread non trouvé : composant ID=" & compid, 2)
                                        End If
	                                Else
	                                    log("ECR : WIR Le bus 1-wire nest pas disponible : " & tabletmp(0)("composants_nom") & "-" & valeur, 2)
	                                End If
	                            Else
	                                log("ECR : WIR Modele 1-wire non géré : " & modelewir & " comp: " & tabletmp(0)("composants_nom") & " : " & modelewir.ToString, 2)
	                            End If
				            Catch ex1 As Exception
				                log("ECR : WIR Exception : " & ex1.ToString & " --> ID=" & compid & " / " & valeur, 2)
				            End Try
                        Case "WI2"
                            '???
                        Case "VIR"
                            '???
                        Case "RFX"
                            '???
                        Case "X10"
                        	try
	                            'verification si on a pas déjà un thread qui ecrit sur le x10 sinon on boucle pour attendre X10_timeout/10 = 5 sec par défaut
                                SyncLock lock_tablethread
                                    tblthread = table_thread.Select("norme='X10' AND source='ECR_X10' AND composant_id<>'" & compid & "'")
                                End SyncLock
                                While (tblthread.GetLength(0) > 0 And limite < (X10_timeout / 10))
                                    wait(10)
                                    SyncLock lock_tablethread
                                        tblthread = table_thread.Select("norme='X10' AND source='ECR_X10' AND composant_id<>'" & compid & "'")
                                    End SyncLock
                                    limite = limite + 1
                                End While
	                            If (limite < (X10_timeout / 10)) Then 'on a attendu moins que le timeout
	                                'maj du thread pour dire qu'on ecrit sur le bus
                                    SyncLock lock_tablethread
                                        tblthread = table_thread.Select("norme='X10' AND source='ECR' AND composant_id='" & compid & "'")
                                    End SyncLock
                                    If tblthread.GetLength(0) = 1 Then
                                        SyncLock lock_tablethread
                                            tblthread(0)("source") = "ECR_X10"
                                        End SyncLock
                                        If valeur2 <> "" Then
                                            err = x10.ecrire(tabletmp(0)("composants_adresse"), valeur, valeur2)
                                        Else
                                            err = x10.ecrire(tabletmp(0)("composants_adresse"), valeur, 0)
                                        End If
                                        If STRGS.Left(err, 4) = "ERR:" Then
                                            log("ECR : X10 : " & err, 2)
                                        Else
                                            log("ECR : X10 : " & err, 5)
                                        End If
                                        wait(50) 'pause de 0.5sec pour recevoir le ack et libérer le bus correctement
                                    Else
                                        log("ECR : X10 : Thread non trouvé : composant ID=" & compid, 2)
                                    End If
	                            Else
	                                log("ECR : Le port X10 nest pas disponible : " & tabletmp(0)("composants_adresse") & "-" & valeur, 2)
	                            End If
				            Catch ex1 As Exception
				                log("ECR : X10 Exception : " & ex1.ToString & " --> ID=" & compid & " / " & valeur, 2)
				            End Try
                        Case "ZIB"
                        	try
	                            'verification si on a pas déjà un thread qui ecrit sur le zib sinon on boucle pour attendre ZIB_timeout/10 = 5 sec par défaut
                                SyncLock lock_tablethread
                                    tblthread = table_thread.Select("norme='ZIB' AND source='ECR_ZIB' AND composant_id<>'" & compid & "'")
                                End SyncLock
                                While (tblthread.GetLength(0) > 0 And limite < (ZIB_timeout / 10))
                                    wait(10)
                                    SyncLock lock_tablethread
                                        tblthread = table_thread.Select("norme='ZIB' AND source='ECR_ZIB' AND composant_id<>'" & compid & "'")
                                    End SyncLock
                                    limite = limite + 1
                                End While
	                            If (limite < (ZIB_timeout / 10)) Then 'on a attendu moins que le timeout
	                                'maj du thread pour dire qu'on ecrit sur le bus
                                    SyncLock lock_tablethread
                                        tblthread = table_thread.Select("norme='ZIB' AND source='ECR' AND composant_id='" & compid & "'")
                                    End SyncLock
                                    If tblthread.GetLength(0) = 1 Then
                                        SyncLock lock_tablethread
                                            tblthread(0)("source") = "ECR_ZIB"
                                        End SyncLock
                                        If valeur2 <> "" Then
                                            err = zibase.Ecrirecommand(tabletmp(0)("composants_id"), valeur, valeur2)
                                        Else
                                            err = zibase.Ecrirecommand(tabletmp(0)("composants_id"), valeur, 0)
                                        End If
                                        If STRGS.Left(err, 4) = "ERR:" Then
                                            log("ECR : ZIB : " & err, 2)
                                        Else
                                            log("ECR : ZIB : " & err, 5)
                                        End If
                                        wait(50) 'pause de 0.5sec pour libérer le bus correctement
                                    Else
                                        log("ECR : ZIB : Thread non trouvé : composant ID=" & compid, 2)
                                    End If
	                            Else
	                                log("ECR : Le port ZIB nest pas disponible pour une ecriture : " & tabletmp(0)("composants_adresse") & "-" & valeur, 2)
	                            End If
				            Catch ex1 As Exception
				                log("ECR : ZIB Exception : " & ex1.ToString & " --> ID=" & compid & " / " & valeur, 2)
				            End Try
                        Case Else
                            log("ECR : norme non gérée : " & tabletmp(0)("composants_modele_norme").ToString & " comp: " & tabletmp(0)("composants_adresse").ToString, 2)
                    End Select
                Else
                    log("ECR : Composant non trouvé dans la table : " & compid, 2)
                End If

            Catch ex As Exception
                log("ECR : Exception Traitement : " & ex.ToString, 2)
            End Try
            
            Try
                '--- suppresion du thread de la liste des threads lancés ---
                SyncLock lock_tablethread
                    tabletmp = table_thread.Select("composant_id = '" & compid & "'")
                End SyncLock
                If tabletmp.GetLength(0) >= 1 Then
                    SyncLock lock_tablethread
                        tabletmp(0).Delete()
                    End SyncLock
                Else
                    log("ECR : Thread non trouvé (pour delete) : composant ID=" & compid, 2)
                End If
            Catch ex As Exception
                log("ECR : Exeption : Suppression thread liste : composant ID=" & compid & " -> " & ex.ToString, 2)
            End Try

        End Sub
    End Class

    'Private Class POL_DS18B20
    '    Private _id As Integer
    '    Public Sub New(ByVal composant_id As Integer)
    '        _id = composant_id
    '    End Sub
    '    Public Sub Execute()
    '        'Forcer le . 
    '        Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
    '        My.Application.ChangeCulture("en-US")

    '        Dim valeur As String
    '        Dim valeur2 As Double
    '        Dim wir1_2 As Boolean = False '= true si sur le deuxieme bus 1-wire
    '        Dim tabletmp() As DataRow
    '        Dim dateheure, Err As String
    '        Try
    '            tabletmp = table_composants.Select("composants_id = '" & _id & "'")
    '            '--- test pour savoir si on est sur la premiere ou deuxieme clé WIR/WI2 ---
    '            If tabletmp(0)("composants_modele_norme").ToString() = "WIR" Then
    '                valeur = onewire.temp_get(tabletmp(0)("composants_adresse").ToString(), WIR_res)
    '            Else
    '                valeur = onewire2.temp_get(tabletmp(0)("composants_adresse").ToString(), WIR_res)
    '            End If
    '            If STRGS.Left(valeur, 4) <> "ERR:" Then 'si y a pas erreur d'acquisition, action
    '                If (tabletmp(0)("composants_correction") <> "") Then
    '                    valeur2 = Math.Round(CDbl(valeur) + CDbl(tabletmp(0)("composants_correction")), 1) 'correction de la temperature
    '                Else
    '                    valeur2 = Math.Round(CDbl(valeur), 1)
    '                End If
    '                'correction de l'etat si pas encore initialisé à une valeur
    '                If tabletmp(0)("composants_etat").ToString() = "" Then tabletmp(0)("composants_etat") = 0
    '                'comparaison du relevé avec le dernier etat
    '                If valeur2.ToString <> tabletmp(0)("composants_etat").ToString() Then
    '                    If domos_svc.lastetat And valeur2.ToString = tabletmp(0)("lastetat").ToString() Then
    '                        domos_svc.log("POL : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur2 & "°C (inchangé lastetat)", 8)
    '                        '--- Modification de la date dans la base SQL ---
    '                        dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
    '                        Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
    '                        If Err <> "" Then log("SQL: table_comp_changed " & Err, 2)
    '                    Else
    '                        If IsNumeric(valeur2) Then
    '                            'on vérifie que la valeur a changé de plus de composants_precision sinon inchangé
    '                            'If (valeur2 + CDbl(tabletmp(0)("composants_precision"))).ToString >= tabletmp(0)("composants_etat").ToString() And (valeur2 - CDbl(tabletmp(0)("composants_precision"))).ToString <= tabletmp(0)("composants_etat").ToString() Then
    '                            If (CDbl(valeur2) + CDbl(tabletmp(0)("composants_precision"))) >= CDbl(tabletmp(0)("composants_etat")) And (CDbl(valeur2) - CDbl(tabletmp(0)("composants_precision"))) <= CDbl(tabletmp(0)("composants_etat")) Then
    '                                domos_svc.log("POL : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur2 & "°C (inchangé precision)", 8)
    '                                '--- Modification de la date dans la base SQL ---
    '                                dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
    '                                Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
    '                                If Err <> "" Then log("SQL: table_comp_changed " & Err, 2)
    '                            Else
    '                                domos_svc.log("POL : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur2 & "°C ", 6)
    '                                '--- modification de l'etat du composant dans la table en memoire ---
    '                                tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
    '                                tabletmp(0)("composants_etat") = valeur2
    '                                tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
    '                            End If
    '                        Else
    '                            domos_svc.log("POL : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur2 & "°C ", 6)
    '                            '--- modification de l'etat du composant dans la table en memoire ---
    '                            tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
    '                            tabletmp(0)("composants_etat") = valeur2
    '                            tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
    '                        End If
    '                    End If
    '                Else
    '                    domos_svc.log("POL : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur2 & "°C (inchangé)", 7)
    '                    '--- Modification de la date dans la base SQL ---
    '                    dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
    '                    Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
    '                    If Err <> "" Then log("SQL: table_comp_changed " & Err, 2)
    '                End If
    '            Else
    '                'erreur
    '                domos_svc.log("POL : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur, 2)
    '            End If
    '            '--- suppresion du thread de la liste des thread lancé ---
    '            tabletmp = table_thread.Select("composant_id = '" & _id & "'")
    '            If tabletmp.GetUpperBound(0) >= 0 Then
    '                tabletmp(0).Delete()
    '            End If
    '        Catch ex As Exception
    '            '--- suppresion du thread de la liste des thread lancé ---
    '            tabletmp = table_thread.Select("composant_id = '" & _id & "'")
    '            If tabletmp.GetUpperBound(0) >= 0 Then
    '                tabletmp(0).Delete()
    '            End If
    '            domos_svc.log("POL : DS18B20 " & ex.ToString, 2)
    '        End Try
    '    End Sub
    'End Class

    'Private Class POL_DS2406_capteur
    '    Private _id As Integer
    '    Public Sub New(ByVal composant_id As Integer)
    '        _id = composant_id
    '    End Sub
    '    Public Sub Execute()
    '        'Forcer le . 
    '        Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
    '        My.Application.ChangeCulture("en-US")

    '        Dim valeur, valeur_etat, valeur_activite As String
    '        Dim tabletmp() As DataRow
    '        Try
    '            tabletmp = table_composants.Select("composants_id = '" & _id & "'")
    '            valeur = onewire.switch_get(tabletmp(0)("composants_adresse").ToString())
    '            If STRGS.Left(valeur, 4) <> "ERR:" Then 'si y a pas erreur d'acquisition, action
    '                '--- comparaison du relevé avec le dernier etat ---
    '                valeur_etat = STRGS.Left(valeur, 1)
    '                valeur_activite = STRGS.Right(valeur, 1)
    '                If valeur_activite <> tabletmp(0)("composants_etat").ToString() Then
    '                    domos_svc.log("POL DS2406_capteur : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur_activite & " ", 6)
    '                    '--- modification de l'etat du composant dans la table en memoire ---
    '                    tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
    '                    tabletmp(0)("composants_etat") = valeur_activite
    '                    tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
    '                Else
    '                    domos_svc.log("POL DS2406_capteur : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur_activite, 7)
    '                End If
    '            Else
    '                'erreur
    '                domos_svc.log("POL DS2406_capteur : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur, 2)
    '            End If
    '            '--- suppresion du thread de la liste des thread lancé ---
    '            tabletmp = table_thread.Select("composant_id = '" & _id & "'")
    '            If tabletmp.GetUpperBound(0) >= 0 Then
    '                tabletmp(0).Delete()
    '            End If
    '        Catch ex As Exception
    '            '--- suppresion du thread de la liste des thread lancé ---
    '            tabletmp = table_thread.Select("composant_id = '" & _id & "'")
    '            If tabletmp.GetUpperBound(0) >= 0 Then
    '                tabletmp(0).Delete()
    '            End If
    '            domos_svc.log("POL : DS2406_capteur " & ex.ToString, 2)
    '        End Try

    '    End Sub
    'End Class

    'Private Class POL_DS2423
    '    Private _id As Integer
    '    Private _AorB As Boolean
    '    Public Sub New(ByVal composant_id As Integer, ByVal AorB As Boolean)
    '        _id = composant_id
    '        _AorB = AorB
    '    End Sub
    '    Public Sub Execute()
    '        'Forcer le . 
    '        Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
    '        My.Application.ChangeCulture("en-US")

    '        Dim valeur As String
    '        Dim tabletmp() As DataRow
    '        Try
    '            tabletmp = table_composants.Select("composants_id = '" & _id & "'")
    '            valeur = onewire.counter(tabletmp(0)("composants_adresse").ToString(), _AorB)
    '            If STRGS.Left(valeur, 4) <> "ERR:" Then 'si y a pas erreur d'acquisition, action
    '                '--- comparaison du relevé avec le dernier etat ---
    '                If valeur <> tabletmp(0)("composants_etat").ToString() Then
    '                    domos_svc.log("POL DS2423 : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur & " ", 6)
    '                    '--- modification de l'etat du composant dans la table en memoire ---
    '                    tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
    '                    tabletmp(0)("composants_etat") = valeur
    '                    tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)

    '                Else
    '                    domos_svc.log("POL DS2423 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur, 7)
    '                End If
    '            Else
    '                'erreur
    '                domos_svc.log("POL DS2423 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur, 2)
    '            End If
    '            '--- suppresion du thread de la liste des thread lancé ---
    '            tabletmp = table_thread.Select("composant_id = '" & _id & "'")
    '            If tabletmp.GetUpperBound(0) >= 0 Then
    '                tabletmp(0).Delete()
    '            End If
    '        Catch ex As Exception
    '            '--- suppresion du thread de la liste des thread lancé ---
    '            tabletmp = table_thread.Select("composant_id = '" & _id & "'")
    '            If tabletmp.GetUpperBound(0) >= 0 Then
    '                tabletmp(0).Delete()
    '            End If
    '            domos_svc.log("POL : DS2423 " & ex.ToString, 2)
    '        End Try

    '    End Sub
    'End Class

End Class
