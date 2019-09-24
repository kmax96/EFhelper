using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data.Entity;
using System.Configuration;

namespace EF
{
    public static class EFhelper
    {
        //setup connection string here or by Code
        private static string connStr = "";

        /// <summary>
        /// Connectionstring use in Entities Framework
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return connStr;
            }
            set
            {
                if (!value.IsNullOrEmpty())
                {
                    connStr = value;
                }
            }
        }

        /// <summary>
        /// connection string get from EF and it can use in SqlConnection
        /// </summary>
        /// <returns>string</returns>
        public static string getDBconnectionString()
        {
            string dbconnectionstring = "";
            using (DbContext dbph = new DbContext(connStr))
            { 
                dbconnectionstring =  dbph.Database.Connection.ConnectionString;
            } 
            return dbconnectionstring;
        }


        /// <summary>
        /// Easy way to dynamic get List of object by EF, but it has not optimize performannce
        /// </summary>
        /// <typeparam name="T">type of EF object</typeparam>
        /// <returns>List of T (which EF object user type in the param)</returns>
        public static List<T> getObject<T>()
        {
            connectionChecking();

            object p = null;

            using (DbContext oa = new DbContext(connStr))
            {
                string table = typeof(T).ToStr().Substring(".", Common.Direction.after);
                var r = oa.Database.SqlQuery<T>("SELECT * FROM " + table).ToList();

                p = r;
            }
            return (List<T>)Convert.ChangeType(p, typeof(List<T>));
        }


        /// <summary>
        /// Trigger StoreProcedure by EF
        /// </summary>
        /// <typeparam name="T">SP type in EF</typeparam>
        /// <param name="sp_name">SP name</param>
        /// <param name="paras">params as Dictionary, if not, please use null</param>
        /// <returns>List of T (which EF object user type in the param)</returns>
        public static List<T> triggerStorePro<T>(string sp_name, Dictionary<string, string> paras)
        {
            connectionChecking();
            object p = null;

            using (DbContext oa = new DbContext(connStr))
            {
                string table = sp_name;// typeof(T).ToStr().Substring(".", Common.Direction.after);


                string sqlStatement = "EXEC " + table;
                if (paras != null)
                {
                    foreach (KeyValuePair<string, string> kvp in paras)
                    {
                        if (kvp.Key.Contains("'"))
                        {
                            throw new Exception("' is not allowed in params key field");
                        }
                        sqlStatement += " " + kvp.Key.rsq() + "=N'" + kvp.Value.rsq() + "'";
                    }
                }
                var r = oa.Database.SqlQuery<T>(sqlStatement).ToList();

                p = r;
            }
            return (List<T>)Convert.ChangeType(p, typeof(List<T>));
        }

        /// <summary>
        /// Easy way to insert EF object to database 
        /// </summary>
        /// <param name="obj">object to be insert</param>
        /// <param name="keyID">provide a field name to tell the program to return which to be return to user</param>
        /// <returns>int</returns>
        public static int insertObject(object obj, string keyID)
        {
            connectionChecking();
            //int id = -1;
            using (DbContext oa = new DbContext(connStr))
            {
                var set = oa.Set(obj.GetType());
                set.Add(obj);
                oa.SaveChanges();
                oa.Dispose();
                //id = obj.id.toInt();
            }

            return getIdFromObj(obj, keyID);
        }


        /// <summary>
        ///  Easy way to Update EF object to database 
        /// </summary>
        /// <typeparam name="T">type of update object</typeparam>
        /// <param name="tbl">update object</param>
        public static void updateObject<T>(T tbl) where T : class
        {
            connectionChecking();
            using (DbContext oa = new DbContext(connStr))
            {
                oa.Set<T>().Attach(tbl);
                oa.Entry(tbl).State = System.Data.Entity.EntityState.Modified;
                oa.SaveChanges();
                oa.Dispose();
            }
        }


        /// <summary>
        /// get specific field from EF objeect
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        private static int getIdFromObj(object obj, string keyId)
        {

            int newId = -1;
            PropertyInfo[] properties = (obj).GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                string name = property.Name;
                string fromval = property.GetValue(obj, null).ToStr();
                if (name == keyId)
                {
                    newId = fromval.toInt();
                    break;
                }
            }
            return newId;
        }


        /// <summary>
        /// function to check EF connection is work or not
        /// </summary>
        private static void connectionChecking()
        {
            if (connStr.IsNullOrEmpty())
            {
                throw new Exception("Connection String not set");
            }
        }
    }
}
