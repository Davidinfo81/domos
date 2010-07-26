<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class config
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(config))
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label4 = New System.Windows.Forms.Label
        Me.Label5 = New System.Windows.Forms.Label
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.txt_mysql_ip = New System.Windows.Forms.TextBox
        Me.txt_mysql_db = New System.Windows.Forms.TextBox
        Me.txt_mysql_login = New System.Windows.Forms.TextBox
        Me.txt_mysql_mdp = New System.Windows.Forms.TextBox
        Me.GroupBox2 = New System.Windows.Forms.GroupBox
        Me.txt_folder_install = New System.Windows.Forms.TextBox
        Me.BTN_Save = New System.Windows.Forms.Button
        Me.BTN_cancel = New System.Windows.Forms.Button
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(38, 19)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(54, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Server IP:"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(5, 46)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(87, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Database Name:"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(56, 73)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(36, 13)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Login:"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(36, 100)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(56, 13)
        Me.Label4.TabIndex = 3
        Me.Label4.Text = "Password:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(3, 16)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(89, 13)
        Me.Label5.TabIndex = 4
        Me.Label5.Text = "Installation folder:"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txt_mysql_mdp)
        Me.GroupBox1.Controls.Add(Me.txt_mysql_login)
        Me.GroupBox1.Controls.Add(Me.txt_mysql_db)
        Me.GroupBox1.Controls.Add(Me.txt_mysql_ip)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(257, 122)
        Me.GroupBox1.TabIndex = 5
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Mysql Connexion"
        '
        'txt_mysql_ip
        '
        Me.txt_mysql_ip.Location = New System.Drawing.Point(98, 16)
        Me.txt_mysql_ip.Name = "txt_mysql_ip"
        Me.txt_mysql_ip.Size = New System.Drawing.Size(152, 20)
        Me.txt_mysql_ip.TabIndex = 4
        '
        'txt_mysql_db
        '
        Me.txt_mysql_db.Location = New System.Drawing.Point(98, 43)
        Me.txt_mysql_db.Name = "txt_mysql_db"
        Me.txt_mysql_db.Size = New System.Drawing.Size(152, 20)
        Me.txt_mysql_db.TabIndex = 5
        '
        'txt_mysql_login
        '
        Me.txt_mysql_login.Location = New System.Drawing.Point(98, 70)
        Me.txt_mysql_login.Name = "txt_mysql_login"
        Me.txt_mysql_login.Size = New System.Drawing.Size(153, 20)
        Me.txt_mysql_login.TabIndex = 6
        '
        'txt_mysql_mdp
        '
        Me.txt_mysql_mdp.Location = New System.Drawing.Point(98, 97)
        Me.txt_mysql_mdp.Name = "txt_mysql_mdp"
        Me.txt_mysql_mdp.Size = New System.Drawing.Size(153, 20)
        Me.txt_mysql_mdp.TabIndex = 7
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.txt_folder_install)
        Me.GroupBox2.Controls.Add(Me.Label5)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 141)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(257, 45)
        Me.GroupBox2.TabIndex = 6
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Divers"
        '
        'txt_folder_install
        '
        Me.txt_folder_install.Enabled = False
        Me.txt_folder_install.ForeColor = System.Drawing.SystemColors.InactiveCaption
        Me.txt_folder_install.Location = New System.Drawing.Point(97, 16)
        Me.txt_folder_install.Name = "txt_folder_install"
        Me.txt_folder_install.Size = New System.Drawing.Size(153, 20)
        Me.txt_folder_install.TabIndex = 7
        '
        'BTN_Save
        '
        Me.BTN_Save.Location = New System.Drawing.Point(71, 199)
        Me.BTN_Save.Name = "BTN_Save"
        Me.BTN_Save.Size = New System.Drawing.Size(75, 23)
        Me.BTN_Save.TabIndex = 7
        Me.BTN_Save.Text = "Save"
        Me.BTN_Save.UseVisualStyleBackColor = True
        '
        'BTN_cancel
        '
        Me.BTN_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.BTN_cancel.Location = New System.Drawing.Point(153, 199)
        Me.BTN_cancel.Name = "BTN_cancel"
        Me.BTN_cancel.Size = New System.Drawing.Size(75, 23)
        Me.BTN_cancel.TabIndex = 8
        Me.BTN_cancel.Text = "Cancel"
        Me.BTN_cancel.UseVisualStyleBackColor = True
        '
        'config
        '
        Me.AcceptButton = Me.BTN_Save
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.BTN_cancel
        Me.ClientSize = New System.Drawing.Size(284, 234)
        Me.Controls.Add(Me.BTN_cancel)
        Me.Controls.Add(Me.BTN_Save)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "config"
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Domos Configuration"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents txt_mysql_ip As System.Windows.Forms.TextBox
    Friend WithEvents txt_mysql_mdp As System.Windows.Forms.TextBox
    Friend WithEvents txt_mysql_login As System.Windows.Forms.TextBox
    Friend WithEvents txt_mysql_db As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents txt_folder_install As System.Windows.Forms.TextBox
    Friend WithEvents BTN_Save As System.Windows.Forms.Button
    Friend WithEvents BTN_cancel As System.Windows.Forms.Button
End Class
