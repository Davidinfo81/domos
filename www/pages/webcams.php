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

echo "<tr height=20><td colspan=5 align=center><div class=\"plan\" id=\"plan\">";


echo "<div class=\"plan\">";

	$sql = "SELECT * from webcams order by webcams_id";
	$res = mysql_query($sql);
	if(mysql_num_rows($res)>0){
		while($row=mysql_fetch_array($res)){
			echo "<div class='cadres_webcam'><table id='weather' cellpadding='4' cellspacing='0' width='205'>";
			echo "<tr><td colspan='2' align='left'><b> ".$row['webcams_nom']." :</b> ".$row['webcams_description']."</td></tr><tr>";
			echo "<td align=center><a href=\"".$row['webcams_lien']."\"><img src=\"".$row['webcams_lien']."\" class='imagewebcam'/></a></td>";
			echo "</tr></table>";
			echo "</div>";
		}
	}

echo "</div>";
?>

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