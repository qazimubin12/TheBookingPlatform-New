using TheBookingPlatform.Database;
using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Data.SqlClient;

namespace TheBookingPlatform.Services
{
    public class CompanyServices
    {

        #region Singleton
        public static CompanyServices Instance
        {
            get
            {
                if (instance == null) instance = new CompanyServices();
                return instance;
            }
        }
        private static CompanyServices instance { get; set; }
        private CompanyServices()
        {
        }
        #endregion
        
        public string GetDomain()
        {
            return "https://localhost:44314";
            //return "https://app.yourbookingplatform.com";
        }
        public List<string> GetTimeZones()
        {
            var ListofString = new List<string>
            {
                "ACT","AET","Africa/Abidjan","Africa/Accra","Africa/Addis_Ababa","Africa/Algiers","Africa/Asmara","Africa/Asmera","Africa/Bamako","Africa/Bangui","Africa/Banjul","Africa/Bissau","Africa/Blantyre","Africa/Brazzaville","Africa/Bujumbura","Africa/Cairo","Africa/Casablanca","Africa/Ceuta","Africa/Conakry","Africa/Dakar","Africa/Dar_es_Salaam","Africa/Djibouti","Africa/Douala","Africa/El_Aaiun","Africa/Freetown","Africa/Gaborone","Africa/Harare","Africa/Johannesburg","Africa/Kampala","Africa/Khartoum","Africa/Kigali","Africa/Kinshasa","Africa/Lagos","Africa/Libreville","Africa/Lome","Africa/Luanda","Africa/Lubumbashi","Africa/Lusaka","Africa/Malabo","Africa/Maputo","Africa/Maseru","Africa/Mbabane","Africa/Mogadishu","Africa/Monrovia","Africa/Nairobi","Africa/Ndjamena","Africa/Niamey","Africa/Nouakchott","Africa/Ouagadougou","Africa/Porto-Novo","Africa/Sao_Tome","Africa/Timbuktu","Africa/Tripoli","Africa/Tunis","Africa/Windhoek","AGT","America/Adak","America/Anchorage","America/Anguilla","America/Antigua","America/Araguaina","America/Argentina/Buenos_Aires","America/Argentina/Catamarca","America/Argentina/ComodRivadavia","America/Argentina/Cordoba","America/Argentina/Jujuy","America/Argentina/La_Rioja","America/Argentina/Mendoza","America/Argentina/Rio_Gallegos","America/Argentina/Salta","America/Argentina/San_Juan","America/Argentina/San_Luis","America/Argentina/Tucuman","America/Argentina/Ushuaia","America/Aruba","America/Asuncion","America/Atikokan","America/Atka","America/Bahia","America/Barbados","America/Belem","America/Belize","America/Blanc-Sablon","America/Boa_Vista","America/Bogota","America/Boise","America/Buenos_Aires","America/Cambridge_Bay","America/Campo_Grande","America/Cancun","America/Caracas","America/Catamarca","America/Cayenne","America/Cayman","America/Chicago","America/Chihuahua","America/Coral_Harbour","America/Cordoba","America/Costa_Rica","America/Cuiaba","America/Curacao","America/Danmarkshavn","America/Dawson","America/Dawson_Creek","America/Denver","America/Detroit","America/Dominica","America/Edmonton","America/Eirunepe","America/El_Salvador","America/Ensenada","America/Fort_Wayne","America/Fortaleza","America/Glace_Bay","America/Godthab","America/Goose_Bay","America/Grand_Turk","America/Grenada","America/Guadeloupe","America/Guatemala","America/Guayaquil","America/Guyana","America/Halifax","America/Havana","America/Hermosillo","America/Indiana/Indianapolis","America/Indiana/Knox","America/Indiana/Marengo","America/Indiana/Petersburg","America/Indiana/Tell_City","America/Indiana/Vevay","America/Indiana/Vincennes","America/Indiana/Winamac","America/Indianapolis","America/Inuvik","America/Iqaluit","America/Jamaica","America/Jujuy","America/Juneau","America/Kentucky/Louisville","America/Kentucky/Monticello","America/Knox_IN","America/La_Paz","America/Lima","America/Los_Angeles","America/Louisville","America/Maceio","America/Managua","America/Manaus","America/Marigot","America/Martinique","America/Mazatlan","America/Mendoza","America/Menominee","America/Merida","America/Mexico_City","America/Miquelon","America/Moncton","America/Monterrey","America/Montevideo","America/Montreal","America/Montserrat","America/Nassau","America/New_York","America/Nipigon","America/Nome","America/Noronha","America/North_Dakota/Center","America/North_Dakota/New_Salem","America/Panama","America/Pangnirtung","America/Paramaribo","America/Phoenix","America/Port_of_Spain","America/Port-au-Prince","America/Porto_Acre","America/Porto_Velho","America/Puerto_Rico","America/Rainy_River","America/Rankin_Inlet","America/Recife","America/Regina","America/Resolute","America/Rio_Branco","America/Rosario","America/Santarem","America/Santiago","America/Santo_Domingo","America/Sao_Paulo","America/Scoresbysund","America/Shiprock","America/St_Barthelemy","America/St_Johns","America/St_Kitts","America/St_Lucia","America/St_Thomas","America/St_Vincent","America/Swift_Current","America/Tegucigalpa","America/Thule","America/Thunder_Bay","America/Tijuana","America/Toronto","America/Tortola","America/Vancouver","America/Virgin","America/Whitehorse","America/Winnipeg","America/Yakutat","America/Yellowknife","Antarctica/Casey","Antarctica/Davis","Antarctica/DumontDUrville","Antarctica/Mawson","Antarctica/McMurdo","Antarctica/Palmer","Antarctica/Rothera","Antarctica/South_Pole","Antarctica/Syowa","Antarctica/Vostok","Arctic/Longyearbyen","ART","Asia/Aden","Asia/Almaty","Asia/Amman","Asia/Anadyr","Asia/Aqtau","Asia/Aqtobe","Asia/Ashgabat","Asia/Ashkhabad","Asia/Baghdad","Asia/Bahrain","Asia/Baku","Asia/Bangkok","Asia/Beirut","Asia/Bishkek","Asia/Brunei","Asia/Calcutta","Asia/Choibalsan","Asia/Chongqing","Asia/Chungking","Asia/Colombo","Asia/Dacca","Asia/Damascus","Asia/Dhaka","Asia/Dili","Asia/Dubai","Asia/Dushanbe","Asia/Gaza","Asia/Harbin","Asia/Ho_Chi_Minh","Asia/Hong_Kong","Asia/Hovd","Asia/Irkutsk","Asia/Istanbul","Asia/Jakarta","Asia/Jayapura","Asia/Jerusalem","Asia/Kabul","Asia/Kamchatka","Asia/Karachi","Asia/Kashgar","Asia/Kathmandu","Asia/Katmandu","Asia/Kolkata","Asia/Krasnoyarsk","Asia/Kuala_Lumpur","Asia/Kuching","Asia/Kuwait","Asia/Macao","Asia/Macau","Asia/Magadan","Asia/Makassar","Asia/Manila","Asia/Muscat","Asia/Nicosia","Asia/Novosibirsk","Asia/Omsk","Asia/Oral","Asia/Phnom_Penh","Asia/Pontianak","Asia/Pyongyang","Asia/Qatar","Asia/Qyzylorda","Asia/Rangoon","Asia/Riyadh","Asia/Riyadh87","Asia/Riyadh88","Asia/Riyadh89","Asia/Saigon","Asia/Sakhalin","Asia/Samarkand","Asia/Seoul","Asia/Shanghai","Asia/Singapore","Asia/Taipei","Asia/Tashkent","Asia/Tbilisi","Asia/Tehran","Asia/Tel_Aviv","Asia/Thimbu","Asia/Thimphu","Asia/Tokyo","Asia/Ujung_Pandang","Asia/Ulaanbaatar","Asia/Ulan_Bator","Asia/Urumqi","Asia/Vientiane","Asia/Vladivostok","Asia/Yakutsk","Asia/Yekaterinburg","Asia/Yerevan","AST","Atlantic/Azores","Atlantic/Bermuda","Atlantic/Canary","Atlantic/Cape_Verde","Atlantic/Faeroe","Atlantic/Faroe","Atlantic/Jan_Mayen","Atlantic/Madeira","Atlantic/Reykjavik","Atlantic/South_Georgia","Atlantic/St_Helena","Atlantic/Stanley","Australia/ACT","Australia/Adelaide","Australia/Brisbane","Australia/Broken_Hill","Australia/Canberra","Australia/Currie","Australia/Darwin","Australia/Eucla","Australia/Hobart","Australia/LHI","Australia/Lindeman","Australia/Lord_Howe","Australia/Melbourne","Australia/North","Australia/NSW","Australia/Perth","Australia/Queensland","Australia/South","Australia/Sydney","Australia/Tasmania","Australia/Victoria","Australia/West","Australia/Yancowinna","BET","Brazil/Acre","Brazil/DeNoronha","Brazil/East","Brazil/West","BST","Canada/Atlantic","Canada/Central","Canada/Eastern","Canada/East-Saskatchewan","Canada/Mountain","Canada/Newfoundland","Canada/Pacific","Canada/Saskatchewan","Canada/Yukon","CAT","CET","Chile/Continental","Chile/EasterIsland","CNT","CST","CST6CDT","CTT","Cuba","EAT","ECT","EET","Egypt","Eire","EST","EST5EDT","Etc/GMT","Etc/GMT-0","Etc/GMT-1","Etc/GMT-10","Etc/GMT-11","Etc/GMT-12","Etc/GMT-13","Etc/GMT-14","Etc/GMT-2","Etc/GMT-3","Etc/GMT-4","Etc/GMT-5","Etc/GMT-6","Etc/GMT-7","Etc/GMT-8","Etc/GMT-9","Etc/GMT+0","Etc/GMT+1","Etc/GMT+10","Etc/GMT+11","Etc/GMT+12","Etc/GMT+2","Etc/GMT+3","Etc/GMT+4","Etc/GMT+5","Etc/GMT+6","Etc/GMT+7","Etc/GMT+8","Etc/GMT+9","Etc/GMT0","Etc/Greenwich","Etc/UCT","Etc/Universal","Etc/UTC","Etc/Zulu","Europe/Amsterdam","Europe/Andorra","Europe/Athens","Europe/Belfast","Europe/Belgrade","Europe/Berlin","Europe/Bratislava","Europe/Brussels","Europe/Bucharest","Europe/Budapest","Europe/Chisinau","Europe/Copenhagen","Europe/Dublin","Europe/Gibraltar","Europe/Guernsey","Europe/Helsinki","Europe/Isle_of_Man","Europe/Istanbul","Europe/Jersey","Europe/Kaliningrad","Europe/Kiev","Europe/Lisbon","Europe/Ljubljana","Europe/London","Europe/Luxembourg","Europe/Madrid","Europe/Malta","Europe/Mariehamn","Europe/Minsk","Europe/Monaco","Europe/Moscow","Europe/Nicosia","Europe/Oslo","Europe/Paris","Europe/Podgorica","Europe/Prague","Europe/Riga","Europe/Rome","Europe/Samara","Europe/San_Marino","Europe/Sarajevo","Europe/Simferopol","Europe/Skopje","Europe/Sofia","Europe/Stockholm","Europe/Tallinn","Europe/Tirane","Europe/Tiraspol","Europe/Uzhgorod","Europe/Vaduz","Europe/Vatican","Europe/Vienna","Europe/Vilnius","Europe/Volgograd","Europe/Warsaw","Europe/Zagreb","Europe/Zaporozhye","Europe/Zurich","GB","GB-Eire","GMT","GMT0","Greenwich","Hongkong","HST","Iceland","IET","Indian/Antananarivo","Indian/Chagos","Indian/Christmas","Indian/Cocos","Indian/Comoro","Indian/Kerguelen","Indian/Mahe","Indian/Maldives","Indian/Mauritius","Indian/Mayotte","Indian/Reunion","Iran","Israel","IST","Jamaica","Japan","JST","Kwajalein","Libya","MET","Mexico/BajaNorte","Mexico/BajaSur","Mexico/General","Mideast/Riyadh87","Mideast/Riyadh88","Mideast/Riyadh89","MIT","MST","MST7MDT","Navajo","NET","NST","NZ","NZ-CHAT","Pacific/Apia","Pacific/Auckland","Pacific/Chatham","Pacific/Easter","Pacific/Efate","Pacific/Enderbury","Pacific/Fakaofo","Pacific/Fiji","Pacific/Funafuti","Pacific/Galapagos","Pacific/Gambier","Pacific/Guadalcanal","Pacific/Guam","Pacific/Honolulu","Pacific/Johnston","Pacific/Kiritimati","Pacific/Kosrae","Pacific/Kwajalein","Pacific/Majuro","Pacific/Marquesas","Pacific/Midway","Pacific/Nauru","Pacific/Niue","Pacific/Norfolk","Pacific/Noumea","Pacific/Pago_Pago","Pacific/Palau","Pacific/Pitcairn","Pacific/Ponape","Pacific/Port_Moresby","Pacific/Rarotonga","Pacific/Saipan","Pacific/Samoa","Pacific/Tahiti","Pacific/Tarawa","Pacific/Tongatapu","Pacific/Truk","Pacific/Wake","Pacific/Wallis","Pacific/Yap","PLT","PNT","Poland","Portugal","PRC","PRT","PST","PST8PDT","ROK","Singapore","SST","SystemV/AST4","SystemV/AST4ADT","SystemV/CST6","SystemV/CST6CDT","SystemV/EST5","SystemV/EST5EDT","SystemV/HST10","SystemV/MST7","SystemV/MST7MDT","SystemV/PST8","SystemV/PST8PDT","SystemV/YST9","SystemV/YST9YDT","Turkey","UCT","Universal","US/Alaska","US/Aleutian","US/Arizona","US/Central","US/Eastern","US/East-Indiana","US/Hawaii","US/Indiana-Starke","US/Michigan","US/Mountain","US/Pacific","US/Pacific-New","US/Samoa","UTC","VST","W-SU","WET","Zulu"
            };
            return ListofString;

        }
        public List<Company> GetCompany(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Companies.Where(p => p.Business != null && p.Business.ToLower()
                                            .Contains(SearchTerm))
                                            .ToList();
                }
                else
                {
                    return context.Companies.OrderBy(x=>x.Business).ToList();
                }
            }
        }


        public Company GetCompanyByName(string Name)
        {
            using (var context = new DSContext())
            {
                // Use SingleOrDefault to directly retrieve the entity without the extra Where clause.
                return context.Companies
                              .AsNoTracking()  // Disable change tracking for a read-only operation.
                              .SingleOrDefault(x => x.Business == Name);
            }
        }

        public void UpdateUsersCompany(string NewBusiness, string OldBusinessName)
        {
            try
            {
                using (var dbContext = new DSContext())
                {
             
                    dbContext.Database.ExecuteSqlCommand($"UPDATE AspNetUsers SET Company = '" + NewBusiness + "' where Company ='" + OldBusinessName + "'");

                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public void UpdateBusinessFromTheTables(string NewBusiness,string OldBusinessName)
        {
            try
            {
                using (var dbContext = new DSContext())
                {
                    // Get a list of tables in the database
                    var tables = dbContext.Database.SqlQuery<string>("SELECT Table_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = 'dbo' and TABLE_NAME NOT IN ('__MigrationHistory','AspNetUsers','AspNetRoles','AspNetUserRoles','AspNetUserClaims','AspNetUserLogins');").ToList();

                    foreach (var tableName in tables)
                    {
                        // Check if the table contains a column named BusinessName
                        var columns = dbContext.Database.SqlQuery<string>($"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = 'Business'").ToList();

                        if (columns.Contains("Business"))
                        {
                            // If the table contains BusinessName column, update the BusinessName
                            dbContext.Database.ExecuteSqlCommand($"UPDATE {tableName} SET Business = '" + NewBusiness + "' where Business ='"+ OldBusinessName + "'");
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public Company GetCompany(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Companies.Find(ID);

            }
        }

       

        public void SaveCompany(Company Company)
        {
            using (var context = new DSContext())
            {
                context.Companies.Add(Company);
                context.SaveChanges();
            }
        }

        public void UpdateCompany(Company Company)
        {
            using (var context = new DSContext())
            {
                context.Entry(Company).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteCompany(int ID)
        {
            using (var context = new DSContext())
            {
                var Company = context.Companies.Find(ID);
                DeleteCompanyByBusinessValue(Company.Business);

            }
        }

        public void DeleteCompanyByBusinessValue(string businessValue)
        {
            using (var context = new DSContext())
            {
                string sql = @"
            DECLARE @TableName NVARCHAR(MAX);
            DECLARE @SQL NVARCHAR(MAX);
            DECLARE table_cursor CURSOR FOR 
            SELECT TABLE_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'Business';

            OPEN table_cursor;
            FETCH NEXT FROM table_cursor INTO @TableName;

            WHILE @@FETCH_STATUS = 0
            BEGIN
                SET @SQL = 'DELETE FROM ' + QUOTENAME(@TableName) + ' WHERE Business = @BusinessValue';
                EXEC sp_executesql @SQL, N'@BusinessValue NVARCHAR(MAX)', @BusinessValue;
                FETCH NEXT FROM table_cursor INTO @TableName;
            END;

            CLOSE table_cursor;
            DEALLOCATE table_cursor;";

                context.Database.ExecuteSqlCommand(sql, new SqlParameter("@BusinessValue", businessValue));
            }
        }



    }
}
