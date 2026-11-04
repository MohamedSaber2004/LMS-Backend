/* ============================================================================
   Country time zone anchor — data hand-off for §1.2 Scheduling
   Date      : 2026-11-04
   Target    : shared SQL Server (Development and Production point at the same DB)
   Run AFTER : docs/plans/2026-11-04-scheduling.sql (adds Countries.TimezoneId)

   The availability generator derives every instructor's time zone from their own
   country row (LMS.Application/Common/Helpers/CountryTimezoneResolver.cs): the
   profile CountryId first, the phone dial code as fallback. Nothing is typed per
   user, so the country table has to carry the anchor. A country with no TimezoneId
   means its instructors are skipped with a logged warning.

   For countries spanning several zones the capital / most populous zone is used.
   Deliberately left empty, because one dial code cannot disambiguate the zone:
   +1 (US/Canada/Caribbean), +7 (Russia/Kazakhstan), +52 (Mexico), +55 (Brazil),
   +61 (Australia), +62 (Indonesia), +590/+599 (multi-island Netherlands),
   +689/+691/+692 (Pacific micro-states). Those can still be set in the Countries
   screen; the value is read on every job run, so no deploy is needed.

   Idempotent: only blank rows are touched, admin edits are never overwritten.
   ========================================================================== */

UPDATE [Countries]
SET [TimezoneId] = CASE LTRIM(RTRIM([Code]), '+')
    WHEN '20'  THEN 'Africa/Cairo'            -- Egypt
    WHEN '211' THEN 'Africa/Juba'             -- South Sudan
    WHEN '212' THEN 'Africa/Casablanca'       -- Morocco
    WHEN '213' THEN 'Africa/Algiers'          -- Algeria
    WHEN '216' THEN 'Africa/Tunis'            -- Tunisia
    WHEN '218' THEN 'Africa/Tripoli'          -- Libya
    WHEN '220' THEN 'Africa/Banjul'           -- Gambia
    WHEN '221' THEN 'Africa/Dakar'            -- Senegal
    WHEN '222' THEN 'Africa/Nouakchott'       -- Mauritania
    WHEN '223' THEN 'Africa/Bamako'           -- Mali
    WHEN '224' THEN 'Africa/Conakry'          -- Guinea
    WHEN '225' THEN 'Africa/Abidjan'          -- Cote d'Ivoire
    WHEN '226' THEN 'Africa/Ouagadougou'      -- Burkina Faso
    WHEN '227' THEN 'Africa/Niamey'           -- Niger
    WHEN '228' THEN 'Africa/Lome'             -- Togo
    WHEN '229' THEN 'Africa/Porto-Novo'       -- Benin
    WHEN '230' THEN 'Indian/Mauritius'        -- Mauritius
    WHEN '231' THEN 'Africa/Monrovia'         -- Liberia
    WHEN '232' THEN 'Africa/Freetown'         -- Sierra Leone
    WHEN '233' THEN 'Africa/Accra'            -- Ghana
    WHEN '234' THEN 'Africa/Lagos'            -- Nigeria
    WHEN '235' THEN 'Africa/Ndjamena'         -- Chad
    WHEN '236' THEN 'Africa/Bangui'           -- Central African Republic
    WHEN '237' THEN 'Africa/Douala'           -- Cameroon
    WHEN '238' THEN 'Atlantic/Cape_Verde'     -- Cabo Verde
    WHEN '240' THEN 'Africa/Malabo'           -- Equatorial Guinea
    WHEN '241' THEN 'Africa/Libreville'       -- Gabon
    WHEN '242' THEN 'Africa/Brazzaville'      -- Congo
    WHEN '243' THEN 'Africa/Kinshasa'         -- DR Congo (west)
    WHEN '244' THEN 'Africa/Luanda'           -- Angola
    WHEN '245' THEN 'Africa/Bissau'           -- Guinea-Bissau
    WHEN '246' THEN 'Indian/Chagos'           -- British Indian Ocean Territory
    WHEN '248' THEN 'Indian/Mahe'             -- Seychelles
    WHEN '249' THEN 'Africa/Khartoum'         -- Sudan
    WHEN '250' THEN 'Africa/Kigali'           -- Rwanda
    WHEN '251' THEN 'Africa/Addis_Ababa'      -- Ethiopia
    WHEN '252' THEN 'Africa/Mogadishu'        -- Somalia
    WHEN '253' THEN 'Africa/Djibouti'         -- Djibouti
    WHEN '254' THEN 'Africa/Nairobi'          -- Kenya
    WHEN '255' THEN 'Africa/Dar_es_Salaam'    -- Tanzania
    WHEN '256' THEN 'Africa/Kampala'          -- Uganda
    WHEN '257' THEN 'Africa/Bujumbura'        -- Burundi
    WHEN '258' THEN 'Africa/Maputo'           -- Mozambique
    WHEN '259' THEN 'Indian/Comoro'           -- Comoros
    WHEN '260' THEN 'Africa/Lusaka'           -- Zambia
    WHEN '261' THEN 'Indian/Antananarivo'     -- Madagascar
    WHEN '262' THEN 'Indian/Reunion'          -- Reunion
    WHEN '263' THEN 'Africa/Harare'           -- Zimbabwe
    WHEN '264' THEN 'Africa/Windhoek'         -- Namibia
    WHEN '265' THEN 'Africa/Blantyre'         -- Malawi
    WHEN '266' THEN 'Africa/Maseru'           -- Lesotho
    WHEN '267' THEN 'Africa/Gaborone'         -- Botswana
    WHEN '268' THEN 'Africa/Mbabane'          -- Eswatini
    WHEN '269' THEN 'Indian/Mayotte'          -- Mayotte
    WHEN '27'  THEN 'Africa/Johannesburg'     -- South Africa
    WHEN '290' THEN 'Atlantic/St_Helena'      -- Saint Helena
    WHEN '291' THEN 'Africa/Asmara'           -- Eritrea
    WHEN '297' THEN 'America/Aruba'           -- Aruba
    WHEN '298' THEN 'Atlantic/Faroe'          -- Faroe Islands
    WHEN '299' THEN 'America/Nuuk'            -- Greenland
    WHEN '30'  THEN 'Europe/Athens'           -- Greece
    WHEN '31'  THEN 'Europe/Amsterdam'        -- Netherlands
    WHEN '32'  THEN 'Europe/Brussels'         -- Belgium
    WHEN '33'  THEN 'Europe/Paris'            -- France
    WHEN '34'  THEN 'Europe/Madrid'           -- Spain
    WHEN '350' THEN 'Europe/Gibraltar'        -- Gibraltar
    WHEN '351' THEN 'Europe/Lisbon'           -- Portugal (mainland)
    WHEN '352' THEN 'Europe/Luxembourg'       -- Luxembourg
    WHEN '353' THEN 'Europe/Dublin'           -- Ireland
    WHEN '354' THEN 'Atlantic/Reykjavik'      -- Iceland
    WHEN '355' THEN 'Europe/Tirane'           -- Albania
    WHEN '356' THEN 'Europe/Malta'            -- Malta
    WHEN '357' THEN 'Asia/Nicosia'            -- Cyprus
    WHEN '358' THEN 'Europe/Helsinki'         -- Finland
    WHEN '359' THEN 'Europe/Sofia'            -- Bulgaria
    WHEN '36'  THEN 'Europe/Budapest'         -- Hungary
    WHEN '370' THEN 'Europe/Vilnius'          -- Lithuania
    WHEN '371' THEN 'Europe/Riga'             -- Latvia
    WHEN '372' THEN 'Europe/Tallinn'          -- Estonia
    WHEN '373' THEN 'Europe/Chisinau'         -- Moldova
    WHEN '374' THEN 'Asia/Yerevan'            -- Armenia
    WHEN '375' THEN 'Europe/Minsk'            -- Belarus
    WHEN '376' THEN 'Europe/Andorra'          -- Andorra
    WHEN '377' THEN 'Europe/Monaco'           -- Monaco
    WHEN '378' THEN 'Europe/San_Marino'       -- San Marino
    WHEN '379' THEN 'Europe/Vatican'          -- Vatican City
    WHEN '380' THEN 'Europe/Kyiv'             -- Ukraine
    WHEN '381' THEN 'Europe/Belgrade'         -- Serbia
    WHEN '382' THEN 'Europe/Podgorica'        -- Montenegro
    WHEN '383' THEN 'Europe/Belgrade'         -- Kosovo
    WHEN '385' THEN 'Europe/Zagreb'           -- Croatia
    WHEN '386' THEN 'Europe/Ljubljana'        -- Slovenia
    WHEN '387' THEN 'Europe/Sarajevo'         -- Bosnia and Herzegovina
    WHEN '389' THEN 'Europe/Skopje'           -- North Macedonia
    WHEN '39'  THEN 'Europe/Rome'             -- Italy
    WHEN '40'  THEN 'Europe/Bucharest'        -- Romania
    WHEN '41'  THEN 'Europe/Zurich'           -- Switzerland
    WHEN '420' THEN 'Europe/Prague'           -- Czechia
    WHEN '421' THEN 'Europe/Bratislava'       -- Slovakia
    WHEN '423' THEN 'Europe/Vaduz'            -- Liechtenstein
    WHEN '43'  THEN 'Europe/Vienna'           -- Austria
    WHEN '44'  THEN 'Europe/London'           -- United Kingdom
    WHEN '45'  THEN 'Europe/Copenhagen'       -- Denmark
    WHEN '46'  THEN 'Europe/Stockholm'        -- Sweden
    WHEN '47'  THEN 'Europe/Oslo'             -- Norway
    WHEN '48'  THEN 'Europe/Warsaw'           -- Poland
    WHEN '49'  THEN 'Europe/Berlin'           -- Germany
    WHEN '500' THEN 'Atlantic/Stanley'        -- Falkland Islands
    WHEN '501' THEN 'America/Belize'          -- Belize
    WHEN '502' THEN 'America/Guatemala'       -- Guatemala
    WHEN '503' THEN 'America/El_Salvador'     -- El Salvador
    WHEN '504' THEN 'America/Tegucigalpa'     -- Honduras
    WHEN '505' THEN 'America/Managua'         -- Nicaragua
    WHEN '506' THEN 'America/Costa_Rica'      -- Costa Rica
    WHEN '507' THEN 'America/Panama'          -- Panama
    WHEN '508' THEN 'America/Miquelon'        -- Saint Pierre and Miquelon
    WHEN '509' THEN 'America/Port-au-Prince'  -- Haiti
    WHEN '51'  THEN 'America/Lima'            -- Peru
    WHEN '53'  THEN 'America/Havana'          -- Cuba
    WHEN '54'  THEN 'America/Buenos_Aires'    -- Argentina
    WHEN '56'  THEN 'America/Santiago'        -- Chile
    WHEN '57'  THEN 'America/Bogota'          -- Colombia
    WHEN '58'  THEN 'America/Caracas'         -- Venezuela
    WHEN '591' THEN 'America/La_Paz'          -- Bolivia
    WHEN '592' THEN 'America/Guyana'          -- Guyana
    WHEN '593' THEN 'America/Guayaquil'       -- Ecuador
    WHEN '594' THEN 'America/Cayenne'         -- French Guiana
    WHEN '595' THEN 'America/Asuncion'        -- Paraguay
    WHEN '596' THEN 'America/Martinique'      -- Martinique
    WHEN '597' THEN 'America/Paramaribo'      -- Suriname
    WHEN '598' THEN 'America/Montevideo'      -- Uruguay
    WHEN '60'  THEN 'Asia/Kuala_Lumpur'       -- Malaysia
    WHEN '63'  THEN 'Asia/Manila'             -- Philippines
    WHEN '64'  THEN 'Pacific/Auckland'        -- New Zealand
    WHEN '65'  THEN 'Asia/Singapore'          -- Singapore
    WHEN '66'  THEN 'Asia/Bangkok'            -- Thailand
    WHEN '670' THEN 'Asia/Dili'               -- Timor-Leste
    WHEN '673' THEN 'Asia/Brunei'             -- Brunei
    WHEN '675' THEN 'Pacific/Port_Moresby'    -- Papua New Guinea
    WHEN '676' THEN 'Pacific/Tongatapu'       -- Tonga
    WHEN '677' THEN 'Pacific/Guadalcanal'     -- Solomon Islands
    WHEN '678' THEN 'Pacific/Efate'           -- Vanuatu
    WHEN '679' THEN 'Pacific/Fiji'            -- Fiji
    WHEN '680' THEN 'Pacific/Palau'           -- Palau
    WHEN '685' THEN 'Pacific/Apia'            -- Samoa
    WHEN '686' THEN 'Pacific/Tarawa'          -- Kiribati (Gilbert Islands)
    WHEN '687' THEN 'Pacific/Noumea'          -- New Caledonia
    WHEN '688' THEN 'Pacific/Funafuti'        -- Tuvalu
    WHEN '690' THEN 'Pacific/Fakaofo'         -- Tokelau
    WHEN '81'  THEN 'Asia/Tokyo'              -- Japan
    WHEN '82'  THEN 'Asia/Seoul'              -- South Korea
    WHEN '84'  THEN 'Asia/Ho_Chi_Minh'        -- Vietnam
    WHEN '850' THEN 'Asia/Pyongyang'          -- North Korea
    WHEN '852' THEN 'Asia/Hong_Kong'          -- Hong Kong
    WHEN '853' THEN 'Asia/Macau'              -- Macau
    WHEN '855' THEN 'Asia/Phnom_Penh'         -- Cambodia
    WHEN '856' THEN 'Asia/Vientiane'          -- Laos
    WHEN '86'  THEN 'Asia/Shanghai'           -- China (single official zone)
    WHEN '880' THEN 'Asia/Dhaka'              -- Bangladesh
    WHEN '886' THEN 'Asia/Taipei'             -- Taiwan
    WHEN '90'  THEN 'Europe/Istanbul'         -- Turkiye
    WHEN '92'  THEN 'Asia/Karachi'            -- Pakistan
    WHEN '93'  THEN 'Asia/Kabul'              -- Afghanistan
    WHEN '94'  THEN 'Asia/Colombo'            -- Sri Lanka
    WHEN '95'  THEN 'Asia/Yangon'             -- Myanmar
    WHEN '960' THEN 'Indian/Maldives'         -- Maldives
    WHEN '961' THEN 'Asia/Beirut'             -- Lebanon
    WHEN '962' THEN 'Asia/Amman'              -- Jordan
    WHEN '963' THEN 'Asia/Damascus'           -- Syria
    WHEN '964' THEN 'Asia/Baghdad'            -- Iraq
    WHEN '965' THEN 'Asia/Kuwait'             -- Kuwait
    WHEN '966' THEN 'Asia/Riyadh'             -- Saudi Arabia
    WHEN '967' THEN 'Asia/Aden'               -- Yemen
    WHEN '968' THEN 'Asia/Muscat'             -- Oman
    WHEN '970' THEN 'Asia/Hebron'             -- Palestine
    WHEN '971' THEN 'Asia/Dubai'              -- United Arab Emirates
    WHEN '972' THEN 'Asia/Jerusalem'          -- Israel
    WHEN '973' THEN 'Asia/Bahrain'            -- Bahrain
    WHEN '974' THEN 'Asia/Qatar'              -- Qatar
    WHEN '975' THEN 'Asia/Thimphu'            -- Bhutan
    WHEN '976' THEN 'Asia/Ulaanbaatar'        -- Mongolia
    WHEN '977' THEN 'Asia/Kathmandu'          -- Nepal
    WHEN '992' THEN 'Asia/Dushanbe'           -- Tajikistan
    WHEN '993' THEN 'Asia/Ashgabat'           -- Turkmenistan
    WHEN '994' THEN 'Asia/Baku'               -- Azerbaijan
    WHEN '995' THEN 'Asia/Tbilisi'            -- Georgia
    WHEN '996' THEN 'Asia/Bishkek'            -- Kyrgyzstan
    WHEN '998' THEN 'Asia/Tashkent'           -- Uzbekistan
    ELSE NULL
