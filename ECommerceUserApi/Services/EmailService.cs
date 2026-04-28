using MailKit.Net.Smtp;
using MimeKit;

namespace ECommerceUserApi.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmail(string toEmail, string otp)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_config["EmailSettings:Email"]));
            email.To.Add(MailboxAddress.Parse(toEmail));

            email.Subject = "Your OTP Verification Code";

            email.Body = new TextPart("plain")
            {
                Text = $"Your OTP is: {otp}. It is valid for 5 minutes."
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["EmailSettings:Host"],
                int.Parse(_config["EmailSettings:Port"]),
                MailKit.Security.SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _config["EmailSettings:Email"],
                _config["EmailSettings:AppPassword"]);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}