using System.Net.Mail;
using System.Net;
using Api.Dtos;

namespace LoginMicroservice.Api.Utils;

public static class EmailSender
{
    public static void SendEmail(EmailDto dto, IConfiguration configuration)
    {
        try
        {

            Task.Run(() =>
            {
                MailMessage newMail = new();
                SmtpClient client = new("smtp.gmail.com");
                newMail.From = new MailAddress(configuration["EmailSender"], "NOT REPLY");
                newMail.To.Add(dto.Email);
                newMail.Subject = "Code to acess your account";
                newMail.IsBodyHtml = true; newMail.Body = $"<h1>Here is your code!</h1>\n<h2>Use this code to login in your account</h2>\n<h3>{dto.Code}</h3>";

                client.EnableSsl = true;
                client.Port = 587;
                client.Credentials = new NetworkCredential("guilhermebernava00@gmail.com", configuration["EmailPassword"]);
                client.Send(newMail);
            });

        }
        catch (Exception)
        {
            throw;
        }
    }
}
