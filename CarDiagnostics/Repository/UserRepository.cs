using CarDiagnostics.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CarDiagnostics.Repository
{
    public class UserRepository
    {
        private readonly string _usersFilePath = "users.json";
        private readonly ReaderWriterLockSlim _lock = new();

        public List<User> GetAllUsers()
        {
            try
            {
                _lock.EnterReadLock();

                if (!File.Exists(_usersFilePath))
                    return new List<User>();

                var json = File.ReadAllText(_usersFilePath);
                return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool IsValidUser(string username, string email)
        {
            var users = GetAllUsers();
            return users.Any(u => u.Username == username && u.Email == email);
        }
    }
}
