
CREATE TABLE `composants` (
  `composants_id` smallint(6) unsigned NOT NULL AUTO_INCREMENT,
  `composants_modele` smallint(6) unsigned NOT NULL,
  `composants_nom` tinytext NOT NULL,
  `composants_adresse` tinytext NOT NULL,
  `composants_description` text NOT NULL,
  `composants_polling` double NOT NULL,
  `composants_actif` tinyint(1) NOT NULL DEFAULT '1',
  `composants_etat` tinytext NOT NULL,
  `composants_etatdate` datetime NOT NULL DEFAULT '2009-01-01 01:01:01',
  `composants_correction` tinytext NOT NULL,
  `composants_precision` text NOT NULL,
  `composants_divers` tinytext NOT NULL,
  `composants_maj` smallint(6) NOT NULL,
  PRIMARY KEY (`composants_id`)
) ENGINE=MyISAM AUTO_INCREMENT=4 DEFAULT CHARSET=latin1;
INSERT INTO `composants` (`composants_id`, `composants_modele`, `composants_nom`, `composants_adresse`, `composants_description`, `composants_polling`, `composants_actif`, `composants_etat`, `composants_etatdate`, `composants_correction`, `composants_precision`, `composants_divers`, `composants_maj`) VALUES
('1', '32', 'JOUR', 'jour', '1=jour, 0=nuit', '0', '1', '1', '2010-01-01 01:01:01', '', '0', '', '0'),
('2', '32', 'JOUR2', 'jour2', '1=jour, 0=nuit, avec correction', '0', '1', '1', '2010-01-01 01:01:01', '', '0', '', '0'),
('3', '32', 'MODE_NUIT', 'mode_nuit', 'Si mode-nuit=1, tout le monde fait dodo', '0', '1', '0', '2010-01-01 01:01:01', '', '0', '', '0');

