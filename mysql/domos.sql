
CREATE TABLE `composants` (
  `composants_id` int(11) NOT NULL AUTO_INCREMENT,
  `composants_modele` int(11) NOT NULL,
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
  `composants_maj` int(11) NOT NULL,
  PRIMARY KEY (`composants_id`)
) ENGINE=MyISAM AUTO_INCREMENT=3 DEFAULT CHARSET=latin1;
INSERT INTO `composants` (`composants_id`, `composants_modele`, `composants_nom`, `composants_adresse`, `composants_description`, `composants_polling`, `composants_actif`, `composants_etat`, `composants_etatdate`, `composants_correction`, `composants_precision`, `composants_divers`, `composants_maj`) VALUES
('1', '32', 'JOUR', 'jour', '1=jour, 0=nuit', '0', '1', '0', '2010-07-16 22:06:05', '', '0', '', '0'),
('2', '32', 'JOUR2', 'jour2', '1=jour, 0=nuit, avec correction', '0', '0', '1', '2009-01-01 01:01:01', '', '0', '', '0');

CREATE TABLE `composants_bannis` (
  `composants_bannis_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `composants_bannis_norme` tinytext NOT NULL,
  `composants_bannis_adresse` tinytext NOT NULL,
  `composants_bannis_description` text NOT NULL,
  PRIMARY KEY (`composants_bannis_id`)
) ENGINE=MyISAM AUTO_INCREMENT=3 DEFAULT CHARSET=latin1;

CREATE TABLE `composants_modele` (
  `composants_modele_id` int(11) NOT NULL AUTO_INCREMENT,
  `composants_modele_nom` tinytext NOT NULL,
  `composants_modele_description` text NOT NULL,
  `composants_modele_norme` tinytext NOT NULL,
  `composants_modele_graphe` int(11) NOT NULL,
  PRIMARY KEY (`composants_modele_id`)
) ENGINE=MyISAM AUTO_INCREMENT=43 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
INSERT INTO `composants_modele` (`composants_modele_id`, `composants_modele_nom`, `composants_modele_description`, `composants_modele_norme`, `composants_modele_graphe`) VALUES
('1', 'DS18B20', 'Capteur de temp&eacute;rature', 'WIR', '3'),
('2', 'DS2406_relais', 'Switch pour commander un relais', 'WIR', '1'),
('3', 'DS2406_capteur', 'D&eacute;tecteur de contact ON/OFF', 'WIR', '1'),
('4', 'MS13', 'D&eacute;tecteur de mouvement', 'RFX', '1'),
('5', 'SS13_KR19-22', 'Interrupteur X10 RF', 'RFX', '1'),
('6', 'LM12', 'Plugin Lampe X10', 'X10', '1'),
('7', '3160', 'Volet roulant PLCBus', 'PLC', '1'),
('8', 'DS18B20', 'Capteur de temp&eacute;rature', 'WI2', '3'),
('9', 'DS2406_capteur', 'interrupteur', 'WI2', '1'),
('10', 'OREGON_THE', 'Sonde Oregon Temperature', 'RFX', '3'),
('11', 'OREGON_HYG', 'Sonde Oregon Hygrometrie', 'RFX', '3'),
('12', 'OREGON_WID', 'Sonde Oregon Vent Direction Degr�', 'RFX', '0'),
('13', 'OREGON_WIF', 'Sonde Oregon Vent Force', 'RFX', '3'),
('14', 'OREGON_BAR', 'Sonde Oregon Barometre', 'RFX', '0'),
('15', 'OREGON_HUM', 'Sonde Oregon Humidit�', 'RFX', '0'),
('16', 'OREGON_FOR', 'Sonde Oregon Temps', 'RFX', '0'),
('17', 'OREGON_BAT', 'Sonde Oregon Batterie', 'RFX', '0'),
('18', 'OREGON_WIS', 'Sonde Oregon Vent Vitesse', 'RFX', '0'),
('19', 'OREGON_RAF', 'Sonde Oregon Pluie Tomb�', 'RFX', '0'),
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
('39', 'DS10', 'D�tecteur de contact X10RF', 'RFX', '1'),
('40', 'telecommande', 'T�l�commande basique', 'RFX', '1'),
('41', 'DS2423_A', 'Compteur A du dualcounter', 'WIR', '2'),
('42', 'DS2423_B', 'Compteur B du dualcounter', 'WIR', '2');

CREATE TABLE `config` (
  `config_id` int(11) NOT NULL AUTO_INCREMENT,
  `config_nom` text NOT NULL,
  `config_valeur` text NOT NULL,
  `config_description` text NOT NULL,
  PRIMARY KEY (`config_id`)
) ENGINE=MyISAM AUTO_INCREMENT=34 DEFAULT CHARSET=latin1;
INSERT INTO `config` (`config_id`, `config_nom`, `config_valeur`, `config_description`) VALUES
('1', 'Serv_X10', '0', '0=d�sactive 1=activ�'),
('2', 'Serv_WIR', '1', '0=d�sactive 1=activ�'),
('3', 'Serv_PLC', '1', '0=d�sactive 1=activ�'),
('4', 'Serv_RFX', '1', '0=d�sactive 1=activ�'),
('6', 'Serv_WI2', '0', '0=d�sactive 1=activ�'),
('7', 'Port_PLC', 'COM9', 'Num�ro du port COM du PLCBUS-1141 : COM6'),
('8', 'Port_RFX', 'COM8', 'Adresse IP ou numero port COM : COM4'),
('9', 'Port_X10', 'COM7', 'Num�ro du port COM du X10-CM11 : COM1'),
('10', 'log_niveau', '0-1-2-3-4-5-6-9', '0-Prog,1-Err crit,2-Err g�n�ral,3-Msg re�ues,4-Lancement macro/timer,5-Actions macro/timer,6-Valeurs ayant chang�,7-Valeurs n ayant pas chang�,8-Valeurs inchang� pr�cision/lastetat,9-Divers'),
('11', 'log_dest', '2', '0=txt, 1=sql, 2=txt sql'),
('12', 'meteo_codeville', 'FRXX1222', 'Code Ville Pr�vision weather.com (FRXX1222)'),
('13', 'meteo_icone', '2', 'Num�ro du pack d\'ic�nes m�t�o'),
('14', 'meteo_codevillereleve', 'LUXX0003', 'Code Ville Relev� weather.com (LUXX0003)'),
('15', 'logs_nbparpage', '1000', 'Nb de logs/Relev�s � afficher par page'),
('16', 'Serv_VIR', '1', '0=d�sactive 1=activ�'),
('17', 'Port_WIR', 'USB1', 'Nom du port de la cl� USB WIR'),
('18', 'Port_WI2', 'USB2', 'Nom du port de la cl� USB WI2'),
('19', 'rfx_tpsentrereponse', '1500', 'Temps entre deux r�ceptions de valeurs � prendre en compte (pour �viter les doublons/triplons) (1500 par d�faut  = 1.5sec)'),
('20', 'PLC_timeout', '500', 'Timeout pour attendre que le port PLCBUS soit disponible en ecriture (D�faut : 500 = 5 sec)'),
('21', 'socket_ip', '192.168.1.2', 'Socket : Adresse IP du serveur'),
('22', 'socket_port', '3852', 'Socket : Port du serveur (3852)'),
('23', 'Serv_SOC', '1', 'Activ� la connexion Socket domos-php'),
('24', 'lastetat', '1', '0=d�sactiv� 1=activ� : ne pas prendre en compte les variations 19.1 19.2 19.1...'),
('25', 'WIR_res', '0.1', 'r�solution 1-wire : 0.1 / 0.5'),
('26', 'Action_timeout', '500', 'TimeOut quand un thread est d�j� actif sur un composant lors d\'un action.'),
('27', 'heure_coucher_correction', '60', 'Ajout de x minutes � l heure de couch� du soleil'),
('28', 'heure_lever_correction', '40', 'Ajout de x minutes � l heure de lev� du soleil'),
('33', 'WIR_adaptername', '', 'Nom de l adaptateur onewire'),
('34', 'gps_longitude', '49.3637', 'longitude de ta maison (calcul soleil)'),
('35', 'gps_latitude', '6.0529', 'Latitude de ta maison (calcul soleil)');

CREATE TABLE `logs` (
  `logs_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `logs_source` tinytext NOT NULL,
  `logs_description` text NOT NULL,
  `logs_date` datetime NOT NULL,
  PRIMARY KEY (`logs_id`)
) ENGINE=MyISAM AUTO_INCREMENT=648325 DEFAULT CHARSET=latin1;

