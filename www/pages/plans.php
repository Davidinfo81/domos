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
              <td class="titre" align="left"><img src="images/_plans.gif" width="32" height="32" border="0"> Gestion des Plans </td>
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
			<input type=\"button\" name=\"a1\" value=\"Ajouter\" onClick=\"mygrid.addRow((new Date()).valueOf(),['1','0','0','0',''],0)\" class=\"formsubmit\">
			<input type=\"button\" name=\"a1\" value=\"Supprimer\" onClick=\"deletee()\" class=\"formsubmit\"><br /><br />
			
			<script>
				mygrid = new dhtmlXGridObject('gridbox');
				mygrid.setImagePath('./dhtmlx/imgs/');
				mygrid.setSkin('dhx_skyblue');
				mygrid.setHeader('Composant,X,Y,Visible,Plan');
				mygrid.setInitWidths('250,50,50,50,*');
				mygrid.setColAlign('left,left,center,center,left');
				mygrid.setColTypes('co,ed,ed,ch,ed'); 
				mygrid.setColSorting('str,str,int,int,str');
				";
				$resultat_tmp = mysql_query("SELECT composants_id,composants_nom from composants order by composants_nom");
				while($row=mysql_fetch_array($resultat_tmp)){echo "mygrid.getCombo(0).put(".$row['composants_id'].",\"".dequote($row['composants_nom'])."\");";}
				echo "
				mygrid.init();
				mygrid.loadXML('pages/dhtmlx_get.php?action=plans');
				myDataProcessor = new dataProcessor('pages/dhtmlx_update_plans.php');
				myDataProcessor.init(mygrid);
				myDataProcessor.enableUTFencoding(false);
				function deletee() {
					if (confirm('Supprimer ?')) {mygrid.deleteRow(mygrid.getSelectedId());}
				}
			</script>
		</div>
		Type de plan : temp, hyg, div, voletaction, voletactionetat, voletetat, lampe, lampeplc, jour.<br /></td></tr>
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
