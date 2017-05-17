using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminService.Controller
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;

    using Microsoft.AspNetCore.Hosting;
    using Admin;

    [Route("api/[controller]")]
    public class KeysController : Controller
    {
        private readonly IReliableStateManager stateManager;

        public KeysController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;

        }

        [HttpGet]
        [Route("key1")]
        public async Task<IActionResult> GetKey1Async()
        {
            string key = await GetKey(Constants.KEY1);
            return Ok(key);
        }

        [HttpGet]
        [Route("key2")]
        public async Task<IActionResult> GetKey2Async()
        {
            string key = await GetKey(Constants.KEY2);
            return Ok(key);
        }

        [HttpPost]
        [Route("key1")]
        public async Task<IActionResult> RegenerateKey1Async()
        {
            string key = await GetKey(Constants.KEY1);
            return Ok(key);
        }

        [HttpPost]
        [Route("key2")]
        public async Task<IActionResult> RegenereateKey2Async()
        {
            string key = await GetKey(Constants.KEY2);
            return Ok(key);
        }

        private async Task<string> GetKey(string keyName)
        {
            var topics = await GetTopicDictionary();

            using (var tx = this.stateManager.CreateTransaction())
            {
                var key = await topics.TryGetValueAsync(tx, keyName);
                if (key.HasValue)
                {
                    return key.Value;
                }

                throw new Exception("No Key initialized.");
            }
        }

        private async Task RegenerateKey(string keyName)
        {
            var topics = await GetTopicDictionary();
            using (var tx = this.stateManager.CreateTransaction())
            {
                string newKey = GenerateNewKey();
                await topics.SetAsync(tx, keyName, newKey);

                await tx.CommitAsync();
            }
        }

        private string GenerateNewKey()
        {
            // TODO generator a valid security key. using basic placeholder for now
            // should also encrypt as the key is stored in plain text when stored in
            // statefule service
            return Guid.NewGuid().ToString();
        }

        private async Task<IReliableDictionary<string, string>> GetTopicDictionary()
        {
            return await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(Constants.COLLECTION_KEYS);
        }
    }
}
