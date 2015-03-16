# General #

Domos has some basic parameters in the registry :
  * IP Address, Login, password and database name for SQL Server : Domos must have this information to connect to the database to retrieve the configuration, components.. and store logs and states. You can found this information in the registry : "HKLM\SOFTWARE\Domos" and manage it with the GUI.

But most of parameters are managed on the web interface :
  * Serv\_X10 = 0 : Activate X10 with a CM11 - 0=inactive 1=active
  * Serv\_WIR = 1 : Activate 1-wire with and USB Key - 0=inactive 1=active
  * Serv\_PLC = 1 : Activate PLCBUS with one PLC-1141 - 0=inactive 1=active
  * Serv\_RFX = 1 : Activate RFXCOM with a RFX-receiver Ethernet or USB - 0=inactive 1=active
  * Serv\_WI2 = 0 : Activate a second 1-wire bus with a second USB Key - 0=inactive 1=active (Not used at this time)
  * Serv\_VIR = 1 : Activate virtual component (obligatory active at this time)   0=inactive 1=active

  * Port\_PLC = COM8 : COM port name for PLCBUS-1141
  * Port\_RFX = COM4 : IP Address or COM port Name
  * Port\_X10 = COM1 : COM port name for X10-CM11 : COM1
  * Port\_WIR = USB1 : COM port name for the first 1-wire USB Key
  * Port\_WI2 = USB2 : COM port name for the second 1-wire USB Key

  * rfx\_tpsentrereponse = 1500 : Time in ms between two value reception (other value will be ignored during this time, to avoid multiple identical message send by components)
  * PLC\_timeout = 500 : Time-out in ms to wait for the PLCBUS disponibility before writing to it.
  * socket\_ip = 192.168.1.20 : IP address of Domos Service (for permitting the website to communicate with it)
  * socket\_port = 3852 : Socket Port Number of Domos Service (3852)
  * Serv\_SOC = 1 : Activate connexion by Socket
  * lastetat = 1 : No save value equal to N-2 value (to avoid for ex : 18.9 - 19 - 18.9 - 19...) - 0=inactive 1=active
  * WIR\_res = 0.1 : resolution for DS18B20 1-wire component : 0.1 / 0.5
  * WIR\_adaptername = "{DS9490}" : Name of 1-wire adapter : By default : "{DS9490"}

  * log\_niveau = 0-1-2-3-4-5-6-7-8-9-A...
    * 0 : Programme : start/ stop / restart...
    * 1 : Critical error : error that could stop the programm
    * 2 : Standard error : component not found, sql error, conversion...
    * 3 : Messages received
    * 4 : Macro/timer start
    * 5 : macro/timer action
    * 6 : Component value changed
    * 7 : Component value not changed
    * 8 : Component value changed but with no more precision / lastetat
    * 9 : Divers
  * log\_dest = 2 : logs destination : 0=txt, 1=sql, 2=txt+sql
  * logs\_nbparpage = 1000 : Number of logs/Value to view by page on the web interface
  * meteo\_codeville = FRXX1222 : Town code from weather.com (take it on weather.com Url)
  * meteo\_icone = 2 : Number of weather meteo icon (1 / 4)
  * meteo\_codevillereleve = LUXX0003 : Town code from weather.com for real time value

  * heure\_lever\_correction = 0 : Number of minute to add (+xx) or soustract (-xx) to sunrise hours
  * heure\_coucher\_correction = 0 : Number of minute to add (+xx) or soustract (-xx) to sunshine hours


# Macros #

