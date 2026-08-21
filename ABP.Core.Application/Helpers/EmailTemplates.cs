namespace ABP.Core.Application.Helpers
{
    /// <summary>
    /// Centralized email templates for the entire application.
    /// All HTML email templates live here — handlers should NOT contain inline HTML.
    /// </summary>
    public static class EmailTemplates
    {
        private static string Wrap(string gradientFrom, string gradientTo, string icon, string title, string bodyContent)
        {
            return $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,{gradientFrom},{gradientTo});padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">{icon} {title}</h1>
</div>
<div style="padding:30px;">
{bodyContent}
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Plataforma de Banca Digital ITLA</p>
</div>
</div>
</body></html>
""";
        }

        private static string Table(params (string label, string value)[] rows)
        {
            var sb = new System.Text.StringBuilder("<table style=\"width:100%;border-collapse:collapse;margin-bottom:24px;\">");
            for (int i = 0; i < rows.Length; i++)
            {
                string bg = i % 2 == 0 ? "#f8fafc" : "#ffffff";
                string radius = i switch
                {
                    0 => "border-radius:6px 0 0 6px;",
                    _ when i == rows.Length - 1 => "border-radius:0 6px 6px 0;",
                    _ => ""
                };
                sb.Append($"<tr><td style=\"padding:10px 14px;background:{bg};color:#64748b;font-size:13px;font-weight:600;{radius}\">{rows[i].label}</td><td style=\"padding:10px 14px;background:{bg};font-size:15px;font-weight:700;color:#0b1f3a;{radius}\">{rows[i].value}</td></tr>");
            }
            sb.Append("</table>");
            return sb.ToString();
        }

        private static string MonetaryRow(string label, string value, string color = "#0b1f3a")
            => $"<tr><td style=\"padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;\">{label}</td><td style=\"padding:10px 14px;background:#f8fafc;font-size:18px;font-weight:700;color:{color};border-radius:0 6px 6px 0;\">{value}</td></tr>";

        // ═══════════════════════════════════════════════════════
        // HermesPay
        // ═══════════════════════════════════════════════════════

        public static string HermesPayCardHolderApproved(string firstName, string commerceName, decimal amount, decimal newDebt, string lastFour, string dateTime)
        {
            var body = $"""
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{firstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Se ha procesado una compra con tu tarjeta de cr&#233;dito en <strong>{commerceName}</strong>.</p>
{Table(("Comercio", commerceName), ("Monto", $"RD${amount:N2}"), ("Tarjeta", $"&#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; {lastFour}"), ("Deuda actual", $"RD${newDebt:N2}"), ("Fecha y hora", dateTime))}
""";
            return Wrap("#7c3aed", "#a78bfa", "&#128722;", "Compra con Hermes Pay", body);
        }

        public static string HermesPayCommerceReceived(string commerceName, decimal amount, string lastFour, string dateTime)
        {
            var body = $"""
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{commerceName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Ha recibido un nuevo pago mediante Hermes Pay.</p>
{Table(("Tarjeta", $"&#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; {lastFour}"), ("Monto recibido", $"RD${amount:N2}"), ("Fecha y hora", dateTime))}
<p style="color:#64748b;font-size:13px;margin:0;">Este mensaje sirve como constancia del pago recibido.</p>
""";
            return Wrap("#16a34a", "#22c55e", "&#128176;", "Pago Recibido - Hermes Pay", body);
        }

        public static string HermesPayCardHolderRejected(string firstName, string commerceName, decimal amount, decimal availableCredit, string dateTime)
        {
            var body = $"""
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{firstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Su intento de pago en <strong>{commerceName}</strong> fue <strong>rechazado</strong> por falta de cr&#233;dito disponible.</p>
{Table(("Comercio", commerceName), ("Monto intentado", $"RD${amount:N2}"), ("Cr&#233;dito disponible", $"RD${availableCredit:N2}"), ("Fecha y hora", dateTime))}
<div style="background:#fee2e2;border-left:4px solid #dc2626;padding:12px 16px;border-radius:0 6px 6px 0;margin-bottom:20px;">
<p style="color:#991b1b;font-size:13px;margin:0;">&#9888;&#65039; Si usted no reconoce esta operaci&#243;n, comun&#237;quese con la entidad bancaria.</p>
</div>
""";
            return Wrap("#dc2626", "#ef4444", "&#10060;", "Pago Rechazado", body);
        }

        // ═══════════════════════════════════════════════════════
        // Account Activation
        // ═══════════════════════════════════════════════════════

        public static string AccountActivation(string userName, string verificationUri)
        {
            var body = $"""
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{userName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Su cuenta ha sido registrada correctamente en <strong>Artemis Banking Pro</strong>. Para activar su usuario, haga clic en el siguiente bot&#243;n:</p>
<div style="text-align:center;margin:0 0 24px;">
<a href="{verificationUri}" style="display:inline-block;background:linear-gradient(135deg,#1a56db,#3b82f6);color:#fff;text-decoration:none;padding:14px 36px;border-radius:8px;font-weight:700;font-size:15px;">&#9989; Activar Mi Cuenta</a>
</div>
<div style="background:#f8fafc;border-radius:8px;padding:16px;margin-bottom:20px;">
<p style="color:#64748b;font-size:12px;margin:0 0 8px;">Si el bot&#243;n no funciona, copie y pegue este enlace en su navegador:</p>
<p style="color:#1a56db;font-size:11px;margin:0;word-break:break-all;">{verificationUri}</p>
</div>
<div style="background:#fef9c3;border-left:4px solid #ca8a04;padding:12px 16px;border-radius:0 6px 6px 0;">
<p style="color:#854d0e;font-size:12px;margin:0;">Si usted no realiz&#243; este registro, puede ignorar este mensaje.</p>
</div>
""";
            return Wrap("#1a56db", "#3b82f6", "&#128231;", "Confirmaci&#243;n de Cuenta", body);
        }

        public static string AccountActivationToken(string userName, string token)
        {
            return $"""
<p>Hola {userName},</p>
<p>Su cuenta ha sido registrada correctamente en Artemis Banking.</p>
<p>Por favor confirme su cuenta usando este token: <strong>{token}</strong></p>
<br/>
<p><small>Si usted no realizó este registro, puede ignorar este mensaje.</small></p>
""";
        }

        // ═══════════════════════════════════════════════════════
        // Password Reset
        // ═══════════════════════════════════════════════════════

        public static string PasswordResetLink(string userName, string resetUri)
        {
            var body = $"""
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{userName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Recibimos una solicitud para restablecer su contrase&#241;a en <strong>Artemis Banking Pro</strong>.</p>
<div style="text-align:center;margin:0 0 24px;">
<a href="{resetUri}" style="display:inline-block;background:linear-gradient(135deg,#dc2626,#ef4444);color:#fff;text-decoration:none;padding:14px 36px;border-radius:8px;font-weight:700;font-size:15px;">&#128274; Restablecer Contrase&#241;a</a>
</div>
<div style="background:#f8fafc;border-radius:8px;padding:16px;margin-bottom:20px;">
<p style="color:#64748b;font-size:12px;margin:0 0 8px;">Si el bot&#243;n no funciona, copie y pegue este enlace:</p>
<p style="color:#dc2626;font-size:11px;margin:0;word-break:break-all;">{resetUri}</p>
</div>
<div style="background:#fef9c3;border-left:4px solid #ca8a04;padding:12px 16px;border-radius:0 6px 6px 0;">
<p style="color:#854d0e;font-size:12px;margin:0;">Si usted no solicit&#243; este cambio, ignore este mensaje.</p>
</div>
""";
            return Wrap("#dc2626", "#ef4444", "&#128274;", "Restablecer Contrase&#241;a", body);
        }

        public static string PasswordResetToken(string userName, string token)
        {
            return $"""
<p>Hola {userName},</p>
<p>Por favor restablezca su contraseña usando este token: <strong>{token}</strong></p>
""";
        }

        // ═══════════════════════════════════════════════════════
        // Loan Assignment
        // ═══════════════════════════════════════════════════════

        public static string LoanAssigned(string firstName, string loanNumber, decimal amount, decimal rate, string nextDueDate, decimal nextAmount)
        {
            var body = $"""
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{firstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Se ha asignado un nuevo pr&#233;stamo a tu cuenta.</p>
{Table(("N&#250;mero de pr&#233;stamo", $"#{loanNumber}"), ("Monto aprobado", $"RD${amount:N2}"), ("Tasa de inter&#233;s anual", $"{rate}%"), ("Pr&#243;xima cuota", $"RD${nextAmount:N2}"), ("Vencimiento pr&#243;xima cuota", nextDueDate), ("Fecha de asignaci&#243;n", DateTime.Now.ToString("dd/MM/yyyy HH:mm")))}
""";
            return Wrap("#1a56db", "#3b82f6", "&#128179;", "Pr&#233;stamo Asignado", body);
        }

        public static string LoanRateUpdated(string firstName, string loanNumber, decimal newRate, decimal nextAmount, string nextDueDate)
        {
            var body = $"""
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{firstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">La tasa de inter&#233;s de tu pr&#233;stamo <strong>#{loanNumber}</strong> ha sido actualizada.</p>
{Table(("Nuevo valor de pr&#243;xima cuota", $"RD${nextAmount:N2}"), ("Fecha de vencimiento", nextDueDate), ("Fecha de cambio", DateTime.Now.ToString("dd/MM/yyyy HH:mm")))}
""";
            return Wrap("#1a56db", "#3b82f6", "&#128200;", "Tasa de Inter&#233;s Actualizada", body);
        }

        // ═══════════════════════════════════════════════════════
        // Credit Card Assignment
        // ═══════════════════════════════════════════════════════

        public static string CreditCardAssigned(string firstName, string lastFour, decimal limit, string expirationDate, string assignmentDate)
        {
            var body = $"""
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{firstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Se ha asignado una nueva tarjeta de cr&#233;dito a tu cuenta.</p>
{Table(("&#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; " + lastFour, ""), ("L&#237;mite de cr&#233;dito", $"RD${limit:N2}"), ("Fecha de expiraci&#243;n", expirationDate), ("Fecha de asignaci&#243;n", assignmentDate))}
""";
            return Wrap("#7c3aed", "#a78bfa", "&#128179;", "Tarjeta de Cr&#233;dito Asignada", body);
        }

        // ═══════════════════════════════════════════════════════
        // Saving Account Assignment
        // ═══════════════════════════════════════════════════════

        public static string SavingAccountAssigned(string firstName, string accountNumber, decimal initialBalance, string assignmentDate)
        {
            var body = $"""
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{firstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Se ha asignado una cuenta de ahorro a tu cuenta.</p>
{Table(("N&#250;mero de cuenta", $"****{accountNumber[^4..]}"), ("Balance inicial", $"RD${initialBalance:N2}"), ("Fecha de asignaci&#243;n", assignmentDate))}
""";
            return Wrap("#0891b2", "#22d3ee", "&#128176;", "Cuenta de Ahorro Asignada", body);
        }

        // ═══════════════════════════════════════════════════════
        // Overdue Installment
        // ═══════════════════════════════════════════════════════

        public static string OverdueInstallment(string firstName, string loanNumber, int installmentNumber, decimal amount, string dueDate, int daysOverdue)
        {
            var body = $"""
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{firstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Una cuota de tu pr&#233;stamo <strong>#{loanNumber}</strong> se encuentra atrasada.</p>
{Table(("Cuota", $"#{installmentNumber}"), ("Monto", $"RD${amount:N2}"), ("Fecha de vencimiento", dueDate), ("D&#237;as de atraso", $"{daysOverdue} d&#237;as"))}
<div style="background:#fee2e2;border-left:4px solid #dc2626;padding:12px 16px;border-radius:0 6px 6px 0;margin-bottom:20px;">
<p style="color:#991b1b;font-size:13px;margin:0;">&#9888;&#65039; Por favor realice el pago lo antes posible para evitar recargos adicionales.</p>
</div>
""";
            return Wrap("#dc2626", "#ef4444", "&#9888;&#65039;", "Cuota Atrasada", body);
        }
    }
}