CREATE TABLE `composants_bannis` (
  `composants_bannis_id` smallint(6) unsigned NOT NULL AUTO_INCREMENT,
  `composants_bannis_norme` tinytext NOT NULL,
  `composants_bannis_adresse` tinytext NOT NULL,
  `composants_bannis_description` text NOT NULL,
  PRIMARY KEY (`composants_bannis_id`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1;

CREATE TABLE `composants_modele` (
  `composants_modele_id` smallint(6) unsigned NOT NULL AUTO_INCREMENT,
  `composants_modele_nom` tinytext NOT NULL,
  `composants_modele_description` text NOT NULL,
  `composants_modele_norme` tinytext NOT NULL,
  `composants_modele_graphe` smallint(6) unsigned NOT NULL,
  PRIMARY KEY (`composants_modele_id`)
) ENGINE=MyISAM AUTO_INCREMENT=75 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
INSERT INTO `composants_modele` (`composants_modele_id`, `composants_modele_nom`, `composants_modele_description`, `composants_modele_norme`, `composants_modele_graphe`) VALUES
('1', 'DS18B20', 'Capteur de temp&eacute;rature', 'WIR', '3'),
('2', 'DS2406_relais', 'Switch pour commander un relais', 'WIR', '1'),
('3', 'DS2406_capteur', 'D&eacute;tecteur de un contact ON/OFF', 'WIR', '1'),
('4', 'MS13', 'D&eacute;tecteur de mouvement', 'RFX', '1'),
('5', 'SS13_KR19-22', 'Interrupteur X10 RF', 'RFX', '1'),
('6', 'LM12', Module Lampe X10', 'X10', '1'),
('7', '3160', 'Volet roulant PLCBus', 'PLC', '1'),
('8', 'DS18B20', 'Capteur de temp&eacute;rature', 'WI2', '3'),
('9', 'DS2406_capteur', 'interrupteur', 'WI2', '1'),
('10', 'OREGON_THE', 'Sonde Oregon Temperature', 'RFX', '3'),
('11', 'OREGON_HUM', 'Sonde Oregon Humidité', 'RFX', '3'),
('12', 'OREGON_WID', 'Sonde Oregon Vent Direction Degré', 'RFX', '0'),
('13', 'OREGON_WIF', 'Sonde Oregon Vent Force', 'RFX', '3'),
('14', 'OREGON_BAR', 'Sonde Oregon Barometre', 'RFX', '0'),
('16', 'OREGON_FOR', 'Sonde Oregon Temps', 'RFX', '0'),
('17', 'OREGON_BAT', 'Sonde Oregon Batterie', 'RFX', '0'),
('18', 'OREGON_WIS', 'Sonde Oregon Vent Vitesse', 'RFX', '0'),
('19', 'OREGON_RAF', 'Sonde Oregon Pluie Tombé', 'RFX', '0'),
('20', 'OREGON_RAT', 'Sonde Oregon Pluie Total', 'RFX', '0'),
('21', 'OREGON_RAP', 'Sonde Oregon Pluie Flip', 'RFX', '0'),
('22', 'OREGON_WIL', 'Sonde Oregon Vent Direction Lettres', 'RFX', '0'),
('23', 'OREGON_UVV', 'Sonde Oregon UV Valeur', 'RFX', '3'),
('24', 'OREGON_UVL', 'Sonde Oregon UV Level', 'RFX', '0'),
('25', 'OREGON_CT1', 'Sonde Oregon Courant 1', 'RFX', '0'),
('26', 'OREGON_CT2', 'Sonde Oregon Courant 2', 'RFX', '0'),
('27', 'OREGON_CT3', 'Sonde Oregon Courant 3', 'RFX', '0'),
('30', '2263-2264', 'Module Lampe', 'PLC', '3'),
('31', '2267-2268', 'Module Appareil', 'PLC', '1'),
('32', 'VIRTUEL', 'Norme pour les composants virtuels', 'VIR', '3'),
('33', 'GB10', 'D&eacute;tecteur bris de glace et luminosit&eacute;', 'RFX', '0'),
('34', 'WD18', 'Detecteur eau', 'RFX', '0'),
('35', 'SD10', 'D&eacute;tecteur de fum&eacute;e', 'RFX', '0'),
('36', 'COD18', 'Detecteur CO', 'RFX', '0'),
('37', 'GD18', 'D&eacute;tecteur de Gaz', 'RFX', '0'),
('38', 'DS10', 'Détecteur de contact X10RF', 'RFX', '1'),
('39', 'HOMEEASY', 'Emetteurs-Recepteurs HomeEasy', 'RFX', '1'),
('40', 'DS2423_A', 'Compteur A du dualcounter', 'WIR', '2'),
('41', 'DS2423_B', 'Compteur B du dualcounter', 'WIR', '2'),
('42', 'RFXPower', 'Mesure tension', 'RFX', '2'),
('43', 'OREGON_THE', 'Sonde Oregon Temperature', 'ZIB', 3),
('44', 'OREGON_BAT', 'Sonde Oregon Batterie', 'ZIB', 0),
('46', 'OREGON_HUM', 'Sonde Oregon Humidite', 'ZIB', 3),
('47', 'ZIB_LNK', 'Etat de la zibase', 'ZIB', 0),
('48', 'ZIB_STA', 'Etat d un switch zibase', 'ZIB', 1),
('49', 'OREGON_TEMC', 'température de consigne thermostat Oregon', 'ZIB', 3),
('50', 'OWL_KWH', 'Sonde OWL Energie totale', 'ZIB', 2),
('51', 'OWL_KW', 'Sonde OWL Energie instantanee', 'ZIB', 3),
('52', 'OREGON_TRA', 'Sonde Oregon Pluie Total', 'ZIB', 2),
('53', 'OREGON_CRA', 'Sonde Oregon Pluie Courant', 'ZIB', 3),
('54', 'OREGON_AWI', 'Sonde Oregon Vent Force', 'ZIB', 3),
('55', 'OREGON_DRT', 'Sonde Oregon Vent Direction', 'ZIB', 3),
('56', 'OREGON_UVL', 'Sonde Oregon UV Valeur', 'ZIB', 3),
('57', 'CHACON_detect', 'Detecteurs Chacon mouvement-fumee-gaz', 'ZIB', 1),
('58', 'CHACON_action', 'Actionneurs Chacon', 'ZIB', 1),
('59', 'X10_divers', 'Actionneurs X10', 'ZIB', 1),
('60', 'BROADC_action', 'Actionneurs BROADCAST', 'ZIB', 1),
('61', 'DOMIA_action', 'Actionneurs DOMIA', 'ZIB', 1),
('62', 'VIS433_action', 'Actionneurs VISONIC 433', 'ZIB', 1),
('63', 'VIS868_action', 'Actionneurs VISONIC 868', 'ZIB', 1),
('64', 'CHACON', 'Emetteurs-Recepteurs Chacon', 'RFX', 1),
('65', 'CHACON', 'Recepteur Chacon', 'TSK', 1),
('66', 'HOMEEASY', 'Recepteur HomeEasy', 'TSK', 1),
('67', 'DS2408', 'Switch pour commander 8 relais', 'WIR', '1'),
('68', 'DS2413', 'Switch pour commander 2 relais', 'WIR', '1'),
('69', 'AM12', 'Module Appareil', 'X10', '1'),
('70', 'AD10', 'Interrupteur Rail-Din', 'X10', '1'),
('71', 'LD11', 'Interrupteur variateur Rail-Din', 'X10', '1'),
('72', 'SW10', 'Interrupteur Volet roulant', 'X10', '1'),
('73', 'SW12', 'MicroModule Volet roulant', 'X10', '1'),
('74', '2269', 'Module Scenes', 'PLC', '1');

CREATE TABLE `config` (
  `config_id` smallint(6) unsigned NOT NULL AUTO_INCREMENT,
  `config_nom` text NOT NULL,
  `config_valeur` text NOT NULL,
  `config_description` text NOT NULL,
  PRIMARY KEY (`config_id`)
) ENGINE=MyISAM AUTO_INCREMENT=43 DEFAULT CHARSET=latin1;
INSERT INTO `config` (`config_id`, `config_nom`, `config_valeur`, `config_description`) VALUES
('1', 'Serv_X10', '1', 'X10 : 0=desactive 1=active'),
('2', 'Serv_WIR', '1', '1-wire : 0=desactive 1=active'),
('3', 'Serv_PLC', '1', 'PLC-BUS : 0=desactive 1=active'),
('4', 'Serv_RFX', '1', 'RFXCOM receiver : 0=desactive 1=active'),
('5', 'X10_timeout', '500', 'Timeout pour attendre que le port X10 soit disponible en ecriture (Defaut : 500 = 5 sec)'),
('6', 'Serv_WI2', '0', '1-wire2 : 0=desactive 1=active'),
('7', 'Port_PLC', 'COM9', 'Numero du port COM du PLCBUS-1141 : COM6'),
('8', 'Port_RFX', 'COM8', 'Adresse IP ou numero port COM : COM4'),
('9', 'Port_X10', 'COM1', 'Numero du port COM du X10-CM11 : COM1'),
('10', 'log_niveau', '-0-1-2-3-4-5-6-9', '0-Prog,1-Err crit,2-Err general,3-Msg recues,4-Lancement macro/timer,5-Actions macro/timer,6-Valeurs ayant change,7-Valeurs n ayant pas change,8-Valeurs inchange precision/lastetat,9-Divers,10-Debug'),
('11', 'log_dest', '2', '0=txt, 1=sql, 2=txt sql'),
('12', 'meteo_codeville', 'FRXX0076', 'Code Ville Prevision weather.com (FRXX0076)'),
('13', 'meteo_icone', '2', 'Numero du pack d icones meteo'),
('14', 'meteo_codevillereleve', 'FRXX0076', 'Code Ville Releve weather.com (FRXX0076)'),
('15', 'logs_nbparpage', '1000', 'Nb de logs/Releves a afficher par page'),
('16', 'Serv_ZIB', '0', 'Zibase : 0=desactive 1=active'),
('17', 'Port_WIR', 'USB1', 'Nom du port de la cle USB WIR ex: USB1 ou COM1'),
('18', 'Port_WI2', 'USB2', 'Nom du port de la cle USB WI2 ex: USB1 ou COM1'),
('19', 'rfx_tpsentrereponse', '1600', 'Temps entre deux receptions de valeurs a prendre en compte (pour eviter les doublons/triplons) (1600 par defaut  = 1.6sec)'),
('20', 'PLC_timeout', '500', 'Timeout pour attendre que le port PLCBUS soit disponible en ecriture (Defaut : 500 = 5 sec)'),
('21', 'socket_ip', '127.0.0.1', 'Socket : Adresse IP du serveur'),
('22', 'socket_port', '3852', 'Socket : Port du serveur (3852)'),
('23', 'socket_portgui', '3853', 'Socket : Port du serveur GUI (3853)'),
('24', 'lastetat', '1', '0=desactive 1=active : ne pas prendre en compte les variations 19.1 19.2 19.1...'),
('25', 'WIR_res', '0.1', 'resolution 1-wire : 0.1 / 0.5'),
('26', 'Action_timeout', '500', 'TimeOut quand un thread est deja actif sur un composant lors d une action.'),
('27', 'heure_coucher_correction', '0', 'Ajout de x minutes a l heure de couche du soleil'),
('28', 'heure_lever_correction', '0', 'Ajout de x minutes a l heure de leve du soleil'),
('29', 'WIR_adaptername', '{DS9490}', 'Nom de l adaptateur onewire usb:{DS9490} serie:{DS9097U} autre:{DS9097U_DS9480}'),
('30', 'gps_longitude', '48.8616', 'longitude de ta maison (calcul soleil)'),
('31', 'gps_latitude', '2.3517', 'Latitude de ta maison (calcul soleil)'),
('32', 'logs_erreur_nb', '3', 'Nb d erreurs identique a loguer pendant logs_erreur_duree'),
('33', 'logs_erreur_duree', '60', 'Duree pendant laquelle on ne logue pas plus de logs_erreur_nb erreurs identiques'),
('34', 'menu_seticone', '1', 'Numero du set d icones pour le menu (www\\images\\menu\\x)'),
('35', 'Serv_TSK', '0', 'Tellstick : 0=desactive 1=active'),
('36', 'mail_smtp', '', 'Adresse du serveur smtp'),
('37', 'mail_from', '', 'Adresse mail qui enverra le mail'),
('38', 'mail_to', '', 'Ton adresse email'),
('39', 'WIR_timeout', '500', 'Timeout pour attendre que le port WIR soit disponible en ecriture (Defaut : 500 = 5 sec)'),
('40', 'ZIB_timeout', '500', 'Timeout pour attendre que le port ZIB soit disponible en ecriture (Defaut : 500 = 5 sec)'),
('41', 'TSK_timeout', '500', 'Timeout pour attendre que le port TSK soit disponible en ecriture (Defaut : 500 = 5 sec)'),
('42', 'mail_action', '4', 'activer les mails 0=desactive 1=manuel 2=manuel-auto 3=manuel-auto-erreurcritique 4=manuel-auto-erreurcritique-erreursredondante 5=manuel-auto-erreurs'),

CREATE TABLE `logs` (
  `logs_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `logs_source` tinytext NOT NULL,
  `logs_description` text NOT NULL,
  `logs_date` datetime NOT NULL,
  PRIMARY KEY (`logs_id`)
) ENGINE=MyISAM AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;
INSERT INTO `logs` (`logs_id`, `logs_source`, `logs_description`, `logs_date`) VALUES
('1', '0', 'installation', '2010-10-01 00:00:00');

CREATE TABLE `macro` (
  `macro_id` smallint(6) unsigned NOT NULL AUTO_INCREMENT,
  `macro_nom` tinytext NOT NULL,
  `macro_description` text NOT NULL,
  `macro_conditions` text NOT NULL,
  `macro_actions` text NOT NULL,
  `macro_actif` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`macro_id`)
) ENGINE=MyISAM AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;
INSERT INTO `macro` (`macro_id`, `macro_nom`, `macro_description`, `macro_conditions`, `macro_actions`, `macro_actif`) VALUES
('1', 'Mode_nuit : ON', 'Passer en mode nuit a 23h00', '([CT#=#0#00#23#*#*#*])', '([AC#3#1])', '1'),
('2', 'Mode_nuit : OFF', 'Quitter le mode nuit a 6h30', '([CT#=#0#30#6#*#*#*])', '([AC#3#0])', '1'),
('3', 'Task - SQL Maintenance', 'Optimiser les tables et Purger les logs de 2 mois', '([CT#=#0#0#4#*#*#*])', '([AN#SQL#purgelogs][AN#SQL#optimise][AN#SQL#reconnect])', '1'),
('4', 'TEST - Log en mode nuit', 'Log un texte quand passage en mode nuit', '([CC#3#=#1])', '([AL#TEST : passage en mode nuit])', '1');

CREATE TABLE `menu` (
`menu_id` smallint(6) unsigned NOT NULL AUTO_INCREMENT PRIMARY KEY ,
`menu_ordre` int(11) NOT NULL ,
`menu_nom` tinytext NOT NULL ,
`menu_lien` tinytext NOT NULL
) ENGINE=MyISAM AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;
INSERT INTO `menu` (`menu_id`, `menu_ordre`, `menu_nom`, `menu_lien`) VALUES
('1', '1', 'test', 'test');

CREATE TABLE `plan` (
  `plan_id` smallint(6) unsigned NOT NULL AUTO_INCREMENT,
  `plan_composant` bigint(20) NOT NULL,
  `plan_nomplan` text NOT NULL,
  `plan_top` smallint(6) NOT NULL,
  `plan_left` smallint(6) NOT NULL,
  `plan_visible` tinyint(4) NOT NULL,
  PRIMARY KEY (`plan_id`)
) ENGINE=MyISAM AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;
INSERT INTO `plan` (`plan_id`, `plan_composant`, `plan_nomplan`, `plan_top`, `plan_left`, `plan_visible`) VALUES
('1', '1', 'test_jour', '20', '20', '1');

CREATE TABLE `releve` (
  `releve_id` bigint(15) unsigned NOT NULL AUTO_INCREMENT,
  `releve_composants` smallint(6) unsigned NOT NULL,
  `releve_valeur` tinytext NOT NULL,
  `releve_dateheure` datetime NOT NULL,
  PRIMARY KEY (`releve_id`),
  KEY `releve_composants` (`releve_composants`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1;

CREATE TABLE `users` (
  `id` smallint(6) unsigned NOT NULL AUTO_INCREMENT,
  `login` tinytext NOT NULL,
  `pwd` tinytext NOT NULL,
  `droits` smallint(6) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=4 DEFAULT CHARSET=latin1;
INSERT INTO `users` (`id`, `login`, `pwd`, `droits`) VALUES
('1', 'administrateur', 'domos', '9'),
('2', 'utilisateur', 'domos', '5'),
('3', 'visiteur', 'domos', '1');

CREATE TABLE `webcams` (
  `webcams_id` smallint(6) unsigned NOT NULL AUTO_INCREMENT PRIMARY KEY ,
  `webcams_nom` TINYTEXT NOT NULL ,
  `webcams_description` TEXT NOT NULL ,
  `webcams_lien` TEXT NOT NULL ,
  `webcams_accueil` TINYINT NOT NULL
) ENGINE = MYISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1;