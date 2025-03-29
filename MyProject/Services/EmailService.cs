using System.Net.Mail;
using System.Net;
using MyProject.Services;

namespace MyProject.Services.Mail;

public class EmailServices : IEmailServices
{
    public Task SendEmailAsync(string email, string subject, string message)
    {
        var mail = "rjcrohitt11sci368@gmail.com";
        var pw = "wbzf eepz mwta hfan";

        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(mail, pw)
        };

        return client.SendMailAsync(
            new MailMessage(from: mail,
                             to: email,
                              subject,
                              message
                            ));
    }
}