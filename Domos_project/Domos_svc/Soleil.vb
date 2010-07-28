Imports Microsoft.VisualBasic
Imports System.Math

Public Class Soleil

    ' -- The following properties are exposed:
    'Sunrise (r) - Sunrise time
    'Sunset (r) - Sunset time
    'SolarNoon (r) - Solar noon
    '
    'CityCount (r) - Number of cities
    'CityName (r) - Name of city, by index
    'City (w) - Sets the longitude/latitude/timezone based off a city
    ' name or city index
    '
    'TimeZone (r/w) - Current Timezone
    'DaySavings (r/w) - Daylight savings time in effect
    'Longitude (r/w) - Longitude to calculate for
    'Latitude (r/w) - Latitude to calculate for
    '
    'DateDay (r/w) - Date to calculate for
    '
    '
    ' -- The following method is exposed
    'CalculateSun - Calculate sunrise, sunset and solar noon

    Private Structure typeMonth 'Déclaration de type de variable
        Public Name As String 'Nom du mois
        Public NumDays As Long 'Nombre de jours dans ce mois
    End Structure

    Public Structure typeCity 'Déclaration de type de variable
        Public Continent As String 'Zone géographique associée à la ville
        Public Country As String 'Pays où la ville est localisée
        Public Name As String 'Nom de la ville
        Public Longitude As Double 'Longitude de la ville
        Public Latitude As Double 'Latitude de la ville
        Public TimeZone As Long 'Zone horaire de la ville
    End Structure

    Private m_cNumberCities As Long 'Nombre de villes enregistrées
    Private m_Cities() As typeCity 'Tableau des villes enregistrées

    Private m_monthList(0 To 11) As typeMonth 'Liste des mois
    Private m_monthLeap(0 To 11) As typeMonth 'Liste des mois pour année bissextiles

    Private m_nTimeZone As Long 'Numéro de la zone horaire sélectionnée
    Private m_bDaySavings As Boolean 'Gestion du changement d'heure sélectionnée
    Private m_nLongitude As Double 'Valeur de la longitude sélectionnée
    Private m_nLatitude As Double 'Valeur de la latitude sélectionnée
    Private m_dateSel As Date 'Date sélectionnée
    Private m_sContinent As String 'Nom de la zone géographique de la ville
    Private m_sCountry As String 'Nom du pays de la ville

    Private m_dateSunrise As Date 'Date du levé du soleil : jours/mois/année heures:minutes:secondes
    Private m_dateSunset As Date 'Date du couché du soleil : jours/mois/année heures:minutes:secondes
    Private m_dateNoon As Date 'Date du zénith du soleil : jours/mois/année heures:minutes:secondes

    Public ReadOnly Property Sunrise() As Date
        Get
            'Propriété de récupération de la date de levé du soleil
            Sunrise = m_dateSunrise
        End Get
    End Property

    Public ReadOnly Property Sunset() As Date
        Get
            'Propriété de récupération de la date de couché du soleil
            Sunset = m_dateSunset
        End Get
    End Property

    Public ReadOnly Property SolarNoon() As Date
        Get
            'Propriété de récupération de la date de zénith du soleil
            SolarNoon = m_dateNoon
        End Get
    End Property

    Public ReadOnly Property CityCount() As Long
        Get
            'Propriété de récupération du nombre de villes enregistrées
            CityCount = m_cNumberCities + 1
        End Get
    End Property

    Public ReadOnly Property CityName(ByVal nCity As Long) As String
        Get
            'Propriété de récupération du nom de la ville récupérée par index

            If nCity < 0 Or nCity > m_cNumberCities Then 'Contrôle de l'index dans le tableau
                CityName = "(Error)"
            Else
                CityName = m_Cities(nCity).Name 'Renvoi du nom de la ville
            End If
        End Get
    End Property

    'Public Sub City(ByVal name As String)
    '    Dim nCity As Long 'Index numérique de la ville
    '    Dim bFound As Boolean 'Variable de contrôle de la présence de la ville dans le tableau

    '    For nCity = 0 To m_cNumberCities 'Recherche du nom de la ville dans le tableau
    '        If Trim(LCase(name)) = Trim(LCase(m_Cities(nCity).Name)) Then
    '            bFound = True 'Ville trouvée
    '            Exit For 'Sortie de la boucle
    '        End If
    '    Next
    '    If bFound Then 'Gestion de la ville non trouvée
    '        Me.City(nCity)
    '    End If
    'End Sub

    'Public Sub City(ByVal value As Long)
    '    'Propriété de sélection de la ville par nom ou par index

    '    Dim nCity As Long 'Index numérique de la ville

    '    nCity = value 'Affectation du paramètre à la variable d'index

    '    If nCity < 0 Or nCity > m_cNumberCities Then 'Contrôle de l'index en dehors du tableau
    '        m_nTimeZone = 0 'Valeur par erreur
    '        m_bDaySavings = False 'Valeur par erreur
    '        m_nLongitude = 0 'Valeur par erreur
    '        m_nLatitude = 0 'Valeur par erreur
    '        m_sContinent = "" 'Valeur par erreur AJOUT
    '        m_sCountry = "" 'Valeur par erreur AJOUT
    '    Else
    '        m_nTimeZone = m_Cities(nCity).TimeZone 'Affectation de la valeur à la variable d'échange
    '        m_bDaySavings = True 'Affectation de la valeur à la variable d'échange
    '        m_nLongitude = m_Cities(nCity).Longitude 'Affectation de la valeur à la variable d'échange
    '        m_nLatitude = m_Cities(nCity).Latitude 'Affectation de la valeur à la variable d'échange
    '        m_sContinent = m_Cities(nCity).Continent 'Affectation de la valeur à la variable d'échange AJOUT
    '        m_sCountry = m_Cities(nCity).Country 'Affectation de la valeur à la variable d'échange AJOUT
    '    End If
    'End Sub

    Public Sub City_gps(ByVal lat As String, ByVal longit As String)
        'Propriété de sélection de la ville par nom ou par index
        m_nTimeZone = 1 'Affectation de la valeur à la variable d'échange
        m_bDaySavings = True 'Affectation de la valeur à la variable d'échange
        m_nLongitude = longit 'Affectation de la valeur à la variable d'échange
        m_nLatitude = lat 'Affectation de la valeur à la variable d'échange
        m_sContinent = "x" 'Affectation de la valeur à la variable d'échange AJOUT
        m_sCountry = "x" 'Affectation de la valeur à la variable d'échange AJOUT
    End Sub
    Public Property TimeZone() As Long
        Get
            'Propriété de récupération du fuseau horaire
            TimeZone = m_nTimeZone
        End Get
        Set(ByVal value As Long)
            'Propriété de sélection du fuseau horaire
            m_nTimeZone = value
        End Set
    End Property

    Public Property DaySavings() As Boolean
        Get
            'Propriété de récupération de la gestion du changement d'heure
            DaySavings = m_bDaySavings
        End Get
        Set(ByVal value As Boolean)
            'Propriété de sélection de la gestion du changement d'heure
            m_bDaySavings = value
        End Set
    End Property

    Public Property Longitude() As Double
        Get
            'Propriété de récupération de la longitude
            Longitude = m_nLongitude
        End Get
        Set(ByVal value As Double)
            'Propriété de sélection de la longitude
            m_nLongitude = value
        End Set
    End Property

    Public Property Latitude() As Double
        Get
            'Propriété de récupération de la latitude
            Latitude = m_nLatitude
        End Get
        Set(ByVal value As Double)
            'Propriété de sélection de la latitude
            m_nLatitude = value
        End Set
    End Property

    Public Property DateDay() As Date
        Get
            'Propriété de récupération de la dade
            DateDay = m_dateSel
        End Get
        Set(ByVal value As Date)
            'Propriété de sélection de la date
            m_dateSel = value
        End Set
    End Property

    Public Property Continent() As String
        Get
            'Propriété de récupération de la zone géographique
            Continent = m_sContinent
        End Get
        Set(ByVal value As String)
            'Propriété de sélection de la zone géographique
            m_sContinent = value
        End Set
    End Property

    Public Property Country() As String
        Get
            'Propriété de récupération du pays
            Country = m_sCountry
        End Get
        Set(ByVal value As String)
            'Propriété de sélection du pays
            m_sCountry = value
        End Set
    End Property

    Public ReadOnly Property TabCities() As typeCity()
        Get
            'Propriété de récupération du tableau des villes


            Dim TabTemp() As typeCity 'Tableau temporaire des villes
            Dim i As Integer 'Indice de boucle

            ReDim TabTemp(0) 'Dimensionnement du tableau

            For i = 0 To m_cNumberCities 'Boucle de remplissage du tableau
                TabTemp(UBound(TabTemp)).Continent = m_Cities(i).Continent
                TabTemp(UBound(TabTemp)).Country = m_Cities(i).Country
                TabTemp(UBound(TabTemp)).Name = m_Cities(i).Name
                TabTemp(UBound(TabTemp)).Latitude = m_Cities(i).Latitude
                TabTemp(UBound(TabTemp)).Longitude = m_Cities(i).Longitude
                TabTemp(UBound(TabTemp)).TimeZone = m_Cities(i).TimeZone

                If i < m_cNumberCities Then 'Contrôle de l'indice de boucle
                    ReDim Preserve TabTemp(UBound(TabTemp) + 1) 'Augmentation de taille du tableau
                End If

            Next i

            TabCities = TabTemp 'Renvoi du tableau des villes enregistrées
        End Get
    End Property

    Private Function IsLeapYear(ByVal nYear As Long) As Boolean
        'Fonction de contrôle de du type d'année : bissextile ou non

        If (nYear Mod 4 = 0 And nYear Mod 100 <> 0) Or nYear Mod 400 = 0 Then
            IsLeapYear = True 'Année bissextile
        Else
            IsLeapYear = False 'Année non bissextile
        End If
        'Remarque: Conditions suffisantes pour que l'année soit bissextile
        ' - année divisible par 4 mais pas par 100 (ex: année 1998 mais pas année 1900)
        ' - année divisible par 400 (ex: année 2000)
    End Function

    Private Function RadToDeg(ByVal angleRad As Double) As Double
        'Fonction de conversion d'angle en radian en angle en degré
        RadToDeg = 180 * angleRad / 3.1415926535
    End Function

    Private Function DegToRad(ByVal angleDeg As Double) As Double
        'Fonction de conversion d'angle en degré en angle en radian
        DegToRad = 3.1415926535 * angleDeg / 180
    End Function

    Private Function CalcJulianDay(ByVal nMonth As Long, ByVal nDay As Long, ByVal bLeapYear _
    As Boolean) As Long
        'Fonction de calcul du jour dans le calendrier julien

        Dim i As Long 'Indice de boucle
        Dim nJulianDay As Long 'Numéro de jour dans le calendrier julien


        If bLeapYear Then 'Traitement du cas de l'année bissextile
            For i = 0 To nMonth - 1 'Boucle sur les mois précédents
                nJulianDay = nJulianDay + m_monthLeap(i).NumDays 'Additions du nombre de jour pour chaque mois
            Next
        Else 'Traitement de l'année non bissextile
            For i = 0 To nMonth - 1 'Boucle sur les mois précédents
                nJulianDay = nJulianDay + m_monthList(i).NumDays 'Additions du nombre de jour pour chaque mois
            Next
        End If

        nJulianDay = nJulianDay + nDay 'Ajout du nombre de jour du mois en cours

        CalcJulianDay = nJulianDay 'Renvoi du numéro du jour dans le calendrier julien
    End Function

    Private Function CalcGamma(ByVal nJulianDay As Long) As Double
        'Fonction de calcul de gamma ("fraction d'année") en fonction du jour en calendrier julien
        CalcGamma = (2 * 3.1415926535 / 365) * (nJulianDay - 1)
    End Function

    Private Function CalcGamma2(ByVal nJulianDay As Long, ByVal hour As Long) As Double
        'Fonction de calcul de gamma ("fraction d'année") en fonction du jour en calendrier julien et de l'heure
        CalcGamma2 = (2 * 3.1415926535 / 365) * (nJulianDay - 1 + (hour / 24))
    End Function

    Private Function CalcEqOfTime(ByVal gamma As Double) As Double
        'Fonction de calcul du décalage horaire du zénith du soleil
        CalcEqOfTime = (229.18 * (0.000075 + 0.001568 * Cos(gamma) - 0.032077 * Sin(gamma) - 0.014615 * Cos(2 * gamma) - 0.040849 * Sin(2 * gamma)))
    End Function

    Private Function CalcSolarDec(ByVal gamma As Double) As Double
        'Fonction de calcul de la déclinaison du soleil
        CalcSolarDec = (0.006918 - 0.399912 * Cos(gamma) + 0.070257 * Sin(gamma) - 0.006758 * Cos(2 * gamma) + 0.000907 * Sin(2 * gamma))
    End Function

    Private Function acos(ByVal x As Double) As Double
        'Fonction de calcul de l'arccosinus
        On Error Resume Next
        acos = Atan(-x / Sqrt(-x * x + 1)) + 2 * Atan(1)
    End Function

    Private Function CalcHourAngle(ByVal lat As Double, ByVal solarDec As Double, ByVal time _
    As Boolean) As Double
        'Fonction de calcul de l'angle horaire du soleil
        Dim latRad As Double
        latRad = DegToRad(lat) 'Définition de la latitude en degré
        If time Then 'Cas général de calcul de l'angle horaire
            CalcHourAngle = (acos(Cos(DegToRad(90.833)) / (Cos(latRad) * Cos(solarDec)) - Tan(latRad) * Tan(solarDec)))
        Else 'Cas de calcul pour l'angle horaire en GMT
            CalcHourAngle = -(acos(Cos(DegToRad(90.833)) / (Cos(latRad) * Cos(solarDec)) - Tan(latRad) * Tan(solarDec)))
        End If
    End Function

    Private Function CalcDayLength(ByVal hourAngle As Double) As Double
        'Fonction de calcul de la durée du jour
        CalcDayLength = (2 * Abs(RadToDeg(hourAngle))) / 15
    End Function

    Public Sub CalculateSun()
        'Fonction principale de la classe
        Dim nLatitude As Double 'Latitude
        Dim nLongitude As Double 'Longitude
        Dim dateCalc As Date 'Date de calcul
        Dim bDaySavings As Long 'Gestion du changement d'heure
        Dim nZone As Long 'Zone horaire
        Dim nJulianDay As Long 'Date en calendrier julien
        Dim gamma_solnoon As Double '"Fraction annuelle" du zénith
        Dim eqTime As Double 'Décalage horaire du zénith
        Dim solarDec As Double 'Déclinaison du soleil
        Dim timeGMT As Double 'Heure GMT du levé
        Dim solNoonGmt As Double 'Heure GMT du zénith
        Dim timeLST As Double 'Heure locale
        Dim solNoonLST As Double 'Heure locale du zénith
        Dim setTimeGMT As Double 'Heure GMT du couché
        Dim setTimeLST As Double 'Heure locale du couché
        Dim oTZ As TimeZone
        nLatitude = m_nLatitude 'Récupération de la latitude de calcul
        nLongitude = m_nLongitude 'Récupération de la longitude de calcul
        dateCalc = m_dateSel 'Récupération de la date de calcul
        If m_bDaySavings Then 'Traitement du changement d'heure : 60 minutes si changement peut-être à affiner
            oTZ = System.TimeZone.CurrentTimeZone
            If oTZ.IsDaylightSavingTime(m_dateSel) Then
                m_bDaySavings = 60
            Else
                m_bDaySavings = 0
            End If
            bDaySavings = IIf(m_bDaySavings, 60, 0)
        Else
            bDaySavings = 0
        End If
        nZone = m_nTimeZone 'Zone horaire
        If nLatitude >= -90 And nLatitude < -89.8 Then 'Définition de la borne négative de la latitude
            nLatitude = -89.8
        End If
        If nLatitude <= 90 And nLatitude > 89.8 Then 'Définition de la borne positive de la latitude
            nLatitude = 89.8
        End If
        '***** Calculate the time of sunrise
        'Calcul de l'heure de levé du soleil
        nJulianDay = CalcJulianDay(Month(dateCalc), Day(dateCalc), IsLeapYear(Year(dateCalc))) 'Calcul de la date dans le calendrier julien
        gamma_solnoon = CalcGamma2(nJulianDay, 12 + (nLongitude / 15)) 'Calcul de la "fraction d'année" de la date en calendrier julien
        eqTime = CalcEqOfTime(gamma_solnoon) 'Calcul du décalage horaire du zénith
        solarDec = CalcSolarDec(gamma_solnoon) 'Calcul de la déclinaison
        timeGMT = CalcSunriseGMT(DatePart("y", dateCalc), nLatitude, nLongitude) 'Calcul de l'heure GMT de levé du soleil
        timeLST = timeGMT + (60 * nZone) + bDaySavings 'Calcul de l'heure locale de levé du soleil
        m_dateSunrise = m_dateSel.Date.AddSeconds(timeLST * 60) 'Définition de la date locale de levé du soleil
        '***** Calculate Solar noon
        'Calcul de l'heure de zénith du soleil
        solNoonGmt = CalcSolNoonGMT(DatePart("y", dateCalc), nLongitude) 'Calcul de l'heure GMT du zénith
        solNoonLST = solNoonGmt - (60 * nZone) + bDaySavings 'Calcul de l'heure locale du zénith
        m_dateNoon = m_dateSel.Date.AddSeconds(solNoonLST * 60) 'Définition de la date locale du zénith
        '***** Calculate the time of sunset
        'Calcul de l'heure de couché du soleil
        setTimeGMT = CalcSunsetGMT(DatePart("y", dateCalc), nLatitude, nLongitude) 'Calcul de l'heure GMT du couché
        setTimeLST = setTimeGMT + (60 * nZone) + bDaySavings 'Calcul de l'heure locale du couché
        m_dateSunset = m_dateSel.Date.AddSeconds(setTimeLST * 60) 'Définition de la date locale du couch
    End Sub

    Private Function CalcSunriseGMT(ByVal nJulianDay As Long, ByVal nLatitude As Double, ByVal nLongitude As Double) As Double
        'Fonction de calcul l'heure de levé du soleil en GMT
        Dim gamma As Double '"Fraction annuelle" du jour julien
        Dim eqTime As Double 'Décalage horaire du jour
        Dim solarDec As Double 'Déclinaison du jour
        Dim hourAngle As Double 'Angle horaire du jour
        Dim delta As Double 'Longitude corrigée
        Dim timeDiff As Double '???
        Dim timeGMT As Double 'Heure GMT du levé et variable intermédiaire
        Dim gamma_sunrise As Double '"Fraction annuelle" du levé
        gamma = CalcGamma(nJulianDay) 'Calcul de la "fraction annuelle"
        eqTime = CalcEqOfTime(gamma) 'Calcul décalage horaire
        solarDec = CalcSolarDec(gamma) 'Calcul de la déclinaison
        hourAngle = CalcHourAngle(nLatitude, solarDec, True) 'Calcul de l'angle horaire
        delta = nLongitude - RadToDeg(hourAngle) 'Calcul de la longitude corrigée
        timeDiff = 4 * delta '???
        timeGMT = 720 + timeDiff - eqTime 'Calcul de l'heure GMT
        gamma_sunrise = CalcGamma2(nJulianDay, timeGMT / 60) 'Calcul de la fraction annuelle pour le levé
        eqTime = CalcEqOfTime(gamma_sunrise) 'Calcul du décalage horaire pour le levé
        solarDec = CalcSolarDec(gamma_sunrise) 'Calcul de la déclinaison pour le levé
        hourAngle = CalcHourAngle(nLatitude, solarDec, True) 'Calcul de l'angle horaire pour le levé
        delta = nLongitude - RadToDeg(hourAngle) 'Calcul de la longitude corrigée pour le levé
        timeDiff = 4 * delta '???
        timeGMT = 720 + timeDiff - eqTime 'Calcul de l'heure GMT du levé
        CalcSunriseGMT = timeGMT 'Renvoi de l'heure GMT du levé de soleil
    End Function

    Private Function CalcSolNoonGMT(ByVal nJulianDay As Long, ByVal nLongitude As Double) As Double
        'Fonction de calcul de l'heure de zénith du soleil en GMT
        Dim gamma_solnoon As Double '"Fraction annuelle" du zénith
        Dim eqTime As Double 'Décalage horaire au zénith
        Dim solarNoonDec As Double 'Déclinaison
        Dim solNoonGmt As Double 'Heure GMT du zénith
        gamma_solnoon = CalcGamma2(nJulianDay, 12 + (nLongitude / 15)) 'Calcul de la "fraction annuelle"
        eqTime = CalcEqOfTime(gamma_solnoon) 'Calcul du décalage horaire du zénith
        solarNoonDec = CalcSolarDec(gamma_solnoon) 'Calcul de la déclinaison
        solNoonGmt = 720 + (nLongitude * 4) - eqTime 'Calcul de l'heure GMT du zénith
        CalcSolNoonGMT = solNoonGmt 'Renvoi de l'heure GMT du zénith
    End Function

    Private Function CalcSunsetGMT(ByVal nJulianDay As Long, ByVal nLatitude As Double, ByVal nLongitude As Double) As Double
        'Fonction de calcul de l'heure de couché du soleil en GMT
        Dim gamma As Double '"Fraction annuelle" du jour julien suivant
        Dim eqTime As Double 'Décalage horaire du jour
        Dim solarDec As Double 'Déclinaison du jour
        Dim hourAngle As Double 'Angle horaire du jour
        Dim delta As Double 'Longitude corrigée
        Dim timeDiff As Double '???
        Dim setTimeGMT As Double 'Heure GMT du couché et variable intermédiare
        Dim gamma_sunset As Double '"Fraction annuelle" du couché
        gamma = CalcGamma(nJulianDay + 1) 'Calcul de la "fraction annuelle"
        eqTime = CalcEqOfTime(gamma) 'Calcul décalage horaire
        solarDec = CalcSolarDec(gamma) 'Calcul de la déclinaison
        hourAngle = CalcHourAngle(nLatitude, solarDec, False) 'Calcul de l'angle horaire
        delta = nLongitude - RadToDeg(hourAngle) 'Calcul de la longitude corrigée
        timeDiff = 4 * delta '???
        setTimeGMT = 720 + timeDiff - eqTime 'Calcul de l'heure GMT
        gamma_sunset = CalcGamma2(nJulianDay, setTimeGMT / 60) 'Calcul de la "fraction annuelle" pour le couché
        eqTime = CalcEqOfTime(gamma_sunset) 'Calcul décalage horaire pour le couché
        solarDec = CalcSolarDec(gamma_sunset) 'Calcul de la déclinaison pour le couché
        hourAngle = CalcHourAngle(nLatitude, solarDec, False) 'Calcul de l'angle horaire pour le couché
        delta = nLongitude - RadToDeg(hourAngle) 'Calcul de la longitude corrigée pour le couché
        timeDiff = 4 * delta '???
        setTimeGMT = 720 + timeDiff - eqTime 'Calcul de l'heure GMT du couché
        CalcSunsetGMT = setTimeGMT 'Renvoi de l'heure GMT du couché
    End Function

    Private Sub InitMonths()
        'Procédure d'initialisation des mois et de leurs nombres de jours
        'Mois année non bissextile
        m_monthList(0).Name = "January" : m_monthList(0).NumDays = 31
        m_monthList(1).Name = "February" : m_monthList(1).NumDays = 28
        m_monthList(2).Name = "March" : m_monthList(2).NumDays = 31
        m_monthList(3).Name = "April" : m_monthList(3).NumDays = 30
        m_monthList(4).Name = "May" : m_monthList(4).NumDays = 31
        m_monthList(5).Name = "June" : m_monthList(5).NumDays = 30
        m_monthList(6).Name = "July" : m_monthList(6).NumDays = 31
        m_monthList(7).Name = "August" : m_monthList(7).NumDays = 31
        m_monthList(8).Name = "September" : m_monthList(8).NumDays = 30
        m_monthList(9).Name = "October" : m_monthList(9).NumDays = 31
        m_monthList(10).Name = "November" : m_monthList(10).NumDays = 30
        m_monthList(11).Name = "DEcember" : m_monthList(11).NumDays = 31
        'Mois année bissextile
        m_monthLeap(0).Name = "January" : m_monthLeap(0).NumDays = 31
        m_monthLeap(1).Name = "February" : m_monthLeap(1).NumDays = 29
        m_monthLeap(2).Name = "March" : m_monthLeap(2).NumDays = 31
        m_monthLeap(3).Name = "April" : m_monthLeap(3).NumDays = 30
        m_monthLeap(4).Name = "May" : m_monthLeap(4).NumDays = 31
        m_monthLeap(5).Name = "June" : m_monthLeap(5).NumDays = 30
        m_monthLeap(6).Name = "July" : m_monthLeap(6).NumDays = 31
        m_monthLeap(7).Name = "August" : m_monthLeap(7).NumDays = 31
        m_monthLeap(8).Name = "September" : m_monthLeap(8).NumDays = 30
        m_monthLeap(9).Name = "October" : m_monthLeap(9).NumDays = 31
        m_monthLeap(10).Name = "November" : m_monthLeap(10).NumDays = 30
        m_monthLeap(11).Name = "DEcember" : m_monthLeap(11).NumDays = 31
    End Sub

    Private Sub AddCity(ByVal sContinent As String, ByVal sCountry As String, ByVal sCity As String, ByVal nLatitude As Double, ByVal nLongitude As _
    Double, ByVal nZone As Long)
        'Procédure d'ajout d'une ville dans le tableau de référence
        m_cNumberCities = m_cNumberCities + 1 'Augmente le nombre de villes
        If m_cNumberCities > UBound(m_Cities) Then 'Contrôle et redimensionne le tableau des villes si besoin
            ReDim Preserve m_Cities(UBound(m_Cities) + 10)
        End If
        m_Cities(m_cNumberCities).Continent = sContinent 'Définition du continent
        m_Cities(m_cNumberCities).Country = sCountry 'Définition du pays
        m_Cities(m_cNumberCities).Name = sCity 'Définition du nom de la ville
        m_Cities(m_cNumberCities).Latitude = nLatitude 'Définition de la latitude
        m_Cities(m_cNumberCities).Longitude = nLongitude 'Définition de la longitude
        m_Cities(m_cNumberCities).TimeZone = nZone 'Définition de la zone horaire
    End Sub

    Private Sub InitCities()
        'Procédure d'initialisation du tableau des villes de référence
        m_cNumberCities = -1
        ReDim m_Cities(0)
        AddCity("Europe", "France", "Metz", 49.1166, -6.1833, 1)
        AddCity("Europe", "France", "Algrange", 49.3637, -6.0529, 1)
        AddCity("Europe", "France", "Paris", 48.8625, -2.3523, 1)
        AddCity("Europe", "France", "Gap", 44.5651, -6.0743, 1)
    End Sub

    Public Sub New()
        'Procédure d'initialisation de la classe
        InitMonths() 'Initialisation des mois
        InitCities() 'Initialisation des villes
        m_dateSel = Now 'Initialisation de la date à celle du jour
    End Sub
End Class
