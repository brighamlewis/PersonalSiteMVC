using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail; //Added for access to mailmessage
using System.Configuration; //Added for access to the configuration manager
using System.Net; //Added for access to networkCredential
using PersonalSiteMVC.Models;

namespace PersonalSiteMVC.Controllers
{
    public class EmailController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        //Contact - GET
        public ActionResult Contact()
        {
            return View();
        }

        //Contact - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(ContactViewModel cvm)
        {

            //When a class has validation attributes, that validation should be checked BEFORE attempting to process any of the data they provided

            if (!ModelState.IsValid)
            {
                //Send them back form

                return View(cvm);
            }

            //Only exec if the form (object) passes the model validation.
            //Build the message -- what we will see when we receive their email

            string message = $"You have received an email from {cvm.Name} with a subject: {cvm.Subject}. Please respond to {cvm.Email} with your response to the following message: <br/>{cvm.Message}";

            //MailMessage (what actually sends the email) - System.Net.Mail
            MailMessage mm = new MailMessage(
                //FROM
                ConfigurationManager.AppSettings["EmailUser"].ToString(),
                //TO - this assumes forwarding by the host
                //you@yourdomain.ext, could be you@gmail.com,hotmail, etc. -- whatever address you wish
                //Sometimes -- SmarterASP has experienced issues with forwarding. If you find that the forwarding doesnt want to work here ou can just hardcode the address instead of this line below
                ConfigurationManager.AppSettings["EmailTo"].ToString(),
                cvm.Subject, message);

            //MailMessage properties
            //Allow HTML formatting in the email (Message has html in it) (similar to html raw)
            mm.IsBodyHtml = true;

            //if you want to mark these emails with high priority
            mm.Priority = MailPriority.High; //Default: Normal

            //Respond to senders email instead of our own SMTP client (Webmail)
            mm.ReplyToList.Add(cvm.Email);

            //SMTP Client
            //This is the information from the HOST (SmarterASP.net)
            //That allows the email to actually be sent
            SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["EmailClient"].ToString());

            //Client credentials (SmarterASP requires your username and password)
            client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["EmailUser"].ToString(), ConfigurationManager.AppSettings["EmailPass"].ToString());

            //It is possible that the mail server is down or we may have some configuration issues so we can encapsulate our code in a try/catch
            try
            {
                //Attempt to send email
                client.Send(mm);
            }
            catch (Exception ex)
            {
                ViewBag.CustomerMessage = $"We're sorry, but your request could not be completed at this time. Please try again later. Error Message: <br/> {ex.StackTrace}";

                return View(cvm);
            }


            return View("EmailConfirmation", cvm);
        }
    }
}