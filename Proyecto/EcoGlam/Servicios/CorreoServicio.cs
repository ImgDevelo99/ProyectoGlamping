using EcoGlam.Models;
using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;

namespace EcoGlam.Servicios
{
    public static class CorreoServicio
    {

        private static string _Host = "smtp.gmail.com";
        private static int _Puerto = 587;

        private static string _NombreEnvia = "EcoGlam";
        private static string _Correo = "ecoglam7@gmail.com";
        private static string _Clave = "zltuaygncljcipwt";

        public static bool Enviar (Correo oCorreo)
        {
            try
            {
                var email = new MimeMessage();

                email.From.Add(new MailboxAddress(_NombreEnvia, _Correo));
                email.To.Add(MailboxAddress.Parse(oCorreo.Para));
                email.Subject = oCorreo.Asunto;
                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = oCorreo.Contenido,
                };

                var smtp = new SmtpClient();
                smtp.Connect(_Host, _Puerto, SecureSocketOptions.StartTls);

                smtp.Authenticate(_Correo, _Clave);
                smtp.Send(email);
                smtp.Disconnect(true);

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
