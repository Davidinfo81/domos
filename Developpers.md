# Programming #
The VB.net program is developed with Visual studio 2008 and use the framework 3.5.
The php and mysql interface are stored on WampServer 2.0i :
  * Apache2.2.14
  * mysql 5.4.41
  * php 5.3.1

# Google Code SVN #
For synchronising the visual studio solution, I uses AnkhSvn-2.1.8420.8.msi
For the mysql and php files, I use TortoiseSVN-1.6.10.19898-win32-svn-1.6.12.msi

# Windows Programs Needs #
For using Domos, you need to install:
  * dotnetfx35.exe
  * last JRE
  * VJredist

# Drivers #
Depending on the technology, you have to install :
  * 1-wire USB dongle : the last driver : install\_1\_wire\_drivers\_x64\_v402.msi
  * PLCBUS USB : the prolific PL2303 driver : _IO Cable\_PL-2303\_Drivers - Generic\_Windows\_allinone\_PL2303\_Prolific\_DriverInstaller\_v10518.zip
  * For the RFXCOM : the standard driver on the RFXCOM CD
  * X10 : not used at this time_

# PHP interface #
The website use different scripts:
  * JPGRAPH 3.0.6
  * dhtmlxGrid 2.5
  * wdweather.php
  * jQuery-1.3.2
  * Interface Elements for jQuery
  * Contextmenu for jQuery
  * XHRConnection

# VB.net program #
We use differents dlls :
  * OneWireAPI.NET.dll from Dallas 1-wire drivers
  * NCrontab.dll (http://code.google.com/p/ncrontab/)
  * MySql.Data.dll (from mysql-connector-net-5.2.5)