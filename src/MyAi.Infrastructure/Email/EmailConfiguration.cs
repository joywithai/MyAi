namespace MyAi.Infrastructure.Email;

public class EmailConfiguration
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; }

    public string SmtpHost { get; set; } = "localhost";

    public int SmtpPort { get; set; } = 587;

    public string SmtpUsername { get; set; } = string.Empty;

    public string SmtpPassword { get; set; } = string.Empty;

    public bool UseSsl { get; set; } = true;

    public string FromAddress { get; set; } = "no-reply@myai.local";

    public string FromName { get; set; } = "MyAi";
}
