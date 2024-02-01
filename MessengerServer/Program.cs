using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Runtime.Remoting.Messaging;

namespace MessengerServer
{
    internal class Program
    {
        public static SqlConnection sqlConnection = null;
        public static string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Garma\\source\\repos\\MessengerServer\\MessengerServer\\Database1.mdf;Integrated Security=True";

        static void Main(string[] args)
        {
            // подключение
            sqlConnection = new SqlConnection(connectionString);
            try
            {
                sqlConnection.Open();

                Console.WriteLine("Подключение к БД прошло успешно.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint ep = new IPEndPoint(ip, 1024);
            s.Bind(ep);
            s.Listen(10);
            try
            {
                while (true)
                {
                    Socket ns = s.Accept();

                    Console.WriteLine('\n' + ns.RemoteEndPoint.ToString() + "  " + DateTime.Now.ToString());

                    byte[] buffer = new byte[1024];
                    int bytesRead = ns.Receive(buffer);
                    string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead); // пришедшая строка
                    string[] data = receivedMessage.Split('|');
                    Console.WriteLine("Received message: " + data[0] + " " + data[1] + " " + data[2]);

                    string answer = "";
                    switch (data[0])
                    {
                        case "Registration":
                            answer = Registration(data[1], data[2]);
                            break;
                        case "Vhod":
                            answer = Vhod(data[1], data[2]);
                            break;
                        case "UserList":
                            answer = UserList();
                            break;
                        case "FriendList":
                            answer = FriendList(data[1]);
                            break;
                        case "RequestList":
                            answer = RequestList(data[1]);
                            break;
                        case "AddRequest":
                            answer = AddRequest(data[1], data[2]);
                            break;
                        case "DeleteRequest":
                            answer = DeleteRequest(data[1], data[2]);
                            break;
                        case "Messages":
                            answer = Messages(data[1], data[2]);
                            break;
                        default:
                            answer = SendMessages(data[0], data[1], data[2]);
                            break;
                    }



                    byte[] responseBytes = Encoding.ASCII.GetBytes(answer);
                    ns.Send(responseBytes);

                    ns.Shutdown(SocketShutdown.Both);
                    ns.Close();
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        public static string Registration(string login, string password)
        {
            try
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(
                    $"SELECT * FROM Users WHERE Login='{login}';",
                    sqlConnection
                );

                DataSet dataSet = new DataSet();

                dataAdapter.Fill(dataSet);

                if (dataSet.Tables[0].Rows.Count == 0)
                {

                    // добавление пользователя
                    try
                    {
                        dataAdapter = new SqlDataAdapter(
                            $"INSERT INTO Users(Login, Password) VALUES ('{login}', '{password}')",
                            sqlConnection
                        );

                        dataSet = new DataSet();

                        dataAdapter.Fill(dataSet);
                        Console.WriteLine("Регистрация: Успех." + '\n');
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    return "True";
                }
                Console.WriteLine("Регистрация: Провал.");
                return "False";

                //   dataSet.Tables[0].Rows.Count;   ->   кол-во строк
                //foreach (DataColumn column in dataSet.Tables[0].Columns)
                //{
                //    Console.WriteLine(dataSet.Tables[0].Rows[0][column]);
                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "";
        }

        public static string Vhod(string login, string password)
        {
            try
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(
                    $"SELECT * FROM Users WHERE Login='{login}';",
                    sqlConnection
                );

                DataSet dataSet = new DataSet();

                dataAdapter.Fill(dataSet);

                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    if (dataSet.Tables[0].Rows[0][2].ToString() == password)
                    {
                        Console.WriteLine("Вход: Успех.");
                        return "True";
                    }
                    Console.WriteLine("пум пум пум " + dataSet.Tables[0].Rows[0][2].ToString() + " | " + password);
                }
                Console.WriteLine("Вход: Провал.");
                return "False";

                //   dataSet.Tables[0].Rows.Count;   ->   кол-во строк
                //foreach (DataColumn column in dataSet.Tables[0].Columns)
                //{
                //    Console.WriteLine(dataSet.Tables[0].Rows[0][column]);
                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "";
        }

        public static string UserList()
        {
            try
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(
                    $"SELECT * FROM Users;",
                    sqlConnection
                );

                DataSet dataSet = new DataSet();

                dataAdapter.Fill(dataSet);

                if (dataSet.Tables[0].Rows.Count == 0)
                    return "";

                string answer = "";
                foreach (DataRow Row in dataSet.Tables[0].Rows)
                {
                    answer += (answer == "") ? Row[1].ToString() : '|' + Row[1].ToString();
                }

                Console.WriteLine("запрос: " + answer);

                return answer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "";
        }

        public static string FriendList(string login)
        {
            try
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(
                    $"SELECT * FROM Friends WHERE UserLogin = '{login}';",
                    sqlConnection
                );

                DataSet dataSet = new DataSet();

                dataAdapter.Fill(dataSet);

                string answer = "";
                foreach (DataRow Row in dataSet.Tables[0].Rows)
                {

                    SqlDataAdapter dataAdapter1 = new SqlDataAdapter(
                        $"SELECT * FROM Friends WHERE UserLogin = '{Row[2].ToString()}' AND FriendLogin = '{login}';",
                        sqlConnection
                    );

                    DataSet dataSet1 = new DataSet();

                    dataAdapter1.Fill(dataSet1);


                    if (dataSet1.Tables[0].Rows.Count == 0) // ничего нет
                        continue;

                    answer += (answer == "") ? dataSet1.Tables[0].Rows[0][1].ToString() : '|' + dataSet1.Tables[0].Rows[0][1].ToString();
                }
                
                if (answer == "")
                    Console.WriteLine("запрос: НИЧЕГО");
                else
                    Console.WriteLine("запрос: " + answer);

                return answer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "";
        }

        public static string RequestList(string login)
        {
            try
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(
                    $"SELECT * FROM Friends WHERE UserLogin = '{login}';",
                    sqlConnection
                );

                DataSet dataSet = new DataSet();

                dataAdapter.Fill(dataSet);

                string answer = "";
                foreach (DataRow Row in dataSet.Tables[0].Rows)
                {

                    SqlDataAdapter dataAdapter1 = new SqlDataAdapter(
                        $"SELECT * FROM Friends WHERE UserLogin = '{Row[2].ToString()}' AND FriendLogin = '{login}';",
                        sqlConnection
                    );

                    DataSet dataSet1 = new DataSet();

                    dataAdapter1.Fill(dataSet1);


                    if (dataSet1.Tables[0].Rows.Count == 0) // ничего нет
                        answer += (answer == "") ? Row[2].ToString() : '|' + Row[2].ToString();

                }

                if (answer == "")
                    Console.WriteLine("перептсочка: НИЧЕГО");
                else
                    Console.WriteLine("переписочка: Отправлено");

                return answer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "";
        }

        public static string AddRequest(string from, string to)
        {
            try
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(
                    $"SELECT * FROM Friends WHERE UserLogin = '{from}' AND FriendLogin = '{to}';",
                    sqlConnection
                );

                DataSet dataSet = new DataSet();

                dataAdapter.Fill(dataSet);


                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    dataAdapter = new SqlDataAdapter(
                        $"INSERT INTO Friends(UserLogin, FriendLogin) VALUES ('{from}', '{to}')",
                        sqlConnection
                    );

                    dataSet = new DataSet();

                    dataAdapter.Fill(dataSet);

                    Console.WriteLine($"заявка в друзья: " + from + " -> " + to + " Успех");
                    return "True";
                }

                Console.WriteLine($"заявка в друзья: " + from + " -> " + to + " Провал");
                return "False";

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "False";
        }

        public static string DeleteRequest(string from, string to)
        {
            try
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(
                    $"DELETE FROM Friends WHERE UserLogin = '{from}' AND FriendLogin = '{to}';",
                    sqlConnection
                );

                DataSet dataSet = new DataSet();

                dataAdapter.Fill(dataSet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Удаление заявки: " + from + " -> " + to);

            return "";
        }

        public static string Messages(string from, string to)
        {
            try
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(
                    $"SELECT * FROM Messages WHERE (FromLogin = '{from}' AND ToLogin = '{to}') OR (FromLogin = '{to}' AND ToLogin = '{from}');",
                    sqlConnection
                );

                DataSet dataSet = new DataSet();

                dataAdapter.Fill(dataSet);

                if (dataSet.Tables[0].Rows.Count == 0)
                    return "";

                string answer = "";
                foreach (DataRow Row in dataSet.Tables[0].Rows)
                {
                    answer += (answer == "") ? "": "\n";
                    if (Row[1].ToString() == from) 
                        answer += "*: ";
                    else 
                        answer += to + ": ";
                    answer += Row[4];
                }

                Console.WriteLine("переписочка: Отправлена");

                return answer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "";
        }

        public static string SendMessages(string from, string to, string text)
        {
            try
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(
                    $"INSERT INTO Messages(FromLogin, ToLogin, Time, Text) VALUES ('{from}', '{to}', '{DateTime.Now}', '{text}')",
                    sqlConnection
                );

                DataSet dataSet = new DataSet();

                dataAdapter.Fill(dataSet);

                Console.WriteLine($"сообщение: " + from + " -> " + to + " Отправлено");
                return "";

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "";
        }
    }
}
