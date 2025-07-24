using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;
using zuHause.Models;

namespace zuHause.Services
{
    public class MemberService
    {
        private readonly IPasswordHasher<Member> _passwordHasher;
        private readonly ZuHauseContext _context;


        public MemberService(IPasswordHasher<Member> passwordHasher, ZuHauseContext context)
        {
            _passwordHasher = passwordHasher;
            _context = context;
        }
        public void Register(Member member, string plainPassword) 
        {
            member.Password = _passwordHasher.HashPassword(member, plainPassword);
            _context.Members.Add(member);
            _context.SaveChanges();
        }
        public bool verifyPassword(Member member, string inputPassword) {
            // 1. 首先嘗試 ASP.NET Core Identity 標準驗證
            try
            {
                var result = _passwordHasher.VerifyHashedPassword(member, member.Password, inputPassword);
                if (result == PasswordVerificationResult.Success)
                {
                    return true;
                }
            }
            catch (FormatException)
            {
                // 密碼格式不是 ASP.NET Core Identity 格式，繼續嘗試其他格式
            }

            // 2. 嘗試明文密碼比對
            if (member.Password == inputPassword)
            {
                // 自動升級明文密碼為雜湊格式
                UpgradePasswordFormat(member, inputPassword);
                return true;
            }

            // 3. 嘗試 MD5 雜湊比對
            if (VerifyMD5Password(inputPassword, member.Password))
            {
                // 自動升級 MD5 密碼為標準格式
                UpgradePasswordFormat(member, inputPassword);
                return true;
            }

            // 4. 嘗試 SHA256 雜湊比對
            if (VerifySHA256Password(inputPassword, member.Password))
            {
                // 自動升級 SHA256 密碼為標準格式
                UpgradePasswordFormat(member, inputPassword);
                return true;
            }

            // 5. 嘗試 SHA256 + Salt 比對（如管理員使用的格式）
            if (VerifySHA256WithSaltPassword(inputPassword, member.Password))
            {
                // 自動升級 SHA256+Salt 密碼為標準格式
                UpgradePasswordFormat(member, inputPassword);
                return true;
            }

            return false;
        }

        private void UpgradePasswordFormat(Member member, string plainPassword)
        {
            try
            {
                // 將密碼升級為 ASP.NET Core Identity 標準格式
                var newHashedPassword = _passwordHasher.HashPassword(member, plainPassword);
                member.Password = newHashedPassword;
                member.UpdatedAt = DateTime.Now;

                // 更新資料庫
                _context.Members.Update(member);
                _context.SaveChanges();
            }
            catch (Exception)
            {
                // 如果升級失敗，記錄但不影響登入流程
                // 可以考慮加入日誌記錄
            }
        }

        private bool VerifyMD5Password(string inputPassword, string storedHash)
        {
            try
            {
                using var md5 = MD5.Create();
                var inputBytes = Encoding.UTF8.GetBytes(inputPassword);
                var hashBytes = md5.ComputeHash(inputBytes);
                var computedHash = Convert.ToHexString(hashBytes).ToLower();

                return storedHash.ToLower() == computedHash;
            }
            catch
            {
                return false;
            }
        }

        private bool VerifySHA256Password(string inputPassword, string storedHash)
        {
            try
            {
                using var sha256 = SHA256.Create();
                var inputBytes = Encoding.UTF8.GetBytes(inputPassword);
                var hashBytes = sha256.ComputeHash(inputBytes);
                var computedHash = Convert.ToHexString(hashBytes).ToLower();

                return storedHash.ToLower() == computedHash;
            }
            catch
            {
                return false;
            }
        }

        private bool VerifySHA256WithSaltPassword(string inputPassword, string storedHash)
        {
            try
            {
                // 嘗試一些常見的 salt 值或從資料庫獲取
                string[] commonSalts = { "", "salt", "zuhause", "password" };
                
                using var sha256 = SHA256.Create();
                foreach (var salt in commonSalts)
                {
                    var combined = inputPassword + salt;
                    var bytes = Encoding.UTF8.GetBytes(combined);
                    var hashBytes = sha256.ComputeHash(bytes);
                    var computedBase64 = Convert.ToBase64String(hashBytes);
                    var computedHex = Convert.ToHexString(hashBytes).ToLower();

                    if (storedHash == computedBase64 || storedHash.ToLower() == computedHex)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public void ResetPassword(Member member, string newPassword)
        {
            member.Password = _passwordHasher.HashPassword(member, newPassword);
            member.UpdatedAt = DateTime.Now;
            _context.Members.Update(member);
            _context.SaveChanges();
        }
    }
}
