using System;
using System.Data;
using System.Windows.Forms;
using Vonix_BlackList.Classes;


namespace Vonix_BlackList
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.Text = Application.ProductName.ToString() + " ".PadLeft(50) + Application.ProductVersion;

            CarregarGridAsync();
        }


        private async void CarregarGridAsync()
        {
            // propr : RowHeadersVisible = False  -- para nao aparecer a 1a coluna do datagridview
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();

            clsVariaveis.GstrSQL = "select ID ,TELEFONE from Vonix_BlackList where DtIncl = cast(getdate() as date) and ativo = 1 and gerou = 0 order by id ";
            DataTable dt = await clsBanco.ExecuteQueryRetornoAsync(clsVariaveis.GstrSQL);
            if (dt.Rows.Count > 0)
            {
                dataGridView1.DataSource = dt;
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    if (column.DataPropertyName == "")
                    { column.Width = 50; }
                    else if (column.DataPropertyName == "ID")
                    { column.Visible = false; }
                    else
                    {
                        column.MinimumWidth = 155;
                        column.SortMode = DataGridViewColumnSortMode.NotSortable;
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    }
                }
            }
        }


        private void Limpar()
        {
            txtFone.Text = string.Empty;
            btnExcluir.Enabled = false;
            CarregarGridAsync();
        }


        private void txtFone_Leave(object sender, EventArgs e)
        {
            if (txtFone.Text != "")
            {
                txtFone.Text = clsFuncoes.RetornaNumero(txtFone.Text).ToString();

                if (clsFuncoes.ValidaFone(txtFone.Text) == false)
                {
                    MessageBox.Show("Fone inválido", txtFone.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtFone.Select();
                }
            }
        }


        private async void btnIncluir_Click(object sender, EventArgs e)
        {
            if (txtFone.Text != "")
            {
                clsVariaveis.GstrSQL = "select * from Vonix_BlackList where Ativo = 1 and Telefone = '" + txtFone.Text.ToString() + "'";
                DataTable dt = await clsBanco.ExecuteQueryRetornoAsync(clsVariaveis.GstrSQL);
                if (dt.Rows.Count == 0)
                {
                    // incluir
                    clsVariaveis.GstrSQL = "insert into Vonix_BlackList ( DtIncl ,Telefone ) values ( cast(getdate() as date) ,'" + txtFone.Text.ToString() + "' )";
                    if (await clsBanco.ExecuteQueryAsync(clsVariaveis.GstrSQL))
                    {
                        Limpar();
                    }
                }
                else
                {
                    string x = Convert.ToDateTime(dt.Rows[0]["DtIncl"].ToString()).ToString("yyyy-MM-dd");
                    string y = DateTime.Now.ToString("yyyy-MM-dd");

                    if ( x == y )
                    {
                        MessageBox.Show("telefone já incluído hoje", "Vonix - BlackList", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show("telefone já cadastrado anteriormente", "Vonix - BlackList", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }


        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if(dataGridView1.Rows.Count > 1)
            {
                txtFone.Text = Convert.ToString(dataGridView1[1, dataGridView1.CurrentRow.Index].Value);
                btnExcluir.Enabled = true;
            }             
        }


        private async void btnExcluir_Click(object sender, EventArgs e)
        {
            if (txtFone.Text != "")
            {
                DialogResult dialogResult = MessageBox.Show("Deseja realmente excluir este telefone ?", "Excluir " + txtFone.Text  , MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    clsVariaveis.GstrSQL = "delete from Vonix_BlackList where Telefone = '" + txtFone.Text.ToString() + "'";

                    if (await clsBanco.ExecuteQueryAsync(clsVariaveis.GstrSQL))
                    {
                        Limpar();
                    }
                }
            }
        }


        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Limpar();
        }

        private async void btnGerarArq_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count > 1)
                {
                    DialogResult dialogResult = MessageBox.Show("Tem certeza ?", "Gerar arquivo e enviar email" + txtFone.Text, MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        btnGerarArq.Enabled = false;
                        this.Cursor = Cursors.WaitCursor;

                        // caminho do arquivo a ser gerado
                        string strCaminho = @"\\172.17.14.3\Vonix\XV\BlackList\Trade_BlackList.csv";

                        clsVariaveis.GstrSQL = "select telefone from VONIX_BLACKLIST where ativo = 1 order by id";
                        DataTable dt = await clsBanco.ExecuteQueryRetornoAsync(clsVariaveis.GstrSQL);
                        if (dt.Rows.Count > 0)
                        {
                            // 1 gerar arquivo
                            if( await clsFuncoes.GeraCsvAsync( dt ,strCaminho ) == false)
                            {
                                MessageBox.Show("erro ao gerar o arquivo", btnGerarArq.Text.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }


                            // 2 enviar email com os fones incluidos hoje
                            // montado mensagem a ser enviada ( telefones incluidos hoje ) 
                            string strTextoEmail = string.Empty;

                            clsVariaveis.GstrSQL = "select Telefone from VONIX_BLACKLIST where ativo = 1 and gerou = 0 order by id";
                            DataTable dt2 = new DataTable();
                            dt2 = await Classes.clsBanco.ConsultaAsync(clsVariaveis.GstrSQL);
                            if (dt2.Rows.Count != 0)
                            {
                                strTextoEmail = "Favor bloquear o(s) numero(s) abaixo : " + "\n" + "\n";

                                foreach (DataRow item2 in dt2.Rows)
                                {
                                    strTextoEmail += "   " + item2["Telefone"].ToString() + "\n";
                                }

                                bool booEmail = await clsFuncoes.EnviaEmailAsync("TRADECALL - Bloquear Telefones", strTextoEmail, "");
                            }


                            // 3 
                            Boolean boo3 = await clsBanco.ExecuteQueryAsync("update VONIX_BLACKLIST set Gerou = 1 where Ativo = 1 and Gerou = 0 and DtIncl = cast(getdate() as date)");


                            MessageBox.Show("Fim", btnGerarArq.Text.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            Limpar();
                        }
                        else
                        {
                            MessageBox.Show("Nada a processar", btnGerarArq.Text.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, btnGerarArq.Text.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            btnGerarArq.Enabled = true;
            this.Cursor = Cursors.Default;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.Forms.FormWindowState.Maximized)
            {
                this.WindowState = System.Windows.Forms.FormWindowState.Normal;
            }
        }
    }



}
