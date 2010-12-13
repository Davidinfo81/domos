



DELETE FROM `composants` WHERE composants_modele=15

DELETE FROM `composants_modele`
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
('74', '2269', 'Module Scenes', 'PLC', '1'),
('75', 'fastpooling', 'composant permettant d'effectuer un fastpooling sur le range de son adresse', 'PLC', '0');

UPDATE `config` SET `config_description`='X10 : 0=desactive 1=active' where `config_id`=1
UPDATE `config` SET `config_description`='1-wire : 0=desactive 1=active' where `config_id`=2
UPDATE `config` SET `config_description`='PLC-BUS : 0=desactive 1=active' where `config_id`=3
UPDATE `config` SET `config_description`='RFXCOM receiver : 0=desactive 1=active' where `config_id`=4
UPDATE `config` SET `config_description`='1-wire2 : 0=desactive 1=active' where `config_id`=6
UPDATE `config` SET `config_valeur`='-0-1-2-3-4-5-6-9', `config_description`='0-Prog,1-Err crit,2-Err general,3-Msg recues,4-Lancement macro/timer,5-Actions macro/timer,6-Valeurs ayant change,7-Valeurs n ayant pas change,8-Valeurs inchange precision/lastetat,9-Divers,10-Debug' where `config_id`=10
UPDATE `config` SET `config_nom`='Serv_ZIB', `config_valeur`='0', `config_description`='Zibase : 0=desactive 1=active' where `config_id`=16
UPDATE `config` SET `config_description`='Nom du port de la cle USB WIR ex: USB1 ou COM1' where `config_id`=17
UPDATE `config` SET `config_description`='Nom du port de la cle USB WI2 ex: USB1 ou COM1' where `config_id`=18
UPDATE `config` SET `config_valeur`='1600' where `config_id`=19
UPDATE `config` SET `config_nom`='socket_portgui', `config_valeur`='3853', `config_description`='Socket : Port du serveur GUI (3853)' where `config_id`=23
UPDATE `config` SET `config_description`='Nom de l adaptateur onewire usb:{DS9490} serie:{DS9097U} autre:{DS9097U_DS9480}' where `config_id`=29

INSERT INTO `config` (`config_id`, `config_nom`, `config_valeur`, `config_description`) VALUES
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
('43', 'PLC_triphase', '0', 'Configuration TriPhase PLCBUS (0:normal, 1:triphase)'), 
('44', 'RFX_ignoreadresse', '0', 'Ignore les adresses composants incorrectes (0:affiche, 1:ignore)'),
('45', 'ZIB_ignoreadresse', '0', 'Ignore les adresses composants incorrectes (0:affiche, 1:ignore)'),
('46', 'PLC_ack', '1', 'PLCBUS : Gestion des acks (renvoyer l ordre si ack non recu) : 0=desactive 1=active');