CREATE TABLE `macro` (
  `macro_id` int(11) NOT NULL AUTO_INCREMENT,
  `macro_nom` tinytext NOT NULL,
  `macro_description` text NOT NULL,
  `macro_conditions` text NOT NULL,
  `macro_actions` text NOT NULL,
  `macro_actif` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`macro_id`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1;

CREATE TABLE `plan` (
  `plan_id` bigint(20) NOT NULL AUTO_INCREMENT,
  `plan_composant` bigint(20) NOT NULL,
  `plan_nomplan` text NOT NULL,
  `plan_top` int(11) NOT NULL,
  `plan_left` int(11) NOT NULL,
  `plan_visible` tinyint(4) NOT NULL,
  PRIMARY KEY (`plan_id`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1;

CREATE TABLE `releve` (
  `releve_id` bigint(15) unsigned NOT NULL AUTO_INCREMENT,
  `releve_composants` int(6) NOT NULL,
  `releve_valeur` tinytext NOT NULL,
  `releve_dateheure` datetime NOT NULL,
  PRIMARY KEY (`releve_id`),
  KEY `releve_composants` (`releve_composants`)
) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1;

CREATE TABLE `users` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `login` tinytext NOT NULL,
  `pwd` tinytext NOT NULL,
  `droits` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=4 DEFAULT CHARSET=latin1;
INSERT INTO `users` (`id`, `login`, `pwd`, `droits`) VALUES
('1', 'administrateur', 'domos', '9'),
('2', 'utilisateur', 'domos', '5'),
('3', 'visiteur', 'domos', '1');
