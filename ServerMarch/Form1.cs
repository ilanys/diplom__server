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
using System.Security.Cryptography;

namespace ServerMarch
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string transform__password(string password)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            return Encoding.UTF8.GetString(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        string sql__connect(string command,bool otv,string text)
        {
            if (!otv)
            {
                sql.Open();
                SqlCommand sql__command;
                using (sql__command = new SqlCommand(command, sql))
                {
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
                string JSON = "";
                using (sql__command = new SqlCommand(command, sql))
                {
                    SqlDataReader dr = sql__command.ExecuteReader();
                    while (dr.Read())
                    {
                        if(text=="Log")
                            otev = dr["logins"].ToString();
                        if (text == "token")
                            otev = dr["logins"].ToString()+"$" + dr["email"].ToString() + "$"+dr["name"].ToString()+ "$" + dr["lastname"].ToString()+ "$" + dr["double__name"].ToString() + "$" + dr["token"].ToString()+"$"+dr["type"].ToString();
                        if (text == "email")
                            otev = dr["email"].ToString();
                        if(text == "News")
                        {
                            JSON += dr["header"].ToString()+"%" + dr["body"] +"%"+ dr["picture"] +"=";
                        }
                        if(text== "Car__info")
                        {
                            JSON += dr["id_car"].ToString() + "%" + dr["Mark"] + "%" + dr["years"] + "=";
                        }

                    }
                }
                
                if(text=="News"||text=="Car__info")
                {
                    sql.Close();
                    return JSON;
                }
                sql.Close();
                return otev;
            }
        }

        public string add__token(string login, string password)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(password+login));
            byte[] result = md5.Hash;
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                strBuilder.Append(result[i].ToString("x2"));
            }
            login = strBuilder.ToString();
            return login;

            return "";
        }


        SqlConnection sql;
        Socket server;
        byte[] buffer;
        byte[] reception=new byte[2048];
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
                    case "user": { // Работа с пользователями

                       switch (reception__string.Split('/')[1])
                       {
                          case "Add": {  // Добавить пользователя
                                        string command = "use Test" + '\n' + " Select logins from users__bd where logins='" + reception__string.Split('/')[2].Split('%')[0] + "'";
                                        string log__text = sql__connect(command, true, "Log");
                                        if (log__text != "")
                                        {
                                            buffer = Encoding.UTF8.GetBytes("Error404");
                                            client.Send(buffer);
                                            return;
                                        }


                                        command = "use Test" + '\n' + " Select logins from users__bd where email = '" + reception__string.Split('/')[4]+ "'";
                                        log__text = sql__connect(command, true, "email");

                                        if (log__text != "" )
                                        {
                                            buffer = Encoding.UTF8.GetBytes("Error405");
                                            client.Send(buffer);
                                            return;
                                        }

                                        command = "INSERT INTO[dbo].[users__bd] "+'\n';
                                        command += " VALUES " + '\n';
                                        command += "('" + reception__string.Split('/')[2] + "', '" + reception__string.Split('/')[3] + "' , '" + reception__string.Split('/')[4]+ "','" + reception__string.Split('/')[5]+ "',";
                                        command += "' ',' ', "+"'"+add__token(reception__string.Split('/')[2],reception__string.Split('/')[3]) +"' , '"+reception__string.Split('/')[6]+"' )";

                                        string a= sql__connect(command,false,"");

                                        buffer = Encoding.UTF8.GetBytes("Пользователь создан");
                                        client.Send(buffer);
                                        break;}

                          case "Drop":{ // Удаление пользователя

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

                          case "IN": // Вход пользователя
                          {

                                        string command = "use Test" + '\n' + " Select logins from users__bd where logins='" + reception__string.Split('/')[2].Split('%')[0] + "'";
                                        string log__text = sql__connect(command, true, "Log");
                                        if (log__text == "" || log__text == " ")
                                        {
                                            buffer = Encoding.UTF8.GetBytes("Error404$");
                                            client.Send(buffer);

                                        }
                                        else
                                        {
                                            command = "Select * from users__bd  where logins ='" + reception__string.Split('/')[2] + "'"; //"'  [passwords] = '" + transform__password(reception__string.Split('/')[3]) + "'";
                                            log__text = sql__connect(command, true, "token");

                                            buffer = Encoding.UTF8.GetBytes(log__text);
                                            client.Send(buffer);
                                        }
                                            break;
                          }
                                case "Pictury":
                                    {
                                        switch(reception__string.Split('/')[2])
                                        {
                                            case "get": { break; }
                                            case "new":
                                                {
                                                    string command = "use Test" + '\n' + " DELETE user__image where user_id ='" + reception__string.Split('/')[3];
                                                    byte[] pictury__byte=new byte[10000000000000];
                                                    sql__connect(command, false, "");
                                                    client.Receive(pictury__byte);

                                                    command = "use Test" + '\n' + "Insert into user_id values ('"+ reception__string.Split('/')[3]+"')";

                                                    break;
                                                }
                                        }

                                        break;
                                    }
                       }


                            break; }

                    case "News" :{  // Работа с новостями 

                                switch(reception__string.Split('/')[1])
                                {

                                    case "get": // Получение новостей
                                    {
                                        string command = "use Test " + '\n' + " Select * from News";
                                        string JSON = sql__connect(command, true, "News");

                                        buffer = Encoding.UTF8.GetBytes(JSON);
                                        client.Send(buffer);
                                    }
                                    break;
                                }

                            break; }


                    case "Cars": // Работа с машинами
                        {
                            switch (reception__string.Split('/')[1])
                            {
                                
                                case "Add":  // Добавление машины
                                    {
                                        //toekn //Mark // name__care //years //pictury
                                        // Создание токена token пользователя + Марка машины + Наименование амшины 
                                        string id_car = add__token(reception__string.Split('/')[2],reception__string.Split('/')[3]+reception__string.Split('/')[4]);

                                        string command = " use Test "+ '\n' + " Insert Car  Values ('"+ reception__string.Split('/')[2] + " ', ' "+ id_car + " ' , ' " +  reception__string.Split('/')[3] + " ' , ' " +  reception__string.Split('/')[4] + " ' ";
                                        sql__connect(command, false, "");

                                        client.Send(Encoding.UTF8.GetBytes("Машина создана"));


                                        break;
                                    }
                                case "Comment__add": // Добавление коментария
                                    {
                                        
                                        //  Получаемые данные   id car // comment // Author

                                        string command = " use Test " + '\n' + " Insert Comment Values ( '" + reception__string.Split('/')[2] + " ', ' " + reception__string.Split('/')[3] + " ', ' " + reception__string.Split('/')[4] + " ') ";

                                        sql__connect(command, false, "");

                                        client.Send(Encoding.UTF8.GetBytes("Коментарий создан"));

                                        break;
                                    }
                                case "Info":     //Получение машин по token пользователя
                                    {
                                        string command = " use Test " + '\n' + "Select * from Car where users = '"+reception__string.Split('/')[2]+"'";

                                        string Car =    sql__connect(command, true, "Car__info");

                                        client.Send(Encoding.UTF8.GetBytes(Car));   

                                        break;
                                    }
                            }
                            break;
                        }
                    case "default": { break; }
                }





                client.Close();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            dat = DateTime.Now;
       //     timer1.Enabled = true;
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
            server__start();
            
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
