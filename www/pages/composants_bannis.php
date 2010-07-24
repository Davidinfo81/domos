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
              <td class="titre" align="left"><img src="images/_composants.gif" width="32" height="32" border="0"> Gestion des Composants bannis </td>
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

$action=isset($_GET["action"])?$_GET["action"]:(isset($_POST["action"])?$_POST["action"]:"gerer");
$composants_id=isset($_GET["composants_id"])?$_GET["composants_id"]:(isset($_POST["composants_id"])?$_POST["composants_id"]:"");

switch ($action) {
case "gerer" :
	echo "<tr height=\"23\" bgcolor=\"#5680CB\">
		<td align=right class=\"titrecolonne\"><a href=\"javascript:history.go(-1);\"><img src=\"./images/plus.gif\" border=\"0\"> Retour</a>&nbsp;&nbsp;&nbsp;</td>
	     </tr>\n";
	echo "
		<tr><td align=center><div class=\"plan\" id=\"plan\" style=\"padding:5px;\">
			<link rel=\"stylesheet\" type=\"text/css\" href=\"dhtmlx/dhtmlx.css\">
			<script  src=\"dhtmlx/dhtmlx.js\"></script>
			<div id=\"gridbox\" style=\"width:915px;height:510px;overflow:hidden\"></div> 
			<script>
				mygrid = new dhtmlXGridObject('gridbox');
				mygrid.setImagePath(\"./dhtmlx/imgs/\");
				mygrid.setSkin(\"dhx_skyblue\");
				mygrid.setHeader(\"ID,Norme,Adresse,Description\");
				mygrid.setInitWidths(\"30,105,105,*\");
				mygrid.setColAlign(\"center,center,center,left\");
				mygrid.setColTypes(\"ro,co,ed,ed\"); 
				mygrid.setColSorting(\"int,str,str,str\");
				mygrid.getCombo(1).put('WIR','1-Wire');
				mygrid.getCombo(1).put('WI2','1-Wire 2');
				mygrid.getCombo(1).put('PLC','PLCBUS');
				mygrid.getCombo(1).put('RFX','RFXCOM');
				mygrid.getCombo(1).put('X10','X10');
				mygrid.init();
				mygrid.loadXML(\"pages/dhtmlx_get.php?action=composants_bannis\");
				myDataProcessor = new dataProcessor(\"pages/dhtmlx_update_composants_bannis.php\");
				myDataProcessor.init(mygrid);
				myDataProcessor.enableUTFencoding(false);
				function deletee() {
					if (confirm('Supprimer ?')) {mygrid.deleteRow(mygrid.getSelectedId());}
				}
			</script>
			<input type=\"button\" name=\"a1\" value=\"Ajouter\" onClick=\"mygrid.addRow((new Date()).valueOf(),['','','',''],0)\" class=\"formsubmit\">
			<input type=\"button\" name=\"a1\" value=\"Supprimer\" onClick=\"deletee()\" class=\"formsubmit\">
		</div></td></tr>
	";
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
