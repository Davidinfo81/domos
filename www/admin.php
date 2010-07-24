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
	<script language="javascript" type="text/javascript" src="./include_cssjs/jquery-1.3.2.js"></script>
	<script language="javascript" type="text/javascript" src="./include_cssjs/interface.js"></script>
	<script language="javascript" type="text/javascript" src="./include_cssjs/contextmenu.js"></script>
</head>
<body>

<!--Context menu-->
    <div class="contextMenu" id="myMenufiseye" style="display:none;">
      <ul>
        <li id="accueil"><img src="images/menu/domos.png" width=24/> Accueil</li>
        <li id="cave"><img src="images/menu/maison_0.png" width=24/> Cave</li>
        <li id="rdc"><img src="images/menu/maison_1.png" width=24 /> RDC</li>
        <li id="premier"><img src="images/menu/maison_2.png" width=24 /> Premier</li>
        <li id="deuxieme"><img src="images/menu/maison_3.png" width=24 /> Deuxième</li>
        <li id="extterasse"><img src="images/menu/maison_4.png" width=24 /> Ext. Terasse</li>
        <li id="extrue"><img src="images/menu/maison_5.png" width=24 /> Ext. Rue</li>
        <li id="meteo"><img src="images/menu/meteo.gif" width=24 /> Météo</li>
        <li id="calendrier"><img src="images/menu/calendrier.png" width=24 /> Calendrier</li>
      </ul>
    </div>
	<script type="text/javascript">
			$(document).ready(
				function() {
					$('a.fisheyeItem').contextMenu('myMenufiseye', {
						menuStyle: {width: '120px'},
						itemStyle: {padding: '0px'},
						bindings: {
							'accueil': function(t) {window.location.href='domos.html';},
							'cave': function(t) {window.location.href='plan-cav.html';},
							'rdc': function(t) {window.location.href='plan-rdc.html';},
							'premier': function(t) {window.location.href='plan-unn.html';},
							'deuxieme': function(t) {window.location.href='plan-deu.html';},
							'extterasse': function(t) {window.location.href='plan-ex1.html';},
							'extrue': function(t) {window.location.href='plan-ex2.html';},
							'meteo': function(t) {window.location.href='meteo.html';},
							'calendrier': function(t) {window.location.href='calendrier.html';},
						}
					});
				}
			);
	</script>
<!--Fisheye menu-->
	<center>
	<table width="950" style="margin:0;padding:0;border:1px solid #53555F;background-color:#FFFFFF;">
	  	<tr align="left" valign="middle" colspan="2"><td width="100%" height="5" style="background-image:url('images/bandeau_2.jpg');background-position:top left;background-repeat:no-repeat;">
			<div id="fisheye" class="fisheye">
					<div class="fisheyeContainter">
						<a href="domos.html" class="fisheyeItem" id="accueil"><img src="images/menu/domos.png" /><span>Accueil</span></a>
						<a href="_composants.html" class="fisheyeItem"><img src="images/menu/composants.gif" /><span>Composants</span></a>
						<a href="_composants_bannis.html" class="fisheyeItem"><img src="images/menu/composants.gif" /><span>Composants bannis</span></a>
						<a href="_modeles.html" class="fisheyeItem"><img src="images/_modele.png" /><span>Modéles</span></a>
						<a href="_plans.html" class="fisheyeItem"><img src="images/menu/plans.gif" /><span>Plans</span></a>
						<a href="_macros.html" class="fisheyeItem"><img src="images/menu/macro.png" /><span>Macros</span></a>
						<a href="_users.html" class="fisheyeItem"><img src="images/menu/users.png" /><span>Users</span></a>
						<a href="_config.html" class="fisheyeItem"><img src="images/menu/config.png" /><span>Configuration</span></a>
						<a href="_logs.html" class="fisheyeItem"><img src="images/menu/logs.png" /><span>Logs</span></a>
						<a href="_divers.html" class="fisheyeItem"><img src="images/menu/display.png" /><span>Divers</span></a>
						<a href="_phpinfo.html" class="fisheyeItem"><img src="images/menu/info.png" /><span>phpinfo</span></a>
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
						proximity: 90,
						halign : 'center'
					}
				)
			}
		);
	</script> 
	
<!--Chargement du contenu-->
	  <tr>
	    <td colspan="2" align="center">
		<?php	//Inclusion des modules
		if(isset($_GET['page']) || isset($_POST['page'])){
			$page=isset($_GET['page'])?$_GET['page']:$_POST['page'];
			if(file_exists("./pages/".$page.".php")){include("./pages/".$page.".php");}else{echo"<center><b><br><br>La page demandée n'est pas disponible.<br><br></b></center>";};
		}
		//module par défaut
		else{
			include("./pages/logs.php");
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
