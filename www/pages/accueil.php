<?php
	if(isset($_SESSION['user_id'])){ 
?>
<div class="main">
 <table border="0" width="100%">
  <tbody>
    <tr>
      <td align="center" valign="middle">
        <table border="0" width="100%">
    
<?php
include("./include_php/config.php");
$action=isset($_GET["action"])?$_GET["action"]:(isset($_POST["action"])?$_POST["action"]:"rdc");

//Recup de la config meteo
$resultat = mysql_query("select config_valeur from config where config_nom='meteo_codeville'");
$meteo_codeville = mysql_result($resultat,0,"config_valeur");
$resultat = mysql_query("select config_valeur from config where config_nom='meteo_codevillereleve'");
$meteo_codevillereleve = mysql_result($resultat,0,"config_valeur");
$resultat = mysql_query("select config_valeur from config where config_nom='meteo_icone'");
$meteo_icone = mysql_result($resultat,0,"config_valeur");

echo "<tr height='20'><td colspan='5' align='center'><div class='plan' id='plan'>";

echo "<div class=cadregauche>";

	//Affichage des dernieres Erreurs
	echo "<div class='cadresl'><table id='weather' cellpadding='4' cellspacing='0' width='100%'>";
	echo "<tr><td colspan='2' align='left'><b> Les derniéres erreurs :</b></td></tr>";
	$sql = "SELECT distinct logs_description, logs_date  from logs where logs_source = '2' order by logs_date desc limit 0,9 ";
	$res = mysql_query ($sql);
	if($res){while($row=mysql_fetch_array($res)){echo ("<tr><td align='center' width='140'>".$row['logs_date']."</td><td align=left>".substr($row['logs_description'],0,95)."...</td></tr>");}}
	else {echo "<tr><td colspan='2'><br /><br /> Pas de logs <br /><br /><br /></td></tr>";}
	echo "</table>";
	echo "<div class='lien'><a href=logs.html> ... </a></div>";
	echo "</div>";
	
	//Affichage des dernieres Alertes
	echo "<div class='cadresl'><table id='weather' cellpadding='4' cellspacing='0' width='100%'>";
	echo "<tr><td colspan='2' align='left'><b> Les derniéres alertes :</b></td></tr>";
	$sql = "SELECT DISTINCT logs_date, logs_description from logs where logs_description like '%Alert%' order by logs_date desc limit 0,9 ";
	//$sql = "SELECT * from logs where logs_description like '%Alert%' order by logs_date desc limit 0,9 ";
	$res = mysql_query ($sql);
	if($res){while($row=mysql_fetch_array($res)){echo ("<tr><td align='center' width='140'>".$row['logs_date']."</td><td align=left>".$row['logs_description']."</td></tr>");}}
	else {echo "<tr><td colspan='2'><br /><br /> Pas de logs <br /><br /><br /></td></tr>";}
	echo "</table>";
	echo "<div class='lien'><a href='logs.html'> ... </a></div>";
	echo "</div>";
	
	//Affichage des composants non à jour
	echo "<div class='cadresl'><table id='weather' cellpadding='4' cellspacing='0' width='100%'>";
	echo "<tr><td colspan='2' align='left'><b> Les composants non à jour :</b></td></tr>";
	$sql = "SELECT * from composants where composants_actif=1 and composants_maj>0 order by composants_nom desc limit 0,20 ";
	$res = mysql_query($sql);
	if(mysql_num_rows($res)>0){
		while($row=mysql_fetch_array($res)){
			$Timestamp = strtotime (date("Y/m/d H:i:s"))-(($row['composants_maj'])*60*60); // on enleve x heures a la date actuelle
			$dateheure = date ('Y-m-d H:i:s', $Timestamp); // on reconverti en date = date actuelle - x heures
			if (($row['composants_etatdate'])<$dateheure) {echo ("<tr><td align='center' width='140'>".$row['composants_etatdate']."</td><td align='left'>".$row['composants_nom']." : ".$row['composants_adresse']."</td></tr>");}
			//echo ("<tr><td align='center' width='140'>".$row['composants_etatdate']." ".$dateheure."</td><td align='left'>".$row['composants_nom']." : ".$row['composants_adresse']." : ".$row['composants_maj']."</td></tr>");
		}
	} else {echo "<tr><td colspan='2'><br />Pas de composants non à jour ! <br /><br /></td></tr>";}
	echo "</table>";
	echo "<div class='lien'><a href='composants.html'> ... </a></div>";
	echo "</div>";

echo "</div><div class=cadredroit>";

	//affichage de la météo
	echo "<div class='cadresr'><table id='weather' cellpadding='4' cellspacing='0' width='200'><tr>";
	require_once("./include_php/wdweather.php");
	$weather = new WdWeather();
	$todays = $weather->getWeather2($meteo_codevillereleve, 1);
	if ($todays) {
		foreach ($todays as $today) {
		    echo "<td>";
		    echo "<img src=\"./images/wdweather/$meteo_icone/".$today['icon'].".png\" alt=\"".$weather->tempstoFR($today['t'])."\" title=\"".$weather->tempstoFR($today['t'])."\" /><br />";
		    echo $today['tmp'] == "N/A" ? "" : ($today['tmp'])."°C - ".$today['hmid']."%<br />";
		    echo "Vent du ".$today['wind']['t']." à ".$today['wind']['s']." km/h <br />";
		    echo $today['bar']['r']."mb -> ".$today['bar']['d']."<br />";
		    echo "Visibilité à ".$today['vis']." km<br />";
		    echo "UVs ".$today['uv']['i']."/10 (".$today['uv']['t'].")";
		    echo "</td>";
		}
	} else {echo "<td align=center><br /><br /><br />Erreur<br />météo<br /><br /><br /></td>";}
	echo "</tr></table>";
	echo "<div class='lien'><a href='meteo.html'> ... </a></div>";
	echo "</div>";

	//affichage des webcams
	$sql = "SELECT * from webcams where webcams_accueil>'0' order by webcams_accueil";
	$res = mysql_query($sql);
	if(mysql_num_rows($res)>0){
		while($row=mysql_fetch_array($res)){
			echo "<div class='cadresr'><table id='weather' cellpadding='4' cellspacing='0' width='205'>";
			echo "<tr><td colspan='2' align='left'><b> ".$row['webcams_nom']." :</b></td></tr><tr>";
			echo "<td align=center><a href=\"".$row['webcams_lien']."\"><img src=\"".$row['webcams_lien']."\" id=\"webcam\" /></a></td>";
			echo "</tr></table>";
			echo "</div>";
		}
	}


echo "</div>";

echo "</div></td></tr>";
?>
        </table>
      </td>
    </tr>
  </tbody>
 </table>
</div>
<?php
	}else{
		header("location:../index.html");
	}
?>