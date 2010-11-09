<?php 
echo '<?xml version="1.0" encoding="UTF-8"?>'; 

include("../include_php/config.php");
$composants_id=!empty($_REQUEST["composants_id"])?$_REQUEST["composants_id"]:"";
$typevaleur=!empty($_REQUEST["typevaleur"])?$_REQUEST["typevaleur"]:"";
$resultat = mysql_query("select * from composants where composants_id='$composants_id'");
$composants_nom = mysql_result($resultat,0,"composants_nom");
$composants_adresse = mysql_result($resultat,0,"composants_adresse");

$titre_graphe = $composants_nom." (".$composants_adresse." ID=".$composants_id.")";
$titre_chart = "$typevaleur :";
$titre_valeur = "temp:";

$texte_size_general = "11";
$texte_size_axes = "10";
$texte_size_legende = "11";
$text_size_titre = "14";

$axey_min = "0";
$axey_max = "10";
$axey_minmaxstrict = "false";
$axey_unite = "";

$graph_type = "line"; //line / column / candlestick / ohlc / step /step_no_risers
$graph_connect = "true"; //connexion entre les points si valeurs manquantes
$graph_periodvalue = "average"; //close / open / low / high / sum / average
$graph_titre = "titre du graphe";
$graph_smooth = "false"; //ligne droite ou arrondi
?>

<!-- Only the settings with values not equal to defaults are in this file. If you want to see the
full list of available settings, check the amstock_settings.xml file in the amstock folder. -->
<settings>
	<margins>5</margins>
	<redraw>true</redraw>
	<max_series>1000</max_series>
	<equal_spacing>false</equal_spacing>
	<js_enabled>false</js_enabled>
	<text_size><?php echo $texte_size_general ?></text_size>
    
	<number_format>  
		<separator>,</separator>
		<decimal_separator>.</decimal_separator>
		<thousand_separator> </thousand_separator>
	</number_format>
    
	<data_sets>
		<data_set did="1">
			<title>datasetun</title>
			<short>temp</short>
			<color>#7f8da9</color>
			<file_name>../pages/composants_grapheflashdata.php?composants_id=<?php echo $composants_id ?></file_name>
			<csv>
				<reverse>false</reverse>
				<date_format>YYYY-MM-DD hh:mm:ss</date_format>
				<separator>,</separator>
				<decimal_separator>.</decimal_separator>
				<columns>
					<column>date</column>
					<column>valeur</column>
				</columns>
			</csv>     
		</data_set>
	</data_sets>

	<charts>
		<chart cid="first">
			<height>100</height>
			<title><?php echo $titre_chart ?></title>
			<border_color>#CCCCCC</border_color>
			<border_alpha>80</border_alpha>
			<bg_color>e5e5e5,ffffff</bg_color>
			<bg_alpha>100</bg_alpha>
 
			<grid>
				<x>
					<enabled>true</enabled>
					<color>#888888</color>
					<alpha>30</alpha>
					<dashed>true</dashed>
					<dash_length>5</dash_length>
				</x>
				<y_left>
					<enabled>true</enabled>
					<color>#888888</color>
					<alpha>30</alpha>
					<dashed>true</dashed>
					<dash_length>5</dash_length>
				</y_left>
			</grid>
           
			<values>
				<x>
					<text_size><?php echo $texte_size_axes ?></text_size>
					<bg_color>#FFFFFF</bg_color>
				</x>  
				<y_left>
					<text_size><?php echo $texte_size_axes ?></text_size>
					<min><?php echo $axey_min ?></min>
					<max><?php echo $axey_max ?></max>
					<strict_min_max><?php echo $axey_minmaxstrict ?></strict_min_max>
					<unit><?php echo $axey_unite ?></unit>
				</y_left>
			</values>
            
			<legend>
				<text_size><?php echo $texte_size_legende ?></text_size>
				<show_date>true</show_date>
			</legend>

			<trend_lines>
				<drawing_enabled>true</drawing_enabled>
				<line_color>#CC0000</line_color>
				<line_alpha>50</line_alpha>
				<line_width>2</line_width>
				<dash_length>5</dash_length>
			</trend_lines>

			<graphs>
				<graph gid="1">
					<type><?php echo $graph_type ?></type>
					<connect><?php echo $graph_connect ?></connect>
					<period_value><?php echo $graph_periodvalue ?></period_value>
					<compare_source>valeur</compare_source>
					<title><?php echo $graph_titre ?></title>
					<color>#A0A0EE</color>
					<cursor_color>#FF0000</cursor_color>
					<cursor_alpha>50</cursor_alpha>
					<width>2</width>
					<alpha>80</alpha>
					<fill_alpha>15</fill_alpha>
					<bullet>round_outline</bullet>
					<bullet_size>5</bullet_size>
					<smoothed><?php echo $graph_smooth ?></smoothed>
  
					<data_sources>
						<close>valeur</close>
					</data_sources>

					<legend>
						<date key="false" title="false"><![CDATA[{close}]]></date>
						<period key="false" title="false"><![CDATA[{close} (min:{low} max:{high} moy:{average})]]></period>
					</legend>    
				</graph>           
			</graphs>

		</chart>    
	</charts>
    
	<data_set_selector>
		<enabled>false</enabled>
	</data_set_selector>
    
	<period_selector>
		<text_size><?php echo $texte_size_general ?></text_size>
		<periods_title>Zoom:</periods_title>
		<custom_period_title>Periode:</custom_period_title>
		<periods>      
			<period type="DD" count="1" selected="true">1D</period>
			<period type="DD" count="7">7D</period>
			<period type="MM" count="1">1M</period>
			<period type="MM" count="3">3M</period>
			<period type="MM" count="3">6M</period>
			<period type="YYYY" count="1">1Y</period>
			<period type="YYYY" count="1">3Y</period>
			<period type="YTD" count="0">YTD</period>
			<period type="MAX">MAX</period>
		</periods>
	</period_selector>

	<header>
		<enabled>true</enabled>
		<text><?php echo $titre_graphe ?></text>
		<text_size><?php echo $text_size_titre ?></text_size>
	</header>

	<plot_area>
		<margins>5</margins>
	</plot_area>

	<scroller>
		<height>50</height>
		<graph_data_source>valeur</graph_data_source>
		<connect><?php echo $graph_connect ?></connect>
		<frequency>1</frequency>
		<bg_color>d5d5d5,ffffff</bg_color>
		<graph_selected_fill_alpha>80</graph_selected_fill_alpha>
		<resize_button_style>dragger</resize_button_style>
		<grid>
			<dashed>true</dashed>
			<max_count>15</max_count>
			<color>#888888</color>
			<alpha>30</alpha>
		</grid>
		<values>
			<text_color>#888888</text_color>
		</values>
		<playback>
			<enabled>true</enabled>
			<speed>100</speed>
			<max_speed>500</max_speed>
			<loop>false</loop>
		</playback>
	</scroller>

	<context_menu>
		<default_items>
			<zoom>true</zoom>
		</default_items>
	</context_menu>

	<export_as_image>
		<file>releve</file>
		<target>_blank</target>
		<color>#5680CB</color>
		<alpha>50</alpha>
	</export_as_image>
	
	<strings>
		<processing_data>Traitement en cours</processing_data>
		<loading_data>Chargement des donnees</loading_data>
		<wrong_date_format>Mauvais format de date</wrong_date_format>
		<export_as_image>Exporter comme Image</export_as_image>
		<collecting_data>Collecte des donnees</collecting_data>
	</strings>
</settings>