END
WHERE ([TimezoneId] IS NULL OR LTRIM(RTRIM([TimezoneId])) = '')
  AND LTRIM(RTRIM([Code]), '+') IN (
    '20','211','212','213','216','218','220','221','222','223','224','225','226','227','228','229',
    '230','231','232','233','234','235','236','237','238','240','241','242','243','244','245','246',
    '248','249','250','251','252','253','254','255','256','257','258','259','260','261','262','263',
    '264','265','266','267','268','269','27','290','291','297','298','299','30','31','32','33','34',
    '350','351','352','353','354','355','356','357','358','359','36','370','371','372','373','374',
    '375','376','377','378','379','380','381','382','383','385','386','387','389','39','40','41','420',
    '421','423','43','44','45','46','47','48','49','500','501','502','503','504','505','506','507',
    '508','509','51','53','54','56','57','58','591','592','593','594','595','596','597','598','60',
    '63','64','65','66','670','673','675','676','677','678','679','680','685','686','687','688','690',
    '81','82','84','850','852','853','855','856','86','880','886','90','92','93','94','95','960','961',
    '962','963','964','965','966','967','968','970','971','972','973','974','975','976','977','992',
    '993','994','995','996','998');
GO

/* Rows still without an anchor, for the operator to review:
SELECT [Code], [CountryEnName] FROM [Countries]
WHERE [IsDeleted] = 0 AND ([TimezoneId] IS NULL OR LTRIM(RTRIM([TimezoneId])) = '')
ORDER BY [Code];
*/
