﻿using System;
using MySql.Data.MySqlClient;
using System.Data;
using Renci.SshNet;

    public class SSH
    {
        public SshClient client;
        public string sshhost;
        public int sshport;
        public string sshuid;
        public string sshpassword;
        public int sshlocalport;
        public System.UInt32 boundport;

        public Mysql mysql = new Mysql();

        public void Initialize(String host, int port, String uid, String password, int localport)
        {
            this.sshhost = host;
            this.sshport = port;
            this.sshuid = uid;
            this.sshpassword = password;
            this.sshlocalport = localport;

            this.client = new SshClient(this.sshhost, this.sshport, this.sshuid, this.sshpassword);
        }

        public void OpenSSHConnection()
        {
            try
            {
                this.client.Connect();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void CloseSSHConnection()
        {
            this.client.Disconnect();
        }

        public void OpenPort()
        {
            ForwardedPortLocal portfwrdl = new ForwardedPortLocal("127.0.0.1", "127.0.0.1", Convert.ToUInt32(this.sshlocalport));
            this.client.AddForwardedPort(portfwrdl);
            
            try
            {
                portfwrdl.Start();
                this.boundport = portfwrdl.BoundPort;
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }
    }

    public class Mysql
    {
        public MySqlConnection mysqlconnection;
        private string sqlserver;
        private string sqlport;
        private string sqldatabase;
        private string sqluid;
        private string sqlpassword;



        public void ConnectToMySQL()
        {

        }

        //Initialize private variables and create connection string
        public void Initialize(String server, String port, String database,String uid, String password)
        {
            this.sqlserver =  server;
            this.sqlport = port;
            this.sqldatabase = database;
            this.sqluid = uid;
            this.sqlpassword = password;

            string connectionString;
            //create connection string with ssl mode off
            connectionString = "SERVER=" + this.sqlserver + ";" + "PORT=" + this.sqlport + ';' + "DATABASE=" + this.sqldatabase + ";" + "UID=" + this.sqluid + ";" + "PASSWORD=" + this.sqlpassword + ";" + "SslMode=none";

            mysqlconnection = new MySqlConnection(connectionString);
        }

        //Open and Close SQL connection
        public bool OpenSQLConnection()
        {
            try
            {
                mysqlconnection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //error code 0: connect failed to server
                //error code 1045: invalid username/password
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("unable to connect to server");
                        break;
                    case 1042:
                        Console.WriteLine("unable to connect to mysql host");
                        break;
                    case 1045:
                        Console.WriteLine("Invalid username or password");
                        break;
                }
                return false;
            }
        }
        public void CloseSQLConnection()
        {
            this.mysqlconnection.Close();
        }

        //SQL Statement Generator

        //Select All From Specified Table
        public String SQLSelectAll(String tablename)
        {
            OpenSQLConnection();
            String context = "SELECT * FROM " + tablename;
            MySqlDataReader reader = null;
            MySqlCommand com = new MySqlCommand(context, this.mysqlconnection);

            reader = com.ExecuteReader();
            DataTable table = reader.GetSchemaTable();

            String re = null;

            if(table.Rows.Count == 0)
            {
                return "no rows returned";
            }
            else
            {
                Boolean flag = true;
                while (reader.Read())
                {
                    //Get table colomn names
                    if (flag)
                    {
                        foreach (DataRow r in table.Rows)
                        {
                            re += String.Format("{0,15}", r["ColumnName"]);
                        }
                        re += '\n';
                        flag = false;
                    }
                    //Get and format table data
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        re += String.Format("{0,15}", reader.GetString(i));
                    }
                    re += "\n";
                }
            }
            reader.Close();
            CloseSQLConnection();
            return re;
        }
        //Select Single Row From Specified Table using ColumnName & a field value you want to match
        //ex: SELECT * FROM tablename WHERE UserName LIKE "Student"
        //will return multiple rows if there are multiple matches
        public String SQLSelectUser(String TableName, String ColumnName, String Matcher)
        {
            OpenSQLConnection();
            String context = "select * from " +  TableName + " where " + ColumnName + " like \"" + Matcher + "\"";

            MySqlDataReader reader = null;
            MySqlCommand com = new MySqlCommand(context, this.mysqlconnection);

            reader = com.ExecuteReader();
            DataTable table = reader.GetSchemaTable();

            String re = null;

            if (table.Rows.Count == 0)
            {
                return "no rows returned";
            }
            else
            {
                Boolean flag = true;
                while (reader.Read())
                {
                    if (flag)
                    {
                        foreach (DataRow r in table.Rows)
                        {
                            re += String.Format("{0,15}", r["ColumnName"]);
                        }
                        re += '\n';
                        flag = false;
                    }
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        re += String.Format("{0,15}", reader.GetString(i));
                    }
                    re += "\n";
                }
            }
            reader.Close();
            CloseSQLConnection();
            return re;
        }
        //Insert User into Usertable
        //ex: INSERT INTO User (UserName, UserPassword) VALUES ("username", "Password");
        public String SQLInsertUser(String UserName, String UserPassword, String UserEmail)
        {
            OpenSQLConnection();
            String context = "INSERT INTO User (UserName, UserPassword, UserEmail) VALUES (\"" + UserName + "\", \"" + UserPassword + "\", \"" + UserEmail + "\")";
            MySqlCommand com = new MySqlCommand(context, this.mysqlconnection);

            String r = null;
            if (com.ExecuteNonQuery() == 0){
                //0 rows affected sql statement was not successful                
                r = "Could not add User: " + UserName;
            }
            else
            {
                //successfully affected rows in database
                r = "Added User: " + UserName;
            }
            CloseSQLConnection();
            return r;

        }
        //Delete user from User table
        public String SQLDeleteUser(String UserName)
        {
            OpenSQLConnection();
            String context = "DELETE FROM User WHERE UserName=\"" + UserName + "\"";
            MySqlCommand com = new MySqlCommand(context, this.mysqlconnection);

            String r = null;
            if (com.ExecuteNonQuery() == 0)
            {
                //0 rows affected sql statement was not successful                
                r = "Could not Delete User: " + UserName;
            }
            else
            {
                //successfully affected rows in database
                r = "Deleted User: " + UserName;
            }
            CloseSQLConnection();
            return r;
        }

    }

