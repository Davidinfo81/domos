Imports System
Imports System.Collections.Generic
Imports System.Text
Imports MySql.Data.MySqlClient

Public Class mysql
    ' Fournie une liste de fonction pour communiquer avec la base de donnée mysql
    ' Necessite l'installation du connector mysql pour .net et l'import de Mysql.data
    ' Auteur : David
    ' Date : 20/01/2009
    Public connected As Boolean = False
    Public error_number As Integer = 0

    Private var_connexion As New MySqlConnection

    Public Function mysql_connect(ByVal serveur As String, ByVal base As String, ByVal user As String, ByVal pass As String) As String
        If Not connected Then
            Try
                var_connexion = New MySqlConnection("Data Source=" & serveur & ";Database=" & base & ";User Id=" & user & ";Password=" & pass)
                var_connexion.Open()
                connected = True
                Return ""
            Catch Ex As Exception
                connected = False
                Return "ERR: SQL_Connect : " & Ex.Message
            End Try
        Else
            Return "ERR: SQL_Connect : Déjà Connecté au serveur"
        End If

    End Function

    Public Function mysql_close() As String
        If connected Then
            Try
                var_connexion.Close() 'ferme la connexion
                var_connexion.Dispose() 'libere la memoire
                var_connexion = Nothing
                connected = False
                Return ""
            Catch Ex As Exception
                Return "ERR: SQL_Close : " & Ex.Message
            End Try
        Else
            Return ""
        End If
    End Function

    Public Function mysql_query(ByRef table As DataTable, ByVal query As String) As String
        'Select
        Dim commande As MySqlCommand
        Dim dataadapter As New MySqlDataAdapter
        Dim data As New DataTable

        If connected Then
            Try
                commande = New MySqlCommand(query, var_connexion) 'creation de la commande pour la requete
                dataadapter.SelectCommand = commande
                dataadapter.Fill(data) 'lancement de la requete et recup du resultat dans le datatable
                table = data
                Return ""
            Catch ex As Exception
                Return "ERR: SQL_Query : " & ex.Message
            End Try
        Else
            Return "ERR: SQL_Query : Non connecté au serveur"
        End If
    End Function

    Public Function mysql_nonquery(ByVal query As String) As String
        'Insert / Delete / Update...
        Dim commande As MySqlCommand
        If connected Then
            Try
                commande = New MySqlCommand(query, var_connexion) 'creation de la commande pour la requete
                commande.ExecuteNonQuery()
                Return ""
            Catch ex As Exception
                Return "ERR: SQL_NonQuery : " & ex.Message
            End Try
        Else
            Return "ERR: SQL_NonQuery : Non connecté au serveur"
        End If
    End Function

End Class
