using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Web;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Entities;
using BioEngine.Extra.IPB.Models;
using BioEngine.Extra.IPB.Publishing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Extra.IPB.Controllers
{
    public class SyncController : IPBController
    {
        private readonly BioContext _dbContext;

        public SyncController(BaseControllerContext context, IPBApiClientFactory factory, BioContext dbContext) : base(
            context, factory)
        {
            _dbContext = dbContext;
        }

        [HttpGet("comments")]
        public async Task<ActionResult<bool>> SyncCommentsAsync()
        {
            var records = await _dbContext.Set<IPBPublishRecord>().ToListAsync();
            foreach (var record in records)
            {
                var page = 1;
                var posts = new List<Post>();
                while (true)
                {
                    var response = await Client.GetPostsAsync(record.TopicId, page);
                    posts.AddRange(response.Results);
                    if (page < response.TotalPages)
                    {
                        page++;
                    }
                    else
                    {
                        break;
                    }
                }

                foreach (var post in posts)
                {
                    var comment = await _dbContext.Set<IPBComment>().Where(c => c.PostId == post.Id)
                                      .FirstOrDefaultAsync() ?? new IPBComment
                                  {
                                      Type = record.Type,
                                      ContentId = record.ContentId,
                                      AuthorId = post.Author.Id,
                                      PostId = post.Id,
                                      DateAdded = post.Date
                                  };
                    comment.DateUpdated = DateTimeOffset.Now;
                    comment.SiteIds = record.SiteIds;
                    if (comment.Id == Guid.Empty)
                    {
                        _dbContext.Add(comment);
                    }
                    else
                    {
                        _dbContext.Update(comment);
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            return Ok(true);
        }
    }
}
