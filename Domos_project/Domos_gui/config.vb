Imports Microsoft.Win32

Public Class config
    Dim mysql_ip, mysql_db, mysql_login, mysql_mdp, install_dir As String

    Private Sub config_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            Dim regKey, regKey2 As RegistryKey
            regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE")
            If regKey Is Nothing Then
                MsgBox("Error Accessing HKLM\Software registry !", MsgBoxStyle.Critical, "ERROR")
                Application.Exit()
            Else
                regKey2 = regKey.OpenSubKey("Domos")
                If regKey2 Is Nothing Then
                    MsgBox("Error Accessing Domos registry configuration ! Reinstall It.", MsgBoxStyle.Critical, "ERROR")
                    Application.Exit()
                Else
                    mysql_ip = regKey2.GetValue("mysql_ip", "127.0.0.1")
                    mysql_db = regKey2.GetValue("mysql_db", "domos")
                    mysql_login = regKey2.GetValue("mysql_login", "domos")
                    mysql_mdp = regKey2.GetValue("mysql_mdp", "domos")
                    install_dir = regKey2.GetValue("install_dir", "C:\Domos\")
                    regKey2.Close()
                End If
                regKey.Close()
            End If
        Catch ex As Exception
            MsgBox("Error while loading Domos Coonfiguration GUI !", MsgBoxStyle.Critical, "ERROR")
            Application.Exit()
        End Try

        txt_mysql_ip.Text = mysql_ip
        txt_mysql_db.Text = mysql_db
        txt_mysql_login.Text = mysql_login
        txt_mysql_mdp.Text = mysql_mdp
        txt_folder_install.Text = install_dir
    End Sub

    Private Sub BTN_Save_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BTN_Save.Click
        Try
            Dim regKey, regKey2 As RegistryKey
            regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE")
            If regKey Is Nothing Then
                MsgBox("Error Accessing HKLM\Software registry !", MsgBoxStyle.Critical, "ERROR")
                Application.Exit()
            Else
                regKey2 = regKey.OpenSubKey("Domos", True)
                If regKey2 Is Nothing Then
                    MsgBox("Error Accessing Domos registry configuration ! Reinstall It.", MsgBoxStyle.Critical, "ERROR")
                    Application.Exit()
                Else
                    regKey2.SetValue("mysql_ip", txt_mysql_ip.Text, RegistryValueKind.String)
                    regKey2.SetValue("mysql_db", txt_mysql_db.Text, RegistryValueKind.String)
                    regKey2.SetValue("mysql_login", txt_mysql_login.Text, RegistryValueKind.String)
                    regKey2.SetValue("mysql_mdp", txt_mysql_mdp.Text, RegistryValueKind.String)
                    regKey2.Flush()
                    MsgBox("Configuration saved." & Chr(10) & Chr(10) & "Restart the service to take effect.", MsgBoxStyle.Information, "Domos Configuration")
                    regKey2.Close()
                End If
                regKey.Close()
            End If
        Catch ex As Exception
            MsgBox("Error while saving Domos Coonfiguration ! Run GUI from administrator account." & Chr(10) & Chr(10) & ex.ToString, MsgBoxStyle.Critical, "ERROR")
            Application.Exit()
        End Try
        Me.Close()
    End Sub

    Private Sub BTN_cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BTN_cancel.Click
        Me.Close()
    End Sub
End Class