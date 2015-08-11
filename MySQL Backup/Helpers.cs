extern alias oldVer;
extern alias newVer;

using MysqlAlias = newVer::MySql.Data.MySqlClient;
using OldMysqlAlias = oldVer::MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySQL_Backup
{
    static class Helpers
    {
        //This checks both styles of connections and return false when the second(older) method ALSO fails
        //maybe should've done this with 2 separate methods but thought against it as those 2 new functions
        //would increase overhead and wouldn't be needed again.
        public static bool TestConnection(BackupItem itm)
        {
            //Begin NewStyle check
            MysqlAlias.MySqlConnection conn = null;
            try
            {
                conn = new MysqlAlias.MySqlConnection(itm.ConnectionString());
                conn.Open();
            }
            catch (MysqlAlias.MySqlException)
            {
                //Begin OldStyle Check
                OldMysqlAlias.MySqlConnection old_conn = null;

                try
                {
                    old_conn = new OldMysqlAlias.MySqlConnection(itm.ConnectionString());
                    old_conn.Open();
                }
                catch (MysqlAlias.MySqlException)
                {
                    return false;
                }catch(Exception)
                {
                    return false;
                }
                finally
                {
                    if (old_conn != null)
                    {
                        old_conn.Close();
                    }
                }
                //End OldStyle Check
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            //End NewStyle check
            return true;
        }
    }
}
