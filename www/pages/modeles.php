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
              <td class="titre" align="left"><img src="images/_modele.png" width="32" height="32" border="0"> Gestion des Modéles de composants </td>
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

	echo "
		<tr><td align=center><div class=\"plan\" id=\"plan\" style=\"padding:5px;\">
			<link rel=\"stylesheet\" type=\"text/css\" href=\"dhtmlx/dhtmlx.css\">
			<script  src=\"dhtmlx/dhtmlx.js\"></script>
			<div id=\"gridbox\" style=\"width:915px;height:510px;overflow:hidden\"></div> 
			<input type=\"button\" name=\"a1\" value=\"Ajouter\" onClick=\"mygrid.addRow((new Date()).valueOf(),['WIR','','0',''],0)\" class=\"formsubmit\">
			<input type=\"button\" name=\"a1\" value=\"Supprimer\" onClick=\"deletee()\" class=\"formsubmit\">
			<script>
				mygrid = new dhtmlXGridObject('gridbox');
				mygrid.setImagePath('./dhtmlx/imgs/');
				mygrid.setSkin('dhx_skyblue');
				mygrid.setHeader('Norme,Nom,Graphe,Description');
				mygrid.setInitWidths('100,100,100,*');
				mygrid.setColAlign('center,left,center,left');
				mygrid.setColTypes('co,ed,co,ed'); 
				mygrid.setColSorting('str,str,str,str');
				mygrid.getCombo(0).put('WIR','1-Wire');
				mygrid.getCombo(0).put('WI2','1-Wire 2');
				mygrid.getCombo(0).put('PLC','PLCBUS');
				mygrid.getCombo(0).put('RFX','RFXCOM');
				mygrid.getCombo(0).put('X10','X10');
				mygrid.getCombo(2).put(0,'Aucun');
				mygrid.getCombo(2).put(1,'On-Off');
				mygrid.getCombo(2).put(2,'Cumul');
				mygrid.getCombo(2).put(3,'Numérique');
				mygrid.init();
				mygrid.loadXML('pages/dhtmlx_get.php?action=modeles');
				myDataProcessor = new dataProcessor('pages/dhtmlx_update_modeles.php');
				myDataProcessor.init(mygrid);
				myDataProcessor.enableUTFencoding(false);
				function deletee() {
					if (confirm('Supprimer ?')) {mygrid.deleteRow(mygrid.getSelectedId());}
				}
			</script>
		</div></td></tr>
	";
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
