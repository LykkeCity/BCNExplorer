using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Linq;

namespace AzureStorage.DocDb
{
    public static class DocDbHelpers
    {
        public static async Task<IEnumerable<T>> QueryAsync<T>(this IDocumentQuery<T> docQuery)
        {
            var batches = new List<IEnumerable<T>>();

            do
            {
                var batch = await docQuery.ExecuteNextAsync<T>();

                batches.Add(batch);
            }
            while (docQuery.HasMoreResults);

            var docs = batches.SelectMany(b => b);

            return docs;
        }
    }
}
