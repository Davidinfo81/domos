Imports System.ComponentModel
Imports System.Configuration.Install
Imports Microsoft.Win32

Public Class ProjectInstaller

    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

        'Add initialization code after the call to InitializeComponent

    End Sub

    Private Sub ServiceProcessInstaller1_AfterInstall(ByVal sender As System.Object, ByVal e As System.Configuration.Install.InstallEventArgs) Handles ServiceProcessInstaller1.AfterInstall

    End Sub

    Public Overrides Sub Install(ByVal stateSaver As System.Collections.IDictionary)
        MyBase.Install(stateSaver)
        Dim ip As String = Me.Context.Parameters.Item("ip")
        Dim login As String = Me.Context.Parameters.Item("login")
        Dim db As String = Me.Context.Parameters.Item("db")
        Dim password As String = Me.Context.Parameters.Item("password")
        Dim targ As String = Me.Context.Parameters.Item("targ")
        MsgBox("ouverture clé domos")
        Dim regKey, regKey2 As RegistryKey
        regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE", True)
        regKey2 = regKey.OpenSubKey("Domos", True)
        MsgBox("clé domos ouverte test")
        If regKey Is Nothing Then
            MsgBox("cre domos")
            regKey.CreateSubKey("Domos")
            regKey.Close()
            regKey2 = regKey.OpenSubKey("Domos", True)
            MsgBox("clé domos not exist : creation")
        End If
        MsgBox("ecriture valeur")
        regKey2.SetValue("mysql_ip", ip)
        regKey2.SetValue("mysql_db", db)
        regKey2.SetValue("mysql_login", login)
        regKey2.SetValue("mysql_mdp", password)
        regKey2.SetValue("install_dir", targ)
        regKey2.Close()
        regKey.Close()
        MsgBox("FIN")
    End Sub


    Private Sub ServiceInstaller1_AfterInstall(ByVal sender As System.Object, ByVal e As System.Configuration.Install.InstallEventArgs) Handles ServiceInstaller1.AfterInstall

    End Sub
End Class
