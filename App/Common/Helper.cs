using System;
using System.Security.Cryptography;
using System.Text;

namespace ProjectBranchSelector.Common
{
    public static class Helper
    {
        public static Guid GenerateGuid(string[] args)
        {            
            if (args!=null && args.Length > 0)
            {                
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(string.Join("", args)));
                    return new Guid(hash);
                }
            }
            return Guid.NewGuid();
        }
    }
}
