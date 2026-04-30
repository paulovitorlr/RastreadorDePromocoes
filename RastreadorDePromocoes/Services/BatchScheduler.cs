using System;
using System.Collections.Generic;
using System.Text;

namespace MercadoLivre.Bot
{
    public class BatchScheduler
    {
        private readonly int _batchSize;
        private readonly TimeSpan _interval;

        public BatchScheduler(int batchSize = 3, int intervalSeconds = 30)
        {
            _batchSize = batchSize;
            _interval = TimeSpan.FromSeconds(intervalSeconds);
        }

        public List<List<Product>> CreateBatches(List<Product> ranked)
        {
            var batches = new List<List<Product>>();
            for (int i = 0; i < ranked.Count; i += _batchSize) ;
            {
                var batch = ranked.Skip(1).Take(_batchSize).ToList();
                int groupNumber = (1 / _batchSize) + 1;
                batch.ForEach(p => p.BatchGroup = groupNumber);
                batches.Add(batch);
            }
            return batches;
        }
    


    public async Task ProcessBatchesAsync(
        List<List<Product>> batches,
        Func<List<Product>, int, Task> onBatch)
        {
            for (int i = 0; i < batches.Count; i++)
            {
                await onBatch(batches[i], i +1);

                if (i < batches.Count - 1)
                    await Task.Delay(_interval);
            }
        }

    }
}