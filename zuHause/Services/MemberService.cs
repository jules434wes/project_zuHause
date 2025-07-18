using Microsoft.AspNetCore.Identity;
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
            var result = _passwordHasher.VerifyHashedPassword(member, member.Password,inputPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
