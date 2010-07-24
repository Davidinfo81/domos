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
              <td class="titre" align="left"><img src="images/_divers.gif" width="32" height="32" border="0"> Configuration </td>
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
			<script>
				mygrid = new dhtmlXGridObject('gridbox');
				mygrid.setImagePath('./dhtmlx/imgs/');
				mygrid.setSkin('dhx_skyblue');
				mygrid.setHeader('Nom,Valeur,Description');
				mygrid.setInitWidths('150,100,*');
				mygrid.setColAlign('left,left,left');
				mygrid.setColTypes('ro,ed,ro'); 
				mygrid.setColSorting('str,str,na');
				mygrid.init();
				mygrid.loadXML('pages/dhtmlx_get.php?action=config');
				myDataProcessor = new dataProcessor('pages/dhtmlx_update_config.php');
				myDataProcessor.init(mygrid);
				myDataProcessor.enableUTFencoding(false);
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
