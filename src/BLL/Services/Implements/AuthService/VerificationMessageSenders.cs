using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Http.Json;
using BLL.Services.Interfaces.AuthService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Implements.AuthService;

public class EmailVerificationSender(
    IConfiguration configuration,
    ILogger<EmailVerificationSender> logger) : IEmailVerificationSender
{
    public async Task SendAsync(string email, string recipientName, string code)
    {
        if (!bool.TryParse(configuration["Notifications:Email:Enabled"], out var enabled) || !enabled)
        {
            logger.LogWarning("DEV EMAIL OTP for {Email}: {Code}", email, code);
            return;
        }

        using var client = new SmtpClient(
            configuration["Notifications:Email:Host"],
            int.TryParse(configuration["Notifications:Email:Port"], out var port) ? port : 587)
        {
            EnableSsl = bool.TryParse(configuration["Notifications:Email:UseSsl"], out var ssl) && ssl,
            Credentials = new NetworkCredential(
                configuration["Notifications:Email:Username"],
                configuration["Notifications:Email:Password"])
        };
        using var message = new MailMessage
        {
            From = new MailAddress(configuration["Notifications:Email:From"]!),
            Subject = "ReThreads - Xác nhận địa chỉ email",
            Body = $"Xin chào {recipientName},\n\nMã xác nhận email của bạn là: {code}\nMã có hiệu lực trong 5 phút.\n\nReThreads",
            IsBodyHtml = false
        };
        message.To.Add(email);
        await client.SendMailAsync(message);
    }
}

public class SmsVerificationSender(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<SmsVerificationSender> logger) : ISmsVerificationSender
{
    public async Task SendAsync(string phoneNumber, string code)
    {
        if (!bool.TryParse(configuration["Notifications:Sms:Enabled"], out var enabled) || !enabled)
        {
            logger.LogWarning("DEV SMS OTP for {PhoneNumber}: {Code}", phoneNumber, code);
            return;
        }

        var endpoint = configuration["Notifications:Sms:Endpoint"]
            ?? throw new InvalidOperationException("SMS endpoint is not configured.");
        var token = configuration["Notifications:Sms:ApiKey"];
        if (!string.IsNullOrWhiteSpace(token))
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.PostAsJsonAsync(endpoint, new
        {
            to = phoneNumber,
            message = $"ReThreads: Ma xac nhan cua ban la {code}. Ma co hieu luc trong 5 phut."
        });
        response.EnsureSuccessStatusCode();
    }
}
