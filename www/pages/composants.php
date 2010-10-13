<?php
	if(isset($_SESSION['user_id'])){
		include("./include_php/config.php");
		$resultat = mysql_query("select config_valeur from config where config_nom='socket_ip'");
		$adresse = mysql_result($resultat,0,"config_valeur");
		$resultat = mysql_query("select config_valeur from config where config_nom='socket_port'");
		$port = mysql_result($resultat,0,"config_valeur");
		$resultat = mysql_query("select config_valeur from config where config_nom='menu_seticone'");
		$menuset = mysql_result($resultat,0,"config_valeur");

		$action=isset($_GET["action"])?$_GET["action"]:(isset($_POST["action"])?$_POST["action"]:"gerer");
		$composants_id=isset($_GET["composants_id"])?$_GET["composants_id"]:(isset($_POST["composants_id"])?$_POST["composants_id"]:"");
?>
<div class="main">
 <table border="0" width="100%">
  <tbody>
    <tr>
      <td align="center" valign="middle">

	<!-- TITRE -->
        <table border="0" width="100%">
            <tr>
              <td class="titre" align="left"><img src="images/menu/<?php echo "$menuset"; ?>/composants.gif" width="32" height="32" border="0"> Gestion des Composants </td>
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
switch ($action) {
case "gerer" :
	echo "<tr height=\"23\" bgcolor=\"#5680CB\">
		<td align=right class=\"titrecolonne\"><a href=\"javascript:history.go(-1);\"><img src=\"./images/plus.gif\" border=\"0\"> Retour</a>&nbsp;&nbsp;&nbsp;</td>
	     </tr>\n";
	echo "
		<tr><td align=center><div class=\"plan\" id=\"plan\" style=\"padding:5px;\">
			<link rel=\"stylesheet\" type=\"text/css\" href=\"dhtmlx/dhtmlx.css\">
			<script  src=\"dhtmlx/dhtmlx.js\"></script>
			<div id=\"gridbox\" style=\"width:915px;height:515px;overflow:hidden\"></div> 
			<script>
				mygrid = new dhtmlXGridObject('gridbox');
				mygrid.setImagePath(\"./dhtmlx/imgs/\");
				mygrid.setSkin(\"dhx_skyblue\");
				mygrid.setHeader(\"&nbsp;,ID,Nom,Modele,Adresse,Description,Poll,On,Valeur,Etat Date,Correction,Precision,Divers,Maj\");
				mygrid.setInitWidths(\"45,30,180,105,105,*,30,30,45,110,30,30,30,30\");
				mygrid.setColAlign(\"left,left,left,left,left,left,center,center,center,center,center,center,left,center\");
				mygrid.setColTypes(\"link,ro,ed,co,ed,txt,ed,ch,ed,ro,ed,ed,ed,ed\"); 
				mygrid.setColSorting(\"na,int,str,str,str,str,int,int,str,str,str,str,str,int\");
				";
				$resultat_tmp = mysql_query("SELECT * from composants_modele order by composants_modele_norme,composants_modele_nom");
				while($row=mysql_fetch_array($resultat_tmp)){echo "mygrid.getCombo(3).put(".$row['composants_modele_id'].",\"".$row['composants_modele_norme']."-".$row['composants_modele_nom']."\");";}
				echo "
				mygrid.init();
				mygrid.loadXML(\"pages/dhtmlx_get.php?action=composants\");
				myDataProcessor = new dataProcessor(\"pages/dhtmlx_update_composants.php\");
				myDataProcessor.init(mygrid);
				myDataProcessor.enableUTFencoding(false);
				function deletee() {
					if (confirm('Supprimer ?')) {mygrid.deleteRow(mygrid.getSelectedId());}
				}
				function grapher() {
					x = mygrid.getSelectedId();
					window.location.href='composants-grapherdyn-'+x+'.html'
				}
				function bannis() {
					x = mygrid.getSelectedId();
					window.location.href='composants_bannis.html'
				}
				function ajouter() {
					mygrid.addRow((new Date()).valueOf(),['','','nom',1,'','',0,0,'','2000-01-01 00:00:00','','0','','0']);
					setTimeout( \"ajouter2();\", 500);
				}
				function ajouter2() {
					window.location.href='composants.html'
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
			</script>
			<input type=\"button\" name=\"a1\" value=\"Ajouter\" onClick=\"ajouter()\" class=\"formsubmit\">
			<input type=\"button\" name=\"a1\" value=\"Supprimer\" onClick=\"deletee()\" class=\"formsubmit\">
			<input type=\"button\" name=\"a1\" value=\"Grapher\" onClick=\"grapher()\" class=\"formsubmit\">
			<input type=\"button\" name=\"a1\" value=\"Bannis\" onClick=\"bannis()\" class=\"formsubmit\">
			<input type=\"button\" name=\"a1\" value=\"Maj SVC\" onClick='sendsocket(\"([AS#maj_composants])\")' class=\"formsubmit\">
		</div></td></tr>
	";
	break;

case "grapher" :
	$composants_id=@$_GET["composants_id"];
	$resultat = mysql_query("select * from composants where composants_id='$composants_id'");
	$composants_nom = mysql_result($resultat,0,"composants_nom");	
	$composants_modele = mysql_result($resultat,0,"composants_modele");	
	$resultat = mysql_query("select composants_modele_graphe from composants_modele where composants_modele_id='$composants_modele'");
	$composants_modele_graphe = mysql_result($resultat,0,"composants_modele_graphe");	
							
	//recup des releves
	$resultat_jour = mysql_query("select * from releve where releve_composants='$composants_id' and releve_dateheure>'".date('Y-m-d H:i:s',time() - (1 * 24 * 60 * 60))."' order by releve_id");
	$resultat_semaine = mysql_query("select * from releve where releve_composants='$composants_id' and releve_dateheure>'".date('Y-m-d H:i:s',time() - (7 * 24 * 60 * 60))."' order by releve_id");
	$resultat_mois = mysql_query("select * from releve where releve_composants='$composants_id' and releve_dateheure>'".date('Y-m-d H:i:s',time() - (31 * 24 * 60 * 60))."' order by releve_id");
	$resultat_annee = mysql_query("select * from releve where releve_composants='$composants_id' and releve_dateheure>'".date('Y-m-d H:i:s',time() - (365 * 24 * 60 * 60))."' order by releve_id");

	//suppression des anciens graphes
	if (file_exists("./images/graphes/$composants_id-jour.png")) {unlink ("./images/graphes/$composants_id-jour.png");}
	if (file_exists("./images/graphes/$composants_id-semaine.png")) {unlink ("./images/graphes/$composants_id-semaine.png");}
	if (file_exists("./images/graphes/$composants_id-mois.png")) {unlink ("./images/graphes/$composants_id-mois.png");}
	if(file_exists("./images/graphes/$composants_id-annee.png")) {unlink ("./images/graphes/$composants_id-annee.png");}
		
	//dessin des graphes
	switch ($composants_modele_graphe) {
		case 1 : //ON/OFF
 			graphe_numerique($resultat_jour,$composants_id."-jour",dequote($composants_nom),"Dernieres 24 Heures",925,200,0,1.3,"H:i",45,0,1,0);
 			graphe_numerique($resultat_semaine,$composants_id."-semaine",dequote($composants_nom),"Les 7 derniers jours",925,200,0,1.3,"D d H:i",85,0,1,0);
 			graphe_numerique($resultat_mois,$composants_id."-mois",dequote($composants_nom),"Le dernier mois",925,200,0,1.3,"m/d H:i",80,0,1,0);
  			graphe_numerique($resultat_annee,$composants_id."-annee",dequote($composants_nom),"Cette année",925,200,0,1.3,"m/d H:i",80,0,1,0);		
			break;
		case 2 : //Cumul
			//graphe_cumul($resultat_jour,$composants_id."-jour",dequote($composants_nom),"Dernieres 24 Heures",925,350,0,2,"H:i",45,0,1,1);
 			//graphe_cumul($resultat_semaine,$composants_id."-semaine",dequote($composants_nom),"Les 7 derniers jours",925,350,0,2,"D d H:i",85,0,1,0);
 			//graphe_cumul($resultat_mois,$composants_id."-mois",dequote($composants_nom),"Le dernier mois",925,350,0,2,"m/d H:i",80,0,4,1);
 			//graphe_cumul($resultat_annee,$composants_id."-annee",dequote($composants_nom),"Cette année",925,350,0,2,"m/d H:i",80,0,8,1);
			graphe_compteur($resultat_jour,$composants_id."-jour",dequote($composants_nom),"Dernieres 24 Heures",925,350,0,2,"H:i",45);
 			graphe_compteur($resultat_semaine,$composants_id."-semaine",dequote($composants_nom),"Les 7 derniers jours",925,350,0,2,"D d H:i",85);
 			graphe_compteur($resultat_mois,$composants_id."-mois",dequote($composants_nom),"Le dernier mois",925,350,0,2,"m/d H:i",80);
 			graphe_compteur($resultat_annee,$composants_id."-annee",dequote($composants_nom),"Cette année",925,350,0,2,"m/d H:i",80);
			break;
		case 3 : //Numerique
 			graphe_numerique($resultat_jour,$composants_id."-jour",dequote($composants_nom),"Dernieres 24 Heures",925,350,0,25,"H:i",45,0,1,0);
 			graphe_numerique($resultat_semaine,$composants_id."-semaine",dequote($composants_nom),"Les 7 derniers jours",925,350,0,25,"D d H:i",85,0,1,0);
 			graphe_numerique($resultat_mois,$composants_id."-mois",dequote($composants_nom),"Le dernier mois",925,350,0,25,"m/d H:i",80,0,4,1);
 			graphe_numerique($resultat_annee,$composants_id."-annee",dequote($composants_nom),"Cette année",925,350,0,25,"m/d H:i",80,0,8,1);		
			break;
	}
	echo "<tr height=\"23\" bgcolor=\"#5680CB\">
		<td align=left class=\"titrecolonne\"> &nbsp;..:: Graphes du composant : $composants_nom ::..</td>
		<td align=right class=\"titrecolonne\"><a href=\"javascript:history.go(-1);\"><img src=\"./images/plus.gif\" border=\"0\"> Retour</a>&nbsp;&nbsp;&nbsp;<a href=\"composants-relever-$composants_id.html\"><img src=\"./images/plus.gif\" border=\"0\"> Relevés</a>&nbsp;&nbsp;&nbsp;<a href=\"composants-gerer.html\"><img src=\"./images/plus.gif\" border=\"0\"> Gérer</a>&nbsp;&nbsp;&nbsp;</td>
	     </tr>\n";
 	if ((!file_exists("./images/graphes/$composants_id-jour.png")) && (!file_exists("./images/graphes/$composants_id-semaine.png")) && (!file_exists("./images/graphes/$composants_id-mois.png")) && (!file_exists("./images/graphes/$composants_id-annee.png"))) {echo "<tr><td colspan=2 align=center><br />Pas de graphes disponibles<br /><br /></td></tr>";}
	if (file_exists("./images/graphes/$composants_id-jour.png")) {echo "<tr><td colspan=2 align=center><br /><img src=\"./images/graphes/$composants_id-jour.png\"></td></tr>";}
	if (file_exists("./images/graphes/$composants_id-semaine.png")) {echo "<tr><td colspan=2 align=center><br /><img src=\"./images/graphes/$composants_id-semaine.png\"></td></tr>";}
	if (file_exists("./images/graphes/$composants_id-mois.png")) {echo "<tr><td colspan=2 align=center><br /><img src=\"./images/graphes/$composants_id-mois.png\"></td></tr>";}
	if (file_exists("./images/graphes/$composants_id-annee.png")) {echo "<tr><td colspan=2 align=center><br /><img src=\"./images/graphes/$composants_id-annee.png\"></td></tr>";}
	break;
	

case "grapherdyn" :
	$composants_id=@$_GET["composants_id"];
	$resultat = mysql_query("select * from composants where composants_id='$composants_id'");
	$composants_nom = mysql_result($resultat,0,"composants_nom");	
	$composants_modele = mysql_result($resultat,0,"composants_modele");	
	$resultat = mysql_query("select composants_modele_graphe from composants_modele where composants_modele_id='$composants_modele'");
	$composants_modele_graphe = mysql_result($resultat,0,"composants_modele_graphe");	

	echo "<tr height=\"23\" bgcolor=\"#5680CB\">
		<td align=left class=\"titrecolonne\"> &nbsp;..:: Graphe dynamique du composant : $composants_nom ::..</td>
		<td align=right class=\"titrecolonne\"><a href=\"javascript:history.go(-1);\"><img src=\"./images/plus.gif\" border=\"0\"> Retour</a>&nbsp;&nbsp;&nbsp;<a href=\"composants-relever-$composants_id.html\"><img src=\"./images/plus.gif\" border=\"0\"> Relevés</a>&nbsp;&nbsp;&nbsp;<a href=\"composants-gerer.html\"><img src=\"./images/plus.gif\" border=\"0\"> Gérer</a>&nbsp;&nbsp;&nbsp;</td>
	     </tr>\n";
	     
	echo "<tr><td colspan=2 align=center>
	<script type=\"text/javascript\">
	function sendgraphe() {
		document.getElementById('graphe').innerHTML='In progress...';
		var XHR = new XHRConnection();
		
		
		 
		 
		request=\"select * from releve where releve_composants='$composants_id' and releve_dateheure>'\"+document.getElementById('gdatemin').value+\"' and releve_dateheure<'\"+document.getElementById('gdatemax').value+\"' order by releve_id\"
		XHR.appendData('request', request);
		XHR.appendData('gnomimage', '".$composants_id."_dyn');
		XHR.appendData('gtitre', document.getElementById('gtitre').value);
		XHR.appendData('gtitrex', document.getElementById('gtitrex').value);
		XHR.appendData('glarg', document.getElementById('glarg').value);
		XHR.appendData('ghaut', document.getElementById('ghaut').value);
		XHR.appendData('gymin', document.getElementById('gymin').value);
		XHR.appendData('gymax', document.getElementById('gymax').value);
		XHR.appendData('gdateformat', document.getElementById('gdateformat').value);
		XHR.appendData('gmarge', document.getElementById('gmarge').value);
		if (document.getElementsByName('gmoyenne')[0].checked) {gmoyenne=1;} else {gmoyenne=0;}
		XHR.appendData('gmoyenne', gmoyenne);
		XHR.appendData('gnbvaleurreel', document.getElementById('gnbvaleurreel').value);
		XHR.appendData('gtypegraphe',document.getElementById('gtypegraphe').value);	
		XHR.sendAndLoad('pages/composants_graphe.php', 'POST', affichergraphe);	
	}
	function affichergraphe(obj) {
		//document.getElementById('graphe').innerHTML='obj.responseText';
		document.getElementById('graphe').innerHTML='';
		var monimg = document.createElement('img');						
		//monimg.id ='grapheimg';
		monimg.setAttribute('src','./images/graphes/".$composants_id."_dyn.png?'+new Date().getTime());
		document.getElementById('graphe').appendChild(monimg);
		//document.getElementById('grapheimg').src='./images/graphes/".$composants_id."_dyn.png';
	}
	</script>
	<form class='formgraphe' action='javascript:sendgraphe()'><br />
		Date min <INPUT class='texte' type='text' name='gdatemin' id='gdatemin' value='".date('Y-m-d H:i:s',time() - (31 * 24 * 60 * 60))."' onchange='javascript:sendgraphe()'>&nbsp;&nbsp;
		Date max <INPUT class='texte' type='text' name='gdatemax' id='gdatemax' value='".date('Y-m-d H:i:s')."' onchange='javascript:sendgraphe()'>&nbsp;&nbsp;
		Titre <INPUT class='texte' type='text' name='gtitre' id='gtitre' value='".dequote($composants_nom)."' onchange='javascript:sendgraphe()'>&nbsp;&nbsp;
		Titre x <INPUT class='texte' type='text' name='gtitrex' id='gtitrex' value='' onchange='javascript:sendgraphe()'>
		Date <Select id='gdateformat' class='select' onchange='javascript:sendgraphe()'>
			<option value='H:i'>H:i
			<option value='d H:i'>d H:i
			<option value='D d H:i'>D d H:i
			<option value='D'>D
			<option value='m/d H:i' selected>m/d H:i
			<option value='Y/m/d'>Y/m/d
			<option value='Y/m/d H:i'>Y/m/d H:i
			</select>&nbsp;&nbsp;
		<br />
		Type <Select id='gtypegraphe' class='select' onchange='javascript:sendgraphe()'>
			<option value='1' selected>Line
			<option value='0'>Plot
			</select>&nbsp;&nbsp;
		Moyenne <INPUT type='checkbox' name='gmoyenne' onchange='javascript:sendgraphe()'>&nbsp;&nbsp;
		Largeur <INPUT type='text' name='glarg' id='glarg' value='925' onchange='javascript:sendgraphe()'>&nbsp;&nbsp;
		Hauteur <INPUT type='text' name='ghaut' id='ghaut' value='350' onchange='javascript:sendgraphe()'>&nbsp;&nbsp;
		Y min <INPUT type='text' name='gymin' id='gymin' value='0' onchange='javascript:sendgraphe()'>&nbsp;&nbsp;
		Y max <INPUT type='text' name='gymax' id='gymax' value='1' onchange='javascript:sendgraphe()'>&nbsp;&nbsp;
		Marge <INPUT type='text' name='gmarge' id='gmarge' value='80' onchange='javascript:sendgraphe()'>&nbsp;&nbsp;
		Nb valeurs <INPUT type='text' name='gnbvaleurreel' id='gnbvaleurreel' value='1' onchange='javascript:sendgraphe()'><br /><br />
		<INPUT type='submit' class='formsubmit' value='Grapher'><br /><br />
	</form>
	<span id='graphe'>Choisis les options !</span>
	</td></tr>";
	
	
	//dessin des graphes
	//graphe_numerique($resultat,$composants_id."-tous",dequote($composants_nom),"Tous les releves",900,300,0,1,"m/d H:i",80,0,1,0);
	//if (file_exists("./images/graphes/$composants_id-tous.png")) {
	//	echo "<tr><td colspan=2 align=center><br /><img src=\"./images/graphes/$composants_id-tous.png\"></td></tr>";
	//} else {echo "<tr><td colspan=2 align=center><br />Pas de graphes disponibles<br /><br /></td></tr>";}
	
	break;
case "relever" :
	$composants_id=@$_GET["composants_id"];
	$resultat = mysql_query("select * from composants where composants_id='$composants_id'");
	$composants_nom = mysql_result($resultat,0,"composants_nom");
	$composants_modele = mysql_result($resultat,0,"composants_modele");	
	$resultat = mysql_query("select composants_modele_graphe from composants_modele where composants_modele_id='$composants_modele'");
	$composants_modele_graphe = mysql_result($resultat,0,"composants_modele_graphe");	
	echo "<tr height=\"23\" bgcolor=\"#5680CB\">
		<td align=left class=\"titrecolonne\"> &nbsp;..:: Relevé du composant : $composants_nom ::..</td>
		<td align=right class=\"titrecolonne\"><a href=\"javascript:history.go(-1);\"><img src=\"./images/plus.gif\" border=\"0\"> Retour</a>&nbsp;&nbsp;&nbsp;";
	if ($composants_modele_graphe>0) {echo "<a href=\"composants-grapher-$composants_id.html\"><img src=\"./images/plus.gif\" border=\"0\"> Graphe</a>&nbsp;&nbsp;&nbsp;";}
	echo "<a href=\"composants-gerer.html\"><img src=\"./images/plus.gif\" border=\"0\"> Gérer</a>&nbsp;&nbsp;&nbsp;</td>
	     </tr>
		<tr><td align=center colspan=2><div class=\"plan\" id=\"plan\" style=\"padding:5px;\">
			<link rel=\"stylesheet\" type=\"text/css\" href=\"dhtmlx/dhtmlx.css\">
			<script  src=\"dhtmlx/dhtmlx.js\"></script>
			<div id=\"gridbox\" style=\"width:920px;height:510px;overflow:hidden\"></div> 
			<span id='nbrelevex'>Nb relevés : </span>
			<input type=\"button\" name=\"a1\" value=\"Retour\" onClick=\"javascript:history.go(-1)\" class=\"formsubmit\">
			<input type=\"button\" name=\"a1\" value=\"Ajouter\" onClick=\"mygrid.addRow((new Date()).valueOf(),[$composants_id,'2000-01-01 00:00:00',''],0)\" class=\"formsubmit\">
			<input type=\"button\" name=\"a1\" value=\"Supprimer\" onClick=\"deletee()\" class=\"formsubmit\">
			<script>
				function setcounter() {
					var span = document.getElementById('nbrelevex');
					span.innerHTML = 'Nb relevés : '+mygrid.getRowsNum();
				}
				function deletee() {
					x = mygrid.getSelectedId();
					//mygrid.deleteRow(mygrid.getSelectedId());
					mygrid.deleteSelectedRows();
					if (x>1) {x=x-1;}
					mygrid.selectid(x);
				}
				mygrid = new dhtmlXGridObject('gridbox');
				mygrid.setImagePath('./dhtmlx/imgs/');
				mygrid.setSkin('dhx_skyblue');
				mygrid.setHeader('Composant,Date Heure,Valeur');
				mygrid.setInitWidths('0,150,*');
				mygrid.setColAlign('center,center,left');
				mygrid.setColTypes('ro,ed,ed'); 
				mygrid.setColSorting('str,date,str');
				mygrid.enableMultiselect(true);
				mygrid.init();
				mygrid.enableSmartRendering(true,50);
				//mygrid.attachEvent('onXLE', setCounter);
				mygrid.loadXML('pages/dhtmlx_get.php?action=relever&compid=$composants_id&limitmin=0&limitmax=10000');
				myDataProcessor = new dataProcessor('pages/dhtmlx_update_relever.php');
				myDataProcessor.init(mygrid);
				myDataProcessor.enableUTFencoding(false);
				timeoutHnd = setTimeout(setcounter, 200);
				timeoutHnd = setTimeout(setcounter, 500);
				timeoutHnd = setTimeout(setcounter, 1000);
				timeoutHnd = setTimeout(setcounter, 10000);
				timeoutHnd = setTimeout(setcounter, 30000);
			</script>
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
