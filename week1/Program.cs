using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;


namespace week1
{
    class Program
    {
        static void Main(string[] args)
        {
            string classID = "F721900";                 //Class ID provided by school
            Console.WriteLine("追蹤中的課程ID= " + classID);
            List<string> target = new List<string>();   //Target email address to be sent
            target.Add("a885566885566@gmail.com");
            target.Add("dfer231314241@gmail.com");
            SendMailByGmail(target, "課程餘額掃描程式啟動", "追蹤中的課程ID=" + classID);
            while (true)
            {
                try
                {
                    string pageHtml = getHtmlByUrl("http://course-query.acad.ncku.edu.tw/qry/qry001.php?dept_no=F7");
                    string classInfo = getClassInfoByHtml(pageHtml, classID, target);
                    DateTime nowTime = DateTime.Now;
                    Console.WriteLine(nowTime.ToString() + "課程資訊=" + classInfo);
                }
                catch
                {
                    Console.WriteLine("發生錯誤");
                }
                System.Threading.Thread.Sleep(5000);
            }
            Console.Read();
        }
        /*Obtain specific infomation from a html file*/
        static string getClassInfoByHtml(string html, string classID, List<string> target)
        {
            string keyword = "<TD style='text-align: center;' >";
            int keywordLen = keyword.Length;
            //Get the index of the class info
            int firstIndex = html.IndexOf(classID);                         

            string frontPart = html.Substring(firstIndex - 500, 500);
            string backPart = html.Substring(firstIndex, 1000);
            //Get the index of class number
            int classNumIndex = frontPart.LastIndexOf(keyword, frontPart.Length-keywordLen);
            string classNum = frontPart.Substring(classNumIndex+keywordLen, 3);
            //Get the index of department number
            int depNumIndex = frontPart.LastIndexOf(keyword, frontPart.Length - 2 * keywordLen);
            string depNum = frontPart.Substring(depNumIndex + keywordLen, 2);

            keyword = "</a></TD>";
            keywordLen = keyword.Length;
            //Get the index of class name
            int classNameIndex = backPart.IndexOf(keyword);
            classNameIndex = backPart.IndexOf(keyword, classNameIndex+10);
            int classNameFirstIndex = backPart.LastIndexOf("\">", classNameIndex);
            string className = backPart.Substring(classNameFirstIndex+2, classNameIndex - classNameFirstIndex-2);

            keyword = "<TD style='text-align: center;' >";
            keywordLen = keyword.Length;
            //Get the index of class rest number
            int restNumFirstIndex = backPart.IndexOf(keyword, classNameIndex + 3 * keywordLen);
            int restNumIndex = backPart.IndexOf("</TD>", restNumFirstIndex);
            string restNum = backPart.Substring(restNumFirstIndex+keywordLen, restNumIndex - restNumFirstIndex-keywordLen);
            int restNumInt = 0;
            //Send class info to the target email address
            if (int.TryParse(restNum, out restNumInt))
            {
                string contant = "課程ID= " + depNum + " " + classNum +
                                 "\n課程名稱= " + className +
                                 "餘額數量= " + restNum;
                SendMailByGmail(target, "課程餘額掃描程式發現餘額", contant);
            }
            return depNum+classNum+className+restNum;
        }

        /* Obtain html file by url */
        static string getHtmlByUrl(string url)
        {
            try
            {
                WebClient MyWebClient = new WebClient();
                MyWebClient.Credentials = CredentialCache.DefaultCredentials;   //獲取或設置用於向Internet資源的請求進行身份驗證的網絡憑據
                Byte[] pageData = MyWebClient.DownloadData(url);                //從指定網站下載數據
                string pageHtml = Encoding.UTF8.GetString(pageData); 
                return pageHtml;
            }
            catch (WebException webEx)
            {
                Console.WriteLine(webEx.Message.ToString());
                return null;
            }
        }

        static void SendMailByGmail(List<string> MailList, string Subject, string Body)
        {
            MailMessage msg = new MailMessage();
            msg.To.Add(string.Join(",", MailList.ToArray()));  //收件者，以逗號分隔不同收件者 ex "test@gmail.com,test2@gmail.com"
            msg.From = new MailAddress("a885566885566@gmail.com", "測試郵件", System.Text.Encoding.UTF8);//郵件標題 
            msg.Subject = Subject;                              //郵件標題編碼  
            msg.SubjectEncoding = System.Text.Encoding.UTF8;    //郵件內容
            msg.Body = Body;
            msg.IsBodyHtml = true;
            msg.BodyEncoding = System.Text.Encoding.UTF8;       //郵件內容編碼 
            msg.Priority = MailPriority.Normal;                 //郵件優先級 
            SmtpClient MySmtp = new SmtpClient("smtp.gmail.com", 587);//建立 SmtpClient 物件 並設定 Gmail的smtp主機及Port 
            //設定你的帳號密碼
            MySmtp.Credentials = new System.Net.NetworkCredential("a885566885566", "makerobotwith89s51");
            //Gmail 的 smtp 使用 SSL
            MySmtp.EnableSsl = true;
            MySmtp.Send(msg);
        }
    }
}
