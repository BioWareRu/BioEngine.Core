using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Core.Helpers
{
    public static class BlocksHelper
    {
        public static async Task<Dictionary<Guid, List<ContentBlock>>> GetBlocksAsync<TEntity>(TEntity[] entities,
            BioContext dbContext)
            where TEntity : IContentEntity
        {
            var ids = entities.Select(e => e.Id).ToArray();
            var blocks = await dbContext.Blocks
                .Where(b => ids.Contains(b.ContentId)).ToListAsync();
            return ids.ToDictionary(id => id,
                id => blocks.Where(b => b.ContentId == id).OrderBy(b => b.Position).ToList());
        }

        public static async Task<List<ContentBlock>> GetBlocksAsync<TEntity>(TEntity entity,
            BioContext dbContext)
            where TEntity : IContentEntity
        {
            return (await GetBlocksAsync(new[] {entity}, dbContext))[entity.Id];
        }

        public static Task AddBlocksAsync<TEntity>(TEntity entity, BioContext dbContext)
            where TEntity : IContentEntity
        {
            foreach (var block in entity.Blocks)
            {
                block.ContentId = entity.Id;
            }

            return dbContext.AddRangeAsync(entity.Blocks);
        }

        public static async Task UpdateBlocksAsync<TEntity>(TEntity entity,
            BioContext dbContext)
            where TEntity : IContentEntity
        {
            var oldBlocks = await dbContext.Blocks
                .Where(b => b.ContentId == entity.Id).AsNoTracking()
                .ToListAsync();
            foreach (var block in entity.Blocks)
            {
                block.ContentId = entity.Id;

                var oldBlock = oldBlocks.FirstOrDefault(b => b.Id == block.Id);
                if (oldBlock == null)
                {
                    dbContext.Blocks.Add(block);
                }
            }

            foreach (var oldBlock in oldBlocks)
            {
                var block = entity.Blocks.FirstOrDefault(b => b.Id == oldBlock.Id);
                if (block == null)
                {
                    dbContext.Remove(oldBlock);
                }
                else
                {
                    dbContext.Attach(block);
                    dbContext.Update(block);
                }
            }
        }

        public static async Task DeleteBlocksAsync<TEntity>(TEntity entity, BioContext dbContext)
            where TEntity : IContentEntity
        {
            var blocks = await GetBlocksAsync(entity, dbContext);

            dbContext.Blocks.RemoveRange(blocks);
        }
    }
}
