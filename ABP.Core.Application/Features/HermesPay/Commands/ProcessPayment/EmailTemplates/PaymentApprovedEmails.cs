namespace ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment.EmailTemplates
{
    public static class PaymentApprovedEmails
    {
        public static string CardHolderEmail(
            string cardOwnerFirstName,
            string commerceName,
            decimal amount,
            decimal newDebt,
            string lastFour,
            string dateTime)
        {
            return $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#7c3aed,#a78bfa);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128722; Compra con Hermes Pay</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{cardOwnerFirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Se ha procesado una compra con tu tarjeta de cr&#233;dito en <strong>{commerceName}</strong>.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Comercio</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">{commerceName}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Monto</td><td style="padding:10px 14px;font-size:18px;font-weight:700;color:#7c3aed;">RD${amount:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Tarjeta</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">&#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; {lastFour}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Deuda actual</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#dc2626;">RD${newDebt:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Fecha y hora</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">{dateTime}</td></tr>
</table>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Hermes Pay</p>
</div>
</div>
</body></html>
""";
        }

        public static string CommerceEmail(
            string commerceName,
            decimal amount,
            string lastFour,
            string dateTime)
        {
            return $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#16a34a,#22c55e);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128176; Pago Recibido - Hermes Pay</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{commerceName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Ha recibido un nuevo pago mediante Hermes Pay.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Tarjeta</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">&#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; {lastFour}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Monto recibido</td><td style="padding:10px 14px;font-size:18px;font-weight:700;color:#16a34a;">RD${amount:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Fecha y hora</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">{dateTime}</td></tr>
</table>
<p style="color:#64748b;font-size:13px;margin:0;">Este mensaje sirve como constancia del pago recibido.</p>
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
