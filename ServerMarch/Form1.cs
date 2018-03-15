using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;
using System.Threading;


namespace ServerMarch
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string sql__connect(string command,bool otv,string text)
        {
            if (!otv)
            {
                sql.Open();
                SqlCommand sql__command;
                using (sql__command = new SqlCommand(command, sql))
                {
                    sql__command.Connection.Open();
                    sql__command.ExecuteNonQuery();
                }
                sql.Close();
                return "";
            }
            else
            {
                sql.Open();
                string otev = "";
                SqlCommand sql__command;
                using (sql__command = new SqlCommand(command, sql))
                {
                    SqlDataReader dr = sql__command.ExecuteReader();
                    while (dr.Read())
                    {
                        if(text=="Log")
                            otev = dr["login"] + "$" + dr["password"];
                        if (text == "token")
                            otev = dr["token"].ToString();
                    }
                }
                sql.Close();
                return otev;
            }
        }



        SqlConnection sql;
        Socket server;
        byte[] buffer;
        byte[] reception;
        IPEndPoint iPEnd;
        DateTime dat = new DateTime();
        void server__start()
        {
            server.Listen(50);
            while(true)
            {
                Socket client = server.Accept();
                int lenght = client.Receive(reception);
                string reception__string = Encoding.UTF8.GetString(reception, 0, lenght);

                switch(reception__string.Split('/')[0])
                {
                    case "user": {

                       switch (reception__string.Split('/')[1])
                       {
                          case "Add": {
                                        string command = "use Test" + '\n' + "Select From dbo.[user] where dbo.[user].login='" + reception__string.Split('/')[2].Split('%')[0] + "'";
                                        string log__text = sql__connect(command, true, "Log");
                                        if (log__text == "" || log__text == " ")
                                        {
                                            buffer = Encoding.UTF8.GetBytes("Ошибка 404 Пользователь нет в базе");
                                            client.Send(buffer);
                                            return;
                                        }


                                        command = "USE [Test]"+'\n';
                                        command += "GO" + '\n';
                                        command += "INSERT INTO[dbo].[user]"+'\n';
                                        command += "VALUES" + '\n';
                                        command += "('" + reception__string.Split('/')[2].Split('%')[0] + "', '" + reception__string.Split('/')[2].Split('%')[1] + "', '" + reception__string.Split('/')[2].Split('%')[2] + "','" + reception__string.Split('/')[2].Split('%')[3] + "',";
                                        command += "'" + reception__string.Split('/')[2].Split('%')[4] + " ','" + "Null" + "','" + reception__string.Split('/')[2].Split('%')[6] + "')";
                                        command += "GO";

                                        string a= sql__connect(command,false,"");

                                        buffer = Encoding.UTF8.GetBytes("Пользователь создан");
                                        client.Send(buffer);
                                        break;}

                          case "Drop":{

                                        string command = "use Test" + '\n' + "Select From dbo.[user] where dbo.[user].login='" + reception__string.Split('/')[2].Split('%')[0] + "'";
                                        string log__text = sql__connect(command, true, "Log");
                                        if (log__text == "" || log__text == " ")
                                        {
                                            buffer = Encoding.UTF8.GetBytes("Ошибка 404 Пользователь нет в базе");
                                            client.Send(buffer);
                                            return;
                                        }

                                        command = "use Test" + '\n' + "Delete From dbo.[user] where dbo.[user].token='" + reception__string.Split('/')[2] + "'";
                                        string a = sql__connect(command,false,"");
                                        buffer = Encoding.UTF8.GetBytes("Пользователь удалён");
                                        client.Send(buffer);
                                        break;}

                          case "IN":
                          {
                                        
                                        string command = "use Test" + '\n' + "Select From dbo.[user] where dbo.[user].login='" + reception__string.Split('/')[2].Split('%')[0] + "'";
                                        string log__text = sql__connect(command, true, "Log");
                                        if(log__text==""||log__text==" ")
                                        {
                                            buffer = Encoding.UTF8.GetBytes("Ошибка 404 Пользователь нет в базе");
                                            client.Send(buffer);
                                            return;
                                        }
                                        command = "Select from dbo.[user].login='"+ reception__string.Split('/')[2].Split('%')[0] + "'";
                                        log__text = sql__connect(command, true, "token");

                                         buffer = Encoding.UTF8.GetBytes(log__text);
                                        client.Send(buffer);
                                        break;
                          }
                       }     


                            break; }
                    case "default": { break; }
                }





                client.Close();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            dat = DateTime.Now;
            timer1.Enabled = true;
            richTextBox1.Text = "Server => Сервер запускается" + '\n';
            sql = new SqlConnection("Data Source=DESKTOP-VB2HVQL\\MYSQL_BD;Initial Catalog=Test;Integrated Security=True");
            richTextBox1.Text += "Server => Подключение к БД" + '\n';
            
            richTextBox1.Text += "Server => Успешно" + '\n';
            server = new Socket(SocketType.Stream, ProtocolType.Tcp);

            richTextBox1.Text += "Server => Получение порта" + '\n';
            IPEndPoint iPEnd = new IPEndPoint(IPAddress.Any, 8080);
            richTextBox1.Text += "Server => Успешно: порт 8080" + '\n';
            server.Bind(iPEnd);
            richTextBox1.Text += "Server => Сервер запущен" + '\n';
            Thread newThread = new Thread(server__start);
            newThread.Start();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "Сервер не запущен";
            richTextBox1.Enabled = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime stopwatch = new DateTime();
            stopwatch = new DateTime();
            
            stopwatch= stopwatch.AddTicks(DateTime.Now.Ticks - dat.Ticks);

            label1.Text = "Сервер запущен => "+ String.Format("{0:HH:mm:ss}",stopwatch);

        }
    }
}
