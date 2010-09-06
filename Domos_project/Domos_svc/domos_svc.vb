Imports STRGS = Microsoft.VisualBasic.Strings
Imports System.Threading
Imports System.Timers
Imports System.IO
Imports System.Globalization
Imports Microsoft.Win32
Imports System.ServiceProcess

Public Class domos_svc
    'declaration de mes classes_
    Shared onewire As New onewire
    Shared onewire2 As New onewire
    Shared socket As New sockets
    Public Shared mysql As New mysql
    Shared rfxcom As New rfxcom
    Shared plcbus As New plcbus

    'variable interne au script
    Public Shared table_config, table_composants, table_composants_bannis, table_macros, table_timer, table_thread As New DataTable
    Public Shared Serv_DOMOS, Serv_WIR, Serv_WI2, Serv_PLC, Serv_X10, Serv_RFX, Serv_VIR, Serv_SOC As Boolean
    Public Shared Port_PLC, Port_X10, Port_RFX, Port_WIR, Port_WI2, socket_ip, WIR_adaptername As String
    Public Shared PLC_timeout, Action_timeout, rfx_tpsentrereponse, socket_port, lastetat, WIR_res As Integer
    Public Shared heure_coucher_correction, heure_lever_correction As Integer
    Public Shared gps_longitude, gps_latitude As String
    Dim WithEvents timer_pool, timer_timer As New System.Timers.Timer
    Dim err As String = ""
    Public Shared log_niveau As String
    Public Shared parametrevb
    Public Shared log_dest As Integer
    Private mysql_ip, mysql_db, mysql_login, mysql_mdp As String
    Private Shared install_dir As String
    Private controller As New ServiceController("DOMOS", ".")
    Private etape_startup As Integer = 0

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

        '---- initialisation des variables par défaut ----
        Serv_WIR = False
        Serv_WI2 = False
        Serv_PLC = False
        Serv_X10 = False
        Serv_RFX = False
        Serv_DOMOS = False
        Serv_SOC = False
        log_niveau = "0-1-2-3-4-5-6-7-8-9-A-B-C-D-E-F" 'log tous les msgs
        log_dest = 2 '0=txt, 1=sql, 2=txt+sql
        PLC_timeout = 500
        Action_timeout = 500
        rfx_tpsentrereponse = 1500
        lastetat = 1
        WIR_res = 0.1
        WIR_adaptername = ""
        heure_coucher_correction = 0
        heure_lever_correction = 0
        gps_longitude = 0
        gps_latitude = 0

        svc_start()
    End Sub

    Protected Overrides Sub OnStop()
        svc_stop()
    End Sub

    Protected Overrides Sub OnCustomCommand(ByVal command As Integer)
        If command = 200 Then
            EventLog.WriteEntry("Custom Command 200 : SQL-optimise")
            action("([AN#SQL#optimise])")
        ElseIf command = 201 Then
            EventLog.WriteEntry("Custom Command 201 : SQL-purgelogs")
            action("([AN#SQL#purgelogs])")
        ElseIf command = 202 Then
            EventLog.WriteEntry("Custom Command 202 : SQL-reconnect")
            action("([AN#SQL#reconnect])")
        ElseIf command = 210 Then
            EventLog.WriteEntry("Custom Command 210 : AFFICHE-tables")
            action("([AS#afftables])")
        ElseIf command = 211 Then
            EventLog.WriteEntry("Custom Command 211 : MAJ-tables")
            action("([AS#maj_all])")
        ElseIf command = 212 Then
            EventLog.WriteEntry("Custom Command 212 : MAJ-table_composants")
            action("([AS#maj_composants])")
        ElseIf command = 213 Then
            EventLog.WriteEntry("Custom Command 213 : MAJ-table_composants_bannis")
            action("([AS#maj_composants_bannis])")
        ElseIf command = 214 Then
            EventLog.WriteEntry("Custom Command 214 : MAJ-table_macros")
            action("([AS#maj_macro])")
        ElseIf command = 215 Then
            EventLog.WriteEntry("Custom Command 215 : MAJ-table_timer")
            action("([AS#maj_timer])")
        End If
    End Sub

    Private Sub svc_start()
        Try
            log("-------------------------------------------------------------------------------", 0)
            Dim x As New DataColumn

            '---------- Récupération configuration Registry ----------
            etape_startup = 1
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
            err = mysql.mysql_connect(mysql_ip, mysql_db, mysql_login, mysql_mdp)
            If err <> "" Then
                log("", 1)
                log("--- Démarrage du Service ---", 1)
                log("SQL : Connexion au serveur " & mysql_ip & " :", 1)
                log("     -> " & err, 1)
                log("--- Service Arrété ---", 1)
                [Stop]()
                Exit Sub
            End If
            log("", 0)
            log("--- Démarrage du Service ---", 0)
            log("--- Démarrage du Service ---", -1)
            log("", 0)
            log("SQL : Connexion au serveur " & mysql_ip & " :", 0)
            log("     -> Connecté à " & mysql_ip & ":" & mysql_db & " avec " & mysql_login & "/" & mysql_mdp, 0)
            log("SQL : Connecté à " & mysql_ip & ":" & mysql_db & " avec " & mysql_login & "/" & mysql_mdp, -1)
            log("", 0)

            '----- recupération de la config -----
            etape_startup = 3
            log("SQL : Récupération de la configuration :", 0)
            log("SQL : Récupération de la configuration", -1)
            err = Table_maj_sql(table_config, "SELECT config_nom,config_valeur FROM config")
            If table_config.Rows.Count() > 0 Then
                log_niveau = table_config.Select("config_nom = 'log_niveau'")(0)("config_valeur")
                log_dest = table_config.Select("config_nom = 'log_dest'")(0)("config_valeur")
                Serv_WIR = table_config.Select("config_nom = 'Serv_WIR'")(0)("config_valeur")
                Serv_WI2 = table_config.Select("config_nom = 'Serv_WI2'")(0)("config_valeur")
                Serv_PLC = table_config.Select("config_nom = 'Serv_PLC'")(0)("config_valeur")
                Serv_X10 = table_config.Select("config_nom = 'Serv_X10'")(0)("config_valeur")
                Serv_RFX = table_config.Select("config_nom = 'Serv_RFX'")(0)("config_valeur")
                Serv_VIR = table_config.Select("config_nom = 'Serv_VIR'")(0)("config_valeur")
                Serv_SOC = table_config.Select("config_nom = 'Serv_SOC'")(0)("config_valeur")
                Port_RFX = table_config.Select("config_nom = 'Port_RFX'")(0)("config_valeur")
                Port_X10 = table_config.Select("config_nom = 'Port_X10'")(0)("config_valeur")
                Port_PLC = table_config.Select("config_nom = 'Port_PLC'")(0)("config_valeur")
                Port_WIR = table_config.Select("config_nom = 'Port_WIR'")(0)("config_valeur")
                Port_WI2 = table_config.Select("config_nom = 'Port_WI2'")(0)("config_valeur")
                PLC_timeout = table_config.Select("config_nom = 'PLC_timeout'")(0)("config_valeur")
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
                log("     -> LOG_NIVEAU=" & log_niveau & " LOG_DESTINATION=" & log_dest, 0)
                log("     -> WIR=" & Serv_WIR & " WI2=" & Serv_WI2 & " PLC=" & Serv_PLC & ":" & Port_PLC & " X10=" & Serv_X10 & ":" & Port_X10 & " RFX=" & Serv_RFX & ":" & Port_RFX, 0)
                log("     -> Action_timeout=" & Action_timeout & " PLC_timeout=" & PLC_timeout & " RFX_tpsentrereponse=" & rfx_tpsentrereponse & " Lastetat=" & lastetat, 0)
                log("     -> heure_lever_correction=" & heure_lever_correction & " heure_coucher_correction=" & heure_coucher_correction, 0)
                log("     -> longitude=" & gps_longitude & " latitude=" & gps_latitude, 0)
                log("     -> WIR_res=" & WIR_res & " WIR_adaptername=" & WIR_adaptername, 0)
                log("     -> Socket Activé=" & Serv_SOC & " IP=" & socket_ip & " Port=" & socket_port, 0)
            Else
                log("     -> ERR: pas de données récupérées : fermeture du programme", 1)
                'svc_stop()
                [Stop]()
                Exit Sub
            End If
            log("", 0)

            '---------- Initialisation de la clé USB 1-wire -------
            etape_startup = 4
            If Serv_WIR Then
                err = onewire.initialisation(WIR_adaptername, Port_WIR)
                If STRGS.Left(err, 4) = "ERR:" Then
                    Serv_WIR = False 'desactivation du onewire car la clé USB n'est pas dispo
                    log("WIR : " & err, 2)
                    log("     -> Désactivation du servive OneWire", 0)
                Else
                    log("WIR : " & err, 0)
                End If
                log("WIR : " & err, -1)
            End If
            '---------- Initialisation de la clé USB 1-wire 2 -------
            etape_startup = 5
            If Serv_WI2 Then
                err = onewire2.initialisation(WIR_adaptername, Port_WI2)
                If STRGS.Left(err, 4) = "ERR:" Then
                    Serv_WI2 = False 'desactivation du onewire car la clé USB n'est pas dispo
                    log("WI2 : " & err, 2)
                    log("     -> Désactivation du servive OneWire", 0)
                Else
                    log("WI2 : " & err, 0)
                End If
                log("WI2 : " & err, -1)
            End If
            '---------- Initialisation du RFXCOM -------
            etape_startup = 6
            If Serv_RFX Then
                err = rfxcom.ouvrir(Port_RFX)
                If STRGS.Left(err, 4) = "ERR:" Then
                    Serv_RFX = False 'desactivation du RFXCOM car erreur d'ouverture
                    log("RFX : " & err, 2)
                    log("     -> Désactivation du servive RFXCOM", 0)
                Else
                    log("RFX : " & err, 0)
                End If
                log("RFX : " & err, -1)
            End If
            '---------- Initialisation du PLCBUS -------
            etape_startup = 7
            If Serv_PLC Then
                err = plcbus.ouvrir(Port_PLC)
                If STRGS.Left(err, 4) = "ERR:" Then
                    Serv_PLC = False 'desactivation du PLCBUS car erreur d'ouverture
                    log("PLC : " & err, 2)
                    log("     -> Désactivation du servive PLCBUS", 0)
                Else
                    log("PLC : " & err, 0)
                End If
                log("PLC : " & err, -1)
            End If
            log("", 0)

            '----- recupération de la liste des composants actifs -----
            etape_startup = 8
            log("SQL : Récupération de la liste des composants actifs :", 0)
            log("SQL : Récupération de la liste des composants actifs", -1)
            Dim Condition_service As String = ""
            If Not Serv_PLC Then Condition_service &= " AND composants_modele_norme<>'PLC'"
            If Not Serv_WIR Then Condition_service &= " AND composants_modele_norme<>'WIR'"
            If Not Serv_WI2 Then Condition_service &= " AND composants_modele_norme<>'WI2'"
            If Not Serv_X10 Then Condition_service &= " AND composants_modele_norme<>'X10'"
            If Not Serv_RFX Then Condition_service &= " AND composants_modele_norme<>'RFX'"
            If Not Serv_VIR Then Condition_service &= " AND composants_modele_norme<>'VIR'"
            err = Table_maj_sql(table_composants, "SELECT composants.*,composants_modele.* FROM composants,composants_modele WHERE composants_modele=composants_modele_id AND composants_actif='1'" & Condition_service)
            If err <> "" Then
                log("SQL : " & err, 1)
                log("Fermeture du programme", 1)
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
                        log("     -> Id : " & table_composants.Rows(i).Item("composants_id") & " -- Nom : " & table_composants.Rows(i).Item("composants_nom") & " -- Adresse : " & table_composants.Rows(i).Item("composants_adresse") & " -- Valeur : " & table_composants.Rows(i).Item("composants_etat") & " -- Polling : " & table_composants.Rows(i).Item("composants_polling") & " -- Type : " & table_composants.Rows(i).Item("composants_modele_norme") & "-" & table_composants.Rows(i).Item("composants_modele_nom"), 0)
                    Next
                Else
                    log("     -> Aucun composant trouvé : fermeture du programme", 1)
                    'svc_stop()
                    [Stop]()
                    Exit Sub
                End If
            End If

            '---------- Initialisation des heures du soleil -------
            etape_startup = 9
            log("Initialisation des heures du soleil", 0)
            'soleil.City("Algrange")
            soleil.City_gps(gps_longitude, gps_latitude)
            soleil.CalculateSun()
            var_soleil_lever = soleil.Sunrise
            var_soleil_coucher = soleil.Sunset
            var_soleil_coucher2 = DateAdd(DateInterval.Minute, heure_coucher_correction, var_soleil_coucher)
            var_soleil_lever2 = DateAdd(DateInterval.Minute, heure_lever_correction, var_soleil_lever)
            log("     -> Heure du lever : " & var_soleil_lever & " (" & var_soleil_lever2 & ")", 0)
            log("     -> Heure du coucher : " & var_soleil_coucher & " (" & var_soleil_coucher2 & ")", 0)
            log("Initialisation des heures du soleil : " & var_soleil_lever & " (" & var_soleil_lever2 & ") - " & var_soleil_coucher & " (" & var_soleil_coucher2 & ")", -1)
            '---------- Calcul pour savoir si on est de jour ou nuit -------
            Dim tabletemp = table_composants.Select("composants_adresse = 'jour'")
            Dim dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
            If tabletemp.GetLength(0) = 1 Then
                If DateAndTime.Now.ToString("HH:mm:ss") > var_soleil_lever And DateAndTime.Now.ToString("HH:mm:ss") < var_soleil_coucher Then
                    tabletemp(0)("composants_etat") = "1" 'maj du composant virtuel JOUR à jour
                    err = mysql.mysql_nonquery("UPDATE composants SET composants_etat='1',composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletemp(0)("composants_id") & "'")
                    If err <> "" Then log("SQL: Start Maj Jour " & err, 2)
                    log("     -> Maj composant virtuel JOUR : JOUR (1)", 0)
                Else
                    tabletemp(0)("composants_etat") = "0" 'maj du composant virtuel JOUR à nuit
                    err = mysql.mysql_nonquery("UPDATE composants SET composants_etat='0',composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletemp(0)("composants_id") & "'")
                    If err <> "" Then log("SQL: Start Maj Jour " & err, 2)
                    log("     -> Maj composant virtuel JOUR : NUIT (0)", 0)
                End If
            Else
                log("     -> ERR: Maj composant virtuel JOUR : Non trouvé", 1)
            End If
            '---------- Calcul pour savoir si on est de jour ou nuit avec heures corrigés-------
            tabletemp = table_composants.Select("composants_adresse = 'jour2'")
            If tabletemp.GetLength(0) = 1 Then
                If DateAndTime.Now.ToString("HH:mm:ss") > var_soleil_lever2 And DateAndTime.Now.ToString("HH:mm:ss") < var_soleil_coucher2 Then
                    tabletemp(0)("composants_etat") = "1" 'maj du composant virtuel JOUR à jour
                    err = mysql.mysql_nonquery("UPDATE composants SET composants_etat='1',composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletemp(0)("composants_id") & "'")
                    If err <> "" Then log("SQL: Start Maj Jour " & err, 2)
                    log("     -> Maj composant virtuel JOUR2 : JOUR (1)", 0)
                Else
                    tabletemp(0)("composants_etat") = "0" 'maj du composant virtuel JOUR à nuit
                    err = mysql.mysql_nonquery("UPDATE composants SET composants_etat='0',composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletemp(0)("composants_id") & "'")
                    If err <> "" Then log("SQL: Start Maj Jour " & err, 2)
                    log("     -> Maj composant virtuel JOUR2 : NUIT (0)", 0)
                End If
            Else
                log("     -> ERR: Maj composant virtuel JOUR : Non trouvé", 1)
            End If
            log("", 0)


            '---------- Ajout d'un handler sur la modification de l'etat d'un composant -------
            etape_startup = 10
            log("Lancement de l'handler sur l etat des composants", 0)
            AddHandler table_composants.ColumnChanged, New DataColumnChangeEventHandler(AddressOf table_composants_changed)
            log("", 0)

            '----- recupération de la liste des composants bannis -----
            log("SQL : Récupération de la liste des composants bannis :", 0)
            log("SQL : Récupération de la liste des composants bannis", -1)
            err = Table_maj_sql(table_composants_bannis, "SELECT * FROM composants_bannis")
            If err <> "" Then
                log("SQL : " & err, 2)
            Else
                If table_composants_bannis.Rows.Count() > 0 Then
                    'affichage de la liste des composants bannis
                    For i = 0 To table_composants_bannis.Rows.Count() - 1
                        log("     -> Id : " & table_composants_bannis.Rows(i).Item("composants_bannis_id") & " -- Norme : " & table_composants_bannis.Rows(i).Item("composants_bannis_norme") & " -- Adresse : " & table_composants_bannis.Rows(i).Item("composants_bannis_adresse") & " -- Description : " & table_composants_bannis.Rows(i).Item("composants_bannis_description"), 0)
                    Next
                Else
                    log("     -> Aucun composant banni trouvé", 0)
                End If
            End If
            log("", 0)

            '----- recupération de la liste des macros -----
            etape_startup = 11
            log("SQL : Récupération de la liste des macros :", 0)
            log("SQL : Récupération de la liste des macros", -1)
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
                        log("     -> Id : " & table_macros.Rows(i).Item("macro_id") & " -- Nom : " & table_macros.Rows(i).Item("macro_nom") & " -- Condition : " & table_macros.Rows(i).Item("macro_conditions") & " -- Action : " & table_macros.Rows(i).Item("macro_actions"), 0)
                    Next
                Else
                    log("     -> Aucune macro trouvée", 0)
                End If
            End If
            log("", 0)

            '----- recupération de la liste des timers -----
            etape_startup = 12
            log("SQL : Récupération de la liste des timers :", 0)
            log("SQL : Récupération de la liste des timers", -1)
            err = Table_maj_sql(table_timer, "SELECT * FROM macro WHERE macro_actif='1' AND macro_conditions LIKE '%CT#%'")
            If err <> "" Then
                log("SQL : " & err, 2)
            Else
                x = New DataColumn
                x.ColumnName = "timer"
                table_timer.Columns.Add(x)
                If table_timer.Rows.Count() > 0 Then
                    'affichage de la liste des timers
                    If table_timer.Rows.Count() > 0 Then
                        For i = 0 To table_timer.Rows.Count() - 1
                            table_timer.Rows(i).Item("timer") = timer_convertendate(table_timer.Rows(i).Item("macro_conditions"))
                            log("     -> Id : " & table_timer.Rows(i).Item("macro_id") & " -- Nom : " & table_timer.Rows(i).Item("macro_nom") & " -- Condition : " & table_timer.Rows(i).Item("macro_conditions") & " -- Action : " & table_timer.Rows(i).Item("macro_actions") & " -- Timer : " & table_timer.Rows(i).Item("timer"), 0)
                        Next
                    Else
                        log("     -> Aucun timer trouvé", 0)
                    End If
                Else
                    log("     -> Aucun timer trouvé", 0)
                End If
            End If
            log("", 0)

            '---------- RFXCOM : Lancement du Handler et paramétrage -------
            etape_startup = 13
            If Serv_RFX Then
                log("RFX : Lancement et paramétrage du RFXCOM", 0)
                log("RFX : Lancement et paramétrage du RFXCOM", -1)
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
                err = rfxcom.ecrire(&HF0, rfxcom.DISHE)
                If STRGS.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err, 0)
                err = rfxcom.ecrire(&HF0, rfxcom.DISKOP)
                If STRGS.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err, 0)
                err = rfxcom.ecrire(&HF0, rfxcom.DISARC)
                If STRGS.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err, 0)
                'err = rfxcom.ecrire(&HF0, rfxcom.SWVERS)
                'If STR.Left(err, 4) = "ERR:" Then log(err, 0) Else log("RFX : " & err)
            End If
            log("", 0)

            '---------- Initialisation du Socket -------
            etape_startup = 14
            If Serv_SOC Then
                err = socket.ouvrir(socket_ip, socket_port)
                If STRGS.Left(err, 4) = "ERR:" Then
                    Serv_SOC = False
                    log("SOCKET : " & err, 2)
                    log("     -> Désactivation du servive SOCKET", 0)
                Else
                    log("SOCKET : " & err, 0)
                End If
                log("", 0)
                log("SOCKET : " & err, -1)
            End If

            '---------- Lancement du pooling -------
            etape_startup = 15
            log("Lancement du Pool", 0)
            log("Lancement du Pool", -1)
            timer_pool = New System.Timers.Timer
            AddHandler timer_pool.Elapsed, AddressOf pool
            timer_pool.Interval = 1000
            timer_pool.Start()
            '---------- Lancement du timer -------
            etape_startup = 16
            log("Lancement du Timer", 0)
            log("Lancement du Timer", -1)
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
            log_niveau = 0
            log("", 0)
            log("--- Arret du Service ---", 0)
            log("--- Arret du Service ---", -1)
            log("", 0)

            '---------- arret du timer -------
            If etape_startup > 16 Then
                log("Arret du Timer :", 0)
                log("Arret du Timer", -1)
                timer_timer.Stop()
                timer_timer.Dispose()
                log("     -> Arrété", 0)
            End If
            '---------- arret du pool -------
            If etape_startup > 15 Then
                log("Arret du Pool :", 0)
                log("Arret du Pool", -1)
                timer_pool.Stop()
                timer_pool.Dispose()
                log("     -> Arrété", 0)
            End If

            '---------- liberation du Socket -------
            If etape_startup > 14 Then
                If Serv_SOC Then
                    log("SOC : Fermeture de la connexion : ", 0)
                    err = socket.fermer()
                    If STRGS.Left(err, 4) = "ERR:" Then
                        log(" -> SOC Fermeture : " & err, 2)
                    Else
                        log(" -> SOC Fermeture : " & err, 0)
                    End If
                    log("SOCKET Fermeture : " & err, -1)
                End If
            End If


            '---------- attente fin des threads : 5 secondes -------
            log("Attente fin des threads :", 0)
            log("Attente fin des threads", -1)
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
                log("     -> OK (Time out)", 0)
            Else
                log("     -> OK", 0)
            End If

            '---------- liberation du 1-wire -------
            If etape_startup > 4 Then
                If Serv_WIR Then
                    log("WIR : Fermeture de la clé USB : ", 0)
                    err = onewire.close()
                    If STRGS.Left(err, 4) = "ERR:" Then
                        log(" -> WIR Fermeture : " & err, 2)
                    Else
                        log(" -> WIR Fermeture : " & err, 0)
                    End If
                    log("WIR Fermeture : " & err, -1)
                End If
            End If

            '---------- liberation du 1-wire 2 -------
            If etape_startup > 5 Then
                If Serv_WI2 Then
                    log("WI2 : Fermeture de la clé USB : ", 0)
                    err = onewire2.close()
                    If STRGS.Left(err, 4) = "ERR:" Then
                        log(" -> WI2 Fermeture : " & err, 2)
                    Else
                        log(" -> WI2 Fermeture : " & err, 0)
                    End If
                    log("WI2 Fermeture : " & err, -1)
                End If
            End If

            '---------- liberation du RFXCOM -------
            If etape_startup > 6 Then
                If Serv_RFX Then
                    log("RFX : Fermeture de la connexion : ", 0)
                    err = rfxcom.fermer()
                    If STRGS.Left(err, 4) = "ERR:" Then
                        log(" -> RFX Fermeture : " & err, 2)
                    Else
                        log(" -> RFX Fermeture : " & err, 0)
                    End If
                    log("RFX Fermeture : " & err, -1)
                End If
            End If

            '---------- liberation du PLCBUS -------
            If etape_startup > 7 Then
                If Serv_PLC Then
                    log("PLC : Fermeture de la connexion : ", 0)
                    err = plcbus.fermer()
                    If STRGS.Left(err, 4) = "ERR:" Then
                        log(" -> PLC Fermeture : " & err, 2)
                    Else
                        log(" -> PLC Fermeture : " & err, 0)
                    End If
                    log("PLC Fermeture : " & err, -1)
                End If
            End If

            '---------- deconnexion mysql -------
            If etape_startup > 2 Then
                log("SQL : Déconnexion du serveur", 0)
                log("", 0)
                err = mysql.mysql_close()
                log("SQL : Déconnexion du serveur " & err, -1)
                If err <> "" Then
                    log("     -> " & err, 2)
                    Exit Sub
                End If
                log("     -> Déconnecté", 0)
            End If

            'le service est arreté
            Serv_DOMOS = False
            etape_startup = 0
            log("", 0)
            log("--- Service arrété ---", 0)
            log("--- Service arrété ---", -1)
            log("", 0)
        Catch ex As Exception
            log("ERR: Close exception : " & ex.ToString, 1)
        End Try
    End Sub

    Private Sub svc_restart()
        svc_stop()
        wait(500)
        svc_start()
    End Sub

    Public Shared Sub log(ByVal texte As String, ByVal niveau As String)
        'log les infos dans un fichier texte et/ou sql
        ' texte : string contenant le texte à logger
        ' niveau : int contenant le type de log
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

        Dim dateheure, fichierlog As String
        Try
            If niveau <> "-1" And STRGS.InStr(log_niveau, niveau) > 0 Then
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
            'Log dans les events logs
            Select Case niveau
                Case "-1" : ecrireevtlog(texte, 3, 100)
                Case "1" : ecrireevtlog(texte, 1, 101)
                Case "3" : ecrireevtlog(texte, 3, 103)
                Case "4" : ecrireevtlog(texte, 3, 104)
            End Select

        Catch ex As Exception
        End Try
    End Sub

    Private Shared Sub ecrireevtlog(ByVal message As String, ByVal type As Integer, ByVal evtid As Integer)
        'message = text to write to event log
        'type = 1:error, 2:warning, 3:information 
        Dim myEventLog = New EventLog()
        myEventLog.Source = "Domos"
        Select Case type
            Case 1 : myEventLog.WriteEntry(message, EventLogEntryType.Error, evtid)
            Case 2 : myEventLog.WriteEntry(message, EventLogEntryType.Warning, evtid)
            Case 3 : myEventLog.WriteEntry(message, EventLogEntryType.Information, evtid)
        End Select
        'Diagnostics.EventLog.WriteEntry("Domos", message)
    End Sub

    Private Shared Sub EcrireFichier(ByVal CheminFichier As String, ByVal Texte As String)
        'ecrit texte dans le fichier CheminFichier
        Dim FreeF As Integer
        Try
            FreeF = FreeFile()
            Texte = Replace(Texte, vbLf, vbCrLf)
            FileOpen(FreeF, CheminFichier, OpenMode.Append)
            Print(FreeF, Texte)
            FileClose(FreeF)
        Catch ex As IOException
            wait(500)
            EcrireFichier(CheminFichier, Texte)
        Catch ex As Exception
            wait(500)
            log("ERR: Ecrire_fichier exception : " & Texte & " ::: " & ex.ToString, 2)
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
            log("ERR: Wait : " & ex.ToString, 2)
        End Try
    End Sub

    Private Function Table_maj_sql(ByRef table As DataTable, ByVal requete As String) As String
        'maj d une table avec les resultats de la requete
        Try
            table.Clear()
            Table_maj_sql = mysql.mysql_query(table, requete)
        Catch ex As Exception
            Table_maj_sql = Nothing
            log("ERR: table_maj exception : " & ex.ToString, 2)
        End Try
    End Function

    Private Function timer_convertendate(ByVal condition As String) As String
        'convertit un timer ([CT#=#ss#mm#hh#jj#MMM#JJJ]) en dateTime
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
    End Function

    Private Sub tables_maj(ByVal table As String)
        'table = ALL / COMPOSANTS / COMPOSANTS_BANNIS / MACRO / TIMER
        Dim table_temp As New DataTable
        Dim result
        Dim x As New DataColumn

        'Maj de la table composant
        If table = "ALL" Or table = "COMPOSANTS" Then
            Try
                log("MAJ: Maj de la table_composants", 0)
                Dim Condition_service As String = ""
                If Not Serv_PLC Then Condition_service &= " AND composants_modele_norme<>'PLC'"
                If Not Serv_WIR Then Condition_service &= " AND composants_modele_norme<>'WIR'"
                If Not Serv_WI2 Then Condition_service &= " AND composants_modele_norme<>'WI2'"
                If Not Serv_X10 Then Condition_service &= " AND composants_modele_norme<>'X10'"
                If Not Serv_RFX Then Condition_service &= " AND composants_modele_norme<>'RFX'"
                If Not Serv_VIR Then Condition_service &= " AND composants_modele_norme<>'VIR'"
                err = Table_maj_sql(table_temp, "SELECT composants.*,composants_modele.* FROM composants,composants_modele WHERE composants_modele=composants_modele_id AND composants_actif='1'" & Condition_service)
                If err <> "" Then
                    log("MAJ: SQL : ERR: " & err, 0)
                Else
                    If table_temp.Rows.Count() > 0 Then 'on a récupéré la nouvelle liste des composants, on fait la maj
                        'Maj et suppression des composants
                        If table_composants.Rows.Count() > 0 Then
                            For i = 0 To table_composants.Rows.Count() - 1
                                result = table_temp.Select("composants_id = " & table_composants.Rows(i).Item("composants_id"))
                                If result.GetLength(0) = 0 Then 'le composant n'existe plus --> DELETE
                                    'log("MAJ: Suppression du composant : " & table_composants.Rows(i).Item("composants_id"))
                                    table_composants.Rows.Remove(table_composants.Rows(i))
                                ElseIf result.GetLength(0) = 1 Then 'le composant existe --> MAJ
                                    'log("MAJ: mise à jour du composant : " & table_composants.Rows(i).Item("composants_id"))
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
                                'log("MAJ: ajout du composant : " & table_temp.Rows(i).Item("composants_id"))
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
                            log("     -> Id : " & table_composants.Rows(i).Item("composants_id") & " -- Nom : " & table_composants.Rows(i).Item("composants_nom") & " -- Adresse : " & table_composants.Rows(i).Item("composants_adresse") & " -- Valeur : " & table_composants.Rows(i).Item("composants_etat") & " -- Polling : " & table_composants.Rows(i).Item("composants_polling") & " -- Type : " & table_composants.Rows(i).Item("composants_modele_norme") & "-" & table_composants.Rows(i).Item("composants_modele_nom"), 0)
                        Next
                    Else
                        log("MAJ: Pas de composant trouvé !", 0)
                    End If
                End If
            Catch ex As Exception
                log("MAJ: composants error : " & ex.Message, 0)
            End Try
        End If

        'Maj de la table composant bannis
        If table = "ALL" Or table = "COMPOSANTS_BANNIS" Then
            Try
                log("MAJ: Maj de la table_composants bannis", 0)
                err = Table_maj_sql(table_temp, "SELECT * FROM composants_bannis")
                If err <> "" Then
                    log("MAJ: SQL : ERR: " & err, 0)
                Else
                    If table_temp.Rows.Count() > 0 Then 'on a récupéré la nouvelle liste des composants bannis, on fait la maj
                        'Maj et suppression des composants bannis
                        If table_composants_bannis.Rows.Count() > 0 Then
                            For i = 0 To table_composants_bannis.Rows.Count() - 1
                                result = table_temp.Select("composants_bannis_id = " & table_composants_bannis.Rows(i).Item("composants_bannis_id"))
                                If result.GetLength(0) = 0 Then 'le composant n'existe plus --> DELETE
                                    'log("MAJ: Suppression du composant banni : " & table_composants_bannis.Rows(i).Item("composants_bannis_id"))
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
                            log("     -> Id : " & table_composants_bannis.Rows(i).Item("composants_bannis_id") & " -- Norme : " & table_composants_bannis.Rows(i).Item("composants_bannis_norme") & " -- Adresse : " & table_composants_bannis.Rows(i).Item("composants_bannis_adresse") & " -- Description : " & table_composants_bannis.Rows(i).Item("composants_bannis_description"), 0)
                        Next
                    Else
                        log("MAJ: Pas de composant banni trouvé !", 0)
                    End If
                End If
            Catch ex As Exception
                log("MAJ: composants_bannis error : " & ex.Message, 0)
            End Try
        End If

        'Maj de la table Macro
        If table = "ALL" Or table = "MACRO" Then
            Try
                log("MAJ: Maj de la table_macros", 0)
                table_temp = New DataTable
                err = Table_maj_sql(table_temp, "SELECT * FROM macro WHERE macro_actif='1' AND macro_conditions NOT LIKE '%CT#%'")
                If err <> "" Then
                    log("MAJ: SQL : ERR: " & err, 0)
                Else
                    If table_temp.Rows.Count() > 0 Then 'on a récupéré la nouvelle liste des macros, on fait la maj
                        'Maj et suppression des macros existantes
                        If table_macros.Rows.Count() > 0 Then
                            For i = 0 To table_macros.Rows.Count() - 1
                                result = table_temp.Select("macro_id = " & table_macros.Rows(i).Item("macro_id"))
                                If result.GetLength(0) = 0 Then 'la macro n'existe plus --> DELETE
                                    'log("MAJ: Suppression de la macro : " & table_macros.Rows(i).Item("macro_id"))
                                    table_macros.Rows.Remove(table_macros.Rows(i))
                                ElseIf result.GetLength(0) = 1 Then 'la macro existe --> MAJ
                                    'log("MAJ: mise à jour de la macro : " & table_macros.Rows(i).Item("macro_id"))
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
                                'log("MAJ: ajout de la macro : " & table_temp.Rows(i).Item("macro_id"))
                                table_temp.Rows(i).Item("verrou") = False
                                'ajout
                                table_macros.ImportRow(table_temp.Rows(i))
                            End If
                        Next
                        'affichage de la nouvelle liste
                        For i = 0 To table_macros.Rows.Count() - 1
                            log("     -> Id : " & table_macros.Rows(i).Item("macro_id") & " -- Nom : " & table_macros.Rows(i).Item("macro_nom") & " -- Condition : " & table_macros.Rows(i).Item("macro_conditions") & " -- Action : " & table_macros.Rows(i).Item("macro_actions"), 0)
                        Next
                    Else
                        log("MAJ: Pas de macro trouvée !", 0)
                    End If
                End If
            Catch ex As Exception
                log("MAJ: macro error : " & ex.Message, 0)
            End Try
        End If

        'Maj de la table Timer
        If table = "ALL" Or table = "TIMER" Then
            Try
                log("MAJ: Maj de la table_timer", 0)
                table_temp = New DataTable
                err = Table_maj_sql(table_temp, "SELECT * FROM macro WHERE macro_actif='1' AND macro_conditions LIKE '%CT#%'")
                If err <> "" Then
                    log("MAJ: SQL : ERR: " & err, 0)
                Else
                    If table_temp.Rows.Count() > 0 Then 'on a récupéré la nouvelle liste des timers, on fait la maj
                        'Maj et suppression des timers existants
                        If table_timer.Rows.Count() > 0 Then
                            For i = 0 To table_timer.Rows.Count() - 1
                                result = table_temp.Select("macro_id = " & table_timer.Rows(i).Item("macro_id"))
                                If result.GetLength(0) = 0 Then 'le timer n'existe plus --> DELETE
                                    'log("MAJ: Suppression du timer : " & table_timer.Rows(i).Item("macro_id"))
                                    table_timer.Rows.Remove(table_timer.Rows(i))
                                ElseIf result.GetLength(0) = 1 Then 'le timer existe --> MAJ
                                    'log("MAJ: mise à jour du timer : " & table_timer.Rows(i).Item("macro_id"))
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
                                'log("MAJ: ajout du timer : " & table_temp.Rows(i).Item("macro_id"))
                                table_temp.Rows(i).Item("timer") = timer_convertendate(table_temp.Rows(i).Item("macro_conditions"))
                                'ajout
                                table_timer.ImportRow(table_temp.Rows(i))
                            End If
                        Next
                        'affichage de la nouvelle liste
                        For i = 0 To table_timer.Rows.Count() - 1
                            table_timer.Rows(i).Item("timer") = timer_convertendate(table_timer.Rows(i).Item("macro_conditions"))
                            log("     -> Id : " & table_timer.Rows(i).Item("macro_id") & " -- Nom : " & table_timer.Rows(i).Item("macro_nom") & " -- Condition : " & table_timer.Rows(i).Item("macro_conditions") & " -- Action : " & table_timer.Rows(i).Item("macro_actions") & " -- Timer : " & table_timer.Rows(i).Item("timer"), 0)
                        Next
                    Else
                        log("MAJ: Pas de Timer trouvé !", 0)
                    End If
                End If
            Catch ex As Exception
                log("MAJ: timer error : " & ex.Message, 0)
            End Try
        End If

    End Sub

    Private Sub tables_aff()
        Dim temp As String

        'Affiche la table composant
        log("AFF: table_composants", 0)
        For i = 0 To table_composants.Rows.Count() - 1
            temp = ""
            For j = 0 To table_composants.Columns.Count() - 1
                temp = temp & table_composants.Columns(j).Caption & ":" & table_composants.Rows(i).Item(j) & " "
            Next
            log("     -> " & temp, 0)
        Next

        'Affiche la table composant bannis
        log("AFF: table_composants_bannis", 0)
        For i = 0 To table_composants_bannis.Rows.Count() - 1
            temp = ""
            For j = 0 To table_composants_bannis.Columns.Count() - 1
                temp = temp & table_composants_bannis.Columns(j).Caption & ":" & table_composants_bannis.Rows(i).Item(j) & " "
            Next
            log("     -> " & temp, 0)
        Next

        'Affiche la table Macro
        log("AFF: table_macros", 0)
        For i = 0 To table_macros.Rows.Count() - 1
            temp = ""
            For j = 0 To table_macros.Columns.Count() - 1
                temp = temp & table_macros.Columns(j).Caption & ":" & table_macros.Rows(i).Item(j) & " "
            Next
            log("     -> " & temp, 0)
        Next

        'Affiche la table Timer
        log("AFF: table_timer", 0)
        For i = 0 To table_timer.Rows.Count() - 1
            temp = ""
            For j = 0 To table_timer.Columns.Count() - 1
                temp = temp & table_timer.Columns(j).Caption & ":" & table_timer.Rows(i).Item(j) & " "
            Next
            log("     -> " & temp, 0)
        Next

        'Affiche la table Thread
        log("AFF: table_thread", 0)
        For i = 0 To table_thread.Rows.Count() - 1
            temp = ""
            For j = 0 To table_thread.Columns.Count() - 1
                temp = temp & table_thread.Columns(j).Caption & ":" & table_thread.Rows(i).Item(j) & " "
            Next
            log("     -> " & temp, 0)
        Next

    End Sub

    Private Sub thread_ajout(ByVal composant_id As String, ByVal norme As String, ByVal source As String, ByRef mythread As Thread)
        Try
            Dim newRow As DataRow
            newRow = table_thread.NewRow()
            newRow.Item("composant_id") = composant_id
            newRow.Item("source") = source
            newRow.Item("norme") = norme
            newRow.Item("datetime") = Date.Now.ToString("yyyy-MM-dd HH:mm:ss")
            newRow.Item("thread") = mythread
            table_thread.Rows.Add(newRow)
        Catch ex As Exception
            log("ERR: Thread_ajout exception : " & ex.ToString, 2)
        End Try
    End Sub

    Private Sub pool(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim y As Thread
        'Dim valeur As String
        Try
            Dim tabletemp = table_composants.Select("composants_polling <> '0'")
            For i = 0 To tabletemp.GetUpperBound(0)
                If tabletemp(i)("timer") <= DateAndTime.Now.ToString("yyyy-MM-dd HH:mm:ss") Then
                    '--- maj du timer du composant ---
                    Dim date_pooling As Date
                    date_pooling = DateAndTime.Now.AddSeconds(tabletemp(i)("composants_polling")) 'on initilisae
                    tabletemp(i)("timer") = date_pooling.ToString("yyyy-MM-dd HH:mm:ss")
                    '--- test pour savoir si un thread est deja lancé sur ce composant ---
                    Dim x = table_thread.Select("composant_id = '" & tabletemp(i)("composants_id") & "' AND source='POL'")
                    If x.GetUpperBound(0) < 0 Then
                        Select Case tabletemp(i)("composants_modele_nom").ToString() 'choix de l'action en fonction du modele
                            Case "DS18B20" 'WIR ou WI2 : capteur de temperature
                                Dim POL_DS18B20 As POL_DS18B20 = New POL_DS18B20(tabletemp(i)("composants_id"))
                                'POL_DS18B20.Execute()
                                y = New Thread(AddressOf POL_DS18B20.Execute)
                                y.Name = "poll_" & tabletemp(i)("composants_id")
                                thread_ajout(tabletemp(i)("composants_id").ToString, tabletemp(i)("composants_modele_norme").ToString, "POL", y)
                                y.Start()
                            Case "DS2406_relais" 'WIR ou WI2 : etat d'un relais
                                log("POL : ERR: DS2406_Relais pas encore géré", 2)
                            Case "DS2406_capteur" 'WIR ou WI2 : capteur d'ouverture d'un switch
                                log("POL : ERR: DS2406_Capteur pas encore géré", 2)
                            Case "DS2423_A" 'WIR ou WI2 : compteur A
                                Dim POL_DS2423 As POL_DS2423 = New POL_DS2423(tabletemp(i)("composants_id"), True)
                                'POL_DS18B20.Execute()
                                y = New Thread(AddressOf POL_DS2423.Execute)
                                y.Name = "poll_" & tabletemp(i)("composants_id")
                                thread_ajout(tabletemp(i)("composants_id").ToString, tabletemp(i)("composants_modele_norme").ToString, "POL", y)
                                y.Start()
                            Case "DS2423_B" 'WIR ou WI2 : compteur B
                                Dim POL_DS2423 As POL_DS2423 = New POL_DS2423(tabletemp(i)("composants_id"), False)
                                'POL_DS18B20.Execute()
                                y = New Thread(AddressOf POL_DS2423.Execute)
                                y.Name = "poll_" & tabletemp(i)("composants_id")
                                thread_ajout(tabletemp(i)("composants_id").ToString, tabletemp(i)("composants_modele_norme").ToString, "POL", y)
                                y.Start()
                            Case "2263-2264" 'PLC : MicroModule lampes
                                'valeur = plcbus.ecrire(tabletemp(i)("composants_adresse").ToString, "STATUS_REQUEST", 0, 0)
                                'If STR.Left(valeur, 4) = "ERR:" Then log("POL : " & valeur)
                                Dim ecrire As ECRIRE = New ECRIRE(tabletemp(i)("composants_id"), "STATUS_REQUEST", "", "")
                                y = New Thread(AddressOf ecrire.action)
                                y.Name = "ecrire_" & tabletemp(i)("composants_id")
                                thread_ajout(tabletemp(i)("composants_id").ToString, tabletemp(i)("composants_modele_norme").ToString, "ECR", y)
                                y.Start()
                            Case "2267-2268" 'PLC : MicroModule Appareils
                                'valeur = plcbus.ecrire(tabletemp(i)("composants_adresse").ToString, "STATUS_REQUEST", 0, 0)
                                'If STR.Left(valeur, 4) = "ERR:" Then log("POL : " & valeur)
                                Dim ecrire As ECRIRE = New ECRIRE(tabletemp(i)("composants_id"), "STATUS_REQUEST", "", "")
                                y = New Thread(AddressOf ecrire.action)
                                y.Name = "ecrire_" & tabletemp(i)("composants_id")
                                thread_ajout(tabletemp(i)("composants_id").ToString, tabletemp(i)("composants_modele_norme").ToString, "ECR", y)
                                y.Start()
                            Case Else
                                log("POL : ERR: Pas de fonction associé à " & tabletemp(i)("composants_modele_nom").ToString() & ":" & tabletemp(i)("composants_nom").ToString(), 2)
                        End Select
                    Else
                        log("POL : ERR: Un thread est déjà associé à " & tabletemp(i)("composants_nom").ToString(), 2)
                    End If
                End If
            Next
        Catch ex As Exception
            log("POL : ERR: exception : " & ex.ToString, 2)
        End Try
    End Sub

    Private Sub timer()
        Try
            '--- HEURES DU SOLEIL --> Composant virtuel JOUR ---
            If STRGS.Right(var_soleil_lever, 8) = DateAndTime.Now.ToString("HH:mm:ss") Then 'si lever du soleil
                Dim tabletemp = table_composants.Select("composants_adresse = 'jour'")
                If tabletemp.GetLength(0) = 1 Then
                    tabletemp(0)("composants_etat") = "1" 'maj du composant virtuel JOUR
                    log("TIMER : lever du soleil -> Maj composant virtuel JOUR=1", 4)
                Else
                    log("TIMER : ERR: Maj composant virtuel JOUR=1 : Non trouvé", 2)
                    tables_aff() 'affichage des tables car pas normal qu'on est pas le composant JOUR
                End If
            ElseIf STRGS.Right(var_soleil_coucher, 8) = DateAndTime.Now.ToString("HH:mm:ss") Then 'si coucher du soleil
                Dim tabletemp = table_composants.Select("composants_adresse = 'jour'")
                If tabletemp.GetLength(0) = 1 Then
                    tabletemp(0)("composants_etat") = "0" 'maj du composant virtuel JOUR
                    log("TIMER : coucher du soleil -> Maj composant virtuel JOUR=0", 4)
                Else
                    log("TIMER : ERR: Maj composant virtuel JOUR=0 : Non trouvé", 2)
                    tables_aff() 'affichage des tables car pas normal qu'on est pas le composant JOUR
                End If
            End If
            '--- HEURES DU SOLEIL CORRIGEES --> Composant virtuel JOUR2 ---
            If STRGS.Right(var_soleil_lever2, 8) = DateAndTime.Now.ToString("HH:mm:ss") Then 'si lever du soleil corrige
                Dim tabletemp = table_composants.Select("composants_adresse = 'jour2'")
                If tabletemp.GetLength(0) = 1 Then
                    tabletemp(0)("composants_etat") = "1" 'maj du composant virtuel JOUR2
                    log("TIMER : lever du soleil corrigé -> Maj composant virtuel JOUR2=1", 4)
                Else
                    log("TIMER : ERR: Maj composant virtuel JOUR2=1 : Non trouvé", 2)
                    tables_aff() 'affichage des tables car pas normal qu'on est pas le composant JOUR2
                End If
            ElseIf STRGS.Right(var_soleil_coucher2, 8) = DateAndTime.Now.ToString("HH:mm:ss") Then 'si coucher du soleil corrige
                Dim tabletemp = table_composants.Select("composants_adresse = 'jour2'")
                If tabletemp.GetLength(0) = 1 Then
                    tabletemp(0)("composants_etat") = "0" 'maj du composant virtuel JOUR2
                    log("TIMER : coucher du soleil corrigé -> Maj composant virtuel JOUR2=0", 4)
                Else
                    log("TIMER : ERR: Maj composant virtuel JOUR2=0 : Non trouvé", 2)
                    tables_aff() 'affichage des tables car pas normal qu'on est pas le composant JOUR2
                End If
            End If

            '--- Actions à faire chaque jour à minuit ---
            If DateAndTime.Now.ToString("HH:mm:ss") = "00:01:00" Then
                'Calcul de l'heure de lever et coucher du soleil
                Me.soleil.CalculateSun()
                var_soleil_lever = Me.soleil.Sunrise
                var_soleil_coucher = Me.soleil.Sunset
                var_soleil_coucher2 = DateAdd(DateInterval.Minute, heure_coucher_correction, var_soleil_coucher)
                var_soleil_lever2 = DateAdd(DateInterval.Minute, heure_lever_correction, var_soleil_lever)
                log("TIMER : maj Heure du soleil : ", 0)
                log("     -> Heure du lever : " & var_soleil_lever & " (" & var_soleil_lever2 & ")", 0)
                log("     -> Heure du coucher : " & var_soleil_coucher & " (" & var_soleil_coucher2 & ")", 0)
            End If

            '--- Traitement des timers
            For i = 0 To table_timer.Rows.Count() - 1
                If table_timer(i)("timer") <= DateAndTime.Now.ToString("yyyy-MM-dd HH:mm:ss") Then
                    'reprogrammation du prochain shedule
                    table_timer(i)("timer") = timer_convertendate(table_timer.Rows(i).Item("macro_conditions"))
                    'lancement des actions
                    log("TIMER: " & table_timer.Rows(i).Item("macro_nom") & " : OK", 4)
                    action(table_timer.Rows(i).Item("macro_actions"))
                End If
            Next

        Catch ex As Exception
            log("TIMER : ERR: exception : " & ex.ToString, 2)
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
                If err <> "" Then log("SQL: table_comp_changed " & err, 2)
                '--- modification de l'etat du composant dans la base ---
                err = mysql.mysql_nonquery("UPDATE composants SET composants_etat='" & etat_temp & "',composants_etatdate='" & dateheure & "' WHERE composants_id='" & args.Row.Item("composants_id") & "'")
                If err <> "" Then log("SQL: table_comp_changed " & err, 2)
                '--- gestion des macros ---
                macro(args.Row.Item("composants_id"), args.Row.Item("composants_etat"))
            End If
        Catch ex As Exception
            log("TCC: table_comp_changed exception : " & ex.ToString, 2)
        End Try
    End Sub

    Private Sub macro(ByVal comp_id, ByVal comp_etat)
        Try
            Dim tabletemp = table_composants.Select("composants_id = '" & comp_id & "'")
            If tabletemp.GetLength(0) = 1 Then 'composant trouvé
                For i = 0 To table_macros.Rows.Count() - 1
                    'test si le composant fait partie d'une macro
                    If STRGS.InStr(table_macros.Rows(i).Item("macro_conditions"), "CC#" & comp_id & "#") > 0 Then
                        If analyse_cond(table_macros.Rows(i).Item("macro_conditions")) Then 'verification des conditions
                            If table_macros.Rows(i).Item("verrou") = False Then 'si la macro n'est pas déjà en cours d'execution
                                table_macros.Rows(i).Item("verrou") = True
                                log("MAC: " & table_macros.Rows(i).Item("macro_nom") & " : OK", 4)
                                action(table_macros.Rows(i).Item("macro_actions"))
                                table_macros.Rows(i).Item("verrou") = False
                            End If
                        End If
                    End If
                Next
            End If
        Catch ex As Exception
            log("MACRO: ERR: exception : " & ex.ToString, 2)
        End Try
    End Sub

    Function analyse_cond(ByVal liste As String) As Boolean
        'fonction recursive d'analyse des conditions d'une macro
        Dim resultat As Boolean = True
        Dim resultat_temp As Boolean = True
        Dim posfin As Integer = 1
        Dim posdebut As Integer = 2
        Dim operateur As String = "&&"
        Dim listesup As Boolean = False
        Try

            'log("liste de base " & liste, 9)

            liste = STRGS.Mid(liste, 2, STRGS.Len(liste) - 2) 'on supprimer les () de chaque cote de la liste
            While (posfin < STRGS.Len(liste)) 'tant que toute la liste n'a pas ete traite
                If liste(posfin - 1) = "(" Then 'c'est une liste
                    'log(" x1 -> " & posdebut & "-" & posfin, 9)
                    posfin = STRGS.InStr(posdebut, liste, ")")
                    'log(" x2 -> " & posdebut & "-" & posfin, 9)
                    For i = posdebut To (posfin - 1)
                        If liste(i) = "(" Then posfin = STRGS.InStr(posfin, liste, ")")
                    Next

                    'log("sous liste " & STRGS.Mid(liste, posdebut - 1, posfin - posdebut + 2), 9)

                    resultat_temp = analyse_cond(STRGS.Mid(liste, posdebut - 1, posfin - posdebut + 2)) 'on relance une analyse sur cette sous liste
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
        Catch ex As Exception
            resultat = False
            log("MACRO: ERR: analyse_cond:exception : " & ex.ToString & " --> " & liste, 2)
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
            log("MACRO: ERR: test-cond:exception : " & ex.ToString & " --> " & text, 2)
        End Try
        Return resultat
    End Function

    Sub action(ByVal liste As String)
        'execute la liste d'actions passé en paramétre
        Dim posdebut As Integer = 2
        Dim posfin As Integer = 2
        Dim contenu
        Dim y As Thread
        Try
            liste = STRGS.Mid(liste, 2, STRGS.Len(liste) - 2) 'on supprimer les () de chaque cote de la liste
            While (posfin < STRGS.Len(liste)) 'tant que toute la liste n'a pas ete traite
                posfin = STRGS.InStr(posdebut, liste, "]")
                contenu = STRGS.Split(STRGS.Mid(liste, posdebut, posfin - posdebut), "#")

                '--------------------------- AC = Action sur un composant -------------------------
                If contenu(0) = "AC" Then 'c'est un composant :  : [AC#compid#Valeur] ou [AC#compid#Valeur#Valeur2]
                    'recherche du composant
                    Dim tabletemp = table_composants.Select("composants_id = '" & contenu(1) & "'")
                    If tabletemp.GetLength(0) = 1 Then
                        'verification si on a pas déjà un thread sur ce comp sinon on boucle pour attendre Action_timeout/10 = 5 sec par défaut
                        Dim tblthread = table_thread.Select("composant_id = '" & contenu(1) & "'")
                        Dim limite As Integer = 0
                        While (tblthread.GetLength(0) > 0 And limite < (Action_timeout / 10))
                            wait(10)
                            tblthread = table_thread.Select("composant_id = '" & contenu(1) & "'")
                            limite = limite + 1
                        End While
                        'a la fin du timeout on verifie si Libre
                        tblthread = table_thread.Select("composant_id = '" & contenu(1) & "'")
                        If tblthread.GetUpperBound(0) < 0 Then
                            Dim x As ECRIRE
                            If UBound(contenu) = 5 Then
                                x = New ECRIRE(tabletemp(0)("composants_id"), contenu(2), contenu(3), contenu(4))
                            ElseIf UBound(contenu) = 4 Then
                                x = New ECRIRE(tabletemp(0)("composants_id"), contenu(2), contenu(3), "")
                            Else
                                x = New ECRIRE(tabletemp(0)("composants_id"), contenu(2), "", "")
                            End If
                            y = New Thread(AddressOf x.action)
                            y.Name = "ecrire_" & tabletemp(0)("composants_id")
                            thread_ajout(tabletemp(0)("composants_id").ToString, tabletemp(0)("composants_modele_norme").ToString, "ECR", y)
                            y.Start()
                            'on modifie l'etat du composant en memoire
                            Dim tabletempp = table_composants.Select("composants_id = '" & contenu(1) & "'")
                            If tabletempp.GetLength(0) = 1 Then
                                tabletempp(0)("composants_etat") = contenu(2)
                            End If

                        Else
                            log("MAC:  -> Un thread est déjà associé à " & tabletemp(0)("composants_nom").ToString(), 2)
                        End If
                    Else
                        log("MAC:  -> Action: Composant ID:" & contenu(1) & " non trouvé", 2)
                    End If
                    wait(100)

                    '--------------------------- ME = Etat d'un composant -------------------------
                ElseIf contenu(0) = "ME" Then 'c'est l'etat d'un composant à modifier :  : [ME#compid#Etat]
                    'recherche du composant
                    Dim tabletemp = table_composants.Select("composants_id = '" & contenu(1) & "'")
                    If tabletemp.GetLength(0) = 1 Then
                        tabletemp(0)("composants_etat") = contenu(2) 'maj du composant en memoire
                        log("MAC:  -> Modif Etat: Composant :" & tabletemp(0)("composants_nom") & " Etat=" & contenu(2), 6)
                    Else
                        log("MAC:  -> Modif Etat: Composant ID:" & contenu(1) & " non trouvé", 2)
                    End If

                    '--------------------------- MA = Adresse d'un composant -------------------------
                ElseIf contenu(0) = "MA" Then 'c'est l'adresse d'un composant à modifier :  : [MA#compid#Adresse]
                    Dim tabletemp = table_composants.Select("composants_id = '" & contenu(1) & "'")
                    If tabletemp.GetLength(0) = 1 Then
                        tabletemp(0)("composants_adresse") = contenu(2) 'maj du composant en memoire
                        log("MAC:  -> Modif Adresse :" & tabletemp(0)("composants_nom") & " Adresse=" & contenu(2), 6)
                    Else
                        log("MAC:  -> Modif Adresse: Composant ID:" & contenu(1) & " non trouvé", 2)
                    End If

                    '--------------------------- MM = condition/action macro -------------------------
                ElseIf contenu(0) = "MM" Then 'c'est la condition/action d'une macro à modifier :  : [MM#macroid#Condition#action]
                    Dim tabletemp = table_macros.Select("macro_id = '" & contenu(1) & "'")
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
                        log("MAC:  -> Modif Macro :" & tabletemp(0)("macro_nom") & " Cond=" & macro_cond & " Action=" & macro_act, 6)
                    Else
                        tabletemp = table_timer.Select("macro_id = '" & contenu(1) & "'")
                        If tabletemp.GetLength(0) = 1 Then
                            'maj des champs en memoire
                            tabletemp(0)("macro_conditions") = macro_cond 'maj de la condition
                            tabletemp(0)("macro_actions") = macro_act 'maj de l'action
                            log("MAC:  -> Modif Timer :" & tabletemp(0)("macro_nom") & " Cond=" & macro_cond & " Action=" & macro_act, 6)
                        Else
                            log("MAC:  -> Modif Macro/Timer :" & tabletemp(0)("macro_nom") & "non trouvé : liste=" & liste, 2)
                        End If
                    End If

                    '---------------------------------- AL = LOG ----------------------------------
                ElseIf contenu(0) = "AL" Then 'on doit juste loguer : [AL#texte]
                    log("MAC:  -> " & contenu(1), 5)

                    '-------------------------- AS = action sur le SERVICE ------------------------
                ElseIf contenu(0) = "AS" Then 'on execute une action sur le service  : [AS#action(#divers)]
                    log("MAC:  -> " & contenu(1), 5)
                    If contenu(1) = "stop" Then 'arret du service
                        If Serv_DOMOS Then svc_stop()
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
                            log("MAC:  -> Optimisation de la base MySQL", 5)
                            err = mysql.mysql_nonquery("OPTIMIZE TABLE logs, releve, composants, macro, plan, users")
                            If err <> "" Then log("MAC: " & err, 2)
                        ElseIf contenu(2) = "purgelogs" Then 'purge de logs > 2 mois
                            log("MAC:  -> Purge des logs de plus 2 mois", 5)

                            err = mysql.mysql_nonquery("DELETE FROM logs WHERE logs_date<'" & DateAdd(DateInterval.Month, -2, DateAndTime.Now).ToString("yyyy/MM/dd") & "'")
                            If err <> "" Then log("MAC: " & err, 2)
                        ElseIf contenu(2) = "reconnect" Then 'deco/reco a la base SQL
                            log("MAC:  -> Déco/Reco à la base SQL", 5)
                            log("SQL:  Déco", 5)
                            err = mysql.mysql_close()
                            If err <> "" Then log("MAC: " & err, 2)
                            err = mysql.mysql_connect(mysql_ip, mysql_db, mysql_login, mysql_mdp)
                            log("SQL:  Reco", 5)
                            If err <> "" Then log("MAC: " & err, 2)
                        End If
                    End If

                    '---------------------------- AM = Lancer une macro ------------------------
                ElseIf contenu(0) = "AM" Then 'execution d'une macro : [AM#macros_id]
                    Dim tabletemp = table_macros.Select("macro_id = '" & contenu(1) & "'")
                    If tabletemp.GetLength(0) = 1 Then 'macro trouvé
                        log("MAC:  -> Analyse Macro : " & tabletemp(0)("macro_nom"), 5)
                        If analyse_cond(tabletemp(0)("macro_conditions")) Then 'verification des conditions
                            If tabletemp(0)("verrou") = False Then 'si la macro n'est pas déjà en cours d'execution
                                tabletemp(0)("verrou") = True
                                log("MAC: " & tabletemp(0)("macro_nom") & " : OK", 4)
                                action(tabletemp(0)("macro_actions"))
                                tabletemp(0)("verrou") = False
                            End If
                        End If
                    End If
                End If

                'on a pas fini de traiter la liste d'actions, on avance à l'element suivant
                If (posfin) < STRGS.Len(liste) Then
                    posdebut = posfin + 2
                    posfin = posfin + 2
                End If
            End While
        Catch ex As Exception
            log("MACRO: ERR: Action:exception : " & ex.ToString & " --> " & liste, 2)
        End Try
    End Sub

    Private Class ECRIRE
        Private compid As Integer
        Private valeur As String
        Private valeur2 As String
        Private valeur3 As String
        Public Sub New(ByVal id As Integer, ByVal val As String, ByVal val2 As String, ByVal val3 As String)
            compid = id
            valeur = val
            valeur2 = val2
            valeur3 = val3
        End Sub
        Public Sub action()
            Dim tabletmp() As DataRow
            Dim limite As Integer = 0
            Dim err As String = ""
            Try
                tabletmp = table_composants.Select("composants_id = '" & compid & "'")
                If tabletmp.GetLength(0) > 0 Then

                    Select Case tabletmp(0)("composants_modele_norme").ToString
                        Case "PLC"
                            'verification si on a pas déjà un thread qui ecrit sur le plcbus sinon on boucle pour attendre PLC_timeout/10 = 5 sec par défaut
                            Dim tblthread = table_thread.Select("norme='PLC' AND source='ECR_PLC' AND composant_id<>'" & compid & "'")
                            While (tblthread.GetLength(0) > 0 And limite < (PLC_timeout / 10))
                                wait(10)
                                tblthread = table_thread.Select("norme='PLC' AND source='ECR_PLC' AND composant_id<>'" & compid & "'")
                                limite = limite + 1
                            End While
                            If (limite < (PLC_timeout / 10)) Then 'on a attendu moins que le timeout
                                'maj du thread pour dire qu'on ecrit sur le bus
                                tblthread = table_thread.Select("norme='PLC' AND source='ECR' AND composant_id='" & compid & "'")
                                If tblthread.GetLength(0) = 1 Then
                                    tblthread(0)("source") = "ECR_PLC"
                                Else
                                    log("ECR: Thread non trouvé (pour PLC)!", 2)
                                End If
                                If valeur2 <> "" Then
                                    err = plcbus.ecrire(tabletmp(0)("composants_adresse"), valeur, valeur2, 0)
                                Else
                                    err = plcbus.ecrire(tabletmp(0)("composants_adresse"), valeur, 0, 0)
                                End If
                                If STRGS.Left(err, 4) = "ERR:" Then
                                    log(err, 2)
                                Else
                                    log(err, 5)
                                End If
                                wait(100) 'pause de 1sec pour recevoir le ack et libérer le bus correctement
                            Else
                                log("ECR: Le port PLCBUS nest pas disponible pour une ecriture : " & tabletmp(0)("composants_adresse") & "-" & valeur, 2)
                            End If
                        Case "WIR"
                            '???
                        Case "WI2"
                            '???
                        Case "X10"
                            '???
                        Case Else
                            'log("ECR: norme non gérée : " & tabletmp(0)("composants_modele_norme").ToString & " comp: " & tabletmp(0)("composants_adresse").ToString)
                    End Select
                Else
                    log("ECR: Composant non trouvé dans la table : " & compid, 2)
                End If

            Catch ex As Exception
                log("Ecrire.action exception : " & ex.ToString, 2)
            End Try
            Try
                '--- suppresion du thread de la liste des thread lancé ---
                tabletmp = table_thread.Select("composant_id = '" & compid & "'")
                If tabletmp.GetLength(0) >= 1 Then
                    tabletmp(0).Delete()
                Else
                    log("ECR: Thread non trouvé (pour delete) !", 2)
                End If
            Catch ex As Exception
                log("Ecrire.action Suppression thread liste : exception : " & ex.ToString, 2)
            End Try

        End Sub
    End Class

    Private Class POL_DS18B20
        Private _id As Integer
        Public Sub New(ByVal composant_id As Integer)
            _id = composant_id
        End Sub
        Public Sub Execute()
            'Forcer le . 
            Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
            My.Application.ChangeCulture("en-US")

            Dim valeur As String
            Dim valeur2 As Double
            Dim wir1_2 As Boolean = False '= true si sur le deuxieme bus 1-wire
            Dim tabletmp() As DataRow
            Dim dateheure, Err As String
            Try
                tabletmp = table_composants.Select("composants_id = '" & _id & "'")
                '--- test pour savoir si on est sur la premiere ou deuxieme clé WIR/WI2 ---
                If tabletmp(0)("composants_modele_norme").ToString() = "WIR" Then
                    valeur = onewire.temp_get(tabletmp(0)("composants_adresse").ToString(), WIR_res)
                Else
                    valeur = onewire2.temp_get(tabletmp(0)("composants_adresse").ToString(), WIR_res)
                End If
                If STRGS.Left(valeur, 4) <> "ERR:" Then 'si y a pas erreur d'acquisition, action
                    If (tabletmp(0)("composants_correction") <> "") Then
                        valeur2 = Math.Round(CDbl(valeur) + CDbl(tabletmp(0)("composants_correction")), 1) 'correction de la temperature
                    Else
                        valeur2 = Math.Round(CDbl(valeur), 1)
                    End If
                    'correction de l'etat si pas encore initialisé à une valeur
                    If tabletmp(0)("composants_etat").ToString() = "" Then tabletmp(0)("composants_etat") = 0
                    'comparaison du relevé avec le dernier etat
                    If valeur2.ToString <> tabletmp(0)("composants_etat").ToString() Then
                        If domos_svc.lastetat And valeur2.ToString = tabletmp(0)("lastetat").ToString() Then
                            domos_svc.log("POL : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur2 & "°C (inchangé lastetat)", 8)
                            '--- Modification de la date dans la base SQL ---
                            dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                            Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                            If Err <> "" Then log("SQL: table_comp_changed " & Err, 2)
                        Else
                            If IsNumeric(valeur2) Then
                                'on vérifie que la valeur a changé de plus de composants_precision sinon inchangé
                                'If (valeur2 + CDbl(tabletmp(0)("composants_precision"))).ToString >= tabletmp(0)("composants_etat").ToString() And (valeur2 - CDbl(tabletmp(0)("composants_precision"))).ToString <= tabletmp(0)("composants_etat").ToString() Then
                                If (CDbl(valeur2) + CDbl(tabletmp(0)("composants_precision"))) >= CDbl(tabletmp(0)("composants_etat")) And (CDbl(valeur2) - CDbl(tabletmp(0)("composants_precision"))) <= CDbl(tabletmp(0)("composants_etat")) Then
                                    domos_svc.log("POL : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur2 & "°C (inchangé precision)", 8)
                                    '--- Modification de la date dans la base SQL ---
                                    dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                    Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                    If Err <> "" Then log("SQL: table_comp_changed " & Err, 2)
                                Else
                                    domos_svc.log("POL : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur2 & "°C ", 6)
                                    '--- modification de l'etat du composant dans la table en memoire ---
                                    tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                                    tabletmp(0)("composants_etat") = valeur2
                                    tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                End If
                            Else
                                domos_svc.log("POL : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur2 & "°C ", 6)
                                '--- modification de l'etat du composant dans la table en memoire ---
                                tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                                tabletmp(0)("composants_etat") = valeur2
                                tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                            End If
                        End If
                    Else
                        domos_svc.log("POL : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur2 & "°C (inchangé)", 7)
                        '--- Modification de la date dans la base SQL ---
                        dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                        Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                        If Err <> "" Then log("SQL: table_comp_changed " & Err, 2)
                    End If
                Else
                    'erreur
                    domos_svc.log("POL : DS18B20 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur, 2)
                End If
                '--- suppresion du thread de la liste des thread lancé ---
                tabletmp = table_thread.Select("composant_id = '" & _id & "'")
                If tabletmp.GetUpperBound(0) >= 0 Then
                    tabletmp(0).Delete()
                End If
            Catch ex As Exception
                '--- suppresion du thread de la liste des thread lancé ---
                tabletmp = table_thread.Select("composant_id = '" & _id & "'")
                If tabletmp.GetUpperBound(0) >= 0 Then
                    tabletmp(0).Delete()
                End If
                domos_svc.log("POL : DS18B20 " & ex.ToString, 2)
            End Try
        End Sub
    End Class

    Private Class POL_DS2406_capteur
        Private _id As Integer
        Public Sub New(ByVal composant_id As Integer)
            _id = composant_id
        End Sub
        Public Sub Execute()
            'Forcer le . 
            Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
            My.Application.ChangeCulture("en-US")

            Dim valeur, valeur_etat, valeur_activite As String
            Dim tabletmp() As DataRow
            Try
                tabletmp = table_composants.Select("composants_id = '" & _id & "'")
                valeur = onewire.switch_get(tabletmp(0)("composants_adresse").ToString())
                If STRGS.Left(valeur, 4) <> "ERR:" Then 'si y a pas erreur d'acquisition, action
                    '--- comparaison du relevé avec le dernier etat ---
                    valeur_etat = STRGS.Left(valeur, 1)
                    valeur_activite = STRGS.Right(valeur, 1)
                    If valeur_activite <> tabletmp(0)("composants_etat").ToString() Then
                        domos_svc.log("POL DS2406_capteur : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur_activite & " ", 6)
                        '--- modification de l'etat du composant dans la table en memoire ---
                        tabletmp(0)("composants_etat") = valeur_activite
                    Else
                        domos_svc.log("POL DS2406_capteur : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur_activite, 7)
                    End If
                Else
                    'erreur
                    domos_svc.log("POL DS2406_capteur : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur, 2)
                End If
            Catch ex As Exception
                domos_svc.log("POL : DS2406_capteur " & ex.ToString, 2)
            End Try

        End Sub
    End Class

    Private Class POL_DS2423
        Private _id As Integer
        Private _AorB As Boolean
        Public Sub New(ByVal composant_id As Integer, ByVal AorB As Boolean)
            _id = composant_id
            _AorB = AorB
        End Sub
        Public Sub Execute()
            'Forcer le . 
            Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
            My.Application.ChangeCulture("en-US")

            Dim valeur As String
            Dim tabletmp() As DataRow
            Try
                tabletmp = table_composants.Select("composants_id = '" & _id & "'")
                valeur = onewire.counter(tabletmp(0)("composants_adresse").ToString(), _AorB)
                If STRGS.Left(valeur, 4) <> "ERR:" Then 'si y a pas erreur d'acquisition, action
                    '--- comparaison du relevé avec le dernier etat ---
                    If valeur <> tabletmp(0)("composants_etat").ToString() Then
                        domos_svc.log("POL DS2423 : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur & " ", 6)
                        '--- modification de l'etat du composant dans la table en memoire ---
                        tabletmp(0)("composants_etat") = valeur
                    Else
                        domos_svc.log("POL DS2423 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur, 7)
                    End If
                Else
                    'erreur
                    domos_svc.log("POL DS2423 : " & tabletmp(0)("composants_nom").ToString() & ":" & tabletmp(0)("composants_adresse").ToString() & " : " & valeur, 2)
                End If
                '--- suppresion du thread de la liste des thread lancé ---
                tabletmp = table_thread.Select("composant_id = '" & _id & "'")
                If tabletmp.GetUpperBound(0) >= 0 Then
                    tabletmp(0).Delete()
                End If
            Catch ex As Exception
                '--- suppresion du thread de la liste des thread lancé ---
                tabletmp = table_thread.Select("composant_id = '" & _id & "'")
                If tabletmp.GetUpperBound(0) >= 0 Then
                    tabletmp(0).Delete()
                End If
                domos_svc.log("POL : DS2423 " & ex.ToString, 2)
            End Try

        End Sub
    End Class

End Class
