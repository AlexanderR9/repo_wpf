using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;


namespace mylib.mymail
{
    public static class MyMail
    {
        public static void sendMail()
        {
            string login = "lub2503@yandex.ru";
            string passwd = "250381";
            string smtp_server = "smtp.yandex.ru";
            string subj = "Subject text";
            string text = "mail text";

            string adr_to = "kosha9@mail.ru";


            //Console.WriteLine("sendMail() 1");
            //объявление адресов, откуда и куда
            //MailAddress adr_from = new MailAddress(login, "from test");
            //MailAddress adr_to = new MailAddress("kosha9@mail.ru");

            //создание сообщения
            MailMessage msg = new MailMessage(login, adr_to, subj, text);
            msg.Priority = MailPriority.Normal;
            //msg.Subject = "Subject text";
            //msg.Body = "Message test text!!!";

            Console.WriteLine("sendMail() 3");
            // создание объекта, с которого будем отправлять письмо
            SmtpClient smtp_obj = new SmtpClient(smtp_server);
            smtp_obj.Port = 25;
            smtp_obj.UseDefaultCredentials = false;
            smtp_obj.DeliveryFormat = SmtpDeliveryFormat.International;
            smtp_obj.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp_obj.Credentials = new NetworkCredential(login, passwd); // логин и пароль
            smtp_obj.EnableSsl = true; //включить шифрование
            smtp_obj.Timeout = 17000;
            
            Console.WriteLine("sendMail()  from {0}  ssl={1}  port={2}", login, smtp_obj.EnableSsl.ToString(), smtp_obj.Port);
            //отправить письмо
            bool ok = true;
            try{smtp_obj.Send(msg);}
            catch {Console.WriteLine("sendMail(): Error send mail  !!!"); ok=false;}
            finally {if (ok) Console.WriteLine("sendMail(): sended mail ok!");}

            smtp_obj.Dispose();


        }
    }

}