

namespace Vonix_BlackList.Classes
{
    class clsVariaveis
    {
        // srv_db03
        //private static string conexao = @"Data Source=10.0.32.63;Initial Catalog=CAD_TRADE; User ID=sa; Password=SRV@admin2012;";

        // srv_db02
        private static string conexao = @"Data Source=10.0.32.54;Initial Catalog=CAD_TRADE; User ID=sa; Password=SRV@admin2012;";


        public static string Conexao
        {
            get { return clsVariaveis.conexao; }
        }

        private static string gstrSQL = string.Empty;
        public static string GstrSQL
        {
            get { return clsVariaveis.gstrSQL; }
            set { clsVariaveis.gstrSQL = value; }
        }

    }
}
