
namespace Grocery.Core.Models
{
    public partial class Client : Model
    {
        private string _emailAddress;
        private string _password;
        
        public string Email => _emailAddress;
        public string Password => _password;
        public Client(int id, string name, string emailAddress, string password) : base(id, name)
        {
            _emailAddress = emailAddress;
            _password = password;
        }
    }
}
