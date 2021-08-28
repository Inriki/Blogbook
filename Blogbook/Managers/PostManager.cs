using Blogbook.Data;
using Blogbook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blogbook.Managers
{
    public interface IPostManager
    {
        public Task<List<Post>> GetPostsAsync(int order, int page, string userName);
        public Task AddPostAsync(Post post, string userName);
    }

    public class PostManager : IPostManager
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        private readonly int pageSize;

        public PostManager(ApplicationDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;

            if(!int.TryParse(_configuration["PostManager:PageSize"], out pageSize)) pageSize = 50;
        }

        public async Task<List<Post>> GetPostsAsync(int order, int page, string userName)
        {
            IQueryable<Post> posts = _context.Posts;

            if (order == 1)
                posts = _context.Posts.OrderBy(p => p.PublicationDate);
            else
                posts = _context.Posts.OrderByDescending(p => p.PublicationDate);

            if (!string.IsNullOrEmpty(userName)) posts = posts.Where(p => p.User.UserName == userName);

            return await posts.Skip(page * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task AddPostAsync(Post post, string userName)
        {
            if (post.PublicationDate == DateTime.MinValue) post.PublicationDate = DateTime.Now;
            post.User = await _userManager.FindByNameAsync(userName);

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }
    }
}
