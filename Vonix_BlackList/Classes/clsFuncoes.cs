using System;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vonix_BlackList.Classes
{
    class clsFuncoes
    {
        public static async Task<bool> EnviaEmailAsync(string _subject, string _body, string _anexo)
        {
            try
            {
                await Task.Run(() =>
                {
                    MailMessage e_mail = new MailMessage();

                    e_mail.To.Add("suporte@vonix.com.br");               // para
                                                                         //               e_mail.CC.Add("copia@tradecall.com.br");           // copia
                    e_mail.Bcc.Add("suporte.ti@tradecall.com.br");       // copia oculta

                    if (_anexo != "")
                    {
                        Attachment anexo = new Attachment(_anexo);
                        e_mail.Attachments.Add(anexo);
                    }

                    e_mail.From = new MailAddress("suporte.ti@tradecall.com.br");
                    e_mail.Subject = _subject;                      // assunto

                    e_mail.IsBodyHtml = false;                      // corpo do email
                    e_mail.Body = _body;
                    e_mail.Priority = MailPriority.High;

                    SmtpClient smtp = new SmtpClient("smtp.tradecall.com.br", 587);
                    //smtp.EnableSsl = true;
                    System.Net.NetworkCredential cred = new System.Net.NetworkCredential("suporte.ti@tradecall.com.br", "Access@2019");
                    smtp.Credentials = cred;
                    //smtp.UseDefaultCredentials = true;

                    smtp.Send(e_mail);
                });
                return true;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "email", MessageBoxButtons.OK, MessageBoxIcon.Information);
                string x = e.Message;
                return false;
            }
        }


        public static async Task<bool> GeraCsvAsync(DataTable _dt, string _path)
        {
            try
            {
                StreamWriter sw = new StreamWriter(_path, false);

                ////headers  
                //for (int i = 0; i < _dt.Columns.Count; i++)
                //{
                //    sw.Write(_dt.Columns[i]);
                //    if (i < _dt.Columns.Count - 1)
                //    {
                //        sw.Write("");
                //    }
                //}
                //sw.Write(sw.NewLine);

                foreach (DataRow dr in _dt.Rows)
                {
                    for (int i = 0; i < _dt.Columns.Count; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            string value = dr[i].ToString();

                            if (value.Contains("\r\n"))                  // se possuir quebra de linha
                            {
                                value = value.Replace("\r\n", "<br>");
                            }
                            if (value.Contains("\n"))                    // se possuir linha em branco
                            {
                                value = value.Replace("\n", "");
                            }
                            if (value.Contains(";"))
                            {
                                value = value.Replace(";", "-");
                            }

                            sw.Write(value);
                        }
                        if (i < _dt.Columns.Count - 1)
                        {
                            sw.Write("");
                        }
                    }
                    await sw.WriteAsync(sw.NewLine);
                }
                sw.Close();
                sw.Dispose();

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Gera Arquivo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

        }


        public static string RetornaNumero(string _texto)
        {
            Regex r = new Regex(@"\d+");

            string result = "";

            foreach (Match m in r.Matches(_texto))
            {
                result += m.Value;
            }

            if (result != "")
            {
                Double i = 0;
                i = Convert.ToDouble(result);

                result = i.ToString();
            }
            else
            {
                result = "0";
            }

            return result;
        }

        public static Boolean ValidaFone(string _fone)
        {
            Boolean booV = false;

            switch (_fone.Length)
            {
                case 10:
                    string x = _fone.Substring(2, 1);

                    switch (_fone.Substring(2, 1))
                    {
                        case "2":
                        case "3":
                        case "4":
                        case "5":
                            booV = true;
                            break;
                    }
                    break;

                case 11:
                    if (_fone.Substring(2, 1) == "9")
                    {
                        booV = true;
                    }
                    break;

                default:
                    break;
            }

            return booV;
        }


    }
}
