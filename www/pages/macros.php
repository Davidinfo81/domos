<?php
	if(isset($_SESSION['user_id'])){
?>
<div class="main">
 <table border="0" width="100%">
  <tbody>
    <tr>
      <td align="center" valign="middle">

	<!-- TITRE -->
        <table border="0" width="100%">
            <tr>
              <td class="titre" align="left"><img src="images/_composants.gif" width="32" height="32" border="0"> Gestion des Macros/Timers </td>
              <td align="right"> </td>
            </tr>
        </table>

	<!-- CONTENU -->
        <table width="92%" class="adminform">
          <tbody>
            <tr>
              <td width="100%" align="left" valign="top">
				<table border="0" cellspacing="0" cellpadding="0" width="100%">
<?php

include("./include_php/config.php");
$resultat = mysql_query("select config_valeur from config where config_nom='socket_ip'");
$adresse = mysql_result($resultat,0,"config_valeur");
$resultat = mysql_query("select config_valeur from config where config_nom='socket_port'");
$port = mysql_result($resultat,0,"config_valeur");

$action=isset($_GET["action"])?$_GET["action"]:(isset($_POST["action"])?$_POST["action"]:"gerer");
$macro_id=isset($_GET["macro_id"])?$_GET["macro_id"]:(isset($_POST["macro_id"])?$_POST["macro_id"]:"");

switch ($action) {
case "gerer" :
	echo "<tr height=\"23\" bgcolor=\"#5680CB\">
		<td align=right class=\"titrecolonne\"><a href=\"javascript:history.go(-1);\"><img src=\"./images/plus.gif\" border=\"0\"> Retour</a>&nbsp;&nbsp;&nbsp;</td>
	     </tr>\n";
	echo "
		<tr><td align=center><div class=\"plan\" id=\"plan\" style=\"padding:5px;\">
			<link rel=\"stylesheet\" type=\"text/css\" href=\"dhtmlx/dhtmlx.css\">
			<script  src=\"dhtmlx/dhtmlx.js\"></script>
			<div id=\"gridbox\" style=\"width:915px;height:488px;overflow:hidden\"></div> 
			<script>
				mygrid = new dhtmlXGridObject('gridbox');
				mygrid.setImagePath(\"./dhtmlx/imgs/\");
				mygrid.setSkin(\"dhx_skyblue\");
				mygrid.setHeader(\"&nbsp;,On,Nom,Description,Conditions,Actions\");
				mygrid.setInitWidths(\"30,30,160,*,0,0\");
				mygrid.setColAlign(\"center,center,left,left,left,left\");
				mygrid.setColTypes(\"link,ch,ed,ed,ro,ro\"); 
				mygrid.setColSorting(\"na,int,str,str,na,na\");
				mygrid.init();
				mygrid.loadXML(\"pages/dhtmlx_get.php?action=macros\");
				myDataProcessor = new dataProcessor(\"pages/dhtmlx_update_macros.php\");
				myDataProcessor.init(mygrid);
				myDataProcessor.enableUTFencoding(false);
				function deletee() {
					if (confirm('Supprimer ?')) {mygrid.deleteRow(mygrid.getSelectedId());}
				}
				function doOnRowSelected(rowID,celInd) {
					var span_conditions = document.getElementById('conditions');
					var span_actions = document.getElementById('actions');
					span_conditions.innerHTML = \"Conditions :  \" + mygrid.cells(rowID,4).cell.innerHTML;
					span_actions.innerHTML = \"Actions : \" + mygrid.cells(rowID,5).cell.innerHTML;
				}
				function sendsocket(message) {
					var XHR = new XHRConnection();
					XHR.appendData('tache', \"socket\");
					XHR.appendData('message', message);
					XHR.appendData('adresse', \"$adresse\");
					XHR.appendData('port', \"$port\");
					XHR.sendAndLoad('pages/actions.php', 'POST', afficherResultats_socket);
				}
				function afficherResultats_socket(obj) {
					alert(obj.responseText);
				}
				mygrid.attachEvent('onRowSelect',doOnRowSelected);
			</script>
			<input type=\"button\" name=\"a1\" value=\"Ajouter\" onClick=\"mygrid.addRow((new Date()).valueOf(),['','0','nom','description'],0)\" class=\"formsubmit\">
			<input type=\"button\" name=\"a1\" value=\"Supprimer\" onClick=\"deletee()\" class=\"formsubmit\">
			<input type=\"button\" name=\"a1\" value=\"Maj SVC\" onClick='sendsocket(\"([AS#maj_macro][AS#maj_timer])\")' class=\"formsubmit\"><br />
			<span id='conditions'> </span><br />
			<span id='actions'> </span><br /><br />
			Conditions -> composant : CC#id#=<>#etat, Timer : CT#ss#mm#hh#j#jj#mm#yy, Heure : CH#=<>#j#hh:mm: ss<br />
			Si = !(...) alors on ne teste que le composant, pas les autres conditions)<br />
			Actions -> composant : AC#id#etat, AS#service, AM#macro_id 
		</div></td></tr>
	";
	break;

case "modifier" :
	$resultat = mysql_query("select * from macro where macro_id='$macro_id'");
	$macro_nom = mysql_result($resultat,0,"macro_nom");
	$macro_description = mysql_result($resultat,0,"macro_description");
	$macro_actif = mysql_result($resultat,0,"macro_actif");
	$macro_conditions = mysql_result($resultat,0,"macro_conditions");
	$macro_actions = mysql_result($resultat,0,"macro_actions");
	echo "<tr height=\"23\" bgcolor=\"#5680CB\">
		<td align=left class=\"titrecolonne\"> &nbsp;..:: Modifier les conditions/actions d'une MACRO ::..</td>
		<td align=right class=\"titrecolonne\"><a href=\"macros.html\"><img src=\"./images/plus.gif\" border=\"0\"> Liste</a>&nbsp;&nbsp;&nbsp;</td>
	     </tr>\n";
	echo "<tr><td colspan=2>
	<div class=\"inscription\">
	<table width=100% border=0><form name=modifier_macro action=\"macros-modifiersave-$macro_id.html\" method=\"post\"><INPUT name=macro_actif type=hidden value=\"$macro_actif\">
	<tr height=20>
	  <td colspan=2 align=center>
		<TABLE border=0 cellPadding=0 cellSpacing=0 width=100%>
			<TR height=5><td align=left><table width=95% height=5 border=0><tr><TD background=../images/barre.gif><img src=../images/transp.gif height=5 width=1></TD></tr></table></td></TR>
			<TR><TD>
			  <TABLE border=0 cellPadding=0 cellSpacing=0 width=100%>
				<TR height=25>
				  <TD align=left valign=center width=150><b> :: Nom ::</b></TD>
				  <TD align=left valign=center>$macro_nom</TD>
				</TR>
				<TR height=25>
				  <TD align=left valign=center width=150><b> :: Description ::</b></TD>
				  <TD align=left valign=center>$macro_description</TD>
				</TR>
				<TR height=25>
				  <TD align=left valign=center width=150><b> :: Conditions ::</b></TD>
				  <TD align=left valign=center><INPUT name=macro_conditions type=text value=\"$macro_conditions\"></TD>
				</TR>
				<TR height=25>
				  <TD align=left valign=center width=150><b> :: Actions ::</b></TD>
				  <TD align=left valign=center><INPUT name=macro_actions type=text value=\"$macro_actions\"></TD>
				</TR>
			  </TABLE>
			</TD></TR>
			<TR><TD align=left height=30>
				<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type=submit value=modifier class=formsubmit>
			</TD></TR>
			<TR height=5><td align=left><table width=95% height=5 border=0><tr><TD background=../images/barre.gif><img src=../images/transp.gif height=5 width=1></TD></tr></table></td></TR>
		</table>
	  </td>
	</tr>
	</form>
	</table></div></td></tr>";
	break;

case "modifiersave" :
	$macro_id=$_GET["macro_id"];
	$macro_conditions = $_POST["macro_conditions"];
	$macro_actions = $_POST["macro_actions"];
	$macro_actif = $_POST["macro_actif"];
	$resultat = mysql_query("update macro set macro_conditions='$macro_conditions', macro_actions='$macro_actions' where macro_id='$macro_id'");
	//envoi par socket de la nouvelle condition/action
	$resultat = mysql_query("select config_valeur from config where config_nom='socket_ip'");
	$adresse = mysql_result($resultat,0,"config_valeur");
	$resultat = mysql_query("select config_valeur from config where config_nom='socket_port'");
	$port = mysql_result($resultat,0,"config_valeur");
	$search  = array('(', '[', '#', ']', ')');
	$replace = array('_1', '_2', '_3', '_4', '_5');
	$macro_conditions2=str_replace($search,$replace,$macro_conditions);
	$macro_actions2=str_replace($search,$replace,$macro_actions);
	if ($macro_actif==1) {socket_simple("([MM#".$macro_id."#".$macro_conditions2."#".$macro_actions2."])",$adresse,$port);}

	echo "<tr height=\"23\" bgcolor=\"#5680CB\">
			<td align=left class=\"titrecolonne\"> &nbsp;..:: Modifier les conditions/actions d'une MACRO ::..</td>
			<td align=right class=\"titrecolonne\"><a href=\"macros.html\"><img src=\"./images/plus.gif\" border=\"0\"> Liste</a>&nbsp;&nbsp;&nbsp;</td>
		     </tr>\n
		     <tr><td colspan=2><br /><br />Macro modifiée avec succès.<br /><br /><br /><br /><br /><br /><br /></td></tr>";
	break;
	
default : echo "<tr><td>choisis une action ci dessus</td></tr>";
	break;
}


?>
		</table>
 	      </td>
            </tr>
          </tbody>
        </table>
      </td>
    </tr>
  </tbody>
 </table>
</div>
<?php
	}else{
		header("location:../index.php");
	}
?>