We have two macro type : macro and timer
  * macro consists in conditions and actions list
  * timer consist in a cron time and action list (a macro could contain a cron condition but a timer contain only one cron condition and is used to execute actions at a precise time : ([CT#....](CT#.....md))

## Conditions : ##

  * CC#id#=<>#valeur ou CI#id#=<>#valeur : Test of one component
    * id : component id,
    * =<> : test : <, >, =, <=, >=, <>
    * valeur : value to test : 19.5, ON... (use dot "." and not ",")
    * CC : the macro will be executed on the component state change
    * CI : test one component value (CC#1#ON & CI#2#OFF : macro executes when 1 change to ON and if 2 are at OFF state but not if 2 passed to OFF)

  * CH#=<>#ss#mm#hh#jj#MMM#JJJ : test with date or time (not used to plan but to test a date/time when a macro will be executed)
    * =<> : signe de test : <, >, =, <=, >=, <>
    * ss : Secondes : 0-59 (**for any)
    * mm : Minute : 0-59 (** for any)
    * hh: Heure : 0-23 (**pfor any)
    * jj: Jour du mois : 1-31 (** for any)
    * MMM: Numero du mois : 1-12 (**for any)
    * JJJ : Numéro du jour de la semaine : 1-7 (** for any)
    * exemple : CH#=#0#0#0#1#**#** = si on est le 1er du mois et minuit
    * exemple : CH#>#0#0#22#**#**#**= Si il est plus que 22h (et < à minuit)
    * exemple : CH#=#**#**#22#**#**#1 = Si on est Lundi entre 22h00 et 22h59**


  * CT#=#ss#mm#hh#jj#MM#JJ : Timer (sert à programmer des actions)
    * ss: Seconde: 0-59 (**for any)
    * mm : Minute : 0-59 (** for any)
    * hh: Heure : 0-23 (**for any)
    * jj: Jour du mois : 1-31 (** for any)
    * MM: Numero du mois : 1-12 (**for any)
    * JJ : Numéro du jour de la semaine : 1-7 (** for any)
    * exemple : CT#=#**#0#0#1#**#**= tous les 1er du mois à minuit
    * exemple : CT#=#**#0#12#**#**#**= tous les jours à 12h00
    * exemple : CT#=#**#**#**#**#**#**= toutes les minutes
    * exemple : CT#=#10#5#2#**#**#2 = toutes les mardis à 2h05m10s
    * exemple : CT#=#**#**#/1#**#**#2 = toutes les mardis toutes les heures
    * exemple : CT#=#**#0#12#**#1-15#** = du premier au 15 du mois à midi
    * exemple : CT#=#**#0#0,9,15#**#**#** = tous les jours à 0h, 9h et 15h

## Actions: ##

  * AC#id#ordre#option1#option2 : Action on a component
    * id : component id
    * ordre : à envoyer au composant : ON
    * option1 : valeur à joindre (ex : PLCBUS : PRESET\_DIM 50 2)
    * option2 : valeur à joindre (ex : PLCBUS : PRESET\_DIM 50 2)

  * ME#id#etat : Modify component state
    * id : component ID
    * etat : Value to write (ON, 15.7...)

  * MA#id#adresse : Modify component adress
    * id : component ID
    * adresse: Adress to modify (2456, 452\_THE, M1...)

  * MM#id#conditions#actions : Modify condition/actions of a macro
    * id : macro ID
    * conditions: conditions to use
    * actions: actions to use

  * AL#texte : texte à loguer

  * AS#service : service à exécuter :
    * stop : arreter le service domos,
    * restart : redémarrer le service,
    * maj\_all : maj des tables,
    * maj\_composants : maj de la table composant,
    * maj\_composants\_bannis : maj de la table composants\_bannis,
    * maj\_macro : maj de la table macro,
    * maj\_timer : maj de la table timer,
    * afftables : log tables content

  * AM#macro\_id : lance la macro macro\_id (verif des conditions puis si OK actions)

  * AN#module#action : module à exécuter :
    * sql#optimise : optimise la base mysql pour réduire sa taille
    * sql#purgeglos : purges les logs plus vieux que 2 mois
    * sql#reconnect : deconnect/connect to the mysql database

# Menu #

You could personnalize the general menu.
Indeed you can had, remove and change order for plan.
Each item consist in 3 parameters :
  * nom : this is the name of the plan which appear in the menu
  * lien : this is the name used in the image menu "www\images\plans\menu\_xxx.png" and the plan name : "www\images\plans\xxx.jpg" which is used in the plan page to localise the captors.
  * ordre : this is the order oh the menu which could be change by clicking on "Changer l'ordre"

# List of possible Order : #

  * PLCBUS :
    * ON
    * OFF
    * ALL\_LIGHTS\_ON
    * ALL\_LIGHTS\_OFF
    * ALL\_UNITS\_OFF
    * All\_USER\_LIGHTS\_ON
    * All\_USER\_UNITS\_OFF
    * All\_USER\_LIGHTS\_OFF
    * DIM : light will dim until FADE\_STOP  is received / data1 = Fade rate
    * BRIGHT : light will bright until FADE\_STOP  is received / data1 = Fade rate
    * BLINK : data1=interval
    * FADE\_STOP
    * PRESET\_DIM data1=level data2=delay
    * STATUS\_ON
    * STATUS\_OFF
    * STATUS\_REQUEST
    * ReceiverMasterAddressSetup : data1=New user code, data2=new home+unitcode
    * TransmitterMasterAddressSetup : data1=New user code, data2=new home+unitcode
    * SceneAddressSetup
    * SceneAddressErase
    * AllSceneAddressErase
    * GetSignalStrength : data1=signal strength
    * GetNoiseStrength : data1=Noise strength
    * ReportSignalStrength
    * ReportNoiseStrength
    * GetAllIdPulse
    * GetOnlyOnIdPulse
    * ReportAllIdPulse3Phase
    * ReportOnlyOnIdPulse3Phase

  * RFX-COM:
    * MODEB32 : Init Mode: 32 bit
    * MODEVAR : Init Mode: Variable length mode
    * MODEKOP : Init Mode: 24 bit KOPPLA
    * MODEARC : Init Mode: Arc
    * MODEHS : Init Mode: RFXCOM-HS plugin mode
    * MODEVISONIC : Init Mode: Visonic only
    * MODENOXLAT : Init Mode: Visonic & Variable mode
    * MODEBD : Init Mode: Bd
    * MODEVISAUX : Init Mode: Visionic AUX
    * DISKOP : Disable Koppla
    * DISX10 : Disable X10
    * DISARC : Disable ARC
    * DISOREGON : Disable Oregon
    * DISATI : Disable ATI Wonder
    * DISHE : Disable HomeEasy
    * DISVIS : Disable Visonic
    * ENALL : Enable ALL RF
    * SWVERS : Version request to receiver

# Components #

## obligatory ##

  * Jour (address : jour, norme : virtual)
  * Jour2 (address : jour2, norme : virtual)

## Modèles ##




# Plans #