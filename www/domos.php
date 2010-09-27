<?php
	session_start();
	if(isset($_SESSION['user_id'])){
		include("./include_php/config.php");
		include ("./include_php/fonctions.php");
		
?>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head>
	<title>DOMOS</title>
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1" />
	<link href="./include_cssjs/style.css" rel="stylesheet" type="text/css" />
	<link rel="SHORTCUT ICON" href="favicon.ico" />
    <link rel="icon" type="image/ico" href="favicon.ico" />
	<script language="javascript" type="text/javascript" src="./include_cssjs/script_XHRConnection.js"></script>
	<script language="javascript" type="text/javascript" src="./include_cssjs/jquery-1.4.2.min.js"></script>
	<script language="javascript" type="text/javascript" src="./include_cssjs/interface.js"></script>
	<script language="javascript" type="text/javascript" src="./include_cssjs/contextmenu.js"></script>
</head>
<body>
<!--Context menu-->
    <div class="contextMenu" id="myMenufisheye" style="display:none;">
      <ul>
        <li id="composants"><img src="images/menu/composants.gif" width=28/> Composants</li>
        <li id="composants_bannis"><img src="images/menu/composants.gif" width=28/> Comp. Bannis</li>
        <li id="modeles"><img src="images/_modele.png" width=28/> Modéles</li>
        <li id="plans"><img src="images/menu/plans.gif" width=28 /> Plans</li>
        <li id="macros"><img src="images/menu/macro.png" width=28 /> Macros</li>
        <li id="menu"><img src="images/menu/menu.png" width=28 /> Menu</li>
        <li id="users"><img src="images/menu/users.png" width=28 /> Users</li>
        <li id="webcamsadmin"><img src="images/menu/webcam.png" width=28 /> Webcams</li>
        <li id="configuration"><img src="images/menu/config.png" width=28 /> Configuration</li>
        <li id="phpinfo"><img src="images/menu/info.png" width=28 /> phpinfo</li>
      </ul>
    </div>
	<script type="text/javascript">
			$(document).ready(function() {
					$('a.fisheyeItem').contextMenu('myMenufisheye', {
						menuStyle: {width: '120px'},
						itemStyle: {padding: '0px'},
						bindings: {
							'composants': function(t) {window.location.href='composants.html';},
							'composants_bannis': function(t) {window.location.href='composants_bannis.html';},
							'modeles': function(t) {window.location.href='modeles.html';},
							'plans': function(t) {window.location.href='plans.html';},
							'macros': function(t) {window.location.href='macros.html';},
							'menu': function(t) {window.location.href='menu.html';},
							'users': function(t) {window.location.href='users.html';},
							'webcamsadmin': function(t) {window.location.href='webcamsadmin.html';},
							'configuration': function(t) {window.location.href='config.html';},
							'phpinfo': function(t) {window.location.href='phpinfo.html';}
						}
					});
			});
	</script>
<center>
<table width="950" style="margin:0;padding:0;border:1px solid #53555F;background-color:#FFFFFF;">
	<tr align="left" valign="middle" colspan="2"><td width="100%" height="5" style="background-image:url('images/bandeau_2.jpg');background-position:top left;background-repeat:no-repeat;">
		<div id="fisheye" class="fisheye">
				<div class="fisheyeContainter">
					<a href="domos.html" class="fisheyeItem"><img src="images/menu/domos.png" /><span>Accueil</span></a>
					<?php
					$resultat = mysql_query("select * from menu order by menu_ordre");
					if($resultat){
						while($row=mysql_fetch_array($resultat)){
							echo "<a href=\"plan-".$row['menu_lien'].".html\" class=\"fisheyeItem\"><img src=\"images/plans/menu_".$row['menu_lien'].".png\" /><span>".$row['menu_nom']."</span></a>";
						}
					}
					?>
					<a href="meteo.html" class="fisheyeItem"><img src="images/menu/meteo.gif" /><span>Météo</span></a>
					<a href="calendrier.html" class="fisheyeItem"><img src="images/menu/calendrier.png" /><span>Calendrier</span></a>
					<a href="logs.html" class="fisheyeItem"><img src="images/menu/logs.png" /><span>Logs</span></a>
					<a href="webcams.html" class="fisheyeItem"><img src="images/menu/webcam.png" /><span>Webcams</span></a>
					<a href="divers.html" class="fisheyeItem"><img src="images/menu/display.png" /><span>Divers</span></a>
				</div>
		</div>
	</td>
  </tr>
<script type="text/javascript">
	$(document).ready(
		function()
		{
			$('#fisheye').Fisheye(
				{
					maxWidth: 50,
					items: 'a',
					itemsText: 'span',
					container: '.fisheyeContainter',
					itemWidth: 40,
					proximity: 30,
					halign : 'center'
				}
			)
		}
	);
</script>
  <tr>
    <td colspan="2" align="center">
	<?php	//Inclusion des modules
	if(isset($_GET['page']) || isset($_POST['page'])){
		$page=isset($_GET['page'])?$_GET['page']:$_POST['page'];
		if(file_exists("./pages/".$page.".php")){include("./pages/".$page.".php");}else{echo"<center><b><br><br>La page demandée n'est pas disponible.<br><br></b></center>";};
	}
	//module par défaut
	else{
		include("./pages/accueil.php");
	} ?>
	</td>
  </tr>
</table>
</center>
</body>
</html>
<?php
	}else{
		header("location:index.html");
	}
?>
