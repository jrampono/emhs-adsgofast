using Microsoft.Data.SqlClient;

namespace WebApplication.Models
{
    public class AdsGoFastDapperContext 
    {
        Helpers.AzureSDK _azureSDK { get; set; }
        string _connectionstring { get; set; }

        public AdsGoFastDapperContext(Helpers.AzureSDK azureSDK, string Server, string Database)
        {
            this._azureSDK = azureSDK;
            SqlConnectionStringBuilder _scsb = new SqlConnectionStringBuilder
            {
                DataSource = Server,
                InitialCatalog = Database
            };
            this._connectionstring = _scsb.ConnectionString;
            
        }

        public SqlConnection GetConnection()
        {;
            SqlConnection _con = new SqlConnection(_connectionstring);
            string _token = _azureSDK.GetAzureRestApiToken("https://database.windows.net/");
            _con.AccessToken = _token;
            return _con;
        }
    }
}
