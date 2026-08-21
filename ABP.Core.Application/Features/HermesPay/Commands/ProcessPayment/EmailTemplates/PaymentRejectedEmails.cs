namespace ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment.EmailTemplates
{
    public static class PaymentRejectedEmails
    {
        public static string CardHolderEmail(
            string cardOwnerFirstName,
            string commerceName,
            decimal amount,
            decimal availableCredit,
            string dateTime)
        {
            return $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#dc2626,#ef4444);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#10060; Pago Rechazado</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{cardOwnerFirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Su intento de pago en <strong>{commerceName}</strong> fue <strong>rechazado</strong> por falta de cr&#233;dito disponible.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Comercio</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">{commerceName}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Monto intentado</td><td style="padding:10px 14px;font-size:18px;font-weight:700;color:#dc2626;">RD${amount:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Cr&#233;dito disponible</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">RD${availableCredit:N2}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Fecha y hora</td><td style="padding:10px 14px;font-size:15px;color:#0b1f3a;">{dateTime}</td></tr>
</table>
<div style="background:#fee2e2;border-left:4px solid #dc2626;padding:12px 16px;border-radius:0 6px 6px 0;margin-bottom:20px;">
<p style="color:#991b1b;font-size:13px;margin:0;">&#9888;&#65039; Si usted no reconoce esta operaci&#243;n, comun&#237;quese con la entidad bancaria.</p>
</div>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Hermes Pay</p>
</div>
</div>
</body></html>
""";
        }
    }
}
